using OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology
{
    /// <summary>
    /// Provides a collection of <see cref="PrimitiveType"/> instances, which
    /// is added to the <see cref="TypeFactory.KnownTypes"/> collection.
    /// </summary>
    public class PrimitiveTypes
    {   
        /// <summary>
        /// Gets a collection of <see cref="PrimitiveType"/> instances.
        /// </summary>
        public static Dictionary<string, DefinedType> KnownPrimitiveTypes
        {
            get
            {
                if( _knownPrimitiveTypes == null )
                {
                    _knownPrimitiveTypes = new Dictionary<string, DefinedType>();
                    _knownPrimitiveTypes.Add( "bool", PrimitiveTypes.Boolean );
                    _knownPrimitiveTypes.Add( "boolean", PrimitiveTypes.Boolean );
                    _knownPrimitiveTypes.Add( "byte", PrimitiveTypes.Byte );
                    _knownPrimitiveTypes.Add( "char", PrimitiveTypes.Char );
                    _knownPrimitiveTypes.Add( "decimal", PrimitiveTypes.Decimal );
                    _knownPrimitiveTypes.Add( "double", PrimitiveTypes.Double );
                    _knownPrimitiveTypes.Add( "float", PrimitiveTypes.Float );
                    _knownPrimitiveTypes.Add( "int", PrimitiveTypes.Int );
                    _knownPrimitiveTypes.Add( "int16", PrimitiveTypes.Int16 );
                    _knownPrimitiveTypes.Add( "int32", PrimitiveTypes.Int32 );
                    _knownPrimitiveTypes.Add( "int64", PrimitiveTypes.Int64 );
                    _knownPrimitiveTypes.Add( "__int16", PrimitiveTypes.Int16 );
                    _knownPrimitiveTypes.Add( "__int32", PrimitiveTypes.Int32 );
                    _knownPrimitiveTypes.Add( "__int64", PrimitiveTypes.Int64 );
                    _knownPrimitiveTypes.Add( "sbyte", PrimitiveTypes.SByte );
                    _knownPrimitiveTypes.Add( "single", PrimitiveTypes.Single );
                    _knownPrimitiveTypes.Add( "string", PrimitiveTypes.String );
                    _knownPrimitiveTypes.Add( "hstring", PrimitiveTypes.String );
                    _knownPrimitiveTypes.Add( "uint", PrimitiveTypes.UInt16 );
                    _knownPrimitiveTypes.Add( "uint16", PrimitiveTypes.UInt16 );
                    _knownPrimitiveTypes.Add( "uint32", PrimitiveTypes.UInt32 );
                    _knownPrimitiveTypes.Add( "uint64", PrimitiveTypes.UInt64 );
                    _knownPrimitiveTypes.Add( "__uint16", PrimitiveTypes.UInt16 );
                    _knownPrimitiveTypes.Add( "__uint32", PrimitiveTypes.UInt32 );
                    _knownPrimitiveTypes.Add( "__uint64", PrimitiveTypes.UInt64 );
                    _knownPrimitiveTypes.Add( "unsigned int", PrimitiveTypes.UInt16 );
                    _knownPrimitiveTypes.Add( "guid", PrimitiveTypes.Guid );
                    _knownPrimitiveTypes.Add( "void", PrimitiveTypes.Void );

                    // Frequently declared generic typenames
                    _knownPrimitiveTypes.Add( "E", PrimitiveTypes.E );
                    _knownPrimitiveTypes.Add( "T", PrimitiveTypes.T );
                    _knownPrimitiveTypes.Add( "U", PrimitiveTypes.U );
                    _knownPrimitiveTypes.Add( "typename E", PrimitiveTypes.E );
                    _knownPrimitiveTypes.Add( "typename T", PrimitiveTypes.T );
                    _knownPrimitiveTypes.Add( "typename U", PrimitiveTypes.U );

                    // C++ types
                    _knownPrimitiveTypes.Add( "HRESULT", PrimitiveTypes.Int );
                    _knownPrimitiveTypes.Add( "signed __int64", PrimitiveTypes.Int64 );
                    _knownPrimitiveTypes.Add( "signed __int32", PrimitiveTypes.Int32 );
                    _knownPrimitiveTypes.Add( "unsigned __int64", PrimitiveTypes.UInt64 );
                    _knownPrimitiveTypes.Add( "unsigned __int32", PrimitiveTypes.UInt32 );
                    // 
                }

                return _knownPrimitiveTypes;
            }
        }

        /// <summary>
        /// Gets a <see cref="PrimitiveType"/> that represents the <code>void</code> type.
        /// </summary>
        public static PrimitiveType Void
        {
            get
            {
                if( _Void == null )
                {
                    _Void = new PrimitiveType( voidFullName );
                }

                return _Void;
            }
        }

        /// <summary>
        /// Gets a <see cref="PrimitiveType"/> that represents the <code>int</code> type.
        /// </summary>
        public static PrimitiveType Int
        {
            get
            {
                return PrimitiveTypes.Int16;
            }
        }

        /// <summary>
        /// Gets a <see cref="PrimitiveType"/> that represents the <code>Int16</code> type.
        /// </summary>
        public static PrimitiveType Int16
        {
            get
            {
                if( _Int16 == null )
                {
                    _Int16 = new PrimitiveType( int16FullName );
                }

                return _Int16;
            }
        }

        /// <summary>
        /// Gets a <see cref="PrimitiveType"/> that represents the <code>UInt16</code> type.
        /// </summary>
        public static PrimitiveType UInt16
        {
            get
            {
                if( _UInt16 == null )
                {
                    _UInt16 = new PrimitiveType( uint16FullName );
                }

                return _UInt16;
            }
        }

        /// <summary>
        /// Gets a <see cref="PrimitiveType"/> that represents the <code>Int32</code> type.
        /// </summary>
        public static PrimitiveType Int32
        {
            get
            {
                if( _Int32 == null )
                {
                    _Int32 = new PrimitiveType( int32FullName );
                }

                return _Int32;
            }
        }

        /// <summary>
        /// Gets a <see cref="PrimitiveType"/> that represents the <code>UInt32</code> type.
        /// </summary>
        public static PrimitiveType UInt32
        {
            get
            {
                if( _UInt32 == null )
                {
                    _UInt32 = new PrimitiveType( uint32FullName );
                }

                return _UInt32;
            }
        }

        /// <summary>
        /// Gets a <see cref="PrimitiveType"/> that represents the <code>Int64</code> type.
        /// </summary>
        public static PrimitiveType Int64
        {
            get
            {
                if( _Int64 == null )
                {
                    _Int64 = new PrimitiveType( int64FullName );
                }

                return _Int64;
            }
        }

        /// <summary>
        /// Gets a <see cref="PrimitiveType"/> that represents the <code>UInt64</code> type.
        /// </summary>
        public static PrimitiveType UInt64
        {
            get
            {
                if( _UInt64 == null )
                {
                    _UInt64 = new PrimitiveType( uint64FullName );
                }

                return _UInt64;
            }
        }

        /// <summary>
        /// Gets a <see cref="PrimitiveType"/> that represents the <code>Float</code> type.
        /// </summary>
        public static PrimitiveType Float
        {
            get
            {
                return PrimitiveTypes.Single;
            }
        }

        /// <summary>
        /// Gets a <see cref="PrimitiveType"/> that represents the <code>Single</code> type.
        /// </summary>
        public static PrimitiveType Single
        {
            get
            {
                if( _Single == null )
                {
                    _Single = new PrimitiveType( singleFullName );
                }

                return _Single;
            }
        }

        /// <summary>
        /// Gets a <see cref="PrimitiveType"/> that represents the <code>Double</code> type.
        /// </summary>
        public static PrimitiveType Double
        {
            get
            {
                if( _Double == null )
                {
                    _Double = new PrimitiveType( doubleFullName );
                }

                return _Double;
            }
        }

        /// <summary>
        /// Gets a <see cref="PrimitiveType"/> that represents the <code>Decimal</code> type.
        /// </summary>
        public static PrimitiveType Decimal
        {
            get
            {
                if( _Decimal == null )
                {
                    _Decimal = new PrimitiveType( decimalFullName );
                }

                return _Decimal;
            }
        }

        /// <summary>
        /// Gets a <see cref="PrimitiveType"/> that represents the <code>Boolean</code> type.
        /// </summary>
        public static PrimitiveType Boolean
        {
            get
            {
                if( _Boolean == null )
                {
                    _Boolean = new PrimitiveType( booleanFullName );
                }

                return _Boolean;
            }
        }

        /// <summary>
        /// Gets a <see cref="PrimitiveType"/> that represents the <code>Byte</code> type.
        /// </summary>
        public static PrimitiveType Byte
        {
            get
            {
                if( _Byte == null )
                {
                    _Byte = new PrimitiveType( byteFullName );
                }

                return _Byte;
            }
        }

        /// <summary>
        /// Gets a <see cref="PrimitiveType"/> that represents the <code>SByte</code> type.
        /// </summary>
        public static PrimitiveType SByte
        {
            get
            {
                if( _SByte == null )
                {
                    _SByte = new PrimitiveType( sbyteFullName );
                }

                return _SByte;
            }
        }

        /// <summary>
        /// Gets a <see cref="PrimitiveType"/> that represents the <code>Char</code> type.
        /// </summary>
        public static PrimitiveType Char
        {
            get
            {
                if( _Char == null )
                {
                    _Char = new PrimitiveType( charFullName );
                }

                return _Char;
            }
        }

        /// <summary>
        /// Gets a <see cref="PrimitiveType"/> that represents the <code>String</code> type.
        /// </summary>
        public static PrimitiveType String
        {
            get
            {
                if( _String == null )
                {
                    _String = new PrimitiveType( stringFullName );
                }

                return _String;
            }
        }

        /// <summary>
        /// Gets a <see cref="PrimitiveType"/> that represents the <code>Guid</code> type.
        /// </summary>
        public static PrimitiveType Guid
        {
            get
            {
                if( _Guid == null )
                {
                    _Guid = new PrimitiveType( guidFullName );
                }

                return _Guid;
            }
        }

        /// <summary>
        /// Gets a facade type that represents the a generic/template 
        /// parameter type named <code>E</code>.
        /// </summary>
        public static GenericParameterType E
        {
            get
            {
                if( _E == null )
                {
                    _E = new GenericParameterType( genericParamEName );
                }

                return _E;
            }
        }

        /// <summary>
        /// Gets a facade type that represents the a generic/template 
        /// parameter type named <code>T</code>.
        /// </summary>
        public static GenericParameterType T
        {
            get
            {
                if( _T == null )
                {
                    _T = new GenericParameterType( genericParamTName );
                }

                return _T;
            }
        }

        /// <summary>
        /// Gets a facade type that represents the a generic/template 
        /// parameter type named <code>U</code>.
        /// </summary>
        public static GenericParameterType U
        {
            get
            {
                if( _U == null )
                {
                    _U = new GenericParameterType( genericParamUName );
                }

                return _U;
            }
        }


        private static Dictionary<string, DefinedType> _knownPrimitiveTypes;

        static PrimitiveType _Void;

        static PrimitiveType _Boolean;
        static PrimitiveType _Byte;
        static PrimitiveType _SByte;

        static PrimitiveType _Int16;
        static PrimitiveType _UInt16;
        static PrimitiveType _Int32;
        static PrimitiveType _UInt32;
        static PrimitiveType _Int64;
        static PrimitiveType _UInt64;

        static PrimitiveType _Single;
        static PrimitiveType _Double;
        static PrimitiveType _Decimal;

        static PrimitiveType _Char;
        static PrimitiveType _String;

        static PrimitiveType _Guid;

        static GenericParameterType _E;
        static GenericParameterType _T;
        static GenericParameterType _U;

        static string genericParamEName = "E";
        static string genericParamTName = "T";
        static string genericParamUName = "U";

        // Built-In Types Table (C# Reference)
        // http://msdn.microsoft.com/en-us/library/ya5y69ds.aspx
        static string voidFullName = Type.GetType( "System.Void" ).AssemblyQualifiedName;
        static string booleanFullName = Type.GetType( "System.Boolean" ).AssemblyQualifiedName;
        
        static string byteFullName = Type.GetType( "System.Byte" ).AssemblyQualifiedName;
        static string sbyteFullName = Type.GetType( "System.SByte" ).AssemblyQualifiedName;

        static string int16FullName = Type.GetType( "System.Int16" ).AssemblyQualifiedName;
        static string int32FullName = Type.GetType( "System.Int32" ).AssemblyQualifiedName;
        static string uint16FullName = Type.GetType( "System.UInt16" ).AssemblyQualifiedName;
        static string uint32FullName = Type.GetType( "System.UInt32" ).AssemblyQualifiedName;
        static string int64FullName = Type.GetType( "System.Int64" ).AssemblyQualifiedName;
        static string uint64FullName = Type.GetType( "System.UInt64" ).AssemblyQualifiedName;

        static string longFullName = Type.GetType( "System.Int64" ).AssemblyQualifiedName;
        static string ulongFullName = Type.GetType( "System.UInt64" ).AssemblyQualifiedName;
        static string shortFullName = Type.GetType( "System.Int16" ).AssemblyQualifiedName;
        static string ushortFullName = Type.GetType( "System.UInt16" ).AssemblyQualifiedName;

        static string singleFullName = Type.GetType( "System.Single" ).AssemblyQualifiedName;
        static string doubleFullName = Type.GetType( "System.Double" ).AssemblyQualifiedName;
        static string decimalFullName = Type.GetType( "System.Decimal" ).AssemblyQualifiedName;

        static string charFullName = Type.GetType( "System.Char" ).AssemblyQualifiedName;
        static string stringFullName = Type.GetType( "System.String" ).AssemblyQualifiedName;

        static string guidFullName = Type.GetType( "System.Guid" ).AssemblyQualifiedName;
    }
}
