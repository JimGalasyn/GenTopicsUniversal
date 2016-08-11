using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OsgContentPublishing.ReferencePipelineLib.TypeTopology;
using System.Xml.Linq;
using OsgContentPublishing.ReferencePipelineLib;

namespace OsgContentPublishing.GenTopicsUniversal.Configuration
{
    public class PipelinesSection : ConfigurationSection
    {
        public PipelinesSection()
        {
        }

        public PipelinesSection( string fullPath )
        {
            this.LoadFromSatelliteConfigFile( fullPath );
        }


        [ConfigurationProperty( "pipelines", IsDefaultCollection = true )]
        [ConfigurationCollection( typeof( PipelinesCollection ),
            AddItemName = "add",
            ClearItemsName = "clear",
            RemoveItemName = "remove" )]
        public PipelinesCollection Pipelines
        {
            get
            {
                PipelinesCollection pipelinesCollection =
                    (PipelinesCollection)base["pipelines"];
                return pipelinesCollection;
            }
        }

        private void LoadFromSatelliteConfigFile( string fullPath )
        {
            XDocument doc = Utilities.LoadXml( fullPath );

            XElement pipelinesElement = doc.Document.Root.Element( "pipelines" );

            List<XElement> pipelineXmlElements = Utilities.GetChildElements( pipelinesElement, "add" );

            foreach( var pipelineXmlElement in pipelineXmlElements )
            {                
                string contentSet = pipelineXmlElement.Attribute( "contentSet" ).Value;
                string indexTitle = pipelineXmlElement.Attribute( "indexTitle" ).Value;
                string name = pipelineXmlElement.Attribute( "name" ).Value;
                string pipelineType = pipelineXmlElement.Attribute( "pipelineType" ).Value;
                string inputFolderPath = pipelineXmlElement.Attribute( "inputFolderPath" ).Value;
                string outputFolderPath = pipelineXmlElement.Attribute( "outputFolderPath" ).Value;
                string siteConfigReferenceRoot = pipelineXmlElement.Attribute( "siteConfigReferenceRoot" ).Value;
                string winmdFilePath = Utilities.TryGetAttributeValue( pipelineXmlElement, "winmdFilePath" );
                string assemblyPath = Utilities.TryGetAttributeValue( pipelineXmlElement, "assemblyPath" );

                PipelineElement pipelineElement = new PipelineElement();
                pipelineElement.AssemblyFilePath = assemblyPath;
                pipelineElement.ContentSet = contentSet;
                pipelineElement.IndexTitle = indexTitle;
                pipelineElement.InputFolderPath = inputFolderPath;
                pipelineElement.Name = name;
                pipelineElement.OutputFolderPath = outputFolderPath;
                pipelineElement.PipelineType = pipelineType;
                pipelineElement.SiteConfigReferenceRoot = siteConfigReferenceRoot;
                pipelineElement.WinmdFilePath = winmdFilePath;

                var namespacesXmlElement = Utilities.TryGetChildElement( pipelineXmlElement, "namespaces" );
                if( namespacesXmlElement != null )
                {
                    var namespaceXmlElements = Utilities.GetChildElements( namespacesXmlElement, "add" );
                    var namespaceNames = namespaceXmlElements.Select( e => 
                        new NameValueConfigurationElement( e.Attribute( "name" ).Value, null ) ).ToList();

                    foreach( var configElement in namespaceNames )
                    {
                        pipelineElement.Namespaces.Add( configElement );
                    }
                }

                this.Pipelines.Add( pipelineElement );
            }
        }
    }

    public class PipelinesCollection : ConfigurationElementCollection
    {
        public PipelinesCollection()
        {
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.AddRemoveClearMap;
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new PipelineElement();
        }

        protected override Object GetElementKey( ConfigurationElement element )
        {
            return ( (PipelineElement)element ).Name;
        }

        public PipelineElement this[int index]
        {
            get
            {
                return (PipelineElement)BaseGet( index );
            }
            set
            {
                if( BaseGet( index ) != null )
                {
                    BaseRemoveAt( index );
                }
                BaseAdd( index, value );
            }
        }

        new public PipelineElement this[string Name]
        {
            get
            {
                return (PipelineElement)BaseGet( Name );
            }
        }

        public int IndexOf( PipelineElement url )
        {
            return BaseIndexOf( url );
        }

        public void Add( PipelineElement url )
        {
            BaseAdd( url );
        }
        protected override void BaseAdd( ConfigurationElement element )
        {
            BaseAdd( element, false );
        }

        public void Remove( PipelineElement url )
        {
            if( BaseIndexOf( url ) >= 0 )
                BaseRemove( url.Name );
        }

        public void RemoveAt( int index )
        {
            BaseRemoveAt( index );
        }

        public void Remove( string name )
        {
            BaseRemove( name );
        }

        public void Clear()
        {
            BaseClear();
        }
    }
}