using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OsgContentPublishing.ReferencePipelineLib.Serializers;

namespace OsgContentPublishing.ReferencePipelineLib.Documentation
{
    /// <summary>
    /// Specifies that an API is for documenting a content set only. 
    /// </summary>
    /// <remarks>
    /// <para>The <see cref="DocumentationOnlyAttribute"/> is used by serializers, 
    /// especially the <see cref="HtmlSerializer"/>, to control how APIs that are
    /// for documentation only are rendered.
    /// </para></remarks>
    [AttributeUsage( AttributeTargets.All, AllowMultiple = false )]  
    public class DocumentationOnlyAttribute : Attribute
    {
        public DocumentationOnlyAttribute( bool isForDocumentationOnly  )
        {
            this.IsForDocumentationOnly = IsForDocumentationOnly;
            this.SectionName = String.Empty;
        }

        public DocumentationOnlyAttribute( bool isForDocumentationOnly, string sectionName )
        {
            this.IsForDocumentationOnly = IsForDocumentationOnly;
            this.SectionName = sectionName;
        }

        public bool IsForDocumentationOnly
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the name of a section, which serializers can use to 
        /// create a heading in documentation.
        /// </summary>
        public string SectionName
        {
            get;
            private set;
        }
    }
}
