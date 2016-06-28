using System;
using System.Xml;

namespace Mnemosyne_Of_Mine
{
    /// <remarks>
    /// This is a partial because this is going to be worked on at multiple places with many functions to allow git to be able to push this properly, aswell as merge
    ///</remarks>
    internal partial class Config
    {
        /// <summary>
        /// This is a method to convert the old file format into the new file format, will add a detection for it being new to old *soon*
        /// </summary>
        /// <param name="location">guess</param>
        public static void ConvertOldToNewData(string location)
        {
            UserData data = new UserData(location, true);
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "\t";
            settings.WriteEndDocumentOnClose = true;
            using (XmlWriter writer = XmlWriter.Create(location, settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("settings");
                writer.WriteElementString("Username", data.Username);
                writer.WriteAttributeString("Password",data.Password);
                writer.WriteFullEndElement();
                writer.WriteElementString("OAuth",data.bUseOAuth.ToString().ToLower());
                writer.WriteAttributeString("Secret",data.OAuthClientSecret);
                writer.WriteAttributeString("ID",data.OAuthClientID);
                writer.WriteAttributeString("RedirURI",data.RedirectURI);
                writer.WriteFullEndElement();
                writer.WriteElementString("Subreddit",data.SubReddit);
                writer.WriteFullEndElement();
                writer.WriteElementString("ReqLimit", data.ReqLimit.ToString());
                writer.WriteAttributeString("SleepTime", data.SleepTime.ToString());
                writer.WriteStartElement("flavortext");
                foreach(var text in data.FlavorText)
                {
                    writer.WriteString(text + "\n");
                }
                writer.WriteFullEndElement();
                writer.WriteFullEndElement();
                writer.WriteEndDocument();
            }
        }
    }
}
