using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology
{
    /// <summary>
    /// Represents a built-in or system type, like <code>uint</code> or
    /// <see cref="System.Guid"/>.
    /// </summary>
    /// <remarks><para>The leaf types of arbitrary type graph frequently 
    /// resolve to primitive or system types. Rather than building up all
    /// of the type metadata that's necessary to describe the type system,
    /// GTU uses the .NET Framework's <see cref="System.Reflection"/> APIs
    /// to represent primitive and system types. Wherever possible, 
    /// the <see cref="PrimitiveType"/> class tries to forward calls to 
    /// an <see cref="UnderlyingType"/> that's created by the 
    /// <see cref="System.Type.ReflectionOnlyGetType"/> method.</para>
    /// <para>The <see cref="PrimitiveTypes"/> class collects many of the 
    /// most commonly used primitive types. Its types are added to the 
    /// <see cref="TypeFactory.KnownTypes"/> collection to aid in type
    /// resolution.</para>
    /// </remarks>
    public class PrimitiveType : DefinedType
    {
        /// <summary>
        /// Initializes a new <see cref="PrimitiveType"/> instance to the specified type name.
        /// </summary>
        /// <param name="typeName">The name of the type to represent.</param>
        /// <remarks><para>The constructor tries to resolve <paramref name="typeName"/> to 
        /// a live <see cref="System.Type"/> by calling the <see cref="System.Type.ReflectionOnlyGetType"/> 
        /// method.</para>
        /// </remarks>
        public PrimitiveType( string typeName )
        {
            this.UnderlyingType = System.Type.ReflectionOnlyGetType( typeName, true, true );

            if( this.UnderlyingType != null )
            {
                this.Name = this.UnderlyingType.Name;
                this.FullName = this.UnderlyingType.FullName;
            }
            else
            {
                this.Name = typeName;
                this.FullName = this.Name;
            }
        }

        /// <summary>
        /// Gets the <see cref="System.Type"/> tht represents the current primitive type.
        /// </summary>
        public System.Type UnderlyingType
        {
            get;
            protected set;
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
                return TypeFactory.CreateAssemblyNamespaceType( this.UnderlyingType.Namespace );
            }

            set
            {
                throw new NotImplementedException( this.Name );
            }
        }

        public override string FriendlyName
        {
            get
            {
                return Utilities.GetFriendlyName( this.Name );
            }
        }

        public override bool IsAbstract
        {
            get
            {
                throw new NotImplementedException( this.Name );
            }
        }

        public override bool IsAttribute
        {
            get
            {
                throw new NotImplementedException( this.Name );
            }
        }

        public override bool IsNamespace
        {
            get
            {
                return false;
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
                return false;
            }
        }

        public override bool IsDelegate
        {
            get
            {
                throw new NotImplementedException( this.Name );
            }
        }

        public override bool IsEnum
        {
            get
            {
                return false;
            }
        }

        public override bool IsGeneric
        {
            get
            {
                throw new NotImplementedException( this.Name );
            }
        }

        public override bool IsInterface
        {
            get
            {
                return false;
            }
        }

        public override bool IsPublic
        {
            get
            {
                throw new NotImplementedException( this.Name );
            }
        }

        public override bool IsReferenceType
        {
            get
            {
                return false;
            }
        }

        public override bool IsSealed
        {
            get
            {
                return false;
            }
        }

        public override bool IsStruct
        {
            get
            {
                return false;
            }
        }

        public override bool IsSystemType
        {
            get
            {
                // Assume that any primitive type also is a system type.
                return true;
            }
        }

        public override bool IsValueType
        {
            get
            {
                return true;
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

        public override TypeModel TypeSystem
        {
            get { return TypeModel.Universal; }
        }

        public override LanguageElement LanguageElement
        {
            get 
            {
                return TypeTopology.LanguageElement.Primitive;
                
            }
        }

        public override string ToString()
        {
            string toString = String.Format( "{0}", this.Name );
            return toString;
        }
    }
}
