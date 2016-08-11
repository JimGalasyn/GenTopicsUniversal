using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen;

/// Contains classes that represent types that are deserialized from 
/// managed source code, like C#. Currently, only for future use.
namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology.Managed
{
    /// <summary>
    /// For future use.
    /// </summary>
    public class ManagedType : DoxygenType
    {
        public ManagedType( DoxType doxType )
            : base( doxType )
        {
        }
    }
}
