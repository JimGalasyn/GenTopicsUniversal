using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OsgContentPublishing.ReferencePipelineLib.TypeTopology;

namespace OsgContentPublishing.GenTopicsUniversal.Configuration
{
    /// <summary>
    /// Represents a configuration element for a <see cref="ContentSet"/>'s 
    /// API reference generation pipeline.
    /// </summary>
    public class PipelineElement : ConfigurationElement
    {
        /// <summary>
        /// Gets or sets the name of the content set that the current pipeline is a member of.
        /// </summary>
        [ConfigurationProperty( "contentSet", DefaultValue = "Name this content set",
            IsRequired = true, IsKey = false )]
        public string ContentSet
        {
            get
            {
                return (string)this["contentSet"];
            }
            set
            {
                this["contentSet"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the title of the pipeline's emitted output, as it
        /// appears in the content set's index.
        /// </summary>
        [ConfigurationProperty( "indexTitle", DefaultValue = "Provide a title for this pipeline",
            IsRequired = true, IsKey = false )]
        public string IndexTitle
        {
            get
            {
                return (string)this["indexTitle"];
            }
            set
            {
                this["indexTitle"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the current pipeline's name, as specified in configuration.
        /// </summary>
        [ConfigurationProperty( "name", DefaultValue = "Name this pipeline",
            IsRequired = true, IsKey = true )]
        public string Name
        {
            get
            {
                return (string)this["name"];
            }
            set
            {
                this["name"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the type of current pipeline, as specified in configuration.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the fully qualified path to the input folder for native types.
        /// </summary>
        /// <remarks><para>Used by the <see cref="GeneratedDeserializer"/>. </para>
        /// </remarks>
        [ConfigurationProperty( "nativeFolderPath", IsRequired = false, IsKey = false )]
        [StringValidator( InvalidCharacters = "~!@#$%^&*()[]{}/;'\"|", MaxLength = max_path )]
        public string NativeFolderPath
        {
            get
            {
                return (string)this["nativeFolderPath"];
            }
            set
            {
                this["nativeFolderPath"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the fully qualified path to the input folder, which contains the
        /// serialized output from a tool like Doxygen.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the fully qualified path to the folder that receives
        /// output from the pipeline.
        /// </summary>
        /// <remarks><para>Frequently, this is the path to the server folder.
        /// </para>
        /// </remarks>
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

        /// <summary>
        /// Gets or sets the fully qualified path to the Windows Metadata (.winmd) file
        /// that contains types to be documented.
        /// </summary>
        [ConfigurationProperty( "winmdFilePath", IsRequired = false, IsKey = false )]
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

        /// <summary>
        /// Gets or sets the fully qualified path to the managed assembly file
        /// that contains types to be documented.
        /// </summary>
        [ConfigurationProperty( "assemblyFilePath", IsRequired = false, IsKey = false )]
        [StringValidator( InvalidCharacters = "~!@#$%^&*()[]{}/;'\"|", MaxLength = max_path )]
        public string AssemblyFilePath
        {
            get
            {
                return (string)this["assemblyFilePath"];
            }
            set
            {
                this["assemblyFilePath"] = value;
            }
        }

        /// <summary>
        /// Gets the server root path as it appears in URLs for types.
        /// </summary>
        [ConfigurationProperty( "siteConfigReferenceRoot", IsRequired = true, IsKey = false )]
        [StringValidator( InvalidCharacters = "~!@#$%^&*()[]{};'\"|", MaxLength = max_path )]
        public string SiteConfigReferenceRoot
        {
            get
            {
                return (string)this["siteConfigReferenceRoot"];
            }
            set
            {
                this["siteConfigReferenceRoot"] = value;
            }
        }

        /// <summary>
        /// Gets the namespaces that deserializer search for 
        /// during deserialization.
        /// </summary>
        [ConfigurationProperty( "namespaces", IsDefaultCollection = true, IsRequired = false )]
        [ConfigurationCollection( typeof( NameValueConfigurationCollection ),
            AddItemName = "add",
            ClearItemsName = "clear",
            RemoveItemName = "remove" )]
        public NameValueConfigurationCollection Namespaces
        {
            get
            {
                NameValueConfigurationCollection namespacesCollection =
                    (NameValueConfigurationCollection)base["namespaces"];
                return namespacesCollection;
            }
        }

        /// <summary>
        /// Gets a value indicating whether type comparisons use the types' names
        /// only, or if comparisons match on the types' full names.
        /// </summary>
        [ConfigurationProperty( "enableLooseTypeComparisons", IsRequired = false, IsKey = false )]
        public bool EnableLooseTypeComparisons
        {
            get
            {
                return (bool)this["enableLooseTypeComparisons"];
            }
            set
            {
                this["enableLooseTypeComparisons"] = value;
            }
        }

        /// <summary>
        /// Specifies the maximum path length on Windows file systems.
        /// </summary>
        protected const int max_path = 260;
    }
}
