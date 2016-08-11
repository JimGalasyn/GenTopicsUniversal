using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen
{
    public class GenericParameterType : DoxygenFacadeType
    {
        public GenericParameterType( string typeName )
            : base( typeName )
        {
        }

        public override DefinedType Namespace
        {
            get
            {
                return null;
            }
        }

        public override bool IsValueType
        {
            get
            {
                return false;
            }
        }

        public override LanguageElement LanguageElement
        {
            get
            {
                return TypeTopology.LanguageElement.GenericParameter;
            }
        }

        public override string ToString()
        {
            string toString = String.Format( "typename {0}", this.Name );
            return toString;
        }
    }
}
