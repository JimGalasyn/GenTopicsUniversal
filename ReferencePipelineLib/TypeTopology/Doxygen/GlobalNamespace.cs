using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen
{
    public class GlobalNamespace : DoxygenNamespace
    {
        public GlobalNamespace( DoxType doxType )
            : base( doxType )
        {
            if( doxType != null )
            {
                throw new ArgumentException( "expected to be null", "doxType" );
            }

            this.Name = TypeFactory.GlobalNamespaceName;
            this.FullName = TypeFactory.GlobalNamespaceName;
            this.Namespace = null;
            this.UnderlyingType = null;
        }

        public override bool IsGlobalNamespace
        {
            get
            {
                return true;
            }
        }

        public override ReferenceContent Content
        {
            get
            {
                if( this._referenceContent == null )
                {
                    this._referenceContent = new ReferenceContent();
                }

                return this._referenceContent;
            }

            set
            {
                base.Content = value;
            }
        }
                
        public override List<DefinedType> ChildTypes
        {
            get
            {
                if( this._childTypes == null )
                {
                    this._childTypes = new List<DefinedType>();
                }

                return this._childTypes;
            }
        }

        public override DefinedType Namespace
        {
            get
            {
                return null;
            }

            set
            {
                // Should never happen.
                base.Namespace = value;
            }
        }

        public override DefinedType ParentType
        {
            get
            {
                return null;
            }

            set
            {
                // Should never happen.
                base.ParentType = value;
            }
        }
    }
}
