using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology.Assembly
{
    public class AssemblyEvent : AssemblyMember
    {
        public AssemblyEvent( EventInfo eventInfo, AssemblyType parentType )
            : base( eventInfo, parentType )
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
                    this._type = TypeFactory.CreateAssemblyType( this.UnderlyingEventInfo.EventHandlerType );

                    //this._type = ManagedType.CreateType( this.UnderlyingEventInfo.EventHandlerType );
                }

                return this._type;
            }
        }

        public override bool IsPublic
        {
            get
            {
                return this.UnderlyingEventInfo.GetAddMethod().IsPublic;
            }
        }

        public override bool IsPrivate
        {
            get
            {
                return this.UnderlyingEventInfo.GetAddMethod().IsPrivate;
            }
        }

        public override bool IsStatic
        {
            get
            {
                return this.UnderlyingEventInfo.GetAddMethod().IsStatic;
            }
        }

        public override bool IsMutable
        {
            get
            {
                return false;
            }
        }

        public override bool IsConst
        {
            get
            {
                return false;
            }
        }

        public AssemblyMethod AddMethod
        {
            get
            {
                if( this._addMethod == null )
                {
                    MethodInfo addMethod = this.UnderlyingEventInfo.GetAddMethod();
                    this._addMethod = AssemblyMember.CreateMember(
                        addMethod,
                        this.ParentType as AssemblyType ) as AssemblyMethod;
                }

                return this._addMethod;
            }
        }

        public AssemblyMethod RemoveMethod
        {
            get
            {
                if( this._removeMethod == null )
                {
                    MethodInfo removeMethod = this.UnderlyingEventInfo.GetRemoveMethod();

                    this._removeMethod = AssemblyMember.CreateMember(
                        removeMethod,
                        this.ParentType as AssemblyType ) as AssemblyMethod;
                }

                return this._removeMethod;
            }
        }

        public EventInfo UnderlyingEventInfo
        {
            get
            {
                return this.UnderlyingMember as EventInfo;
            }
        }

        public override string ToString()
        {
            string toString = String.Format( "{0}.{1} event", this.ParentType.Name, this.Name );
            return toString;
        }


        private AssemblyMethod _addMethod;
        private AssemblyMethod _removeMethod;
    }
}
