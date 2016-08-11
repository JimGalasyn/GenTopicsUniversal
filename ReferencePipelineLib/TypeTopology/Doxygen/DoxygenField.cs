using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology.Doxygen
{
    public class DoxygenField : DoxygenMember
    {
        public DoxygenField( MemberDef memberDef, DoxygenType parentType )
            : base( memberDef, parentType )
        {
        }

        public DoxygenField( EnumValue enumValue, DoxygenType parentType )
            : base( null, parentType )
        {
            this.Name = enumValue.name;
            this.Type = PrimitiveTypes.Int;

            if( enumValue.initializer != null )
            {
                string initValueString = string.Empty;
                try
                {
                    initValueString = enumValue.initializer.Replace( "= ", String.Empty );
                    initValueString = initValueString.Replace( "=", String.Empty );
                    initValueString = initValueString.Replace( " ", String.Empty );
                    this.IsHexadecimal = initValueString.StartsWith( "0x" );
                    if( this.IsHexadecimal )
                    {
                        string initValueStringNoPrefix = initValueString.Replace( "0x", String.Empty );
                        this.Value = int.Parse( initValueStringNoPrefix, NumberStyles.AllowHexSpecifier );
                    }
                    else
                    {
                        this.Value = int.Parse( initValueString );
                    }
                }
                catch( Exception ex )
                {
                    this.Value = initValueString;
                    Debug.WriteLine( ex.ToString() );
                }
            }

            this.Content = new ReferenceContent( enumValue );
        }

        public override DefinedType Type
        {
            get
            {
                if( this._type == null )
                {
                    if( this.UnderlyingMember.IsEnum )
                    {
                        this._type = PrimitiveTypes.Int;
                    }
                    else
                    {
                        string rawFieldType = this.UnderlyingMember.type;

                        // TBD: Hack to remove embedded spaces
                        if( rawFieldType.StartsWith( "Windows " ) )
                        {
                            rawFieldType = rawFieldType.Replace( ' ', '.' );
                        }

                        this._parseResults = Utilities.ParseRawTypeDeclaration( rawFieldType );
                        string fieldType = this._parseResults.FullName;

                        this._type = TypeFactory.CreateType( fieldType );
                    }
                }

                return this._type;
            }

            set
            {
                this._type = value;
            }
        }


        public bool HasValue
        {
            get
            {
                return ( this.Value != null );
            }
        }

        public bool IsHexadecimal
        {
            get;
            protected set;
        }

        public override LanguageElement LanguageElement
        {
            get { return LanguageElement.Field; }
        }

        public override string ToString()
        {
            string toString = null;

            if( this.HasValue )
            {
                toString = String.Format( "{0}.{1} = {2}", this.ParentType.Name, this.Name, this.Value.ToString() );
            }
            else
            {
                toString = String.Format( "{0}.{1}", this.ParentType.Name, this.Name );
            }

            return toString;
        }

        TypeDeclarationParseResults _parseResults;
    }
}
