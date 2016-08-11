using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen
{
    public class ParameterItem
    {
        public ParameterItem( string parameterName, string parameterDescription, int position )
        {
            this.ParameterName = parameterName;
            this.ParameterDescription = parameterDescription;
            this.Position = position;
        }

        public string ParameterName
        {
            get;
            private set;
        }

        public string ParameterDescription
        {
            get;
            private set;
        }

        public int Position
        {
            get;
            private set;
        }

        public override string ToString()
        {
            string msg = String.Format( 
                "{0}: {1} - {2}", 
                this.Position, 
                this.ParameterName, 
                this.ParameterDescription );
            return msg;
        }
    }
}
