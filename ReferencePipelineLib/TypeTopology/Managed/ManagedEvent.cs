using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology.Managed
{
    public class ManagedEvent : ManagedMember
    {
        public ManagedEvent( MemberDef memberDef, DoxygenType parentType )
            : base( memberDef, parentType )
        {
        }

        public DefinedType DelegateType
        {
            get
            {
                return this.Type;
            }
        }

        public override DefinedType Type
        {
            get
            {
                if( this._type == null )
                {
                    throw new NotImplementedException();
                    //this._type = TypeFactory.CreateType( this.UnderlyingEventInfo.EventHandlerType );

                }

                return this._type;
            }
        }

        public override bool IsPublic
        {
            get
            {
                throw new NotImplementedException();
                //return this.UnderlyingEventInfo.GetAddMethod().IsPublic;
            }
        }

        public override bool IsPrivate
        {
            get
            {
                throw new NotImplementedException();
                //return this.UnderlyingEventInfo.GetAddMethod().IsPrivate;
            }
        }

        public override bool IsStatic
        {
            get
            {
                throw new NotImplementedException();
                //return this.UnderlyingEventInfo.GetAddMethod().IsStatic;
            }
        }

        //public ManagedMethod AddMethod
        //{
        //    get
        //    {
        //        if( this._addMethod == null )
        //        {
        //            MethodInfo addMethod = this.UnderlyingEventInfo.GetAddMethod();
        //            this._addMethod = ManagedMember.CreateMember(
        //                addMethod,
        //                this.ParentType as AssemblyType ) as ManagedMethod;
        //        }

        //        return this._addMethod;
        //    }
        //}

        //public ManagedMethod RemoveMethod
        //{
        //    get
        //    {
        //        if( this._removeMethod == null )
        //        {
        //            MethodInfo removeMethod = this.UnderlyingEventInfo.GetRemoveMethod();

        //            this._removeMethod = ManagedMember.CreateMember(
        //                removeMethod,
        //                this.ParentType as AssemblyType ) as ManagedMethod;
        //        }

        //        return this._removeMethod;
        //    }
        //}

        //public EventInfo UnderlyingEventInfo
        //{
        //    get
        //    {
        //        return this.UnderlyingMember as EventInfo;
        //    }
        //}

        public override string ToString()
        {
            string toString = String.Format( "{0}.{1} event", this.ParentType.Name, this.Name );
            return toString;
        }


        //private ManagedMethod _addMethod;
        //private ManagedMethod _removeMethod;
    }
}
