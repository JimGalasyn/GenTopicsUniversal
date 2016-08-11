using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OsgContentPublishing.ReferencePipelineLib.TypeTopology;
using OsgContentPublishing.ReferencePipelineLib.TypeTopology.Assembly;
using ReflectionUtilities;

namespace OsgContentPublishing.ReferencePipelineLib.Documentation
{
    public class DocumentationType : AssemblyType
    {
        public DocumentationType( ObservableType observableType ) : base( observableType )
        {

        }
    }
}
