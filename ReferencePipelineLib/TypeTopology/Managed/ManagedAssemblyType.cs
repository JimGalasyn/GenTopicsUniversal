using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OsgContentPublishing.ReferencePipelineLib.TypeTopology.Assembly;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology.Managed
{
    public class ManagedAssemblyType : DefinedType
    {
        public ManagedAssemblyType( ManagedType managedType, AssemblyType assemblyType )
        {

            //if( nativeType != null && assemblyType != null )
            //{
            //    this.NativeType = nativeType;
            //    this.AssemblyType = assemblyType;
            //    this.FullName = this.AssemblyType.FullName;
            //    this.Name = this.AssemblyType.Name;
            //    this.FriendlyName = Utilities.GetFriendlyName( this.AssemblyType );
            //    this.CopyContent();
            //}
            //else
            //{
            //    throw new ArgumentNullException();
            //}


            if( managedType != null && assemblyType != null )
            {
                this.ManagedType = managedType;
                this.AssemblyType = assemblyType;
                this.FullName = this.AssemblyType.FullName;
                this.Name = this.AssemblyType.Name;
                //    this.CopyContent();
            }
            else
            {
                throw new ArgumentNullException();
            }
        }

        protected override void InitializeMembers()
        {
        }

        protected override void InitializeBaseTypes()
        {
        }

        protected override void InitializeDerivedTypes()
        {
        }

        protected override void InitializeGenericTypes()
        {
        }

        ///////////////////////////////////////////////////////////////////////
        #region Public Properties

        public ManagedType ManagedType
        {
            get;
            protected set;
        }

        public AssemblyType AssemblyType
        {
            get;
            protected set;
        }

        public override DefinedType Namespace
        {
            get
            {
                throw new NotImplementedException();
                //return AssemblyType.CreateNamespaceType( this.UnderlyingType.Namespace );
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public override string FriendlyName
        {
            get
            {
                return Utilities.GetFriendlyName( this.AssemblyType );
            }
        }

        public override bool IsAbstract
        {
            get
            {
                throw new NotImplementedException();
                //return this.UnderlyingType.IsAbstract;
            }
        }

        public override bool IsAttribute
        {
            get
            {
                throw new NotImplementedException();
                //return this.UnderlyingType.IsAttribute;
            }
        }

        public override bool IsNamespace
        {
            get
            {
                throw new NotImplementedException();
                //return this.UnderlyingType.IsClass;
            }
        }

        public override bool IsGlobalNamespace
        {
            get
            {
                throw new NotImplementedException();
                //return this.UnderlyingType.IsClass;
            }
        }

        public override bool IsClass
        {
            get
            {
                throw new NotImplementedException();
                //return this.UnderlyingType.IsClass;
            }
        }

        public override bool IsDelegate
        {
            get
            {
                throw new NotImplementedException();
                //return this.UnderlyingType.IsDelegate;
            }
        }

        public override bool IsEnum
        {
            get
            {
                throw new NotImplementedException();
                //return this.UnderlyingType.IsEnum;
            }
        }

        public override bool IsGeneric
        {
            get
            {
                throw new NotImplementedException();
                //return this.UnderlyingType.IsGeneric;
            }
        }

        public override bool IsInterface
        {
            get
            {
                throw new NotImplementedException();
                //return this.UnderlyingType.IsInterface;
            }
        }

        public override bool IsPublic
        {
            get
            {
                throw new NotImplementedException();
                //return this.UnderlyingType.IsPublic;
            }
        }

        public override bool IsReferenceType
        {
            get
            {
                throw new NotImplementedException();
                //return !this.UnderlyingType.IsValueType;
            }
        }

        public override bool IsSealed
        {
            get
            {
                throw new NotImplementedException();
                //return this.UnderlyingType.IsSealed;
            }
        }

        public override bool IsStruct
        {
            get
            {
                throw new NotImplementedException();
                //return this.UnderlyingType.IsStruct;
            }
        }

        public override bool IsSystemType
        {
            get
            {
                throw new UnknownTypeException( this.Name );
            }
        }

        public override bool IsValueType
        {
            get
            {
                throw new NotImplementedException();
                //return this.UnderlyingType.IsValueType;
            }
        }


        public override List<DefinedType> ChildTypes
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override List<DefinedType> DerivedTypes
        {
            get { throw new NotImplementedException(); }
        }

        public override TypeModel TypeSystem
        {
            get { throw new NotImplementedException(); }
        }

        public override LanguageElement LanguageElement
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
        
    }
}
