using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen
{
    /// <summary>
    /// Represents a Doxygen compoundref element.
    /// </summary>
    public class CompoundRef
    {
        /// <summary>
        /// Initializes a new <see cref="CompoundRef"/> intance to the specified 
        /// XML element and deserialized Doxygen type.
        /// </summary>
        /// <param name="compoundRefElement">The element to deserialize.</param>
        /// <param name="parentType">The parent type of <paramref name="compoundRefElement"/>.</param>
        public CompoundRef( XElement compoundRefElement, DoxType parentType )
        {
            this.refid = Utilities.TryGetAttributeValue( compoundRefElement, "refid" );
            this.prot = Utilities.TryGetAttributeValue( compoundRefElement, "prot" );
            this.virt = Utilities.TryGetAttributeValue( compoundRefElement, "virt" );

            this.RawName = compoundRefElement.Value;

            this.ParentType = parentType;

            if( this.refid != null )
            {
                string sourceDir = Path.GetDirectoryName( ParentType.SourceFilePath );
                this.SourceFile = Path.ChangeExtension( this.refid, "xml" );
                this.SourceFile = Path.Combine( sourceDir, this.SourceFile );
            }
        }

        /// <summary>
        /// Gets the value of the compoundref element.
        /// </summary>
        public string RawName
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the parent type of the current compoundref.
        /// </summary>
        public DoxType ParentType
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the fully qualified path to the XML source document.
        /// </summary>
        public string SourceFile
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether the current compoundref 
        /// is an inner class.
        /// </summary>
        public bool IsInnerClass
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets a value indicating whether the current compoundref 
        /// is an inner namespace.
        /// </summary>
        public bool IsInnerNamespace
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets the unique identifier for the current compoundref.
        /// </summary>
        public string refid
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the value of the prot attribute for the current compoundref.
        /// </summary>
        public string prot
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the value of the virt attribute for the current compoundref.
        /// </summary>
        public string virt
        {
            get;
            private set;
        }

        public override string ToString()
        {
            string toString = String.Format( "{0} CompoundRef", this.RawName );
            return toString;
        }
    }
}
