using System;
using System.Diagnostics;
using System.IO;
using System.Text;

#if false
using NUnit.Framework;
#endif

namespace OsgContentPublishing.ReferencePipelineLib
{
    /// <summary>
    /// A StreamReader that excludes XML-illegal characters while reading.
    /// </summary>
    /// <remarks>From here: http://seattlesoftware.wordpress.com/2008/09/11/hexadecimal-value-0-is-an-invalid-character/ </remarks>
    public class XmlSanitizingStream : StreamReader
    {
        /// <summary>
        /// The character that denotes the end of a file has been reached.
        /// </summary>
        private const int EOF = -1;

        /// <summary>Create an instance of XmlSanitizingStream.</summary>
        /// <param name="streamToSanitize">
        /// The stream to sanitize of illegal XML characters.
        /// </param>
        public XmlSanitizingStream( Stream streamToSanitize )
            : base( streamToSanitize, true )
        { }

        /// <summary>
        /// Get whether an integer represents a legal XML 1.0 or 1.1 character. See
        /// the specification at w3.org for these characters.
        /// </summary>
        /// <param name="xmlVersion">
        /// The version number as a string. Use "1.0" for XML 1.0 character
        /// validation, and use "1.1" for XML 1.1 character validation.
        /// </param>
        public static bool IsLegalXmlChar( string xmlVersion, int character )
        {
            switch( xmlVersion )
            {
                case "1.1": // http://www.w3.org/TR/xml11/#charsets
                    {
                        return
                        !(
                             character <= 0x8 ||
                             character == 0xB ||
                             character == 0xC ||
                            ( character >= 0xE && character <= 0x1F ) ||
                            ( character >= 0x7F && character <= 0x84 ) ||
                            ( character >= 0x86 && character <= 0x9F ) ||
                             character > 0x10FFFF
                        );
                    }
                case "1.0": // http://www.w3.org/TR/REC-xml/#charsets
                    {
                        return
                        (
                             character == 0x9 /* == '\t' == 9   */          ||
                             character == 0xA /* == '\n' == 10  */          ||
                             character == 0xD /* == '\r' == 13  */          ||
                            ( character >= 0x20 && character <= 0xD7FF ) ||
                            ( character >= 0xE000 && character <= 0xFFFD ) ||
                            ( character >= 0x10000 && character <= 0x10FFFF )
                        );
                    }
                default:
                    {
                        throw new ArgumentOutOfRangeException( 
                            "xmlVersion", 
                            string.Format( "'{0}' is not a valid XML version." ) );
                    }
            }
        }

        /// <summary>
        /// Gets a value indicating whether an integer represents a 
        /// legal XML 1.0 character. See the specification at w3.org 
        /// for these characters.
        /// </summary>
        public static bool IsLegalXmlChar( int character )
        {
            return XmlSanitizingStream.IsLegalXmlChar( "1.0", character );
        }

        public override int Read()
        {
            // Read each character, skipping over characters that XML has prohibited
            int nextCharacter;

            do
            {
                // Read a character
                if( ( nextCharacter = base.Read() ) == EOF )
                {
                    // If the character denotes the end of the file, stop reading
                    break;
                }
            }

            // Skip the character if it's prohibited, and try the next
            while( !XmlSanitizingStream.IsLegalXmlChar( nextCharacter ) );

            return nextCharacter;
        }

        public override int Peek()
        {
            // Return the next legl XML character without reading it
            int nextCharacter;

            do
            {
                // See what the next character is 
                nextCharacter = base.Peek();

                if( !XmlSanitizingStream.IsLegalXmlChar( nextCharacter ) )
                {
                    string msg = String.Format(
                        "Character {0} is not a valid XML character, stream {1}",
                        nextCharacter,
                        this.ToString() );
                    Debug.WriteLine( msg );
                }
            }
            while
            (
                // If it's prohibited XML, skip over the character in the stream
                // and try the next.
                !XmlSanitizingStream.IsLegalXmlChar( nextCharacter ) &&
                ( nextCharacter = base.Read() ) != EOF
            );

            return nextCharacter;

        } // method

        #region Read*() method overrides

        // The following methods are exact copies of the methods in TextReader, 
        // extracting by disassembling it in Refelctor

        public override int Read( char[] buffer, int index, int count )
        {
            if( buffer == null )
            {
                throw new ArgumentNullException( "buffer" );
            }
            if( index < 0 )
            {
                throw new ArgumentOutOfRangeException( "index" );
            }
            if( count < 0 )
            {
                throw new ArgumentOutOfRangeException( "count" );
            }
            if( ( buffer.Length - index ) < count )
            {
                throw new ArgumentException();
            }
            int num = 0;
            do
            {
                int num2 = this.Read();
                if( num2 == -1 )
                {
                    return num;
                }
                buffer[index + num++] = (char)num2;
            }
            while( num < count );
            return num;
        }

        public override int ReadBlock( char[] buffer, int index, int count )
        {
            int num;
            int num2 = 0;
            do
            {
                num2 += num = this.Read( buffer, index + num2, count - num2 );
            }
            while( ( num > 0 ) && ( num2 < count ) );
            return num2;
        }

        public override string ReadLine()
        {
            StringBuilder builder = new StringBuilder();
            while( true )
            {
                int num = this.Read();
                switch( num )
                {
                    case -1:
                        if( builder.Length > 0 )
                        {
                            return builder.ToString();
                        }
                        return null;

                    case 13:
                    case 10:
                        if( ( num == 13 ) && ( this.Peek() == 10 ) )
                        {
                            this.Read();
                        }
                        return builder.ToString();
                }
                builder.Append( (char)num );
            }
        }

        public override string ReadToEnd()
        {
            int num;
            char[] buffer = new char[0x1000];
            StringBuilder builder = new StringBuilder( 0x1000 );
            while( ( num = this.Read( buffer, 0, buffer.Length ) ) != 0 )
            {
                builder.Append( buffer, 0, num );
            }
            return builder.ToString();
        }

        #endregion

    } // class

} // namespace

#if false

[TestFixture]
public class XmlSanitizingStreamTest
{

	[Test]
	public void ReadOnlyReturnsLegalXmlCharacters()
	{
		// This should be stripped to "\t\r\n<>:"

		string xml = "\0\t\a\r\b\n<>:";

		// Load the XML as a Stream

		using (var buffer = new MemoryStream(System.Text.Encoding.Default.GetBytes(xml)))
		{
			using (var sanitizer = new XmlSanitizingStream(buffer))
			{
				Assert.AreEqual(sanitizer.Read(), '\t');
				Assert.AreEqual(sanitizer.Read(), '\r');
				Assert.AreEqual(sanitizer.Read(), '\n');
				Assert.AreEqual(sanitizer.Read(), '<');
				Assert.AreEqual(sanitizer.Read(), '>');
				Assert.AreEqual(sanitizer.Read(), ':');
				Assert.AreEqual(sanitizer.Read(), -1);
				Assert.IsTrue(sanitizer.EndOfStream);
			}
		}

		using (var buffer = new MemoryStream(System.Text.Encoding.Default.GetBytes(xml)))
		{
			using (var sanitizer = new XmlSanitizingStream(buffer))
			{
				Assert.AreEqual(sanitizer.ReadToEnd(), "\t\r\n<>:");
			}
		}

	} // method

} // class

#endif