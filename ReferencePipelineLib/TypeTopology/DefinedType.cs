using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using OsgContentPublishing.EventLogging;
using OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen;
using ReflectionUtilities;

/// Contains types for representing arbitrary type graphs from the C family of languages.
namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology
{
    /// <summary>
    /// Provides the base implementation for representing a type.
    /// </summary>
    /// <remarks><para>The <see cref="DefinedType"/> class represents a type that's defined in 
    /// pure native code (C/C++, IDL, WIN32), pure managed code (C#, VB), or 
    /// Windows Runtime (WinRT) code (RIDL).</para>
    /// <para>Use the following derived classes to represent types from the various programming
    /// environments:</para>
    /// <para><list>
    /// <item><see cref="DoxygenType"/>: Pure native types</item>
    /// <item><see cref="ManagedType"/>: Pure managed types</item>
    /// <item><see cref="ProjectedType"/>: Windows Runtime (WinRT) types</item>
    /// </list></para>
    /// </remarks>
    public abstract class DefinedType
    {
        ///////////////////////////////////////////////////////////////////////
        #region Construction

        public DefinedType()
        {
        }

        /// <summary>
        /// Initializes a new <see cref="DefinedType"/> instance to the 
        /// specified <see cref="ObservableType"/>.
        /// </summary>
        /// <param name="managedType">A source type that was deserialized 
        /// from a managed assembly or winmd file.</param>
        public DefinedType( ObservableType managedType )
        {
        }

        /// <summary>
        /// Initializes a new <see cref="DefinedType"/> instance to the 
        /// specified <see cref="DoxType"/>.
        /// </summary>
        /// <param name="doxType">A source type that was deserialized 
        /// from native code.</param>
        public DefinedType( DoxType doxType )
        {
        }

        /// <summary>
        /// Initializes a new <see cref="DefinedType"/> instance to the 
        /// specified namespace string.
        /// </summary>
        /// <param name="namespaceFullName">The full name of the namespace.
        /// The full name is the namespace name prepended with its ancestor 
        /// namespaces.</param>
        public DefinedType( string namespaceFullName )
        {   
        }

        /// <summary>
        /// Initializes a new <see cref="DefinedType"/> instance to the 
        /// specified <see cref="ObservableType"/> and <see cref="DoxType"/>.
        /// </summary>
        /// <param name="managedType">A source type that was deserialized 
        /// from a managed assembly or winmd file.</param>
        /// <param name="doxygenType">A source type that was deserialized 
        /// from native code.</param>
        /// <remarks><para>Used to initialize <see cref="ProjectedType"/> instances.</para>
        /// </remarks>
        public DefinedType( ObservableType managedType, DoxType doxygenType )
        {
        }

        /// <summary>
        /// When overriden in a derived class, creates and populates 
        /// the <see cref="Members"/> collection.
        /// </summary>
        protected abstract void InitializeMembers();

        /// <summary>
        /// When overriden in a derived class, creates and populates 
        /// the <see cref="BaseTypes"/> collection.
        /// </summary>
        protected abstract void InitializeBaseTypes();

        /// <summary>
        /// When overriden in a derived class, creates and populates 
        /// the <see cref="DerivedTypes"/> collection.
        /// </summary>
        protected abstract void InitializeDerivedTypes();

        /// <summary>
        /// When overriden in a derived class, creates and populates 
        /// the <see cref="GenericTypes"/> collection.
        /// </summary>
        protected abstract void InitializeGenericTypes();

        #endregion

        ///////////////////////////////////////////////////////////////////////
        #region Public Properties

        /// <summary>
        /// Gets the name of the current type.
        /// </summary>
        /// <remarks><para>The <see cref="Name"/> property is the name of the type
        /// without any decorations or generic/template parameters.</para>
        /// </remarks>
        public string Name
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets the full name of the current type.
        /// </summary>
        /// <remarks><para>The <see cref="FullName"/> property is the name of the type,
        /// appended with generic/template parameters, and prepended with the name of 
        /// the parent type or namespace. Most type comparisons use the <see cref="FullName"/> 
        /// property, for example in the <see cref="DefinedTypeComparer"/>.</para>
        /// <para>Sometimes, "loose" comparisons are necessary, when only the
        /// <see cref="Name"/> of the a type is known. In this case, create a 
        /// <see cref="DefinedTypeComparer"/> instance with the <code>enableLooseTypecomparisons</code> 
        /// parameter set to true.</para>
        /// </remarks>
        public string FullName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the friendly name of the current type.
        /// </summary>
        /// <remarks><para>the friendly name is the <see cref="Name"/> of the type,
        /// appended with generic/template parameters.</para>
        /// </remarks>
        public virtual string FriendlyName
        {
            get
            {
                return Utilities.GetFriendlyName( this );
            }
        }

        /// <summary>
        /// Gets the type that represents the current type's parent namespace.
        /// </summary>
        public abstract DefinedType Namespace
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the generic/template parameters of the current type, if it's 
        /// declared to be a generic/template class.
        /// </summary>
        /// <remarks><para>Currently, there's a bug in the design: this property
        /// should be a collection of <see cref="DefinedParameter"/> instances,
        /// because the generic/template parameter types need to have their context
        /// available, like whether they're pointers or references.
        /// </para>
        /// </remarks>
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
        /// Gets a collection of types that the current type derives from. 
        /// </summary>
        public virtual List<DefinedType> BaseTypes
        {
            get
            {
                if( this._baseTypes == null )
                {
                    this.InitializeBaseTypes();
                }

                return this._baseTypes;
            }

            protected set
            {
                if( value != this._baseTypes )
                {
                    this._baseTypes = value;
                }
            }
        }

        /// <summary>
        /// Gets a collection of interfaces that the current type derives from. 
        /// </summary>
        public List<DefinedType> Interfaces
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets the base type that the current type derives from. 
        /// </summary>
        /// <remarks><para>this is a convenience property for type systems
        /// that allow only single inheritance.</para>
        /// </remarks>
        public DefinedType BaseType
        {
            get;
            protected set;
        }

        /// <summary>
        /// Get the parent type of the current type.
        /// </summary>
        /// <remarks><para>In most cases, this is a namespace, but some
        /// types are declared within a class, for example, inner classes,
        /// enums and structs. Note that this distinct from inheritance.</para>
        /// </remarks>
        public virtual DefinedType ParentType
        {
            get
            {
                if( this._parentType == null )
                {
                    return this.Namespace;
                }
                else
                {
                    return _parentType;
                }
            }

            set
            {
                if( this._parentType != value )
                {
                    _parentType = value;
                }
            }
        }

        /// <summary>
        /// Gets a collection of the current type's child types.
        /// </summary>
        /// <remarks><para>In most cases, the current type is a namespace,
        /// and the child types are all of the types within the namespace,
        /// including child namespaces.</para>
        /// <para>In some cases, the current type declares inner classes, enums,
        /// or structs. These are collected in the <see cref="ChildTypes"/>
        /// property. Note that this distinct from inheritance.</para>
        /// </remarks>
        public abstract List<DefinedType> ChildTypes
        {
            get;
        }

        /// <summary>
        /// Gets a collection of types that derive from the current type.
        /// </summary>
        public virtual List<DefinedType> DerivedTypes
        {
            get
            {
                if( this._derivedTypes == null )
                {
                    this.InitializeDerivedTypes();
                }

                return this._derivedTypes;
            }

            protected set
            {
                if( value != this._derivedTypes )
                {
                    this._derivedTypes = value;
                }
            }
        }

        /// <summary>
        /// Gets the attributes that are applied to the current type.
        /// </summary>
        public List<DefinedType> Attributes
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets the MSDN topic ID for the current type.
        /// </summary>
        public string TopicId
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets the content for the current type.
        /// </summary>
        /// <remarks><para>Currently, all content is harvested from code comments,
        /// or in the case of some constructors, content is autogenerated.</para>
        /// </remarks>
        public virtual ReferenceContent Content
        {
            get;
            set;
        }

        /// <summary>
        /// Copies content from the specified type to the current type.
        /// </summary>
        /// <param name="sourceType">The type to copy content from.</param>
        /// <remarks><para>Content is copied from <paramref name="sourceType"/> to 
        /// the current type, including content for all members and parameters.
        /// </para>
        /// </remarks>
        public virtual void CopyContent( DefinedType sourceType )
        {
            if( this.Content.IsEmpty && sourceType.HasContent )
            {
                this.Content = sourceType.Content;
            }

            var memberComparer = new Utilities.DefinedMemberComparer();
            foreach( var member in this.Members )
            {
                DefinedMember matchingMember = sourceType.Members.Find( m => 
                    memberComparer.Equals( m, member ) );
                if( matchingMember != null )
                {
                    if( member.Content.IsEmpty &&
                        matchingMember.HasContent )
                    {
                        member.Content = matchingMember.Content;
                    }

                    if( member.IsMethod )
                    {
                        // TBD: Implement a DefinedMethod type for generality?
                        DoxygenMethod method = member as DoxygenMethod;
                        foreach( var param in method.Parameters )
                        {
                            DoxygenMethod matchingMethod = matchingMember as DoxygenMethod;
                            if( matchingMethod != null )
                            {
                                DefinedParameter matchingParam = matchingMethod.Parameters.Find( 
                                    p => p.Name == param.Name );
                                if( matchingParam != null )
                                {
                                    param.Content = matchingParam.Content;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets a collection of the current type's members. 
        /// </summary>
        /// <remarks><para>Members comprise the type's properties, methods,
        /// events, and fields.</para>
        /// </remarks>
        public virtual List<DefinedMember> Members
        {
            get
            {
                if( this._members == null )
                {
                    this.InitializeMembers();
                }

                return this._members;
            }

            protected set
            {
                if( value != null )
                {
                    this._members = value;
                }
            }
        }

        /// <summary>
        /// Gets the member fields for the current type.
        /// </summary>
        public virtual List<DefinedMember> Fields
        {
            get
            {
                if( this._fields == null )
                {
                    this._fields = this.Members.Where( m => m.IsField ).ToList();
                }

                return this._fields;
            }
        }

        /// <summary>
        /// Gets the member properties for the current type.
        /// </summary>
        public virtual List<DefinedMember> Properties
        {
            get
            {
                if( this._properties == null )
                {
                    this._properties = this.Members.Where( m => m.IsProperty ).ToList();
                }

                return this._properties;
            }
        }

        /// <summary>
        /// Gets the member methods/functions for the current type.
        /// </summary>
        public virtual List<DefinedMember> Methods
        {
            get
            {
                if( this._methods == null )
                {
                    this._methods = this.Members.Where( m => 
                        m.IsMethod && 
                        !m.IsSystemObjectMember &&
                        !m.IsPropertyAccessor && 
                        !m.IsEventAccessor &&
                        !m.IsConstructor && 
                        !m.IsDestructor ).ToList();
                }

                return this._methods;
            }
        }

        /// <summary>
        /// Gets the constructors for the current type.
        /// </summary>
        public virtual List<DefinedMember> Constructors
        {
            get
            {
                if( this._constructors == null )
                {
                    this._constructors = this.Members.Where( m => m.IsConstructor ).ToList();
                }

                return this._constructors;
            }
        }

        /// <summary>
        /// Gets the destructors for the current type.
        /// </summary>
        public virtual DefinedMember Destructor
        {
            get
            {
                if( this._destructor == null )
                {
                    this._destructor = this.Members.FirstOrDefault( m => m.IsDestructor );
                }

                return this._destructor;
            }
        }

        /// <summary>
        /// Gets the member events for the current type.
        /// </summary>
        public virtual List<DefinedMember> Events
        {
            get
            {
                if( this._events == null )
                {
                    this._events = this.Members.Where( m => m.IsEvent ).ToList();
                }

                return this._events;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current type is an abstract class.
        /// </summary>
        public abstract bool IsAbstract
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether the current type is an attribute.
        /// </summary>
        public abstract bool IsAttribute
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether the current type is a namespace.
        /// </summary>
        public abstract bool IsNamespace
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether the current type is the global namespace.
        /// </summary>
        public abstract bool IsGlobalNamespace
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether the current type is a class.
        /// </summary>
        public abstract bool IsClass
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether the current type is a delegate
        /// or function pointer.
        /// </summary>
        public abstract bool IsDelegate
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether the current type is an enumeration.
        /// </summary>
        public abstract bool IsEnum
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether the current type is a generic or 
        /// template class.
        /// </summary>
        public abstract bool IsGeneric
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether the current type is an interface.
        /// </summary>
        public abstract bool IsInterface
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether the current type has public access level.
        /// </summary>
        public abstract bool IsPublic
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether the current type is a reference type.
        /// </summary>
        public abstract bool IsReferenceType
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether the current type can be derived from.
        /// </summary>
        public abstract bool IsSealed
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether the current type is a struct.
        /// </summary>
        public abstract bool IsStruct
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether the current type is a system type.
        /// </summary>
        /// <remarks><para>A system type can be a primitive type, like <code>uint</code>,
        /// or a system type, like <see cref="System.Guid"/>.</para>
        /// </remarks>
        public abstract bool IsSystemType
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether the current type is a value type.
        /// </summary>
        public abstract bool IsValueType
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether the current type has field members.
        /// </summary>
        public virtual bool HasFields
        {
            get
            {
                return ( this.Fields.Count > 0 );
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current type has property members.
        /// </summary>
        public virtual bool HasProperties
        {
            get
            {
                return ( this.Properties.Count > 0 );
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current type has method/function members.
        /// </summary>
        public virtual bool HasMethods
        {
            get
            {
                return ( this.Methods.Count > 0 );
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current type has constructors.
        /// </summary>
        public virtual bool HasConstructors
        {
            get
            {
                return ( this.Constructors.Count > 0 );
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current type has a destructor.
        /// </summary>
        public virtual bool HasDestructor
        {
            get
            {
                return( this.Destructor != null );
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current type has a 
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
        /// Gets a value indicating whether the current type has event members.
        /// </summary>
        public virtual bool HasEvents
        {
            get
            {
                return ( this.Events.Count > 0 );
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current type has child types,
        /// like inner classes, enums, or structs.
        /// </summary>
        public virtual bool HasChildTypes
        {
            get
            {
                return( this.ChildTypes.Count > 0 );
            }
        }

        /// <summary>
        /// Gets the language element that describes the current type, like
        /// "class" or "interface".
        /// </summary>
        public abstract LanguageElement LanguageElement
        {
            get;
        }

        /// <summary>
        /// Gets the name of the language element that describes the current type.
        /// </summary>
        /// <remarks><para>This is useful for serializers, when emitting a 
        /// human-readable type description.</para>
        /// </remarks>
        public virtual string LanguageElementName
        {
            get
            {
                return LanguageElements.GetLanguageElementName( this.LanguageElement );
            }
        }

        /// <summary>
        /// Gets the source code language that the current type was declared in.
        /// </summary>
        public string SourceLanguage
        {
            get;
            protected set;
        }

        /// <summary>
        /// For future use.
        /// </summary>
        public abstract TypeModel TypeSystem
        {
            get;
        }

        #endregion

        protected List<DefinedMember> _members;
        protected List<DefinedMember> _fields;
        protected List<DefinedMember> _properties;
        protected List<DefinedMember> _methods;
        protected List<DefinedMember> _constructors;
        protected List<DefinedMember> _events;
        protected List<DefinedType> _childTypes;
        protected List<DefinedType> _baseTypes;
        protected List<DefinedType> _derivedTypes;
        protected List<DefinedType> _genericParameterTypes;

        protected DefinedType _parentType;
        protected DefinedType _namespace;
        protected ReferenceContent _referenceContent;
        protected DefinedMember _destructor;
    }
}
