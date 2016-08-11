using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen;

/// Contains classes that represent types that are deserialized from 
/// native source code (C++). Currently, only for future use.
namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology.Native
{
    public class NativeType : DoxygenType
    {
        ///////////////////////////////////////////////////////////////////////
        #region Construction

        public NativeType( DoxType doxType )
            : base( doxType )
        {
            this.FullName = this.UnderlyingType.FullName;
            this.Name = this.UnderlyingType.Name;
        }

        protected override void InitializeMembers()
        {
            //if( this.UnderlyingType.BaseType != null )
            //{
            //    this.BaseType = NativeType.CreateType( this.UnderlyingType.BaseType );
            //}

            //// Copy interfaces that managedType implements or inherits.
            //if( this.UnderlyingType.HasInterfaces )
            //{
            //    this.Interfaces = this.UnderlyingType.Interfaces.Select( i =>
            //        NativeType.CreateType( i ) as DefinedType ).ToList();
            //}
            //else
            //{
            //    this.Interfaces = new List<DefinedType>();
            //}

            //// Copy generic arguments.
            //if( this.UnderlyingType.GenericArguments.Count > 0 )
            //{
            //    this.GenericParameters = this.UnderlyingType.GenericArguments.Select( g =>
            //        NativeType.CreateType( g ) as DefinedType ).ToList();
            //}
            //else
            //{
            //    this.GenericParameters = new List<DefinedType>();
            //}

            //if( this.UnderlyingType.HasAttributes )
            //{
            //    this.Attributes = this.UnderlyingType.Attributes.Select( a => NativeType.CreateType( a ) as DefinedType ).ToList();
            //}
            //else
            //{
            //    this.Attributes = new List<DefinedType>();
            //}
        }

        //public static NativeType CreateType( CustomAttributeData attribute )
        //{
        //    return ( NativeType.CreateType( new ObservableType( attribute.AttributeType ) ) );
        //}

        //public static NativeType CreateType( Type type )
        //{
        //    return ( NativeType.CreateType( new ObservableType( type ) ) );
        //}

        //public static NativeType CreateType( DoxType doxygenType )
        //{
        //    NativeType nativeType = null;

        //    if( doxygenType != null )
        //    {
        //        if( NativeType.KnownTypes.ContainsKey( doxygenType.FullName ) )
        //        {
        //            nativeType = NativeType.KnownTypes[doxygenType.FullName];
        //        }
        //        else
        //        {
        //            nativeType = CreateNewType( doxygenType );
        //            NativeType.KnownTypes.Add( nativeType.FullName, nativeType );
        //        }
        //    }
        //    else
        //    {
        //        throw new ArgumentNullException( "observableType" );
        //    }

        //    return nativeType;
        //}

        //public static NativeType CreateNamespaceType( string namespaceFullName )
        //{
        //    // TBD
        //    return null;
        //    //return new NativeNamespace( namespaceFullName );
        //}

        //public static NativeType CreateNewType( DoxType doxygenType )
        //{
        //    NativeType nativeType = null;

        //    if( doxygenType.IsEnum )
        //    {
        //        nativeType = new NativeEnum( doxygenType );
        //    }
        //    else if( doxygenType.IsStruct )
        //    {
        //        nativeType = new NativeStruct( doxygenType );
        //    }
        //    //else if( doxygenType.IsDelegate )
        //    //{
        //    //    nativeType = new NativeDelegate( doxygenType );
        //    //}
        //    //else if( doxygenType.IsAttribute )
        //    //{
        //    //    nativeType = new NativeAttribute( doxygenType );
        //    //}
        //    else if( doxygenType.IsInterface )
        //    {
        //        nativeType = new NativeInterface( doxygenType );
        //    }
        //    else if( doxygenType.IsClass )
        //    {
        //        nativeType = new NativeClass( doxygenType );
        //    }

        //    return nativeType;
        //}


        #endregion


        ///////////////////////////////////////////////////////////////////////
        #region Public Properties
        
        //public static Dictionary<string, NativeType> KnownTypes
        //{
        //    get
        //    {
        //        if( NativeType._knownTypes == null )
        //        {
        //            NativeType._knownTypes = new Dictionary<string,NativeType>();
        //        }

        //        return NativeType._knownTypes;
        //    }
        //}

        //public DoxType UnderlyingType
        //{
        //    get;
        //    protected set;
        //}

        public override DefinedType Namespace
        {
            get
            {
                // TBD
                return null;
                //return NativeType.CreateNamespaceType( this.UnderlyingType.Namespace );
            }
        }

        public override bool IsAbstract
        {
            get
            {
                // TBD
                return false;
                //return this.UnderlyingType.IsAbstract;
            }
        }

        public override bool IsAttribute
        {
            get
            {
                return this.UnderlyingType.IsAttribute;
            }
        }

        public override bool IsClass
        {
            get
            {
                return this.UnderlyingType.IsClass;
            }
        }

        public override bool IsDelegate
        {
            get
            {
                return this.UnderlyingType.IsDelegate;
            }
        }

        public override bool IsEnum
        {
            get
            {
                return this.UnderlyingType.IsEnum;
            }
        }

        public override bool IsGeneric
        {
            get
            {
                return this.UnderlyingType.IsGeneric;
            }
        }

        public override bool IsInterface
        {
            get
            {
                return this.UnderlyingType.IsInterface;
            }
        }

        public override bool IsPublic
        {
            get
            {
                // TBD
                return false;
                //return this.UnderlyingType.IsPublic;
            }
        }

        public override bool IsReferenceType
        {
            get
            {
                // TBD
                return false;
                //return !this.UnderlyingType.IsValueType;
            }
        }

        public override bool IsSealed
        {
            get
            {
                // TBD
                return false;
                //return this.UnderlyingType.IsSealed;
            }
        }

        public override bool IsStruct
        {
            get
            {
                return this.UnderlyingType.IsStruct;
            }
        }

        public override bool IsValueType
        {
            get
            {
                // TBD
                return false;
                //return this.UnderlyingType.IsValueType;
            }
        }

        public override List<DefinedType> ChildTypes
        {
            get
            {
                throw new NotImplementedException();
            }
        } 


        #endregion
        
        public override string ToString()
        {
            string toString = String.Format( "{0}.{1}", this.Namespace, this.Name );
            return toString;
        }

        //protected static Dictionary<string, NativeType> _knownTypes;

    }
}
