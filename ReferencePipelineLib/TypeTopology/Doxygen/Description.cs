using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using OsgContentPublishing.ReferencePipelineLib.Deserializers;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen
{
    /// <summary>
    /// Represents the Doxygen briefdescription and detaileddescription elements.
    /// </summary>
    public class Description
    {
        /// <summary>
        /// Initializes a new <see cref="Description"/> instance to the
        /// specified XML element.
        /// </summary>
        /// <param name="element"></param>
        public Description( XElement element )
        {
            this.title = Utilities.TryGetChildElementValue( element, "title" );

            this.ParseParagraphs( element );

            if( this.Paragraphs != null && this.Paragraphs.Count > 0 )
            {
                Para firstPara = this.Paragraphs[0];
                if( firstPara.ParameterItems != null )
                {
                    this.ParametersPara = firstPara;
                    this.Paragraphs.RemoveAt( 0 );
                }

                if( this.Paragraphs.Count > 0 )
                {
                    this.Desc = this.Paragraphs[0].Content;
                    this.Paragraphs.RemoveAt( 0 );
                }
            }
            else
            {
                this.Desc = element.Value;
            }

            if( String.IsNullOrEmpty( this.Desc ) )
            {
                this.Desc = String.Empty;
            }
            else
            {
                this.Desc = this.Desc.Trim();
            }
        }

        public Description( string value )
        {
            this.Desc = value;
        }

        public string Desc
        {
            get;
            set;
        }

        public List<Para> Paragraphs
        {
            get;
            private set;
        }

        public Para ParametersPara
        {
            get;
            private set;
        }

        public bool HasParametersPara
        {
            get
            {
                return( this.ParametersPara != null );
            }
        }

        private void ParseParagraphs( XElement descElement )
        {
            var paraElements = Utilities.TryGetChildElements( descElement, "para" );
            if( paraElements != null )
            {
                this.Paragraphs = paraElements.Select( p => new Para( p ) ).ToList();
            }
        }

        public void ResolveLinks( DoxType type, List<DoxType> doxTypes )
        {
            // First, check for any references that the Description already knows about.
            foreach( var para in this.Paragraphs )
            {

                //string typeNameWithWhitespace = String.Format( " {0} ", type.Name ); 
                //bool typeIsEmbeddedRef = para.EmbeddedRefs.Contains( typeNameWithWhitespace );

                var referencedTypes = doxTypes.Where( t => ( t != type ) && para.EmbeddedRefs.Contains( t.Name ) );
                foreach( var referencedType in referencedTypes )
                {
                    //string link = Utilities.GetMarkdownLink( referencedType );
                    string link = DoxygenDeserializer.GetAnchor( referencedType );
                    // Terrible hack

                    string typeNameWithPeriod = String.Format( " {0}.", referencedType.Name );
                    string typeNamePossessive = String.Format( " {0}'s", referencedType.Name );
                    string typeNameWithWhitespace = String.Format( " {0} ", referencedType.Name );

                    string replaceStringWithPeriod = String.Format( " {0}.", link );
                    string replaceStringPossessive = String.Format( " {0}'s", link );
                    string replaceStringWithWhitespace = String.Format( " {0} ", link );

                    para.Content = para.Content.Replace( typeNameWithPeriod, replaceStringWithPeriod );
                    para.Content = para.Content.Replace( typeNamePossessive, replaceStringPossessive );
                    para.Content = para.Content.Replace( typeNameWithWhitespace, replaceStringWithWhitespace );
                }
            }

            if( this.Paragraphs.Count > 0 )
            {
                this.Desc = this.Paragraphs[0].Content;
            }
        }

        public string title
        {
            get;
            private set;
        }

        public override string ToString()
        {
            string toString = String.Format( "{0} {1}", this.title, this.Desc );
            return toString;
        }
    }
}
