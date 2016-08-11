using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Reflection;
using System.Text;

using ReflectionUtilities;
using OsgContentPublishing.EventLogging;

/// Contains classes for representing type graphs that are extracted from
/// managed assemblies and winmd files.
namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology.Assembly
{
    /// <summary>
    /// Represents a type that was loaded from a managed assembly or
    /// Windows Metadata (winmd) file.
    /// </summary>
    public class AssemblyType : DefinedType
    {
        ///////////////////////////////////////////////////////////////////////
        #region Construction

        /// <summary>
        /// Initializes a new <see cref="AssemblyType"/> instance to the 
        /// specified <see cref="ObservableType"/>.
        /// </summary>
        /// <param name="observableType">The managed type.</param>
        public AssemblyType( ObservableType observableType ) : base( observableType )
        {
            if( observableType != null )
            {
                this.UnderlyingType = observableType;
                this.Name = Utilities.GetUndecoratedName( this.UnderlyingType );
                
                if( String.IsNullOrEmpty( this.UnderlyingType.FullName ) )
                {
                    // TBD: This is a hack for template parameter types.
                    this.FullName = this.Name;
                }
                else
                {
                    this.FullName = Utilities.GetUndecoratedName( this.UnderlyingType.FullName );
                }

                this.Content = new ReferenceContent();
                this.TopicId = Utilities.GetTopicIdForType( this.UnderlyingType );
                this.SourceLanguage = "C#"; // TBD: hard-coded for now.
            }
            else
            {
                throw new ArgumentNullException( "observableType" );
            }
        }

        /// <summary>
        /// Initializes a new <see cref="AssemblyType"/> instance to the 
        /// specified namespace.
        /// </summary>
        /// <param name="namespaceFullName">The full name of a namespace.</param>
        public AssemblyType( string namespaceFullName )
            : base( namespaceFullName )
        {
            if( namespaceFullName != null && namespaceFullName != string.Empty )
            {
                this.UnderlyingType = null; // Being explicit about this.
                this.FullName = namespaceFullName; // TBD: Not assigning Name?
                this.Content = new ReferenceContent();
                this.TopicId = Utilities.GetTopicIdForNamespce( this.FullName );
                this.SourceLanguage = "C#"; // TBD: hard-coded for now.
            }
            else
            {
                throw new ArgumentNullException( namespaceFullName );
            }
        }

        protected override void InitializeMembers()
        {
            if( this.UnderlyingType.BaseType != null )
            {
                this.BaseType = TypeFactory.CreateAssemblyType( this.UnderlyingType.BaseType );
            }

            // Copy interfaces that managedType implements or inherits.
            if( this.UnderlyingType.HasInterfaces )
            {
                this.Interfaces = this.UnderlyingType.Interfaces.Select( i =>
                    TypeFactory.CreateAssemblyType( i ) as DefinedType ).ToList();
            }
            else
            {
                this.Interfaces = new List<DefinedType>();
            }

            // Copy generic arguments.
            if( this.UnderlyingType.GenericArguments.Count > 0 )
            {
                this.GenericParameterTypes = this.UnderlyingType.GenericArguments.Select( g => 
                    TypeFactory.CreateAssemblyType( g ) as DefinedType ).ToList();
            }
            else
            {
                this.GenericParameterTypes = new List<DefinedType>();
            }

            if( this.UnderlyingType.HasAttributes )
            {
                this.Attributes = this.UnderlyingType.Attributes.Select( a => 
                    TypeFactory.CreateAssemblyType( a ) as DefinedType ).ToList();
            }
            else
            {
                this.Attributes = new List<DefinedType>();
            }
        }

        protected override void InitializeBaseTypes()
        {
            this.BaseTypes = new List<DefinedType>();
        }

        protected override void InitializeDerivedTypes()
        {
            this.DerivedTypes = new List<DefinedType>();
        }

        protected override void InitializeGenericTypes()
        {
            this.GenericParameterTypes = new List<DefinedType>();
        }


        #endregion

        ///////////////////////////////////////////////////////////////////////
        #region Public Properties

        public ObservableType UnderlyingType
        {
            get;
            protected set;
        }

        public override DefinedType Namespace
        {
            get
            {
                return TypeFactory.CreateAssemblyNamespaceType( this.UnderlyingType.Namespace );
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public override string FriendlyName
        {
            get
            {
                return Utilities.GetFriendlyName( this.UnderlyingType );
            }
        }

        public override bool IsAbstract
        {
            get
            {
                return this.UnderlyingType.IsAbstract;
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
                return( this is AssemblyNamespace );
            }
        }

        public override bool IsGlobalNamespace
        {
            get
            {   
                return( this is GlobalNamespace );
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
                bool isEnum = false;

                try
                {
                    isEnum = this.UnderlyingType.IsEnum;
                }
                catch( Exception ex )
                {
                    Debug.WriteLine( "AssemblyType.IsEnum threw" );
                }

                return isEnum;
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
                return( this.ParentProjectedType != null );
            }
        }

        public override bool IsPublic
        {
            get
            {
                return this.UnderlyingType.IsPublic;
            }
        }

        public override bool IsReferenceType
        {
            get
            {
                return !this.UnderlyingType.IsValueType;
            }
        }

        public override bool IsSealed
        {
            get
            {
                return this.UnderlyingType.IsSealed;
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
                bool isSystemType =
                    this.FullName.StartsWith( "System", StringComparison.OrdinalIgnoreCase ) ||
                    this.FullName.StartsWith( "Windows.Foundation.Collections", StringComparison.OrdinalIgnoreCase ) ||
                    this.FullName.StartsWith( "Windows.Foundation.Metadata", StringComparison.OrdinalIgnoreCase ) ||
                    this.FullName.StartsWith( "Windows.Storage", StringComparison.OrdinalIgnoreCase ) ||
                    this.Name.StartsWith( "TypedEventHandler", StringComparison.OrdinalIgnoreCase ) ||
                    this.Name.StartsWith( "DateTime", StringComparison.OrdinalIgnoreCase ) ||
                    this.Name.StartsWith( "TimeSpan", StringComparison.OrdinalIgnoreCase ); 

                return isSystemType;
            }
        }

        public override bool IsValueType
        {
            get
            {
                return this.UnderlyingType.IsValueType;
            }
        }

        public DefinedType ParentProjectedType
        {
            get;
            set;
        }

        public override List<DefinedType> ChildTypes
        {
            get
            {
                if( this._childTypes == null )
                {
                    this._childTypes = this.UnderlyingType.NestedTypes.Select( t =>
                        TypeFactory.CreateAssemblyType( t ) as DefinedType ).ToList();
                }

                return this._childTypes;
            }
        }

                
        public override string ToString()
        {
            string toString = String.Format( "{0}.{1}", this.Namespace.FullName, this.Name );
            return toString;
        }


        public override TypeModel TypeSystem
        {
            get { return TypeModel.Assembly; }
        }

        public override LanguageElement LanguageElement
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

    }
}
