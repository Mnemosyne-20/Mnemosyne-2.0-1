using System;
using System.Xml;

namespace Mnemosyne_Of_Mine
{
    internal partial class Config
    {
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
            }
        }
    }
}
