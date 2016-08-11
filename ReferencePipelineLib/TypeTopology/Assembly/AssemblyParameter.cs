using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology.Assembly
{
    /// <summary>
    /// Represents a parameter in an <see cref="AssemblyMethod"/>.
    /// </summary>
    public class AssemblyParameter : DefinedParameter
    {
        /// <summary>
        /// Initializes a new <see cref="AssemblyParameter"/> instance to the 
        /// specified <see cref="ParameterInfo"/> and parent method.
        /// </summary>
        /// <param name="parameterInfo">the parameter information.</param>
        /// <param name="parentMethod">The <see cref="AssemblyMethod"/> parent.</param>
        public AssemblyParameter( ParameterInfo parameterInfo, AssemblyMethod parentMethod )
            : base( parameterInfo, parentMethod )
        {
            if( parameterInfo != null )
            {
                this.UnderlyingParameter = parameterInfo;
            }
            else
            {
                throw new ArgumentNullException();
            }
        }

        public override DefinedType Type
        {
            get
            {
                if( this._type == null )
                {
                    this._type = TypeFactory.CreateAssemblyType( this.UnderlyingParameter.ParameterType );
                }

                return this._type;
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
                return this.UnderlyingParameter.ParameterType.Name.Contains( '[' );
            }
        }

        public override bool IsConst
        {
            get
            {
                return false;
                // TBD
                //return this.UnderlyingParameter.IsC;
            }
        }

        public override bool IsInParam
        {
            get
            {
                return this.UnderlyingParameter.IsIn;
            }
        }

        public override bool IsOutParam
        {
            get
            {
                return this.UnderlyingParameter.IsOut;
            }
        }

        public override bool IsOptional
        {
            get
            {
                return this.UnderlyingParameter.IsOptional;
            }
        }

        public override bool IsReturnValue
        {
            get
            {
                return this.UnderlyingParameter.IsRetval;
            }
        }

        public override bool IsReference
        {
            get
            {
                return this.UnderlyingParameter.ParameterType.Name.Contains( '&' );
            }
        }

        public override bool IsPointer
        {
            get
            {
                return this.UnderlyingParameter.ParameterType.Name.Contains( '*' );
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
                return this.UnderlyingParameter.Position;
            }
        }

        /// <summary>
        /// Gets the parameter information as represented by <see cref="System.Reflection"/>.
        /// </summary>
        public ParameterInfo UnderlyingParameter
        {
            get;
            protected set;
        }

        /// <summary>
        /// Back the <see cref="Type"/> Property.
        /// </summary>
        /// <remarks>TBD: Not <see cref="DefinedType"/>?</remarks>
        protected AssemblyType _type;
    }
}
