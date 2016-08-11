using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen
{
    /// <summary>
    /// Represents a parameter in a <see cref="DoxygenMethod"/>.
    /// </summary>
    public class DoxygenParameter : DefinedParameter
    {
        /// <summary>
        /// Initializes a new <see cref="DoxygenParameter"/> instance to the 
        /// specified <see cref="Param"/> and parent method.
        /// </summary>
        /// <param name="param">The parameter info.</param>
        /// <param name="parentMethod">The <see cref="DefinedMember"/> parent. 
        /// TBD: not <see cref="DoxygenMethod"/>? </param>
        public DoxygenParameter( Param param, DefinedMember parentMethod ) 
            : base( param, parentMethod )
        {
            if( param != null && parentMethod != null )
            {
                this.UnderlyingParameter = param;
                this.Name = param.declname;
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
                    string paramType = this.UnderlyingParameter.FullName;
                    this._type = TypeFactory.CreateType( paramType );
                }

                return this._type;
            }

            set
            {
                this._type = value;
            }
        }

        public override ReferenceContent Content
        {
            get
            {
                if( this._referenceContent == null )
                {
                    // TBD what to do about enums?
                    //if( this.UnderlyingMember != null )
                    {
                        this._referenceContent = new ReferenceContent( this.UnderlyingParameter );
                    }
                }

                return this._referenceContent;
            }
            set
            {
                this._referenceContent = value;
            }
        }

        public override bool IsArray
        {
            get
            {
                return this.UnderlyingParameter.IsArray;
            }
        }

        public override bool IsConst
        {
            get
            {   
                return this.UnderlyingParameter.IsConst;
            }
        }

        public override bool IsInParam
        {
            get
            {
                return this.UnderlyingParameter.IsInParam;
            }
        }

        public override bool IsOutParam
        {
            get
            {
                return this.UnderlyingParameter.IsOutParam;
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
                // TBD
                return false;

                //return this.UnderlyingParameter.IsRetval;
            }
        }

        public override bool IsReference
        {
            get
            {
                return this.UnderlyingParameter.IsReference;
            }
        }

        public override bool IsPointer
        {
            get
            {
                return this.UnderlyingParameter.IsPointer;
            }
        }

        public override int PointerDepth
        {
            get
            {
                return this.UnderlyingParameter.PointerDepth;
            }
        }


        public override int Position
        {
            get
            {
                // TBD
                return 0;
                //return this.UnderlyingParameter.Position;
            }
        }

        /// <summary>
        /// Gets the parameter info, as represented by Doxygen's param element. 
        /// </summary>
        public Param UnderlyingParameter
        {
            get;
            protected set;
        }

        //public override LanguageElement LanguageElement
        //{
        //    get { return LanguageElement.Parameter; }
        //}

        public override string ToString()
        {
            string toString = String.Format( "{0} param", this.Name );
            return toString;
        }

        /// <summary>
        /// Backs the <see cref="DefinedParameter.Type"/> property.
        /// </summary>
        protected DefinedType _type;
    }
}
