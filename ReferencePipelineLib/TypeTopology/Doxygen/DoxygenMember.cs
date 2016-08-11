using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen
{
    /// <summary>
    /// Represents a member that was deserialized from Doxygen's XML output.
    /// </summary>
    public class DoxygenMember : DefinedMember
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DoxygenMember"/> class to the
        /// specified <see cref="MemberDef"/> and parent type.
        /// </summary>
        /// <param name="memberDef"></param>
        /// <param name="parentType"></param>
        public DoxygenMember( MemberDef memberDef, DoxygenType parentType )
            : base( memberDef, parentType )
        {
            if( memberDef != null )
            {
                this.UnderlyingMember = memberDef;
                this.Content = new ReferenceContent( this.UnderlyingMember );
            }

            if( parentType != null )
            {
                this.ParentType = parentType;
            }
            else
            {
                if( memberDef.ParentType == null )
                {
                    // Assign global namespace
                    this.ParentType = TypeFactory.KnownTypes[String.Empty];
                }
                else
                {
                    this.ParentType = TypeFactory.CreateType( memberDef.ParentType );
                }
            }
        }

        /// <summary>
        /// Creates a new member from the specified <see cref="MemberDef"/> and 
        /// parent type.
        /// </summary>
        /// <param name="memberDef">A deserialized Doxygen memberdef element.</param>
        /// <param name="parentType">The <see cref="DoxygenType"/> parent.</param>
        /// <returns></returns>
        public static DoxygenMember CreateMember( MemberDef memberDef, DoxygenType parentType )
        {
            DoxygenMember newMember = null;

            if( memberDef != null )
            {
                switch( memberDef.kind )
                {
                    case "constructor":
                        {
                            newMember = new DoxygenConstructor( memberDef, parentType );
                            break;
                        }

                    case "destructor":
                        {
                            newMember = new DoxygenDestructor( memberDef, parentType );
                            break;
                        }

                    case "event":
                        {
                            newMember = new DoxygenEvent( memberDef, parentType );
                            break;
                        }

                    case "variable":
                    case "enum":
                    case "friend":
                        {
                            // case enum to handle nested enums. Seems to work...
                            newMember = new DoxygenField( memberDef, parentType );
                            break;
                        }

                    case "function":
                        {
                            newMember = new DoxygenMethod( memberDef, parentType );
                            break;
                        }

                    case "property":
                        {
                            newMember = new DoxygenProperty( memberDef, parentType );
                            break;
                        }

                    case "typedef":
                        {
                            // TBD: figure out what to do here.
                            break;
                        }

                    default:
                        {
                            throw new ArgumentException( "Unknown member type", "memberDef" );
                        }
                }
            }
            else
            {
                throw new ArgumentNullException( "memberDef" );
            }

            return newMember;
        }

        /// <summary>
        /// Creates an enumeration value from the specified <see cref="EnumValue"/> and
        /// parent type.
        /// </summary>
        /// <param name="enumValue">The value of the enumeration member.</param>
        /// <param name="parentType">The <see cref="DoxygenType"/> parent.</param>
        /// <returns></returns>
        public static DoxygenMember CreateEnumValue( EnumValue enumValue, DoxygenType parentType )
        {
            DoxygenMember newMember = null;

            if( enumValue != null )
            {
                newMember = new DoxygenField( enumValue, parentType );
            }
            else
            {
                throw new ArgumentNullException( "enumValue" );
            }

            return newMember;
        }

        /// <summary>
        /// Gets the member information as represented by Doxygen's memberdef element.
        /// </summary>
        public MemberDef UnderlyingMember
        {
            get;
            protected set;
        }

        public override DefinedType Type
        {
            get
            {
                if( this._type == null && this.UnderlyingMember != null )
                {
                    this._type = TypeFactory.CreateType( this.UnderlyingMember.type );
                }

                return this._type;
            }

            set
            {
                this._type = value;
            }
        }

        public override bool IsEnumValue
        {
            get
            {
                // TBD
                return false;
                //return this.UnderlyingMember.Is
            }
        }

        public override bool IsField
        {
            get
            {
                return this.UnderlyingMember.IsField;
            }
        }

        public override bool IsProperty
        {
            get
            {
                return this.UnderlyingMember.IsProperty;
            }
        }

        public override bool IsPropertyAccessor
        {
            get
            {
                // TBD: This test needs validating.
                return ( this.UnderlyingMember.IsMethod &&
                    this.UnderlyingMember.name.StartsWith( "get_" ) ||
                    this.UnderlyingMember.name.StartsWith( "put_" ) );
            }
        }

        public override bool IsMethod
        {
            get
            {
                if( this.UnderlyingMember != null )
                {
                    return this.UnderlyingMember.IsMethod;
                }
                else
                {
                    return false;
                }
            }
        }

        public override bool IsConstructor
        {
            get
            {
                return this.UnderlyingMember.IsConstructor;
            }
        }

        public override bool IsDestructor
        {
            get
            {
                return this.UnderlyingMember.IsDestructor;
            }
        }

        public override bool IsEvent
        {
            get
            {
                return this.UnderlyingMember.IsEvent;
            }
        }


        public override bool IsEventAccessor
        {
            get
            {
                // TBD: This test needs validating.
                return ( this.UnderlyingMember.IsMethod &&
                    this.UnderlyingMember.name.StartsWith( "add_" ) ||
                    this.UnderlyingMember.name.StartsWith( "remove_" ) );
            }
        }

        public override bool IsPublic
        {
            get
            {
                // TBD
                return true;
                //return this.UnderlyingMember.Is
            }
        }

        public override bool IsPrivate
        {
            get
            {
                // TBD
                return !this.IsPublic;
            }
        }

        public override bool IsStatic
        {
            get
            {
                return ( this.UnderlyingMember.IsStatic == true ? true : false );
            }
        }

        public override bool IsSystemObjectMember
        {
            get
            {
                // TBD: This probably isn't the implementation we want here.
                bool isSystemObjectMember = false;

                if( this.ParentType.BaseType != null &&
                    this.ParentType.BaseType.Name == "Object" )
                {
                    isSystemObjectMember = ( this.ParentType.BaseType.Members.FirstOrDefault( m => m.Name == this.Name ) != null );
                }

                return isSystemObjectMember;
            }
        }
        public override bool IsConst
        {
            get
            {
                return ( this.UnderlyingMember.IsConst == true ? true : false );
            }
        }

        public override bool IsMutable
        {
            get
            {
                return ( this.UnderlyingMember.IsMutable == true ? true : false );
            }
        }

        public override TypeModel TypeSystem
        {
            get { return TypeModel.Doxygen; }
        }

        public override LanguageElement LanguageElement
        {
            get { throw new NotImplementedException(); }
        }

        protected override void InitializeGenericTypes()
        {
            //this.GenericParameterTypes = this.UnderlyingMember.GenericParameters.Select( p =>
            //    TypeFactory.CreateType( p.type ) as DefinedType ).ToList();

            this.GenericParameterTypes = this.UnderlyingMember.GenericParameters.Select( p =>
                TypeFactory.CreateType( p.FullName ) as DefinedType ).ToList();

        }

        protected DefinedType _type;

    }
}
