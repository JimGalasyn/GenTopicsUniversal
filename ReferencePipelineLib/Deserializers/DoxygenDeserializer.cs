using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

using OsgContentPublishing.ReferencePipelineLib.TypeTopology;
using OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen;

namespace OsgContentPublishing.ReferencePipelineLib.Deserializers
{
    public class DoxygenDeserializer : XmlDeserializer
    {
        public DoxygenDeserializer( string inputFolder, string schemaPath, List<string> namespaces )
            : base( inputFolder, schemaPath, namespaces )
        {
        }

        public DoxygenDeserializer( 
            string inputFolder, 
            string schemaPath, 
            List<string> namespaces,
            bool enableLooseTypecomparisons )
            : base( inputFolder, schemaPath, namespaces, enableLooseTypecomparisons )
        {
        }

        public override List<DefinedType> Deserialize()
        {
            List<DoxType> doxTypes = this.LoadDoxTypes();

            var doxygenTypes = doxTypes.Select( d => TypeFactory.CreateType( d ) ).ToList();

            this.ResolveFacadeTypes( doxygenTypes );

            var definedTypes = doxygenTypes.Select( t => t as DefinedType ).ToList();

            return definedTypes;
        }

        protected List<DoxType> LoadDoxTypes()
        {
            List<DoxType> doxTypes = null;

            XDocument index = Index;

            var compoundElements = Utilities.GetElements( index, "compound" );
            var compounds = compoundElements.Select( ce => new Compound( ce, this.InputFolder ) );
            var typeCompounds = compounds.Where( c => c.kind != "file" && c.kind != "dir" );
            doxTypes = typeCompounds.Select( c => TypeFactory.CreateDoxType( c ) ).ToList();

            var fileCompounds = compounds.Where( c => c.kind == "file" ).ToList();
            List<DoxType> fileCompoundTypes = ExtractTypesFromFileCompounds( fileCompounds );
            doxTypes.AddRange( fileCompoundTypes );

            List<DoxType> doxTypesOrdered = doxTypes.OrderBy( t => t.FullName ).ToList();

            List<DoxType> resolvedTypes = this.ResolveCompoundRefs( doxTypes );
            var resolvedParents = ResolveParentDoxTypes( resolvedTypes );

            List<DoxType> resolvedTypesOrdered = resolvedParents.OrderBy( t => t.FullName ).ToList();

            var doxTypesFlatList = Utilities.GetFlatList( resolvedTypesOrdered ).ToList();

            List<DoxType> typesToCreate = FilterDoxTypesByRequestedNamespaces( doxTypesFlatList );

            return typesToCreate;
        }

        private List<DoxType> ResolveCompoundRefs( List<DoxType> doxTypes )
        {
            List<DoxType> resolvedTypes = doxTypes.Select( t => t ).ToList();

            foreach( DoxType doxType in doxTypes )
            {
                List<DoxType> innerClasses = doxType.InnerClasses.Select( c =>
                    TypeFactory.CreateDoxType( c, doxType ) ).ToList();
                doxType.ChildTypes.AddRange( innerClasses );

                List<DoxType> innerNamespaces = doxType.InnerNamespaces.Select( n =>
                    TypeFactory.CreateDoxType( n, doxType ) ).ToList();
                doxType.ChildTypes.AddRange( innerNamespaces );

                doxType.ChildTypes = doxType.ChildTypes.Distinct().ToList();

                foreach( CompoundRef compoundRef in doxType.BaseCompoundRefs )
                {
                    DoxType baseType = TypeFactory.CreateDoxType( compoundRef );

                    doxType.BaseTypes.Add( baseType );
                    baseType.DerivedTypes.Add( doxType );

                    baseType.DerivedTypes = baseType.DerivedTypes.Distinct().ToList();
                }

                foreach( CompoundRef compoundRef in doxType.DerivedCompoundRefs )
                {
                    DoxType derivedType = TypeFactory.CreateDoxType( compoundRef );

                    doxType.DerivedTypes.Add( derivedType );
                    derivedType.BaseTypes.Add( doxType );
                }

                doxType.BaseTypes = doxType.BaseTypes.Distinct().ToList();
                doxType.DerivedTypes = doxType.DerivedTypes.Distinct().ToList();
            }

            return resolvedTypes;
        }

        private List<DoxType> ResolveParentDoxTypes( List<DoxType> doxTypes )
        {
            foreach( DoxType doxType in doxTypes )
            {
                TraverseTypeGraph( doxType );
            }

            return doxTypes;
        }


        public static void TraverseTypeGraph( DoxType parentType )
        {
            foreach( DoxType childType in parentType.ChildTypes )
            {
                childType.ParentType = parentType;

                TraverseTypeGraph( childType );
            }
        }

        private List<DoxType> ExtractTypesFromFileCompounds( List<Compound> fileCompounds )
        {
            List<DoxType> types = new List<DoxType>();

            foreach( var file in fileCompounds )
            {
                XDocument doc = Utilities.LoadXml( file.SourceFile );
                List<XElement> memberDefElements = Utilities.GetElements( doc, "memberdef" );

                var enumMemberDefElements = memberDefElements.Where( e =>
                    e.Attribute( "kind" ).Value == "enum" ).ToList();

                if( enumMemberDefElements.Count > 0 )
                {
                    var enumMemberDefsInFile = enumMemberDefElements.Select( e => new MemberDef( e, null ) );
                    var enumTypesInFile = enumMemberDefsInFile.Select( m => new DoxType( m, null ) );
                    types.AddRange( enumTypesInFile );
                }

                
                var functionMemberDefElements = memberDefElements.Where( e =>
                    e.Attribute( "kind" ).Value == "function" ).ToList();

                if( functionMemberDefElements.Count > 0 )
                {
                    var functionMemberDefsInFile = functionMemberDefElements.Select( m => new MemberDef( m, null ) );
                    var functionTypesInFile = functionMemberDefsInFile.Select( m => new DoxType( m, null ) );

                    // Filter any functions named "DllMain".
                    functionTypesInFile = functionTypesInFile.Where( t => t.Name != "DllMain" );

                    types.AddRange( functionTypesInFile );
                }
            }

            return types;
        }

        private List<DoxType> FilterDoxTypesByRequestedNamespaces( List<DoxType> doxTypes )
        {
            List<DoxType> requestedDoxTypes = null;
            List<DoxType> requestedNamespaces = null;

            if( this.EnableLooseTypeComparisons )
            {
                return doxTypes;
            }

            if( this.Namespaces != null && this.Namespaces.Count > 0 )
            {
                requestedNamespaces = GetRequestedNamespaces( doxTypes );

                var join = doxTypes.Join(
                    requestedNamespaces,
                    t => t.ParentNamespaceFullCsStyle,
                    ns => ns.FullName,
                    ( t, ns ) => t );

                requestedDoxTypes = join.ToList();

                requestedDoxTypes.AddRange( requestedNamespaces );

                requestedDoxTypes = requestedDoxTypes.Distinct().ToList();
            }
            else
            {
                requestedDoxTypes = doxTypes;
            }

            return requestedDoxTypes;
        }

        private List<DoxType> GetRequestedNamespaces( List<DoxType> doxTypes )
        {
            List<DoxType> requestedNamespaces = new List<DoxType>();

            var namespaceDoxTypes = doxTypes.Where( d => d.IsNamespace );

            foreach( string requestedNamespace in this.Namespaces )
            {
                if( requestedNamespace.EndsWith( "*" ) )
                {
                    string requestedNamespaceMinusStar = requestedNamespace.Replace( ".*", String.Empty );
                    requestedNamespaceMinusStar = requestedNamespaceMinusStar.Replace( "*", String.Empty );
                    var namespaces = namespaceDoxTypes.Where( n => n.FullName.StartsWith( requestedNamespaceMinusStar ) ).ToList();
                    requestedNamespaces.AddRange( namespaces );
                }
                else
                {
                    var ns = namespaceDoxTypes.FirstOrDefault( n => n.FullName == requestedNamespace );
                    if( ns != null )
                    {
                        requestedNamespaces.Add( ns );
                    }
                }
            }

            // TBD: something like http://solutionizing.net/2009/09/19/hacking-linq-expressions-join-with-comparer/
            //NamespaceComparer comparer = new NamespaceComparer();
            //var requestedNamespaces = assemblyNamespaces.Join(
            //    this.Namespaces,
            //    an => an,
            //    rn => rn,
            //    ( an, rn ) => an,
            //    comparer );

            return requestedNamespaces;
        }


        protected void ResolveFacadeTypes( List<DefinedType> doxygenTypes )
        {
            // Find all references to facade types in members.
            foreach( DefinedType type in doxygenTypes )
            {
                if( type is DoxygenFacadeType )
                {
                    continue;
                }

                if( type is DoxygenFunction )
                {
                    DoxygenFunction doxygenFunction = type as DoxygenFunction;
                    var method = doxygenFunction.UnderlyingMethod;

                    var facadeParams = method.Parameters.Where( p =>
                        !( p.Type is GenericParameterType ) && p.Type is DoxygenFacadeType );
                    if( facadeParams != null )
                    {
                        foreach( var facadeParam in facadeParams )
                        {
                            string name = facadeParam.Name;
                            string typeName = facadeParam.Type.Name;

                            facadeParam.Type = ResolveFacadeType( facadeParam.Type as DoxygenFacadeType );
                        }
                    }

                    continue;
                }

                if( type.Members != null && type.Members.Count > 0 )
                {
                    var facadeMembers = type.Members.Where( m => m.Type is DoxygenFacadeType );
                    if( facadeMembers != null )
                    {
                        foreach( var facadeMember in facadeMembers )
                        {
                            string name = facadeMember.Name;
                            string typeName = facadeMember.Type.Name;

                            facadeMember.Type = ResolveFacadeType( facadeMember.Type as DoxygenFacadeType );
                        }
                    }

                    // Find all references to facade types in parameters.
                    var methods = type.Members.Where( m => m is DoxygenMethod );
                    var membersWithParams = methods.Where( m => ( (DoxygenMethod)m ).Parameters.Count > 0 );
                    if( membersWithParams != null )
                    {
                        foreach( var member in membersWithParams )
                        {
                            DoxygenMethod method = member as DoxygenMethod;
                            var facadeParams = method.Parameters.Where( p => 
                                !( p.Type is GenericParameterType ) && p.Type is DoxygenFacadeType );
                            if( facadeParams != null )
                            {
                                foreach( var facadeParam in facadeParams )
                                {
                                    string name = facadeParam.Name;
                                    string typeName = facadeParam.Type.Name;

                                    facadeParam.Type = ResolveFacadeType( facadeParam.Type as DoxygenFacadeType );
                                }
                            }
                        }
                    }
                }

                for( int i = 0; i < type.BaseTypes.Count; i++ )
                {
                    var baseType = type.BaseTypes[i];
                    if( baseType is DoxygenFacadeType )
                    {
                        type.BaseTypes[i] = ResolveFacadeType( baseType as DoxygenFacadeType );
                    }
                }

                for( int i = 0; i < type.DerivedTypes.Count; i++ )
                {
                    var derivedType = type.DerivedTypes[i];
                    if( derivedType is DoxygenFacadeType )
                    {
                        type.DerivedTypes[i] = ResolveFacadeType( derivedType as DoxygenFacadeType );
                    }
                }
            }
        }

        private DefinedType ResolveFacadeType( DoxygenFacadeType facadeType )
        {
            DefinedType resolvedType = facadeType;

            string typeName = facadeType.Name;

            if( !typeName.Contains( ' ' ) )
            {
                // Probably a type without a namespace qualifier. 
                var candidateKnownTypes = TypeFactory.KnownTypes.Where( kvp =>
                    !( kvp.Value is DoxygenFacadeType ) && kvp.Key.Contains( typeName ) );

                if( candidateKnownTypes != null )
                {
                    var candidateKnownTypeList = candidateKnownTypes.ToList();
                    if( candidateKnownTypeList.Count > 0 )
                    {
                        if( candidateKnownTypeList.Count == 1 )
                        {
                            var candidateKnownType = candidateKnownTypeList[0];
                            resolvedType = TypeFactory.KnownTypes[candidateKnownType.Key];
                        }
                        else
                        {
                            // TBD: Do something smarter
                            // Not a namespace, match with shortest string length
                            var candidateKnownTypeListOrdered = candidateKnownTypeList.OrderBy( kvp => kvp.Key.Length ).ToList();
                            var candidateKnownType = candidateKnownTypeListOrdered.FirstOrDefault( kvp => !kvp.Value.IsNamespace );
                            if( candidateKnownType.Key != null )
                            {
                                resolvedType = TypeFactory.KnownTypes[candidateKnownType.Key];
                            }
                        }
                    }
                }
            }
            else
            {
                // Probably a namespace-qualified type, but with a munged namespace.
                // e.g., spaces instead of dots or colons.
                string typeNameWithDots = typeName.Replace( ' ', '.' );
                if( TypeFactory.KnownTypes.ContainsKey( typeNameWithDots ) )
                {
                    resolvedType = TypeFactory.KnownTypes[typeNameWithDots];
                }
            }

            return resolvedType;
        }

        //private bool IsSystemType( DefinedType type )
        //{
        //    bool isSystemType = false;

        //    return isSystemType;
        //}

        private XDocument LoadIndex()
        {
            string path = Path.Combine( InputFolder, indexFilename );
            XDocument doc = Utilities.LoadXml( path );
            return doc;
        }

        private XDocument Index
        {
            get
            {
                if( indexDocument == null )
                {
                    indexDocument = LoadIndex();
                }

                return indexDocument;
            }
        }

        public static bool? ParseYesNo( string yesNoString )
        {
            bool? retval = null;

            if( yesNoString != null && yesNoString.Length > 1 )
            {
                string yesNoLowerCase = yesNoString.ToLower();

                if( yesNoLowerCase == "yes" )
                {
                    retval = true;
                }
                else if( yesNoLowerCase == "no" )
                {
                    retval = false;
                }
            }

            return retval;
        }

        public static bool ParseVirt( string virtString )
        {
            bool retval = false;

            if( virtString != null && virtString.Length > 1 )
            {
                string virtLowerCase = virtString.ToLower();

                if( virtLowerCase == "pure-virtual" || virtLowerCase == "virtual" )
                {
                    retval = true;
                }
            }

            return retval;
        }

        public static Description GetBriefDescription( XDocument doc )
        {
            return GetDescription( doc, "briefdescription" );
        }

        public static Description GetDetailedDescription( XDocument doc )
        {
            return GetDescription( doc, "detaileddescription" );
        }

        private static Description GetDescription( XDocument doc, string descElementName )
        {
            Description description = null;
            XElement compounddef = Utilities.TryGetElement( doc, "compounddef" );
            XElement descElement = Utilities.TryGetChildElement( compounddef, descElementName );
            if( descElement != null )
            {
                description = new Description( descElement );
            }

            return description;
        }

        public static Description GetBriefDescription( XElement memberDefElement )
        {
            return GetDescription( memberDefElement, "briefdescription" );
        }

        public static Description GetDetailedDescription( XElement memberDefElement )
        {
            return GetDescription( memberDefElement, "detaileddescription" );
        }

        public static Description GetInBodyDescription( XElement memberDefElement )
        {
            return GetDescription( memberDefElement, "inbodydescription" );
        }

        private static Description GetDescription( XElement memberDefElement, string descElementName )
        {
            Description description = null;
            XElement descElement = Utilities.TryGetChildElement( memberDefElement, descElementName );
            if( descElement != null )
            {
                description = new Description( descElement );
            }

            return description;
        }

        public static string GetAnchor( DoxType doxType )
        {
            string anchor = String.Format(
                "<a href='{0}'>{1}</a>",
                doxType.SiteConfigPath.ToLower(),
                doxType.Name );
            return anchor;
        }

        //public static List<Param> ReadTemplateParams( XDocument doc )
        //{
        //    List<Param> genericParameters = null;

        //    XElement templateParamListElement = Utilities.TryGetElement(
        //        doc,
        //        _templateParamsElementName );
        //    if( templateParamListElement != null )
        //    {
        //        List<XElement> paramElements = Utilities.TryGetChildElements(
        //            templateParamListElement,
        //            _paramElementName );
        //        if( paramElements != null )
        //        {
        //            genericParameters = paramElements.Select( pe =>
        //                new Param( pe ) ).ToList();
        //        }
        //    }

        //    return genericParameters;
        //}

        public static List<Param> ReadTemplateParams( XElement element )
        {
            List<Param> genericParameters = null;

            XElement templateParamListElement = Utilities.TryGetChildElement(
                element,
                _templateParamsElementName );
            if( templateParamListElement != null )
            {
                List<XElement> paramElements = Utilities.TryGetChildElements(
                    templateParamListElement,
                    _paramElementName );
                if( paramElements != null )
                {
                    genericParameters = paramElements.Select( pe =>
                        new Param( pe ) ).ToList();
                    genericParameters.ForEach( p => p.IsGenericType = true );
                }
            }

            return genericParameters;
        }


        private const string _templateParamsElementName = "templateparamlist";
        private const string _paramElementName = "param";

        XDocument indexDocument = null;

        // TBD: should go in configuration.
        const string indexFilename = "index.xml";

        // TBD: This string should go in config.
        //public const string emptyDescriptionNagFormat = "Doc this {0} and visit [Analog docs comment hunt](http://analogdocs) to collect your bounty.";
        public const string emptyDescriptionNagString = "TBD";
    }
}
