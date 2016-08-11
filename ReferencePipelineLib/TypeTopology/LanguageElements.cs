using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsgContentPublishing.ReferencePipelineLib.TypeTopology
{
    public class LanguageElements
    {
        public static LanguageElement Attribute
        {
            get
            {
                return LanguageElement.Type | LanguageElement.Attribute;
            }
        }

        public static LanguageElement Struct
        {
            get
            {
                return LanguageElement.Type | LanguageElement.Struct;
            }
        }

        public static string GetLanguageElementName( LanguageElement languageElement )
        {
            string name = Enum.GetName( typeof( LanguageElement ), languageElement );
            return name;
        }
    }
}
