using System;
using System.Xml;

namespace Mnemosyne_Of_Mine
{
    internal partial class Config
    {
        static void WriteNewData(string location)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "\t";
            settings.WriteEndDocumentOnClose = true;
            using (XmlWriter writer = XmlWriter.Create(location, settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("settings");
                writer.WriteAttributeString("New", true.ToString().ToLower());
                Console.WriteLine("What is your username?");
                writer.WriteElementString("Username", Console.ReadLine());
                Console.WriteLine("Password? type Y if you want to type it in each time, as it's unencrypted");
                writer.WriteAttributeString("Password", Console.ReadLine());
                writer.WriteFullEndElement();
                Console.WriteLine("Using OAuth?");
                bool useOAuth = bool.Parse(Console.ReadLine().ToLower());
                writer.WriteElementString("OAuth", useOAuth.ToString().ToLower());
                string secret = null;
                string id = null;
                string redirectURI = null;
                if (useOAuth)
                {
                    Console.WriteLine("What is the client secret?");
                    secret = Console.ReadLine();
                    Console.WriteLine("Client ID?");
                    id = Console.ReadLine();
                    Console.WriteLine("Redirect URI?");
                    redirectURI = Console.ReadLine();
                }
                writer.WriteAttributeString("Secret", secret);
                writer.WriteAttributeString("ID", id);
                writer.WriteAttributeString("RedirURI", redirectURI);
                writer.WriteFullEndElement();
                Console.WriteLine("What subreddit will this be checking?");
                writer.WriteElementString("Subreddit", Console.ReadLine());
                writer.WriteFullEndElement();
                Console.WriteLine("What is your perferred request limit?");
                writer.WriteElementString("ReqLimit", Console.ReadLine());
                Console.WriteLine("Time the bot sleeps after requesting a batch of data?");
                writer.WriteAttributeString("SleepTime", Console.ReadLine());
                writer.WriteStartElement("flavortext");
                writer.WriteEndElement();
                Console.WriteLine("Manually enter the flavortext, as that is too much to do here, delimited with a \\n");
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }
    }
}
