using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology
{
    /// <summary>
    /// Represents the results of parsing a type declaration string.
    /// </summary>
    /// <remarks>
    /// <para>Call the <see cref="ParseTypeDeclaration"/> method to parse
    /// a type name from a type declaration string.</para>
    /// <para>The <see cref="TypeDeclarationParseResults"/> class represents
    /// the context in which a type appears. The context includes decorations
    /// like pointer and reference syntax, and keywords like const and readonly.
    /// </para>
    /// </remarks>
    public class TypeDeclarationParseResults
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TypeDeclarationParseResults"/> class.
        /// </summary>
        public TypeDeclarationParseResults()
        {
            this.PointerDepth = 0;
        }

        /// <summary>
        /// Gets or sets the type declaration string as it appears in the 
        /// output XML from Doxygen, before GTU does any processing on it.
        /// </summary>
        public string RawTypeDeclaration
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the type is an array.
        /// </summary>
        public bool IsArray
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the type is a COM smart pointer.
        /// </summary>
        public bool IsComPtr
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the type is declared const.
        /// </summary>
        public bool IsConst
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the type represents event arguments.
        /// </summary>
        public bool IsEventArgs
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the type is an event token, used
        /// for registering events with the runtime.
        /// </summary>
        public bool IsEventToken
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the type is a generic 
        /// or a template class.
        /// </summary>
        public bool IsGeneric
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the type is used as 
        /// a generic or template parameter in another type's declaration.
        /// </summary>
        public bool IsGenericParam
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the type is an in parameter
        /// in a method or function.
        /// </summary>
        public bool IsInParam
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the type is nullable, 
        /// which uses the "?" syntax in C#.
        /// </summary>
        public bool IsNullable
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the type is an optional 
        /// parameter in a method or function.
        /// </summary>
        public bool IsOptional
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the type is an out 
        /// parameter in a method or function.
        /// </summary>
        public bool IsOutParam
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the type is referenced 
        /// as a pointer.
        /// </summary>
        public bool IsPointer
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the type is read only.
        /// </summary>
        public bool IsReadOnly
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the type is a reference,
        /// in C++ syntax.
        /// </summary>
        public bool IsReference
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the type is returned
        /// by a method or function.
        /// </summary>
        public bool IsReturnValue
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the type is a runtime class 
        /// parameter in a method or function.
        /// </summary>
        public bool IsRuntimeClassReference
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating how many pointer characters 
        /// decorate the type declaration.
        /// </summary>
        /// <remarks>
        /// <para>Currently, only values of 0, 1, and 2 are supported.</para>
        /// </remarks>
        public int PointerDepth
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the type name that was discovered by the 
        /// <see cref="ParseTypeDeclaration"/> method. 
        /// </summary>
        /// <remarks>
        /// <para>The type name is the name without any decoration,
        /// namespace, and generic parameters. The <see cref="FullName"/>
        /// property is the type name prepended with the parent namespace
        /// or parent type name.
        /// </para></remarks>
        public string TypeName
        {
            get
            {
                if( this._typeName == null )
                {
                    this._typeName = Utilities.GetTypeName( this.FullName );
                }

                return this._typeName;
            }
        }

        /// <summary>
        /// Gets the namespace of the type specified by the <see cref="FullName"/> property.
        /// </summary>
        /// <remarks>
        /// <para>Keep in mind that during the parsing pass, <see cref="ParseRawTypeDeclaration"/>
        /// can't distinguish between a type's namespace and a type's parent class.
        /// </para>
        /// </remarks>
        public string Namespace
        {
            get
            {
                if( this._namespace == null )
                {
                    this._namespace = Utilities.GetParentNamespaceFull( this.FullName );
                }

                return this._namespace;
            }
        }

        /// <summary>
        /// Gets the name of the current type's parent type.
        /// </summary>
        /// <remarks>
        /// <para>Keep in mind that during the parsing pass, <see cref="ParseRawTypeDeclaration"/>
        /// cant' distinguish between a type's namespace and a type's parent class.
        /// </para>
        /// </remarks>
        public string ParentType
        {
            get
            {
                if( this._parentTypeName == null )
                {
                    this._parentTypeName = Utilities.GetParentNamespaceFull( this.FullName );
                }

                return this._parentTypeName;
            }
        }

        /// <summary>
        /// Gets or sets the name of the type prepended with the 
        /// name of the type's namespace or parent type.
        /// </summary>
        public string FullName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the generic parameter type names, if the
        /// type is declared to be a generic or template class.
        /// </summary>
        public List<TypeDeclarationParseResults> GenericParameterTypes
        {
            get;
            set;
        }

        private string _typeName;
        private string _namespace;
        private string _parentTypeName;
    }
}
