using OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology
{
    public class DefinedEvent : DefinedMethod
    {
        public DefinedEvent( MemberDef memberDef )
            : base( memberDef )
        {

        }

        //public DefinedEvent( EventInfo eventInfo, DefinedType parentType )
        //    : base( eventInfo, parentType )
        //{
        //    //this.Initialize( methodInfo );
        //}

        protected override void Initialize( MemberInfo memberInfo, DefinedType parentType )
        {
            base.Initialize( memberInfo, parentType );

            EventInfo eventInfo = memberInfo as EventInfo;
            if( eventInfo != null )
            {
                base.Initialize( eventInfo, parentType );

                //this.Type = DefinedType.CreateType( eventInfo.EventHandlerType );
            }
            else
            {
                throw new ArgumentException( "is not an EventInfo", "memberInfo" );
            }

        }

        public override DefinedType Type
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}
