using OsgContentPublishing.ReferencePipelineLib.TypeTopology;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OsgContentPublishing.ReferencePipelineLib.Serializers
{
    class IntellisenseSerializer : Serializer
    {
        public IntellisenseSerializer( List<DefinedType> definedTypes, string outputFolder ) 
            : base( definedTypes, outputFolder )
        {

        }

        public IntellisenseSerializer( 
            string assemblyName, 
            List<DefinedType> definedTypes, 
            string inputFileName, 
            string outputFolder, 
            string intelliSensefileName )
            : base( definedTypes, outputFolder )
        {
            this.AssemblyName = assemblyName;
            this.TypesToSerialize = definedTypes;
            this.InputFileName = inputFileName;
            this.IntelliSenseFileName = intelliSensefileName;
        }

        public string AssemblyName
        {
            get;
            private set;
        }

        public List<DefinedType> TypesToSerialize
        {
            get;
            private set;
        }

        public string InputFileName
        {
            get;
            private set;
        }

        public string IntelliSenseFileName
        {
            get;
            private set;
        }

        public string IntelliSenseFullPath
        {
            get
            {
                return Path.Combine( this.OutputFolder, this.IntelliSenseFileName );
            }
        }


        public override void Serialize()
        {
            this.SerializeToIntellisenseFile();
        }

        private XDocument SerializeToIntellisense( XDocument intellisenseDoc )
        {
            //string intellisenseTemplatePath = Path.Combine( "Templates", MdUtilities.Utilities.intellisenseTemplateFileName );
            //string intelliSenseTemplateString = String.Format( _intelliSenseFileTemplate, this.AssemblyName );
            //XDocument intellisenseDoc = XDocument.Load( intelliSenseTemplateString );
            XElement docElement = intellisenseDoc.Document.Element( "doc" );
            XElement membersElement = docElement.Element( "members" );

            foreach( DefinedType definedType in this.DefinedTypes )
            {
                if( definedType.TopicId != null )
                {
                    XElement typeElement = new XElement( "member", new XAttribute( "name", definedType.TopicId ) );
                    membersElement.Add( typeElement );

                    XElement summaryElement = new XElement( "summary", definedType.Content.Abstract );
                    typeElement.Add( summaryElement );

                    if( !definedType.IsNamespace )
                    {
                        foreach( DefinedMember member in definedType.Members )
                        {
                            string topicId = member.TopicId == null ? String.Empty : member.TopicId;
                            XElement memberElement = new XElement( "member", new XAttribute( "name", topicId ) );
                            membersElement.Add( memberElement );

                            XElement memberSummaryElement = new XElement( "summary", member.Content.Abstract );
                            memberElement.Add( memberSummaryElement );

                            if( member.IsProperty )
                            {
                                XElement memberReturnsElement = new XElement( "returns", Utilities.GetFriendlyName( member.Type ) );
                                memberElement.Add( memberReturnsElement );
                            }

                            if( member.IsMethod || member.IsConstructor )
                            {
                                // TBD: return value
                                //XElement memberReturnsElement = new XElement( "returns", member.type ); // TBD: GetFriendlyName
                                //memberElement.Add( memberReturnsElement );

                                foreach( DefinedParameter param in member.Parameters )
                                {
                                    XElement paramElement = new XElement( "param", new XAttribute( "name", param.Name ) );
                                    paramElement.Value = param.Content.Abstract;
                                    memberElement.Add( paramElement );
                                }
                            }
                        }
                    }
                }
            }

            return intellisenseDoc;
        }


        private XDocument SerializeToIntellisenseFile()
        {
            XDocument inputDoc = null;            
            if( File.Exists( this.InputFileName ) )
            {
                inputDoc = XDocument.Load( this.InputFileName );
            }
            else
            {
                string intelliSenseTemplateString = String.Format( _intelliSenseFileTemplate, this.AssemblyName );

                inputDoc = XDocument.Parse( intelliSenseTemplateString );
            }

            XDocument intellisenseDoc = this.SerializeToIntellisense( inputDoc );
            
            //XElement docElement = inputDoc.Document.Element( "doc" );
            //XElement membersElement = docElement.Element( "members" );

            //var newMembers = intellisenseDoc.Descendants( "member" );

            //membersElement.Add( newMembers );
            //outputDoc = inputDoc;

            //outputDoc.Save( this.IntelliSenseFullPath );

            intellisenseDoc.Save( this.IntelliSenseFullPath );
            return intellisenseDoc;
        }

        const string _defaultIntelliSenseFileName = "intellisense.xml";
        const string _intelliSenseFileTemplate = 
            "<?xml version='1.0' encoding='utf-8'?><doc><assembly><name>{0}</name></assembly><members/></doc>";
    }
}
