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
                string subreddit = Console.ReadLine();
                writer.makeReadable("subreddit", subreddit);
                Console.WriteLine("What do you want the request limit to be?");
                string reqlimit = Console.ReadLine();
                writer.makeReadable("ReqLimit",reqlimit);
                Console.WriteLine("What is the sleep time?");
                string sleepTime = Console.ReadLine();
                writer.makeReadable("SleepTime",sleepTime);
                Console.WriteLine("Use OAuth? (Y/N)");
                bool bUseOAuth = false;
                string useOAuthResponse = Console.ReadLine();
                if(useOAuthResponse.ToUpper() == "Y")
                {
                    bUseOAuth = true;
                }
                writer.makeReadable("UseOAuth",bUseOAuth.ToString().ToLower());
                if (bUseOAuth)
                {
                    Console.WriteLine("OAuth Client ID?");
                    string oauthClientID = Console.ReadLine();
                    writer.makeReadable("OAuthClientID",oauthClientID);
                    Console.WriteLine("OAuth Client Secret?");
                    string oauthClientSecret = Console.ReadLine();
                    writer.makeReadable("OAuthClientSecret",oauthClientSecret);
                    Console.WriteLine("Redirect URI?"); // pointless for a bot but the auth API still asks for it
                    string redirectURI = Console.ReadLine();
                    writer.makeReadable("RedirectURI",redirectURI);
                }
                Console.WriteLine("What is your username?");
                string username = Console.ReadLine();
                writer.makeReadable("Username",username);
                Console.WriteLine("What about password? note: this is stored in plaintext, don't actually send out in a git or type Y (just \"Y\") to not use a password in the config, and require one on startup");
                string password = Console.ReadLine();
                writer.makeReadable("Password",password);
                Console.WriteLine("You have to add flavortext manually after the fact, go into the config file and seperate each flavor text with a \"");
                writer.makeReadable("flavortext","");
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
