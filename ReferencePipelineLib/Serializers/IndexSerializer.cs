using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HtmlAgilityPack;
using OsgContentPublishing.ReferencePipelineLib.Pipelines;
using OsgContentPublishing.ReferencePipelineLib.TypeTopology;
using OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen;
using OsgContentPublishing.ReferencePipelineLib.TypeTopology.Projected;

namespace OsgContentPublishing.ReferencePipelineLib.Serializers
{
    public class IndexSerializer
    {
        public IndexSerializer( string outputFolder, string siteConfigReferenceRoot )
        {
            this.OutputFolder = outputFolder;
            this.SiteConfigReferenceRoot = siteConfigReferenceRoot;
        }

        public string OutputFolder
        {
            get;
            private set;
        }

        public string SiteConfigReferenceRoot
        {
            get;
            private set;
        }

        public Dictionary<string, Pipeline> Pipelines
        {
            get
            {
                if( this._pipelines == null )
                {
                    this._pipelines = new Dictionary<string, Pipeline>();
                }

                return this._pipelines;
            }
        }

        public virtual void EmitIndex( string title, List<DefinedType> knownTypes )
        {
            HtmlDocument indexDoc = new HtmlDocument();

            string titleString = String.Format( indexTitleElements, title );
            HtmlNode titleNode = indexDoc.CreateTextNode( titleString );
            indexDoc.DocumentNode.AppendChild( titleNode );

            foreach( var kvp in this.Pipelines )
            {
                string pipelineName = kvp.Key;
                Pipeline pipeline = kvp.Value;

                this.EmitIndex( indexDoc, this.OutputFolder, pipeline, knownTypes );
            }

            indexDoc.Save( Path.Combine( this.OutputFolder, "index.html" ) );
        }


        public void EmitIndex(
            HtmlDocument indexDoc,
            string outputRootFolder,
            Pipeline pipeline,
            List<DefinedType> knownTypes )
        {
            string pipelineTitleString = String.Format( indexTitleElements, pipeline.IndexTitle );
            HtmlNode pipelineTitleNode = indexDoc.CreateTextNode( pipelineTitleString );
            indexDoc.DocumentNode.AppendChild( pipelineTitleNode );

            var typesAtGlobalScope = pipeline.Types.Where( t =>
                !( t is DoxygenFacadeType ) &&
                !( t is GlobalNamespace ) &&
                ( t.Namespace.IsGlobalNamespace ) &&
                t.Name != "Windows" ).ToList();

            RidlPipeline ridlPipeline = pipeline as RidlPipeline;
            bool projectedTypesOnly = ( ridlPipeline != null ) ? true : false;

            HtmlNode tableNode = indexDoc.CreateElement( "table" );
            indexDoc.DocumentNode.AppendChild( tableNode );

            this.AddChildTypesToIndexTable(
                typesAtGlobalScope,
                indexDoc,
                tableNode,
                pipeline.SiteConfigReferenceRoot,
                knownTypes,
                projectedTypesOnly );

            var requestedNamespacesOrdered = this.RequestedNamespaces.OrderBy( n => n );

            foreach( string requestedNamespace in requestedNamespacesOrdered )
            {
                var ns = pipeline.Types.FirstOrDefault( t =>
                    t.IsNamespace &&
                    IsRequestedNamespace( t, requestedNamespace ) );

                if( ns != null )
                {
                    string anchor = Utilities.GetAnchor(
                        ns,
                        knownTypes,
                        pipeline.SiteConfigReferenceRoot,
                        true,
                        false );

                    string namespaceString = String.Format( namespaceBlockElements, anchor );
                    HtmlNode namespaceNode = indexDoc.CreateTextNode( namespaceString );
                    indexDoc.DocumentNode.AppendChild( namespaceNode );

                    if( ns.Content.HasContent )
                    {
                        string namespaceAbstractString = String.Format( namespaceAbstractElements, ns.Content.Abstract );
                        HtmlNode namespaceAbstractNode = indexDoc.CreateTextNode( namespaceAbstractString );
                        indexDoc.DocumentNode.AppendChild( namespaceAbstractNode );
                    }

                    HtmlNode brNode = indexDoc.CreateTextNode( "<br>" );
                    indexDoc.DocumentNode.AppendChild( brNode );

                    tableNode = indexDoc.CreateElement( "table" );
                    indexDoc.DocumentNode.AppendChild( tableNode );

                    string tableheaderString = String.Format( twoColumnTableHeaderElements, "Type", "Description" );
                    HtmlNode tableheaderNode = indexDoc.CreateTextNode( tableheaderString );
                    tableNode.AppendChild( tableheaderNode );

                    var childNamespaces = ns.ChildTypes.Where( t => t.IsNamespace ).ToList();
                        this.AddChildTypesToIndexTable(
                            childNamespaces,
                            indexDoc,
                            tableNode,
                            pipeline.SiteConfigReferenceRoot,
                            knownTypes,
                            projectedTypesOnly );

                    var childTypes = ns.ChildTypes.Where( t => !t.IsNamespace ).ToList();
                    this.AddChildTypesToIndexTable(
                            childTypes,
                            indexDoc,
                            tableNode,
                            pipeline.SiteConfigReferenceRoot,
                            knownTypes,
                            projectedTypesOnly );

                    var projectedChildTypes = childTypes.Where( t => t is ProjectedType ).ToList();

                    int numChildTypes = projectedTypesOnly ? projectedChildTypes.Count : childTypes.Count;

                    if( numChildTypes > maxNumberOfChildTypesToList )
                    {
                        string moreString = String.Format( spanningTableDivElements, "more: " + anchor );
                        HtmlNode moreNode = indexDoc.CreateTextNode( moreString );
                        tableNode.AppendChild( moreNode );
                    }

                    brNode = indexDoc.CreateTextNode( "<br>" );
                    indexDoc.DocumentNode.AppendChild( brNode );
                }
            }
        }

        private bool IsProjectedType( DefinedType definedType )
        {
            return (
                definedType != null &&
                definedType is ProjectedType );
        }

        private bool IsProjectedNamespace( DefinedType definedType )
        {
            return (
                definedType.IsNamespace &&
                IsProjectedType( definedType ) );
        }

        private void AddNamespaceToIndex(
            DefinedType ns,
            HtmlDocument indexDoc,
            HtmlNode parentNode,
            string siteConfigReferenceRoot,
            List<DefinedType> knownTypes,
            bool projectedTypesOnly )
        {
            if( !ns.IsGlobalNamespace &&
                ns.Name != "Windows" &&
                this.IsInRequestedNamespace( ns ) )
            {
                // Filter types that aren't projected, if
                // projectedTypesOnly is selected.
                if( projectedTypesOnly && !IsProjectedNamespace( ns ) )
                {
                    return;
                }

                bool renderFullName = this.IsRequestedNamespace( ns ) ? true : false;
                string anchor = Utilities.GetAnchor(
                    ns,
                    knownTypes,
                    siteConfigReferenceRoot,
                    renderFullName,
                    false );

                string nodeText = String.Format(
                    indexNamespaceAnchorItem, anchor,
                    ns.Content.Abstract );

                HtmlNode namespaceNode = indexDoc.CreateTextNode( nodeText );
                parentNode.AppendChild( namespaceNode );
            }

            var childNamespaces = ns.ChildTypes.Where( c => c.IsNamespace ).ToList();
            if( childNamespaces != null && childNamespaces.Count > 0 )
            {
                HtmlNode ulNode = indexDoc.CreateElement( "ul" );
                parentNode.AppendChild( ulNode );

                foreach( var childNamespace in childNamespaces )
                {
                    if( ns is ProjectedType && !( childNamespace is ProjectedType ) )
                    {
                        // If the current namespace is projected, 
                        // emit only projected child namespaces.
                        continue;
                    }

                    AddNamespaceToIndex(
                        childNamespace,
                        indexDoc,
                        ulNode,
                        siteConfigReferenceRoot,
                        knownTypes,
                        projectedTypesOnly );
                }
            }
        }

        public List<string> RequestedNamespaces
        {
            get
            {
                if( this._requestedNamespaces == null )
                {
                    this._requestedNamespaces = new List<string>();

                    foreach( var pipeline in this.Pipelines.Values )
                    {
                        this._requestedNamespaces.AddRange( pipeline.Namespaces );
                    }
                }

                return this._requestedNamespaces;
            }
        }

        private bool IsRequestedNamespace( DefinedType definedType )
        {
            bool isRequestedNamespace = false;

            if( definedType.IsNamespace )
            {
                foreach( string requestedNamespace in this.RequestedNamespaces )
                {
                    isRequestedNamespace = IsRequestedNamespace( definedType, requestedNamespace );
                    if( isRequestedNamespace )
                    {
                        break;
                    }
                }
            }

            return isRequestedNamespace;
        }

        private static bool IsRequestedNamespace( DefinedType definedType, string requestedNamespace )
        {
            bool isRequestedNamespace = false;

            if( definedType.IsNamespace )
            {
                string requestedNamespaceScrubbed = String.Copy( requestedNamespace );

                if( requestedNamespace.EndsWith( "*" ) )
                {
                    requestedNamespaceScrubbed = requestedNamespace.Replace( ".*", String.Empty );
                    requestedNamespaceScrubbed = requestedNamespaceScrubbed.Replace( "*", String.Empty );
                }

                DefinedTypeComparer comparer = new DefinedTypeComparer();
                isRequestedNamespace = comparer.Equals( definedType, requestedNamespaceScrubbed );
            }

            return isRequestedNamespace;
        }

        private bool IsInRequestedNamespace( DefinedType definedType )
        {
            bool isInRequestedNamespace = false;

            foreach( string requestedNamespace in this.RequestedNamespaces )
            {
                isInRequestedNamespace = IsInRequestedNamespace( definedType, requestedNamespace );
                if( isInRequestedNamespace )
                {
                    break;
                }
            }

            return isInRequestedNamespace;
        }

        private static bool IsInRequestedNamespace( DefinedType definedType, string requestedNamespace )
        {
            bool isInRequestedNamespace = false;

            string requestedNamespaceScrubbed = String.Copy( requestedNamespace );

            if( requestedNamespace.EndsWith( "*" ) )
            {
                requestedNamespaceScrubbed = requestedNamespace.Replace( ".*", String.Empty );
                requestedNamespaceScrubbed = requestedNamespaceScrubbed.Replace( "*", String.Empty );
            }

            isInRequestedNamespace = definedType.FullName.StartsWith( requestedNamespaceScrubbed );

            return isInRequestedNamespace;
        }

        private void AddChildTypesToIndex(
            DefinedType ns,
            HtmlDocument indexDoc,
            HtmlNode parentNode,
            string siteConfigReferenceRoot,
            List<DefinedType> knownTypes,
            bool projectedTypesOnly )
        {
            List<DefinedType> childTypes = ns.ChildTypes;

            if( projectedTypesOnly )
            {
                var projectedChildTypes = ns.ChildTypes.Where( t => IsProjectedType( t ) ).ToList();
                if( projectedChildTypes.Count > 0 )
                {
                    childTypes = projectedChildTypes;
                }
            }

            this.AddChildTypesToIndex(
                childTypes,
                indexDoc,
                parentNode,
                siteConfigReferenceRoot,
                knownTypes );
        }

        private void AddChildTypesToIndex(
            List<DefinedType> childTypes,
            HtmlDocument indexDoc,
            HtmlNode parentNode,
            string siteConfigReferenceRoot,
            List<DefinedType> knownTypes,
            bool projectedTypesOnly )
        {
            // For brevity, only list classes and interfaces.
            var filteredChildTypes = childTypes.Where( t => t.IsClass || t.IsInterface );

            if( projectedTypesOnly )
            {
                filteredChildTypes = filteredChildTypes.Where( t => IsProjectedType( t ) );
            }

            foreach( var childType in filteredChildTypes )
            {
                string anchor = Utilities.GetAnchor(
                    childType,
                    knownTypes,
                    siteConfigReferenceRoot,
                    false );

                string nodeText = String.Format(
                    indexChildTypeAnchorItem,
                    anchor,
                    childType.LanguageElementName.ToLower(),
                    childType.Content.Abstract );

                HtmlNode childTypeNode = indexDoc.CreateTextNode( nodeText );
                parentNode.AppendChild( childTypeNode );
            }
        }


        private void AddChildTypesToIndex(
            List<DefinedType> childTypes,
            HtmlDocument indexDoc,
            HtmlNode parentNode,
            string siteConfigReferenceRoot,
            List<DefinedType> knownTypes )
        {
            // For brevity, only list classes and interfaces.
            //var filteredChildTypes = childTypes.Where( t => t.IsClass || t.IsInterface );
            var filteredChildTypes = childTypes;

            foreach( var childType in filteredChildTypes )
            {
                string anchor = Utilities.GetAnchor(
                    childType,
                    knownTypes,
                    siteConfigReferenceRoot,
                    false );

                string nodeText = String.Format(
                    indexChildTypeAnchorItem,
                    anchor,
                    childType.LanguageElementName.ToLower(),
                    childType.Content.Abstract );

                HtmlNode childTypeNode = indexDoc.CreateTextNode( nodeText );
                parentNode.AppendChild( childTypeNode );
            }
        }

        private void AddChildTypesToIndexTable(
            List<DefinedType> childTypes,
            HtmlDocument indexDoc,
            HtmlNode parentNode,
            string siteConfigReferenceRoot,
            List<DefinedType> knownTypes,
            bool projectedTypesOnly )
        {
            // For brevity, only list classes and interfaces.
            //var filteredChildTypes = childTypes.Where( t => t.IsClass || t.IsInterface ).ToList();
            var filteredChildTypes = childTypes;

            if( projectedTypesOnly )
            {
                filteredChildTypes = filteredChildTypes.Where( t => 
                    IsProjectedType( t ) ).Take( maxNumberOfChildTypesToList ).ToList();
            }

            this.AddChildTypesToIndexTable(
                filteredChildTypes,
                indexDoc,
                parentNode,
                siteConfigReferenceRoot,
                knownTypes );
        }

        private void AddChildTypesToIndexTable(
            List<DefinedType> childTypes,
            HtmlDocument indexDoc,
            HtmlNode parentTableNode,
            string siteConfigReferenceRoot,
            List<DefinedType> knownTypes )
        {
            foreach( var childType in childTypes )
            {
                this.AddChildTypeToIndexTable( 
                    childType,
                    indexDoc,
                    parentTableNode,
                    siteConfigReferenceRoot,
                    knownTypes );
            }
        }


        private void AddChildTypeToIndexTable(
            DefinedType childType,
            HtmlDocument indexDoc,
            HtmlNode parentTableNode,
            string siteConfigReferenceRoot,
            List<DefinedType> knownTypes )
        {
            string anchor = Utilities.GetAnchor(
                childType,
                knownTypes,
                siteConfigReferenceRoot,
                false );

            string nodeText = String.Format(
                childTypeTableRowElements,
                anchor,
                childType.LanguageElementName.ToLower(),
                childType.Content.Abstract );

            HtmlNode childTypeNode = indexDoc.CreateTextNode( nodeText );
            parentTableNode.AppendChild( childTypeNode );
        }

        private static bool IsTopLevelNamespace( DefinedType ns )
        {
            bool isTopLevelNamespace = false;

            if( ns.IsNamespace )
            {
                isTopLevelNamespace = ( ns.Namespace == null || ns.Namespace.IsGlobalNamespace ) ? true : false;
            }

            return isTopLevelNamespace;
        }

        //public void FindLinks( List<DefinedType> definedTypes )
        //{
        //    foreach( DefinedType type in definedTypes )
        //    {
        //        List<ReferenceContent> allContentForType = new List<ReferenceContent>();

        //        // Get descriptions for the type.
        //        //descriptions.Add( this.BriefDescription );
        //        //descriptions.Add( this.DetailedDescription );
        //        allContentForType.Add( type.Content );

        //        // Get descriptions for the type's members.
        //        var memberBriefDescriptions = this.MemberDefs.Select( m => m.BriefDescription );
        //        var memberDetailedDescriptions = this.MemberDefs.Select( m => m.DetailedDescription );
        //        descriptions.AddRange( memberBriefDescriptions );
        //        descriptions.AddRange( memberDetailedDescriptions );

        //        foreach( var member in this.MemberDefs )
        //        {
        //            descriptions.Add( member.BriefDescription );
        //            descriptions.Add( member.DetailedDescription );

        //            if( member.IsMethod || member.IsConstructor )
        //            {
        //                var paramDescriptions = member.Parameters.Select( p => p.BriefDescription );
        //                descriptions.AddRange( paramDescriptions );

        //                //foreach( var param in member.Parameters )
        //                //{
        //                //    var paramType = mergedTypes.FirstOrDefault( t => t.Name == param.type );
        //                //    if( paramType != null )
        //                //    {
        //                //        param.DoxType = paramType;
        //                //    }
        //                //}
        //            }
        //        }

        //        var filteredDescriptions = descriptions.Where( d => d != null && d.Desc != String.Empty );

        //        foreach( var description in filteredDescriptions )
        //        {
        //            description.ResolveLinks( this, definedTypes );
        //        }
        //    }
        //}


        protected Dictionary<string, Pipeline> _pipelines;
        protected List<string> _requestedNamespaces;

        private const string divElement = "<div>{0}</div>\n";
        private const string indexTitleElements = "<div><h1>{0}</h1></div>\n";
        private const string indexNamespaceAnchorItem = "<li><b>{0} namespace</b> | {1}</li>\n";
        private const string indexChildTypeAnchorItem = "<li>{0} {1} | {2}</li>\n";
        private const string typeAtGlobalScopeElements = "<div>{0}</div>\n";

        private const string spanningTableDivElements = "<tr><td align=\"center\" style=\"font-size:larger;font-weight:bold\" colspan=\"2\">{0}</td></tr>\n";
        private const string twoColumnTableHeaderElements = "<tr><th>{0}</th><th>{1}</th></tr>\n";
        private const string twoColumnTableRowElements = "<tr><td>{0}</td><td>{1}</td></tr>\n";
        private const string namespaceBlockElements = "<div><h2>{0}</h2></div>\n";
        private const string namespaceAbstractElements = "<div><h3>{0}</h3></div>\n";
        private const string childTypeTableRowElements = "<tr><td>{0}&nbsp;{1}</td><td>{2}</td></tr>\n";
        
        private const int numberOfNamespacesToExpand = 2;
        private const int maxNumberOfChildTypesToList = 7;
    }
}
