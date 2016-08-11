using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using OsgContentPublishing.ReferencePipelineLib.TypeTopology.Assembly;
using OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen;
using OsgContentPublishing.ReferencePipelineLib.TypeTopology.Managed;
using OsgContentPublishing.ReferencePipelineLib.TypeTopology.Native;
using OsgContentPublishing.ReferencePipelineLib.TypeTopology.Projected;
using ReflectionUtilities;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology
{
    /// <summary>
    /// Creates unique type instances from low-level deserialized objects, 
    /// like <see cref="DoxType"/> and <see cref="ObservableType"/>.
    /// </summary>
    /// <remarks>
    /// <para>Use <see cref="TypeFactory"/> to maintain a collection of all
    /// known types that are encountered during and after deserialization and 
    /// to create <see cref="DefinedType"/> wrappers around deserialized objects.</para>
    /// <para>Types from different deserialization streams are kept in 
    /// separate collections: types that come from Doxygen's XML output
    /// are kept in the <see cref="KnownTypes"/> collection, and types that
    /// come from a managed assembly are kept in the <see cref="KnownAssemblyTypes"/> 
    /// collection.</para>
    /// <para>Facade types originate from the Doxygen deserialization stream only.
    /// These are types about which nothing is known but their name. GTU makes
    /// an effort to resolve these types against the known-type collections; ideally,
    /// no facade types remain in a <see cref="ContentSet"/> after type resolution,
    /// but frequently, this goal is unachievable.
    /// </para>
    /// </remarks>
    public class TypeFactory
    {
        /// <summary>
        /// Gets the name of the global namespace.
        /// </summary>
        public static string GlobalNamespaceName
        {
            get
            {
                return _globalNamespaceName;
            }
        }

        /// <summary>
        /// Gets a collection of known generic type names, like List<T>.
        /// </summary>
        /// <remarks>
        /// <para>The collection of generic type names is populated in the 
        /// <see cref="InitializeGenericTypeNames"/> method.</para>
        /// </remarks>
        public static List<string> GenericTypeNames
        {
            get
            {
                if( _genericTypeNames == null )
                {
                    InitializeGenericTypeNames();
                }

                return _genericTypeNames;
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="ProjectedType"/> instances that are
        /// created by the <see cref="CreateProjectedType"/> method.
        /// </summary>
        public static Dictionary<string, ProjectedType> KnownProjectedTypes
        {
            get
            {
                if( TypeFactory._knownProjectedTypes == null )
                {
                    TypeFactory._knownProjectedTypes = new Dictionary<string, ProjectedType>();
                }

                return TypeFactory._knownProjectedTypes;
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="DefinedType"/> instances that are
        /// created by the <see cref="CreateType"/> method.
        /// </summary>
        /// <remarks>
        /// <para>this collection contains <see cref="DoxygenType"/> and 
        /// <see cref="DoxygenFacadeType"/> instances.</para>
        /// </remarks>
        public static Dictionary<string, DefinedType> KnownTypes
        {
            get
            {
                if( TypeFactory._knownTypes == null )
                {
                    TypeFactory.InitKnownTypes();
                }

                return TypeFactory._knownTypes;
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="AssemblyType"/> instances that are
        /// created by the <see cref="CreateAssemblyType"/> method.
        /// </summary>
        /// <remarks>
        /// <para>This collection contains <see cref="AssemblyType"/> instances
        /// that are created from managed assemblies and Windows metadata (.winmd)
        /// files.</para>
        /// </remarks>
        public static Dictionary<string, AssemblyType> KnownAssemblyTypes
        {
            get
            {
                if( TypeFactory._knownAssemblyTypes == null )
                {
                    TypeFactory.InitKnownAssemblyTypes();
                }

                return TypeFactory._knownAssemblyTypes;
            }
        }

        /// <summary>
        /// For future use.
        /// </summary>
        public static Dictionary<string, NativeType> KnownNativeTypes
        {
            get
            {
                if( TypeFactory._knownNativeTypes == null )
                {
                    TypeFactory._knownNativeTypes = new Dictionary<string, NativeType>();
                }

                return TypeFactory._knownNativeTypes;
            }
        }

        /// <summary>
        /// For future use.
        /// </summary>
        public static Dictionary<string, DoxType> KnownDoxTypes
        {
            get
            {
                if( TypeFactory._knownDoxTypes == null )
                {
                    TypeFactory._knownDoxTypes = new Dictionary<string, DoxType>();
                }

                return TypeFactory._knownDoxTypes;
            }
        }

        /// <summary>
        /// Initializes a facade type from the specified type name, or returns
        /// a known type, if it exists in the <see cref="KnownTypes"/> collection. 
        /// </summary>
        /// <param name="typeName">The name of the type to create.</param>
        /// <returns>A facade type that represents <paramref name="typeName"/>.</returns>
        public static DoxygenType CreateFacadeType( string typeName )
        {
            DoxygenFacadeType facadeType = null;

            if( TypeFactory.KnownTypes.ContainsKey( typeName ) )
            {
                facadeType = TypeFactory.KnownTypes[typeName] as DoxygenFacadeType;
            }
            else
            {
                facadeType = new DoxygenFacadeType( typeName );
                TypeFactory.KnownTypes.Add( typeName, facadeType );
            }

            return facadeType;
        }

        /// <summary>
        /// Initializes a <see cref="DefinedType"/> from the specified type name, or returns
        /// a known type, if it exists in the <see cref="KnownTypes"/> collection. 
        /// </summary>
        /// <param name="typeName">The name of the type to create.</param>
        /// <returns>A <see cref="DefinedType"/> that represents <paramref name="typeName"/>.</returns>
        /// <remarks>
        /// <para>If <paramref name="typeName"/> is null, or the empty string, <see cref="CreateType"/>
        /// returns the <see cref="DefinedType"/> that represents the global namespace.</para>
        /// </remarks>
        public static DefinedType CreateType( string typeName )
        {
            DefinedType definedType = null;

            if( !String.IsNullOrEmpty( typeName ) )
            {
                if( TypeFactory.KnownTypes.ContainsKey( typeName ) )
                {
                    definedType = TypeFactory.KnownTypes[typeName];
                }
                else if( TypeFactory.KnownTypes.ContainsKey( typeName.ToLower() ) )
                {
                    // TBD: Is this check still necessary?
                    definedType = TypeFactory.KnownTypes[typeName.ToLower()];
                }
                else
                {
                    // Parse the type declaration string.
                    TypeDeclarationParseResults parseResult = Utilities.ParseRawTypeDeclaration( typeName );
                    string parsedTypeName = parseResult.FullName;

                    if( TypeFactory.KnownTypes.ContainsKey( parsedTypeName ) )
                    {
                        definedType = TypeFactory.KnownTypes[parsedTypeName];
                    }
                    else
                    {
                        definedType = TypeFactory.CreateFacadeType( parsedTypeName );
                    }
                }
            }
            else
            {
                definedType = TypeFactory.KnownTypes[String.Empty];
            }

            return definedType;
        }

        /// <summary>
        /// Initializes a <see cref="DefinedType"/> from the specified <see cref="DoxType"/>, or returns
        /// a known type, if it exists in the <see cref="KnownTypes"/> collection.
        /// </summary>
        /// <param name="doxType">A deserialized Doxygen type.</param>
        /// <returns>A <see cref="DefinedType"/> that represents <paramref name="doxType"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="doxType"/> is null.</exception>
        /// <remarks><para> 
        /// </para></remarks>
        public static DefinedType CreateType( DoxType doxType )
        {
            DefinedType doxygenType = null;

            if( doxType != null )
            {
                if( TypeFactory.KnownTypes.ContainsKey( doxType.FullName ) )
                {
                    doxygenType = TypeFactory.KnownTypes[doxType.FullName];
                }
                else
                {
                    try
                    {
                        doxygenType = CreateNewType( doxType );
                        TypeFactory.KnownTypes.Add( doxygenType.FullName, doxygenType );
                    }
                    catch( Exception ex )
                    {
                        string msg = String.Format( "{0}: {1} / {2}", ex.Message, doxType.FullName, doxygenType.FullName );
                        Debug.WriteLine( msg );
                    }
                }
            }
            else
            {
                throw new ArgumentNullException( "doxType" );
            }

            return doxygenType;
        }

        /// <summary>
        /// Initializes a <see cref="DoxygenType"/> from the specified <see cref="DoxType"/>.
        /// </summary>
        /// <param name="doxType">A deserialized Doxygen compound.</param>
        /// <returns>A new <see cref="DoxygenType"/> instance that represents <paramref name="doxType"/>.</returns>
        /// <remarks>
        /// <para>The <see cref="CreateNewType"/> method creates a <see cref="DoxygenType"/> subclass 
        /// that corresponds with the kind of compound that <paramref name="doxType"/> represents.
        /// </para>
        /// </remarks>
        private static DoxygenType CreateNewType( DoxType doxType )
        {
            DoxygenType doxygenType = null;

            if( doxType.IsFacadeType )
            {
                doxygenType = new DoxygenFacadeType( doxType );
            }
            else if( doxType.IsEnum )
            {
                doxygenType = new DoxygenEnum( doxType );
            }
            else if( doxType.IsStruct )
            {
                doxygenType = new DoxygenStruct( doxType );
            }
            else if( doxType.IsDelegate )
            {
                Debug.WriteLine( "TBD: Delegate type" );
                //doxygenType = new ManagedDelegate( doxType );
            }
            else if( doxType.IsAttribute )
            {
                Debug.WriteLine( "TBD: Attribute type" );
                //doxygenType = new ManagedAttribute( doxType );
            }
            else if( doxType.IsNamespace )
            {
                doxygenType = new DoxygenNamespace( doxType );
            }
            else if( doxType.IsInterface )
            {
                doxygenType = new DoxygenInterface( doxType );
            }
            else if( doxType.IsClass )
            {
                doxygenType = new DoxygenClass( doxType );
            }
            else if( doxType.IsFunction )
            {       
                doxygenType = new DoxygenFunction( doxType );
            }

            if( doxType.ParentType == null )
            {
                Doxygen.GlobalNamespace global = TypeFactory.KnownTypes[String.Empty] as Doxygen.GlobalNamespace;
                doxygenType.Namespace = global;
                global.ChildTypes.Add( doxygenType );
            }

            return doxygenType;
        }

        /// <summary>
        /// Initializes a <see cref="ProjectedType"/> from the specified <see cref="NativeType"/> 
        /// and <see cref="AssemblyType"/>, or returns a known type, if it exists in the 
        /// <see cref="KnownProjectedTypes"/> collection.
        /// </summary>
        /// <param name="nativeType">A <see cref="DoxygenType"/> sublass.</param>
        /// <param name="assemblyType">A <see cref="AssemblyType"/> that corresponds with <paramref name="nativeType"/>.</param>
        /// <returns>A type that represents the projection of <paramref name="nativeType"/> into the 
        /// Application binary Interface (ABI).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="nativeType"/> or <paramref name="assemblyType"/> is null.</exception>
        public static ProjectedType CreateProjectedType( DefinedType nativeType, DefinedType assemblyType )
        {
            ProjectedType projectedType = null;

            if( nativeType != null && assemblyType != null )
            {
                // By convention, projected type gets its name from the assembly type.
                if( TypeFactory.KnownProjectedTypes.ContainsKey( assemblyType.FullName ) )
                {
                    projectedType = TypeFactory.KnownProjectedTypes[assemblyType.FullName];
                }
                else
                {
                    projectedType = TypeFactory.CreateNewProjectedType( nativeType as DoxygenType, assemblyType as AssemblyType );
                    TypeFactory.KnownProjectedTypes.Add( assemblyType.FullName, projectedType );
                }
            }
            else
            {
                throw new ArgumentNullException();
            }

            return projectedType;
        }

        /// <summary>
        /// Initializes a <see cref="ProjectedType"/> from the specified <see cref="NativeType"/> 
        /// and <see cref="AssemblyType"/>.
        /// </summary>
        /// <param name="nativeType">A <see cref="DoxygenType"/> sublass.</param>
        /// <param name="assemblyType">An <see cref="AssemblyType"/> that corresponds with <paramref name="nativeType"/>.</param>
        /// <returns>A type that represents the projection of <paramref name="nativeType"/> into the 
        /// Application binary Interface (ABI).</returns>
        private static ProjectedType CreateNewProjectedType( DoxygenType nativeType, AssemblyType assemblyType )
        {
            ProjectedType newProjectedType = null;

            if( assemblyType.IsNamespace )
            {
                newProjectedType = new ProjectedNamespace( nativeType, assemblyType );
            }
            else
            {
                newProjectedType = new ProjectedType( nativeType, assemblyType );
            }

            return newProjectedType;
        }

        /// <summary>
        /// Initializes an <see cref="AssemblyAttribute"/> from the specified attribute data, 
        /// or returns a known type, if it exists in the <see cref="KnownAssemblyTypes"/> collection. 
        /// </summary>
        /// <param name="attribute">The attribute data, represented by Reflection's 
        /// <see cref="CustomAttributeData"/> class.</param>
        /// <returns>An <see cref="AssemblyType"/> that represents <paramref name="attribute"/>.</returns>
        public static AssemblyType CreateAssemblyType( CustomAttributeData attribute )
        {
            return ( TypeFactory.CreateAssemblyType( new ObservableType( attribute.AttributeType ) ) );
        }

        /// <summary>
        /// Initializes an <see cref="AssemblyAttribute"/> from the specified <see cref="System.Type"/>,
        /// or returns a known type, if it exists in the <see cref="KnownAssemblyTypes"/> collection. 
        /// </summary>
        /// <param name="type">The type, as represented by Reflection.</param>
        /// <returns>An <see cref="AssemblyType"/> that represents <paramref name="type"/>.</returns>
        public static AssemblyType CreateAssemblyType( Type type )
        {
            return ( TypeFactory.CreateAssemblyType( new ObservableType( type ) ) );
        }

        /// <summary>
        /// Initializes an <see cref="AssemblyAttribute"/> from the specified <see cref="ObservableType"/>,
        /// or returns a known type, if it exists in the <see cref="KnownAssemblyTypes"/> collection. 
        /// </summary>
        /// <param name="observableType">An <see cref="ObservableType"/> wrapper around <see cref="System.Type"/>.</param>
        /// <returns>An <see cref="AssemblyType"/> that represents <paramref name="observableType"/>.</returns>
        public static AssemblyType CreateAssemblyType( ObservableType observableType )
        {
            AssemblyType assemblyType = null;

            if( observableType != null )
            {
                //string scrubbedFullName = Utilities.ScrubTypeString( observableType.FullName );
                string scrubbedFullName = Utilities.GetUndecoratedName( observableType );
                if( TypeFactory.KnownAssemblyTypes.ContainsKey( scrubbedFullName ) )
                {
                    assemblyType = TypeFactory.KnownAssemblyTypes[scrubbedFullName];
                }
                else
                {
                    assemblyType = TypeFactory.CreateNewAssemblyType( observableType );
                    TypeFactory.KnownAssemblyTypes.Add( scrubbedFullName, assemblyType );
                }
            }
            else
            {
                throw new ArgumentNullException( "observableType" );
            }

            return assemblyType;
        }

        /// <summary>
        /// Initializes an <see cref="AssemblyNamespace"/> from the specified name string,
        /// or returns a known type, if it exists in the <see cref="KnownAssemblyTypes"/> collection. 
        /// </summary>
        /// <param name="namespaceFullName">The full name of the namesapce.</param>
        /// <returns>An <see cref="AssemblyType"/> that represents <paramref name="namespaceFullName"/>.</returns>
        public static AssemblyType CreateAssemblyNamespaceType( string namespaceFullName )
        {
            AssemblyType assemblyType = null;

            if( namespaceFullName == null ||
                namespaceFullName == String.Empty )
            {
                assemblyType = TypeFactory.KnownAssemblyTypes[TypeFactory.GlobalNamespaceName];
            }
            else if( TypeFactory.KnownAssemblyTypes.ContainsKey( namespaceFullName ) )
            {
                assemblyType = TypeFactory.KnownAssemblyTypes[namespaceFullName];
            }
            else if( namespaceFullName == TypeFactory.GlobalNamespaceName )
            {
                assemblyType = new Assembly.GlobalNamespace( namespaceFullName );
                TypeFactory.KnownAssemblyTypes.Add( namespaceFullName, assemblyType );
            }
            else
            {
                assemblyType = new AssemblyNamespace( namespaceFullName );
                TypeFactory.KnownAssemblyTypes.Add( namespaceFullName, assemblyType );
            }

            return assemblyType;
        }

        /// <summary>
        /// Initializes an <see cref="AssemblyType"/> from the specified <see cref="ObservableType"/>.
        /// </summary>
        /// <param name="observableType">An <see cref="ObservableType"/> the represents a <see cref="System.Type"/>
        /// from a managed assembly or winmd file.</param>
        /// <returns>An <see cref="AssemblyType"/> that represents <paramref name="observableType"/>.</returns>
        private static AssemblyType CreateNewAssemblyType( ObservableType observableType )
        {
            AssemblyType assemblyType = null;

            try 
            {
                // For some reason, LMR may throw in the IsEnum property.
                if( observableType.IsEnum )
                {
                    assemblyType = new AssemblyEnum( observableType );
                }
            }
            catch( Exception ex )
            {
                Debug.WriteLine( "ObservableType.IsEnum threw" );
            }

            if( assemblyType == null )
            {
                if( observableType.IsStruct )
                {
                    assemblyType = new AssemblyStruct( observableType );
                }
                else if( observableType.IsDelegate )
                {
                    assemblyType = new AssemblyDelegate( observableType );
                }
                else if( observableType.IsAttribute )
                {
                    assemblyType = new AssemblyAttribute( observableType );
                }
                else if( observableType.IsInterface )
                {
                    assemblyType = new AssemblyInterface( observableType );
                }
                else if( observableType.IsClass )
                {
                    assemblyType = new AssemblyClass( observableType );
                }
                else
                {
                    throw new ArgumentException( "Unknown ObservableType", "observableType" );
                }
            }

            return assemblyType;
        }

        /// <summary>
        /// For future use.
        /// </summary>
        /// <param name="doxType">The deserialized Doxygen type.</param>
        /// <returns>A <see cref="DefinedType"/> that represents a type 
        /// from native code.</returns>
        public static NativeType CreateNativeType( DoxType doxType )
        {
            NativeType nativeType = null;

            if( doxType != null )
            {
                if( TypeFactory.KnownNativeTypes.ContainsKey( doxType.Name ) )
                {
                    nativeType = TypeFactory.KnownNativeTypes[doxType.Name];
                }
                else
                {
                    nativeType = TypeFactory.CreateNewNativeType( doxType );
                    TypeFactory.KnownNativeTypes.Add( nativeType.FullName, nativeType );
                }
            }
            else
            {
                throw new ArgumentNullException( "doxType" );
            }

            return nativeType;
        }
        
        /// <summary>
        /// Initializes a <see cref="NativeType"/> from the specified <see cref="DoxType"/>.
        /// </summary>
        /// <param name="doxType">A deserialized Doxygen type.</param>
        /// <returns>A <see cref="NativeType"/> that represents <paramref name="doxType"/>.</returns>
        private static NativeType CreateNewNativeType( DoxType doxType )
        {
            NativeType nativeType = null;

            if( doxType.IsEnum )
            {
                nativeType = new NativeEnum( doxType );
            }
            else if( doxType.IsStruct )
            {
                nativeType = new NativeStruct( doxType );
            }
            //else if( doxygenType.IsDelegate )
            //{
            //    nativeType = new NativeDelegate( doxygenType );
            //}
            //else if( doxygenType.IsAttribute )
            //{
            //    nativeType = new NativeAttribute( doxygenType );
            //}
            else if( doxType.IsInterface )
            {
                nativeType = new NativeInterface( doxType );
            }
            else if( doxType.IsClass )
            {
                nativeType = new NativeClass( doxType );
            }
            else
            {
                throw new ArgumentException( "Unknown DoxType", "doxType" );
            }

            return nativeType;
        }

        /// <summary>
        /// Deserializes a <see cref="DoxType"/> from the specified Doxygen <see cref="Compound"/>. 
        /// </summary>
        /// <param name="compound">The compound to deserialize from.</param>
        /// <returns>A <see cref="DoxType"/> that represents <paramref name="compound"/>.</returns>
        /// <remarks><para>Doxygen represents types by using three different schema elements,
        /// which GTU implements in the <see cref="Compound"/>, <see cref="CompoundRef"/>, and
        /// <see cref="MemberDef"/> classes. This means that the <see cref="TypeFactory"/> must
        /// create <see cref="DoxType"/> instances from three different deserialization paths.</para>
        /// </remarks>
        public static DoxType CreateDoxType( Compound compound )
        {
            DoxType doxType = null;

            if( compound.refid != null )
            {
                if( TypeFactory.KnownDoxTypes.ContainsKey( compound.refid ) )
                {
                    doxType = TypeFactory.KnownDoxTypes[compound.refid];
                }
                else
                {
                    try
                    {
                        doxType = new DoxType( compound );
                        TypeFactory.KnownDoxTypes.Add( doxType.id, doxType );
                    }
                    catch( Exception ex )
                    {
                        Debug.WriteLine( ex.ToString() ); ;
                    }
                }
            }
            else
            {
                throw new ArgumentNullException( "compoundRef.RawName" );
            }

            return doxType;
        }        
        
        /// <summary>
        /// Deserializes a <see cref="DoxType"/> from the specified Doxygen <see cref="MemberDef"/>
        /// and parents it to the specified parent type.
        /// </summary>
        /// <param name="memberDef">The <see cref="MemberDef"/> to deserialize from.</param>
        /// <param name="parentType">The parent type of the type represented by <paramref name="memberDef"/>.</param>
        /// <returns>A <see cref="DoxType"/> that represents <paramref name="memberDef"/>.</returns>
        /// <remarks><para>Doxygen represents types by using three different schema elements,
        /// which GTU implements in the <see cref="Compound"/>, <see cref="CompoundRef"/>, and
        /// <see cref="MemberDef"/> classes. This means that the <see cref="TypeFactory"/> must
        /// create <see cref="DoxType"/> instances from three different deserialization paths.</para>
        /// </remarks>
        public static DoxType CreateDoxType( MemberDef memberDef, DoxType parentType )
        {
            DoxType doxType = null;

            if( memberDef.id != null )
            {
                if( TypeFactory.KnownDoxTypes.ContainsKey( memberDef.id ) )
                {
                    doxType = TypeFactory.KnownDoxTypes[memberDef.id];
                }
                else
                {
                    try
                    {
                        doxType = new DoxType( memberDef, parentType );
                        TypeFactory.KnownDoxTypes.Add( doxType.id, parentType );
                    }
                    catch( Exception ex )
                    {
                        Debug.WriteLine( ex.ToString() ); ;
                    }
                }
            }
            else
            {
                throw new ArgumentNullException( "memberDef" );
            }

            return doxType;            
        }

        /// <summary>
        /// Deserializes a <see cref="DoxType"/> from the specified Doxygen <see cref="CompoundRef"/>.
        /// </summary>
        /// <param name="compoundRef">The <see cref="CompoundRef"/> to deserialize from.</param>
        /// <returns>A <see cref="DoxType"/> that represents <paramref name="compoundRef"/>.</returns>
        /// <remarks><para>Doxygen represents types by using three different schema elements,
        /// which GTU implements in the <see cref="Compound"/>, <see cref="CompoundRef"/>, and
        /// <see cref="MemberDef"/> classes. This means that the <see cref="TypeFactory"/> must
        /// create <see cref="DoxType"/> instances from three different deserialization paths.</para>
        /// </remarks>
        public static DoxType CreateDoxType( CompoundRef compoundRef )
        {
            return CreateDoxType( compoundRef, null );
        }

        /// <summary>
        /// Deserializes a <see cref="DoxType"/> from the specified Doxygen <see cref="CompoundRef"/>
        /// and parents it to the specified parent type.
        /// </summary>
        /// <param name="compoundRef">The <see cref="CompoundRef"/> to deserialize from.</param>
        /// <param name="parentType">The parent type of the type represented by <paramref name="compoundRef"/>.</param>
        /// <returns>A <see cref="DoxType"/> that represents <paramref name="compoundRef"/>.</returns>
        /// <remarks><para>Doxygen represents types by using three different schema elements,
        /// which GTU implements in the <see cref="Compound"/>, <see cref="CompoundRef"/>, and
        /// <see cref="MemberDef"/> classes. This means that the <see cref="TypeFactory"/> must
        /// create <see cref="DoxType"/> instances from three different deserialization paths.</para>
        /// </remarks>
        public static DoxType CreateDoxType( CompoundRef compoundRef, DoxType parentType )
        {
            DoxType doxType = null;

            if( compoundRef.refid != null )
            {
                if( TypeFactory.KnownDoxTypes.ContainsKey( compoundRef.refid ) )
                {
                    doxType = TypeFactory.KnownDoxTypes[compoundRef.refid];
                }
                else
                {
                    try
                    {
                        doxType = new DoxType( compoundRef, parentType );
                        TypeFactory.KnownDoxTypes.Add( doxType.id, doxType );
                        if( parentType != null )
                        {
                            parentType.ChildTypes.Add( doxType );
                        }
                    }
                    catch( Exception ex )
                    {
                        Debug.WriteLine( ex.ToString() ); ;
                    }
                }
            }
            else
            {
                TypeDeclarationParseResults parseResults = Utilities.ParseRawTypeDeclaration( compoundRef.RawName );
                string typeName = parseResults.FullName;

                if( TypeFactory.KnownDoxTypes.ContainsKey( typeName ) )
                {
                    doxType = TypeFactory.KnownDoxTypes[typeName];
                }
                else
                {
                    try
                    {
                        doxType = new DoxType( compoundRef, parentType );
                        TypeFactory.KnownDoxTypes.Add( typeName, doxType );
                        if( parentType != null )
                        {
                            parentType.ChildTypes.Add( doxType );
                        }
                    }
                    catch( Exception ex )
                    {
                        Debug.WriteLine( ex.ToString() ); ;
                    }
                }
            }

            return doxType;

        }

        /// <summary>
        /// Creates the <see cref="KnownTypes"/> collection and populates it with
        /// primitive and system types.
        /// </summary>
        /// <remarks><para>The collection is populated initially by calling the 
        /// <see cref="PrePopulateKnownTypes"/> method.</para>
        /// </remarks>
        private static void InitKnownTypes()
        {
            TypeFactory._knownTypes = new Dictionary<string, DefinedType>();

            TypeFactory.PrePopulateKnownTypes();
        }

        /// <summary>
        /// Creates the <see cref="KnownAssemblyTypes"/> collection and populates it with
        /// primitive and system types.
        /// </summary>
        /// <remarks><para>The collection is populated initially by calling the 
        /// <see cref="PrePopulateKnownAssemblyTypes"/> method.</para>
        /// </remarks>
        private static void InitKnownAssemblyTypes()
        {
            TypeFactory._knownAssemblyTypes = new Dictionary<string, AssemblyType>();

            TypeFactory.PrePopulateKnownAssemblyTypes();
        }

        /// <summary>
        /// Populates the <see cref="KnownTypes"/> collection with 
        /// primitive and system types.
        /// </summary>
        private static void PrePopulateKnownTypes()
        {
            // Create the global namespace.
            Doxygen.GlobalNamespace globalNamespace = new Doxygen.GlobalNamespace( null );
            TypeFactory._knownTypes.Add( String.Empty, globalNamespace );

            // Add primitive types.
            foreach( var kvp in PrimitiveTypes.KnownPrimitiveTypes )
            {
                TypeFactory._knownTypes.Add( kvp.Key, kvp.Value );
            }

            foreach( var kvp in SystemTypes.KnownSystemTypes )
            {
                TypeFactory._knownTypes.Add( kvp.Key, kvp.Value );
            }
        }

        /// <summary>
        /// Populates the <see cref="KnownAssemblyTypes"/> collection with 
        /// primitive and system types.
        /// </summary>
        /// <remarks><para>Currently, only the global namespace is added.</para>
        /// </remarks>
        private static void PrePopulateKnownAssemblyTypes()
        {
            // Create the global namespace.
            TypeFactory.CreateAssemblyNamespaceType( TypeFactory.GlobalNamespaceName );
        }

        private static void InitializeGenericTypeNames()
        {
            _genericTypeNames = new List<string>();
            _genericTypeNames.Add( "EventHandler" );
            _genericTypeNames.Add( "List" );
            _genericTypeNames.Add( "Dictionary" );
            _genericTypeNames.Add( "IEnumerable" );
            _genericTypeNames.Add( "ReadOnlyCollection" );
            _genericTypeNames.Add( "IEquatable" );
            _genericTypeNames.Add( "Action" );
            _genericTypeNames.Add( "Func" );
            _genericTypeNames.Add( "SingletonBehavior" );
        }

        private static Dictionary<string, DoxType> _knownDoxTypes;
        private static Dictionary<string, DefinedType> _knownTypes;
        private static Dictionary<string, AssemblyType> _knownAssemblyTypes;
        private static Dictionary<string, NativeType> _knownNativeTypes;
        private static Dictionary<string, ProjectedType> _knownProjectedTypes;

        private static string _globalNamespaceName = "GLOBAL";

        static List<string> _genericTypeNames;

    }


    ///////////////////////////////////////////////////////////////////////
    #region Uniqueness Testing

    // If anything goes wrong with uniqueness testing this is the place to look. 

    /// <summary>
    /// Compares two <see cref="DefinedType"/> instances.
    /// </summary>
    /// <remarks><para>You should perform all tests for equality 
    /// between <see cref="DefinedType"/> instances by using the
    /// <see cref="DefinedTypeComparer"/> class. This ensures that
    /// comparisons are consistent; ad-hoc comparisons on type name
    /// are likely to fail in hard-to-diagnose ways.</para>
    /// <para>Typical uses of the <see cref="DefinedTypeComparer"/> class
    /// are joins between sets of types from different sources, 
    /// for example in the <see cref="RidlDeserializer"/>.
    /// </para>
    /// </remarks>
    public class DefinedTypeComparer : IEqualityComparer<DefinedType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefinedTypeComparer"/> class.
        /// </summary>
        public DefinedTypeComparer()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefinedTypeComparer"/> class that
        /// supports type comparisons on name only, instead of the types' full names.
        /// </summary>
        /// <param name="enableLooseTypecomparisons">true to compare types on name only; 
        /// false to compare on <see cref="DefinedType.FullName"/>.</param>
        public DefinedTypeComparer( bool enableLooseTypecomparisons )
        {
            this.EnableLooseTypeComparisons = enableLooseTypecomparisons;
        }

        /// <summary>
        /// Gets a value indicating whether the comparison is on name only, 
        /// instead of the types' full names.
        /// </summary>
        public bool EnableLooseTypeComparisons
        {
            get;
            private set;
        }

        /// <summary>
        /// Compares two <see cref="DefinedType"/> instances.
        /// </summary>
        /// <param name="lhs">The left-hand type to compare.</param>
        /// <param name="rhs">the right-hand type to compare.</param>
        /// <returns>true, if the types are semantically equivalent; otherwise, false.</returns>
        public bool Equals( DefinedType lhs, DefinedType rhs )
        {
            // Check whether the compared objects reference the same data.
            if( Object.ReferenceEquals( lhs, rhs ) )
            {
                return true;
            }

            //Check whether either of the compared objects is null.
            if( Object.ReferenceEquals( lhs, null ) ||
                Object.ReferenceEquals( rhs, null ) )
            {
                return false;
            }

            //if( lhs.DeclaringType != rhs.DeclaringType )
            //{
            //    return false;
            //}

            if( lhs.FullName != rhs.FullName )
            {
                if( this.EnableLooseTypeComparisons )
                {
                    if( lhs.Name == rhs.Name )
                    {
                        return true;
                    }
                }

                return false;
            }

            //if( lhs.PropertyType != rhs.PropertyType )
            //{
            //    return false;
            //}

            return true;
        }

        public bool Equals( DefinedType lhs, string rhs )
        {
            //Check whether either of the compared objects is null.
            if( Object.ReferenceEquals( lhs, null ) ||
                Object.ReferenceEquals( rhs, null ) )
            {
                return false;
            }

            if( lhs.FullName != rhs )
            {
                if( this.EnableLooseTypeComparisons )
                {
                    if( lhs.Name == rhs )
                    {
                        return true;
                    }
                }

                return false;
            }

            return true;
        }

        /// <summary>
        /// Computes a hash code for the specified <see cref="DefinedType"/>.
        /// </summary>
        /// <param name="definedType">The type to compute a hash code for.</param>
        /// <returns>The hash code.</returns>
        public int GetHashCode( DefinedType definedType )
        {
            //Check whether the object is null
            if( Object.ReferenceEquals( definedType, null ) ) return 0;

            //Calculate the hash code for the property.
            int hashCode = definedType.FullName.GetHashCode();

            ////Get hash code for the Name field if it is not null.
            //int hashPropertyName = pi.Name == null ? 0 : pi.Name.GetHashCode();

            ////Calculate the hash code for the property.
            //int hashCode = hashPropertyName.GetHashCode();

            //int propertyTypeHashCode = pi.PropertyType.Name.GetHashCode();
            //hashCode ^= propertyTypeHashCode;

            ////int declaringTypeHashCode = mi.DeclaringType.Name.GetHashCode();
            ////hashCode ^= declaringTypeHashCode;

            return hashCode;
        }

    }

    class DoxTypeComparer : IEqualityComparer<DoxType>
    {
        public bool Equals( DoxType lhs, DoxType rhs )
        {
            // Check whether the compared objects reference the same data.
            if( Object.ReferenceEquals( lhs, rhs ) )
            {
                return true;
            }

            //Check whether either of the compared objects is null.
            if( Object.ReferenceEquals( lhs, null ) ||
                Object.ReferenceEquals( rhs, null ) )
            {
                return false;
            }

            if( lhs.FullName != rhs.FullName )
            {
                return false;
            }

            return true;
        }

        public int GetHashCode( DoxType doxType )
        {
            //Check whether the object is null
            if( Object.ReferenceEquals( doxType, null ) ) return 0;

            //Calculate the hash code for the property.
            int hashCode = doxType.FullName.GetHashCode();

            ////Get hash code for the Name field if it is not null.
            //int hashPropertyName = pi.Name == null ? 0 : pi.Name.GetHashCode();

            ////Calculate the hash code for the property.
            //int hashCode = hashPropertyName.GetHashCode();

            //int propertyTypeHashCode = pi.PropertyType.Name.GetHashCode();
            //hashCode ^= propertyTypeHashCode;

            ////int declaringTypeHashCode = mi.DeclaringType.Name.GetHashCode();
            ////hashCode ^= declaringTypeHashCode;

            return hashCode;
        }

    }

    /// <summary>
    /// For future use.
    /// </summary>
    class PropertyInfoComparer : IEqualityComparer<PropertyInfo>
    {
        public bool Equals( PropertyInfo lhs, PropertyInfo rhs )
        {
            // Check whether the compared objects reference the same data.
            if( Object.ReferenceEquals( lhs, rhs ) )
            {
                return true;
            }

            //Check whether either of the compared objects is null.
            if( Object.ReferenceEquals( lhs, null ) ||
                Object.ReferenceEquals( rhs, null ) )
            {
                return false;
            }

            //if( lhs.DeclaringType != rhs.DeclaringType )
            //{
            //    return false;
            //}

            if( lhs.Name != rhs.Name )
            {
                return false;
            }

            if( lhs.PropertyType != rhs.PropertyType )
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Computes a hash code for the specified <see cref="PropertyInfo"/>.
        /// </summary>
        /// <param name="pi">The <see cref="PropertyInfo"/> to hash.</param>
        /// <returns>The hash code.</returns>
        /// <remarks><para>If Equals() returns true for a pair of objects, the
        /// GetHashCode method must return the same value for these objects.</para>
        /// </remarks>
        public int GetHashCode( PropertyInfo pi )
        {
            //Check whether the object is null
            if( Object.ReferenceEquals( pi, null ) ) return 0;

            //Get hash code for the Name field if it is not null.
            int hashPropertyName = pi.Name == null ? 0 : pi.Name.GetHashCode();

            //Calculate the hash code for the property.
            int hashCode = hashPropertyName.GetHashCode();

            int propertyTypeHashCode = pi.PropertyType.Name.GetHashCode();
            hashCode ^= propertyTypeHashCode;

            //int declaringTypeHashCode = mi.DeclaringType.Name.GetHashCode();
            //hashCode ^= declaringTypeHashCode;

            return hashCode;
        }
    }

    /// <summary>
    /// For future use.
    /// </summary>
    class MethodInfoComparer : IEqualityComparer<MethodInfo>
    {
        public bool Equals( MethodInfo lhs, MethodInfo rhs )
        {
            // Check whether the compared objects reference the same data.
            if( Object.ReferenceEquals( lhs, rhs ) )
            {
                return true;
            }

            //Check whether either of the compared objects is null.
            if( Object.ReferenceEquals( lhs, null ) ||
                Object.ReferenceEquals( rhs, null ) )
            {
                return false;
            }

            if( lhs.ReturnType != rhs.ReturnType )
            {
                return false;
            }

            //if( lhs.DeclaringType != rhs.DeclaringType )
            //{
            //    return false;
            //}

            ParameterInfo[] lhsParams = lhs.GetParameters();
            ParameterInfo[] rhsParams = rhs.GetParameters();

            if( lhsParams.Length == 0 &&
                rhsParams.Length == 0 )
            {
                return true;
            }

            if( lhsParams.Length != rhsParams.Length )
            {
                return false;
            }

            int intersection = lhsParams.Intersect( rhsParams, new ParameterInfoComparer() ).Count();

            if( intersection != lhsParams.Length )
            {
                return false;
            }

            return true;
        }

        // If Equals() returns true for a pair of objects, the
        // GetHashCode method must return the same value for these objects.
        public int GetHashCode( MethodInfo mi )
        {
            int hashCode = TypeUtilities.GetMethodHashCode( mi );

            return hashCode;
        }
    }

    /// <summary>
    /// Compares two <see cref="ParameterInfo"/> instances.
    /// </summary>
    public class ParameterInfoComparer : IEqualityComparer<ParameterInfo>
    {
        public bool Equals( ParameterInfo lhs, ParameterInfo rhs )
        {
            //Check whether the compared objects reference the same data.
            if( Object.ReferenceEquals( lhs, rhs ) )
            {
                return true;
            }

            //Check whether any of the compared objects is null.
            if( Object.ReferenceEquals( lhs, null ) || Object.ReferenceEquals( rhs, null ) )
            {
                return false;
            }

            if( lhs.Position != rhs.Position )
            {
                return false;
            }

            //if( lhs.Name != rhs.Name )
            //{
            //    return false;
            //}

            if( lhs.ParameterType != rhs.ParameterType )
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Computes a hash code for the specified <see cref="ParameterInfo"/>.
        /// </summary>
        /// <param name="pi">The <see cref="ParameterInfo"/> to hash.</param>
        /// <returns>The hash code.</returns>
        /// <remarks><para>If Equals() returns true for a pair of objects, the
        /// GetHashCode method must return the same value for these objects.</para>
        /// </remarks>
        public int GetHashCode( ParameterInfo pi )
        {
            return TypeUtilities.GetParameterHashCode( pi );
        }
    }

    /// <summary>
    /// For future use.
    /// </summary>
    class EventInfoComparer : IEqualityComparer<EventInfo>
    {
        public bool Equals( EventInfo lhs, EventInfo rhs )
        {
            // Check whether the compared objects reference the same data.
            if( Object.ReferenceEquals( lhs, rhs ) )
            {
                return true;
            }

            //Check whether either of the compared objects is null.
            if( Object.ReferenceEquals( lhs, null ) ||
                Object.ReferenceEquals( rhs, null ) )
            {
                return false;
            }

            //if( lhs.DeclaringType != rhs.DeclaringType )
            //{
            //    return false;
            //}

            if( lhs.Name != rhs.Name )
            {
                return false;
            }

            if( lhs.EventHandlerType != rhs.EventHandlerType )
            {
                return false;
            }

            return true;
        }

        // If Equals() returns true for a pair of objects, the
        // GetHashCode method must return the same value for these objects.
        public int GetHashCode( EventInfo ei )
        {
            //Check whether the object is null
            if( Object.ReferenceEquals( ei, null ) ) return 0;

            //Get hash code for the Name field if it is not null.
            int hashEventName = ei.Name == null ? 0 : ei.Name.GetHashCode();

            //Calculate the hash code for the Event.
            int hashCode = hashEventName.GetHashCode();

            int EventTypeHashCode = ei.EventHandlerType.Name.GetHashCode();
            hashCode ^= EventTypeHashCode;

            //int declaringTypeHashCode = ei.DeclaringType.Name.GetHashCode();
            //hashCode ^= declaringTypeHashCode;

            return hashCode;
        }
    }

    #endregion

}
