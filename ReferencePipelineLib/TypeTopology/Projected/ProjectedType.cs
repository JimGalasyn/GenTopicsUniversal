using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen;
using OsgContentPublishing.ReferencePipelineLib.TypeTopology.Native;
using OsgContentPublishing.ReferencePipelineLib.TypeTopology.Assembly;
using ReflectionUtilities;

/// Contains classes for representing types that are projected
/// from Windows Runtime IDL (RIDL) into the Application Binary Interface (ABI)
/// and serialized to Windows Metadata (winmd) files.
namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology.Projected
{
    public class ProjectedType : DefinedType
    {
        ///////////////////////////////////////////////////////////////////////
        #region Construction

        public ProjectedType( DoxygenType nativeType, AssemblyType assemblyType )
        {
            if( nativeType != null && assemblyType != null )
            {
                this.NativeType = nativeType;
                this.NativeType.ParentProjectedType = this;
                this.AssemblyType = assemblyType;
                this.AssemblyType.ParentProjectedType = this;
                this.FullName = this.AssemblyType.FullName;
                this.Name = this.AssemblyType.Name;
                this.CopyContent();
                this.TopicId = this.AssemblyType.TopicId;
                this.SourceLanguage = this.NativeType.SourceLanguage;
            }
            else
            {
                throw new ArgumentNullException();
            }
        }

        private void CopyContent()
        {
            if( this.AssemblyType.Content.IsEmpty && 
                this.NativeType.Content.HasContent )
            {
                this.AssemblyType.Content = this.NativeType.Content;
            }

            var memberComparer = new Utilities.DefinedMemberComparer();
            foreach( var member in this.AssemblyType.Members )
            {
                DefinedMember matchingMember = this.NativeType.Members.Find( m => memberComparer.Equals( m, member ) );
                if( matchingMember != null )
                {
                    if( member.Content.IsEmpty &&
                        matchingMember.Content.HasContent )
                    {
                        member.Content = matchingMember.Content;
                    }

                    if( member.IsMethod )
                    {
                        AssemblyMethod method = member as AssemblyMethod;

                        foreach( var param in method.Parameters )
                        {
                            DoxygenMethod matchingMethod = matchingMember as DoxygenMethod;
                            if( matchingMethod != null )
                            {
                                DefinedParameter matchingParam = matchingMethod.Parameters.Find( p => p.Name == param.Name );
                                if( matchingParam != null )
                                {
                                    param.Content = matchingParam.Content;
                                }
                            }
                        }
                    }
                }
            }
        }


        public ProjectedType( ObservableType managedType, DoxType doxygenType )
            : base( managedType, doxygenType )
        {

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

        #endregion


        ///////////////////////////////////////////////////////////////////////
        #region Public Properties

        public DoxygenType NativeType
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
                if( this._namespace == null )
                {
                    string key = this.AssemblyType.Namespace.FullName;
                    this._namespace = TypeFactory.KnownProjectedTypes.ContainsKey( key ) ?
                        TypeFactory.KnownProjectedTypes[key] as DefinedType :
                        this.AssemblyType.Namespace;
                }

                return this._namespace;
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

        public override ReferenceContent Content
        {
            get
            {
                this._referenceContent = this.NativeType.Content;
                return this._referenceContent;
            }
            set
            {
                this._referenceContent = value;
            }
        }

        public override List<DefinedMember> Members
        {
            get
            {
                return this.AssemblyType.Members;
            }

            protected set
            {
            }
        }


        public override bool IsAbstract
        {
            get
            {
                return this.AssemblyType.IsAbstract;
            }
        }

        public override bool IsAttribute
        {
            get
            {
                return this.AssemblyType.IsAbstract;
            }
        }

        public override bool IsNamespace
        {
            get
            {
                return this.AssemblyType.IsNamespace;
            }
        }

        public override bool IsGlobalNamespace
        {
            get
            {
                return this.AssemblyType.IsGlobalNamespace;
            }
        }


        public override bool IsClass
        {
            get
            {
                return this.AssemblyType.IsClass;
            }
        }

        public override bool IsDelegate
        {
            get
            {
                return this.AssemblyType.IsDelegate;
            }
        }

        public override bool IsEnum
        {
            get
            {
                return this.AssemblyType.IsEnum;
            }
        }

        public override bool IsGeneric
        {
            get
            {
                return this.AssemblyType.IsGeneric;
            }
        }

        public override bool IsInterface
        {
            get
            {
                return this.AssemblyType.IsInterface;
            }
        }

        public override bool IsPublic
        {
            get
            {
                return this.AssemblyType.IsPublic;
            }
        }

        public override bool IsReferenceType
        {
            get
            {
                return this.AssemblyType.IsReferenceType;
            }
        }

        public override bool IsSealed
        {
            get
            {
                return this.AssemblyType.IsSealed;
            }
        }

        public override bool IsStruct
        {
            get
            {
                return this.AssemblyType.IsStruct;
            }
        }

        public override bool IsSystemType
        {
            get
            {
                return this.AssemblyType.IsSystemType;
            }
        }

        public override bool IsValueType
        {
            get
            {
                return this.AssemblyType.IsValueType;
            }
        }

        public override DefinedType ParentType
        {
            get
            {
                if( this._parentType == null )
                {
                    string key = this.AssemblyType.ParentType.FullName;
                    this._parentType = TypeFactory.KnownProjectedTypes.ContainsKey( key ) ?
                        TypeFactory.KnownProjectedTypes[key] as DefinedType :
                        this.AssemblyType.ParentType;
                }

                return this._parentType;
            }
        }


        public override List<DefinedType> ChildTypes
        {
            get
            {
                if( this._childTypes == null )
                {
                    this._childTypes = this.AssemblyType.ChildTypes.Select( t =>

                        TypeFactory.KnownProjectedTypes.ContainsKey( t.FullName ) ?
                        TypeFactory.KnownProjectedTypes[t.FullName] as DefinedType :
                        t ).ToList();
                }

                this._childTypes.ForEach( t => t.ParentType = this );

                return this._childTypes;
            }
        }

        public override List<DefinedType> BaseTypes
        {
            get
            {
                if( this._baseTypes == null )
                {
                    this._baseTypes = this.AssemblyType.BaseTypes.Select( t =>
                        TypeFactory.KnownProjectedTypes.ContainsKey( t.FullName ) ?
                        TypeFactory.KnownProjectedTypes[t.FullName] as DefinedType :
                        t ).ToList();
                }

                // TBD: Remove existing!
                // TBD: Rely on DerivedTypes property to to the reverse hookup
                //this._baseTypes.ForEach( t => t.DerivedTypes.Add( this ) );

                return this._baseTypes;
            }

            protected set
            {
                throw new NotImplementedException();
            }
        }

        //private List<DefinedType> ReplaceAssemblyTypesWithProjectedTypes( List<DefinedType> typesToFilter )
        //{

        //}

        public override List<DefinedType> DerivedTypes
        {
            get
            {
                if( this._derivedTypes == null )
                {
                    this._derivedTypes = this.AssemblyType.DerivedTypes.Select( t =>
                        TypeFactory.KnownProjectedTypes.ContainsKey( t.FullName ) ?
                        TypeFactory.KnownProjectedTypes[t.FullName] as DefinedType :
                        t ).ToList();
                }

                // TBD: Rely on DerivedTypes property to to the reverse hookup
                //this._baseTypes.ForEach( t => t.DerivedTypes.Add( this ) );

                return this._derivedTypes;
            }
        }


        public override TypeModel TypeSystem
        {
            get { return TypeModel.Projected; }
        }

        public override LanguageElement LanguageElement
        {
            get { return this.AssemblyType.LanguageElement; }
        }

        #endregion

        public override string ToString()
        {
            string toString = String.Format( "{0} projection", this.AssemblyType.ToString() );
            return toString;
        }
    }
}
