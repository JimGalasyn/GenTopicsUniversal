using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology.Native
{
    public class NativeParameter : DefinedParameter
    {

        public NativeParameter( Param param, DefinedMember parentMethod ) : base( param, parentMethod )
        {

        }

        ///////////////////////////////////////////////////////////////////////
        #region Public Properties

        public override DefinedType Type
        {
            get
            {
                throw new NotImplementedException();

                //if( this._type == null )
                //{
                //    this._type = ManagedType.CreateType( this.UnderlyingParameter.ParameterType );
                //}

                //return this._type;
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public override bool IsArray
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override bool IsConst
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override bool IsInParam
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override bool IsOutParam
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override bool IsOptional
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override bool IsReturnValue
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override bool IsReference
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override bool IsPointer
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override int PointerDepth
        {
            get
            {
                // TBD
                return 0;

                //return this.UnderlyingParameter.ParameterType.Name.Contains( '*' );
            }
        }


        public override int Position
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Param UnderlyingParameter
        {
            get;
            protected set;
        }

        #endregion

    }
}
