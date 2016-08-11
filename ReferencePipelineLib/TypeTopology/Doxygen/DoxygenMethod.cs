using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen
{
    public class DoxygenMethod : DoxygenMember
    {
        public DoxygenMethod( MemberDef memberDef, DoxygenType parentType )
            : base( memberDef, parentType )
        {
        }

        public override DefinedType Type
        {
            get
            {
                if( this._type == null )
                {
                    string rawMethodType = this.UnderlyingMember.type;

                    // Remove _Use_decl_annotations_
                    string methodReturnType = rawMethodType.Replace( "_Use_decl_annotations_", String.Empty );

                    // Remove override
                    methodReturnType = methodReturnType.Replace( "override", String.Empty );

                    // Remove abstract
                    methodReturnType = methodReturnType.Replace( "abstract", String.Empty );

                    // Remove delegate
                    methodReturnType = methodReturnType.Replace( "delegate", String.Empty );

                    // Remove new
                    methodReturnType = methodReturnType.Replace( "new", String.Empty );

                    // Remove embedded spaces
                    methodReturnType = methodReturnType.Replace( " ", String.Empty );

                    // Remove whitespace
                    methodReturnType = methodReturnType.Trim();

                    this._type = TypeFactory.CreateType( methodReturnType );
                }

                return this._type;
            }

            set
            {
                this._type = value;
            }
        }


        public override List<DefinedParameter> Parameters
        {
            get
            {
                if( this._parameters == null )
                {   
                    if( this.UnderlyingMember.HasParameters )
                    {
                        this._parameters = this.UnderlyingMember.Parameters.Select( p =>
                            new DoxygenParameter( p, this ) as DefinedParameter ).ToList();
                    }
                    else
                    {
                        this._parameters = new List<DefinedParameter>();
                    }
                }

                return this._parameters;
            }
        }

        public override LanguageElement LanguageElement
        {
            get { return LanguageElement.Method; }
        }


        public override string ToString()
        {
            string toString = String.Format( "{0}.{1} method", this.ParentType.Name, this.Name );
            return toString;
        }

        protected List<DefinedParameter> _parameters;
    }
}
