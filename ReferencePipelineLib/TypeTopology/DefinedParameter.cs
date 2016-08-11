using ReflectionUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology
{
    /// <summary>
    /// Represents a parameter in a <see cref="DefinedMember"/>.
    /// </summary>
    /// <remarks><para>In the future, this class also will represent 
    /// generic/template parameters.
    /// </para>
    /// </remarks>
    public abstract class DefinedParameter
    {
        /// <summary>
        /// Initializes a new <see cref="DefinedParameter"/> instance to the 
        /// specified <see cref="ParameterInfo"/> and parent method/function.
        /// </summary>
        /// <param name="parameterInfo">A source parameter that was deserialized 
        /// from a managed assembly or winmd file.</param>
        /// <param name="parentMethod">The parent method of the parameter 
        /// represented by <paramref name="parameterInfo"/>.</param>
        public DefinedParameter( ParameterInfo parameterInfo, DefinedMember parentMethod )
        {
            this.Initialize( parameterInfo, parentMethod );
        }

        private void Initialize( ParameterInfo parameterInfo, DefinedMember parentMethod )
        {
            if( parameterInfo != null && parentMethod != null )
            {
                this.Name = parameterInfo.Name;
                this.ParentMethod = parentMethod;
                this.Content = new ReferenceContent();
            }
            else
            {
                throw new ArgumentNullException();
            }
        }

        /// <summary>
        /// Initializes a new <see cref="DefinedParameter"/> instance to the 
        /// specified <see cref="Param"/> and parent method/function. 
        /// </summary>
        /// <param name="param">A source parameter that was deserialized 
        /// from Doxygen XML output.</param>
        /// <param name="parentMethod">The parent method of the parameter 
        /// represented by <paramref name="param"/>.</param>
        public DefinedParameter( Param param, DefinedMember parentMethod )
        {
            this.Content = new ReferenceContent( param );
        }

        ///////////////////////////////////////////////////////////////////////
        #region Public Properties

        /// <summary>
        /// Gets or sets the name of the current parameter.
        /// </summary>
        /// <remarks><para>This is the simple name of the parameter, without
        /// decorations or keywords. </para>
        /// </remarks>
        public string Name
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets the content for the current parameter.
        /// </summary>
        /// <remarks><para>Currently, all content is harvested from code comments.</para>
        /// </remarks>
        public virtual ReferenceContent Content
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the method that the current parameter is bound to.
        /// </summary>
        public DefinedMember ParentMethod
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets the current parameter's type.
        /// </summary>
        /// <remarks><para>This is the type of the parameter, without
        /// decorations or keywords.</para>
        public abstract DefinedType Type
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the parameter is an array.
        /// </summary>
        public abstract bool IsArray
        {
            get;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the parameter is declared const.
        /// </summary>
        public abstract bool IsConst
        {
            get;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the parameter
        /// is an input to a method/function.
        /// </summary>
        public abstract bool IsInParam
        {
            get;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the parameter
        /// is an output from a method/function.
        /// </summary>
        public abstract bool IsOutParam
        {
            get;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the parameter is an optional 
        /// input to a method/function.
        /// </summary>
        public abstract bool IsOptional
        {
            get;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the parameter is returned
        /// by a method or function.
        /// </summary>
        public abstract bool IsReturnValue
        {
            get;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the parameter is a reference,
        /// in C++ syntax.
        /// </summary>
        public abstract bool IsReference
        {
            get;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the parameter is a pointer
        /// to a type.
        /// </summary>
        public abstract bool IsPointer
        {
            get;
        }

        /// <summary>
        /// Gets or sets a value indicating how many pointer characters 
        /// decorate the parameter declaration.
        /// </summary>
        /// <remarks>
        /// <para>Currently, only values of 0, 1, and 2 are supported.</para>
        /// </remarks>
        public abstract int PointerDepth
        {
            get;
        }

        /// <summary>
        /// Gets the position of the parameter in a list of parameters.
        /// </summary>
        /// <remarks><para>Currently, this isn't used.</para>
        /// </remarks>
        public abstract int Position
        {
            get;
        }

        #endregion

        public override string ToString()
        {
            string toString = String.Format( "{0}.{1} param", this.ParentMethod.Name, this.Name );
            return toString;
        }

        protected ReferenceContent _referenceContent;
    }
}
