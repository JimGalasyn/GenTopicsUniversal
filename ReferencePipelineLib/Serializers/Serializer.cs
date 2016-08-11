using OsgContentPublishing.ReferencePipelineLib.TypeTopology;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// Contains classes that that emit output based on queries to the
/// the in-memory database of types, in the TypeFactory's 
/// Known*Types collections.
namespace OsgContentPublishing.ReferencePipelineLib.Serializers
{
    public abstract class Serializer
    {
        public Serializer( List<DefinedType> definedTypes, string outputFolder )
        {
            this.DefinedTypes = definedTypes;
            this.OutputFolder = outputFolder;
        }

        public Serializer( List<DefinedType> definedTypes, string outputFolder, List<string> namespaces )
        {
            this.DefinedTypes = definedTypes;
            this.OutputFolder = outputFolder;
            this.Namespaces = namespaces;
        }

        public Serializer( 
            List<DefinedType> definedTypes, 
            List<DefinedType> knownTypes, 
            string outputFolder, 
            List<string> namespaces )
        {
            this.DefinedTypes = definedTypes;
            this.KnownTypes = knownTypes;
            this.OutputFolder = outputFolder;
            this.Namespaces = namespaces;
        }

        public virtual void Serialize()
        {
        }

        public virtual void Serialize( List<DefinedType> knownTypes )
        {
            this.KnownTypes = knownTypes;
        }


        public List<DefinedType> DefinedTypes
        {
            get;
            protected set;
        }

        public List<DefinedType> KnownTypes
        {
            get;
            protected set;
        }

        public string OutputFolder 
        { 
            get;
            protected set;
        }

        public List<string> Namespaces
        {
            get;
            protected set;
        }
    }
}
