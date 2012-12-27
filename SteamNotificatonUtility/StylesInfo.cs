using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace SteamNotificatonUtility
{
    public enum PanelPosition { BottomRight, BottomLeft, TopRight, TopLeft };
    public enum FontSizeEnum { f12 = 12, f14 = 14, f16 = 16, f18 = 18, f20 = 20 };

    public class FontSize
    {
        public FontSize(string name)
        {
            this.Name = name;
        }

        public string Name { get; set; }
        public FontSizeEnum Size { get; set; }

        public override string ToString()
        {
            return this.Name.ToString() + " " + (int)this.Size;
        }
    }
    
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
        public Regex FontSizeRegex { get; set; }

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

            // Match a two digit number immediatly followng "font-size="
            this.FontSizeRegex = new Regex(@"(?<=font-size=)\d+");
                        
            StylesInfo.Steam.FontSizes = new List<FontSize>(12);
            
            // These are all the areas in the steam styles file where a fontsize should be changed
            StylesInfo.Steam.FontSizes.Add(new FontSize("console_text_error"));
            StylesInfo.Steam.FontSizes.Add(new FontSize("console_text"));
            StylesInfo.Steam.FontSizes.Add(new FontSize("friends_chat_text_self"));
            StylesInfo.Steam.FontSizes.Add(new FontSize("friends_chat_text"));            
            StylesInfo.Steam.FontSizes.Add(new FontSize("friends_chat_event"));
            StylesInfo.Steam.FontSizes.Add(new FontSize("friends_chat_bright_event"));
            StylesInfo.Steam.FontSizes.Add(new FontSize("friends_chat_url"));
            StylesInfo.Steam.FontSizes.Add(new FontSize("friends_chat_name_ingame"));
            StylesInfo.Steam.FontSizes.Add(new FontSize("friends_chat_self"));
            StylesInfo.Steam.FontSizes.Add(new FontSize("friends_chat_name"));
            StylesInfo.Steam.FontSizes.Add(new FontSize("friends_chat_accountid"));
            StylesInfo.Steam.FontSizes.Add(new FontSize("friends_chat_securitylink"));
                        
            this.SteamPath = path + @"\resource\styles";

            // Match a string between two quotes not including the quotes
            // (?<=\")  -> Positive lookbehind (?<=expr) - requires that the match be preceded by a specific 
            //             expression - a quote (") (escaped: \")
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
                        // Panel Position is in this line, could be in Steam or Styles file
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
                        // Using LINQ: If any of the FontSize names are contained in the line 
                        else if (StylesInfo.Steam.FontSizes.Any(s=>line.Contains(s.Name)))
                        {
                            // The fontsize in question
                            FontSize temp = StylesInfo.Steam.FontSizes.Where(s => line.Contains(s.Name)).First();

                            // Read until we find the font size that isn't for OSX
                            while (!this.FontSizeRegex.IsMatch(line) && !line.Contains(this.Osx))
                                line = file.ReadLine();

                            temp.Size = (FontSizeEnum)Enum.Parse(typeof(FontSizeEnum), this.FontSizeRegex.Match(line).ToString());
                        }

                        line = file.ReadLine();
                    }
                }
            }
        }

        internal void ChangePanelPosition(PanelPosition panelPosition)
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
                        temp = PanelPositionRegex.Replace(fileLine, panelPosition.ToString(), 1);
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

        internal void ChangeFontSize(FontSizeEnum fontSizeEnum)
        {
            foreach (string f in StyleFiles)
            {
                StringBuilder newFile = new StringBuilder();
                string temp = "";
                string[] fileAsString = File.ReadAllLines(f);
                bool searchingForFontSize = false;

                foreach (string fileLine in fileAsString)
                {
                    if (searchingForFontSize && this.FontSizeRegex.IsMatch(fileLine) && !fileLine.Contains(this.Osx))
                    {
                        temp = FontSizeRegex.Replace(fileLine, ((int)fontSizeEnum).ToString(), 1);
                        newFile.AppendLine(temp);
                        searchingForFontSize = false;
                    }
                    else if (StylesInfo.Steam.FontSizes.Any(s => fileLine.Contains(s.Name)))
                    {
                        searchingForFontSize = true;
                        newFile.AppendLine(fileLine);
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

            public static List<FontSize> FontSizes { get; set; }
        }

        public static class Gameoverlay
        {
            public static PanelPosition NotificationPanelPosition { get; set; }
        }

        public static string FontSizesToString()
        {
            StringBuilder sb = new StringBuilder();

            foreach (FontSize f in Steam.FontSizes)
                sb.AppendLine(f.ToString());

            return sb.ToString();
        }

        public static new string ToString()
        {
            //return "steam.styles:\t\t" + Steam.NotificationPanelPosition.ToString() + "\n" +
            //        "steam.styles OSX:\t" + Steam.NotificationPanelPositionOSX.ToString() + "\n" +
            //        "gameoverlay.styles:\t" + Gameoverlay.NotificationPanelPosition.ToString() + "\n";                
            return Steam.NotificationPanelPosition.ToString();
        }
    }
}
