using OsgContentPublishing.ReferencePipelineLib.TypeTopology;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

/// Contains classes that create types from serialized sources,
/// like Doxygen's XML output, managed assemblies, and
/// Windows Metadata (winmd) files.
namespace OsgContentPublishing.ReferencePipelineLib.Deserializers
{
    /// <summary>
    /// Represents a class that reads type info from a serialized store,
    /// like Doxygen's XML output, managed assemblies, and
    /// Windows Metadata (winmd) files.
    /// </summary>
    public abstract class Deserializer
    {
        public Deserializer( string inputFolder, List<string> namespaces )
        {
            this.Initialize( inputFolder );
            this.Namespaces = namespaces;
        }

        public Deserializer( 
            string inputFolder, 
            List<string> namespaces, 
            bool enableLooseTypecomparisons )
        {
            this.Initialize( inputFolder );
            this.Namespaces = namespaces;
            this.EnableLooseTypeComparisons = enableLooseTypecomparisons;
        }

        /// <summary>
        /// Gets the folder on the file system to read serialized type info from.
        /// </summary>
        public string InputFolder 
        { 
            get; 
            protected set; 
        }
        
        /// <summary>
        /// Gets the namespaces that the deserializer searches for 
        /// during deserialization.
        /// </summary>
        /// <remarks><para>Deserializers can use the <see cref="Namespaces"/> property
        /// to filter a type graph to the set of types that are of interest to a 
        /// content set.</para>
        /// </remarks>
        public List<string> Namespaces 
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets a value indicating whether type comparisons use the types' names
        /// only, or if comparisons match on the types' full names.
        /// </summary>
        public bool EnableLooseTypeComparisons
        {
            get;
            protected set;
        }

        private void Initialize( string inputFolder )
        {
            if( Directory.Exists( inputFolder ) )
            {
                this.InputFolder = inputFolder;
            }
            else
            {
                throw new ArgumentException( "Path does not exist", "inputFolder" );
            }
        }

        /// <summary>
        /// When overriden in a derived class, performs the deserialization operation.
        /// </summary>
        /// <returns></returns>
        public abstract List<DefinedType> Deserialize();
    }
}
