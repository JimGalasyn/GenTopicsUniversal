using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// Contains classes for representing type graphs deserialized from Doxygen XML.
namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen
{
    /// <summary>
    /// Represents a type that was deserialized from Doxygen's XML output.
    /// </summary>
    public class DoxygenType : DefinedType
    {
        ///////////////////////////////////////////////////////////////////////
        #region Construction

        /// <summary>
        /// Initializes a new <see cref="DoxygenType"/> from the specified <see cref="DoxType"/>.
        /// </summary>
        /// <param name="doxType">The deserialized type information.</param>
        public DoxygenType( DoxType doxType )
            : base( doxType )
        {
            if( doxType != null )
            {
                this.UnderlyingType = doxType;
                this.Name = doxType.Name;

                // TBD: Decide if we take the parsed FullName or we synthesize
                // FullName from the resolved ParentType and Namespace defined types.
                // In general, we should never be synthesizing a FullName outside
                // of the deserialization layer (DoxType and TypeDeclarationParser).
                this.FullName = doxType.FullName;

                //string fullName = null;

                //if( this.ParentType != null && !this.ParentType.IsGlobalNamespace )
                //{
                //    fullName = String.Format( "{0}.{1}", this.ParentType.FullName, this.Name );
                //}
                //else if( this.Namespace != null && !this.Namespace.IsGlobalNamespace )
                //{
                //    fullName = String.Format( "{0}.{1}", this.Namespace.FullName, this.Name );
                //}
                //else
                //{
                //    fullName = this.Name;
                //}

                //this.FullName = fullName;

                this.SetSourceLanguage();
            }
            else
            {
                // TBD: not checking null for DoxygenField and namespace hacks
                //throw new ArgumentNullException( "doxType" );
            }
        }

        protected override void InitializeMembers()
        {
        }

        protected override void InitializeBaseTypes()
        {
            this.BaseTypes = this.UnderlyingType.BaseTypes.Select( t => 
                TypeFactory.CreateType( t ) as DefinedType ).ToList();
        }

        protected override void InitializeDerivedTypes()
        {
            this.DerivedTypes = this.UnderlyingType.DerivedTypes.Select( t =>
                TypeFactory.CreateType( t ) as DefinedType ).ToList();
        }

        protected override void InitializeGenericTypes()
        {
            //this.GenericParameterTypes = this.UnderlyingType.GenericParameters.Select( p =>
            //    TypeFactory.CreateType( p.type ) as DefinedType ).ToList();

            this.GenericParameterTypes = this.UnderlyingType.GenericParameters.Select( p =>
                TypeFactory.CreateType( p.FullName ) as DefinedType ).ToList();

        }

        private void SetSourceLanguage()
        {
            this.SourceLanguage = String.IsNullOrEmpty( this.UnderlyingType.language ) ? 
                String.Empty : 
                this.UnderlyingType.language;
        }

        #endregion

        ///////////////////////////////////////////////////////////////////////
        #region Public Properties

        public DoxType UnderlyingType
        {
            get;
            protected set;
        }

        public override DefinedType Namespace
        {
            get
            {
                if( this._namespace == null )
                {
                    if( this.UnderlyingType.Namespace != null )
                    {
                        this._namespace = TypeFactory.CreateType( this.UnderlyingType.Namespace );
                    }
                    else
                    {
                        this._namespace = TypeFactory.KnownTypes[String.Empty];
                    }
                }

                return this._namespace;
            }

            set
            {
                this._namespace = value;
            }
        }

        public override ReferenceContent Content
        {
            get
            {
                if( this._referenceContent == null )
                {
                    this._referenceContent = new ReferenceContent( this.UnderlyingType );
                }

                return this._referenceContent;
            }
            set
            {
                this._referenceContent = value;
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

        public override bool IsNamespace
        {
            get
            {
                bool isNamespace = false;

                if( this.IsGlobalNamespace )
                {
                    isNamespace = true;
                }
                else
                {
                    if( this.UnderlyingType != null )
                    {
                        isNamespace = this.UnderlyingType.IsNamespace;
                    }
                }

                return isNamespace;
            }
        }

        public override bool IsGlobalNamespace
        {
            get
            {
                return false;
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

        public bool IsInProjectedType
        {
            get
            {
                return ( this.ParentProjectedType != null );
            }
        }

        public override bool IsPublic
        {
            get
            {
                // TBD
                return true;
                //return this.UnderlyingType.IsPublic;
            }
        }

        public override bool IsReferenceType
        {
            get
            {
                // TBD
                return true;
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

        public override bool IsSystemType
        {
            get
            {
                bool isSystemType = this.FullName.StartsWith(
                    "System", StringComparison.OrdinalIgnoreCase );

                return isSystemType;
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

        public DefinedType ParentProjectedType
        {
            get;
            set;
        }

        public override DefinedType ParentType
        {
            get
            {
                if( this.UnderlyingType.ParentType != null )
                {
                    return TypeFactory.CreateType( this.UnderlyingType.ParentType );
                }
                else
                {
                    return TypeFactory.CreateType( String.Empty );
                }
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public override List<DefinedType> ChildTypes
        {
            get
            {
                if( this._childTypes == null )
                {
                    this._childTypes = this.UnderlyingType.ChildTypes.Select( t =>
                        TypeFactory.CreateType( t ) as DefinedType ).ToList();
                }

                return this._childTypes;
            }
        }

        //public override List<DefinedType> DerivedTypes
        //{
        //    get 
        //    {
        //        if( this._derivedTypes == null )
        //        {
        //            this._derivedTypes = this.UnderlyingType.DerivedTypes.Select( t =>
        //                TypeFactory.CreateType( t ) as DefinedType ).ToList();
        //        }

        //        return this._derivedTypes;   
        //    }
        //}

        public override TypeModel TypeSystem
        {
            get { return TypeModel.Native; }
        }

        public override LanguageElement LanguageElement
        {
            get { throw new NotImplementedException(); }
        }


        #endregion

        public override string ToString()
        {
            string toString = String.Format( "{0}.{1}", this.Namespace.FullName, this.Name );
            return toString;
        }
    }
}