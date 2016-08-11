using OsgContentPublishing.ReferencePipelineLib.TypeTopology;
using OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen;
using OsgContentPublishing.ReferencePipelineLib.TypeTopology.Projected;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsgContentPublishing.ReferencePipelineLib.Deserializers
{
    public class GeneratedDeserializer : Deserializer
    {
        public GeneratedDeserializer(
            string nativeFolder,
            string inputFolder,
            string schemaPath,
            List<string> namespaces )
            : base( inputFolder, namespaces )
        {
            this.NativeDeserializer = new NativeDeserializer(
                nativeFolder,
                schemaPath,
                this.Namespaces );

            this.GeneratedInterfaceDeserializer = new DoxygenDeserializer(
                inputFolder,
                schemaPath,
                this.Namespaces );
        }

        public GeneratedDeserializer(
            string nativeFolder,
            string inputFolder,
            string schemaPath,
            List<string> namespaces,
            bool enableLooseTypecomparisons )
            : base( inputFolder, namespaces, enableLooseTypecomparisons )
        {
            this.NativeDeserializer = new NativeDeserializer(
                nativeFolder,
                schemaPath,
                this.Namespaces,
                enableLooseTypecomparisons );

            this.GeneratedInterfaceDeserializer = new DoxygenDeserializer(
                inputFolder,
                schemaPath,
                this.Namespaces,
                enableLooseTypecomparisons );
        }

        private DoxygenDeserializer GeneratedInterfaceDeserializer
        {
            get;
            set;
        }

        private NativeDeserializer NativeDeserializer
        {
            get;
            set;
        }

        public override List<DefinedType> Deserialize()
        {
            var nativeTypes = this.NativeDeserializer.Deserialize();
            var generatedTypes = this.GeneratedInterfaceDeserializer.Deserialize();

            // Find all of the native types that match assembly types on the FullName property.
            //var join = nativeTypes.Join(
            //        generatedTypes,
            //        nt => this.EnableLooseTypeComparisons ? nt.Name : nt.FullName,
            //        gt => this.EnableLooseTypeComparisons ? gt.Name : gt.FullName,
            //        ( nt, gt ) => nt );

            var join = generatedTypes.Join(
                    nativeTypes,
                    gt => this.EnableLooseTypeComparisons ? gt.Name : gt.FullName,
                    nt => this.EnableLooseTypeComparisons ? nt.Name : nt.FullName,
                    ( gt, nt ) => gt );

            DefinedTypeComparer comparer = new DefinedTypeComparer( this.EnableLooseTypeComparisons );

            foreach( var generatedType in join )
            {
                var sourceType = nativeTypes.Find( t => comparer.Equals( generatedType, t ) );
                if( sourceType != null )
                {
                    generatedType.CopyContent( sourceType );
                }
            }

            //var newGeneratedTypes = join.Select( t =>
            //    TypeFactory.CreateGeneratedType( nativeTypes.Find( nt => comparer.Equals( nt, t ) ), t ) as DefinedType );

            //var noNamespace = newGeneratedTypes.Where( t => t.Namespace == null );
            //var facadeTypes = newGeneratedTypes.Where( t => ( (GeneratedType)t ).GeneratedInterfaceType is DoxygenFacadeType );

            //var generatedTypesList = newGeneratedTypes.OrderBy( p => p.Namespace.FullName ).ToList();
            //ResolveContent( generatedTypesList );

            //return generatedTypesList;

            return join.ToList();
        }

        public List<DefinedType> DisjointSet
        {
            get;
            private set;
        }

    }
}
