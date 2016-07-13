using System;
using System.Xml;

namespace Mnemosyne_Of_Mine
{
    static class ConfigWriter
    {
        /// <summary>
        /// Runs user through process of creating a config file with semi-interface type deal
        /// </summary>
        /// <param name="settings">settings to write to the file with (tabs, etc)</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Console.WriteLine(System.String)")]
        internal static void writeConfig(XmlWriterSettings settings)
        {
            XmlWriter writer = null;
            using (writer = XmlWriter.Create(@".\config.xml", settings)) //this should be obvious
            {
                #region writers
                writer.WriteStartDocument();
                writer.WriteStartElement("Settings");
                Console.WriteLine("So, what is your subreddit this bot will run on?");
                writer.makeReadable("subreddit", Console.ReadLine());
                Console.WriteLine("What do you want the request limit to be?");
                writer.makeReadable("ReqLimit", Console.ReadLine());
                Console.WriteLine("What is the sleep time?");
                writer.makeReadable("SleepTime", Console.ReadLine());
                Console.WriteLine("Use OAuth? (Y/N)");
                bool bUseOAuth = false;
                if (Console.ReadLine().ToUpper() == "Y")
                {
                    bUseOAuth = true;
                }
                writer.makeReadable("UseOAuth", bUseOAuth.ToString().ToLower());
                if (bUseOAuth)
                {
                    Console.WriteLine("OAuth Client ID?");
                    writer.makeReadable("OAuthClientID", Console.ReadLine());
                    Console.WriteLine("OAuth Client Secret?");
                    writer.makeReadable("OAuthClientSecret", Console.ReadLine());
                    Console.WriteLine("Redirect URI?"); // pointless for a bot but the auth API still asks for it
                    writer.makeReadable("RedirectURI", Console.ReadLine());
                }
                Console.WriteLine("What is your username?");
                writer.makeReadable("Username", Console.ReadLine());
                Console.WriteLine("What about password? note: this is stored in plaintext, don't actually send out in a git or type Y (just \"Y\") to not use a password in the config, and require one on startup");
                writer.makeReadable("Password", Console.ReadLine());
                Console.WriteLine("You have to add flavortext manually after the fact, go into the config file and seperate each flavor text with a \"");
                writer.makeReadable("flavortext", "");
                Console.WriteLine("Do you want to use SQLite or a normal file?");
                writer.WriteStartAttribute("SQLite");
                writer.WriteAttributeString("thisdoesntmatter", Console.ReadLine().ToLower());
                writer.WriteEndElement();
                writer.WriteEndDocument();
                writer.Flush();
                #endregion
            }
        }
        /// <summary>
        /// This makes it all readable
        /// </summary>
        /// <param name="writer">the writer you're using</param>
        /// <param name="element">elemet to write</param>
        /// <param name="text">text to writer</param>
        private static void makeReadable(this XmlWriter writer, string element, string text)
        {
            writer.WriteStartElement(element);
            writer.WriteString(text);
            writer.WriteEndElement();
        }
    }
}
