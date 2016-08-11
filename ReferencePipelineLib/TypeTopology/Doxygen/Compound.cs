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
    /// Represents a Doxygen compounddef element.
    /// </summary>
    public class Compound
    {
        /// <summary>
        /// Initializes a new <see cref="Compound"/> instance to 
        /// the specified <see cref="XElement"/>.
        /// </summary>
        /// <param name="element">The XML element to deserializes from.</param>
        /// <param name="inputFolder">The file system folder that contains the 
        /// Doxygen XML file.</param>
        /// <remarks><para>
        /// </para>
        /// </remarks>
        public Compound( XElement element, string inputFolder )
        {
            XAttribute refidAttr = element.Attribute( "refid" );
            refid = refidAttr.Value;

            XAttribute kindAttr = element.Attribute( "kind" );
            kind = kindAttr.Value;

            name = Utilities.GetChildElement( element, "name" ).Value;

            this.SourceFile = Path.ChangeExtension( refid, "xml" );
            this.SourceFile = Path.Combine( inputFolder, this.SourceFile );

            this.MemberElements = Utilities.GetChildElements( element, "member" );
            this.Members = this.MemberElements.Select( me => new Member( me, this ) ).ToList();
        }

        /// <summary>
        /// Gets the member elements that are contained in the compounddef.
        /// </summary>
        public List<XElement> MemberElements
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a collection of <see cref="Member"/> instances that corresponds with
        /// the <see cref="MemberElements"/> collection.
        /// </summary>
        public List<Member> Members
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
        /// Gets the unique identifier for the compounddef.
        /// </summary>
        public string refid
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the kind of language element represented by the compounddef.
        /// </summary>
        public string kind
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the name of the compounddef.
        /// </summary>
        public string name
        {
            get;
            private set;
        }

        /// <summary>
        /// For future use.
        /// </summary>
        public bool IsWrlType
        {
            get
            {
                return this.name.Contains( "WrlFinal" );
            }
        }

        public override string ToString()
        {
            string toString = String.Format( "{0} | {1}", this.name, this.kind );
            return toString;
        }
    }
}
