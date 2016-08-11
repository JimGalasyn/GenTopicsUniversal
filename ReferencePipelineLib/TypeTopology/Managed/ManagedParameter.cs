using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology.Managed
{
    public class ManagedParameter : DoxygenParameter
    {
        public ManagedParameter( Param param, DefinedMember parentMethod )
            : base( param, parentMethod )
        {   
        }

        public override DefinedType Type
        {
            get
            {
                if( this._type == null )
                {
                    throw new NotImplementedException();
                    //this._type = TypeFactory.CreateType( this.UnderlyingParameter.ParameterType );
                }

                return this._type;
            }
        }

        public override bool IsArray
        {
            get
            {
                throw new NotImplementedException();
                //return this.UnderlyingParameter.ParameterType.Name.Contains( '[' );
            }
        }

        public override bool IsInParam
        {
            get
            {
                throw new NotImplementedException();
                //return this.UnderlyingParameter.IsIn;
            }
        }

        public override bool IsOutParam
        {
            get
            {
                throw new NotImplementedException();
                //return this.UnderlyingParameter.IsOut;
            }
        }

        public override bool IsOptional
        {
            get
            {
                throw new NotImplementedException();
                //return this.UnderlyingParameter.IsOptional;
            }
        }

        public override bool IsReturnValue
        {
            get
            {
                throw new NotImplementedException();
                //return this.UnderlyingParameter.IsRetval;
            }
        }

        public override bool IsReference
        {
            get
            {
                throw new NotImplementedException();
                //return this.UnderlyingParameter.ParameterType.Name.Contains( '&' );
            }
        }

        public override bool IsPointer
        {
            get
            {
                throw new NotImplementedException();
                //return this.UnderlyingParameter.ParameterType.Name.Contains( '*' );
            }
        }

        public override int Position
        {
            get
            {
                throw new NotImplementedException();
                //return this.UnderlyingParameter.Position;
            }
        }

        //public Param UnderlyingParameter
        //{
        //    get;
        //    protected set;
        //}

        //protected ManagedType _type;
    }
}
