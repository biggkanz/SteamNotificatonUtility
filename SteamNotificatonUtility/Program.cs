using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Permissions;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace SteamNotificatonUtility
{
    class Program
    {
        const string welcomeMessage = "Steam Notification Utility" + "\n" + "By biggskanz" + "\n";

        static void Main(string[] args)
        {
            Console.WriteLine(welcomeMessage);

            // Find the location of steam through the registry
            string steamPath = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam", "SteamPath", null).ToString();

            if (string.IsNullOrEmpty(steamPath))
                throw new DirectoryNotFoundException("Steam was not found. Abort!");

            steamPath = steamPath.Replace('/', '\\');
            
            // StyleChanger does the main work.
            StylesChanger styleChanger = new StylesChanger(steamPath);

            if (styleChanger.Verbose == true)
                Console.WriteLine("Steam was found: " + steamPath + "\n");

            // Get the current notification location
            styleChanger.PopulateStylesInfo();
            styleChanger.CreateBackup();

            // KeyValuePair for main options
            Dictionary<ConsoleKey, string> MainKeyOptions = new Dictionary<ConsoleKey, string>();
            MainKeyOptions.Add(ConsoleKey.D1, "Font");
            MainKeyOptions.Add(ConsoleKey.D2, "Panel Position");
            MainKeyOptions.Add(ConsoleKey.D3, "Exit");

            // Create a KeyValuePair for storing all possible panel key options and their corresponding action
            Dictionary<ConsoleKey, PanelPosition> PanelKeyOptions = new Dictionary<ConsoleKey, PanelPosition>();

            // Create a numbered list of all possible positions in PanelPosition            
            foreach (string enumName in Enum.GetNames(typeof(PanelPosition)))
            {
                PanelPosition pp = (PanelPosition)Enum.Parse(typeof(PanelPosition), enumName);
                // Get the console key that corresponds to the panel position enum integer value + 1
                ConsoleKey key = (ConsoleKey)Enum.Parse(typeof(ConsoleKey), "D" + ((int)pp + 1).ToString());

                PanelKeyOptions.Add(key, pp);
            }

            // Create a KeyValuePair for storing all possible panel key options and their corresponding action
            Dictionary<ConsoleKey, FontSizeEnum> FontKeyOptions = new Dictionary<ConsoleKey, FontSizeEnum>();
            int keyOption = 0;
            foreach (string enumName in Enum.GetNames(typeof(FontSizeEnum)))
            {
                FontSizeEnum fs = (FontSizeEnum)Enum.Parse(typeof(FontSizeEnum), enumName);
                // Get the console key that corresponds to the panel position enum integer value + 1
                ConsoleKey key = (ConsoleKey)Enum.Parse(typeof(ConsoleKey), "D" + ++keyOption);

                FontKeyOptions.Add(key, fs);
            }


            ConsoleKey userInput;

            do
            {
                Console.WriteLine("Panel Position: " + StylesInfo.ToString());
                Console.WriteLine("Font size: " + (int)StylesInfo.Steam.FontSizes.First().Size + "\n");

                // Write user input key options to the console
                Console.WriteLine("Options:");
                foreach (KeyValuePair<ConsoleKey, string> pair in MainKeyOptions)
                    Console.WriteLine(pair.Key.ToString().Remove(0, 1) + " " + pair.Value);

                userInput = Console.ReadKey(true).Key;

                //  FONT
                if (userInput == MainKeyOptions.Keys.Where(k => k == ConsoleKey.D1).First())
                {
                    Console.WriteLine(System.Environment.NewLine + "Change font size to:");

                    // Put all values from FontSizeEnum into array
                    Array values = Enum.GetValues(typeof(FontSizeEnum));

                    foreach (KeyValuePair<ConsoleKey, FontSizeEnum> pair in FontKeyOptions)
                        Console.WriteLine(pair.Key.ToString().Remove(0, 1) + " " + pair.Value.ToString().TrimStart('f'));

                    Console.WriteLine();

                    userInput = Console.ReadKey(true).Key;

                    // Change the font
                    if (FontKeyOptions.Keys.Contains<ConsoleKey>(userInput))
                        styleChanger.ChangeFontSize(FontKeyOptions[userInput]);
                }

                // PANEL POSITION
                else if (userInput == MainKeyOptions.Keys.Where(k => k == ConsoleKey.D2).First())
                {
                    Console.WriteLine(System.Environment.NewLine + "Change notification panel location to:");
                    foreach (KeyValuePair<ConsoleKey, PanelPosition> pair in PanelKeyOptions)
                        Console.WriteLine(pair.Key.ToString().Remove(0, 1) + " " + pair.Value);

                    userInput = Console.ReadKey(true).Key;

                    // Change the notification panel position
                    if (PanelKeyOptions.Keys.Contains<ConsoleKey>(userInput))
                        styleChanger.ChangePanelPosition(PanelKeyOptions[userInput]);

                    Console.WriteLine();
                }

                // EXIT
                else if (userInput == MainKeyOptions.Keys.Where(k => k == ConsoleKey.D3).First())
                {
                    Environment.Exit(1);
                }
            }
            while (ConsoleKey.Escape != userInput);



        }
    }
}
