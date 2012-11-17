using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace SteamNotificatonUtility
{
    public enum PanelPosition { BottomRight, BottomLeft, TopRight, TopLeft };

    public class StylesChanger
    {
        public bool Verbose { get; set; }

        public string AllStyles { get; set; }
        public string AllBackups { get; set; }
        public string BackupExtension
        {
            get
            {
                return this.AllBackups.Remove(0, 1);
            }
        }

        public string Osx { get; set; }

        public string SteamStylesFileName { get; set; }
        public string GameoverlayStylesFileName { get; set; }

        public string NotificationPanelPosition { get; set; }

        public string SteamPath { get; set; }

        public Regex PanelPositionRegex { get; set; }

        public IEnumerable<string> StyleFiles { get; set; }
        public IEnumerable<string> BackupFiles { get; set; }

        public StylesChanger() : this(@"C:\Program Files (x86)\Steam")
        { }

        public StylesChanger(string path)
        {
            this.Verbose = false;

            this.AllBackups = "*.backup";
            this.AllStyles = "*.styles";

            this.Osx = "[$OSX]";

            this.SteamStylesFileName = "steam";
            this.GameoverlayStylesFileName = "gameoverlay";

            this.NotificationPanelPosition = "Notifications.PanelPosition";

            this.SteamPath = path + @"\resource\styles";

            // Match a string between two quotes not including the quotes
            // (?<=\")  -> Positive lookbehind (?<=expr) - requires that the match be preceded by a specific expression - an quote (") (escaped: \")
            // [^\"]    -> Match any character that is not a single quote
            // *        -> Zero or more times
            this.PanelPositionRegex = new Regex("(?<=\")[^\"]*");

            StyleFiles = Directory.EnumerateFiles(SteamPath, AllStyles, SearchOption.TopDirectoryOnly);
            BackupFiles = Directory.EnumerateFiles(SteamPath, AllBackups, SearchOption.TopDirectoryOnly);
        }

        public void PopulateStylesInfo()
        {
            foreach (string f in StyleFiles)
            {
                // Cannot open file if it is read only.
                new FileInfo(f).IsReadOnly = false;

                using (System.IO.StreamReader file = new System.IO.StreamReader(f))
                {
                    var line = file.ReadLine();
                    while (line != null)
                    {
                        if (line.Contains(NotificationPanelPosition))
                        {
                            // Populate the StylesInfo class with the current panelPosition values
                            if (new FileInfo(f).Name.Contains(this.SteamStylesFileName))
                                if (!line.Contains(this.Osx))
                                    StylesInfo.Steam.NotificationPanelPosition =
                                        (PanelPosition)Enum.Parse(typeof(PanelPosition), this.PanelPositionRegex.Match(line).ToString());
                                else
                                    StylesInfo.Steam.NotificationPanelPositionOSX =
                                        (PanelPosition)Enum.Parse(typeof(PanelPosition), this.PanelPositionRegex.Match(line).ToString());
                            else if (new FileInfo(f).Name.Contains(this.GameoverlayStylesFileName))
                                StylesInfo.Gameoverlay.NotificationPanelPosition =
                                    (PanelPosition)Enum.Parse(typeof(PanelPosition), this.PanelPositionRegex.Match(line).ToString());
                        }

                        line = file.ReadLine();
                    }
                }
            }
        }

        public void ChangePanelPosition(PanelPosition panelPosition)
        {
            foreach (string f in StyleFiles)
            {
                StringBuilder newFile = new StringBuilder();
                string temp = "";
                string[] fileAsString = File.ReadAllLines(f);

                foreach (string fileLine in fileAsString)
                {
                    if (fileLine.Contains(NotificationPanelPosition))
                    {
                        temp = fileLine.Replace(PanelPositionRegex.Match(fileLine).ToString(), panelPosition.ToString());                        
                        newFile.AppendLine(temp);

                        if(this.Verbose == true)
                            Console.WriteLine(temp.TrimStart());
                    }
                    else
                    {
                        newFile.AppendLine(fileLine);
                    }
                }

                File.WriteAllText(f, newFile.ToString());
            }

            this.PopulateStylesInfo();

            if (this.Verbose == true)
                Console.WriteLine();
        }

        public void CreateBackup()
        {
             // Create a backup
            foreach (string fileName in StyleFiles)
            {
                string backupFileName = fileName + this.BackupExtension;

                if (System.IO.File.Exists(backupFileName))
                {
                    if (this.Verbose)
                        Console.WriteLine("Deleting old backup file.");

                    new FileInfo(backupFileName).IsReadOnly = false;
                    System.IO.File.Delete(backupFileName);
                }

                System.IO.File.Copy(fileName, backupFileName);

                if (this.Verbose)
                    Console.WriteLine("Created a new backup at: " + backupFileName);
            }

            if (this.Verbose)
                Console.WriteLine();
        }
    }

    public static class StylesInfo
    {
        public static class Steam
        {
            public static PanelPosition NotificationPanelPosition { get; set; }
            public static PanelPosition NotificationPanelPositionOSX { get; set; }
        }

        public static class Gameoverlay
        {
            public static PanelPosition NotificationPanelPosition { get; set; }
        }

        public static new string ToString()
        {
            return "steam.styles:\t\t" + Steam.NotificationPanelPosition.ToString() + "\n" +
                    "steam.styles OSX:\t" + Steam.NotificationPanelPositionOSX.ToString() + "\n" +
                    "gameoverlay.styles:\t" + Gameoverlay.NotificationPanelPosition.ToString() + "\n";
        }
    }
}
