using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen
{
    public class DoxygenFacadeType : DoxygenType
    {
        public DoxygenFacadeType( string typeName ) : base( null )
        {
            this.FullName = typeName;
            this.Name = this.FullName;
        }

        public DoxygenFacadeType( DoxType doxType )
            : base( doxType )
        {
            this.FullName = doxType.Name;
            this.Name = doxType.Name;
        }

        //public override DoxType UnderlyingType
        //{
        //    get
        //    {
        //        throw new UnknownTypeException( this.FullName );
        //    }
        //    set
        //    {
        //    }
        //}

        protected override void InitializeMembers()
        { 
        }

        public override ReferenceContent Content
        {
            get
            {
                if( this._referenceContent == null )
                {
                    this._referenceContent = new ReferenceContent();
                }

                return this._referenceContent;
            }

            set
            {
                this._referenceContent = value;
            }
        }

        public override DefinedType Namespace
        {
            get
            {
                return TypeFactory.KnownTypes[String.Empty];
                //return null;
                //throw new UnknownTypeException( this.Name );
            }
        }

        public override bool IsAbstract
        {
            get
            {
                throw new UnknownTypeException( this.Name );
            }
        }

        public override bool IsAttribute
        {
            get
            {
                throw new UnknownTypeException( this.Name );
            }
        }

        public override bool IsClass
        {
            get
            {
                throw new UnknownTypeException( this.Name );
            }
        }

        public override bool IsDelegate
        {
            get
            {
                throw new UnknownTypeException( this.Name );
            }
        }

        public override bool IsEnum
        {
            get
            {
                throw new UnknownTypeException( this.Name );
            }
        }

        public override bool IsGeneric
        {
            get
            {
                throw new UnknownTypeException( this.Name );
            }
        }

        public override bool IsInterface
        {
            get
            {
                throw new UnknownTypeException( this.Name );
            }
        }

        public override bool IsPublic
        {
            get
            {
                throw new UnknownTypeException( this.Name );
            }
        }

        public override bool IsReferenceType
        {
            get
            {
                throw new UnknownTypeException( this.Name );
            }
        }

        public override bool IsSealed
        {
            get
            {
                throw new UnknownTypeException( this.Name );
            }
        }

        public override bool IsStruct
        {
            get
            {
                throw new UnknownTypeException( this.Name );
            }
        }

        public override bool IsValueType
        {
            get
            {
                throw new UnknownTypeException( this.Name );
            }
        }

        public override List<DefinedType> ChildTypes
        {
            get
            {
                if( this._childTypes == null )
                {
                    this._childTypes = new List<DefinedType>();
                }

                return this._childTypes;
            }
        }

        public override List<DefinedType> BaseTypes
        {
            get
            {
                if( this._baseTypes == null )
                {
                    this._baseTypes = new List<DefinedType>();
                }

                return this._baseTypes;
            }
        }

        public override List<DefinedType> DerivedTypes
        {
            get 
            {
                if( this._derivedTypes == null )
                {
                    this._derivedTypes = new List<DefinedType>();
                }

                return this._derivedTypes; 
            }
        }

        public override List<DefinedMember> Members
        {
            get
            {
                if( this._members == null )
                {
                    this._members = new List<DefinedMember>();
                }

                return this._members;
            }
        }

        public override LanguageElement LanguageElement
        {
            get
            {
                return LanguageElement.Unknown;
            }
        }

        public override string ToString()
        {
            string toString = String.Format( "{0} FACADE TYPE", this.Name );
            return toString;
        }
    }
}
