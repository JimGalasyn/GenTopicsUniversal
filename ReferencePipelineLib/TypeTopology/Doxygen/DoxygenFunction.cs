using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen
{
    public class DoxygenFunction : DoxygenType
    {
        public DoxygenFunction( DoxType doxType )
            : base( doxType )
        {
            if( !doxType.IsFunction )
            {
                throw new ArgumentException( "must be a function", "doxType" );
            }
        }

        public List<DefinedParameter> Parameters
        {
            get
            {
                return this.UnderlyingMethod.Parameters;
            }
        }

        /// <summary>
        /// Gets the function information as represented by Doxygen's memberdef element.
        /// </summary>
        public DoxygenMethod UnderlyingMethod
        {
            get
            {
                if( this._underlyingMethod == null )
                {
                    this._underlyingMethod = new DoxygenMethod(
                        this.UnderlyingType.UnderlyingMember, 
                        null );
                }

                return this._underlyingMethod;
            }
        }

        public override LanguageElement LanguageElement
        {
            get { return LanguageElement.Function; }
        }

        public override string ToString()
        {
            string toString = String.Format( "{0} function", this.FullName );
            return toString;
        }

        protected DoxygenMethod _underlyingMethod;
    }
}
