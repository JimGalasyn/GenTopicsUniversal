using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

//using ReflectionUtilities;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen
{
    public class DoxType
    {
        public DoxType( Compound compound, string siteConfigReferenceRoot )
        {
            this.ContainingFolder = compound.ContainingFolder;
            this.OutputRootFolder = compound.OutputRootFolder;

            this.kind = compound.kind;
            this.id = compound.refid;
            string sourcePath = Path.Combine( this.ContainingFolder, compound.refid );
            this.SourceFilePath = Path.ChangeExtension( sourcePath, "xml" );
            XDocument doc = XDocument.Load( this.SourceFilePath );
            List<XElement> memberDefElements = Utilities.GetElements( doc, "memberdef" );

            this.RawName = compound.name;
            this.Name = Utilities.GetTypeName( this.RawName );
            this.ParentNamespaceFull = Utilities.GetParentNamespaceFull( this.RawName );
            this.ParentNamespace = Utilities.GetParentNamespace( this.ParentNamespaceFull );

            string relativePath = this.RawName.Replace( "Windows::", "" );
            this.RelativePath = relativePath.Replace( "::", "\\" );

            this.OutputFolder = Path.Combine( this.OutputRootFolder, this.RelativePath );
            this.FullPath = Path.Combine( this.OutputFolder, this.Name );
            this.FullPath = Path.ChangeExtension( this.FullPath, "md" );
            this.SiteConfigReferenceRootPath = siteConfigReferenceRoot;
            this.SiteConfigPath = this.GetSiteConfigPath2();

            this.MemberDefs = memberDefElements.Select( me => new MemberDef( me, this ) ).ToList();

            if( this.kind == "namespace" )
            {
                var childClasses = this.MemberDefs.Where( md => md.kind == "class" || md.kind == "enum" );
                this.ChildTypes = childClasses.Select( c => new DoxygenType( c, this ) ).ToList();

                string managedStyleName = this.RawName.Replace( "::", "." );
                this.TopicId = Utilities.GetTopicIdForNamespce( managedStyleName );
            }

            this.Properties = this.MemberDefs.Where( md => md.kind == "property" ).ToList();
            this.Methods = this.MemberDefs.Where( md => md.kind == "function" && md.IsEvent == false ).ToList();
            this.Events = this.MemberDefs.Where( md => md.IsEvent == true ).ToList();
            this.Fields = this.MemberDefs.Where( md => md.kind == "variable" ).ToList();

            this.BriefDescription = OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen.Utilities.GetBriefDescription( doc );
            this.DetailedDescription = OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen.Utilities.GetDetailedDescription( doc );

            List<XElement> basecompoundrefElements = Utilities.GetElements( doc, "basecompoundref" );
            this.BaseCompoundRefStrings = basecompoundrefElements.Select( b => b.Value ).ToList();
            List<XElement> derivedcompoundrefElements = Utilities.GetElements( doc, "derivedcompoundref" );
            this.DerivedCompoundRefStrings = derivedcompoundrefElements.Select( b => b.Value ).ToList();
        }

        public DoxType( MemberDef memberDef, DoxType parentDoxType )
        {
            if( memberDef.kind == "class" )
            {
                this.kind = "class";
            }
            else if( memberDef.kind == "enum" )
            {
                this.kind = "enum";
            }

            if( memberDef.kind == "class" || memberDef.kind == "enum" )
            {
                this.Name = memberDef.name;
                this.ParentNamespace = parentDoxType.Name;
                this.ParentNamespaceFull = String.Format( "{0}::{1}", parentDoxType.ParentNamespaceFull, parentDoxType.Name );
                this.BriefDescription = memberDef.BriefDescription;
                this.DetailedDescription = memberDef.DetailedDescription;

                this.ContainingFolder = parentDoxType.ContainingFolder;
                this.OutputRootFolder = parentDoxType.OutputRootFolder;

                string relativePath = this.ParentNamespaceFull.Replace( "Windows::", "" );
                relativePath = relativePath.Replace( "::", "\\" );
                this.RelativePath = Path.Combine( relativePath, this.Name );
                this.OutputFolder = Path.Combine( this.OutputRootFolder, this.RelativePath );
                this.FullPath = Path.Combine( this.OutputFolder, this.Name );
                this.FullPath = Path.ChangeExtension( this.FullPath, "md" );

                this.SiteConfigReferenceRootPath = parentDoxType.SiteConfigReferenceRootPath;
                this.SiteConfigPath = this.GetSiteConfigPath2();

                //this.Properties = this.MemberDefs.Where( md => md.kind == "property" ).ToList();
                //this.Methods = this.MemberDefs.Where( md => md.kind == "function" && md.IsEvent == false ).ToList();
                //this.Events = this.MemberDefs.Where( md => md.IsEvent == true ).ToList();
                //this.Fields = this.MemberDefs.Where( md => md.kind == "variable" ).ToList();

                this.Properties = new List<MemberDef>();
                this.Methods = new List<MemberDef>();
                this.Events = new List<MemberDef>();
                this.Fields = new List<MemberDef>();
                this.EnumValues = memberDef.EnumValues;
            }
            else
            {
                throw new ArgumentException( "Trying to create a DoxType from a member" );
            }

        }

        //public DoxType(
        //    ObservableType type,
        //    string serializationFolder,
        //    string siteConfigReferenceRoot )
        //{
        //    this.InitFromManagedType(
        //        type,
        //        serializationFolder,
        //        siteConfigReferenceRoot );
        //}

        public DoxType(
            string fullNamespace,
            string serializationFolder,
            string siteConfigReferenceRoot )
        {
            this.OutputRootFolder = serializationFolder;
            this.kind = "namespace";

            int indexOfName = fullNamespace.LastIndexOf( "." );

            if( indexOfName != -1 )
            {
                this.Name = fullNamespace.Substring( indexOfName + 1 );
                this.ParentNamespaceFull = fullNamespace.Substring( 0, indexOfName );

                int indexOfParentNamespace = this.ParentNamespaceFull.LastIndexOf( "." );
                if( indexOfParentNamespace != -1 )
                {
                    this.ParentNamespace = this.ParentNamespaceFull.Substring( indexOfParentNamespace + 1 );
                }
                else
                {
                    this.ParentNamespace = this.ParentNamespaceFull;
                }
            }
            else
            {
                this.Name = fullNamespace;
                this.ParentNamespaceFull = String.Empty;
                this.ParentNamespace = String.Empty;
            }

            if( this.ParentNamespace != String.Empty )
            {
                this.OutputFolder = Path.Combine( this.OutputRootFolder, this.Name );
            }
            else
            {
                this.OutputFolder = this.OutputRootFolder;
            }

            this.FullPath = Path.Combine( this.OutputFolder, this.Name );
            this.FullPath = Path.ChangeExtension( this.FullPath, "md" );
            this.SiteConfigReferenceRootPath = siteConfigReferenceRoot;

        }

        //public DoxType(
        //    ObservableType managedType,
        //    DoxType nativeType,
        //    string serializationFolder,
        //    string siteConfigReferenceRoot )
        //{
        //    this.InitFromManagedType( managedType, serializationFolder, siteConfigReferenceRoot );

        //    this.CopyContent( nativeType );
        //}

        //private void InitFromManagedType(
        //    ObservableType type,
        //    string serializationFolder,
        //    string siteConfigReferenceRoot )
        //{
        //    this.UnderlyingType = type;
        //    this.kind = Utilities.TranscribeKind( type );

        //    this.OutputRootFolder = serializationFolder;
        //    this.ParentNamespace = type.Namespace;
        //    this.RawName = type.FullName;
        //    this.TopicId = Utilities.GetTopicIdForType( type );

        //    int indexOfName = this.RawName.LastIndexOf( "." );
        //    this.Name = RawName.Substring( indexOfName + 1 );
        //    this.ParentNamespaceFull = RawName.Substring( 0, indexOfName );

        //    string relativePath = this.RawName.Replace( "Windows.", "" );
        //    relativePath = relativePath.Replace( ".", "\\" );
        //    this.RelativePath = relativePath; // Path.Combine( relativePath, this.Name );
        //    this.OutputFolder = Path.Combine( this.OutputRootFolder, this.Name );
        //    this.FullPath = Path.Combine( this.OutputFolder, this.Name );
        //    this.FullPath = Path.ChangeExtension( this.FullPath, "md" );
        //    this.SiteConfigReferenceRootPath = siteConfigReferenceRoot;
        //    this.SiteConfigPath = this.GetSiteConfigPath2();

        //    this.MemberDefs = type.Members.Select( m => new MemberDef( m, this ) ).ToList();
        //    OsgContentPublishing.ReferencePipelineLib.TypeTopology.TypeTopology.Doxygen.Utilities.FilterObjectMethods( this );
        //    OsgContentPublishing.ReferencePipelineLib.TypeTopology.TypeTopology.Doxygen.Utilities.FilterAccessorMethods( this );
        //    OsgContentPublishing.ReferencePipelineLib.TypeTopology.TypeTopology.Doxygen.Utilities.FilterEnumMethods( this );

        //    this.Properties = this.MemberDefs.Where( md => md.kind == "property" ).ToList();
        //    this.Methods = this.MemberDefs.Where( md => md.kind == "function" && !md.IsEvent ).ToList();
        //    this.Events = this.MemberDefs.Where( md => md.IsEvent ).ToList();
        //    this.Fields = this.MemberDefs.Where( md => md.kind == "field" ).ToList();
        //    this.Constructors = type.Constructors.Select( c => new MemberDef( c, this ) ).ToList();
        //    //this.MemberDefs.AddRange( this.Constructors );
        //    this.EnumValues = new List<EnumValue>();
        //}

        //// Content that can be copied directly from the corresponding native type. 
        //// This is content that doesn't come from base types / interfaces.
        //private void CopyContent( DoxType nativeType )
        //{
        //    if( this.Description == null || this.Description == String.Empty || this.Description == Utilities.emptyDescriptionNagString )
        //    {
        //        this.BriefDescription = nativeType.BriefDescription;
        //        this.DetailedDescription = nativeType.DetailedDescription;
        //    }

        //    if( this.IsInterface )
        //    {
        //        foreach( MemberDef member in this.MemberDefs )
        //        {
        //            if( member.Description == null || member.Description == String.Empty || member.Description == Utilities.emptyDescriptionNagString )
        //            {
        //                var nativeMember = nativeType.MemberDefs.FirstOrDefault( m => m.name == member.name );
        //                if( nativeMember != null )
        //                {
        //                    member.BriefDescription = nativeMember.BriefDescription;
        //                    member.DetailedDescription = nativeMember.DetailedDescription;

        //                    if( member.IsMethod || member.IsConstructor )
        //                    {
        //                        foreach( Param param in member.Parameters )
        //                        {
        //                            if( param.Description == null || param.Description == String.Empty || param.Description == Utilities.emptyDescriptionNagString )
        //                            {
        //                                foreach( Param baseParam in nativeMember.Parameters )
        //                                {
        //                                    if( param.declname == baseParam.declname )
        //                                    {
        //                                        param.BriefDescription = baseParam.BriefDescription;
        //                                    }
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    if( this.IsEnum )
        //    {
        //        this.EnumValues = nativeType.EnumValues;
        //    }

        //    if( this.HasFields )
        //    {
        //        foreach( var field in this.Fields )
        //        {
        //            if( field.Description == null || field.Description == String.Empty || field.Description == Utilities.emptyDescriptionNagString )
        //            {
        //                var nativeField = nativeType.Fields.FirstOrDefault( f => f.name == field.name );
        //                if( nativeField != null )
        //                {
        //                    field.BriefDescription = nativeField.BriefDescription;
        //                    field.DetailedDescription = nativeField.DetailedDescription;
        //                }
        //            }
        //        }
        //    }
        //}

        public void ResolveContent( List<DoxType> nativeTypes )
        {
            // Collect all of members from the base types of this type.
            List<MemberDef> allBaseMembers = new List<MemberDef>();
            foreach( DoxType baseType in this.BaseTypes )
            {
                allBaseMembers.AddRange( baseType.MemberDefs );
            }

            // Copy descriptions from the base members into this type's members.            
            foreach( MemberDef member in this.MemberDefs )
            {
                if( member.Description == null || member.Description == String.Empty || member.Description == Utilities.emptyDescriptionNagString )
                {
                    MemberDef baseMember = allBaseMembers.FirstOrDefault( m => m.name == member.name );
                    if( baseMember != null )
                    {
                        member.BriefDescription = baseMember.BriefDescription;
                        member.DetailedDescription = baseMember.DetailedDescription;

                        if( member.IsMethod || member.IsConstructor )
                        {
                            foreach( Param param in member.Parameters )
                            {
                                if( param.Description == null || param.Description == String.Empty || param.Description == Utilities.emptyDescriptionNagString )
                                {
                                    //var baseMemberParam = baseMember.Parameters.First( p => p.declname == param.declname );
                                    foreach( Param baseParam in baseMember.Parameters )
                                    {
                                        if( param.declname == baseParam.declname )
                                        {
                                            param.BriefDescription = baseParam.BriefDescription;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void FindLinks( List<DoxType> mergedTypes )
        {
            List<Description> descriptions = new List<Description>();

            // Get descriptions for the type.
            descriptions.Add( this.BriefDescription );
            descriptions.Add( this.DetailedDescription );

            // Get descriptions for the type's members.
            var memberBriefDescriptions = this.MemberDefs.Select( m => m.BriefDescription );
            var memberDetailedDescriptions = this.MemberDefs.Select( m => m.DetailedDescription );
            descriptions.AddRange( memberBriefDescriptions );
            descriptions.AddRange( memberDetailedDescriptions );

            foreach( var member in this.MemberDefs )
            {
                descriptions.Add( member.BriefDescription );
                descriptions.Add( member.DetailedDescription );

                if( member.IsMethod || member.IsConstructor )
                {
                    var paramDescriptions = member.Parameters.Select( p => p.BriefDescription );
                    descriptions.AddRange( paramDescriptions );

                    //foreach( var param in member.Parameters )
                    //{
                    //    var paramType = mergedTypes.FirstOrDefault( t => t.Name == param.type );
                    //    if( paramType != null )
                    //    {
                    //        param.DoxType = paramType;
                    //    }
                    //}
                }
            }

            var filteredDescriptions = descriptions.Where( d => d != null && d.Desc != String.Empty );

            foreach( var description in filteredDescriptions )
            {
                description.ResolveLinks( this, mergedTypes );
            }
        }

        //public DoxType(
        //    DoxType nativeType,
        //    List<ObservableType> managedTypes,
        //    List<DoxType> nativeTypes,
        //    string serializationFolder,
        //    string siteConfigReferenceRoot )
        //{
        //    var managedType = managedTypes.FirstOrDefault( t => t.Name == nativeType.Name );
        //    if( managedType != null )
        //    {
        //        DoxType newDoxType = new DoxType( managedType, nativeType, serializationFolder, siteConfigReferenceRoot );

        //        if( managedType.HasBaseType )
        //        {
        //            ObservableType baseType = managedType.BaseType;
        //            DoxType baseDoxType = nativeTypes.FirstOrDefault( t => t.Name == baseType.Name );
        //            newDoxType.BaseTypes.Add( baseDoxType );
        //        }

        //        if( managedType.HasInterfaces )
        //        {
        //            List<ObservableType> interfaces = managedType.Interfaces;
        //            foreach( ObservableType interfaceType in interfaces )
        //            {
        //                DoxType interfaceDoxType = nativeTypes.FirstOrDefault( t => t.Name == interfaceType.Name );
        //                newDoxType.BaseTypes.Add( interfaceDoxType );
        //            }
        //        }

        //        if( managedType.HasAttributes )
        //        {
        //            foreach( var attribute in managedType.Attributes )
        //            {
        //                string attributeName = TypeUtilities.GetAttributeName( attribute, false );

        //            }
        //        }
        //    }
        //}

        public string TopicId
        {
            get;
            private set;
        }

        //public ObservableType UnderlyingType
        //{
        //    get;
        //    private set;
        //}


        public static void BuildTree( List<DoxType> doxTypes )
        {
            var namespaces = doxTypes.Where( d => d.IsNamespace == true );
            var topLevelNamespaces = namespaces.Where( ns => ns.ParentNamespace == String.Empty );
            foreach( var ns in namespaces )
            {
                foreach( DoxType doxType in doxTypes )
                {
                    if( ns.RawName == doxType.ParentNamespaceFull )
                    {
                        doxType.ParentType = ns;
                        ns.ChildTypes.Add( doxType );
                    }
                }
            }
        }

        public Description BriefDescription
        {
            get;
            set;
        }

        public Description DetailedDescription
        {
            get;
            set;
        }

        public DoxType ParentType
        {
            get;
            private set;
        }

        public List<DoxType> ChildTypes
        {
            get
            {
                if( this._childTypes == null )
                {
                    this._childTypes = new List<DoxType>();
                }

                return this._childTypes;
            }

            private set
            {
                this._childTypes = value;
            }
        }

        public List<string> BaseCompoundRefStrings
        {
            get;
            private set;
        }

        public List<string> DerivedCompoundRefStrings
        {
            get;
            private set;
        }

        public List<DoxType> BaseTypes
        {
            //get;
            //private set;

            get
            {
                if( this._baseTypes == null )
                {
                    this._baseTypes = new List<DoxType>();
                }

                return this._baseTypes;
            }
        }

        public string Description
        {
            get
            {
                string description = OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen.Utilities.GetDescriptionString( this.BriefDescription, this.DetailedDescription );
                return description;
            }
        }

        public string Html
        {
            get
            {
                return this.SerializeToHtml();
            }
        }

        public string Markdown
        {
            get
            {
                return this.SerializeToMarkdown();
            }
        }

        public string MarkdownTemplate
        {
            get;
            private set;
        }

        private string SerializeToHtml()
        {
            //HtmlDocument htmlDoc = new HtmlDocument();
            //HtmlNode headNode = new HtmlNode( HtmlNodeType.Element, htmlDoc, 0 );
            //headNode.Name = "HEAD";
            //htmlDoc.DocumentNode.AppendChild( headNode );
            //htmlDoc.Save( "test.html" );
            return null;
        }

        private string SerializeToMarkdown( string html )
        {
            return null;
        }

        private string SerializeToMarkdown()
        {

            return null;
        }

        public string ContainingFolder
        {
            get;
            private set;
        }

        public string OutputRootFolder
        {
            get;
            private set;
        }

        public string OutputFolder
        {
            get;
            private set;
        }

        public string FullPath
        {
            get;
            private set;
        }

        public string RelativePath
        {
            get;
            private set;
        }

        public string PathFragment
        {
            get
            {
                string dirName = Path.GetDirectoryName( this.FullPath );
                int index = dirName.LastIndexOf( Path.DirectorySeparatorChar );
                string dir = dirName.Substring( index + 1 );

                string pathFragment = null;

                if( this.IsClass || this.IsInterface )
                {
                    pathFragment = dir;
                }
                else
                {
                    pathFragment = Path.Combine( dir, this.Name );
                }

                return pathFragment;
            }
        }

        private string GetSiteConfigPath()
        {
            string siteConfigPath = null;

            if( this.IsRootNamespace )
            {
                string siteConfigPathRaw = Path.Combine( this.SiteConfigReferenceRootPath, this.PathFragment );
                siteConfigPath = siteConfigPathRaw.Replace( '\\', '/' );
            }
            else
            {
                if( this.ParentNamespace == null )
                {
                    throw new InvalidOperationException( "ParentNamespace is null" );
                }
                else
                {
                    string siteConfigPathRaw = Path.Combine( this.SiteConfigReferenceRootPath, this.RelativePath );
                    siteConfigPath = siteConfigPathRaw.Replace( '\\', '/' );
                }
            }

            return siteConfigPath;
        }

        private string GetSiteConfigPath2()
        {
            string siteConfigPath = Path.Combine( this.SiteConfigReferenceRootPath, this.FullName );
            //siteConfigPath += ".html";
            siteConfigPath = siteConfigPath.Replace( '\\', '/' );
            
            return siteConfigPath;
        }

        public string SiteConfigPath
        {
            get;
            private set;
        }

        public string SiteConfigReferenceRootPath
        {
            get;
            private set;
        }

        public string SourceFilePath
        {
            get;
            private set;
        }

        public string id
        {
            get;
            private set;
        }

        public string kind
        {
            get;
            private set;
        }

        public string prot
        {
            get;
            private set;
        }

        public string briefdescription
        {
            get;
            private set;
        }

        public string detaileddescription
        {
            get;
            private set;
        }

        public string compoundname
        {
            get;
            private set;
        }

        public bool IsDelegate
        {
            get
            {
                return false;
            }
        }

        public bool IsAttribute
        {
            get
            {
                return false;
            }
        }

        public bool IsEnum
        {
            get
            {
                return this.kind == "enum";
            }
        }

        public bool IsInterface
        {
            get
            {
                return this.kind == "interface";
            }
        }

        public bool IsStruct
        {
            get
            {
                return this.kind == "struct";
            }
        }

        public bool IsClass
        {
            get
            {
                return this.kind == "class";
            }
        }

        public bool IsNamespace
        {
            get
            {
                return this.kind == "namespace";
            }
        }

        public bool IsRootNamespace
        {
            get
            {
                return ( this.IsNamespace && this.ParentNamespace == String.Empty );
            }
        }

        public bool IsGeneric
        {
            get
            {
                return false;
            }
        }

        public List<MemberDef> Properties
        {
            get;
            private set;
        }

        public bool HasProperties
        {
            get
            {
                return ( this.Properties.Count > 0 );
            }
        }

        public List<MemberDef> Constructors
        {
            get;
            private set;
        }

        public List<MemberDef> Methods
        {
            get;
            private set;
        }

        public bool HasMethods
        {
            get
            {
                return ( this.Methods.Count > 0 );
            }
        }

        public bool HasConstructors
        {
            get
            {
                return ( this.Constructors != null && this.Constructors.Count > 0 );
            }
        }

        public List<MemberDef> Events
        {
            get;
            private set;
        }

        public bool HasEvents
        {
            get
            {
                return ( this.Events.Count > 0 );
            }
        }

        public List<MemberDef> Fields
        {
            get;
            private set;
        }

        public bool HasFields
        {
            get
            {
                return ( this.Fields.Count > 0 );
            }
        }

        public List<EnumValue> EnumValues
        {
            get;
            private set;
        }

        public string Name
        {
            get;
            private set;
        }

        public string RawName
        {
            get;
            private set;
        }

        public string FullName
        {
            get
            {
                string fullName = String.Format( "{0}.{1}", this.ParentNamespaceFullCsStyle, this.Name );
                return fullName;
            }
        }


        public string ParentNamespace
        {
            get;
            private set;
        }

        public string ParentNamespaceFull
        {
            get;
            private set;
        }

        public string ParentNamespaceFullCsStyle
        {
            get
            {
                string parentNamespaceFullCsStyle = this.ParentNamespaceFull.Replace( "::", "." );
                return parentNamespaceFullCsStyle;
            }
        }


        public List<MemberDef> MemberDefs
        {
            get;
            private set;
        }

        public override string ToString()
        {
            string toString = String.Format( "{0}::{1} | {2}", this.ParentNamespaceFull, this.Name, this.kind );
            return toString;
        }

        private List<DoxType> _baseTypes;
        private List<DoxType> _childTypes;
    }
}
