using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen
{
    public class Para
    {
        public Para( XElement paraElement )
        {
            this.ParseParaElement( paraElement );
        }

        private void ParseParaElement( XElement paraElement )
        {
            var linknode = paraElement.Nodes().FirstOrDefault( n => 
                n.NodeType == XmlNodeType.Element );

            if( linknode == null )
            {
                this.Content = paraElement.Value;
            }
            else
            {
                string content = String.Empty;
                foreach( var node in paraElement.Nodes() )
                {
                    if( node is XText )
                    {
                        XText textNode = node as XText;
                        content += textNode.Value;
                    }

                    if( node is XElement )
                    {
                        XElement nodeElement = node as XElement;
                        if( nodeElement.Name == "ulink" )
                        {
                            string anchorString = nodeElement.ToString(); // Necessary to preserve markup tags
                            anchorString = anchorString.Replace( "ulink", "a" );
                            anchorString = anchorString.Replace( "url", "href" );
                            content += anchorString; 
                            this.EmbeddedLinks.Add( nodeElement.Attribute( "url" ).Value );

                            if( IsSampleLink( anchorString ) )
                            {
                                this.HasCodeSample = true;
                            }
                        }
                        else if( nodeElement.Name == "ref" )
                        {
                            string refValue = nodeElement.Value;
                            content += refValue;
                            if( !this.EmbeddedRefs.Contains( refValue ) )
                            {
                                this.EmbeddedRefs.Add( refValue );
                            }
                        }
                        else if( nodeElement.Name == "heading" )
                        {
                            content += nodeElement.Value;
                        }
                        else if( nodeElement.Name == "parameterlist" )
                        {
                            this.ParameterItems = ParseParameterList( nodeElement );
                        }
                        else if( nodeElement.Name == "emphasis" )
                        {
                            content += "<em>";
                            content += nodeElement.Value;
                            content += "</em>";
                        }
                        else if( nodeElement.Name == "code" )
                        {
                        }
                    }
                }

                this.Content = content;
            }
        }

        public string Content
        {
            get;
            set;
        }

        public List<string> EmbeddedRefs
        {
            get
            {
                if( this._embeddedRefs == null )
                {
                    this._embeddedRefs = new List<string>();
                }

                return this._embeddedRefs;
            }
        }

        public List<string> EmbeddedLinks
        {
            get
            {
                if( this._embeddedLinks == null )
                {
                    this._embeddedLinks = new List<string>();
                }

                return this._embeddedLinks;
            }
        }

        public bool HasCodeSample
        {
            get;
            private set;
        }


        public List<ParameterItem> ParameterItems
        {
            get;
            private set;
        }

        private bool IsSampleLink( string url )
        {
            return false;
        }

        // <parameterlist kind="param">
        //   <parameteritem>
        //     <parameternamelist>
        //       <parametername>sourceType</parametername>
        //     </parameternamelist>
        //     <parameterdescription>
        //       <para>The type to copy content from.</para>
        //     </parameterdescription>
        //   </parameteritem>
        // </parameterlist>



        private List<ParameterItem> ParseParameterList( XElement parameterListElement )
        {
            List<ParameterItem> items = new List<ParameterItem>();
            int position = 0;

            var parameterItemElements = parameterListElement.Elements( "parameteritem" );
            foreach( XElement parameterItemElement in parameterItemElements )
            {
                var parameterNameListElement = parameterItemElement.Element( "parameternamelist" );
                string parameterName = parameterNameListElement.Element( "parametername" ).Value;

                var parameterDescriptionElement = parameterItemElement.Element( "parameterdescription" );
                var parameterDescriptionParaElement = parameterDescriptionElement.Element( "para" );

                string parameterDescription = String.Empty;

                if( parameterDescriptionParaElement != null )
                {
                    parameterDescription = parameterDescriptionParaElement.Value;
                }

                ParameterItem item = new ParameterItem( parameterName, parameterDescription, position );
                items.Add( item );
                position++;
            }

            return items;
        }

        List<string> _embeddedLinks;
        List<string> _embeddedRefs;
    }
}
