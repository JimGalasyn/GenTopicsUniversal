using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OsgContentPublishing.ReferencePipelineLib;
using OsgContentPublishing.ReferencePipelineLib.Deserializers;
using OsgContentPublishing.ReferencePipelineLib.Documentation;
using OsgContentPublishing.ReferencePipelineLib.Pipelines;
using OsgContentPublishing.ReferencePipelineLib.Serializers;
using OsgContentPublishing.ReferencePipelineLib.TypeTopology;

/// <summary>
/// APIs that are used exclusively for documenting GenTopicsUniversal (GTU).
/// </summary>
namespace OsgContentPublishing.GenTopicsUniversal.Documentation
{
    /// <summary>
    /// GenTopicsUniversal (GTU) is a command-line tool that works with Doxygen 
    /// to produce API reference documentation from source code comments. 
    /// </summary>
    /// <remarks>
    /// <para>Use GTU to produce documentation automatically from native C++ code, 
    /// C# code, or Windows Runtime (WinRT) IDL.</para>
    /// <para>Clone the "Content Tools" Git repo to get the GTU source code:
    /// "https://vstfpn.partners.extranet.microsoft.com:8443/tfs/PlatformNext/B/_git/Content Tools"
    /// </para>
    /// <para>Start GTU from the command line:</para>
    /// <para>     >GenTopicsUniversal.exe [optional path to config file]</para>
    /// <para>GTU is controlled by using configuration files. For more info, see the 
    /// <see cref="ConfigurationSection"/>.</para>
    /// </remarks>
    [DocumentationOnly( true, "GenTopicsUniversal Developer Notes" )]
    public class GenTopicsOverviewDocumentation
    {
        /// <summary>
        /// GenTopicsUniversal (GTU) is controlled by using satellite configuration 
        /// files that are referenced in the main app.config file.
        /// </summary>
        [DocumentationOnly( true, "Configuration" )]
        public int ConfigurationSection { get; set; }

        /// <summary>
        /// History and motivations for building GenTopicsUniversal.
        /// </summary>
        /// <remarks>
        /// <para>GenTopicsUniversal evolved from the earlier API reference generation  
        /// tool, named GenTopics, which was developed by Jim Galasyn (code) and 
        /// Julie Solon (schema) to document the original Windows Runtime (WinRT) APIs 
        /// in Windows 8. GenTopics generates stub topics from the main windows.winmd file. 
        /// These topics are XML files that conform to the Windows Developer Content Markup 
        /// Language (WDCML) schema.</para>
        /// <para>GenTopics enumerates all of the public types in windows.winmd 
        /// and emits topics that writers author into later. These topics are stored 
        /// in the OSG CPub source depot (SD) repository, and writers author content 
        /// into them by using XMetaL. Writers are required to keep the SD topics in sync 
        /// with source code changes manually, which is time-consuming and error-prone.
        /// GenTopics is limited to reading only winmd (Windows metadata) files and 
        /// generating only WDCML topic stubs. </para>
        /// <para>GenTopicsUniversal enables a different workflow by assuming that the 
        /// source code itself is the single "source of truth". Reference content is 
        /// authored by developers and/or writers directly in the source code, so there 
        /// is no intermediate store that needs to be synched with the source. </para>
        /// <para>GTU is designed to be scripted directly into product builds, so fresh 
        /// documentation is generated and placed with each build.</para>
        /// </remarks>
        [DocumentationOnly( true, "Background" )]
        public int BackgroundSection { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// <para>
        /// </para>
        /// <para>
        /// </para>
        /// <para>
        /// </para>
        /// <para>
        /// </para>
        /// </remarks>
        [DocumentationOnly( true, "Architecture" )]
        public int ArchitectureSection { get; set; }

        /// <summary>
        /// Make the ctor private, so it doesn't show up in docs.
        /// </summary>
        private GenTopicsOverviewDocumentation()
        {

        }
    }
}
