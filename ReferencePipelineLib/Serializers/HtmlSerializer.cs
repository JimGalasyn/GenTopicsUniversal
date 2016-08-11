using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using HtmlAgilityPack;
using OsgContentPublishing.EventLogging;
using OsgContentPublishing.ReferencePipelineLib.Documentation;
using OsgContentPublishing.ReferencePipelineLib.TypeTopology;
using OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen;
using OsgContentPublishing.ReferencePipelineLib.TypeTopology.Projected;
using OsgContentPublishing.ReferencePipelineLib.TypeTopology.Assembly;

namespace OsgContentPublishing.ReferencePipelineLib.Serializers
{
    public class HtmlSerializer : Serializer
    {
        public HtmlSerializer(
            List<DefinedType> definedTypes,
            List<DefinedType> knownTypes,
            string outputFolder,
            string siteConfigReferenceRoot,
            List<string> namespaces )
            : base( definedTypes, knownTypes, outputFolder, namespaces )
        {
            this.SiteConfigReferenceRoot = siteConfigReferenceRoot;
            this.ProjectedTypesOnly = false;
        }

        public HtmlSerializer(
            List<DefinedType> definedTypes,
            List<DefinedType> knownTypes,
            string outputFolder,
            string siteConfigReferenceRoot,
            List<string> namespaces,
            bool projectedTypesOnly )
            : base( definedTypes, knownTypes, outputFolder, namespaces )
        {
            this.SiteConfigReferenceRoot = siteConfigReferenceRoot;
            this.ProjectedTypesOnly = projectedTypesOnly;
        }

        public string SiteConfigReferenceRoot
        {
            get;
            private set;
        }

        public bool ProjectedTypesOnly
        {
            get;
            private set;
        }

        public override void Serialize()
        {
            this.SerializeToHtml( this.DefinedTypes, this.OutputFolder );
        }

        public override void Serialize( List<DefinedType> knownTypes )
        {
            base.Serialize( knownTypes );

            this.SerializeToHtml( this.DefinedTypes, this.OutputFolder );
        }

        private void SerializeToHtml( List<DefinedType> definedTypes, string outputPath )
        {
            DefinedType resolvedType = null;

            try
            {
                foreach( DefinedType definedType in definedTypes )
                {
                    // TBD: embed anchor tags in doxType.BriefDescription

                    try
                    {
                        if( definedType is DoxygenFacadeType )
                        {
                            continue;
                        }

                        // If ProjectedTypesOnly is true, filter any types 
                        // that arent' projected. This supports RidlPipeline.
                        if( this.ProjectedTypesOnly && !( definedType is ProjectedType ) )
                        {
                            continue;
                        }

                        resolvedType = definedType;

                        if( definedType is DoxygenType )
                        {
                            if( this.KnownTypes != null && this.KnownTypes.Count > 0 )
                            {
                                var knownType = this.KnownTypes.FirstOrDefault( t => t.FullName == definedType.FullName );
                                if( knownType == null )
                                {
                                    continue;
                                }
                                else
                                {
                                    if( knownType is ProjectedType || knownType is AssemblyType )
                                    {
                                        resolvedType = knownType;
                                    }
                                }
                            }
                        }

                        HtmlDocument htmlDoc = CreateHtmlDocument( resolvedType );

                        if( IsForDocumentationOnly( resolvedType ) )
                        {
                            htmlDoc = SerializeDocumentationToHtml( htmlDoc, resolvedType );
                        }
                        else if( resolvedType.IsEnum )
                        {
                            htmlDoc = SerializeEnumToHtml( htmlDoc, resolvedType );
                        }
                        else if( resolvedType.IsStruct )
                        {
                            htmlDoc = SerializeStructToHtml( htmlDoc, resolvedType );
                        }
                        else if( resolvedType.IsNamespace )
                        {
                            htmlDoc = SerializeNamespaceToHtml( htmlDoc, resolvedType );
                            this.SerializeToHtml( resolvedType.ChildTypes, outputPath );
                        }
                        else if( resolvedType.IsInterface || resolvedType.IsClass )
                        {
                            htmlDoc = SerializeClassToHtml( htmlDoc, resolvedType );
                            this.SerializeToHtml( resolvedType.ChildTypes, outputPath );
                        }
                        else if( resolvedType is DoxygenFunction )
                        {
                            htmlDoc = SerializeFunctionToHtml( htmlDoc, resolvedType );
                        }

                        if( htmlDoc != null )
                        {
                            this.PopulateTimestamp( htmlDoc );
                        }

                        if( htmlDoc != null )
                        {
                            string safeString = Utilities.GetSafeString( resolvedType.FullName );
                            string outputFile = Path.Combine( outputPath, safeString );
                            outputFile += ".html";
                            outputFile = outputFile.ToLower();

                            htmlDoc.Save( outputFile );
                        }
                    }
                    catch( Exception ex )
                    {
                        GenTopicsEventLogger.Log.LogException( ex );
                        throw;
                    }
                }
            }
            catch( Exception ex )
            {
                // TODO: Evaluate exceptions and resolve content dropout issues.
                GenTopicsEventLogger.Log.LogException( ex );

                Debug.WriteLine(
                    "Exception",
                    ex.ToString(),
                    ", source: ",
                    ex.Source,
                    ". This exception may explain content dropouts." );
                throw;
            }
        }

        private HtmlDocument SerializeDocumentationToHtml( HtmlDocument htmlDoc, DefinedType definedType )
        {
            try
            {
                if( IsForDocumentationOnly( definedType ) )
                {
                    var docOnlyAttr = GetDocumentationOnlyAttribute( definedType );
                    string sectionName = GetDocumentationOnlySection( definedType );

                    PopulateDocumentTitleAndDescription( htmlDoc, definedType );

                    PopulateDocumentationProperties( htmlDoc, definedType );
                }
                else
                {
                    throw new ArgumentException(
                        "must be a documentation-only type",
                        "definedType" );
                }
            }
            catch( Exception ex )
            {
                Debug.WriteLine( ex.ToString() );
                throw;
            }

            return htmlDoc;
        }

        private void PopulateSection( HtmlDocument htmlDoc, DefinedType definedType )
        {
            string typeName = null;

            if( definedType.IsNamespace )
            {
                typeName = definedType.FullName;
            }
            else if( !( definedType.ParentType is DoxygenFacadeType ) && !definedType.ParentType.IsNamespace )
            {
                typeName = String.Format( "{0}.{1}", definedType.ParentType.Name, definedType.Name );
            }
            else
            {
                typeName = definedType.Name;
            }

            string titleBlockString = String.Format(
                titleBlockElements,
                typeName,
                definedType.LanguageElementName );
            HtmlNode titleNode = htmlDoc.CreateTextNode( titleBlockString );
            HtmlNode bodyNode = GetBodyTag( htmlDoc );
            bodyNode.AppendChild( titleNode );

            if( definedType.IsClass || definedType.IsInterface )
            {
                PopulateInheritsSection( htmlDoc, definedType );
                PopulateDerivedTypesSection( htmlDoc, definedType );
            }

            if( definedType.Content != null )
            {
                string abstractDivString = String.Format( pElement, definedType.Content.Abstract );
                HtmlNode abstractNode = htmlDoc.CreateTextNode( abstractDivString );
                bodyNode.AppendChild( abstractNode );

                this.PopulateRemarksSection( htmlDoc, bodyNode, definedType.Content );

                //if( definedType.Content.Description != String.Empty )
                //{
                //    string remarksDivString = String.Format( 
                //        pHeadingElement, 
                //        "Remarks" );
                //    HtmlNode remarksNode = htmlDoc.CreateTextNode( remarksDivString );
                //    bodyNode.AppendChild( remarksNode );

                //    string descriptionDivString = String.Format( 
                //        pElement, 
                //        definedType.Content.Description );
                //    HtmlNode descriptionNode = htmlDoc.CreateTextNode( descriptionDivString );
                //    bodyNode.AppendChild( descriptionNode );

                //    foreach( var para in definedType.Content.Paragraphs )
                //    {
                //        string paraDivString = String.Format( pElement, para.Content );
                //        HtmlNode paraNode = htmlDoc.CreateTextNode( paraDivString );
                //        bodyNode.AppendChild( paraNode );
                //    }
                //}
            }
            HtmlNode hrNode = htmlDoc.CreateTextNode( hrElements );
            bodyNode.AppendChild( hrNode );
        }


        private HtmlDocument CreateHtmlDocument( DefinedType definedType )
        {
            HtmlDocument htmlDoc = new HtmlDocument();

            PopulateHtmlTag( htmlDoc, definedType );
            PopulateHeadTag( htmlDoc, definedType );
            PopulateXmlIsland( htmlDoc, definedType );

            return htmlDoc;
        }

        private static void PopulateHtmlTag( HtmlDocument htmlDoc, DefinedType definedType )
        {
            HtmlNode htmlNode = htmlDoc.CreateElement( "html" ); // htmlDoc.CreateTextNode( htmlTag );
            htmlNode.Name = "html";

            HtmlAttribute dirAttr = htmlDoc.CreateAttribute( "dir", "ltr" );
            htmlNode.Attributes.Add( dirAttr );

            HtmlAttribute xmlnsAttr = htmlDoc.CreateAttribute( "xmlns", "http://www.w3.org/1999/xhtml" );
            htmlNode.Attributes.Add( xmlnsAttr );

            HtmlAttribute langAttr = htmlDoc.CreateAttribute( "lang", "en" );
            htmlNode.Attributes.Add( langAttr );
            htmlDoc.DocumentNode.AppendChild( htmlNode );
        }

        private static void PopulateHeadTag( HtmlDocument htmlDoc, DefinedType definedType )
        {
            string metaTagsString = String.Format(
                metaTags,
                definedType.Content,
                definedType.Name,
                definedType.LanguageElementName );

            HtmlNode headNode = htmlDoc.CreateElement( "head" );
            HtmlNode metaTagsNode = htmlDoc.CreateTextNode( metaTagsString );
            headNode.AppendChild( metaTagsNode );

            HtmlNode htmlNode = GetHtmlTag( htmlDoc );
            htmlNode.AppendChild( headNode );

            HtmlNode bodyNode = htmlDoc.CreateElement( "body" );
            htmlNode.AppendChild( bodyNode );
        }

        private static void PopulateXmlIsland( HtmlDocument htmlDoc, DefinedType definedType )
        {
            HtmlNode xmlNode = htmlDoc.CreateElement( xmlTag );
            HtmlNode headNode = GetHeadTag( htmlDoc );
            headNode.AppendChild( xmlNode );

            // w_found.asyncstatus
            string undecoratedTypeName = Utilities.GetUndecoratedName( definedType.Name ).ToLower();
            string indexAElementString = String.Format( indexAElement, undecoratedTypeName );
            HtmlNode indexAElementNode = htmlDoc.CreateTextNode( indexAElementString );
            xmlNode.AppendChild( indexAElementNode );

            string indexKElementString = String.Format(
                indexKElement,
                undecoratedTypeName,
                definedType.LanguageElementName );
            HtmlNode indexKElementNode = htmlDoc.CreateTextNode( indexKElementString );
            xmlNode.AppendChild( indexKElementNode );

            string tocTitleElementString = String.Format(
                tocTitleElement,
                undecoratedTypeName,
                definedType.LanguageElementName );
            HtmlNode tocTitleElementNode = htmlDoc.CreateTextNode( tocTitleElementString );
            xmlNode.AppendChild( tocTitleElementNode );

            string rlTitleElementString = String.Format(
                rlTitleElement,
                undecoratedTypeName,
                definedType.LanguageElementName );
            HtmlNode rlTitleElementNode = htmlDoc.CreateTextNode( rlTitleElementString );
            xmlNode.AppendChild( rlTitleElementNode );

            //<MSHelp:Attr Name="APIName" Value="Windows.Foundation.AsyncStatus"></MSHelp:Attr>
            //<MSHelp:Attr Name="APILocation" Value="Windows.Foundation.dll"></MSHelp:Attr>
            //<MSHelp:Attr Name="TopicType" Value="kbSyntax"></MSHelp:Attr>
            //<MSHelp:Attr Name="AssetID" Value="T:Windows.Foundation.AsyncStatus"></MSHelp:Attr>

            string mshelpAttrElementsString = String.Format(
                mshelpAttrElements,
                definedType.FullName,
                "Windows.winmd", // TBD: do the right thing here
                definedType.TopicId );
            HtmlNode mshelpAttrElementsNode = htmlDoc.CreateTextNode( mshelpAttrElementsString );
            xmlNode.AppendChild( mshelpAttrElementsNode );
        }

        private static HtmlNode GetHeadTag( HtmlDocument htmlDoc )
        {
            HtmlNode headNode = htmlDoc.DocumentNode.Descendants( "head" ).First();
            return headNode;
        }

        private static HtmlNode GetBodyTag( HtmlDocument htmlDoc )
        {
            HtmlNode headNode = htmlDoc.DocumentNode.Descendants( "body" ).First();
            return headNode;
        }

        private static HtmlNode GetHtmlTag( HtmlDocument htmlDoc )
        {
            HtmlNode htmlNode = htmlDoc.DocumentNode.ChildNodes.FirstOrDefault( n => n.Name == "html" );
            return htmlNode;
        }

        private void PopulateDocumentTitleAndDescription( HtmlDocument htmlDoc, DefinedType definedType )
        {
            if( IsForDocumentationOnly( definedType ) )
            {
                string sectionName = GetDocumentationOnlySection( definedType );

                string titleBlockString = String.Format(
                    titleBlockElements,
                    sectionName,
                    String.Empty );
                HtmlNode titleNode = htmlDoc.CreateTextNode( titleBlockString );
                HtmlNode bodyNode = GetBodyTag( htmlDoc );
                bodyNode.AppendChild( titleNode );

                if( definedType.Content != null )
                {
                    string abstractDivString = String.Format( pElement, definedType.Content.Abstract );
                    HtmlNode abstractNode = htmlDoc.CreateTextNode( abstractDivString );
                    bodyNode.AppendChild( abstractNode );

                    //string propertySectionString = String.Format( sectionBlockElements, "Properties" );
                    //HtmlNode propertiesSectionNode = htmlDoc.CreateTextNode( propertySectionString );
                    //bodyNode.AppendChild( propertiesSectionNode );
                }

                HtmlNode hrNode = htmlDoc.CreateTextNode( hrElements );
                bodyNode.AppendChild( hrNode );
            }
        }

        private void PopulateTitleAndDescription( HtmlDocument htmlDoc, DefinedType definedType )
        {
            string typeName = null;

            if( definedType.IsNamespace )
            {
                typeName = definedType.FullName;
            }
            else if(
                !( definedType.ParentType is DoxygenFacadeType ) &&
                !definedType.ParentType.IsNamespace )
            {
                typeName = String.Format(
                    "{0}.{1}",
                    definedType.ParentType.Name,
                    definedType.FriendlyName );
            }
            else
            {
                typeName = definedType.FriendlyName;
            }

            string titleBlockString = String.Format(
                titleBlockElements,
                typeName,
                definedType.LanguageElementName );
            HtmlNode titleNode = htmlDoc.CreateTextNode( titleBlockString );
            HtmlNode bodyNode = GetBodyTag( htmlDoc );
            bodyNode.AppendChild( titleNode );

            // Render the source code language.
            if( !String.IsNullOrEmpty( definedType.SourceLanguage ) )
            {
                string sourceLanguage = String.Format(
                    sourceLanguageFormatString,
                    definedType.SourceLanguage );
                string sourceLanguageString = String.Format(
                    divElement,
                    sourceLanguage );
                HtmlNode sourceLanguageNode = htmlDoc.CreateTextNode( sourceLanguageString );
                bodyNode.AppendChild( sourceLanguageNode );
            }

            if( definedType.IsClass || definedType.IsInterface )
            {
                PopulateInheritsSection( htmlDoc, definedType );
                PopulateDerivedTypesSection( htmlDoc, definedType );
            }

            if( definedType.Content != null )
            {
                string abstractDivString = String.Format( pElement, definedType.Content.Abstract );
                HtmlNode abstractNode = htmlDoc.CreateTextNode( abstractDivString );
                bodyNode.AppendChild( abstractNode );

                this.PopulateRemarksSection( htmlDoc, bodyNode, definedType.Content );

                //if( definedType.Content.Description != String.Empty )
                //{
                //    string remarksDivString = String.Format( 
                //        pHeadingElement, 
                //        "Remarks" );
                //    HtmlNode remarksNode = htmlDoc.CreateTextNode( remarksDivString );
                //    bodyNode.AppendChild( remarksNode );

                //    string descriptionDivString = String.Format( 
                //        pElement, 
                //        definedType.Content.Description );
                //    HtmlNode descriptionNode = htmlDoc.CreateTextNode( descriptionDivString );
                //    bodyNode.AppendChild( descriptionNode );

                //    foreach( var para in definedType.Content.Paragraphs )
                //    {
                //        string paraDivString = String.Format( pElement, para.Content );
                //        HtmlNode paraNode = htmlDoc.CreateTextNode( paraDivString );
                //        bodyNode.AppendChild( paraNode );
                //    }
                //}
            }
            HtmlNode hrNode = htmlDoc.CreateTextNode( hrElements );
            bodyNode.AppendChild( hrNode );
        }

        private void PopulateRemarksSection(
            HtmlDocument htmlDoc,
            HtmlNode parentNode,
            ReferenceContent content )
        {
            if( content.Description != String.Empty )
            {
                string remarksDivString = String.Format(
                    pHeadingElement,
                    "Remarks" );
                HtmlNode remarksNode = htmlDoc.CreateTextNode( remarksDivString );
                parentNode.AppendChild( remarksNode );

                string descriptionDivString = String.Format(
                    pElement,
                    content.Description );
                HtmlNode descriptionNode = htmlDoc.CreateTextNode( descriptionDivString );
                parentNode.AppendChild( descriptionNode );

                foreach( var para in content.Paragraphs )
                {
                    string paraDivString = String.Format( pElement, para.Content );
                    HtmlNode paraNode = htmlDoc.CreateTextNode( paraDivString );
                    parentNode.AppendChild( paraNode );
                }
            }
        }

        private void PopulateInheritsSection( HtmlDocument htmlDoc, DefinedType definedType )
        {
            string baseTypesDiv = String.Empty;
            for( int i = 0; i < definedType.BaseTypes.Count; i++ )
            {
                if( i == 0 )
                {
                    baseTypesDiv = "<div>inherits ";
                }

                DefinedType baseType = definedType.BaseTypes[i];
                string link = Utilities.GetAnchor(
                    baseType,
                    this.KnownTypes,
                    this.SiteConfigReferenceRoot,
                    false );

                baseTypesDiv += link;
                if( i < definedType.BaseTypes.Count - 1 )
                {
                    baseTypesDiv += ", ";
                }
                else
                {
                    baseTypesDiv += "</div><br>";
                }
            }

            if( baseTypesDiv != String.Empty )
            {
                HtmlNode inheritsNode = htmlDoc.CreateTextNode( baseTypesDiv );
                HtmlNode bodyNode = GetBodyTag( htmlDoc );
                bodyNode.AppendChild( inheritsNode );
            }
        }

        private void PopulateDerivedTypesSection( HtmlDocument htmlDoc, DefinedType definedType )
        {
            string derivedTypesDiv = String.Empty;
            for( int i = 0; i < definedType.DerivedTypes.Count; i++ )
            {
                if( i == 0 )
                {
                    derivedTypesDiv = "<div>derived types: ";
                }

                DefinedType derivedType = definedType.DerivedTypes[i];
                string link = Utilities.GetAnchor(
                    derivedType,
                    this.KnownTypes,
                    this.SiteConfigReferenceRoot,
                    false );

                derivedTypesDiv += link;
                if( i < definedType.DerivedTypes.Count - 1 )
                {
                    derivedTypesDiv += ", ";
                }
                else
                {
                    derivedTypesDiv += "</div><br>";
                }
            }

            if( derivedTypesDiv != String.Empty )
            {
                HtmlNode derivedTypesNode = htmlDoc.CreateTextNode( derivedTypesDiv );
                HtmlNode bodyNode = GetBodyTag( htmlDoc );
                bodyNode.AppendChild( derivedTypesNode );
            }
        }


        private HtmlDocument SerializeEnumToHtml( HtmlDocument enumDoc, DefinedType enumType )
        {
            //HtmlDocument enumDoc = new HtmlDocument();

            PopulateTitleAndDescription( enumDoc, enumType );

            HtmlNode tableNode = enumDoc.CreateElement( "table" );
            string tableHeaderString = String.Format( threeColumnTableHeaderElements, "Enum", "Value", "Description" );

            foreach( DefinedMember field in enumType.Members )
            {
                string tableRowString = String.Format(
                    threeColumnTableRowElements,
                    field.Name,
                    field.Value,
                    field.Content != null ? field.Content.Abstract : String.Empty );
                tableHeaderString += tableRowString;
            }

            tableNode.InnerHtml = tableHeaderString;
            HtmlNode bodyNode = GetBodyTag( enumDoc );
            bodyNode.AppendChild( tableNode );

            return enumDoc;
        }

        private HtmlDocument SerializeStructToHtml( HtmlDocument htmlDoc, DefinedType structType )
        {
            PopulateTitleAndDescription( htmlDoc, structType );

            HtmlNode tableNode = htmlDoc.CreateElement( "table" );
            string tableHeaderString = String.Format(
                threeColumnTableHeaderElements,
                "Field",
                "Type",
                "Description" );

            foreach( DefinedMember field in structType.Members )
            {
                string fieldTypeString = null;
                if( field.Type != null )
                {
                    fieldTypeString = Utilities.GetAnchor(
                        field.Type,
                        this.KnownTypes,
                        this.SiteConfigReferenceRoot,
                        false );
                }
                else
                {
                    fieldTypeString = field.Type.Name;
                }



                string tableRowString = String.Format(
                    threeColumnTableRowElements,
                    field.Name,
                    fieldTypeString,
                    field.Content.Abstract );
                tableHeaderString += tableRowString;
            }

            tableNode.InnerHtml = tableHeaderString;
            HtmlNode bodyNode = GetBodyTag( htmlDoc );
            bodyNode.AppendChild( tableNode );

            return htmlDoc;
        }

        private HtmlDocument SerializeNamespaceToHtml( HtmlDocument htmlDoc, DefinedType namespaceType )
        {
            PopulateTitleAndDescription( htmlDoc, namespaceType );

            HtmlNode childTableNode = htmlDoc.CreateElement( "table" );
            string tableHeaderString = String.Format(
                threeColumnTableHeaderElements,
                "Type name",
                "Type",
                "Description" );

            var orderedChildTypes = namespaceType.ChildTypes.OrderBy( t => t.Name );
            foreach( DefinedType childType in orderedChildTypes )
            {
                // If emitting projected types only, and the child type
                // isn't projected, skip it.
                if( this.ProjectedTypesOnly && !( childType is ProjectedType ) )
                {
                    continue;
                }

                string childTypeAnchor = Utilities.GetAnchor(
                    childType,
                    this.KnownTypes,
                    this.SiteConfigReferenceRoot,
                    false );

                string tableRowString = String.Format(
                    threeColumnTableRowElements,
                    childTypeAnchor,
                    childType.LanguageElementName,
                    childType.Content.Abstract );
                tableHeaderString += tableRowString;

                if( !childType.IsNamespace && childType.HasChildTypes )
                {
                    foreach( var innerType in childType.ChildTypes )
                    {
                        string innerTypeAnchor = Utilities.GetAnchor(
                            innerType,
                            this.KnownTypes,
                            this.SiteConfigReferenceRoot,
                            false );

                        tableRowString = String.Format(
                            threeColumnTableRowElements,
                            innerTypeAnchor,
                            innerType.LanguageElementName,
                            innerType.Content.Abstract );
                        tableHeaderString += tableRowString;
                    }
                }
            }

            childTableNode.InnerHtml = tableHeaderString;
            HtmlNode bodyNode = GetBodyTag( htmlDoc );
            bodyNode.AppendChild( childTableNode );

            return htmlDoc;
        }

        private HtmlDocument SerializeClassToHtml( HtmlDocument htmlDoc, DefinedType classType )
        {
            PopulateTitleAndDescription( htmlDoc, classType );

            if( classType.HasConstructors )
            {
                PopulateConstructors( htmlDoc, classType );
            }

            if( classType.HasDestructor )
            {
                PopulateDestructor( htmlDoc, classType );
            }

            if( classType.HasProperties )
            {
                PopulateProperties( htmlDoc, classType );
            }

            if( classType.HasMethods )
            {
                PopulateMethods( htmlDoc, classType );
            }

            if( classType.HasEvents )
            {
                PopulateEvents( htmlDoc, classType );
            }

            return htmlDoc;
        }

        private HtmlDocument SerializeFunctionToHtml( HtmlDocument htmlDoc, DefinedType functionType )
        {
            PopulateTitleAndDescription( htmlDoc, functionType );

            DoxygenFunction doxygenFunction = functionType as DoxygenFunction;
            if( doxygenFunction != null )
            {
                PopulateMember( htmlDoc, doxygenFunction.UnderlyingMethod );
            }

            return htmlDoc;
        }

        private void PopulateProperties( HtmlDocument htmlDoc, DefinedType definedType )
        {
            if( definedType.HasProperties )
            {
                string propertySectionString = String.Format( sectionBlockElements, "Properties" );
                HtmlNode propertiesSectionNode = htmlDoc.CreateTextNode( propertySectionString );
                HtmlNode bodyNode = GetBodyTag( htmlDoc );
                bodyNode.AppendChild( propertiesSectionNode );

                foreach( DefinedMember property in definedType.Properties )
                {
                    PopulateMember( htmlDoc, property );
                }
            }
        }

        private void PopulateDocumentationProperties( HtmlDocument htmlDoc, DefinedType definedType )
        {
            if( IsForDocumentationOnly( definedType ) )
            {
                string propertySectionString = String.Format( sectionBlockElements, "Overview" );
                HtmlNode propertiesSectionNode = htmlDoc.CreateTextNode( propertySectionString );
                HtmlNode bodyNode = GetBodyTag( htmlDoc );
                bodyNode.AppendChild( propertiesSectionNode );

                // By convention, only put docs on properties.
                foreach( DefinedMember property in definedType.Properties )
                {
                    PopulateDocumentationMember( htmlDoc, property );
                }
            }
        }

        private void PopulateConstructors( HtmlDocument htmlDoc, DefinedType definedType )
        {
            try
            {
                if( definedType.HasConstructors )
                {
                    string constructorSectionString = String.Format( sectionBlockElements, "Constructors" );
                    HtmlNode sectionNode = htmlDoc.CreateTextNode( constructorSectionString );
                    HtmlNode bodyNode = GetBodyTag( htmlDoc );
                    bodyNode.AppendChild( sectionNode );

                    foreach( DefinedMember ctor in definedType.Constructors )
                    {
                        PopulateMember( htmlDoc, ctor );
                    }
                }
            }
            catch( Exception ex )
            {
                GenTopicsEventLogger.Log.LogException( ex );
                throw;
            }
        }

        private void PopulateDestructor( HtmlDocument htmlDoc, DefinedType definedType )
        {
            try
            {
                if( definedType.HasDestructor )
                {
                    string destructorSectionString = String.Format( sectionBlockElements, "Destructor" );
                    HtmlNode sectionNode = htmlDoc.CreateTextNode( destructorSectionString );
                    HtmlNode bodyNode = GetBodyTag( htmlDoc );
                    bodyNode.AppendChild( sectionNode );

                    PopulateMember( htmlDoc, definedType.Destructor );
                }
            }
            catch( Exception ex )
            {
                Debug.WriteLine( ex.ToString() );
                throw;
            }
        }


        private void PopulateMethods( HtmlDocument htmlDoc, DefinedType definedType )
        {
            if( definedType.HasMethods )
            {
                string methodSectionString = String.Format( sectionBlockElements, "Methods" );
                HtmlNode sectionNode = htmlDoc.CreateTextNode( methodSectionString );
                HtmlNode bodyNode = GetBodyTag( htmlDoc );
                bodyNode.AppendChild( sectionNode );

                foreach( DefinedMember method in definedType.Methods )
                {
                    PopulateMember( htmlDoc, method );
                }
            }
        }

        private void PopulateEvents( HtmlDocument htmlDoc, DefinedType definedType )
        {
            if( definedType.HasEvents )
            {
                string eventSectionString = String.Format( sectionBlockElements, "Events" );
                HtmlNode sectionNode = htmlDoc.CreateTextNode( eventSectionString );
                HtmlNode bodyNode = GetBodyTag( htmlDoc );
                bodyNode.AppendChild( sectionNode );

                foreach( DefinedMember member in definedType.Events )
                {
                    PopulateMember( htmlDoc, member );
                }
            }
        }

        private void PopulateDocumentationMember( HtmlDocument htmlDoc, DefinedMember member )
        {
            try
            {
                AssemblyMember assemblyMember = member as AssemblyMember;
                string sectionName = GetDocumentationOnlySection( assemblyMember.UnderlyingMember );

                string sectionIdString = GetSectionId( member );
                HtmlNode sectionNode = htmlDoc.CreateElement( "section" );
                HtmlAttribute idAttribute = htmlDoc.CreateAttribute( "id", sectionIdString );
                sectionNode.Attributes.Add( idAttribute );
                HtmlNode bodyNode = GetBodyTag( htmlDoc );
                bodyNode.AppendChild( sectionNode );

                string memberDivString = String.Format(
                    methodDivElements,
                    sectionName );
                HtmlNode memberNode = htmlDoc.CreateTextNode( memberDivString );
                sectionNode.ChildNodes.Add( memberNode );

                //string memberSignatureString = null;

                //if( member.IsProperty )
                //{
                //    string propertyType = null;
                //    if( member.Type != null )
                //    {
                //        propertyType = Utilities.GetAnchor(
                //            member.Type,
                //            this.KnownTypes,
                //            this.SiteConfigReferenceRoot,
                //            false );
                //    }
                //    else
                //    {
                //        propertyType = member.Type.Name;
                //    }

                //    memberSignatureString = String.Format(
                //        propertySignatureFormatString,
                //        propertyType,
                //        member.ParentType.Name,
                //        member.Name );
                //}

                if( member.Content != null )
                {
                    HtmlNode abstractNode = htmlDoc.CreateTextNode( member.Content.Abstract );
                    sectionNode.ChildNodes.Add( abstractNode );

                    this.PopulateRemarksSection( htmlDoc, sectionNode, member.Content );
                }

                HtmlNode hrNode = htmlDoc.CreateTextNode( hrElements );
                bodyNode.AppendChild( hrNode );

            }
            catch( Exception ex )
            {
                Debug.Write( ex.ToString() );
                throw;
            }
        }


        private void PopulateMember( HtmlDocument htmlDoc, DefinedMember member )
        {
            try
            {
                if( !member.IsEventAccessor &&
                    !member.IsPropertyAccessor &&
                    !member.IsSystemObjectMember )
                {
                    string sectionIdString = GetSectionId( member );
                    HtmlNode sectionNode = htmlDoc.CreateElement( "section" );
                    HtmlAttribute idAttribute = htmlDoc.CreateAttribute( "id", sectionIdString );
                    sectionNode.Attributes.Add( idAttribute );
                    HtmlNode bodyNode = GetBodyTag( htmlDoc );
                    bodyNode.AppendChild( sectionNode );

                    string memberDivString = String.Format(
                        methodDivElements,
                        member.IsConstructor ? member.ParentType.Name : member.Name );
                    HtmlNode memberNode = htmlDoc.CreateTextNode( memberDivString );
                    sectionNode.ChildNodes.Add( memberNode );

                    string memberSignatureString = null;

                    if( member.IsProperty )
                    {
                        string propertyAccessorString = member.IsMutable == true ? "read-write" : "read-only";
                        string propertyString = String.Format( "{0} {1}", member.Name, propertyAccessorString );
                        sectionNode.ChildNodes.Remove( memberNode ); // TBD: a bit klunky
                        memberDivString = String.Format( propertyDivElements, member.Name, propertyAccessorString );
                        memberNode = htmlDoc.CreateTextNode( memberDivString );
                        sectionNode.ChildNodes.Add( memberNode );

                        string propertyType = null;
                        if( member.Type != null )
                        {
                            propertyType = Utilities.GetAnchor(
                                member.Type,
                                this.KnownTypes,
                                this.SiteConfigReferenceRoot,
                                false );
                        }
                        else
                        {
                            propertyType = member.Type.Name;
                        }

                        memberSignatureString = String.Format(
                            propertySignatureFormatString,
                            propertyType,
                            member.ParentType.Name,
                            member.Name );
                    }
                    else if( member.IsMethod || member.IsEvent )
                    {
                        string memberType = member.Type.Name;
                        if( member.Type != null )
                        {
                            memberType = Utilities.GetAnchor(
                                member.Type,
                                this.KnownTypes,
                                this.SiteConfigReferenceRoot,
                                false );
                        }

                        string signature = Utilities.GetMethodSignature(
                            member,
                            this.KnownTypes,
                            this.SiteConfigReferenceRoot,
                            this.Namespaces );

                        memberSignatureString = String.Format(
                            methodSignatureFormatString,
                            signature );
                    }
                    else if( member.IsConstructor )
                    {
                        string signature = Utilities.GetMethodSignature(
                            member,
                            this.KnownTypes,
                            this.SiteConfigReferenceRoot,
                            this.Namespaces );

                        memberSignatureString = String.Format(
                            methodSignatureFormatString,
                            signature );
                    }
                    else if( member.IsDestructor )
                    {
                        string signature = Utilities.GetMethodSignature(
                            member,
                            this.KnownTypes,
                            this.SiteConfigReferenceRoot,
                            this.Namespaces );

                        memberSignatureString = String.Format(
                            methodSignatureFormatString,
                            signature );
                    }

                    else if( member.IsEvent )
                    {
                        //memberSignatureString = String.Format(
                        //    eventSignatureFormatString,
                        //    member.definition );
                    }

                    if( memberSignatureString != null )
                    {
                        HtmlNode memberSignatureNode = htmlDoc.CreateTextNode( memberSignatureString );
                        sectionNode.ChildNodes.Add( memberSignatureNode );
                    }

                    if( member.Content != null )
                    {
                        HtmlNode abstractNode = htmlDoc.CreateTextNode( member.Content.Abstract );
                        sectionNode.ChildNodes.Add( abstractNode );

                        this.PopulateRemarksSection( htmlDoc, sectionNode, member.Content );
                    }

                    HtmlNode paramTableNode = htmlDoc.CreateElement( "table" );
                    string tableHeaderString = String.Format( threeColumnTableHeaderElements, "Parameter", "Type", "Description" );

                    if( member.HasParameters )
                    {
                        foreach( DefinedParameter param in member.Parameters )
                        {
                            bool isGenericType = param.Type is GenericParameterType;

                            string paramType = null;

                            if( isGenericType )
                            {
                                paramType = param.Type.Name;
                            }
                            else
                            {
                                paramType = Utilities.GetAnchor(
                                    param.Type,
                                    this.KnownTypes,
                                    this.SiteConfigReferenceRoot,
                                    true );
                            }

                            if( param.IsArray )
                            {
                                paramType += Utilities.arrayCharacters;
                            }

                            if( param.IsReference )
                            {
                                paramType += Utilities.referenceCharacter;
                            }

                            if( param.IsPointer )
                            {
                                paramType += Utilities.pointerCharacter;

                                if( param.PointerDepth == 2 )
                                {
                                    paramType += Utilities.pointerCharacter;
                                }
                            }

                            string tableRowString = String.Format(
                                paramTableRowElements,
                                param.Name,
                                paramType,
                                param.Content != null ? param.Content.Abstract : String.Empty );
                            tableHeaderString += tableRowString;
                        }

                        paramTableNode.InnerHtml = tableHeaderString;
                        sectionNode.ChildNodes.Add( paramTableNode );
                    }

                    HtmlNode hrNode = htmlDoc.CreateTextNode( hrElements );
                    bodyNode.AppendChild( hrNode );
                }
            }
            catch( Exception ex )
            {
                GenTopicsEventLogger.Log.LogException( ex );
                throw;
            }
        }

        private void PopulateTimestamp( HtmlDocument htmlDoc )
        {
            string timestampString = String.Format(
                timestampFormatString,
                DateTime.Now.ToLongDateString(),
                Environment.MachineName );
            HtmlNode sectionNode = htmlDoc.CreateTextNode( timestampString );
            HtmlNode bodyNode = GetBodyTag( htmlDoc );
            bodyNode.AppendChild( sectionNode );
        }

        private DefinedType ResolveType( DefinedType definedType, List<DefinedType> knownTypes )
        {
            DefinedType resolvedType = null;

            return resolvedType;
        }

        private List<DefinedType> GetHighestLevelNamespace( List<DefinedType> definedTypes )
        {
            var topLevelNamespaces = definedTypes.Where( t => IsTopLevelNamespace( t ) ).ToList();
            return topLevelNamespaces;
        }

        private void AddNamespaceToIndex( DefinedType ns, HtmlDocument indexDoc, HtmlNode parentNode )
        {
            HtmlNode ulNode = indexDoc.CreateElement( "ul" );
            parentNode.AppendChild( ulNode );

            string anchor = Utilities.GetAnchor(
                ns,
                this.KnownTypes,
                this.SiteConfigReferenceRoot,
                false );

            string nodeText = String.Format( indexNameSpaceAnchorItem, anchor, ns.Content.Abstract );
            HtmlNode namespaceNode = indexDoc.CreateTextNode( nodeText );
            ulNode.AppendChild( namespaceNode );

            var childNamespaces = ns.ChildTypes.Where( c => c.IsNamespace );
            if( childNamespaces != null )
            {
                foreach( var childNamespace in childNamespaces )
                {
                    AddNamespaceToIndex( childNamespace, indexDoc, ulNode );
                }
            }
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

        private static bool IsForDocumentationOnly( DefinedType definedType )
        {
            bool isForDocumentationOnly = false;

            ProjectedType projectedType = definedType as ProjectedType;
            if( projectedType != null )
            {
                isForDocumentationOnly = GetDocumentationOnlyAttribute( projectedType ) != null;
            }

            return isForDocumentationOnly;
        }

        private static CustomAttributeData GetDocumentationOnlyAttribute( DefinedType definedType )
        {
            CustomAttributeData atttributeData = null;

            // TBD: Move to AssemblyType and ProjectedType
            if( definedType is ProjectedType )
            {
                ProjectedType projectedType = definedType as ProjectedType;
                atttributeData = GetDocumentationOnlyAttribute( projectedType.AssemblyType );
            }
            else if( definedType is AssemblyType )
            {
                AssemblyType assemblyType = definedType as AssemblyType;
                atttributeData = GetDocumentationOnlyAttribute( assemblyType );
            }

            return atttributeData;
        }


        private static CustomAttributeData GetDocumentationOnlyAttribute( AssemblyType assemblyType )
        {
            CustomAttributeData attributeData = null;

            if( assemblyType != null &&
                assemblyType.UnderlyingType != null &&
                assemblyType.UnderlyingType.Attributes != null )
            {
                attributeData = assemblyType.UnderlyingType.Attributes.FirstOrDefault( a =>
                    a.AttributeType.Name == "DocumentationOnlyAttribute" );
            }

            return attributeData;
        }

        private static CustomAttributeData GetDocumentationOnlyAttribute( MemberInfo memberInfo )
        {
            var attrs = memberInfo.GetCustomAttributesData();
            var docOnlyAttr = attrs.FirstOrDefault( a =>
                a.AttributeType.Name == "DocumentationOnlyAttribute" );
            return docOnlyAttr;
        }

        private static string GetDocumentationOnlySection( MemberInfo memberInfo )
        {
            CustomAttributeData docOnlyAttr = GetDocumentationOnlyAttribute( memberInfo );
            return GetDocumentationOnlySection( docOnlyAttr );
        }


        private static string GetDocumentationOnlySection( DefinedType definedType )
        {
            CustomAttributeData docOnlyAttr = GetDocumentationOnlyAttribute( definedType );
            return GetDocumentationOnlySection( docOnlyAttr );
        }

        private static string GetDocumentationOnlySection( CustomAttributeData docOnlyAttr )
        {
            string sectionName = String.Empty;

            if( docOnlyAttr.ConstructorArguments.Count > sectionNameIndex )
            {
                sectionName = docOnlyAttr.ConstructorArguments[sectionNameIndex].Value as string;
            }

            return sectionName;
        }

        public static string GetSectionId( DefinedType definedType )
        {
            string id = String.Format(
                "{0}_{1}",
                definedType.LanguageElement,
                definedType.Name.ToLower() );

            return id;
        }

        public static string GetSectionId( DefinedMember member )
        {
            string id = String.Format( "{0}_{1}", member, member.Name.ToLower() );
            return id;
        }


        //        <head>
        //<meta http-equiv="Content-Type" content="text/html; charset=UTF-8"/>
        //<meta name="Description" content="Specifies the status of an asynchronous operation."/>
        //<meta name="MSHAttr" content="PreferredSiteName:MSDN"/>
        //<meta name="MSHAttr" content="PreferredLib:/library/windows/apps"/>
        //<title>AsyncStatus enumeration</title>
        //<xml>
        //<MSHelp:Keyword Index="A" Term="w_found.asyncstatus"/>
        //<MSHelp:TOCTitle Title="AsyncStatus enumeration"></MSHelp:TOCTitle>
        //<MSHelp:RLTitle Title="AsyncStatus enumeration - Windows app development"></MSHelp:RLTitle>
        //<MSHelp:Keyword Index="K" Term="AsyncStatus enumeration"></MSHelp:Keyword>
        //<MSHelp:Keyword Index="F" Term="Windows.Foundation.AsyncStatus"></MSHelp:Keyword>
        //<MSHelp:Keyword Index="F" Term="Windows.Foundation.AsyncStatus.Canceled"></MSHelp:Keyword>
        //<MSHelp:Keyword Index="F" Term="Windows.Foundation.AsyncStatus.Completed"></MSHelp:Keyword>
        //<MSHelp:Keyword Index="F" Term="Windows.Foundation.AsyncStatus.Error"></MSHelp:Keyword>
        //<MSHelp:Keyword Index="F" Term="Windows.Foundation.AsyncStatus.Started"></MSHelp:Keyword>
        //<MSHelp:Keyword Index="F" Term="Windows::Foundation::AsyncStatus"></MSHelp:Keyword>
        //<MSHelp:Keyword Index="F" Term="Windows::Foundation::AsyncStatus::Canceled"></MSHelp:Keyword>
        //<MSHelp:Keyword Index="F" Term="Windows::Foundation::AsyncStatus::Completed"></MSHelp:Keyword>
        //<MSHelp:Keyword Index="F" Term="Windows::Foundation::AsyncStatus::Error"></MSHelp:Keyword>
        //<MSHelp:Keyword Index="F" Term="Windows::Foundation::AsyncStatus::Started"></MSHelp:Keyword>
        //<MSHelp:Attr Name="Locale" Value="kbEnglish"></MSHelp:Attr>
        //<MSHelp:Attr Name="APIType" Value="Assembly"></MSHelp:Attr>
        //<MSHelp:Attr Name="APIName" Value="Windows.Foundation.AsyncStatus"></MSHelp:Attr>
        //<MSHelp:Attr Name="APILocation" Value="Windows.Foundation.dll"></MSHelp:Attr>
        //<MSHelp:Attr Name="TopicType" Value="kbSyntax"></MSHelp:Attr>
        //<MSHelp:Attr Name="AssetID" Value="T:Windows.Foundation.AsyncStatus"></MSHelp:Attr>
        //</xml>
        //</head>
        private const int sectionNameIndex = 1;

        private const string divElement = "<div>{0}</div>\n";
        private const string pElement = "<p>{0}</p>\n";
        private const string pHeadingElement = "<p><h3>{0}</h3></p>\n";
        private const string twoColumnTableHeaderElements = "<tr><th>{0}</th><th>{1}</th></tr>\n";
        private const string twoColumnTableRowElements = "<tr><td>{0}</td><td>{1}</td></tr>\n";
        private const string threeColumnTableHeaderElements = "<tr><th>{0}</th><th>{1}</th><th>{2}</th></tr>\n";
        private const string threeColumnTableRowElements = "<tr><td>{0}</td><td>{1}</td><td>{2}</td></tr>\n";
        private const string titleBlockElements = "<div><h1>{0} {1}</h1></div>\n";
        private const string sectionBlockElements = "<div><h2>{0}</h2></div>\n";
        private const string propertyDivElements = "<div><h3>{0} ({1})</h3></div>\n";
        private const string methodDivElements = "<div><h3>{0}</h3></div>\n";
        //private const string sectionDivElements = "<section id='{0}'></section>\n";
        private const string paramTableRowElements = "<tr><td>{0}</td><td><code>{1}</code></td><td>{2}</td></tr>\n";
        private const string hrElements = "<div><hr size=\"1\" /></div>\n";

        private const string propertySignatureFormatString = "<div><code>{0} {1}.{2}</code></div>\n";
        private const string methodSignatureFormatString = "<div><code>{0}</code></div>\n";
        //private const string eventSignatureFormatString = "<div><code>{0}</code></div>\n";
        private const string timestampFormatString = "<div><br>Generated: {0} on {1}</div>\n";
        private const string sourceLanguageFormatString = "source language: {0}";

        private const string indexTitleElements = "<div><h1>V0.2 MPC APIs</h1></div>\n";
        private const string indexNameSpaceAnchorItem = "<li>{0} | {1}</li>\n";

        private const string htmlTag = "<!DOCTYPE html>\n" +
            "<html dir=\"ltr\" xmlns=\"http://www.w3.org/1999/xhtml\" lang=\"en\">\n";

        private const string metaTags = "<meta http-equiv=\"Content-Type\" content=\"text/html; charset=UTF-8\"/>\n" +
            "<meta name=\"Description\" content=\"{0}\"/>\n" +
            "<meta name=\"MSHAttr\" content=\"PreferredSiteName:MSDN\"/>\n" +
            "<meta name=\"MSHAttr\" content=\"PreferredLib:/library/windows/apps\"/>\n" +
            "<title>{1} {2}</title>";
        private const string xmlTag = "xml";

        private const string tocTitleElement = "<MSHelp:TOCTitle Title=\"{0} {1}\"></MSHelp:TOCTitle>\n";
        private const string rlTitleElement = "<MSHelp:RLTitle Title=\"{0} {1} - Windows app development\"></MSHelp:RLTitle>\n";

        private const string indexAElement = "<MSHelp:Keyword Index=\"A\" Term=\"{0}\"/>\n";
        private const string indexKElement = "<MSHelp:Keyword Index=\"K\" Term=\"{0} {1}\"/>\n";

        private const string mshelpAttrElements =
            "<MSHelp:Attr Name=\"Locale\" Value=\"kbEnglish\"></MSHelp:Attr>\n" +
            "<MSHelp:Attr Name=\"APIType\" Value=\"Assembly\"></MSHelp:Attr>\n" +
            "<MSHelp:Attr Name=\"APIName\" Value=\"{0}\"></MSHelp:Attr>\n" +
            "<MSHelp:Attr Name=\"APILocation\" Value=\"{1}\"></MSHelp:Attr>\n" +
            "<MSHelp:Attr Name=\"TopicType\" Value=\"kbSyntax\"></MSHelp:Attr>\n" +
            "<MSHelp:Attr Name=\"AssetID\" Value=\"{2}\"></MSHelp:Attr>\n";
    }
}
