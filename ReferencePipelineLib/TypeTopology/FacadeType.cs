using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology
{
    /// <summary>
    /// Represents a type about which nothing is known but its name.
    /// </summary>
    /// <remarks><para>Facade types occur during deserialization from the 
    /// Doxygen XML output. Ideally, a facade type is resolved at a later time,
    /// when all of the types in the content set have been deserialized, but
    /// frequently, facade types are never resolved. In this case, they're 
    /// essentially placeholders.</para>
    /// <para>Serializers must be able to handle facade types when they're 
    /// encountered during serialization. For example, when creating links,
    /// the <see cref="HtmlSerializer"/> creates links to facade types 
    /// by handing the facade type's name to MSDN search.</para>
    /// </remarks>
    public class FacadeType : DefinedType
    {
        /// <summary>
        /// Initializes a new <see cref="FacadeType"/> instance to the specified type name.
        /// </summary>
        /// <param name="typeName"></param>
        public FacadeType( string typeName )
        {
            this.FullName = typeName;
            this.Name = this.FullName;
        }

        /// <summary>
        /// Not defined for a facade type. Raises an <see cref="UnknownTypeException"/>. 
        /// </summary>
        public string UnderlyingType
        {
            get
            {
                throw new UnknownTypeException( this.FullName );
            }
            set
            {
            }
        }

        protected override void InitializeMembers()
        { 
        }

        protected override void InitializeBaseTypes()
        {
        }

        protected override void InitializeDerivedTypes()
        {   
        }

        protected override void InitializeGenericTypes()
        {
        }

        public override DefinedType Namespace
        {
            get
            {
                throw new UnknownTypeException( this.Name );
            }

            set
            {
                throw new UnknownTypeException( this.Name );
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

        public override bool IsNamespace
        {
            get
            {
                throw new UnknownTypeException( this.Name );
            }
        }

        public override bool IsGlobalNamespace
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

        public override bool IsSystemType
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

        public override DefinedType ParentType
        {
            get
            {
                throw new NotImplementedException();
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
                throw new NotImplementedException();
            }
        }

        public override List<DefinedType> DerivedTypes
        {
            get { throw new NotImplementedException(); }
        }

        public override List<DefinedType> GenericParameterTypes
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override TypeModel TypeSystem
        {
            get { throw new NotImplementedException(); }
        }

        public override LanguageElement LanguageElement
        {
            get { throw new NotImplementedException(); }
        }

        public override string ToString()
        {
            string toString = String.Format( "{0}", this.Name );
            return toString;
        }
    }
}
