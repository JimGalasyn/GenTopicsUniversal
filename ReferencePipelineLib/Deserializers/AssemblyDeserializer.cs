using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using OsgContentPublishing.ReferencePipelineLib.TypeTopology;
using OsgContentPublishing.ReferencePipelineLib.TypeTopology.Assembly;
using ReflectionUtilities;

namespace OsgContentPublishing.ReferencePipelineLib.Deserializers
{
    public class AssemblyDeserializer : Deserializer
    {
        public AssemblyDeserializer( 
            string inputFolder, 
            string assemblyPath, 
            List<string> namespaces )
            : base( inputFolder, namespaces )
        {
            this.AssignAssemblyPath( assemblyPath );
        }

        public AssemblyDeserializer(
            string inputFolder,
            string assemblyPath,
            List<string> namespaces,
            bool enableLooseTypecomparisons )
            : base( inputFolder, namespaces, enableLooseTypecomparisons )
        {
            this.AssignAssemblyPath( assemblyPath );
        }

        private void AssignAssemblyPath( string assemblyPath )
        {
            if( assemblyPath != null && File.Exists( assemblyPath ) )
            {
                this.AssemblyPath = assemblyPath;
            }
            else
            {
                throw new FileNotFoundException( fileNotFoundExceptionMessage, assemblyPath );
            }
        }

        public string AssemblyPath
        {
            get;
            protected set;
        }

        public override List<DefinedType> Deserialize()
        {
            var assemblyTypes = this.LoadTypesFromAssembly( this.AssemblyPath );

            return assemblyTypes;
        }

        private List<DefinedType> LoadTypesFromAssembly( string assemblyPath )
        {
            List<DefinedType> definedTypes = null;

            // Load the metadata assembly (or any managed assembly).
            Assembly assembly = TypeUtilities.LoadMetadataAssemblyFromFile( assemblyPath );
           
            // Get all of the types in the assembly, including non-public types.
            List<ObservableType> types = TypeUtilities.GetObservableTypes( assembly, false );

            if( types != null && types.Count > 0 )
            {
                List<ObservableType> typesToCreate = null;

                List<string> requestedNamespaces = GetRequestedNamespaces( assembly );
                if( requestedNamespaces.Count > 0 )
                {
                    // Find all of the types that are in the requested namespaces
                    // or their child namespaces.
                    var join = types.Join(
                                 requestedNamespaces,
                                 t => t.Namespace,
                                 ns => ns,
                                 ( t, ns ) => t.Namespace.StartsWith( ns ) ? t : null );

                    typesToCreate = join.ToList();
                }
                else
                {
                    typesToCreate = types;
                }

                List<DefinedType> assemblyTypes = typesToCreate.Select( t => 
                    TypeFactory.CreateAssemblyType( t ) as DefinedType ).ToList();

                // Collect the AssemblyNamespace types and add them to the list.
                // This query triggers the lazy evaluation of the Namespace property 
                // in assembly types and causes AssemblyNamespaces to be created.
                // Must call ToList to evaulate before we do anything else!
                var namespaceTypes = assemblyTypes.Select( t => t.Namespace ).ToList();

                // That gets most, but not all of the namespaces. 
                var parentNamespaces = namespaceTypes.Select( ns => ns.Namespace ).ToList(); 
                
                assemblyTypes.AddRange( namespaceTypes );
                assemblyTypes.AddRange( parentNamespaces );

                DefinedTypeComparer comparer = new DefinedTypeComparer();
                var assemblyTypesDistinct = assemblyTypes.Distinct( comparer ).ToList();

                definedTypes = Utilities.GetFlatList( assemblyTypesDistinct );
                //definedTypes = flatList.Distinct( comparer ).ToList();
            }
            else
            {
                throw new ArgumentException( "No types found in assembly", "assemblyPath" );
            }

            return definedTypes;
        }

        private List<string> GetRequestedNamespaces( Assembly assembly )
        { 
            List<string> assemblyNamespaces = TypeUtilities.GetNamespaces( assembly );

            List<string> requestedNamespaces = new List<string>();

            foreach( string requestedNamespace in this.Namespaces )
            {
                if( requestedNamespace.EndsWith( "*" ) )
                {
                    string requestedNamespaceMinusStar = requestedNamespace.Replace( ".*", String.Empty );
                    requestedNamespaceMinusStar = requestedNamespaceMinusStar.Replace( "*", String.Empty );
                    var namespaces = assemblyNamespaces.Where( n => n.StartsWith( requestedNamespaceMinusStar ) ).ToList();
                    requestedNamespaces.AddRange( namespaces );
                }
                else
                {
                    var ns = assemblyNamespaces.FirstOrDefault( n => n == requestedNamespace );
                    if( ns != null )
                    {
                        requestedNamespaces.Add( ns );
                    }
                }
            }

            // TBD: something like http://solutionizing.net/2009/09/19/hacking-linq-expressions-join-with-comparer/
            //NamespaceComparer comparer = new NamespaceComparer();
            //var requestedNamespaces = assemblyNamespaces.Join(
            //    this.Namespaces,
            //    an => an,
            //    rn => rn,
            //    ( an, rn ) => an,
            //    comparer );

            return requestedNamespaces;
        }

        private List<ObservableType> GetTypesInNamespace( string ns, Assembly metadataAssembly )
        {
            List<ObservableType> allTypes = GetObservableTypes( metadataAssembly );

            return ( GetTypesInNamespace( ns, allTypes ) );
        }

        private List<ObservableType> GetTypesInNamespace( string ns, List<ObservableType> allTypes )
        {
            List<ObservableType> typesInNamespace = allTypes.FindAll( ot => ot.Namespace == ns );

            return typesInNamespace;
        }

        private List<ObservableType> GetObservableTypes( Assembly managedAssembly )
        {
            if( managedAssembly != null )
            {
                Type[] types = managedAssembly.GetTypes();
                List<ObservableType> observableTypes = types.Select( t => new ObservableType( t ) ).ToList();

                //var publicTypes = types.Where( t => t.IsPublic );
                //List<ObservableType> observableTypes = publicTypes.Select( t => new ObservableType( t ) ).ToList();
                return observableTypes;
            }
            else
            {
                throw new ArgumentNullException( "managedAssembly", "must be assigned" );
            }
        }

        //internal class NamespaceComparer : EqualityComparer<string>
        //{
        //    public override bool Equals( string lhsNamespace, string rhsNamespace )
        //    {
        //        return lhsNamespace.StartsWith( rhsNamespace );
        //    }

        //    public override int GetHashCode( string obj )
        //    {
        //        return obj == null ? 0 : obj.GetHashCode();
        //    }
        //}

        private const string fileNotFoundExceptionMessage = "Must be assigned and must be accessible from this account";
    }
}
