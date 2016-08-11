using OsgContentPublishing.EventLogging;
using OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen;
using ReflectionUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology
{
    /// <summary>
    /// Provides the base impementation for a member of a <see cref="DefinedType"/>.
    /// </summary>
    public abstract class DefinedMember
    {
        ///////////////////////////////////////////////////////////////////////
        #region Construction

        /// <summary>
        /// Initializes a new instance of the <see cref="DefinedMember"/> class to the 
        /// specified <see cref="MemberDef"/> and parent type.
        /// </summary>
        /// <param name="memberDef">A deserialized Doxygen <see cref="MemberDef"/>.</param>
        /// <param name="parentType">The parent type of the type represented 
        /// by <paramref name="memberDef"/>.</param>
        public DefinedMember( MemberDef memberDef, DefinedType parentType )
        {
            this.Initialize( memberDef, parentType );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefinedMember"/> class to the 
        /// specified <see cref="MemberInfo"/> and parent type.
        /// </summary>
        /// <param name="memberInfo">A <see cref="MemberInfo"/> from a managed assembly or 
        /// winmd file.</param>
        /// <param name="parentType">The parent type of the type represented 
        /// by <paramref name="memberInfo"/>.</param>
        public DefinedMember( MemberInfo memberInfo, DefinedType parentType )
        {
            this.Initialize( memberInfo, parentType );
        }

        private void Initialize( MemberInfo memberInfo, DefinedType parentType )
        {
            if( memberInfo != null && parentType != null )
            {
                this.Name = memberInfo.Name;
                this.ParentType = parentType;
                this.Content = new ReferenceContent();
            }
            else
            {
                throw new ArgumentNullException();
            }
        }

        private void Initialize( MemberDef memberDef, DefinedType parentType )
        {
            if( memberDef != null )
            {
                this.Name = memberDef.name;
                this.Content = new ReferenceContent( memberDef );
            }

            if( parentType != null )
            {
                this.ParentType = parentType;
            }
            else
            {
                // null parent is permissible, in the case of
                // functions at global scope, as implemented 
                // in the DoxygenFunction class.
                //throw new ArgumentNullException();
            }
        }

        #endregion 

        ///////////////////////////////////////////////////////////////////////
        #region Public Properties

        /// <summary>
        /// Gets or sets the type that characterizes the current member.
        /// </summary>
        /// <remarks><para>A member is characterized by a specific type, 
        /// and the interpretation of this type varies depending on the 
        /// kind of member. For example, the <see cref="Type"/> property 
        /// for a method or function is its return value. The 
        /// <see cref="Type"/> property for an enum (usually) is an integer.
        /// The <see cref="Type"/> property for an event is an event handler 
        /// delegate.</para>
        /// </remarks>
        public virtual DefinedType Type
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the current member.
        /// </summary>
        /// <remarks><para>This is the simple name of the member, without
        /// generic/template parameters or method signature. 
        /// </para>
        /// </remarks>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the MSDN topic ID for the current member.
        /// </summary>
        public string TopicId
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets a collection of parameters for the current type.
        /// </summary>
        /// <remarks><para>This is the collection of parameters in a 
        /// method or event signature. It doesn't include generic/template
        /// parameters.</para>
        /// </remarks>
        public virtual List<DefinedParameter> Parameters
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets a value indicating whether the current member has parameters.
        /// </summary>
        public bool HasParameters
        {
            get
            {
                bool hasParameters = false;

                if( this.Parameters != null && 
                    this.Parameters.Count > 0 )
                {
                    hasParameters = true;
                }

                return hasParameters;
            }
        }

        /// <summary>
        /// Gets or sets the generic/template parameters for the current member.
        /// </summary>
        public virtual List<DefinedType> GenericParameterTypes
        {
            get
            {
                if( this._genericParameterTypes == null )
                {
                    this.InitializeGenericTypes();
                }

                return this._genericParameterTypes;
            }

            protected set
            {
                if( value != this._genericParameterTypes )
                {
                    this._genericParameterTypes = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the type that the current member is bound to.
        /// </summary>
        public DefinedType ParentType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the content for the current member.
        /// </summary>
        /// <remarks><para>Currently, all content is harvested from code comments,
        /// or in the case of some constructors, content is autogenerated.</para>
        /// </remarks>
        public ReferenceContent Content
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the compile-time value of the current member.
        /// </summary>
        /// <remarks><para>Currently, this is used exclusively for field members
        /// in enum types.</para>
        /// </remarks>
        public object Value
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating whether the current member is a value 
        /// in an enum type.
        /// </summary>
        public abstract bool IsEnumValue
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether the current member is a field
        /// in a class, enum, or struct.
        /// </summary>
        public abstract bool IsField
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether the current member has 
        /// generic/template parameters.
        /// </summary>
        public virtual bool IsGeneric
        {
            get
            {
                bool isGeneric =
                    this.GenericParameterTypes != null &&
                    this.GenericParameterTypes.Count > 0;

                return isGeneric;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current member is a property.
        /// </summary>
        public abstract bool IsProperty
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether the current member is a 
        /// property accessor method/function.
        /// </summary>
        public abstract bool IsPropertyAccessor
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether the current member is a method/function.
        /// </summary>
        public abstract bool IsMethod
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether the current member has overloads.
        /// </summary>
        public bool IsOverload
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets a value indicating whether the current member is a constructor.
        /// </summary>
        public abstract bool IsConstructor
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether the current member is a destructor.
        /// </summary>
        public abstract bool IsDestructor
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether the current member represents an event.
        /// </summary>
        public abstract bool IsEvent
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether the current member is an event
        /// accessor method/function.
        /// </summary>
        public abstract bool IsEventAccessor
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether the current member has public access level.
        /// </summary>
        public abstract bool IsPublic
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether the current member has private access level.
        /// </summary>
        public abstract bool IsPrivate
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether the current member is static.
        /// </summary>
        public abstract bool IsStatic
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether the current member is a member of 
        /// the <see cref="System.Object"/> type.
        /// </summary>
        public abstract bool IsSystemObjectMember
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether the current member is const.
        /// </summary>
        public abstract bool IsConst
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether the current member's value canbe changed.
        /// </summary>
        public abstract bool IsMutable
        {
            get;
        }

        /// <summary>
        /// Gets the language element that describes the current member, like
        /// "constructor" or "method".
        /// </summary>
        public abstract LanguageElement LanguageElement
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether the current member has a 
        /// non-empty <see cref="ReferenceContent"/> instance.
        /// </summary>
        public bool HasContent
        {
            get
            {
                bool hasContent = this.Content != null && this.Content.HasContent;
                return hasContent;
            }
        }

        /// <summary>
        /// Gets the name of the language element that describes the current member.
        /// </summary>
        /// <remarks><para>This is useful for serializers, when emitting a 
        /// human-readable member description.</para>
        /// </remarks>
        public virtual string LanguageElementName
        {
            get
            {
                return LanguageElements.GetLanguageElementName( this.LanguageElement );
            }
        }

        /// <summary>
        /// When overriden in a derived class, creates and populates 
        /// the <see cref="GenericTypes"/> collection.
        /// </summary>
        protected abstract void InitializeGenericTypes();

        /// <summary>
        /// For future use.
        /// </summary>
        public abstract TypeModel TypeSystem
        {
            get;
        }

        /// <summary>
        /// Gets the attributes that are applied to the current member.
        /// </summary>
        public List<DefinedType> Attributes
        {
            get;
            protected set;
        }

        // TBD
        //public List<DefinedType> AttributeData
        //{
        //    get;
        //    protected set;
        //}

        #endregion 

        public override string ToString()
        {
            string toString = String.Format( "{0}.{1}", this.ParentType.Name, this.Name );
            return toString;
        }

        protected ReferenceContent _referenceContent;
        protected List<DefinedType> _genericParameterTypes;
    }
}
