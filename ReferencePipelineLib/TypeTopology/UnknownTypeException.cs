using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology
{
    /// <summary>
    /// Indicates that a type can't be resolved in a context that requires a 
    /// resolved type.
    /// </summary>
    /// <remarks><para>Mostly, this exception is raised in <see cref="FacadeType"/> properties.
    /// </para>
    /// </remarks>
    [Serializable]
    public class UnknownTypeException : Exception
    {
        public UnknownTypeException()
        {

        }

        public UnknownTypeException( string typeName )
        {

        }

        public UnknownTypeException( string typeName, string message ) : base( message )
        {

        }
    }
}
