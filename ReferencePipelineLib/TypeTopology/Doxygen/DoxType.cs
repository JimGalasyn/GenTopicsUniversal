using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using OsgContentPublishing.ReferencePipelineLib.Deserializers;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen
{
    /// <summary>
    /// Deserializes XML output from Doxygen.
    /// </summary>
    public class DoxType
    {
        /// <summary>
        /// Creates a new instance of the <see cref="DoxType" class./>
        /// </summary>
        /// <param name="fullPath">The fully qualified path to the source file, 
        /// which is an XML file that's emitted by Doxygen.</param>
        /// <remarks>The source file doesn't specify the parent type or namespace,
        /// so the <see cref="ParentType"/> property needs to be assigned later,
        /// by the deserializer.</remarks>
        //public DoxType( string fullPath )
        //{
        //    this.Initialize( fullPath );
        //}

        /// <summary>
        /// Initializes a new <see cref="DoxType"/> instance to the specified <see cref="Compound"/>.
        /// </summary>
        /// <param name="compound">A deserialized Doxygen compound element.</param>
        public DoxType( Compound compound )
        {
            this.SourceFilePath = compound.SourceFile;
            this.Initialize( this.SourceFilePath );
        }

        /// <summary>
        /// Initializes a new <see cref="DoxType"/> instance to the specified 
        /// <see cref="CompoundRef"/> and parents it to the specified <see cref="DoxType"/>.
        /// </summary>
        /// <param name="compoundRef">A deserialized Doxygen compoundref element.</param>
        /// <param name="parentDoxType">The <see cref="DoxType"/> parent.</param>
        public DoxType( CompoundRef compoundRef, DoxType parentDoxType )
        {
            this.SourceFilePath = compoundRef.SourceFile;
            this.ParentType = parentDoxType;
            if( this.SourceFilePath != null )
            {
                this.Initialize( this.SourceFilePath );
            }
            else
            {
                this.InitializeFacadeType( compoundRef );
            }
        }

        /// <summary>
        /// Initializes a new <see cref="DoxType"/> instance to the specified 
        /// <see cref="MemberDef"/> and parents it to the specified <see cref="DoxType"/>.
        /// </summary>
        /// <param name="memberDef">A deserialized Doxygen memberdef element.</param>
        /// <param name="parentDoxType">The <see cref="DoxType"/> parent.</param>
        public DoxType( MemberDef memberDef, DoxType parentDoxType )
        {
            this.UnderlyingMember = memberDef;
            this.ParentType = parentDoxType;

            this.FixupKind( memberDef );

            if( this.kind == "class" || 
                this.kind == "enum" || 
                this.kind == "struct" || 
                this.kind == "function" )
            {
                this.RawName = memberDef.name;
                this.FixupName( this.RawName );
                this.id = memberDef.id;
                this.BriefDescription = memberDef.BriefDescription;
                this.DetailedDescription = memberDef.DetailedDescription;

                this.Properties = new List<MemberDef>();
                this.Methods = new List<MemberDef>();
                this.Events = new List<MemberDef>();
                this.Fields = new List<MemberDef>();
                this.InnerClasses = new List<CompoundRef>();
                this.InnerNamespaces = new List<CompoundRef>();
                this.BaseCompoundRefs = new List<CompoundRef>();
                this.DerivedCompoundRefs = new List<CompoundRef>();
                this.EnumValues = memberDef.EnumValues;

                if( !String.IsNullOrEmpty( memberDef.language ) )
                {
                    this.language = memberDef.language;
                }
                else if( this.ParentType != null &&
                    !String.IsNullOrEmpty( this.ParentType.language ) )
                {
                    this.language = this.ParentType.language;
                }
                else
                {
                    this.language = String.Empty;
                }
            }
            else
            {
                throw new ArgumentException( "Failed to create a DoxType from a MemberDef" );
            }
        }

        /// <summary>
        /// Deserializes a <see cref="DoxType"/> from the specified Doxygen XML file.
        /// </summary>
        /// <param name="fullPath">The path to the Doxygen XML file to deserialize.</param>
        private void Initialize( string fullPath )
        {
            try
            {
                this.SourceFilePath = fullPath;
                XDocument doc = Utilities.LoadXml( this.SourceFilePath );

                this.ReadCompoundName( doc );
                this.ReadCompoundDef( doc );
                this.ReadInnerClasses( doc );
                this.ReadInnerNamespaces( doc );
                this.ReadMemberDefs( doc );
                this.ReadBaseCompoundRefElements( doc );
                this.ReadDerivedCompoundRefElements( doc );
                this.ReadTemplateParams( doc );
                this.ReadDescriptions( doc );

                this.AssignMembers();
                this.AssignChildTypes();
            }
            catch( Exception ex )
            {
                string msg = String.Format( "Failed to create DoxType at {0}", this.SourceFilePath );
                Debug.WriteLine( ex.ToString() );
                throw;
            }
        }

        private void InitializeFacadeType( CompoundRef compoundRef )
        {
            this.RawName = compoundRef.RawName;
            this.FixupName( this.RawName );
            this.IsFacadeType = true;
        }

        private void ReadCompoundName( XDocument doc )
        {
            XElement compoundName = Utilities.TryGetElement( doc, _compoundNameElementName );
            if( compoundName != null )
            {
                this.RawName = compoundName.Value;
                this.FixupName( this.RawName );
            }
            else
            {
                throw new ArgumentException( "must have a compoundname element", "doc" );
            }
        }

        private void ReadCompoundDef( XDocument doc )
        {
            XElement compoundDef = Utilities.TryGetElement( doc, _compoundDefElementName );
            if( compoundDef != null )
            {
                this.id = Utilities.TryGetAttributeValue( compoundDef, _idAttributeName );
                this.kind = Utilities.TryGetAttributeValue( compoundDef, _kindAttributeName );
                this.language = Utilities.TryGetAttributeValue( compoundDef, _languageAttributeName );
                this.prot = Utilities.TryGetAttributeValue( compoundDef, _protAttributeName );

                this.FixupKind();
            }
            else
            {
                throw new ArgumentException( "doesn't contain a compoundDef element", "doc" );
            }
        }

        public TypeDeclarationParseResults ParseResults
        {
            get;
            private set;
        }

        /// <summary>
        /// Scrubs the raw type name that was deserialized from the Doxygen compound.
        /// </summary>
        /// <param name="rawName">The type name to scrub.</param>
        /// <returns>The scrubbed type name.</returns>
        private void FixupName( string rawName )
        {
            string fixedName = null;

            if( !String.IsNullOrEmpty( rawName ) )
            {
                this.ParseResults = Utilities.ParseRawTypeDeclaration( rawName );
                this.Name = this.ParseResults.TypeName;

                if( this.ParentType == null || String.IsNullOrEmpty( this.ParentType.Name ) )
                {
                    this.FullName = this.ParseResults.FullName;
                }
                else
                {
                    // Prefer the ParentType's FullName, because it has been 
                    // resolved to a DoxType and isn't just an opaque string.
                    this.FullName = this.ParentType.FullName + "." + this.Name; 
                }

                // TBD: other DoxType-specific fixes will go here.
            }
            else
            {
                throw new ArgumentNullException( "rawName", "must be assigned and not empty" );
            }
        }

        /// <summary>
        /// Converts the deserialized kind string from obscure kinds into more common kinds,
        /// for example, a protocol (From Objective-C) kind becomes an interface.
        /// </summary>
        /// <remarks>
        /// <para>Some loss of fidelity occurs here, but this is probably okay for v1.</para>
        /// <para>A future version should preserve the origin language's constructs, to support
        /// projection and interface generation scenarios, like Analog's Xtools. 
        /// </para>
        /// </remarks>
        private void FixupKind()
        {
            if( this.kind != null )
            {
                switch( this.kind )
                {
                    case "protocol":
                        {
                            this.kind = "interface";
                            break;
                        }
                    case "friend":
                            {
                                this.kind = "class";
                                break;
                            }
                    default:
                        {
                            break;
                        }
                }
            }
        }

        private void FixupKind( MemberDef memberDef )
        {
            this.kind = memberDef.kind;

            if( memberDef.kind == "class" ||
                memberDef.kind == "friend" ||
                memberDef.kind == "protocol" ) // from Objective-C
            {
                this.kind = "class";
            }
            else if( memberDef.kind == "typedef" )
            {
                this.IsTypedef = true;

                if( memberDef.definition.Contains( "struct" ) )
                {
                    this.kind = "struct";
                }
                else if( memberDef.definition.Contains( "enum" ) )
                {
                    this.kind = "enum";
                }
                else
                {
                    // TBD: Some bizarre syntax that Doxygen doesn't understand, like this abomination from DUSK2:
                    // using Dusk2.BufferAndDescription = typedef System.Collections.Generic.KeyValuePair<DUSK.Windows.Storage.Streams.IBuffer, DUSK.Windows.World.Surfaces.BufferDescription>
                    // Set to "unknown"?
                    this.kind = "struct";
                }
            }
            //else if( memberDef.kind == "enum" )
            //{
            //    this.kind = "enum";
            //}
            //else if( memberDef.kind == "function" )
            //{
            //    this.kind = "function";
            //}
        }

        private void ReadInnerClasses( XDocument doc )
        {
            List<XElement> innerClassElements = Utilities.TryGetElements( doc, _innerClassElementName );
            if( innerClassElements != null )
            {
                this.InnerClasses = innerClassElements.Select( i => 
                    new InnerClass( i, this ) as CompoundRef ).ToList();
            }
        }

        private void ReadInnerNamespaces( XDocument doc )
        {
            List<XElement> innerNamespaceElements = Utilities.TryGetElements( doc, _innerNamespaceElementName );
            if( innerNamespaceElements != null )
            {
                this.InnerNamespaces = innerNamespaceElements.Select( i =>
                    new InnerNamespace( i, this ) as CompoundRef ).ToList();
            }
        }

        private void ReadMemberDefs( XDocument doc )
        {
            List<XElement> memberDefElements = Utilities.TryGetElements( doc, _memberDefElementName );
            if( memberDefElements != null )
            {
                List<MemberDef> memberDefs = memberDefElements.Select( me => 
                    new MemberDef( me, this ) ).ToList();
                this.MemberDefs = FilterSpuriousTypes( memberDefs );
            }
            else
            {
                this.MemberDefs = new List<MemberDef>();
            }
        }

        private void ReadTemplateParams( XDocument doc )
        {
            XElement compoundDefElement = Utilities.TryGetElement( doc, _compoundDefElementName );
            List<Param> genericParameters = DoxygenDeserializer.ReadTemplateParams( compoundDefElement );
            if( genericParameters != null )
            {
                this.GenericParameters = genericParameters;
            }
            else
            {
                this.GenericParameters = new List<Param>();
            }
        }

        private void AssignMembers()
        {
            this.Properties = this.MemberDefs.Where( md => md.kind == "property" ).ToList();
            this.Methods = this.MemberDefs.Where( md => md.kind == "function" && md.IsEvent == false ).ToList();
            this.Events = this.MemberDefs.Where( md => md.IsEvent == true ).ToList();
            this.Fields = this.MemberDefs.Where( md => md.kind == "variable" ).ToList();
        }

        private void AssignChildTypes()
        {
            // TBD: functions and variables
            var memberDefs = this.MemberDefs.Where( md => md.IsEnum || md.IsClass ); // for namespace compounds
            this.ChildTypes.AddRange( memberDefs.Select( md => TypeFactory.CreateDoxType( md, this ) ) );
            this.ChildTypes.AddRange( this.InnerClasses.Select( c => TypeFactory.CreateDoxType( c, this ) ) );
            this.ChildTypes.AddRange( this.InnerNamespaces.Select( n => TypeFactory.CreateDoxType( n, this ) ) );
        }

        private void ReadDescriptions( XDocument doc )
        {
            this.BriefDescription = DoxygenDeserializer.GetBriefDescription( doc );
            this.DetailedDescription = DoxygenDeserializer.GetDetailedDescription( doc );
        }

        private void ReadBaseCompoundRefElements( XDocument doc )
        {
            List<XElement> basecompoundrefElements = Utilities.GetElements( doc, _baseCompoundRefElementName );
            if( basecompoundrefElements != null )
            {
                this.BaseCompoundRefs = basecompoundrefElements.Select( b => 
                    new CompoundRef( b, this ) ).ToList();
            }
        }

        private void ReadDerivedCompoundRefElements( XDocument doc )
        {
            List<XElement> derivedCompoundRefElements = Utilities.GetElements( doc, _derivedCompoundRefElementName );
            if( derivedCompoundRefElements != null )
            {
                this.DerivedCompoundRefs = derivedCompoundRefElements.Select( b => 
                    new CompoundRef( b, this ) ).ToList();
            }
        }

        //public void ResolveContent( List<DoxType> nativeTypes )
        //{
        //    // Collect all of members from the base types of this type.
        //    List<MemberDef> allBaseMembers = new List<MemberDef>();
        //    foreach( DoxType baseType in this.BaseTypes )
        //    {
        //        allBaseMembers.AddRange( baseType.MemberDefs );
        //    }

        //    // Copy descriptions from the base members into this type's members.            
        //    foreach( MemberDef member in this.MemberDefs )
        //    {
        //        if( member.Description == null || 
        //            member.Description == String.Empty || 
        //            member.Description == DoxygenDeserializer.emptyDescriptionNagString )
        //        {
        //            MemberDef baseMember = allBaseMembers.FirstOrDefault( m => m.name == member.name );
        //            if( baseMember != null )
        //            {
        //                member.BriefDescription = baseMember.BriefDescription;
        //                member.DetailedDescription = baseMember.DetailedDescription;

        //                if( member.IsMethod || member.IsConstructor )
        //                {
        //                    foreach( Param param in member.Parameters )
        //                    {
        //                        if( param.Description == null || 
        //                            param.Description == String.Empty ||
        //                            param.Description == DoxygenDeserializer.emptyDescriptionNagString )
        //                        {
        //                            foreach( Param baseParam in baseMember.Parameters )
        //                            {
        //                                if( param.declname == baseParam.declname )
        //                                {
        //                                    param.BriefDescription = baseParam.BriefDescription;
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        //public void FindLinks( List<DoxType> doxygenTypes )
        //{
        //    List<Description> descriptions = new List<Description>();

        //    // Get descriptions for the type.
        //    descriptions.Add( this.BriefDescription );
        //    descriptions.Add( this.DetailedDescription );

        //    // Get descriptions for the type's members.
        //    var memberBriefDescriptions = this.MemberDefs.Select( m => m.BriefDescription );
        //    var memberDetailedDescriptions = this.MemberDefs.Select( m => m.DetailedDescription );
        //    descriptions.AddRange( memberBriefDescriptions );
        //    descriptions.AddRange( memberDetailedDescriptions );

        //    foreach( var member in this.MemberDefs )
        //    {
        //        descriptions.Add( member.BriefDescription );
        //        descriptions.Add( member.DetailedDescription );

        //        if( member.IsMethod || member.IsConstructor )
        //        {
        //            var paramDescriptions = member.Parameters.Select( p => p.BriefDescription );
        //            descriptions.AddRange( paramDescriptions );

        //            //foreach( var param in member.Parameters )
        //            //{
        //            //    var paramType = mergedTypes.FirstOrDefault( t => t.Name == param.type );
        //            //    if( paramType != null )
        //            //    {
        //            //        param.DoxType = paramType;
        //            //    }
        //            //}
        //        }
        //    }

        //    var filteredDescriptions = descriptions.Where( d => d != null && d.Desc != String.Empty );

        //    foreach( var description in filteredDescriptions )
        //    {
        //        description.ResolveLinks( this, doxygenTypes );
        //    }
        //}

        /// <summary>
        /// Gets the MSDN topic ID for the current <see cref="DoxType"/>.
        /// </summary>
        public string TopicId
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the content of Doxygen's briefdescription element.
        /// </summary>
        public Description BriefDescription
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the content of Doxygen's detaileddescription element.
        /// </summary>
        public Description DetailedDescription
        {
            get;
            set;
        }

        /// <summary>
        /// Get the parent <see cref="DoxType"/> of the current type.
        /// </summary>
        /// <remarks><para>If the type declaration was parsed successfully 
        /// by the <see cref="TypeDeclarationParser"/> and the <see cref="ParseResults"/>
        /// property is assigned, the <see cref="ParentType"/> property queries
        /// the <see cref="TypeFactory.KnownDoxTypes"/> collection for a type
        /// that matches the name specified by the <see cref="TypeDeclarationParseResults.ParentType"/>
        /// property.</para>
        /// </remarks>
        public DoxType ParentType
        {
            get
            {
                if( this._parentType == null )
                {
                    if( this.ParseResults != null )
                    {
                        if( !String.IsNullOrEmpty( this.ParseResults.ParentType ) )
                        {
                            string parentTypeName = this.ParseResults.ParentType;

                            var candidateParentType = TypeFactory.KnownDoxTypes.Values.FirstOrDefault( v => 
                                v.Name == parentTypeName );

                            if( candidateParentType != null )
                            {
                                this._parentType = candidateParentType;
                            }
                        }
                    }
                }

                return this._parentType;
            }

            set
            {
                this._parentType = value;
            }
        }

        /// <summary>
        /// Gets or sets a collection of types that are children of the 
        /// current <see cref="DoxType"/>.  
        /// </summary>
        /// <remarks><para>In most cases, the current type is a namespace,
        /// and the child types are all of the types within the namespace,
        /// including child namespaces.</para>
        /// <para>In some cases, the current type declares inner classes, enums,
        /// or structs. These are collected in the <see cref="ChildTypes"/>
        /// property. Note that this distinct from inheritance.</para>
        /// </remarks>
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

            set
            {
                this._childTypes = value;
            }
        }

        /// <summary>
        /// Gets a collection of Doxygen basecompoundrefs elements for 
        /// the current type.
        /// </summary>
        public List<CompoundRef> BaseCompoundRefs
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a collection of Doxygen derivedcompoundrefs elements for 
        /// the current type.
        /// </summary>
        public List<CompoundRef> DerivedCompoundRefs
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets a collection of types that the current type derives from.
        /// </summary>
        public List<DoxType> BaseTypes
        {
            get
            {
                if( this._baseTypes == null )
                {
                    this._baseTypes = new List<DoxType>();
                }

                return this._baseTypes;
            }

            set
            {
                this._baseTypes = value;
            }
        }

        /// <summary>
        /// Gets a collection of types that derive from the current type.
        /// </summary>
        public List<DoxType> DerivedTypes
        {
            get
            {
                if( this._derivedTypes == null )
                {
                    this._derivedTypes = new List<DoxType>();
                }

                return this._derivedTypes;
            }

            set
            {
                this._derivedTypes = value;
            }
        }

        /// <summary>
        /// Gets the generic/template parameters of the current type, if it's 
        /// declared to be a generic/template class.
        /// </summary>
        public List<Param> GenericParameters
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

        public string language
        {
            get;
            private set;
        }

        /// <summary>
        /// Removes types that result from parsing errors.
        /// </summary>
        /// <param name="memberDefs"></param>
        /// <returns>A filtered list of <see cref="MemberDef"/> objects.</returns>
        private List<MemberDef> FilterSpuriousTypes( List<MemberDef> memberDefs )
        {
            List<MemberDef> filteredMemberDefs = null;

            if( memberDefs != null )
            {
                // Make a copy to filter.
                filteredMemberDefs = memberDefs.Select( m => m ).ToList();
                if( filteredMemberDefs.Count > 0 )
                {
                    // Doxygen introduces a spurious "declare" class when it parses RIDL.
                    var declareMember = filteredMemberDefs.FirstOrDefault( m => m.name == "declare" );
                    if( declareMember != null )
                    {
                        filteredMemberDefs.Remove( declareMember );
                    }

                    filteredMemberDefs.RemoveAll( m => m.kind == "typedef" );

                    // Doxygen doesn't understand runtimeclass declarations, and 
                    // it stashes them in the namespace as a "variable".
                    // Fortunately, Doxygen does emit XML for runtimeclasses, so 
                    // we can hook up the class to its namespace in a later operation.
                    filteredMemberDefs.RemoveAll( m =>
                        m.ParentType.IsNamespace &&
                        m.kind == "variable" );

                    //var variableMembers = filteredMemberDefs.Where( m =>
                    //    m.kind == "variable" && !m.type.Contains( "runtimeclass" ) );
                    //if( variableMembers != null )
                    //{
                    //    foreach( var variableMember in variableMembers )
                    //    {
                    //        filteredMemberDefs.Remove( variableMember );
                    //    }
                    //}
                }
            }
            else
            {
                throw new ArgumentNullException( "memberDefs", "Must be assigned" );
            }

            return filteredMemberDefs;
        }

        public string SiteConfigPath
        {
            get;
            private set;
        }

        public string SourceFilePath
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the unique Doxygen identifier for the current type. 
        /// </summary>
        public string id
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the Doxygen kind attribute for the current type.
        /// </summary>
        public string kind
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the Doxygen prot attribute for the current type.
        /// </summary>
        public string prot
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the Doxygen briefdescription element value for the current type.
        /// </summary>
        public string briefdescription
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the Doxygen detaileddescription element value for the current type.
        /// </summary>
        public string detaileddescription
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the Doxygen compoundname element value for the current type.
        /// </summary>
        public string compoundname
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether the current type represents a 
        /// delegate, event handler, or function pointer.
        /// </summary>
        public bool IsDelegate
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current type represents an attribute.
        /// </summary>
        public bool IsAttribute
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current type represents an enumeration. 
        /// </summary>
        public bool IsEnum
        {
            get
            {
                return this.kind == "enum";
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current type is a facade type.
        /// </summary>
        public bool IsFacadeType
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether the current type represents a function.
        /// </summary>
        public bool IsFunction
        {
            get
            {
                return this.kind == "function";
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current type represents an interface. 
        /// </summary>
        public bool IsInterface
        {
            get
            {
                return this.kind == "interface";
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current type represents a struct.
        /// </summary>
        public bool IsStruct
        {
            get
            {
                return this.kind == "struct";
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current type represents a class.
        /// </summary>
        public bool IsClass
        {
            get
            {
                return this.kind == "class";
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current type represents a namespace.
        /// </summary>
        /// <remarks><para>In the GTU framework, namespaces are treated as 
        /// first-class types, not just as opaque strings.</para>
        /// </remarks>
        public bool IsNamespace
        {
            get
            {
                return this.kind == "namespace";
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current type represents a typedef.
        /// </summary>
        public bool IsTypedef
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a collection of property members for the current type.
        /// </summary>
        public List<MemberDef> Properties
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether the current type contains
        /// any property members.
        /// </summary>
        public bool HasProperties
        {
            get
            {
                return ( this.Properties.Count > 0 );
            }
        }

        /// <summary>
        /// Gets a collection of constructors for the current type.
        /// </summary>
        public List<MemberDef> Constructors
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a collection of method members for the current type.
        /// </summary>
        public List<MemberDef> Methods
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether the current type contains
        /// any method members.
        /// </summary>
        public bool HasMethods
        {
            get
            {
                return ( this.Methods.Count > 0 );
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current type contains
        /// any constructors.
        /// </summary>
        public bool HasConstructors
        {
            get
            {
                return ( this.Constructors != null && this.Constructors.Count > 0 );
            }
        }

        /// <summary>
        /// Gets a collection of property members for the current type.
        /// </summary>
        public List<MemberDef> Events
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether the current type contains
        /// any event members.
        /// </summary>
        public bool HasEvents
        {
            get
            {
                return ( this.Events.Count > 0 );
            }
        }

        /// <summary>
        /// Gets a collection of field members for the current type.
        /// </summary>
        public List<MemberDef> Fields
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether the current type contains
        /// any field members.
        /// </summary>
        public bool HasFields
        {
            get
            {
                return ( this.Fields.Count > 0 );
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current type is declared
        /// with generic/template parameters.
        /// </summary>
        public bool HasGenericParameters
        {
            get
            {
                return ( this.GenericParameters != null && 
                    this.GenericParameters.Count > 0 );
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current type is declared
        /// to be a generic/template class.
        /// </summary>
        public bool IsGeneric
        {
            get
            {
                return this.HasGenericParameters;
            }
        }

        /// <summary>
        /// Gets a collection of enumeration values for the current type.
        /// </summary>
        public List<EnumValue> EnumValues
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the name of the current type.
        /// </summary>
        /// <remarks><para>The <see cref="Name"/> property is the name of the type
        /// without any decorations or generic/template parameters.</para>
        /// </remarks>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the name of the current type, as deserialized directly from XML.
        /// </summary>
        /// <remarks><para>The <see cref="RawName"/> property is the name of the type
        /// before GTU has performed any processing on it.</para>
        /// </remarks>
        public string RawName
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the full name of the current type.
        /// </summary>
        /// <remarks><para>The <see cref="FullName"/> property is the name of the type,
        /// appended with generic/template parameters, and prepended with the name of 
        /// the parent type or namespace.</para>
        /// </remarks>
        public string FullName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the parent namespace of the current type.
        /// </summary>
        public DoxType Namespace
        {
            get
            {
                DoxType ns = null;
                if( this.ParentType != null )
                {
                    if( this.ParentType.IsNamespace )
                    {
                        ns = this.ParentType;
                    }
                    else
                    {
                        ns = this.ParentType.Namespace;
                    }
                }

                return ns;
            }
        }

        /// <summary>
        /// Gets the full name of the current type's parent namespace,
        /// with the C++ selection string ("::").
        /// </summary>
        public string ParentNamespaceFullCsStyle
        {
            get
            {
                string parentNamespaceFullCsStyle = String.Empty;

                if( this.Namespace != null )
                {   
                    parentNamespaceFullCsStyle = this.Namespace.FullName.Replace( "::", "." );
                } 

                return parentNamespaceFullCsStyle;
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="MemberDef"/> that represent the 
        /// current type's members. 
        /// </summary>
        public List<MemberDef> MemberDefs
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a collection of Doxygen innerclass elements for the current type. 
        /// </summary>
        public List<CompoundRef> InnerClasses
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a collection of Doxygen innernamespace elements for the current type. 
        /// </summary>
        public List<CompoundRef> InnerNamespaces
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the <see cref="MemberDef"/> that the current <see cref="DoxType"/> was
        /// created from. 
        /// </summary>
        /// <remarks><para>Most <see cref="DoxType"/> instances are created from Doxygen's
        /// compounddef element, but some are created from the memberdef element. Usually, 
        /// this applies to enums and functions.</para>
        /// </remarks>
        public MemberDef UnderlyingMember
        {
            get;
            private set;
        }

        public override string ToString()
        {
            string toString = String.Format( 
                "{0} | {1} {2} DoxType", 
                this.FullName, 
                this.kind,
                this.IsFacadeType ? "FACADE" : String.Empty );
            return toString;
        }

        private List<DoxType> _baseTypes;
        private List<DoxType> _derivedTypes;
        private List<DoxType> _childTypes;

        // compounddef elements 
        private const string _baseCompoundRefElementName = "basecompoundref";
        private const string _compoundDefElementName = "compounddef";
        private const string _compoundNameElementName = "compoundname";
        private const string _derivedCompoundRefElementName = "derivedcompoundref";
        //private const string _templateParamsElementName = "templateparamlist";
        //private const string _paramElementName = "param";
        private const string _idAttributeName = "id";
        private const string _kindAttributeName = "kind";
        private const string _languageAttributeName = "language";
        private const string _memberDefElementName = "memberdef";
        private const string _protAttributeName = "prot";

        // compoundRef elements
        private const string _innerClassElementName = "innerclass";
        private const string _innerNamespaceElementName = "innernamespace";

        private DoxType _parentType;
    }
}
