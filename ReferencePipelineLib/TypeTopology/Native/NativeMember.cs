using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology.Native
{
    public class NativeMember : DoxygenMember
    {
        public NativeMember( MemberDef memberDef, NativeType parentType )
            : base( memberDef, parentType )
        {
            if( memberDef != null && parentType != null )
            {
                this.UnderlyingMember = memberDef;
                this.ParentType = parentType;
            }
            else
            {
                throw new ArgumentNullException();
            }
        }

        public static NativeMember CreateMember( MemberDef memberDef, NativeType parentType )
        {
            NativeMember newMember = null;

            if( memberDef != null && parentType != null )
            {

                switch( memberDef.kind )
                {
                    case "constructor":
                        {
                            newMember = new NativeConstructor( memberDef, parentType );
                            break;
                        }

                    case "event":
                        {
                            newMember = new NativeEvent( memberDef, parentType );
                            break;
                        }

                    case "field":
                        {
                            newMember = new NativeField( memberDef, parentType );
                            break;
                        }

                    case "function":
                        {
                            newMember = new NativeMethod( memberDef, parentType );
                            break;
                        }

                    case "property":
                        {
                            // TBD
                            //newMember = new NativeProperty( memberDef, parentType );
                            break;
                        }

                    default:
                        {
                            throw new ArgumentException( "Unknown MemberType", "memberDef" );
                        }
                }
            }
            else
            {
                throw new ArgumentNullException();
            }

            return newMember;
        }



        public override bool IsPublic
        {
            get
            {
                // TBD
                return false;
                //return this.UnderlyingMember.
            }
        }

        public override bool IsPrivate
        {
            get
            {
                // TBD
                return false;
                //return this.UnderlyingMember.
            }
        }

        public override bool IsStatic
        {
            get
            {
                return this.UnderlyingMember.IsStatic == true ? true : false;
            }
        }


        //public MemberDef UnderlyingMember
        //{
        //    get;
        //    protected set;
        //}

        //protected
    }
}
