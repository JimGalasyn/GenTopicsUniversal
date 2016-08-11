using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology.Assembly
{
    /// <summary>
    /// Represents a member that was loaded from a managed assembly or
    /// Windows Metadata (winmd) file.
    /// </summary>
    public abstract class AssemblyMember : DefinedMember
    {
        /// <summary>
        /// Initializes a new <see cref="AssemblyMember"/> instance from the specified
        /// <see cref="MemberInfo"/> and parent type.
        /// </summary>
        /// <param name="memberInfo">The member information.</param>
        /// <param name="parentType">The <see cref="AssemblyType"/> parent.</param>
        public AssemblyMember( MemberInfo memberInfo, AssemblyType parentType )
            : base( memberInfo, parentType )
        {
            if( memberInfo != null && parentType != null )
            {
                this.UnderlyingMember = memberInfo;
                this.ParentType = parentType;
                this.InitializeAttributes();
                this.TopicId = Utilities.GetTopicIdForMember( this.UnderlyingMember, parentType.UnderlyingType );
            }
            else
            {
                throw new ArgumentNullException();
            }
        }

        /// <summary>
        /// Creates a new member from the specified <see cref="MemberInfo"/> and 
        /// parent type.
        /// </summary>
        /// <param name="memberInfo">The member information.</param>
        /// <param name="parentType">The <see cref="AssemblyType"/> parent.</param>
        /// <returns></returns>
        public static AssemblyMember CreateMember( MemberInfo memberInfo, AssemblyType parentType )
        {
            AssemblyMember newMember = null;

            if( memberInfo != null )
            {
                if( !memberInfo.DeclaringType.Name.StartsWith( "System." ) )
                {
                    switch( memberInfo.MemberType )
                    {
                        case MemberTypes.Constructor:
                            {
                                newMember = new AssemblyConstructor( memberInfo as ConstructorInfo, parentType );
                                break;
                            }

                        case MemberTypes.Event:
                            {
                                newMember = new AssemblyEvent( memberInfo as EventInfo, parentType );
                                break;
                            }

                        case MemberTypes.Field:
                            {
                                newMember = new AssemblyField( memberInfo as FieldInfo, parentType );
                                break;
                            }

                        case MemberTypes.Method:
                            {
                                newMember = new AssemblyMethod( memberInfo as MethodInfo, parentType );
                                break;
                            }

                        case MemberTypes.Property:
                            {
                                newMember = new AssemblyProperty( memberInfo as PropertyInfo, parentType );
                                break;
                            }

                        default:
                            {
                                throw new ArgumentException( "Unknown MemberType", "memberInfo" );
                            }
                    }
                }
            }
            else
            {
                throw new ArgumentNullException( "memberInfo" );
            }

            // TBD: do something if newMember is null. 
            return newMember;
        }

        /// <summary>
        /// Gets the member information as represented by <see cref="System.Reflection"/>.
        /// </summary>
        public MemberInfo UnderlyingMember
        {
            get;
            protected set;
        }

        public override bool IsEnumValue
        {
            get
            {
                // TBD
                return false;
                //return this.UnderlyingMember
            }
        }

        public override bool IsField
        {
            get
            {
                return( this.UnderlyingMember is FieldInfo );
            }
        }

        public override bool IsProperty
        {
            get
            {
                return( this.UnderlyingMember is PropertyInfo );
            }
        }

        public override bool IsPropertyAccessor
        {
            get
            {
                return ( this.UnderlyingMember is MethodInfo &&
                    this.UnderlyingMember.Name.StartsWith( "get_" ) ||
                    this.UnderlyingMember.Name.StartsWith( "put_" ) );
            }
        }

        public override bool IsSystemObjectMember
        {
            get
            {
                bool isSystemObjectMember = false;

                if( this.ParentType.BaseType != null &&
                    this.ParentType.BaseType.Name == "Object" &&
                    !this.IsConstructor )
                {
                    isSystemObjectMember = ( this.ParentType.BaseType.Members.FirstOrDefault( m => m.Name == this.Name ) != null );
                }

                return isSystemObjectMember;
            }
        }

        public override bool IsMethod
        {
            get
            {
                return( this.UnderlyingMember is MethodInfo );
            }
        }

        /// <summary>
        /// Gets a value indicating whether the member is a constructor.
        /// </summary>
        public override bool IsConstructor
        {
            get
            {
                return( this.UnderlyingMember is ConstructorInfo );
            }
        }

        /// <summary>
        /// Gets a value indicating whether the member is a destructor.
        /// </summary>
        /// <remarks>
        /// <para>This happens rarely in managed code.</para>
        /// </remarks>
        public override bool IsDestructor
        {
            get
            {
                return ( this.UnderlyingMember.Name.Contains( "~" ) );
            }
        }

        public override bool IsEvent
        {
            get
            {
                return( this.UnderlyingMember is EventInfo );
            }
        }

        public List<CustomAttributeData> AttributeData
        {
            get;
            private set;
        }

        public override bool IsEventAccessor
        {
            get
            {
                return( this.UnderlyingMember is MethodInfo && 
                    this.UnderlyingMember.Name.StartsWith( "add_" ) ||
                    this.UnderlyingMember.Name.StartsWith( "remove_" ) );
            }
        }

        protected void InitializeAttributes()
        {
            var attributes = this.UnderlyingMember.CustomAttributes.ToList();
            if( attributes.Count > 0  )
            {
                this.Attributes = attributes.Select( a => 
                    TypeFactory.CreateAssemblyType( a ) as DefinedType ).ToList();

                this.AttributeData = this.UnderlyingMember.GetCustomAttributesData().ToList();
            }
            else
            {
                this.Attributes = new List<DefinedType>();
                this.AttributeData = new List<CustomAttributeData>();
            }
        }

        protected override void InitializeGenericTypes()
        {
            //throw new NotImplementedException();
        }


        public override TypeModel TypeSystem
        {
            get { return TypeModel.Assembly; }
        }

        public override LanguageElement LanguageElement
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Back the <see cref="Type"/> property.
        /// </summary>
        protected AssemblyType _type;
    }
}
