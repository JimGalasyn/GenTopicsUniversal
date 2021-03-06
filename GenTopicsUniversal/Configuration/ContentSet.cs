﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OsgContentPublishing.EventLogging;
using OsgContentPublishing.ReferencePipelineLib;
using OsgContentPublishing.ReferencePipelineLib.Serializers;
using OsgContentPublishing.ReferencePipelineLib.TypeTopology;
using OsgContentPublishing.ReferencePipelineLib.Pipelines;
using OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen;
using OsgContentPublishing.ReferencePipelineLib.TypeTopology.Assembly;
using OsgContentPublishing.ReferencePipelineLib.TypeTopology.Projected;
using ReflectionUtilities;

/// Contains classes and configuration files for managing API reference
/// generation pipelines.
namespace OsgContentPublishing.GenTopicsUniversal.Configuration
{
    /// <summary>
    /// Represents a collection of related developer content. 
    /// </summary>
    /// <remarks>
    /// <para>A <see cref="ContentSet"/> is a collection of one or more <see cref="Pipeline"/> instances. 
    /// A pipeline deserializes Doxygen output from a set of source code into an in-memory 
    /// database built from <see cref="DefinedType"/> instances. This database is filtered, 
    /// sorted, and serialized to an endpoint, like HTML or IntelliSense.</para>
    /// <para>Pipelines in a content set may have dependencies on each other, which means that
    /// the content set must resolve types across all of its pipelines.</para>
    /// </remarks>
    public class ContentSet
    {
        /// <summary>
        /// INitializes a new instance of the <see cref="ContentSet"/> class.
        /// </summary>
        /// <param name="name">The name of the content set, usually assigned from configuration.</param>
        /// <param name="outputFolder">The fully qualified path to the folder that output is saved to.</param>
        /// <param name="siteConfigReferenceRoot">The root URL of the content, used for creating links.</param>
        public ContentSet( string name, string outputFolder, string siteConfigReferenceRoot )
        {
            this.Name = name;
            this.OutputFolder = outputFolder;
            this.SiteConfigReferenceRoot = siteConfigReferenceRoot;
        }

        /// <summary>
        /// Creates the content set's output.
        /// </summary>
        /// <returns>An integer value that represents the success code of the 
        /// <see cref="Generate"/> operation.</returns>
        /// <remarks>
        /// <para>The <see cref="Generate"/> method runs all of the pipelines 
        /// in the content set. If a pipelines fails, the method logs the error 
        /// and continues running other pipelines.</para>
        /// <para>After the <see cref="Pipeline.Deserialize"/> and <see cref="Pipeline.Serialize"/> 
        /// methods are called on each pipeline, the index for the content set is 
        /// generated by caling the <see cref="IndexSerializer.EmitIndex"/> method. The index
        /// spans all of the pipelines.</para>
        /// </remarks>
        public int Generate()
        {
            int retval = succeededRetVal;

            List<DefinedType> allTypesInContentSet = new List<DefinedType>();

            List<string> allRequestedNamespaces = new List<string>();

            foreach( Pipeline pipeline in this.Pipelines.Values )
            {
                allRequestedNamespaces.AddRange( pipeline.Namespaces );
            }

            foreach( var kvp in this.Pipelines )
            {
                Pipeline pipeline = kvp.Value;

                try
                {
                    List<DefinedType> deserializedTypes = pipeline.Deserialize();
                    this.IndexSerializer.Pipelines.Add( pipeline.Name, pipeline );
                    allTypesInContentSet.AddRange( deserializedTypes );
                }
                catch( Exception ex )
                {
                    string errorMessage = String.Format( 
                        "{0} failed with error {1}", 
                        pipeline.Name, 
                        ex.ToString() );
                    Debug.WriteLine( errorMessage );
                    GenTopicsEventLogger.Log.LogError( errorMessage );
                    retval = failedRetVal;
                }
            }

            var allTypesOrdered = Utilities.GetFlatList( allTypesInContentSet ); 
            List<DefinedType> resolvedTypes = Utilities.ResolveTypes( allTypesOrdered );

            foreach( var kvp in this.Pipelines )
            {
                Pipeline pipeline = kvp.Value;

                try
                {
                    pipeline.Serialize( resolvedTypes );

                    string msg = String.Format( 
                        "{0} completed successfully", 
                        pipeline.Name );
                    Debug.WriteLine( msg );
                    GenTopicsEventLogger.Log.LogInformational( msg );
                }
                catch( Exception ex )
                {
                    string errorMessage = String.Format( 
                        "{0} failed with error {1}", 
                        pipeline.Name, 
                        ex.ToString() );
                    Debug.WriteLine( errorMessage );
                    GenTopicsEventLogger.Log.LogError( errorMessage );
                    retval = failedRetVal;
                }
            }

            this.IndexSerializer.EmitIndex( this.Name, resolvedTypes );

            return retval;
        }

        /// <summary>
        /// Gets the name of the content set, as specified in a configuration file.
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the output folder in the file system, as specified in a configuration file.
        /// </summary>
        public string OutputFolder
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the server root path as it appears in URLs for types.
        /// </summary>
        public string SiteConfigReferenceRoot
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the serializer that emits the index for the content set.
        /// </summary>
        public IndexSerializer IndexSerializer
        {
            get
            {
                if( this._indexSerializer == null )
                {
                    this._indexSerializer = new IndexSerializer(
                        this.OutputFolder,
                        this.SiteConfigReferenceRoot );
                }

                return this._indexSerializer;
            }
        }

        /// <summary>
        /// Gets a collection of pipelines that comprise the content set.
        /// </summary>
        public Dictionary<string, Pipeline> Pipelines
        {
            get
            {
                if( this._pipelines == null )
                {
                    this._pipelines = new Dictionary<string, Pipeline>();
                }

                return this._pipelines;
            }
        }

        public override string ToString()
        {
            string msg = String.Format( "{0} content set", this.Name );
            return msg;
        }

        /// <summary>
        /// Backs the <see cref="Pipelines"/> property.
        /// </summary>
        private Dictionary<string, Pipeline> _pipelines;

        /// <summary>
        /// Bacsk the <see cref="IndexSerializer"/> property.
        /// </summary>
        private IndexSerializer _indexSerializer;

        const int succeededRetVal = 0;
        const int failedRetVal = -1;
    }
}
