using OsgContentPublishing.ReferencePipelineLib.Deserializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen
{
    public class Param
    {
        public Param( XElement element )
        {
            this.type = Utilities.TryGetChildElementValue( element, "type" );
            this.declname = Utilities.TryGetChildElementValue( element, "declname" );
            this.defname = Utilities.TryGetChildElementValue( element, "defname" );
            this.defval = Utilities.TryGetChildElementValue( element, "defval" );
            this.array = Utilities.TryGetChildElementValue( element, "array" );
            this.attributes = Utilities.TryGetChildElementValue( element, "attributes" );

            this.BriefDescription = DoxygenDeserializer.GetBriefDescription( element );

            this.FixupType();
            this.InitFromTypeDeclaration( this.type );
            this.AssignAttributes();
        }

        private void InitFromTypeDeclaration( string typeDeclaration )
        {
            TypeDeclarationParseResults parseResults = Utilities.ParseRawTypeDeclaration( this.type );

            this.ParseResults = parseResults;
            this.IsArray = parseResults.IsArray;
            this.IsConst = parseResults.IsConst;
            this.IsEventArgs = parseResults.IsEventArgs;
            this.IsEventToken = parseResults.IsEventToken;
            this.IsGenericType = parseResults.IsGeneric;
            this.IsGenericTypeName = parseResults.IsGenericParam;
            this.IsInParam = parseResults.IsInParam;            
            this.IsOutParam = parseResults.IsOutParam;
            this.IsPointer = parseResults.IsPointer;
            this.IsReference = parseResults.IsReference;
            this.IsRuntimeClassReference = parseResults.IsRuntimeClassReference;
            this.IsOptional = parseResults.IsOptional;
            this.PointerDepth = parseResults.PointerDepth;

            this.ParentType = parseResults.ParentType;
            this.Namespace = parseResults.Namespace;
            this.FullName = parseResults.FullName;
            this.TypeName = parseResults.TypeName;

            // TBD: Not sure why this assignment was happening...
            //this.FullName = this.declname;
        }

        public TypeDeclarationParseResults ParseResults
        {
            get;
            private set;
        }

        public Description BriefDescription
        {
            get;
            set;
        }

        public string type
        {
            get;
            private set;
        }

        public DoxType DoxType
        {
            get;
            set;
        }

        public string declname
        {
            get;
            private set;
        }

        public string defname
        {
            get;
            private set;
        }

        public string defval
        {
            get;
            private set;
        }

        public string array
        {
            get;
            private set;
        }

        public string briefdescription
        {
            get;
            private set;
        }

        public string attributes
        {
            get; 
            private set;
        }

        public bool IsArray
        {
            get;
            private set;
            //{
            //    return this.type.Contains( Utilities.arrayCharacters );
            //}
        }

        public bool IsConst
        {
            get;
            private set;
            //{
            //    return this.type.Contains( Utilities.constString );
            //}
        }

        public bool IsEventArgs
        {
            get;
            private set;
        }

        public bool IsEventToken
        {
            get;
            private set;
        }

        public bool IsGenericType
        {
            get;
            set;
        }

        public bool IsGenericTypeName
        {
            get;
            private set;
        }

        public bool IsInParam
        {
            get;
            set;
        }

        public bool IsOptional
        {
            get;
            set;
        }

        public bool IsOutParam
        {
            get;
            set;
        }

        public bool IsPointer
        {
            get;
            set;
        }

        public bool IsReference
        {
            get;
            private set;
        }

        public bool IsReturnValue
        {
            get;
            set;
        }

        public bool IsRuntimeClassReference
        {
            get;
            set;
        }

        public int PointerDepth
        {
            get;
            set;
        }

        public string TypeName
        {
            get;
            set;
        }

        public string Namespace
        {
            get;
            set;
        }

        public string ParentType
        {
            get;
            set;
        }


        public string FullName
        {
            get;
            set;
        }


        /// <summary>
        /// This is a workaround for a Doxygen bug, in which some template 
        /// parameters are reported as having type == "typename".
        /// </summary>
        private void FixupType()
        {
            // Example breakage:
            //
            // <templateparamlist>
            // <param>
            //   <type>typename</type> 
            //   <declname>T</declname>
            //   <defname>T</defname>
            // </param>
            // </templateparamlist>
            if( this.type == Utilities.typenameString ||
                this.type == Utilities.classString )
            {
                this.type = this.declname;
            }
        }

        private void AssignAttributes()
        {
            if( !String.IsNullOrEmpty( this.attributes ) )
            {
                if( this.attributes == Utilities.inString )
                {
                    this.IsInParam = true;
                }                 
                else if( this.attributes == Utilities.outString )
                {
                    this.IsOutParam = true;
                }

                // TBD: other attributes
            }
        }

        public override string ToString()
        {
            string toString = String.Format( "{0} param", this.FullName );
            return toString;
        }
    }
}
