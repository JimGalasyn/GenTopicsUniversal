using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen
{
    public class DoxygenNamespace : DoxygenType
    {
        public DoxygenNamespace( DoxType doxType )
            : base( doxType )
        {
            if( doxType != null && !doxType.IsNamespace )
            {
                throw new ArgumentException( "must be a namespace", "doxType" );
            }
        }

        protected override void InitializeMembers()
        {
            base.InitializeMembers();

            if( this.UnderlyingType != null && 
                this.UnderlyingType.MemberDefs != null &&
                this.UnderlyingType.MemberDefs.Count > 0 )
            {
                // TBD: These should be added to the ChildTypes collection, 
                // instead of the Members collection.
                var validMembers = this.UnderlyingType.MemberDefs.Where( md => !md.IsTypedef && !md.IsClass );
                var members = validMembers.Select( m => DoxygenMember.CreateMember( m, this ) as DefinedMember );
                this.Members = members.ToList();
            }
            else
            {
                this.Members = new List<DefinedMember>();
            }
        }

        protected override void InitializeBaseTypes()
        {
            this.BaseTypes = new List<DefinedType>();
        }

        protected override void InitializeDerivedTypes()
        {
            this.DerivedTypes = new List<DefinedType>();
        }

        public override List<DefinedType> GenericParameterTypes
        {
            get
            {
                if( this._genericParameterTypes == null )
                {
                    this._genericParameterTypes = new List<DefinedType>();
                }

                return this._genericParameterTypes;
            }

            protected set
            {
                base.GenericParameterTypes = value;
            }
        }

        public override bool IsGeneric
        {
            get
            {
                return false;
            }
        }

        public override bool IsAttribute
        {
            get
            {
                return false; ;
            }
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

        public override bool IsInterface
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


        public override LanguageElement LanguageElement
        {
            get { return LanguageElement.Namespace; }
        }

        public override string ToString()
        {
            string toString = String.Format( "{0} namespace", this.FullName );
            return toString;
        }
    }
}
