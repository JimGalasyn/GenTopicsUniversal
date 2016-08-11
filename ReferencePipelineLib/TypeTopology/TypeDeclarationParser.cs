using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology
{
    /// <summary>
    /// Parses a type declaration from the C family of languages.
    /// </summary>
    public class TypeDeclarationParser
    {
        /// <summary>
        /// Parses the specified string for a type declaration.
        /// </summary>
        /// <param name="typeDeclaration">The type declaration string to parse. 
        /// This string must be from the C family of languages.</param>
        /// <returns>The parse results.</returns>
        /// <remarks><para>Objective-C types may fail, for example, "NSObject".</para>
        /// <para>The <see cref="ParseTypeDeclaration"/> method first scrubs noise 
        /// from the type declaration string, like "IInspectable requires", then
        /// it searches for various keywords and decorations to detect details 
        /// of the context in which the type appears.</para>
        /// <para>Detected decorations and keywords are removed, until the remaining
        /// string (hopefully) contains only the type name and any generic/template
        /// parameters that are attached to it.
        /// </para>
        /// <para>Some of this filtering could be accomplished by using Doxygen's
        /// preprocessor feature, but GTU shouldn't assume that an arbitrary
        /// doxygen config file has been hand-tuned to remove noise.</para>
        /// </remarks>
        public static TypeDeclarationParseResults ParseTypeDeclaration( string typeDeclaration )
        {
            // TBD: handle case, like out vs. OUT
            string scrubbedTypeName = String.Copy( typeDeclaration );

            TypeDeclarationParseResults parseResults = new TypeDeclarationParseResults();
            parseResults.RawTypeDeclaration = typeDeclaration;

            if( typeDeclaration.Contains( eventArgsString ) )
            {
                parseResults.IsEventArgs = true;
            }

            // "IInspectable requires"
            if( typeDeclaration.Contains( iinspectablerequiresString ) )
            {
                scrubbedTypeName = scrubbedTypeName.Replace(
                    iinspectablerequiresString,
                    String.Empty );
            }

            // Remove "typename"
            if( typeDeclaration.Contains( typenameString ) )
            {
                scrubbedTypeName = scrubbedTypeName.Replace(
                    typenameString,
                    String.Empty );

                parseResults.IsGenericParam = true;
            }

            // Remove "readonly"
            if( typeDeclaration.Contains( readonlyString ) )
            {
                scrubbedTypeName = scrubbedTypeName.Replace(
                    readonlyString,
                    String.Empty );

                parseResults.IsReadOnly = true;
            }

            // Remove "const"
            if( typeDeclaration.Contains( constString ) )
            {
                scrubbedTypeName = scrubbedTypeName.Replace(
                    constString,
                    String.Empty );

                parseResults.IsConst = true;
            }

            // Remove "_COM_Outptr_result_maybenull_"
            if( typeDeclaration.Contains( comOutPtrMayBeNullString ) )
            {
                scrubbedTypeName = scrubbedTypeName.Replace(
                    comOutPtrMayBeNullString,
                    String.Empty );

                parseResults.IsComPtr = true;
                parseResults.IsOutParam = true;
            }

            // Remove "_COM_Outptr_"
            if( typeDeclaration.Contains( comOutPtrString ) )
            {
                scrubbedTypeName = scrubbedTypeName.Replace(
                    comOutPtrString,
                    String.Empty );

                parseResults.IsComPtr = true;
                parseResults.IsOutParam = true;
            }

            // Remove _In_reads_(*)
            if( typeDeclaration.Contains( inreadsString ) )
            {
                scrubbedTypeName = scrubbedTypeName.Replace(
                    inreadsString,
                    String.Empty );

                // Assume this is always at the start of the declaration.
                int index = scrubbedTypeName.IndexOf( ')' );
                scrubbedTypeName = scrubbedTypeName.Remove( 0, index + 1 );

                parseResults.IsInParam = true;
            }

            // Remove _Out_writes_all_(*)
            if( typeDeclaration.Contains( outwritesallString ) )
            {
                scrubbedTypeName = scrubbedTypeName.Replace(
                    outwritesallString,
                    String.Empty );

                // Assume this is always at the start of the declaration.
                int index = scrubbedTypeName.IndexOf( ')' );
                scrubbedTypeName = scrubbedTypeName.Remove( 0, index + 1 );

                parseResults.IsOutParam = true;
            }

            // Remove "unsafe"
            if( typeDeclaration.Contains( "unsafe" ) )
            {
                scrubbedTypeName = scrubbedTypeName.Replace(
                    "unsafe",
                    String.Empty );

                //parseResults.IsUnsafe = true;
            }

            // Remove "_In_"
            if( typeDeclaration.Contains( inStringUnderscore ) )
            {
                scrubbedTypeName = scrubbedTypeName.Replace(
                    inStringUnderscore,
                    String.Empty );

                parseResults.IsInParam = true;
            }

            // Remove "[in]"
            if( typeDeclaration.Contains( inString ) )
            {
                scrubbedTypeName = scrubbedTypeName.Replace(
                    inString,
                    String.Empty );

                parseResults.IsInParam = true;
            }

            // Remove "[out]"
            if( typeDeclaration.Contains( outString ) )
            {
                scrubbedTypeName = scrubbedTypeName.Replace(
                outString,
                String.Empty );

                parseResults.IsOutParam = true;
            }

            // Remove "_Out_"
            if( typeDeclaration.Contains( outStringUnderscore ) )
            {
                scrubbedTypeName = scrubbedTypeName.Replace(
                outStringUnderscore,
                String.Empty );

                parseResults.IsOutParam = true;
            }

            // Remove "this"
            if( typeDeclaration.Contains( thisString ) )
            {
                scrubbedTypeName = scrubbedTypeName.Replace(
                    thisString,
                    String.Empty );
            }

            // Remove "ref"
            if( typeDeclaration.Contains( refString ) )
            {
                scrubbedTypeName = scrubbedTypeName.Replace(
                    refString,
                    String.Empty );

                parseResults.IsReference = true;
            }

            // Remove "&amp;"
            if( typeDeclaration.Contains( ampEntityString ) )
            {
                scrubbedTypeName = scrubbedTypeName.Replace(
                    ampEntityString,
                    String.Empty );

                parseResults.IsReference = true;
            }

            // Deal with Doxygen munging 'System.IDisposable'.
            scrubbedTypeName = FixupMungedSystemTypeName( scrubbedTypeName );

            if( !ParseTypeName( scrubbedTypeName, parseResults ) )
            {
                string msg = String.Format( "Failed to parse: {0} | {1}", scrubbedTypeName, typeDeclaration );
                Debug.WriteLine( msg );
            }

            return parseResults;
        }

        public static bool ParseTypeName( string typeDeclaration, TypeDeclarationParseResults parseResults )
        {
            string typeNameCopy = string.Copy( typeDeclaration );

            // Convert from C++ selector syntax to C# syntax.
            typeNameCopy = typeNameCopy.Replace( "::", "." );

            // Remove spaces.
            typeNameCopy = typeNameCopy.Replace( " ", String.Empty );

            // Apply the regular expression to the type declaration.
            var match = TypeNameRegex.Match( typeNameCopy );

            // If nothing matched, exit.
            if( !match.Success )
            {
                parseResults.FullName = typeNameCopy;

                string msg = String.Format( "Failed to match type name: {0} | raw declaration: {1}", typeNameCopy, typeDeclaration );
                Debug.WriteLine( msg );
                return false;
            }

            // Examine the results from the regular expression.
            var typeName = match.Groups["TypeName"].Value;
            var genericParams = match.Groups["GenericParams"].Value;
            bool isArray = !string.IsNullOrWhiteSpace( match.Groups["Array"].Value );
            bool isPointer = !string.IsNullOrWhiteSpace( match.Groups["Pointer"].Value );
            bool isReference = !string.IsNullOrWhiteSpace( match.Groups["Reference"].Value );
            bool isRuntimeReference = !string.IsNullOrWhiteSpace( match.Groups["RuntimeReference"].Value );
            bool isNullable = !string.IsNullOrWhiteSpace( match.Groups["Nullable"].Value );

            // It's possible that the type-name match failed, but 
            // other parts of the regex matched.
            if( String.IsNullOrEmpty( typeName ) )
            {
                // regex match, but no type name, e.g., <NSObject>
                parseResults.FullName = typeNameCopy;
            }
            else
            {
                if( String.IsNullOrEmpty( genericParams ) )
                {
                    parseResults.FullName = typeName;
                }
                else
                {
                    parseResults.FullName = String.Format( "{0}<{1}>", typeName, genericParams );
                }
            }

            parseResults.IsArray = isArray;
            parseResults.IsNullable = isNullable;
            parseResults.IsReference = isReference;
            parseResults.IsRuntimeClassReference = isRuntimeReference;

            parseResults.IsPointer = isPointer;
            if( parseResults.IsPointer )
            {
                parseResults.PointerDepth =
                    match.Groups["Pointer"].Value == pointerPointerCharacterString ? 2 : 1;
            }

            // Are there generic parameters?
            if( !string.IsNullOrWhiteSpace( genericParams ) )
            {
                parseResults.IsGeneric = true;
                parseResults.GenericParameterTypes = new List<TypeDeclarationParseResults>();

                // Split each type by the comma character.
                var genericParamNames = SplitByComma( genericParams );

                // Iterate through all inner type names and attempt to parse them recursively.
                foreach( string genericParamName in genericParamNames )
                {
                    TypeDeclarationParseResults genericParamParseResults = new TypeDeclarationParseResults();
                    genericParamParseResults.IsGenericParam = true;

                    var trimmedGenericParamName = genericParamName.Trim();
                    var success = ParseTypeName( trimmedGenericParamName, genericParamParseResults );

                    // Parse fails if parsing the embedded generic params declaration fails.
                    if( !success )
                    {
                        return false;
                    }

                    parseResults.GenericParameterTypes.Add( genericParamParseResults );
                }
            }

            return true;
        }

        /// <summary>
        /// Breaks up a string by the comma ',' delimiter.
        /// </summary>
        /// <param name="value">the string to parse.</param>
        /// <returns>A collection of substrings.</returns>
        /// <remarks>
        /// <para>From: http://stackoverflow.com/questions/20532691/how-to-parse-c-sharp-generic-type-names
        /// </para></remarks>
        private static IEnumerable<string> SplitByComma( string value )
        {
            var strings = new List<string>();
            var sb = new StringBuilder();
            var level = 0;

            foreach( var c in value )
            {
                if( c == ',' && level == 0 )
                {
                    strings.Add( sb.ToString() );
                    sb.Clear();
                }
                else
                {
                    sb.Append( c );
                }

                if( c == '<' )
                {
                    level++;
                }

                if( c == '>' )
                {
                    level--;
                }
            }

            strings.Add( sb.ToString() );

            return strings;
        }

        /// <summary>
        /// A hack to work around Doxygen's occasional munging of type names, 
        /// like System.IDisposable.
        /// </summary>
        /// <param name="mungedSystemTypeName">The type name to fix.</param>
        /// <returns>The fixed string. For example, if <paramref name="mungedSystemTypeName"/> is
        /// 'SystemIDisposable', as happens with the Xtools.NetworkOutMessage class, the
        /// returned string is 'System.IDisposable'.</returns>
        private static string FixupMungedSystemTypeName( string mungedSystemTypeName )
        {
            string fixedTypeName = mungedSystemTypeName;

            if( mungedSystemTypeName.StartsWith( "System" ) &&
                mungedSystemTypeName != "System" &&
                !mungedSystemTypeName.Contains( '.' ) )
            {
                fixedTypeName = mungedSystemTypeName.Replace( "System", "System." );
            }

            return fixedTypeName;
        }

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
        public const string constString = "const";
        public const string readonlyString = "readonly";
        public const string outString = "[out]";
        public const string inString = "[in]";
        public const string outStringUnderscore = "_Out_";
        public const string inStringUnderscore = "_In_";
        public const string inStringUnderscoreWithTrailingSpace = "_In_ ";
        public const string unsafeString = "unsafe";
        public const string eventTokenString = "EventRegistrationToken";
        public const string thisString = "this";
        public const string refString = "ref";
        public const string runtimeclassChar = "^";
        public const string eventArgsString = "EventArgs";
        public const string comOutPtrString = "_COM_Outptr_";
        public const string comOutPtrMayBeNullString = "_COM_Outptr_result_maybenull_";
        public const string iinspectablerequiresString = "IInspectable requires";
        public const string inreadsString = "_In_reads_(";
        public const string outwritesallString = "_Out_writes_all_(";

        private const string typeNameRegexString = @"^"+ // Start of line
            @"(?<TypeName>[a-zA-Z0-9_\-\.@]+)?" + // Match full name
            @"(?<Pointer>[\*]+)?"+ // Match pointer (C/C++ syntax) TBD: reference
            @"(?<Nullable>[\?])?"+ // Match nullable (C# syntax)
            @"(<(?<GenericParams>[a-zA-Z0-9_\-\.\*\&,\<\>\s\[\]]+)>)?"+ // Match template/generic syntax (C# and C++)
            @"(?<Pointer>[\*]+)?"+ // Match embedded pointer (C++)
            @"(?<Reference>[\&])?"+ // Match embedded reference (C++)
            @"(?<RuntimeReference>[\^])?"+ // Match RuntimeReference (C++ WinRT reference)
            @"(?<Array>(\[\]|\[,\]))?"+ // Match array (C# and C/C++)
            @"$"; // End of line

        static readonly Regex TypeNameRegex = new Regex( typeNameRegexString, RegexOptions.Compiled );

        #endregion
    }
}
