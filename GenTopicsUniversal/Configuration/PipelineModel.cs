using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsgContentPublishing.GenTopicsUniversal.Configuration
{
    public enum PipelineModel
    {
        Unknown = 0,
        Native = 1,
        Managed = 2,
        Assembly = 3,
        ManagedAssembly = 4,
        RidlPipeline = 5,
        Universal = 6,
        Doxygen = 7
    }
}
