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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "subreddit")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "seperate")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "Oauth")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "git")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "flavortext")]
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
                writer.WriteStartElement("subreddit");
                writer.WriteString(subreddit);
                writer.WriteEndElement();
                Console.WriteLine("What do you want the request limit to be?");
                string reqlimit = Console.ReadLine();
                writer.WriteStartElement("ReqLimit");
                writer.WriteString(reqlimit);
                writer.WriteEndElement();
                Console.WriteLine("What is the sleep time?");
                string sleepTime = Console.ReadLine();
                writer.WriteStartElement("SleepTime");
                writer.WriteString(sleepTime);
                writer.WriteEndElement();
                Console.WriteLine("Use OAuth? (Y/N)");
                bool bUseOAuth = false;
                string useOAuthResponse = Console.ReadLine();
                if(useOAuthResponse.ToUpper() == "Y")
                {
                    bUseOAuth = true;
                }
                writer.WriteStartElement("UseOAuth");
                writer.WriteString(bUseOAuth.ToString().ToLower());
                writer.WriteEndElement();
                if (bUseOAuth)
                {
                    Console.WriteLine("OAuth Client ID?");
                    string oauthClientID = Console.ReadLine();
                    writer.WriteStartElement("OAuthClientID");
                    writer.WriteString(oauthClientID);
                    writer.WriteEndElement();
                    Console.WriteLine("OAuth Client Secret?");
                    string oauthClientSecret = Console.ReadLine();
                    writer.WriteStartElement("OAuthClientSecret");
                    writer.WriteString(oauthClientSecret);
                    writer.WriteEndElement();
                    Console.WriteLine("Redirect URI?"); // pointless for a bot but the auth API still asks for it
                    string redirectURI = Console.ReadLine();
                    writer.WriteStartElement("RedirectURI");
                    writer.WriteString(redirectURI);
                    writer.WriteEndElement();
                }
                Console.WriteLine("What is your username?");
                string username = Console.ReadLine();
                writer.WriteStartElement("Username");
                writer.WriteString(username);
                writer.WriteEndElement();
                Console.WriteLine("What about password? note: this is stored in plaintext, don't actually send out in a git or type Y (just \"Y\") to not use a password in the config, and require one on startup");
                string password = Console.ReadLine();
                writer.WriteStartElement("Password"); //Password feild, as OAuth hasn't been setup yet
                writer.WriteString(password);
                writer.WriteEndElement();
                Console.WriteLine("You have to add flavortext manually after the fact, go into the config file and seperate each flavor text with a \"");
                writer.WriteStartElement("flavortext");
                writer.WriteString("");
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndDocument();
                writer.Flush();
                #endregion
            }
        }
    }
}
