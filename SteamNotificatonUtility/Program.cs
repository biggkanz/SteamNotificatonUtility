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

            Console.WriteLine("Current notification panel location:");
            Console.WriteLine(StylesInfo.ToString());

            // Create a KeyValuePair for storing all possible and their corresponding action
            Dictionary<ConsoleKey, PanelPosition> KeyOptions = new Dictionary<ConsoleKey, PanelPosition>();

            // Create a numbered list of all possible positions in PanelPosition            
            foreach (string enumName in Enum.GetNames(typeof(PanelPosition)))
            {
                PanelPosition pp = (PanelPosition)Enum.Parse(typeof(PanelPosition), enumName);
                // Get the console key that corresponds to the panel position enum integer value + 1
                ConsoleKey key = (ConsoleKey)Enum.Parse(typeof(ConsoleKey), "D" + ((int)pp + 1).ToString());
                
                KeyOptions.Add(key, pp);
            }

            // Write user input key options to the console
            Console.WriteLine(System.Environment.NewLine + "Change notification panel location to:");
            foreach (KeyValuePair<ConsoleKey, PanelPosition> pair in KeyOptions)
                Console.WriteLine(pair.Key.ToString().Remove(0,1) + " " + pair.Value);

            ConsoleKey userInput = Console.ReadKey(true).Key;

            // Change the notification panel position
            if(KeyOptions.Keys.Contains<ConsoleKey>(userInput))
                styleChanger.ChangePanelPosition(KeyOptions[userInput]);

            Console.WriteLine();
            Console.WriteLine("New notification panel location:");
            Console.WriteLine(StylesInfo.ToString());

            Console.WriteLine(System.Environment.NewLine + "I'm done. Esc to quit");
            
            // Wait for the user to quit
            do {
                while (!Console.KeyAvailable) { }
            } while (Console.ReadKey(true).Key != ConsoleKey.Escape); 
        }
    }
}
