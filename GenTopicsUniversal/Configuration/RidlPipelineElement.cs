using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsgContentPublishing.GenTopicsUniversal.Configuration
{
    public class RidlPipelineElement : ConfigurationElement
    {
        [ConfigurationProperty( "pipelineType", DefaultValue = "RidlPipeline", IsRequired = true )]
        [StringValidator( InvalidCharacters = "~!@#$%^&*()[]{}/;'\"|\\", MinLength = 1, MaxLength = 60 )]
        public String PipelineType
        {
            get
            {
                return (String)this["pipelineType"];
            }
            set
            {
                this["pipelineType"] = value;
            }
        }

        [ConfigurationProperty( "inputFolderPath", IsRequired = true, IsKey = false )]
        [StringValidator( InvalidCharacters = "~!@#$%^&*()[]{}/;'\"|", MaxLength = max_path )]
        public string InputFolderPath
        {
            get
            {
                return (string)this["inputFolderPath"];
            }
            set
            {
                this["inputFolderPath"] = value;
            }
        }

        [ConfigurationProperty( "outputFolderPath", IsRequired = true, IsKey = false )]
        [StringValidator( InvalidCharacters = "~!@#$%^&*()[]{}/;'\"|", MaxLength = max_path )]
        public string OutputFolderPath
        {
            get
            {
                return (string)this["outputFolderPath"];
            }
            set
            {
                this["outputFolderPath"] = value;
            }
        }

        [ConfigurationProperty( "winmdFilePath", IsRequired = true, IsKey = false )]
        [StringValidator( InvalidCharacters = "~!@#$%^&*()[]{}/;'\"|", MaxLength = max_path )]
        public string WinmdFilePath
        {
            get
            {
                return (string)this["winmdFilePath"];
            }
            set
            {
                this["winmdFilePath"] = value;
            }
        }

        const int max_path = 260;
    }
}
