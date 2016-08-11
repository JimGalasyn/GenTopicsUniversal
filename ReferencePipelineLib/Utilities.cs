using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using OsgContentPublishing.EventLogging;
using OsgContentPublishing.ReferencePipelineLib.TypeTopology;
using OsgContentPublishing.ReferencePipelineLib.TypeTopology.Assembly;
using OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen;
using OsgContentPublishing.ReferencePipelineLib.TypeTopology.Projected;
using ReflectionUtilities;

namespace OsgContentPublishing.ReferencePipelineLib
{
    /// <summary>
    /// Provides tools for working with types, type names, topic IDs, and XML documents.
    /// </summary>
    public class Utilities
    {
        ///////////////////////////////////////////////////////////////////////
        #region Type and Member Name Helpers

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

            if( String.IsNullOrEmpty( typeName ) )
            {
                typeName = fullTypeName;
            }

            return typeName;
        }

        public static TypeDeclarationParseResults ParseRawTypeDeclaration( string typeDeclaration )
        {
            return TypeDeclarationParser.ParseTypeDeclaration( typeDeclaration );
        }


        public static string GetTypeDeclaration( DefinedParameter parameter )
        {
            bool isGenericType = parameter.Type is GenericParameterType;

            string paramTypeDeclaration = null;

            if( isGenericType )
            {
                paramTypeDeclaration = parameter.Type.Name;
            }

            //parameter.Type.GenericParameterTypes.Select( t => )

            if( parameter.IsArray )
            {
                paramTypeDeclaration += Utilities.arrayCharacters;
            }

            if( parameter.IsReference )
            {
                paramTypeDeclaration += referenceCharacter;
            }

            if( parameter.IsPointer )
            {
                string pointerDecl = pointerCharacterString;

                if( parameter.PointerDepth == 2 )
                {
                    pointerDecl = pointerPointerCharacterString;
                }

                paramTypeDeclaration += pointerDecl;
            }

            if( parameter.IsConst )
            {
                paramTypeDeclaration = paramTypeDeclaration.Insert( 0, constStringWithTrailingSpace );
            }

            

            return null;

        }

        public static string GetTypeDeclaration( DefinedMember definedMember )
        {
            return null;
        }

        public static string GetAnchor(
            DefinedType definedType,
            List<DefinedType> knownTypes,
            string siteConfigReferenceRootPath,
            bool isParam )
        {
            return GetAnchor(
                definedType,
                knownTypes,
                siteConfigReferenceRootPath,
                false,
                isParam );
        }

        public static string GetAnchor(
            DefinedType definedType,
            List<DefinedType> knownTypes,
            string siteConfigReferenceRootPath,
            bool useFullName,
            bool isParam )
        {
            string siteConfigPath = null;

            DefinedType resolvedType = definedType;

            if( definedType is DoxygenType )
            {
                resolvedType = Utilities.ResolveType( definedType as DoxygenType, knownTypes );
                if( resolvedType == null )
                {
                    resolvedType = definedType;
                }
            }

            if( resolvedType.IsSystemType || resolvedType is DoxygenFacadeType )
            {
                string msdnQueryString = "https://social.msdn.microsoft.com/Search/en-US?query={0}";

                siteConfigPath = String.Format(
                    msdnQueryString,
                    GetUndecoratedName( resolvedType ) );
            }
            else
            {
                // TODO: Move this to a function
                var formedRoot =
                    ( ( siteConfigReferenceRootPath != null ) &&
                    ( siteConfigReferenceRootPath.Length > 0 ) ) ?
                    siteConfigReferenceRootPath + "/" : "";
                siteConfigPath = String.Format(
                    "{0}{1}",
                    formedRoot,
                    Utilities.GetSafeString( resolvedType.FullName ) );
            }

            string typeName = resolvedType.FriendlyName;
            if( isParam )
            {
                typeName = resolvedType.Name;
            }
            else if( useFullName )
            {
                // Mostly for namespaces.
                typeName = resolvedType.FullName;
            }

            // TODO: Make this a build config option
            var extension =
                ( ( siteConfigReferenceRootPath == null ) ||
                ( siteConfigReferenceRootPath.Length == 0 ) ) ?
                ".html" : "";
            string anchor = String.Format(
                "<a href='{0}{1}'>{2}</a>",
                siteConfigPath.ToLower(),
                extension,
                typeName );

            return anchor;
        }


        public static string GetAnchor(
            Type type,
            List<DefinedType> knownTypes,
            string siteConfigReferenceRootPath,
            bool isParam )
        {
            DefinedType definedType = TypeFactory.CreateAssemblyType( type );

            return GetAnchor(
                definedType,
                knownTypes,
                siteConfigReferenceRootPath,
                isParam );
        }

        public static bool IsCppStyleTypeName( string fullTypeName )
        {
            return fullTypeName.Contains( "::" );
        }

        public static bool IsCSharpStyleTypeName( string fullTypeName )
        {
            return fullTypeName.Contains( "." );
        }

        public static string GetUndecoratedName( string typeName )
        {
            string undecoratedName = String.Copy( typeName );

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

            if( undecoratedName.Contains( pointerCharacter ) )
            {
                undecoratedName = undecoratedName.Replace( pointerCharacterString, String.Empty );
            }

            if( undecoratedName.Contains( arrayCharacters ) )
            {
                undecoratedName = undecoratedName.Replace( arrayCharacters, String.Empty );
            }

            undecoratedName = undecoratedName.Trim();

            return undecoratedName;
        }


        public static string GetUndecoratedName( ObservableType type )
        {
            return GetUndecoratedName( type.Name );
        }

        public static string GetUndecoratedName( DefinedType definedType )
        {
            return GetUndecoratedName( definedType.Name );
        }

        // TBD: Consider refactoring these into corresponding classes.
        public static string GetFriendlyName( AssemblyType assemblyType )
        {
            string friendlyName = null;

            if( assemblyType.UnderlyingType != null )
            {
                friendlyName = GetFriendlyName( assemblyType.UnderlyingType );
            }
            else
            {
                friendlyName = assemblyType.Name;
            }

            return friendlyName;
        }

        public static string GetFriendlyName( ProjectedType projectedType )
        {
            return GetFriendlyName( projectedType.AssemblyType );
        }

        public static string GetFriendlyName( PrimitiveType primitiveType )
        {
            return primitiveType.Name;
        }


        public static string GetFriendlyName( DefinedType definedType )
        {
            string friendlyName = null;

            if( definedType is AssemblyType )
            {
                AssemblyType type = definedType as AssemblyType;
                if( type.UnderlyingType != null )
                {
                    friendlyName = GetFriendlyName( type.UnderlyingType );
                }
                else
                {
                    friendlyName = definedType.Name;
                }
            }
            else if( definedType is ProjectedType )
            {
                ProjectedType type = definedType as ProjectedType;
                friendlyName = GetFriendlyName( type.AssemblyType.UnderlyingType );
            }
            else if( definedType is PrimitiveType )
            {
                friendlyName = definedType.Name;
            }
            else
            {
                // DoxygenType
                friendlyName = GetFriendlyName( definedType.Name );
            }

            //if( !( definedType is DoxygenFacadeType ) && definedType.IsGeneric )
            //{
            //    // Append the generic type declarations.
            //    string genericDeclarationsString =
            //        Utilities.GetGenericParametersDeclaration( definedType.GenericParameterTypes );

            //    friendlyName = friendlyName + genericDeclarationsString;
            //}

            // Finally, add spaces to comma delimiters.
            friendlyName = friendlyName.Replace( ",", ", " );

            return friendlyName;
        }

        public static string GetFriendlyName( string typeName )
        {
            string friendlyName = null;

            friendlyName = typeName.Replace( "<", "&lt;" );
            friendlyName = friendlyName.Replace( ">", "&gt;" );

            return friendlyName;
        }

        public static string GetFriendlyName( Type type )
        {
            return GetFriendlyName( new ObservableType( type ) );
        }

        public static string GetFriendlyName( ObservableType type, bool preserveArrayChars )
        {
            string friendlyName = GetUndecoratedName( type );

            if( IsGenericType( type ) && type.GenericArguments.Count > 0 )
            {
                friendlyName += "&lt;";

                for( int i = 0; i < type.GenericArguments.Count; i++ )
                {
                    friendlyName += GetUndecoratedName( type.GenericArguments[i] );

                    if( i < type.GenericArguments.Count - 1 )
                    {
                        friendlyName += ", ";
                    }
                }

                friendlyName += "&gt;";
            }

            if( preserveArrayChars )
            {
                if( type.UnderlyingType.IsArray )
                {
                    // Append the array characters that were removed by 
                    // the GetUndecoratedName method.
                    friendlyName += arrayCharacters;
                }
            }

            return friendlyName;
        }


        public static string GetFriendlyName( ObservableType type )
        {
            return GetFriendlyName( type, true );
        }

        private static bool IsGenericType( Type type )
        {
            return ( type.IsGenericType || type.Name.Contains( genericCharacter ) || type.ContainsGenericParameters );
        }

        private static bool IsGenericType( ObservableType type )
        {
            return IsGenericType( type.UnderlyingType );
        }

        public static string GetUndecoratedName( MethodBase method )
        {
            string methodName = method.IsConstructor ? method.DeclaringType.Name : method.Name;
            return GetUndecoratedName( methodName );
        }

        /// <summary>
        /// Converts the specified string to a string that GTU can use as a filename.
        /// </summary>
        /// <param name="s">The string to convert.</param>
        /// <returns>A string that can be used by GTU for a filename.</returns>
        /// <remarks><para>Order of operations in important: must match the longest strings first.
        /// </para>
        /// </remarks>
        public static string GetSafeString( string s )
        {
            string safeString = s.Replace( "operator<<=", "compound_shift_left_operator" );
            safeString = safeString.Replace( "operator>>=", "compound_shift_right_operator" );
            safeString = safeString.Replace( "operator<<", "shift_left_operator" );
            safeString = safeString.Replace( "operator>>", "shift_right_operator" );
            safeString = safeString.Replace( "operator<=", "operator_less_than_or_equal" );
            safeString = safeString.Replace( "operator>=", "operator_greater_than_or_equal" );
            safeString = safeString.Replace( "operator<", "operator_less_than" );
            safeString = safeString.Replace( "operator>", "operator_greater_than" );
            safeString = safeString.Replace( "operator==", "equality_operator" );
            safeString = safeString.Replace( "operator!=", "inequality_operator" );
            safeString = safeString.Replace( "operator*=", "compound_multiplication_operator" );
            safeString = safeString.Replace( "operator*", "multiplication_operator" );
            safeString = safeString.Replace( "operator+=", "compound_addition_operator" );
            safeString = safeString.Replace( "operator+", "addition_operator" );
            safeString = safeString.Replace( "operator-=", "compound_subtraction_operator" );
            safeString = safeString.Replace( "operator-", "subtraction_operator" );
            safeString = safeString.Replace( "operator/=", "compound_division_operator" );
            safeString = safeString.Replace( "operator/", "division_operator" );
            safeString = safeString.Replace( "operator^=", "compound_exp_operator" );
            safeString = safeString.Replace( "operator^", "exp_operator" );
            safeString = safeString.Replace( "operator%=", "compound_modulo_operator" );
            safeString = safeString.Replace( "operator%", "modulo_operator" );
            safeString = safeString.Replace( "operator&=", "compound_and_operator" );
            safeString = safeString.Replace( "operator&", "and_operator" );
            safeString = safeString.Replace( "operator|=", "compound_or_operator" );
            safeString = safeString.Replace( "operator|", "or_operator" );

            safeString = safeString.Replace( "operator=", "assignment_operator" );
            
            safeString = safeString.Replace( genericCharacter, separatorCharacter );

            if( safeString.Contains( referenceCharacter ) )
            {
                int index = safeString.IndexOf( referenceCharacter );
                safeString = safeString.Remove( index );
            }

            if( safeString.Contains( arrayCharacters ) )
            {
                safeString = safeString.Replace( arrayCharacters, String.Empty );
            }

            safeString = safeString.Replace( '<', separatorCharacter );
            safeString = safeString.Replace( '>', separatorCharacter );
            safeString = safeString.Replace( ',', separatorCharacter );
            safeString = safeString.Replace( " ", String.Empty );
            safeString = safeString.Trim();

            return safeString;
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

        //public static string GetParentNamespace( string fullNamespace )
        //{
        //    string parentNamespace = null;

        //    if( IsCppStyleTypeName( fullNamespace ) )
        //    {
        //        int indexOfParentNamespace = fullNamespace.LastIndexOf( "::" );
        //        parentNamespace = fullNamespace.Substring( indexOfParentNamespace + 2 );
        //    }
        //    else if( IsCSharpStyleTypeName( fullNamespace ) )
        //    {
        //        int indexOfParentNamespace = fullNamespace.LastIndexOf( "." );
        //        parentNamespace = fullNamespace.Substring( indexOfParentNamespace + 1 );
        //    }
        //    else
        //    {
        //        parentNamespace = fullNamespace;
        //    }

        //    return parentNamespace;
        //}

        public static string GetMethodSignature(
            DefinedMember member,
            List<DefinedType> knownTypes,
            string siteConfigReferenceRoot,
            List<string> namespaces )
        {
            string signature = null;

            if( member.IsMethod )
            {
                string paramsString = GetParamsString(
                    member,
                    knownTypes,
                    siteConfigReferenceRoot,
                    namespaces );

                string retVal = GetAnchor(
                    member.Type,
                    knownTypes,
                    siteConfigReferenceRoot,
                    false );

                string genericParamsString = String.Empty;
                if( member.IsGeneric )
                {
                    genericParamsString = GetGenericParametersDeclaration(
                        member.GenericParameterTypes );
                }

                string staticKeyword = member.IsStatic ? "static" : String.Empty;
                signature = String.Format(
                    "{0} {1} {2}{3}({4})",
                    staticKeyword,
                    retVal,
                    genericParamsString,
                    member.Name,
                    paramsString );
            }
            else if( member.IsEvent )
            {
                string retVal = GetAnchor(
                    member.Type,
                    knownTypes,
                    siteConfigReferenceRoot,
                    false );
                signature = String.Format( "{0} {1}()", retVal, member.Name );
            }
            else if( member.IsConstructor )
            {
                string paramsString = GetParamsString(
                    member,
                    knownTypes,
                    siteConfigReferenceRoot,
                    namespaces );
                signature = String.Format( "{0}({1})", member.ParentType.Name, paramsString );
            }
            else if( member.IsDestructor )
            {
                signature = String.Format( "~{0}()", member.ParentType.Name );
            }

            else
            {
                throw new ArgumentException( "is not a method", "member" );
            }

            return signature;
        }

        public static string GetGenericParametersDeclaration( List<DefinedType> genericParameterTypes )
        {
            string genericParamsString = String.Empty;
            if( genericParameterTypes != null && genericParameterTypes.Count > 0 )
            {
                genericParamsString += genericCharacterLtEntity;

                for( int i = 0; i < genericParameterTypes.Count; i++ )
                {
                    var genericParam = genericParameterTypes[i];
                    genericParamsString += genericParam.Name;

                    if( i < genericParameterTypes.Count - 1 )
                    {
                        genericParamsString += ",";
                    }
                }

                genericParamsString += genericCharacterGtEntity;
            }

            return genericParamsString;
        }

        public static string GetParamsString(
            DefinedMember definedMethod,
            List<DefinedType> knownTypes,
            string siteConfigReferenceRoot,
            List<string> namespaces )
        {
            string paramsString = null;

            // TBD: consider refactoring into corresponding classes.
            if( definedMethod is AssemblyMethod )
            {
                paramsString = GetParamsString(
                    definedMethod as AssemblyMethod,
                    knownTypes,
                    siteConfigReferenceRoot );
            }
            else if( definedMethod is DoxygenMethod )
            {
                paramsString = GetParamsString(
                    definedMethod as DoxygenMethod,
                    knownTypes,
                    siteConfigReferenceRoot );
            }

            return paramsString;
        }

        public static string GetParamsString(
            MethodBase method,
            List<DefinedType> knownTypes,
            string siteConfigReferenceRoot )
        {
            string paramsString = String.Empty;

            ParameterInfo[] parameters = method.GetParameters();

            //int genericParamIndex = 0;
            for( int i = 0; i < parameters.Length; i++ )
            {
                // TBD: decide on representation of pointer types
                // Currently using the ABI representation.
                ParameterInfo parameter = parameters[i];

                string paramTypeName = GetAnchor(
                    parameter.ParameterType,
                    knownTypes,
                    siteConfigReferenceRoot,
                    true );
                string paramName = parameter.Name;
                string paramSubstring = String.Format( "{0} {1}", paramTypeName, paramName );

#if false
                // Handle generic params.
                if( parameter.ParameterType.IsGenericParameter )
                {
                    // If the parameter is a type parameter in the definition of a generic type or method,
                    // use the special "`" format to represent it, e.g., "`0" for the first generic param
                    // in the method's parameters list.
                    paramName = String.Format( commentIdGenericParamFormat, genericParamIndex );
                    genericParamIndex++;
                }
                else if( IsGenericType( parameter.ParameterType ) ||
                         IsArrayOfGenericType( parameter.ParameterType ) )
                {
                    // If the parameter is a generic type or an array of a generic type, append the 
                    // parameter type name to the params string.
                    // Some types (e.g., generic parameters) don't have a full name. 
                    paramName = parameter.ParameterType.Namespace + "." + parameter.ParameterType.Name;
                }
                else
                {
                    if( parameter.ParameterType.FullName == null )
                    {
                        // Some types don't have a full name. 
                        paramName = parameter.ParameterType.Namespace + "." + parameter.ParameterType.Name;
                    }
                    else
                    {
                        paramName = parameter.ParameterType.FullName;
                    }
                }

                // Replace the '&' character for by-reference params
                // with the comment ID character, '@'.
                if( IsByRefParameter( parameter ) )
                {
                    paramName = paramName.Replace( referenceCharacter, commentIdReferenceCharacter );
                }
#endif
                paramsString += paramSubstring;

                // If there are more parameters, append a comma.
                if( i < parameters.Length - 1 )
                {
                    paramsString += ", ";
                }
            }

            return paramsString;
        }

        public static string GetParamsString(
            AssemblyMethod assemblyMethod,
            List<DefinedType> knownTypes,
            string siteConfigReferenceRoot )
        {
            string paramsString = null;

            MethodBase methodBase = null;

            if( assemblyMethod.IsConstructor )
            {
                methodBase = ( (AssemblyConstructor)assemblyMethod ).UnderlyingConstructorInfo as MethodBase;
            }
            else
            {
                methodBase = ( (AssemblyMethod)assemblyMethod ).UnderlyingMember as MethodBase;
            }

            paramsString = GetParamsString( methodBase, knownTypes, siteConfigReferenceRoot );

            return paramsString;
        }

        public static string GetParamsString(
            DoxygenMethod doxygenMethod,
            List<DefinedType> knownTypes,
            string siteConfigReferenceRoot )
        {
            string paramsString = String.Empty;

            for( int i = 0; i < doxygenMethod.Parameters.Count; i++ )
            {
                // TBD: decide on representation of pointer types
                // Currently using the ABI representation.
                DefinedParameter parameter = doxygenMethod.Parameters[i];

                bool isGenericType = parameter.Type is GenericParameterType;

                string paramTypeName = null;

                if( isGenericType )
                {
                    paramTypeName = parameter.Type.Name;
                }
                else
                {
                    paramTypeName = GetAnchor(
                        parameter.Type,
                        knownTypes,
                        siteConfigReferenceRoot,
                        true );
                }

                if( parameter.IsArray )
                {
                    paramTypeName += Utilities.arrayCharacters;
                }

                if( parameter.IsReference )
                {
                    paramTypeName += referenceCharacter;
                }

                if( parameter.IsPointer )
                {
                    string pointerDecl = pointerCharacterString;

                    if( parameter.PointerDepth == 2 )
                    {
                        pointerDecl = pointerPointerCharacterString;
                    }

                    paramTypeName += pointerDecl;
                }

                if( parameter.IsConst )
                {
                    paramTypeName = paramTypeName.Insert( 0, constStringWithTrailingSpace );
                }

                string paramName = parameter.Name;
                string paramSubstring = String.Format( "{0} {1}", paramTypeName, paramName );
                paramsString += paramSubstring;

                // If there are more parameters, append a comma.
                if( i < doxygenMethod.Parameters.Count - 1 )
                {
                    paramsString += ", ";
                }
            }

            return paramsString;
        }

        #endregion

        ///////////////////////////////////////////////////////////////////////
        #region Uniqueness Testing

        public class DefinedMemberComparer : IEqualityComparer<DefinedMember>
        {
            public bool Equals( DefinedMember lhs, DefinedMember rhs )
            {
                try
                {
                    // Check whether the compared objects reference the same data.
                    if( Object.ReferenceEquals( lhs, rhs ) )
                    {
                        return true;
                    }

                    //Check whether either of the compared objects is null.
                    if( Object.ReferenceEquals( lhs, null ) ||
                        Object.ReferenceEquals( rhs, null ) )
                    {
                        return false;
                    }

                    // TBD: Will need to do this for deep inheritance graphs, e.g. XAML types.
                    //if( lhs.DeclaringType != rhs.DeclaringType )
                    //{
                    //    return false;
                    //}

                    // TBD: This is the proper way to test for equality.
                    //if( lhs.TopicId != rhs.TopicId )
                    //{
                    //    return false;
                    //}

                    // TBD: This is a heuristic hack. It may fail for
                    // method overloads that have the same number of params.
                    // Note that comparisons across type systems will be 
                    // are difficult, because equivalent methods may have 
                    // different signatures, e.g., [out,retval] params in IDL
                    // become return values in .NET.
                    // The RIDL OverloadAttribute explicitly associates overloaded 
                    // methods in projected types, so that's helpful in the 
                    // implementation of AssemblyMethod.IsOverload and
                    // AssemblyMethod.OverloadName. 
                    if( lhs.IsMethod && rhs.IsMethod )
                    {
                        if( lhs is AssemblyMethod && rhs is AssemblyMethod )
                        {
                            if( lhs.Name == rhs.Name )
                            {
                                AssemblyMethod lhsAssemblyMethod = lhs as AssemblyMethod;
                                AssemblyMethod rhsAssemblyMethod = rhs as AssemblyMethod;

                                if( lhsAssemblyMethod.Parameters.Count == rhsAssemblyMethod.Parameters.Count )
                                {
                                    // TBD: check parameter types
                                    return true;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                        }
                        else if( lhs is AssemblyMethod && rhs is DoxygenMethod )
                        {
                            AssemblyMethod lhsAssemblyMethod = lhs as AssemblyMethod;
                            DoxygenMethod rhsDoxygenMethod = rhs as DoxygenMethod;
                            if( lhsAssemblyMethod.IsOverload )
                            {
                                if( lhsAssemblyMethod.OverloadName == rhsDoxygenMethod.Name )
                                {
                                    // TBD: probably unnecessary
                                    if( lhsAssemblyMethod.Parameters.Count == rhsDoxygenMethod.Parameters.Count )
                                    {
                                        return true;
                                    }
                                }
                                else
                                {
                                    return false;
                                }
                            }
                        }
                        else if( lhs is DoxygenMethod && rhs is AssemblyMethod )
                        {
                            DoxygenMethod lhsDoxygenMethod = lhs as DoxygenMethod;
                            AssemblyMethod rhsAssemblyMethod = rhs as AssemblyMethod;
                            if( rhsAssemblyMethod.IsOverload )
                            {
                                if( rhsAssemblyMethod.OverloadName == lhsDoxygenMethod.Name )
                                {
                                    // TBD: probably unnecessary
                                    if( lhsDoxygenMethod.Parameters.Count == rhsAssemblyMethod.Parameters.Count )
                                    {
                                        return true;
                                    }
                                }
                                else
                                {
                                    return false;
                                }
                            }
                        }
                    }

                    if( lhs.Name == rhs.Name )
                    {
                        return true;
                    }

                    return false;
                }
                catch( Exception ex )
                {
                    GenTopicsEventLogger.Log.LogError( ex.ToString() );
                    return false;
                }
            }

            public int GetHashCode( DefinedMember definedMember )
            {
                //Check whether the object is null
                if( Object.ReferenceEquals( definedMember, null ) ) return 0;

                //Calculate the hash code for the property.
                int hashCode = definedMember.TopicId.GetHashCode();

                ////Get hash code for the Name field if it is not null.
                //int hashPropertyName = pi.Name == null ? 0 : pi.Name.GetHashCode();

                ////Calculate the hash code for the property.
                //int hashCode = hashPropertyName.GetHashCode();

                //int propertyTypeHashCode = pi.PropertyType.Name.GetHashCode();
                //hashCode ^= propertyTypeHashCode;

                ////int declaringTypeHashCode = mi.DeclaringType.Name.GetHashCode();
                ////hashCode ^= declaringTypeHashCode;

                return hashCode;
            }

        }

        #endregion

        ///////////////////////////////////////////////////////////////////////
        #region Topic ID Utilities

        // The following methods support generating topic IDs, 
        // which are used for Intellisense, uniqueness testing, etc.
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
        public static string GetTopicIdForType( ObservableType type )
        {
            string topicId = null;

            string typeName = GetFriendlyName( type );

            if( type.IsGeneric )
            {
                topicId = String.Format(
                    genericTypeIdStringFormat,
                    type.Namespace,
                    GetUndecoratedName( type ),
                    type.GenericArguments.Count );
            }
            else
            {
                topicId = String.Format(
                    typeIdStringFormat,
                    type.Namespace,
                    typeName );
            }

            return topicId;
        }





        /// <summary>
        /// Creates a topic ID for the specified member on the specified type.
        /// </summary>
        /// <param name="mi">The member to generate the topic ID for.</param>
        /// <param name="type">The type that declares <paramref name="mi"/>.</param>
        /// <returns>The topic ID that corresponds with <paramref name="mi"/>.</returns>
        public static string GetTopicIdForMember( MemberInfo mi, ObservableType type )
        {
            string topicId = null;

            if( mi is FieldInfo )
            {
                topicId = GetTopicIdForField( mi as FieldInfo, type );
            }
            else if( mi is PropertyInfo )
            {
                topicId = GetTopicIdForProperty( mi as PropertyInfo, type );
            }
            else if( mi is MethodBase )
            {
                topicId = GetTopicIdForMethod( mi as MethodBase, type );
            }
            else if( mi is EventInfo )
            {
                topicId = GetTopicIdForEvent( mi as EventInfo, type );
            }

            return topicId;
        }
        public static string GetTopicIdForField( FieldInfo fi, ObservableType type )
        {
            string topicId = String.Format( fieldIdStringFormat, type.Namespace, type.Name, fi.Name );

            return topicId;
        }


        ///// <summary>
        ///// Creates a topic ID for the specified property on the specified type.
        ///// </summary>
        ///// <param name="pi">The property to generate the topic ID for.</param>
        ///// <param name="type">The type that declares <paramref name="pi"/>.</param>
        ///// <returns>The topic ID that corresponds with <paramref name="pi"/>.</returns>
        public static string GetTopicIdForProperty( PropertyInfo pi, ObservableType type )
        {
            string topicId = String.Format( propertyIdStringFormat, type.Namespace, type.Name, pi.Name );

            return topicId;
        }

        /// <summary>
        /// Creates a topic ID for the specified method on the specified type.
        /// </summary>
        /// <param name="method">The method to generate the topic ID for.</param>
        /// <param name="type">The type that declares <paramref name="method"/>.</param>
        /// <returns>The topic ID that corresponds with <paramref name="method"/>.</returns>
        public static string GetTopicIdForMethod( MethodBase method, ObservableType type )
        {
            string topicId = null;

            ParameterInfo[] parameters = method.GetParameters();
            bool hasParams = parameters.Length > 0 ? true : false;

            string paramsString = Utilities.GetFullNameParamsString( method );

            if( method.IsConstructor )
            {
                if( hasParams )
                {
                    // TFS 224957: Overloads are just methods everywhere
                    // but in the method_overlod_winrt page type. 
                    topicId = String.Format(
                        paramsCtorIdStringFormat, //overloadedParamsCtorIdStringFormat,
                        type.Namespace,
                        type.Name,
                        paramsString );
                }
                else
                {
                    topicId = String.Format(
                        defaultCtorIdStringFormat,
                        type.Namespace,
                        type.Name );
                }
            }
            else
            {
                if( hasParams )
                {
                    topicId = String.Format(
                        paramsMethodIdStringFormat,
                        type.Namespace,
                        type.Name,
                        method.Name,
                        paramsString );
                }
                else
                {
                    topicId = String.Format(
                        methodIdStringFormat,
                        type.Namespace,
                        type.Name,
                        method.Name );
                }
            }

            return topicId;
        }

        /// <summary>
        /// Creates a topic ID for the specified event on the specified type.
        /// </summary>
        /// <param name="ei">The event to generate the topic ID for.</param>
        /// <param name="type">The type that declares <paramref name="ei"/>.</param>
        /// <returns>The topic ID that corresponds with <paramref name="ei"/>.</returns>
        public static string GetTopicIdForEvent( EventInfo ei, ObservableType type )
        {
            string topicId = String.Format( eventIdStringFormat, type.Namespace, type.Name, ei.Name );

            return topicId;
        }

        public static string GetFullNameParamsString( DefinedMember member, string siteConfigReferenceRoot )
        {
            AssemblyMethod method = member as AssemblyMethod;

            if( method != null )
            {
                return GetFullNameParamsString( method.UnderlyingMember as MethodBase, siteConfigReferenceRoot );
            }
            else
            {
                return String.Empty;
            }

            // TBD 
            //DoxygenMethod doxygenMethod = member as DoxygenMethod;

            //if( doxygenMethod != null )
            //{
            //    return GetFullNameParamsString( doxygenMethod.UnderlyingMember, siteConfigReferenceRoot );
            //}
        }

        //public static string GetFullNameParamsString( MemberInfo memberInfo )
        //{
        //    return GetFullNameParamsString( memberInfo as MethodBase, null );
        //}


        public static string GetFullNameParamsString( MethodBase method )
        {
            return GetFullNameParamsString( method, null );
        }

        // This method doesn't embed the string in parentheses,
        // as does GetParamsString, because it's used for
        // creating topic IDs.
        public static string GetFullNameParamsString( MethodBase method, string siteConfigReferenceRoot )
        {
            string paramsString = String.Empty;

            ParameterInfo[] parameters = method.GetParameters();

            int genericParamIndex = 0;
            for( int i = 0; i < parameters.Length; i++ )
            {
                string fullName = String.Empty;
                ParameterInfo parameter = parameters[i];

                // Handle generic params.
                if( parameter.ParameterType.IsGenericParameter )
                {
                    // If the parameter is a type parameter in the definition of a generic type or method,
                    // use the special "`" format to represent it, e.g., "`0" for the first generic param
                    // in the method's parameters list.
                    fullName = String.Format( commentIdGenericParamFormat, genericParamIndex );
                    genericParamIndex++;
                }
                else if( IsGenericType( parameter.ParameterType ) ||
                         IsArrayOfGenericType( parameter.ParameterType ) )
                {
                    // If the parameter is a generic type or an array of a generic type, append the 
                    // parameter type name to the params string.
                    // Some types (e.g., generic parameters) don't have a full name. 
                    fullName = parameter.ParameterType.Namespace + "." + parameter.ParameterType.Name;
                }
                else
                {
                    if( parameter.ParameterType.FullName == null )
                    {
                        // Some types don't have a full name. 
                        fullName = parameter.ParameterType.Namespace + "." + parameter.ParameterType.Name;
                    }
                    else
                    {
                        fullName = parameter.ParameterType.FullName;
                    }
                }

                // Replace the '&' character for by-reference params
                // with the comment ID character, '@'.
                if( IsByRefParameter( parameter ) )
                {
                    fullName = fullName.Replace( referenceCharacter, commentIdReferenceCharacter );
                }

                //if( siteConfigReferenceRoot  != null )
                //{
                //    string fullNameLink = Utilities.GetAnchor( parameter.ParameterType, siteConfigReferenceRoot );
                //}

                paramsString += fullName;

                // If there are more parameters, append a comma.
                if( i < parameters.Length - 1 )
                {
                    paramsString += ",";
                }
            }

            return paramsString;
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


        private static bool IsArrayRefType( ObservableType type )
        {
            return ( IsArrayRefType( type.UnderlyingType ) );
        }

        // type.IsArray returns false for array ref parameters.
        private static bool IsArrayRefType( Type type )
        {
            return (
                type.IsByRef &&
                type.Name.Contains( arrayCharacters ) );
        }


        // Use this when you need to detect an array of a generic type.
        // type.IsGenericType returns false in this case, which is
        // correct but unhelpful.
        private static bool IsArrayOfGenericType( Type type )
        {
            return ( IsArrayType( type ) && IsGenericType( type ) );
        }

        private static bool IsArrayOfGenericType( ObservableType type )
        {
            return ( IsArrayOfGenericType( type.UnderlyingType ) );
        }


        #endregion

        ///////////////////////////////////////////////////////////////////////
        #region XML helpers

        public static XDocument LoadXml( string fullPath )
        {
            XDocument doc = null;

            using( FileStream fileStream = new FileStream( fullPath, FileMode.Open ) )
            {
                XmlSanitizingStream xmlSanitizingStream = new XmlSanitizingStream( fileStream );
                doc = XDocument.Load( xmlSanitizingStream );
            }

            //XDocument doc = XDocument.Load( "file://" + fullPath, LoadOptions.PreserveWhitespace );

            return doc;
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
                Debug.WriteLine( ex.ToString() );
            }

            return childElement;
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
                // This exception is expected for certain types.
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
                // This exception is expected for certain types.
                Debug.WriteLine( ex.ToString() );
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


        public static List<XElement> TryGetChildElements( XElement parentElement, string elementName )
        {
            List<XElement> childElements = null;

            try
            {
                childElements = GetChildElements( parentElement, elementName );
            }
            catch( Exception ex )
            {
                Debug.WriteLine( ex.ToString() );
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
                Debug.WriteLine( ex.ToString() );
            }

            return attribute;
        }

        #endregion

        ///////////////////////////////////////////////////////////////////////
        #region Type resolution

        /// <summary>
        /// Matches DoxygenTypes to corresponding ProjectedTypes and AssemblyTypes. 
        /// </summary>
        /// <param name="definedTypes">The list of types to resolve.</param>
        /// <returns>A list of resolved types.</returns>
        /// <remarks>
        /// <para>GTU collects types from Doxygen output and also from managed assemblies
        /// and winmd files. Frequently, Doxygen types are unresolved and consist solely
        /// of opaque strings (facade types). The <see cref="ResolveTypes"/> method finds 
        /// Doxygen types in the provided list and looks for corresponding types that were 
        /// loaded from an assembly. 
        /// </para>
        /// <para>GTU always prefers <see cref="ProjectedType"/> as the canonical type 
        /// representation. This is because the type metadata comes from a managed assembly
        /// or winmd file, which means that the type information was generated by a 
        /// proper compiler. The <see cref="DoxygenType"/> class is populated with type
        /// information from Doxygen XML files, which may have missing type metadata, as
        /// well as lexing errors of various sorts. Ideally, the only purpose of the
        /// <see cref="DoxygenType"/> class is to provide API reference content, in the 
        /// form of comments scraped from source code. 
        /// </para>
        /// </remarks>
        public static List<DefinedType> ResolveTypes( List<DefinedType> definedTypes )
        {
            List<DefinedType> resolvedTypes = new List<DefinedType>();
            List<DefinedType> unResolvedTypes = new List<DefinedType>();

            // Find the ProjectedTypes in the collection.
            var projectedTypes = definedTypes.Where( t => t is ProjectedType );
            var projectedTypesList = projectedTypes.Select( t => t as ProjectedType ).ToList();

            // Find the AssemblyTypes that aren't contained in a ProjectedType.
            var assemblyTypes = definedTypes.Where( t => t is AssemblyType );
            var notProjectedAssemblyTypesCast = assemblyTypes.Select( t => t as AssemblyType );
            var notProjectedAssemblyTypes = notProjectedAssemblyTypesCast.Where( t => !t.IsInProjectedType );

            // Find the DoxygenTypes that aren't contained in a ProjectedType. 
            var notProjectedDoxygenTypes = definedTypes.Where( t => t is DoxygenType );
            var notProjectedDoxygenTypesCast = notProjectedDoxygenTypes.Select( t => t as DoxygenType );
            var notProjectedDoxygenTypesList = notProjectedDoxygenTypesCast.Where( t => !t.IsInProjectedType );

            // Collect the ProjectedTypes and the AssemblyTypes that aren't contained in a ProjectedType.
            // This is the canonical collection of types against which DoxygenTypes are resolved.
            List<DefinedType> allProjectedAndAssemblyTypesList = new List<DefinedType>();
            allProjectedAndAssemblyTypesList.AddRange( projectedTypesList );
            allProjectedAndAssemblyTypesList.AddRange( notProjectedAssemblyTypes );
            resolvedTypes.AddRange( allProjectedAndAssemblyTypesList );

            // The allProjectedAndAssemblyTypesList is sufficient for RidlPipelines,
            // which assume that all type info for a feature comes from a winmd file only.

            var count = allProjectedAndAssemblyTypesList.Where( t => t.ChildTypes.Count > 0 ).ToList();


            DefinedType resolvedType = null;

            // Try to find each DoxygenType in the canonical list of types. 
            foreach( var doxygenType in notProjectedDoxygenTypesList )
            {
                resolvedType = Utilities.ResolveType( doxygenType, allProjectedAndAssemblyTypesList );
                if( resolvedType != null )
                {
                    // The DoxygenType was found in the canonical type list,
                    // so add it to the list of resolved types.
                    resolvedTypes.Add( resolvedType );
                }
                else
                {
                    // The DoxygenType wasn't found in the canonical type list,
                    // so add it to the list of unresolved types.
                    // This collection is for debugging convenience only.
                    unResolvedTypes.Add( doxygenType );
                }
            }

            // Order the list of resolved types, for debugging convience.
            List<DefinedType> resolvedTypesOrdered = resolvedTypes.OrderBy( t => t.FullName ).ToList();

            return resolvedTypesOrdered;
        }

        //private static List<DefinedType> FixupParentProjectedTypes()
        //{

        //}

        /// <summary>
        /// Finds the specified type in the provided canonical list of types.
        /// </summary>
        /// <param name="doxygenType">The type to resolve.</param>
        /// <param name="allProjectedAndAssemblyTypes">The canonical list of types to search for <paramref name="doxygenType"/> in.</param>
        /// <returns>The resolved type, if successful; otherwise, null.</returns>
        public static DefinedType ResolveType( DoxygenType doxygenType, List<DefinedType> allProjectedAndAssemblyTypes )
        {
            DefinedType resolvedType = null;

            if( doxygenType is DoxygenFacadeType )
            {
                DoxygenFacadeType doxygenFacadeType = doxygenType as DoxygenFacadeType;
                resolvedType = ResolveFacadeType( doxygenFacadeType, allProjectedAndAssemblyTypes );
                if( resolvedType == null )
                {
                    resolvedType = ResolveFacadeType( doxygenFacadeType );
                }
            }
            else if( doxygenType.IsInProjectedType )
            {
                resolvedType = doxygenType.ParentProjectedType;
            }
            else
            {
                // TBD: Use DefinedTypeComparer.
                resolvedType = allProjectedAndAssemblyTypes.FirstOrDefault( t => t.FullName == doxygenType.FullName );
                if( resolvedType == null )
                {
                    resolvedType = doxygenType;
                }
            }

            return resolvedType;
        }

        /// <summary>
        /// Finds the specified facade type in the provided canonical list of types.
        /// </summary>
        /// <param name="facadeType">The facade type to resolve.</param>
        /// <param name="allTypes">The canonical list of types to search for <paramref name="facadeType"/> in.</param>
        /// <returns>The resolved type, if successful; otherwise, null.</returns>
        /// <remarks>
        /// <para>There's a lot of heuristic goo in here, and resolving more facade types 
        /// will require a lot more. Currently, the <see cref="ResolveFacadeType"/> method 
        /// tries to match on type name, and failing that, it tries other things. 
        /// </para>
        /// <para>If <paramref name="facadeType"/> is a generic type, the <see cref="ResolveFacadeType"/> 
        /// method tries to create a live generic type by using the <see cref="Type.GetType"/> and 
        /// <see cref="Type.MakeGenericType"/> methods. A lot more could be done to make this more
        /// effective.
        /// </para>
        /// <para>Also, this implementation includes a hack for types in the DUSK namespace, which
        /// has Unity wrappers for the Analog platform APIs.
        /// </para>
        /// </remarks>
        public static DefinedType ResolveFacadeType( DefinedType facadeType, List<DefinedType> allTypes )
        {
            DefinedType resolvedType = null;

            // TBD: Use DefinedTypeComparer.
            DefinedType candidateType = allTypes.FirstOrDefault( t => t.Name == facadeType.Name );
            if( candidateType != null )
            {
                resolvedType = candidateType;
            }
            else
            {
                // TBD: look for fooImpl that corresponds with foo facade type
                if( IsGenericType( facadeType.Name ) )
                {
                    string genericTypeName = TypeFactory.GenericTypeNames.FirstOrDefault( t =>
                        facadeType.Name.StartsWith( t ) );
                    if( genericTypeName != null )
                    {
                        string s = facadeType.Name.Replace( genericTypeName, String.Empty );
                        s = s.Replace( "<", String.Empty );
                        s = s.Replace( ">", String.Empty );

                        candidateType = allTypes.FirstOrDefault( t => t.Name == s );
                        if( candidateType != null )
                        {
                            if( candidateType is AssemblyType || candidateType is ProjectedType )
                            {
                                string typeName = String.Format(
                                    "{0}<{1}>",
                                    genericTypeName,
                                    candidateType.FullName );

                                string fullGenericTypeName = "System.Collections.Generic." + genericTypeName + "`1";

                                //var listType = typeof( List<> );
                                var listType = Type.GetType( fullGenericTypeName );
                                if( listType != null )
                                {
                                    Type type = candidateType is AssemblyType ?
                                        ( (AssemblyType)candidateType ).UnderlyingType.UnderlyingType :
                                        ( (ProjectedType)candidateType ).AssemblyType.UnderlyingType.UnderlyingType;
                                    Type concreteType = listType.MakeGenericType( type );

                                    ObservableType ot = new ObservableType( concreteType );
                                    AssemblyType at = new AssemblyType( ot ); // TypeFactory.CreateAssemblyType( ot ); // new AssemblyType( ot );

                                    resolvedType = at;

                                    //var genericArgs = prop.PropertyType.GetGenericArguments();
                                    //var concreteType = listType.MakeGenericType( genericArgs );
                                }
                            }
                        }
                    }
                }
                else
                {
                    // Hack for DUSK
                    string nameWithoutDUSK = facadeType.Name.Replace( "DUSK.", String.Empty );
                    candidateType = allTypes.FirstOrDefault( t => t.FullName == nameWithoutDUSK );
                    if( candidateType != null )
                    {
                        resolvedType = candidateType;
                    }
                }
            }

            return resolvedType;
        }

        /// <summary>
        /// Finds the specified facade type in the collection of known types
        /// provided by the <see cref="TypeFactory"/>.
        /// </summary>
        /// <param name="facadeType">The facade type to resolve.</param>
        /// <returns>The resolved type, if successful; otherwise, null.</returns>
        /// <remarks>
        /// <para>If type resolution fails against the canonical list of known types,
        /// try calling this method, which looks for <paramref name="facadeType"/> in 
        /// the <see cref="TypeFactory.KnownTypes"/> collection. 
        /// </para>
        /// </remarks>
        public static DefinedType ResolveFacadeType( DefinedType facadeType )
        {
            DefinedType resolvedType = null;

            // Get a list of all of the known DoxygenTypes from the TypeFactory.
            var allDoxygenTypes = TypeFactory.KnownTypes.Values.ToList();

            // Filter the list to remove all of the facade types.
            var nonFacadeTypes = allDoxygenTypes.Where( t => !( t is DoxygenFacadeType ) );

            // Try to resolve against the filtered list of DoxygenTypes.
            resolvedType = ResolveFacadeType( facadeType, nonFacadeTypes.ToList() );

            return resolvedType;
        }

        public static bool IsGenericType( string typeName )
        {
            return ( typeName.Contains( genericCharacterLt ) );
        }

        public static void TraverseTypeGraph(
            List<DoxType> types,
            Dictionary<string, DoxType> doxTypeDictionary )
        {
            foreach( DoxType type in types )
            {
                string key = type.id != null ? type.id : type.FullName;
                if( !doxTypeDictionary.ContainsKey( key ) )
                {
                    doxTypeDictionary.Add( key, type );
                }

                TraverseTypeGraph( type.BaseTypes, doxTypeDictionary );
                //TraverseTypeGraph( type.DerivedTypes, doxTypeDictionary );
                TraverseTypeGraph( type.ChildTypes, doxTypeDictionary );

                // TBD: 
                //TraverseTypeGraph( type.GenericParameterTypes, flatList )
            }
        }

        public static List<DoxType> GetFlatList( List<DoxType> allTypes )
        {
            Dictionary<string, DoxType> typeDictionary = new Dictionary<string, DoxType>();
            //List<DoxType> flatList = new List<DoxType>();

            TraverseTypeGraph( allTypes, typeDictionary );

            List<DoxType> flatList = typeDictionary.Values.Select( d => d ).ToList();

            var orderedFlatList = flatList.OrderBy( t => t.FullName );

            DoxTypeComparer comparer = new DoxTypeComparer();
            return orderedFlatList.Distinct( comparer ).ToList();
        }

        /// <summary>
        /// Traverses a type graph and adds every referenced type to a flat list.
        /// </summary>
        /// <param name="allTypes">The collecction of types to flatten.</param>
        /// <returns>A list of all of the types in the <paramref name="allTypes"/> graph.</returns>
        /// <remarks>
        /// <para>A frequent source of confusion in GTU is the structure of the type graph.
        /// This is because, in general, a type list doesn't show every type in the type graph.
        /// Usually, there are many other types buried in the graph, including child types, 
        /// base types, derived types, member types in fields, return values, and parameters. 
        /// </para>
        /// <para>The <see cref="GetFlatList"/> method traverses the entire type graph in 
        /// <param name="allTypes">, and inspects each type for its related types in
        /// the following properties: 
        /// <list type="bullet">
        /// <item><see cref="DefinedType.BaseTypes"/></item>
        /// <item><see cref="DefinedType.DerivedTypes"/></item>
        /// <item><see cref="DefinedType.ChildTypes"/></item>
        /// <item><see cref="DefinedType.Members"/>Including <see cref="DefinedMember.Type"/> and 
        /// <see cref="DefinedParameter.Type"/>.</item>
        /// <item><see cref="DefinedType.GenericTypes"/>TBD</item>
        /// </list>
        /// </para>
        /// <para>All of these types are added to the flat list. This list is useful for
        /// creating a canonical list of all types. The canonical list is used to resolve 
        /// types and to create links, for example, in the <see cref="GetAnchor"/> and 
        /// <see cref="ResolveTypes"/> methods. For this reason, facade types are excluded 
        /// from the flat list.
        /// </para>
        /// </remarks>
        public static List<DefinedType> GetFlatList( List<DefinedType> allTypes )
        {
            List<DefinedType> flatList = new List<DefinedType>();

            foreach( DefinedType type in allTypes )
            {
                try
                {
                    if( type is DoxygenFacadeType )
                    {
                        continue;
                    }

                    flatList.Add( type );

                    // TBD: Can't we just call AddRange?
                    foreach( DefinedType baseType in type.BaseTypes )
                    {
                        flatList.Add( baseType );
                    }

                    foreach( DefinedType derivedType in type.DerivedTypes )
                    {
                        flatList.Add( derivedType );
                    }

                    foreach( DefinedType childType in type.ChildTypes )
                    {
                        flatList.Add( childType );
                    }

                    if( type.Members != null && type.Members.Count > 0 )
                    {
                        foreach( DefinedMember member in type.Members )
                        {
                            flatList.Add( member.Type );

                            if( member.HasParameters )
                            {
                                foreach( DefinedParameter param in member.Parameters )
                                {
                                    flatList.Add( param.Type );
                                }
                            }
                        }
                    }
                }
                catch( Exception ex )
                {
                    Debug.WriteLine( ex.ToString() );
                    throw;
                }
            }

            var orderedFlatList = flatList.Distinct().OrderBy( t => t.FullName );
            return orderedFlatList.ToList();
        }



        #endregion

        ///////////////////////////////////////////////////////////////////////
        #region Name parsing strings

        public const string genericCharacterLt = "<";
        public const string genericCharacterGt = ">";
        public const string genericCharacterLtEntity = "&lt;";
        public const string genericCharacterGtEntity = "&gt;";
        public const char genericCharacter = '`';
        public const char referenceCharacter = '&';
        public const string referenceCharacterString = "&";
        public const string referenceCharacterEntity = ampEntityString;
        public const char pointerCharacter = '*';
        public const char separatorCharacter = '_';
        public const char pointerCharacterChar = '*';
        public const string pointerCharacterString = "*";
        public const string pointerPointerCharacterString = "**";
        public const string arrayCharacters = "[]";
        public const string arrayRankWarningFormat = "WARNING: Parameter {0} has array rank > 2. Markup does not match ABI.";
        public const string ampEntityString = "&amp;";
        public const string classString = "class";
        public const string typenameString = "typename";
        public const string typenameStringWithTrailingSpace = "typename ";
        public const string constString = "const";
        public const string constStringWithTrailingSpace = "const ";
        public const string readonlyString = "readonly";
        public const string outString = "[out]";
        public const string outStringWithTrailingSpace = "[out] ";
        public const string inString = "[in]";
        public const string inStringWithTrailingSpace = "[in] ";
        public const string outStringUnderscore = "_Out_";
        public const string outStringUnderscoreWithTrailingSpace = "_Out_ ";
        public const string inStringUnderscore = "_In_";
        public const string inStringUnderscoreWithTrailingSpace = "_In_ ";
        public const string unsafeString = "unsafe";
        public const string eventTokenString = "EventRegistrationToken";
        public const string thisString = "this";
        public const string thisStringWithTrailingSpace = "this ";
        public const string refString = "ref";
        public const string refStringWithTrailingSpace = "ref ";
        public const string runtimeclassChar = "^";
        public const string eventArgsString = "EventArgs";
        public const string comOutPtrString = "_COM_Outptr_";
        public const string comOutPtrMayBeNullString = "_COM_Outptr_result_maybenull_";
        public const string iinspectablerequiresString = "IInspectable requires";

        public const string inreadsString = "_In_reads_(";
        public const string outwritesallString = "_Out_writes_all_(";
       
        #endregion

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
