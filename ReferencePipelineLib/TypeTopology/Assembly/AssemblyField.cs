using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology.Assembly
{
    public class AssemblyField : AssemblyMember
    {
        public AssemblyField( FieldInfo fieldInfo, AssemblyType parentType )
            : base( fieldInfo, parentType )
        {
            if( parentType.IsEnum )
            {
                if( this.Name == "value__" )
                {
                    // This should never happen -- ObservableType.Fields filters
                    // this field. If it's present, GetRawConstantValue throws.
                    throw new ArgumentException( 
                        "can't be the value__ field of a managed enum", 
                        "fieldInfo" );
                }
                else
                {
                    this.Value = fieldInfo.GetRawConstantValue();
                }
            }
        }

        public override DefinedType Type
        {
            get
            {
                if( this._type == null )
                {
                    FieldInfo fieldInfo = this.UnderlyingMember as FieldInfo;
                    this._type = TypeFactory.CreateAssemblyType( fieldInfo.FieldType );
                }

                return this._type;
            }
        }

        public override bool IsPublic
        {
            get
            {   
                return this.UnderlyingFieldInfo.IsPublic;
            }
        }

        public override bool IsPrivate
        {
            get
            { 
                return this.UnderlyingFieldInfo.IsPrivate; 
            }
        }

        public override bool IsStatic
        {
            get
            {
                return this.UnderlyingFieldInfo.IsStatic;
            }
        }

        public override bool IsConst
        {
            get
            {
                // TBD
                return false;
            }
        }


        public override bool IsMutable
        {
            get
            {
                return !this.IsConst;
            }
        }

        public FieldInfo UnderlyingFieldInfo
        {
            get
            {
                return this.UnderlyingMember as FieldInfo;
            }
        }

        public override LanguageElement LanguageElement
        {
            get { return LanguageElement.Field; }
        }


        public override string ToString()
        {
            string toString = String.Format( "{0}.{1} field", this.ParentType.Name, this.Name );
            return toString;
        }
    }
}
