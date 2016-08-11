using OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology.Assembly
{
    public class AssemblyNamespace : AssemblyType
    {
        public AssemblyNamespace( string namespaceFullName )
            : base( namespaceFullName )
        {
            this.Name = Utilities.GetTypeName( this.FullName );
        }
       
        public override DefinedType Namespace
        {
            get
            {
                if( this is GlobalNamespace )
                {
                    return null;
                }
                else
                {
                    string parentNamespace = Utilities.GetParentNamespaceFull( this.FullName );
                    return TypeFactory.CreateAssemblyNamespaceType( parentNamespace );
                }
            }
        }

        public override string FriendlyName
        {
            get
            {
                return this.Name;
            }
        }


        protected override void InitializeMembers()
        {
            // Assemblies have no members, so do nothing.
            this.Members = new List<DefinedMember>();
        }

        public override bool IsClass
        {
            get
            {
                return false;
            }
        }

        public override bool IsDelegate
        {
            get
            {
                return false;
            }
        }

        public override bool IsEnum
        {
            get
            {
                return false;
            }
        }

        public override bool IsGeneric
        {
            get
            {
                return false;
            }
        }

        public override bool IsInterface
        {
            get
            {
                return false;
            }
        }

        public override bool IsPublic
        {
            get
            {
                return true;
            }
        }

        public override bool IsReferenceType
        {
            get
            {
                return false;
            }
        }

        public override bool IsSealed
        {
            get
            {
                return false;
            }
        }

        public override bool IsStruct
        {
            get
            {
                return false;
            }
        }

        public override bool IsValueType
        {
            get
            {
                return false;
            }
        }


        public override List<DefinedType> ChildTypes
        {
            get
            {
                if( this._typesInThisNamespace == null )
                {
                    // TBD: It would be nice to do a direct comparison of AssemblyNamespaces,
                    // instead of matching on FullName.
                    // Make a copy of the KnownAssemblyTypes collection, because the Where query
                    // will change the KnownAssemblyTypes as namespace types are created by
                    // accessing the t.Namespace.FullName property.
                    var knownAssemblyTypessnapshot = TypeFactory.KnownAssemblyTypes.Values.Select( t => t ).ToList();

                    var typesInThisNamespace = knownAssemblyTypessnapshot.Where( t =>
                        !t.IsGlobalNamespace && ( t.Namespace.FullName == this.FullName ) );

                    if( typesInThisNamespace != null )
                    {
                        var typesInThisNamespaceList = typesInThisNamespace.ToList();
                        var types = typesInThisNamespaceList.Select( t => t as DefinedType );
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

        public override LanguageElement LanguageElement
        {
            get { return LanguageElement.Namespace; }
        }

        public override string ToString()
        {
            string toString = String.Format( "{0} namespace", this.FullName );
            return toString;
        }

        protected List<DefinedType> _typesInThisNamespace;
    }
}
