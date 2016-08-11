//using ReflectionUtilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen
{
    public class Utilities
    {
        public static string GetTypeName( string fullTypeName )
        {
            string typeName = null;

            string selectorString = Utilities.IsCppStyleTypeName( fullTypeName ) == true ? "::" : ".";

            int indexOfName = fullTypeName.LastIndexOf( selectorString );
            if( indexOfName < 0 )
            {
                typeName = fullTypeName;
            }
            else if( indexOfName == 0 )
            {
                typeName = fullTypeName.Remove( 0, selectorString.Length );
            }
            else
            {
                typeName = fullTypeName.Substring( indexOfName + selectorString.Length );
            }

            return typeName;
        }

        public static bool IsCppStyleTypeName( string fullTypeName )
        {
            return fullTypeName.Contains( "::" );
        }

        public static bool IsCSharpStyleTypeName( string fullTypeName )
        {
            return fullTypeName.Contains( "." );
        }


        public static string GetParentNamespaceFull( string fullTypeName )
        {
            string parentNamespaceFull = null;

            if( IsCppStyleTypeName( fullTypeName ) )
            {
                int indexOfName = fullTypeName.LastIndexOf( "::" );
                string typeName = fullTypeName.Substring( indexOfName + 2 );
                parentNamespaceFull = fullTypeName.Substring( 0, indexOfName );
            }
            else if( IsCSharpStyleTypeName( fullTypeName ) )
            {
                int indexOfName = fullTypeName.LastIndexOf( "." );
                string typeName = fullTypeName.Substring( indexOfName + 1 );
                parentNamespaceFull = fullTypeName.Substring( 0, indexOfName );
            }
            else
            {
                parentNamespaceFull = String.Empty;
            }

            return parentNamespaceFull;
        }

        public static string GetParentNamespace( string fullNamespace )
        {
            string parentNamespace = null;

            if( IsCppStyleTypeName( fullNamespace ) )
            {
                int indexOfParentNamespace = fullNamespace.LastIndexOf( "::" );
                parentNamespace = fullNamespace.Substring( indexOfParentNamespace + 2 );
            }
            else if( IsCSharpStyleTypeName( fullNamespace ) )
            {
                int indexOfParentNamespace = fullNamespace.LastIndexOf( "." );
                parentNamespace = fullNamespace.Substring( indexOfParentNamespace + 1 );
            }
            else
            {
                //parentNamespace = String.Empty;
                parentNamespace = fullNamespace;
            }

            return parentNamespace;
        }

        public static List<XElement> TryGetElements( XDocument xdoc, string elementName )
        {
            List<XElement> elements = null;
            try
            {
                var xelements = xdoc.Descendants().Where( xelem => xelem.Name == elementName );
                if( xelements != null )
                {
                    elements = xelements.ToList();
                }
            }
            catch( Exception ex )
            {
                Debug.WriteLine( ex.ToString() );
            }

            return elements;
        }

        public static XElement TryGetElement( XDocument xdoc, string elementName )
        {
            XElement element = null;

            try
            {
                element = xdoc.Descendants().SingleOrDefault( xelem => xelem.Name == elementName );
            }
            catch( Exception ex )
            {
                // NOTE: This exception is expected for certain types.
                //Trace.WriteLine( ex.ToString() );
            }

            return element;
        }

        public static string TryGetElementValue( XDocument xdoc, string elementName )
        {
            string elementValue = null;

            XElement descendantElement = TryGetElement( xdoc, elementName );
            if( descendantElement != null )
            {
                elementValue = descendantElement.Value;
            }

            return elementValue;
        }


        public static List<XElement> GetChildElements( XElement parentElement, string elementName )
        {
            var elements = parentElement.Elements().Where( xelem => xelem.Name.LocalName == elementName );

            return ( elements.ToList() );
        }

        public static List<XElement> GetElements( XDocument xdoc, string elementName )
        {
            var elements = xdoc.Descendants().Where( xelem => xelem.Name.LocalName == elementName );

            return ( elements.ToList() );
        }

        public static XElement GetChildElement( XElement parentElement, string elementName )
        {
            XElement element = null;

            try
            {
                element = parentElement.Elements().SingleOrDefault( xelem => xelem.Name.LocalName == elementName );
            }
            catch( Exception ex )
            {
                Debug.WriteLine( ex.ToString() );
            }

            return element;
        }

        public static string TryGetChildElementValue( XElement parentElement, string elementName )
        {
            string elementValue = null;

            XElement childElement = TryGetChildElement( parentElement, elementName );
            if( childElement != null )
            {
                elementValue = childElement.Value;
            }

            return elementValue;
        }

        public static XElement TryGetChildElement( XElement parentElement, string elementName )
        {
            XElement childElement = null;

            try
            {
                childElement = Utilities.GetChildElement( parentElement, elementName );
            }
            catch( Exception ex )
            {
                Debug.WriteLine( "Element {0} not found", elementName );
            }

            return childElement;
        }

        public static List<XElement> TryGetChildElements( XElement parentElement, string elementName )
        {
            List<XElement> childElements = null;

            try
            {
                childElements = GetChildElements( parentElement, elementName );
            }
            catch( Exception ex )
            {
                Debug.WriteLine( "Element {0} not found", elementName );
            }

            return childElements;
        }


        public static string TryGetAttributeValue( XElement element, string attributeName )
        {
            string attributeValue = null;

            XAttribute attribute = TryGetAttribute( element, attributeName );
            if( attribute != null )
            {
                attributeValue = attribute.Value;
            }

            return attributeValue;
        }

        public static XAttribute TryGetAttribute( XElement element, string attributeName )
        {
            XAttribute attribute = null;

            try
            {
                attribute = element.Attribute( attributeName );
            }
            catch( Exception ex )
            {
                Debug.WriteLine( "Attribute {0} not found", attributeName );
            }

            return attribute;
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

        public static string GetDescriptionString( Description briefDescription, Description detailedDescription )
        {
            string descriptionString = null;

            if( briefDescription != null )
            {
                descriptionString = briefDescription.Desc;

                if( descriptionString == null || descriptionString == String.Empty )
                {
                    if( ( detailedDescription != null ) && ( detailedDescription.Desc != String.Empty ) )
                    {
                        descriptionString = detailedDescription.Desc;
                    }

                    else
                    {
                        descriptionString = emptyDescriptionNagString;
                    }
                }
            }
            else
            {
                descriptionString = emptyDescriptionNagString;
            }

            return descriptionString;
        }

        //public static string GetDescriptionString2( Description briefDescription, Description detailedDescription )
        //{

        //}


        public static Description GetBriefDescription( XDocument doc )
        {
            return GetDescription( doc, "briefdescription" );
        }

        public static Description GetDetailedDescription( XDocument doc )
        {
            return GetDescription( doc, "detaileddescription" );
        }

        private  static Description GetDescription( XDocument doc, string descElementName )
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

        public static string TranscribeKind( MemberInfo mi )
        {
            string kindString = null;

            switch( mi.MemberType )
            {
                case MemberTypes.Field:
                    {
                        kindString = "field";
                        break;
                    }

                case MemberTypes.Method:
                    {
                        kindString = "function";
                        break;
                    }

                case MemberTypes.Constructor:
                    {
                        kindString = "constructor";
                        break;
                    }

                case MemberTypes.Property:
                    {
                        kindString = "property";
                        break;
                    }

                case MemberTypes.Event:
                    {
                        kindString = "event";
                        break;
                    }

                default:
                    {
                        throw new ArgumentException( "Unknown member type" );
                        break;
                    }
            }

            return kindString;

            //  <xsd:simpleType name="DoxMemberKind">
            //    <xsd:restriction base="xsd:string">
            //    <xsd:enumeration value="define" />
            //    <xsd:enumeration value="property" />
            //    <xsd:enumeration value="event" />
            //    <xsd:enumeration value="variable" />
            //    <xsd:enumeration value="typedef" />
            //    <xsd:enumeration value="enum" />
            //    <xsd:enumeration value="function" />
            //    <xsd:enumeration value="signal" />
            //    <xsd:enumeration value="prototype" />
            //    <xsd:enumeration value="friend" />
            //    <xsd:enumeration value="dcop" />
            //    <xsd:enumeration value="slot" />
            //    <xsd:enumeration value="interface" />
            //    <xsd:enumeration value="service" />
            //  </xsd:restriction>
            //</xsd:simpleType>

        }

        //public static string TranscribeProt( MemberInfo mi )
        //{
        //    string protString = null;

        //    if( mi.MemberType == MemberTypes.Method ) // || mi.MemberType == MemberTypes.Event )
        //    {
        //        MethodBase method = mi as MethodBase;

        //        protString = TranscribeMethodProt( method );
        //    }
        //    else if( mi.MemberType == MemberTypes.Property )
        //    {
        //        PropertyInfo pi = mi as PropertyInfo;

        //        MethodInfo[] accessorMethods = pi.GetAccessors( true );
        //        MethodInfo getAccessor = accessorMethods[0];

        //        protString = TranscribeMethodProt( getAccessor );
        //    }
        //    else if( mi.MemberType == MemberTypes.Event )
        //    {
        //        EventInfo ei = mi as EventInfo;
        //        MethodInfo addMethod = ei.GetAddMethod( true );

        //        protString = TranscribeMethodProt( addMethod );
        //    }
        //    else if( mi.MemberType == MemberTypes.Field )
        //    {
        //        FieldInfo field = mi as FieldInfo;

        //        if( field.IsFamily )
        //        {
        //            protString = "protected";
        //        }
        //        else if( field.IsPrivate )
        //        {
        //            protString = "private";
        //        }
        //        else if( field.IsPublic )
        //        {
        //            protString = "public";
        //        }
        //        else if( field.IsFamilyAndAssembly )
        //        {
        //            // TBD: I'm guessing about this mapping.
        //            protString = "package";
        //        }
        //    }

        //    return protString;

        //    //  <xsd:simpleType name="DoxProtectionKind">
        //    //    <xsd:restriction base="xsd:string">
        //    //    <xsd:enumeration value="public" />
        //    //    <xsd:enumeration value="protected" />
        //    //    <xsd:enumeration value="private" />
        //    //    <xsd:enumeration value="package" />
        //    //  </xsd:restriction>
        //    //</xsd:simpleType>


        //}

        //public static string TranscribeKind( ObservableType type )
        //{
        //    string kindString = null;

        //    if( type.IsInterface )
        //    {
        //        kindString = "interface";
        //    }
        //    else if( type.IsEnum )
        //    {
        //        kindString = "enum";
        //    }
        //    else if( type.IsStruct )
        //    {
        //        kindString = "struct";
        //    }
        //    if( type.IsClass )
        //    {
        //        kindString = "class";
        //    }

        //    return kindString;
        //}


        //public static string TranscribeMethodProt( MethodBase method )
        //{
        //    string protString = null;

        //    if( method.IsFamily )
        //    {
        //        protString = "protected";
        //    }
        //    else if( method.IsPrivate )
        //    {
        //        protString = "private";
        //    }
        //    else if( method.IsPublic )
        //    {
        //        protString = "public";
        //    }
        //    else if( method.IsFamilyAndAssembly )
        //    {
        //        // TBD: I'm guessing about this mapping.
        //        protString = "package";
        //    }

        //    return protString;
        //}

        //public static string TransscribeKind( MemberInfo mi )
        //{
        //    string kindString = null;

        //    switch( mi.MemberType )
        //    {
        //        case MemberTypes.Field:
        //            {
        //                kindString = "field";
        //                break;
        //            }

        //        case MemberTypes.Method:
        //            {
        //                kindString = "function";
        //                break;
        //            }

        //        case MemberTypes.Property:
        //            {
        //                kindString = "property";
        //                break;
        //            }

        //        case MemberTypes.Event:
        //            {
        //                kindString = "event";
        //                break;
        //            }

        //        default:
        //            {
        //                throw new ArgumentException( "Unknown member type" );
        //                break;
        //            }

        //    }

        //    return kindString;

        //}

        public static string GetMarkdownLink( DoxType doxType )
        {
            string link = String.Format( "[{0}]({1})", doxType.Name, doxType.SiteConfigPath );

            return link;
        }

        public static string GetAnchor( DoxType doxType )
        {
            //string siteConfigPath = String.Format( "{0}/{1}", doxType.SiteConfigReferenceRootPath, doxType.Name );
            string anchor = String.Format( "<a href='{0}'>{1}</a>", doxType.SiteConfigPath.ToLower(), doxType.Name );

            return anchor;
        }

        public static string GetSectionId( DoxType doxType )
        {
            string id = String.Format( "{0}_{1}", doxType.kind, doxType.Name.ToLower() );

            return id;
        }

        public static string GetSectionId( MemberDef member )
        {
            string kind = member.kind == "function" ? "method" : member.kind;
            string id = String.Format( "{0}_{1}", kind, member.name.ToLower() );

            return id;
        }


        //public static string GetMethodNameWithParams( MethodBase method )
        //{
        //    string methodName = method.IsConstructor ? method.DeclaringType.Name : method.Name;

        //    ParameterInfo[] parameters = method.GetParameters();

        //    if( parameters.Length > 0 )
        //    {
        //        // Append the parameters to the method name.
        //        methodName += GetParamsString( method );
        //    }
        //    else
        //    {
        //        // For overloaded methods with no parameters, we want
        //        // to show empty parentheses, as in the .NET docs.
        //        methodName += "()";
        //    }

        //    return methodName;
        //}

        // This method doesn't embed the string in parentheses,
        // as does GetParamsString, because it's used for
        // creating topic IDs.
        //private static string GetFullNameParamsString( MethodBase method )
        //{
        //    string paramsString = String.Empty;

        //    ParameterInfo[] parameters = method.GetParameters();

        //    int genericParamIndex = 0;
        //    for( int i = 0; i < parameters.Length; i++ )
        //    {
        //        string fullName = String.Empty;
        //        ParameterInfo parameter = parameters[i];

        //        // Handle generic params.
        //        if( parameter.ParameterType.IsGenericParameter )
        //        {
        //            // If the parameter is a type parameter in the definition of a generic type or method,
        //            // use the special "`" format to represent it, e.g., "`0" for the first generic param
        //            // in the method's parameters list.
        //            fullName = String.Format( commentIdGenericParamFormat, genericParamIndex );
        //            genericParamIndex++;
        //        }
        //        else if( IsGenericType( parameter.ParameterType ) ||
        //                 IsArrayOfGenericType( parameter.ParameterType ) )
        //        {
        //            // If the parameter is a generic type or an array of a generic type, append the 
        //            // parameter type name to the params string.
        //            // Some types (e.g., generic parameters) don't have a full name. 
        //            fullName = parameter.ParameterType.Namespace + "." + parameter.ParameterType.Name;
        //        }
        //        else
        //        {
        //            if( parameter.ParameterType.FullName == null )
        //            {
        //                // Some types don't have a full name. 
        //                fullName = parameter.ParameterType.Namespace + "." + parameter.ParameterType.Name;
        //            }
        //            else
        //            {
        //                fullName = parameter.ParameterType.FullName;
        //            }
        //        }

        //        // Replace the '&' character for by-reference params
        //        // with the comment ID character, '@'.
        //        if( IsByRefParameter( parameter ) )
        //        {
        //            fullName = fullName.Replace( referenceCharacter, commentIdReferenceCharacter );
        //        }

        //        paramsString += fullName;

        //        // If there are more parameters, append a comma.
        //        if( i < parameters.Length - 1 )
        //        {
        //            paramsString += ",";
        //        }
        //    }

        //    return paramsString;
        //}

        //public static string GetParamsString( MethodBase method )
        //{
        //    string paramsString = String.Empty;

        //    ParameterInfo[] parameters = method.GetParameters();

        //    if( parameters.Length > 0 )
        //    {
        //        paramsString += "(";
        //    }

        //    for( int i = 0; i < parameters.Length; i++ )
        //    {   
        //        var parameter = parameters[i];
        //        var attributes = parameter.Attributes;
        //        if( attributes != ParameterAttributes.None )
        //        {
        //            string attributeString = String.Format( " [{0}] ", attributes.ToString() );
        //            paramsString += attributeString;
        //        }
                
        //        paramsString += GetFriendlyName( parameter.ParameterType );
        //        paramsString += " " + parameter.Name;

        //        if( i < parameters.Length - 1 )
        //        {
        //            paramsString += ", ";
        //        }
        //    }

        //    if( parameters.Length > 0 )
        //    {
        //        paramsString += " )";
        //    }

        //    return paramsString;
        //}

        //public static string GetUndecoratedName( ObservableType type )
        //{
        //    return GetUndecoratedName( type.Name );
        //}

        //public static string GetFriendlyName( Type type )
        //{
        //    return GetFriendlyName( new ObservableType( type ) );
        //}

        //public static string GetFriendlyName( ObservableType type )
        //{
        //    string friendlyName = GetUndecoratedName( type );

        //    if( IsGenericType( type ) && type.GenericArguments.Count > 0 )
        //    {
        //        friendlyName += "&lt;";

        //        for( int i = 0; i < type.GenericArguments.Count; i++ )
        //        {
        //            friendlyName += GetUndecoratedName( type.GenericArguments[i] );

        //            if( i < type.GenericArguments.Count - 1 )
        //            {
        //                friendlyName += ", ";
        //            }
        //        }

        //        friendlyName += "&gt;";
        //    }

        //    if( type.UnderlyingType.IsArray )
        //    {
        //        // Append the array characters that were removed by 
        //        // the GetUndecoratedName method.
        //        friendlyName += arrayCharacters;
        //    }

        //    return friendlyName;
        //}

        //private static bool IsGenericType( Type type )
        //{
        //    return ( type.IsGenericType || type.Name.Contains( genericCharacter ) || type.ContainsGenericParameters );
        //}

        //private static bool IsGenericType( ObservableType type )
        //{
        //    return IsGenericType( type.UnderlyingType );
        //}

        //public static string GetUndecoratedName( MethodBase method )
        //{
        //    string methodName = method.IsConstructor ? method.DeclaringType.Name : method.Name;
        //    return GetUndecoratedName( methodName );
        //}

        public static string GetUndecoratedName( string applies )
        {
            string undecoratedName = String.Copy( applies );

            if( undecoratedName.Contains( genericCharacter ) )
            {
                // Assume that everything after the generic character
                // represents the number of generic parameters. 
                int index = undecoratedName.IndexOf( genericCharacter );
                undecoratedName = undecoratedName.Remove( index );
            }

            if( undecoratedName.Contains( referenceCharacter ) )
            {
                int index = undecoratedName.IndexOf( referenceCharacter );
                undecoratedName = undecoratedName.Remove( index );
            }

            if( undecoratedName.Contains( arrayCharacters ) )
            {
                undecoratedName = undecoratedName.Replace( arrayCharacters, String.Empty );
            }

            return undecoratedName;

        }

        public static void FilterEnumMethods( DoxType doxType )
        { 
            if( doxType.IsEnum )
            {
                var methodsToFilter = doxType.MemberDefs.Where( m => enumMethods.Contains( m.name ) ).ToList();
                foreach( var method in methodsToFilter )
                {
                    doxType.MemberDefs.Remove( method );
                }
            }
        }


        public static void FilterObjectMethods( DoxType doxType )
        {
            var methodsToFilter = doxType.MemberDefs.Where( m => objectMethods.Contains( m.name ) ).ToList();
            foreach( var method in methodsToFilter )
            {
                doxType.MemberDefs.Remove( method );
            }
        }

        public static void FilterAccessorMethods( DoxType doxType )
        {
            var methodsToFilter = doxType.MemberDefs.Where( m => IsAccessorMethod( m.name ) ).ToList();
            foreach( var method in methodsToFilter )
            {
                doxType.MemberDefs.Remove( method );
            }
        }

        private static bool IsAccessorMethod( string methodName )
        {
            bool isAccessorMethod = IsPropertyAccessorMethod( methodName ) || IsEventAccessorMethod( methodName );
            return isAccessorMethod;
        }

        private static bool IsPropertyAccessorMethod( string methodName )
        {
            return ( methodName.ToLower().StartsWith( "get_" ) ||
                     methodName.ToLower().StartsWith( "set_" ) ||
                     methodName.ToLower().StartsWith( "put_" ) );
        }

        private static bool IsGetAccessorMethod( string methodName )
        {
            return ( methodName.ToLower().StartsWith( "get_" ) );
        }

        private static bool IsPutAccessorMethod( string methodName )
        {
            return ( methodName.ToLower().StartsWith( "set_" ) ||
                     methodName.ToLower().StartsWith( "put_" ) );
        }

        private static bool IsEventAccessorMethod( string methodName )
        {
            return ( methodName.ToLower().StartsWith( "add_" ) ||
                     methodName.ToLower().StartsWith( "remove_" ) );
        }

        private static bool IsByRefParameter( ParameterInfo pi )
        {
            return ( IsByRefType( pi.ParameterType ) );
        }

        private static bool IsByRefType( Type type )
        {
#if DEBUG
            if( type.IsByRef )
            {
                Trace.Assert( type.Name.Contains( referenceCharacter ) );
            }
#endif
            return type.IsByRef;
        }

        private static bool IsArrayType( Type type )
        {
            return type.IsArray;
        }

        //private static bool IsArrayType( ObservableType type )
        //{
        //    return ( IsArrayType( type.UnderlyingType ) );
        //}


        // type.IsArray returns false for array ref parameters.
        private static bool IsArrayRefType( Type type )
        {
            return (
                type.IsByRef &&
                type.Name.Contains( arrayCharacters ) );
        }

        //private static bool IsArrayRefType( ObservableType type )
        //{
        //    return ( IsArrayRefType( type.UnderlyingType ) );
        //}

        // Use this when you need to detect an array of a generic type.
        // type.IsGenericType returns false in this case, which is
        // correct but unhelpful.
        //private static bool IsArrayOfGenericType( Type type )
        //{
        //    return ( IsArrayType( type ) && IsGenericType( type ) );
        //}

        //private static bool IsArrayOfGenericType( ObservableType type )
        //{
        //    return ( IsArrayOfGenericType( type.UnderlyingType ) );
        //}

        ///////////////////////////////////////////////////////////////////////
        #region Topic ID Utilities

        // The following methods support generating topic IDs, 
        // which are used for Intellisense, etc.
        // For an overview of the topic ID schema, see
        // "Processing the XML File" at
        // http://msdn.microsoft.com/en-us/library/fsbx0t7x.aspx.

        /// <summary>
        /// Creates a topic ID for the specified namespace.
        /// </summary>
        /// <param name="ns">The namespace to generate the topic ID for.</param>
        /// <returns>The topic ID that corresponds with <paramref name="ns"/>.</returns>
        public static string GetTopicIdForNamespce( string ns )
        {
            string topicId = String.Format( namespaceIdStringFormat, ns );

            return topicId;
        }

        /// <summary>
        /// Creates a topic ID for the specified type.
        /// </summary>
        /// <param name="type">The type to generate the topic ID for.</param>
        /// <returns>The topic ID that corresponds with <paramref name="type"/>.</returns>
        //public static string GetTopicIdForType( ObservableType type )
        //{
        //    string topicId = null;

        //    string typeName = GetFriendlyName( type );

        //    if( type.IsGeneric )
        //    {
        //        topicId = String.Format(
        //            genericTypeIdStringFormat,
        //            type.Namespace,
        //            GetUndecoratedName( type ),
        //            type.GenericArguments.Count );
        //    }
        //    else
        //    {
        //        topicId = String.Format(
        //            typeIdStringFormat,
        //            type.Namespace,
        //            typeName );
        //    }

        //    return topicId;
        //}

        /// <summary>
        /// Creates a topic ID for the specified member on the specified type.
        /// </summary>
        /// <param name="mi">The member to generate the topic ID for.</param>
        /// <param name="type">The type that declares <paramref name="mi"/>.</param>
        /// <returns>The topic ID that corresponds with <paramref name="mi"/>.</returns>
        //public static string GetTopicIdForMember( MemberInfo mi, ObservableType type )
        //{
        //    string topicId = null;

        //    if( mi is FieldInfo )
        //    {
        //        topicId = GetTopicIdForField( mi as FieldInfo, type );
        //    }
        //    else if( mi is PropertyInfo )
        //    {
        //        topicId = GetTopicIdForProperty( mi as PropertyInfo, type );
        //    }
        //    else if( mi is MethodBase )
        //    {
        //        topicId = GetTopicIdForMethod( mi as MethodBase, type );
        //    }
        //    else if( mi is EventInfo )
        //    {
        //        topicId = GetTopicIdForEvent( mi as EventInfo, type );
        //    }

        //    return topicId;
        //}
        //public static string GetTopicIdForField( FieldInfo fi, ObservableType type )
        //{
        //    string topicId = String.Format( fieldIdStringFormat, type.Namespace, type.Name, fi.Name );

        //    return topicId;
        //}


        ///// <summary>
        ///// Creates a topic ID for the specified property on the specified type.
        ///// </summary>
        ///// <param name="pi">The property to generate the topic ID for.</param>
        ///// <param name="type">The type that declares <paramref name="pi"/>.</param>
        ///// <returns>The topic ID that corresponds with <paramref name="pi"/>.</returns>
        //public static string GetTopicIdForProperty( PropertyInfo pi, ObservableType type )
        //{
        //    string topicId = String.Format( propertyIdStringFormat, type.Namespace, type.Name, pi.Name );

        //    return topicId;
        //}

        /// <summary>
        /// Creates a topic ID for the specified method on the specified type.
        /// </summary>
        /// <param name="method">The method to generate the topic ID for.</param>
        /// <param name="type">The type that declares <paramref name="method"/>.</param>
        /// <returns>The topic ID that corresponds with <paramref name="method"/>.</returns>
        //public static string GetTopicIdForMethod( MethodBase method, ObservableType type )
        //{
        //    string topicId = null;

        //    ParameterInfo[] parameters = method.GetParameters();
        //    bool hasParams = parameters.Length > 0 ? true : false;

        //    string paramsString = Utilities.GetFullNameParamsString( method );

        //    if( method.IsConstructor )
        //    {
        //        if( hasParams )
        //        {
        //            // TFS 224957: Overloads are just methods everywhere
        //            // but in the method_overlod_winrt page type. 
        //            topicId = String.Format(
        //                paramsCtorIdStringFormat, //overloadedParamsCtorIdStringFormat,
        //                type.Namespace,
        //                type.Name,
        //                paramsString );
        //        }
        //        else
        //        {
        //            topicId = String.Format(
        //                defaultCtorIdStringFormat,
        //                type.Namespace,
        //                type.Name );
        //        }
        //    }
        //    else
        //    {
        //        if( hasParams )
        //        {
        //            topicId = String.Format(
        //                paramsMethodIdStringFormat,
        //                type.Namespace,
        //                type.Name,
        //                method.Name,
        //                paramsString );
        //        }
        //        else
        //        {
        //            topicId = String.Format(
        //                methodIdStringFormat,
        //                type.Namespace,
        //                type.Name,
        //                method.Name );
        //        }
        //    }

        //    return topicId;
        //}

        /// <summary>
        /// Creates a topic ID for the specified event on the specified type.
        /// </summary>
        /// <param name="ei">The event to generate the topic ID for.</param>
        /// <param name="type">The type that declares <paramref name="ei"/>.</param>
        /// <returns>The topic ID that corresponds with <paramref name="ei"/>.</returns>

        //public static string GetTopicIdForEvent( EventInfo ei, ObservableType type )
        //{
        //    string topicId = String.Format( eventIdStringFormat, type.Namespace, type.Name, ei.Name );

        //    return topicId;
        //}

        #endregion

        private const char genericCharacter = '`';
        private const char referenceCharacter = '&';
        private const char separatorCharacter = '_';
        private const string arrayCharacters = "[]";
        private const string arrayRankWarningFormat = "WARNING: Parameter {0} has array rank > 2. Markup does not match ABI.";

        // TBD: This string should go in config.
        //public const string emptyDescriptionNagFormat = "Doc this {0} and visit [Analog docs comment hunt](http://analogdocs) to collect your bounty.";
        public const string emptyDescriptionNagString = "TBD";

        // Filter out these methods from the object base class of managed types. 
        private static string[] objectMethods = new string[] { "Equals", "Finalize", "GetHashCode", "GetType", "MemberwiseClone", "ToString" };

        // Filter out these methods from the base class of managed enums.  
        private static string[] enumMethods = new string[] { "CompareTo", "HasFlag", "GetTypeCode" };

        ///////////////////////////////////////////////////////////////////////
        #region Topic ID Strings

        // The following string support generating topic IDs, 
        // which are used for Intellisense, etc.
        // For an overview of the topic ID schema, see
        // "Processing the XML File" at
        // http://msdn.microsoft.com/en-us/library/fsbx0t7x.aspx.

        // Namespace N: 'N:{0}'
        private const string namespaceIdStringFormat = "N:{0}";

        // type X: 'T:N.X'
        private const string typeIdStringFormat = "T:{0}.{1}";

        // Generic type X<K,V>: 'T:N.X`2'
        private const string genericTypeIdStringFormat = "T:{0}.{1}`{2}";

        // class X default ctor: 'M:N.X.#ctor'
        private const string defaultCtorIdStringFormat = "M:{0}.{1}.#ctor";

        // class X ctor with params: M:N.X.#ctor(...)".
        private const string paramsCtorIdStringFormat = "M:{0}.{1}.#ctor({2})";

        // Overoaded class X ctor with params: Overload:N.X.#ctor(...)".
        private const string overloadedParamsCtorIdStringFormat = "Overload:{0}.{1}.#ctor({2})";

        // Method f in class X: 'M:N.X.f'
        private const string methodIdStringFormat = "M:{0}.{1}.{2}";

        // Method f with parameters in class X: 'M:N.X.f(...)'
        private const string paramsMethodIdStringFormat = "M:{0}.{1}.{2}({3})";

        // Overloaded method f without parameters in class X: 'Overload:N.X.f'
        private const string overloadedMethodIdStringFormat = "Overload:{0}.{1}.{2}";

        // Overloaded method f with parameters in class X: 'Overload:N.X.f(...)'
        private const string overloadedParamsMethodIdStringFormat = "Overload:{0}.{1}.{2}({3})";

        // Field q of class X: 'F:N.X.q'
        private const string fieldIdStringFormat = "F:{0}.{1}.{2}";

        // Constant PI in class X: 'F:N.X.PI'
        private const string constantIdStringFormat = "F:{0}.{1}.{2}";

        // Property prop in class X: 'P:N.X.prop'
        private const string propertyIdStringFormat = "P:{0}.{1}.{2}";

        // Event ev in class X: 'E:N.X.ev'
        private const string eventIdStringFormat = "E:{0}.{1}.{2}";

        // Nested class nest in class X: 'T:N.X.nest'
        private const string nestedClassIdStringFormat = "T:{0}.{1}.{2}";

        // Delegate D: 'T:N.X.D' 
        // TBD: Really? Not 'T:N.D'?
        private const string delegateIdStringFormat = "T:{0}.{1}.{2}";

        private const char commentIdReferenceCharacter = '@';
        private const string commentIdGenericParamFormat = "`{0}";

        #endregion

    }
}
