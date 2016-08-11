using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen
{
    public class DoxygenDestructor : DoxygenMethod
    {
        public DoxygenDestructor( MemberDef memberDef, DoxygenType parentType )
            : base( memberDef, parentType )
        {
        }

        public override bool IsDestructor
        {
            get
            {
                return true;
            }
        }

        public override DefinedType Type
        {
            get
            {
                this._type = TypeFactory.KnownTypes["void"];
                return this._type;
            }

            set
            {
                base.Type = value;
            }
        }

        public override string ToString()
        {
            string toString = String.Format( "{0} dtor", this.ParentType.Name );
            return toString;
        }
    }
}
