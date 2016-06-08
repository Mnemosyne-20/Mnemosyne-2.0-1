using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Mnemosyne_Of_Mine
{
    class ConfigWriter
    {
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
                writer.WriteEndElement();
                Console.WriteLine("What's your Oauth token?");
                string oAuth = Console.ReadLine();
                writer.WriteStartElement("Oauth");
                writer.WriteString(oAuth);
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndDocument();
                writer.Flush();
                #endregion
            }
        }
    }
}
