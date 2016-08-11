using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using OsgContentPublishing.ReferencePipelineLib.TypeTopology;
using OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen;
using OsgContentPublishing.ReferencePipelineLib.TypeTopology.Managed;
using ReflectionUtilities;
using OsgContentPublishing.ReferencePipelineLib.TypeTopology.Projected;
using OsgContentPublishing.ReferencePipelineLib.TypeTopology.Assembly;

namespace OsgContentPublishing.ReferencePipelineLib.Deserializers
{
    public class RidlDeserializer : Deserializer
    {
        public RidlDeserializer( string inputFolder, string schemaPath, string winmdPath, List<string> namespaces )
            : base( inputFolder, namespaces )
        {
            // TBD: Just one inputFolder?
            this.AssemblyDeserializer = new AssemblyDeserializer( inputFolder, winmdPath, this.Namespaces );
            this.NativeDeserializer = new NativeDeserializer( inputFolder, schemaPath, this.Namespaces );
        }

        public RidlDeserializer( 
            string inputFolder, 
            string schemaPath, 
            string winmdPath, 
            List<string> namespaces,
            bool enableLooseTypecomparisons )
            : base( inputFolder, namespaces, enableLooseTypecomparisons )
        {
            this.AssemblyDeserializer = new AssemblyDeserializer( 
                inputFolder, 
                winmdPath, 
                this.Namespaces, 
                enableLooseTypecomparisons );
            
            this.NativeDeserializer = new NativeDeserializer( 
                inputFolder, 
                schemaPath, 
                this.Namespaces, 
                enableLooseTypecomparisons );
        }

        private AssemblyDeserializer AssemblyDeserializer
        {
            get;
            set;
        }

        private NativeDeserializer NativeDeserializer
        {
            get;
            set;
        }

        public string WinmdPath
        {
            get;
            private set;
        }

        public override List<DefinedType> Deserialize()
        {
            var nativeTypes = this.NativeDeserializer.Deserialize();
            var assemblyTypes = this.AssemblyDeserializer.Deserialize();

            // Find all of the native types that match assembly types on the FullName property.
            var join = assemblyTypes.Join(
                    nativeTypes,
                    at => this.EnableLooseTypeComparisons ? at.Name : at.FullName,
                    nt => this.EnableLooseTypeComparisons ? nt.Name : nt.FullName,
                    ( at, nt ) => at );

            DefinedTypeComparer comparer = new DefinedTypeComparer( this.EnableLooseTypeComparisons );

            var newProjectedTypes = join.Select( t =>
                TypeFactory.CreateProjectedType( nativeTypes.Find( nt => comparer.Equals( nt, t ) ), t ) as DefinedType );

            this.DisjointSet = nativeTypes.Except( assemblyTypes, comparer ).ToList();

            // TBD: resolve disjoint types in winmd assembly, by namespace
            var disjointNamespaces = this.DisjointSet.Select( t => t.Namespace.Name ).Distinct();

            var projectedTypesList = newProjectedTypes.OrderBy( p => p.Namespace.FullName ).ToList();
            ResolveContent( projectedTypesList );

            //foreach( DoxType doxType in mergedTypes )
            //{
            //    doxType.ResolveContent( nativeTypes );
            //    doxType.FindLinks( mergedTypes );
            //}

            return projectedTypesList;
        }

        public List<DefinedType> DisjointSet
        {
            get;
            private set;
        }

        //public 

        private DefinedType ResolveFacadeType( DoxygenFacadeType facadeType )
        {
            DefinedType resolvedType = null;

            string typeName = facadeType.Name;

            if( !typeName.Contains( ' ' ) )
            {
                // Probably a type without a namespace qualifier. 
                var candidateKnownTypes = TypeFactory.KnownTypes.Where( kvp =>
                    !( kvp.Value is DoxygenFacadeType ) && kvp.Key.Contains( typeName ) );

                if( candidateKnownTypes != null )
                {
                    var candidateKnownTypeList = candidateKnownTypes.ToList();
                    if( candidateKnownTypeList.Count > 0 )
                    {
                        if( candidateKnownTypeList.Count == 1 )
                        {
                            var candidateKnownType = candidateKnownTypeList[0];
                            resolvedType = TypeFactory.KnownTypes[candidateKnownType.Key];
                        }
                        else
                        {
                            // TBD: Do something smarter
                            // Not a namespace, match with shortest string length
                            var candidateKnownTypeListOrdered = candidateKnownTypeList.OrderBy( kvp => kvp.Key.Length ).ToList();
                            var candidateKnownType = candidateKnownTypeListOrdered.Find( kvp => !kvp.Value.IsNamespace );
                            resolvedType = TypeFactory.KnownTypes[candidateKnownType.Key];
                        }
                    }
                }
            }
            else
            {
                // Probably a namespace-qualified type, but with a munged namespace.
                // e.g., spaces instead of dots or colons.
                string typeNameWithDots = typeName.Replace( ' ', '.' );
                if( TypeFactory.KnownTypes.ContainsKey( typeNameWithDots ) )
                {
                    resolvedType = TypeFactory.KnownTypes[typeNameWithDots];
                }
            }

            return resolvedType;
        }

        private List<ObservableType> LoadProjectedTypes()
        {
            List<ObservableType> managedTypes = new List<ObservableType>();

            // Load the metadata assembly (or any managed assembly).
            Assembly assembly = TypeUtilities.LoadMetadataAssemblyFromFile( this.WinmdPath );

            // Get all of the types in the assembly.
            List<ObservableType> types = TypeUtilities.GetObservableTypes( assembly );

            foreach( string ns in this.Namespaces.OrderBy( ns => ns ) )
            {
                List<ObservableType> typesInNamespace = GetTypesInNamespace( ns, assembly );
                managedTypes.AddRange( typesInNamespace );
            }

            return managedTypes;
        }

        public void ResolveContent( List<DefinedType> projectedTypes )
        {
            // Collect all of members from the base types of this type.

            foreach( ProjectedType projectedType in projectedTypes )
            {
                List<DefinedMember> allBaseMembers = new List<DefinedMember>();

                // TBD: DefinedType.HasBaseTypes
                if( projectedType.BaseTypes != null && projectedType.BaseTypes.Count > 0 )
                {
                    foreach( var baseType in projectedType.BaseTypes )
                    {
                        allBaseMembers.AddRange( baseType.Members );
                    }

                    var memberComparer = new Utilities.DefinedMemberComparer();

                    foreach( DefinedType baseType in projectedType.BaseTypes )
                    {
                        // Copy descriptions from the base members into this type's members.            
                        foreach( DefinedMember member in projectedType.Members )
                        {
                            if( member.Content.IsEmpty )
                            {
                                DefinedMember baseMember = allBaseMembers.FirstOrDefault( m =>
                                        memberComparer.Equals( m, member ) );
                                if( baseMember != null )
                                {
                                    if( baseMember.Content.HasContent )
                                    {
                                        member.Content = baseMember.Content;
                                    }

                                    if( member.IsMethod || member.IsConstructor )
                                    {
                                        foreach( AssemblyParameter param in ( (AssemblyMethod)member ).Parameters )
                                        {
                                            if( param.Content.IsEmpty )
                                            {
                                                foreach( AssemblyParameter baseParam in ( (AssemblyMethod)baseMember ).Parameters )
                                                {
                                                    if( param.Name == baseParam.Name )
                                                    {
                                                        if( baseParam.Content.HasContent )
                                                        {
                                                            param.Content = baseParam.Content;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                                if( member.IsConstructor && member.Content.IsEmpty )
                                {
                                    // Insert constructor boilerplate.
                                    string ctorDescription = String.Format(
                                            "Initializes an instance of the {0} class",
                                            projectedType.Name );

                                    if( member.HasParameters )
                                    {
                                        ctorDescription += " with the specified ";
                                        for( int i = 0; i < member.Parameters.Count; i++ )
                                        {
                                            DefinedParameter param = member.Parameters[i];
                                            string paramTypeName = param.Type.Name;
                                            ctorDescription += paramTypeName;

                                            if( member.Parameters.Count >= 2 )
                                            {
                                                if( i == member.Parameters.Count - 2 )
                                                {
                                                    ctorDescription += " and ";
                                                }
                                                else if( i < member.Parameters.Count - 1 )
                                                {
                                                    ctorDescription += ", ";
                                                }

                                            }
                                        }
                                    }

                                    ctorDescription += ".";

                                    member.Content.Abstract = ctorDescription;
                                }
                            }
                        }
                    }
                }
            }
        }

        //public void FindLinks( List<DoxType> doxygenTypes )
        //{
        //    List<Description> descriptions = new List<Description>();

        //    // Get descriptions for the type.
        //    descriptions.Add( this.BriefDescription );
        //    descriptions.Add( this.DetailedDescription );

        //    // Get descriptions for the type's members.
        //    var memberBriefDescriptions = this.MemberDefs.Select( m => m.BriefDescription );
        //    var memberDetailedDescriptions = this.MemberDefs.Select( m => m.DetailedDescription );
        //    descriptions.AddRange( memberBriefDescriptions );
        //    descriptions.AddRange( memberDetailedDescriptions );

        //    foreach( var member in this.MemberDefs )
        //    {
        //        descriptions.Add( member.BriefDescription );
        //        descriptions.Add( member.DetailedDescription );

        //        if( member.IsMethod || member.IsConstructor )
        //        {
        //            var paramDescriptions = member.Parameters.Select( p => p.BriefDescription );
        //            descriptions.AddRange( paramDescriptions );

        //            //foreach( var param in member.Parameters )
        //            //{
        //            //    var paramType = mergedTypes.FirstOrDefault( t => t.Name == param.type );
        //            //    if( paramType != null )
        //            //    {
        //            //        param.DoxType = paramType;
        //            //    }
        //            //}
        //        }
        //    }

        //    var filteredDescriptions = descriptions.Where( d => d != null && d.Desc != String.Empty );

        //    foreach( var description in filteredDescriptions )
        //    {
        //        description.ResolveLinks( this, doxygenTypes );
        //    }
        //}



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

        private List<ObservableType> GetObservableTypes( Assembly metadataAssembly )
        {
            if( metadataAssembly != null )
            {
                Type[] types = metadataAssembly.GetTypes();
                List<ObservableType> observableTypes = types.Select( t => new ObservableType( t ) ).ToList();

                //var publicTypes = types.Where( t => t.IsPublic );
                //List<ObservableType> observableTypes = publicTypes.Select( t => new ObservableType( t ) ).ToList();
                return observableTypes;
            }
            else
            {
                throw new ArgumentNullException( "metadataAssembly", "must be assigned" );
            }
        }

        const string emptyDescriptionNagString = "TBD";

    }
}
