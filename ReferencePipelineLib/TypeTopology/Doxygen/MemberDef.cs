using OsgContentPublishing.ReferencePipelineLib.Deserializers;
//using ReflectionUtilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen
{
    public class MemberDef
    {
        public MemberDef( XElement element, DoxType parentType )
        {
            this.ParentType = parentType;

            this.BriefDescription = DoxygenDeserializer.GetBriefDescription( element );
            this.DetailedDescription = DoxygenDeserializer.GetDetailedDescription( element );

            name = Utilities.TryGetChildElementValue( element, "name" );
            id = Utilities.TryGetAttributeValue( element, "id" );
            kind = Utilities.TryGetAttributeValue( element, "kind" );
            type = Utilities.TryGetChildElementValue( element, "type" );

            // TBD: A hack to work around embedded ref element in some type elements. Need to make GetChildElement smarter.
            // <type><ref refid="_windows_8_b_8idl_1a32af05d0457a86641b5ff9392de94cb7" kindref="member">MIRAGE_EXPERIMENTAL</ref> runtimeclass</type>
            //if( type != null && type.Contains( "runtimeclass" ) ) 
            //{
            //    this.kind = "class";
            //}

            prot = Utilities.TryGetAttributeValue( element, "prot" );
            compoundname = Utilities.TryGetAttributeValue( element, "compoundname" );
            definition = Utilities.TryGetChildElementValue( element, "definition" );
            this.FixupDefinition();

            argsstring = Utilities.TryGetChildElementValue( element, "argsstring" );
            briefdescription = Utilities.TryGetChildElementValue( element, "briefdescription" );
            detaileddescription = Utilities.TryGetChildElementValue( element, "detaileddescription" );
            inbodydescription = Utilities.TryGetChildElementValue( element, "inbodydescription" );

            IsVirtual = DoxygenDeserializer.ParseVirt( Utilities.TryGetAttributeValue( element, "virtual" ) );
            IsStatic = DoxygenDeserializer.ParseYesNo( Utilities.TryGetAttributeValue( element, "static" ) );
            IsExplicit = DoxygenDeserializer.ParseYesNo( Utilities.TryGetAttributeValue( element, "explicit" ) );
            IsMutable = DoxygenDeserializer.ParseYesNo( Utilities.TryGetAttributeValue( element, "mutable" ) );
            IsConst = DoxygenDeserializer.ParseYesNo( Utilities.TryGetAttributeValue( element, "const" ) );

            this.ReadTemplateParams( element );

            XElement locationElement = Utilities.TryGetChildElement( element, "location" );
            if( locationElement != null )
            {
                fileLocation = locationElement.Attribute( "file" ).Value;
                line = int.Parse( locationElement.Attribute( "line" ).Value );
                column = int.Parse( locationElement.Attribute( "column" ).Value );
            }

            if( this.IsMethod )
            {
                var paramElements = Utilities.GetChildElements( element, "param" );
                this.Parameters = paramElements.Select( p => new Param( p ) ).ToList();

                if( this.DetailedDescription.HasParametersPara )
                {
                    var parameterItems = this.DetailedDescription.ParametersPara.ParameterItems;

                    foreach( var param in this.Parameters )
                    {
                        var item = parameterItems.FirstOrDefault( i => i.ParameterName == param.FullName );
                        if( item != null )
                        {
                            param.BriefDescription = new Description( item.ParameterDescription );
                            //TBD: param.Position = item.Position;
                        }
                    }
                }
            }

            this.FixupType();

            if( this.ParentType != null )
            {
                this.language = this.ParentType.language;
            }

            if( this.IsEnum )
            {
                var enumvalueElements = Utilities.GetChildElements( element, "enumvalue" );
                this.EnumValues = enumvalueElements.Select( e => new EnumValue( e ) ).ToList();
            }
        }

        public string TopicId
        {
            get;
            private set;
        }

        public DoxType ParentType
        {
            get;
            private set;
        }

        public string id
        {
            get;
            private set;
        }

        public string kind
        {
            get;
            private set;
        }

        public string language
        {
            get;
            private set;
        }

        public string prot
        {
            get;
            private set;
        }

        public string compoundname
        {
            get;
            private set;
        }

        public bool? IsStatic
        {
            get;
            private set;
        }

        public bool? IsConst
        {
            get;
            private set;
        }

        public bool? IsExplicit
        {
            get;
            private set;
        }

        public bool? IsInline
        {
            get;
            private set;
        }

        public bool IsVirtual
        {
            get;
            private set;
        }

        public bool IsTypedef
        {
            get
            {
                return( this.kind == "typedef" );
            }
        }

        public bool IsClass
        {
            get
            {
                return ( this.kind == "class" );
            }
        }


        public bool? IsMutable
        {
            get;
            private set;
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

        public string definition
        {
            get;
            private set;
        }

        public string argsstring
        {
            get;
            private set;
        }

        public string name
        {
            get;
            private set;
        }

        public string briefdescription
        {
            get;
            private set;
        }

        public string detaileddescription
        {
            get;
            private set;
        }

        public string inbodydescription
        {
            get;
            private set;
        }

        public Description BriefDescription
        {
            get;
            set;
        }

        public Description DetailedDescription
        {
            get;
            set;
        }

        public string fileLocation
        {
            get;
            private set;
        }

        public int line
        {
            get;
            private set;
        }

        public int column
        {
            get;
            private set;
        }

        public bool IsProperty
        {
            get
            {
                return this.kind == "property";
            }
        }

        public bool IsMethod
        {
            get
            {
                return( this.kind == "function"  && !this.IsEvent );
            }
        }

        public bool IsConstructor
        {
            get
            {
                return ( this.kind == "constructor" );
            }
        }

        public bool IsDestructor
        {
            get
            {
                return ( this.kind == "destructor" );
            }
        }


        public bool IsField
        {
            get
            {
                return this.kind == "variable" || this.kind == "field";
            }
        }

        public bool IsEvent
        {
            get
            {
                return this.kind == "event";
            }
            //private set; // Assigned in ctor
        }

        public bool IsEnum
        {
            get
            {
                return ( this.kind == "enum" );
            }
        }

        public bool HasParameters
        {
            get
            {
                return ( this.Parameters != null && this.Parameters.Count > 0 );
            }
        }

        public List<Param> Parameters
        {
            get; private set;
        }

        public List<Param> GenericParameters
        {
            get
            {
                return this._genericParameters;
            }
        }

        private void ReadTemplateParams( XElement element )
        {
            List<Param> genericParameters = DoxygenDeserializer.ReadTemplateParams( element );
            if( genericParameters != null )
            {
                this._genericParameters = genericParameters;
            }
            else
            {
                this._genericParameters = new List<Param>();
            }
        }

        public List<EnumValue> EnumValues
        {
            get;
            private set;
        }

        private void FixupDefinition()
        {
            if( this.definition != null )
            {
                if( this.definition.StartsWith( this.type ) )
                {
                    // Remove return value from signature.
                    this.definition = this.definition.Remove( 0, this.type.Length + 1 ); // +1 to pick up the space between return type and function name
                }

                // to handle munging of this sort:
                //<definition>Microsoft:: Xbox:: Services:: Achievements:: AchievementMediaAssetType AchievementMediaAsset::MediaAssetType</definition>
                this.definition = this.definition.Replace( ":: ", "::" );
            }

        }

        private void FixupType()
        {
            // TBD: A hack to work around embedded ref element in some type elements. Need to make GetChildElement smarter.
            // <type><ref refid="_windows_8_b_8idl_1a32af05d0457a86641b5ff9392de94cb7" kindref="member">MIRAGE_EXPERIMENTAL</ref> runtimeclass</type>
            if( this.type != null && this.type.Contains( "runtimeclass" ) )
            {
                this.kind = "class";
            }

            if( this.IsMethod )
            {
                if( this.Parameters.Any( p => p.IsEventToken ) )
                {
                    this.kind = "event";
                    //this.IsEvent = true;
                    // TBD: kind = "event", although this doesn't exist in Doxygen's schema
                }

                if( this.ParentType != null )
                {
                    string dtorName = "~" + this.ParentType.Name;
                    if( this.name == this.ParentType.Name )
                    {
                        // TBD: dtor
                        this.kind = "constructor";
                        this.type = this.ParentType.Name;
                    }
                    else if( this.name == dtorName )
                    {
                        this.kind = "destructor";
                        this.type = "void";
                    }
                }
            }
        }

        public override string ToString()
        {
            string toString = String.Format( "{0} | {1}", this.name, this.kind );
            return toString;
        }

        private List<Param> _genericParameters;
    }
}
