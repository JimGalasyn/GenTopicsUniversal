using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology
{
    /// <summary>
    /// Provides a collection of <see cref="PrimitiveType"/> instances that
    /// represent system types.
    /// </summary>
    /// <remarks><para>The <see cref="KnownSystemTypes"/> collection
    /// is added to the <see cref="TypeFactory.KnownTypes"/> collection.
    /// </para>
    /// </remarks>
    public class SystemTypes
    {
        /// <summary>
        /// Gets a collection of <see cref="PrimitiveType"/> instances that
        /// represent system types.
        /// </summary>
        public static Dictionary<string, DefinedType> KnownSystemTypes
        {
            get
            {
                if( _knownSystemTypes == null )
                {
                    _knownSystemTypes = new Dictionary<string, DefinedType>();
                    _knownSystemTypes.Add( "Action", SystemTypes.Action );
                    _knownSystemTypes.Add( "Array", SystemTypes.Array );
                    _knownSystemTypes.Add( "Attribute", SystemTypes.Attribute );
                    _knownSystemTypes.Add( "Delegate", SystemTypes.Delegate );
                    _knownSystemTypes.Add( "Enum", SystemTypes.Enum );
                    _knownSystemTypes.Add( "EventArgs", SystemTypes.EventArgs );
                    _knownSystemTypes.Add( "EventHandler", SystemTypes.EventHandler );
                    _knownSystemTypes.Add( "Exception", SystemTypes.Exception );
                    _knownSystemTypes.Add( "IAsyncResult", SystemTypes.IAsyncResult );
                    _knownSystemTypes.Add( "IEnumerable", SystemTypes.IEnumerable );
                    _knownSystemTypes.Add( "IEnumerator", SystemTypes.IEnumerator );
                    _knownSystemTypes.Add( "IDisposable", SystemTypes.IDisposable );
                    _knownSystemTypes.Add( "IntPtr", SystemTypes.IntPtr );
                    _knownSystemTypes.Add( "object", SystemTypes.Object );
                }

                return _knownSystemTypes;
            }
        }

        /// <summary>
        /// Gets a <see cref="PrimitiveType"/> that represents the <see cref="System.Action"/> type.
        /// </summary>
        public static PrimitiveType Action
        {
            get
            {
                if( _Action == null )
                {
                    _Action = new PrimitiveType( actionFullName );
                }

                return _Action;
            }
        }

        /// <summary>
        /// Gets a <see cref="PrimitiveType"/> that represents the <see cref="System.Array"/> type.
        /// </summary>
        public static PrimitiveType Array
        {
            get
            {
                if( _Array == null )
                {
                    _Array = new PrimitiveType( arrayFullName );
                }

                return _Array;
            }
        }

        /// <summary>
        /// Gets a <see cref="PrimitiveType"/> that represents the <see cref="System.Attribute"/> type.
        /// </summary>
        public static PrimitiveType Attribute
        {
            get
            {
                if( _Attribute == null )
                {
                    _Attribute = new PrimitiveType( attributeFullName );
                }

                return _Attribute;
            }
        }

        /// <summary>
        /// Gets a <see cref="PrimitiveType"/> that represents the <see cref="System.Buffer"/> type.
        /// </summary>
        public static PrimitiveType Buffer
        {
            get
            {
                if( _Buffer == null )
                {
                    _Buffer = new PrimitiveType( bufferFullName );
                }

                return _Buffer;
            }
        }

        /// <summary>
        /// Gets a <see cref="PrimitiveType"/> that represents the <see cref="System.Delegate"/> type.
        /// </summary>
        public static PrimitiveType Delegate
        {
            get
            {
                if( _Delegate == null )
                {
                    _Delegate = new PrimitiveType( delegateFullName );
                }

                return _Delegate;
            }
        }

        /// <summary>
        /// Gets a <see cref="PrimitiveType"/> that represents the <see cref="System.Enum"/> type.
        /// </summary>
        public static PrimitiveType Enum
        {
            get
            {
                if( _Enum == null )
                {
                    _Enum = new PrimitiveType( enumFullName );
                }

                return _Enum;
            }
        }

        /// <summary>
        /// Gets a <see cref="PrimitiveType"/> that represents the <see cref="System.EventArgs"/> type.
        /// </summary>
        public static PrimitiveType EventArgs
        {
            get
            {
                if( _EventArgs == null )
                {
                    _EventArgs = new PrimitiveType( eventArgsFullName );
                }

                return _EventArgs;
            }
        }

        /// <summary>
        /// Gets a <see cref="PrimitiveType"/> that represents the <see cref="System.EventHandler"/> type.
        /// </summary>
        public static PrimitiveType EventHandler
        {
            get
            {
                if( _EventHandler == null )
                {
                    _EventHandler = new PrimitiveType( eventHandlerFullName );
                }

                return _EventHandler;
            }
        }

        /// <summary>
        /// Gets a <see cref="PrimitiveType"/> that represents the <see cref="System.Exception"/> type.
        /// </summary>
        public static PrimitiveType Exception
        {
            get
            {
                if( _Exception == null )
                {
                    _Exception = new PrimitiveType( exceptionFullName );
                }

                return _Exception;
            }
        }

        /// <summary>
        /// Gets a <see cref="PrimitiveType"/> that represents the <see cref="System.IAsyncResult"/> type.
        /// </summary>
        public static PrimitiveType IAsyncResult
        {
            get
            {
                if( _IAsyncResult == null )
                {
                    _IAsyncResult = new PrimitiveType( iasyncresultFullName );
                }

                return _IAsyncResult;
            }
        }

        /// <summary>
        /// Gets a <see cref="PrimitiveType"/> that represents the 
        /// <see cref="System.Collections.IEnumerable"/> type.
        /// </summary>
        public static PrimitiveType IEnumerable
        {
            get
            {
                if( _IEnumerable == null )
                {
                    _IEnumerable = new PrimitiveType( ienumerableFullName );
                }

                return _IEnumerable;
            }
        }

        /// <summary>
        /// Gets a <see cref="PrimitiveType"/> that represents the 
        /// <see cref="System.Collections.IEnumerator"/> type.
        /// </summary>
        public static PrimitiveType IEnumerator
        {
            get
            {
                if( _IEnumerator == null )
                {
                    _IEnumerator = new PrimitiveType( ienumeratorFullName );
                }

                return _IEnumerator;
            }
        }

        /// <summary>
        /// Gets a <see cref="PrimitiveType"/> that represents the <see cref="System.IDisposable"/> type.
        /// </summary>
        public static PrimitiveType IDisposable
        {
            get
            {
                if( _IDisposable == null )
                {
                    _IDisposable = new PrimitiveType( idisposableFullName );
                }

                return _IDisposable;
            }
        }

        /// <summary>
        /// Gets a <see cref="PrimitiveType"/> that represents the <see cref="System.IntPtr"/> type.
        /// </summary>
        public static PrimitiveType IntPtr
        {
            get
            {
                if( _IntPtr == null )
                {
                    _IntPtr = new PrimitiveType( intptrFullName );
                }

                return _IntPtr;
            }
        }

        /// <summary>
        /// Gets a <see cref="PrimitiveType"/> that represents the <see cref="System.Object"/> type.
        /// </summary>
        public static PrimitiveType Object
        {
            get
            {
                if( _Object == null )
                {
                    _Object = new PrimitiveType( objectFullName );
                }

                return _Object;
            }
        }

        private static Dictionary<string, DefinedType> _knownSystemTypes;

        static string actionFullName = Type.GetType( "System.Action" ).AssemblyQualifiedName;
        static string arrayFullName = Type.GetType( "System.Array" ).AssemblyQualifiedName;
        static string attributeFullName = Type.GetType( "System.Attribute" ).AssemblyQualifiedName;
        static string bufferFullName = Type.GetType( "System.Buffer" ).AssemblyQualifiedName;
        static string delegateFullName = Type.GetType( "System.Delegate" ).AssemblyQualifiedName;
        static string enumFullName = Type.GetType( "System.Enum" ).AssemblyQualifiedName;
        static string eventArgsFullName = Type.GetType( "System.EventArgs" ).AssemblyQualifiedName;
        static string eventHandlerFullName = Type.GetType( "System.EventHandler" ).AssemblyQualifiedName;
        static string exceptionFullName = Type.GetType( "System.Exception" ).AssemblyQualifiedName;
        static string iasyncresultFullName = Type.GetType( "System.IAsyncResult" ).AssemblyQualifiedName;
        static string idisposableFullName = Type.GetType( "System.IDisposable" ).AssemblyQualifiedName;
        static string ienumerableFullName = Type.GetType( "System.Collections.IEnumerable" ).AssemblyQualifiedName;
        static string ienumeratorFullName = Type.GetType( "System.Collections.IEnumerator" ).AssemblyQualifiedName;
        static string intptrFullName = Type.GetType( "System.IntPtr" ).AssemblyQualifiedName;
        static string objectFullName = Type.GetType( "System.Object" ).AssemblyQualifiedName;

        static PrimitiveType _Action;
        static PrimitiveType _Array;
        static PrimitiveType _Attribute;
        static PrimitiveType _Buffer;
        static PrimitiveType _Delegate;
        static PrimitiveType _Enum;
        static PrimitiveType _EventArgs;
        static PrimitiveType _EventHandler;
        static PrimitiveType _Exception;
        static PrimitiveType _IAsyncResult;
        static PrimitiveType _IEnumerable;
        static PrimitiveType _IEnumerator;
        static PrimitiveType _IDisposable;
        static PrimitiveType _IntPtr;
        static PrimitiveType _Object;
    }
}
