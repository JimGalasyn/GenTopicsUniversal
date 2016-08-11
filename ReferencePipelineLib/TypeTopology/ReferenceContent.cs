using OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology
{
    /// <summary>
    /// Represents the API reference content for a <see cref="DefinedType"/>.
    /// </summary>
    public class ReferenceContent
    {
        /// <summary>
        /// Initializes a new <see cref="ReferenceContent"/> instance to the 
        /// specified <see cref="DoxType"/>.
        /// </summary>
        /// <param name="doxType">A deserialized Doxygen type.</param>
        /// <remarks><para>The content originates from code comments. Doxygen scrapes
        /// out these comments and emits them, along with its understanding of the
        /// source code's type graph, to an XML representation.</para>
        /// <para>The <see cref="DoxType"/> class deserializes Doxygen's XML and
        /// copies the code comments as a collection of <see cref="Para"/> instances.
        /// </para>
        /// </remarks>
        public ReferenceContent( DoxType doxType )
        {
            if( doxType != null )
            {
                this.CopyContent( doxType );
            }
            else
            {
                throw new ArgumentNullException( "doxType" );
            }
        }

        /// <summary>
        /// Initializes a new <see cref="ReferenceContent"/> instance to the 
        /// specified <see cref="MemberDef"/>.
        /// </summary>
        /// <param name="memberDef">A deserialized Doxygen memberdef element.</param>
        /// <remarks><para>The content originates from code comments. Doxygen scrapes
        /// out these comments and emits them, along with its understanding of the
        /// source code's type graph, to an XML representation.</para>
        /// <para>The <see cref="MemberDef"/> class deserializes Doxygen's XML and
        /// copies the code comments as a collection of <see cref="Para"/> instances.
        /// </para>
        /// </remarks>
        public ReferenceContent( MemberDef memberDef )
        {
            if( memberDef != null )
            {
                this.CopyContent( memberDef );
            }
            else
            {
                throw new ArgumentNullException( "memberDef" );
            }
        }

        /// <summary>
        /// Initializes a new <see cref="ReferenceContent"/> instance to the 
        /// specified <see cref="Param"/>.
        /// </summary>
        /// <param name="param">A deserialized Doxygen param element.</param>
        public ReferenceContent( Param param )
        {
            if( param != null )
            {
                this.CopyContent( param );
            }
            else
            {
                throw new ArgumentNullException( "param" );
            }
        }

        /// <summary>
        /// Initializes a new <see cref="ReferenceContent"/> instance to the 
        /// specified <see cref="EnumValue"/>.
        /// </summary>
        /// <param name="enumValue">A deserialized doxygen enumvalue element.</param>
        public ReferenceContent( EnumValue enumValue )
        {
            if( enumValue != null )
            {
                this.CopyContent( enumValue );
            }
            else
            {
                throw new ArgumentNullException( "enumValue" );
            }
        }

        public ReferenceContent()
        {
        }

        private void CopyContent( DoxType doxType )
        {
            this._briefDescription = doxType.BriefDescription;
            this._detailedDescription = doxType.DetailedDescription;
        }

        private void CopyContent( MemberDef memberDef )
        {
            this._briefDescription = memberDef.BriefDescription;
            this._detailedDescription = memberDef.DetailedDescription;
        }

        private void CopyContent( Param param )
        {
            this._briefDescription = param.BriefDescription;
            this._detailedDescription = null;
        }

        private void CopyContent( EnumValue enumValue )
        {
            if( enumValue.detaileddescription != null )
            {
                this._description = enumValue.detaileddescription;
            }

            if( enumValue.briefdescription != null )
            {
                this._abstract = enumValue.briefdescription;
            }
        }

        /// <summary>
        /// For future use.
        /// </summary>
        public string Title
        {
            get
            {
                // TBD: Parse this.
                return String.Empty;
            }
        }

        /// <summary>
        /// Gets the text of the abstract.
        /// </summary>
        /// <remarks><para>Usually, the abstract text is copied from
        /// Doxygen's briefdescription element.
        /// </para>
        /// </remarks>
        public string Abstract
        {
            get
            {
                if( this._abstract == null )
                {
                    if( this._briefDescription != null &&
                        this._briefDescription.Desc != String.Empty )
                    {
                        this._abstract = this._briefDescription.Desc;
                    }
                    else if( this._detailedDescription != null &&  
                        this._detailedDescription.Desc != String.Empty )
                    {
                        this._abstract = this._detailedDescription.Desc;
                    }
                    else
                    {
                        this._abstract = String.Empty;
                    }
                }

                return this._abstract;
            }

            set
            {
                if( this._abstract != value )
                {
                    this._abstract = value;
                }
            }
        }

        /// <summary>
        /// Gets the description text.
        /// </summary>
        /// <remarks><para>Usually, the description text is copied from
        /// Doxygen's detaileddescription element.
        /// </para>
        /// </remarks>
        public string Description
        {
            get
            {
                if( this._description == null )
                {
                    this._description = String.Empty;

                    if( this._detailedDescription != null &&
                        this._detailedDescription.Desc != String.Empty )
                    {
                        // Check if the detailed description already has been
                        // assigned to the abstract. 
                        if( this.Abstract != this._detailedDescription.Desc )
                        {
                            this._description = this._detailedDescription.Desc;
                        }
                    }
                }

                return this._description;
            }

            private set
            {
                if( this._description != value )
                {
                    this._description = value;
                }
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="Para"/> instances that contain 
        /// code comments.
        /// </summary>
        public List<Para> Paragraphs
        {
            get
            {
                if( this._paragraphs == null )
                {
                    if( this._detailedDescription != null )
                    {
                        this._paragraphs = this._detailedDescription.Paragraphs;
                    }
                    else
                    {
                        this._paragraphs = new List<Para>();
                    }
                }

                return this._paragraphs;
            }
        }

        /// <summary>
        /// For future use.
        /// </summary>
        public Uri SampleUri
        {
            get
            {
                throw new System.NotImplementedException();
            }
            private set
            {
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current <see cref="ReferenceContent"/> has
        /// is empty of code comments.
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                bool isEmpty =
                    this.Abstract == string.Empty &&
                    this.Description == string.Empty &&
                    this.Paragraphs.Count == 0 &&
                    this.Title == string.Empty;

                return isEmpty;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current <see cref="ReferenceContent"/> has
        /// any code comments.
        /// </summary>
        public bool HasContent
        {
            get
            {
                return !this.IsEmpty;
            }
        }

        public override string ToString()
        {
            string toString = String.Format( "{0}", this.Abstract );
            return toString;
        }

        private string _abstract;
        private string _description;
        private List<Para> _paragraphs;

        private Description _briefDescription;
        private Description _detailedDescription;
    }
}
