using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace libAG3DBCrevival
{
    public class Ag3DbRevivalApi
    {
        private const string settingsFilename = "revival-settings.xml";
        public Ag3DbRevivalApi()
        {
            settingsSerializer = new XmlSerializer(typeof(Ag3DbRevivalSettings));
            FileInfo settingsFile = new FileInfo(settingsFilename);
            if (settingsFile.Exists)
            {
                FileStream fileStream = settingsFile.OpenRead();
                settings = (Ag3DbRevivalSettings)settingsSerializer.Deserialize(fileStream);

            }
            else
            {
                settings = new Ag3DbRevivalSettings();
                settings.SetDefaults();
                SaveSettings();
                settingsJustCreated = true;
            }
        }

        private XmlSerializer settingsSerializer;
        private Ag3DbRevivalSettings settings;
        private bool settingsJustCreated;

        public void SaveSettings()
        {
            FileInfo fi = new FileInfo(settingsFilename);
            if (fi.Exists)
                fi.Delete();
            FileStream fileStream = fi.OpenWrite();
            settingsSerializer.Serialize(fileStream, settings);
            fileStream.Flush();
            fileStream.Close();
        }

        public bool FirstRun => settingsJustCreated;
        public string ServerUrl => settings.ServerURL;
    }
}
