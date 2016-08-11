using OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology.Managed
{
    public class ManagedNamespace : ManagedType
    {
        public ManagedNamespace( string namespaceFullName )
            : base( null )
        {
            this.FullName = namespaceFullName;
            this.Name = Utilities.GetTypeName( this.FullName );
        }


        public override DefinedType Namespace
        {
            get
            {
                return TypeFactory.CreateAssemblyNamespaceType( this.FullName );
            }
        }

        public override List<DefinedType> ChildTypes
        {
            get
            {
                if( this._typesInThisNamespace == null )
                {
                    var typesInThisNamespace = TypeFactory.KnownTypes.Where( t =>
                        t.Value.Namespace.FullName == this.FullName );

                    if( typesInThisNamespace != null )
                    {
                        var types = typesInThisNamespace.Select( t => t.Value as DefinedType );
                        this._typesInThisNamespace = types.ToList();
                    }
                    else
                    {
                        this._typesInThisNamespace = new List<DefinedType>();
                    }
                }

                return this._typesInThisNamespace;
            }
        }

        public override string ToString()
        {
            string toString = String.Format( "{0} namespace", this.FullName );
            return toString;
        }

        protected List<DefinedType> _typesInThisNamespace;
    }
}
