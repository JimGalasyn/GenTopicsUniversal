using OsgContentPublishing.ReferencePipelineLib.TypeTopology;
using OsgContentPublishing.ReferencePipelineLib.TypeTopology.Assembly;
using OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen;
using OsgContentPublishing.ReferencePipelineLib.TypeTopology.Projected;
using ReflectionUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OsgContentPublishing.ReferencePipelineLib.Deserializers
{
    public class ManagedAssemblyDeserializer : Deserializer
    {
        public ManagedAssemblyDeserializer( string inputFolder, string schemaPath, string assemblyPath, List<string> namespaces )
            : base( inputFolder, namespaces )
        {   
            this.AssemblyDeserializer = new AssemblyDeserializer( inputFolder, assemblyPath, this.Namespaces );
            this.ManagedDeserializer = new ManagedDeserializer( inputFolder, schemaPath, this.Namespaces );
        }

        private AssemblyDeserializer AssemblyDeserializer
        {
            get;
            set;
        }

        private ManagedDeserializer ManagedDeserializer
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
            var managedTypes = this.ManagedDeserializer.Deserialize();
            var assemblyTypes = this.AssemblyDeserializer.Deserialize();

            //var join = assemblyTypes.Join(
            //        managedTypes,
            //        at => at.Name,
            //        nt => nt.Name,
            //        ( at, nt ) => at );

            // Find all of the native types that match assembly types on the FullName property.
            var join = assemblyTypes.Join(
                    managedTypes,
                    at => at.FullName,
                    nt => nt.FullName,
                    ( at, nt ) => at );

            DefinedTypeComparer comparer = new DefinedTypeComparer();

            var newProjectedTypes = join.Select( t => 
                TypeFactory.CreateProjectedType( managedTypes.Find( mt => comparer.Equals( mt, t ) ), t ) as DefinedType );

            this.DisjointSet = managedTypes.Except( assemblyTypes, comparer ).ToList();

            // TBD: resolve disjoint types in winmd assembly, by namespace
            var disjointNamespaces = this.DisjointSet.Select( t => t.Namespace.Name );

            var projectedTypesList = newProjectedTypes.ToList();
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
                            if( member.Content == null || 
                                member.Content.Description == emptyDescriptionNagString )
                            {
                                DefinedMember baseMember = allBaseMembers.FirstOrDefault( m => 
                                    memberComparer.Equals( m, member ) );
                                if( baseMember != null )
                                {
                                    member.Content = baseMember.Content;

                                    if( member.IsMethod || member.IsConstructor )
                                    {
                                        foreach( AssemblyParameter param in ((AssemblyMethod)member).Parameters )
                                        {
                                            if( param.Content == null || 
                                                param.Content.Description == emptyDescriptionNagString )
                                            {
                                                foreach( AssemblyParameter baseParam in ((AssemblyMethod)baseMember).Parameters )
                                                {
                                                    if( param.Name == baseParam.Name )
                                                    {
                                                        param.Content = baseParam.Content;
                                                    }
                                                }
                                            }
                                        }
                                    }
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
