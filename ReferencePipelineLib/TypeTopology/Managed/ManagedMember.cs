using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology.Managed
{
    public abstract class ManagedMember : DoxygenMember
    {
        public ManagedMember( MemberDef memberDef, DoxygenType parentType )
            : base( memberDef, parentType )
        {
        }

        //public static ManagedMember CreateMember( MemberInfo memberInfo, AssemblyType parentType )
        //{
        //    ManagedMember newMember = null;

        //    if( memberInfo != null && !memberInfo.DeclaringType.Name.StartsWith( "System" ) )
        //    {
        //        switch( memberInfo.MemberType )
        //        {
        //            case MemberTypes.Constructor:
        //                {
        //                    newMember = new ManagedConstructor( memberInfo as ConstructorInfo, parentType );
        //                    break;
        //                }

        //            case MemberTypes.Event:
        //                {
        //                    newMember = new ManagedEvent( memberInfo as EventInfo, parentType );
        //                    break;
        //                }

        //            case MemberTypes.Field:
        //                {
        //                    newMember = new ManagedField( memberInfo as FieldInfo, parentType );
        //                    break;
        //                }

        //            case MemberTypes.Method:
        //                {
        //                    newMember = new ManagedMethod( memberInfo as MethodInfo, parentType );
        //                    break;
        //                }

        //            case MemberTypes.Property:
        //                {
        //                    newMember = new ManagedProperty( memberInfo as PropertyInfo, parentType );
        //                    break;
        //                }

        //            default:
        //                {
        //                    throw new ArgumentException( "Unknown MemberType", "memberInfo" );
        //                }
        //        }
        //    }
        //    else
        //    {
        //        throw new ArgumentNullException( "memberInfo" );
        //    }

        //    return newMember;
        //}


        //public MemberInfo UnderlyingMember
        //{
        //    get;
        //    protected set;
        //}

        //protected AssemblyType _type;
    }
}
