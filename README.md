# SteamNotificatonUtility
A tool written in C# to edit valve's steam style files.

---

A windows command line interface to automate the editing of the **_*.styles_** text documents located in the steam directory designated in the windows registry at: **_HKEY_CURRENT_USER\Software\Valve\Steam_**.

Font size (12, 14, 16, 18, 20) changed for:
* console_text_error
* console_text
* friends_chat_text_self
* friends_chat_text
* friends_chat_event
* friends_chat_bright_event
* friends_chat_url
* friends_chat_name_ingame
* friends_chat_self
* friends_chat_name
* friends_chat_accountid
* friends_chat_securitylink

Notification panel position (BottomRight, BottomLeft, TopRight, TopLeft) changed for:
* Notifications.PanelPosition

---


This was an excuse to use LINQ, lambdas, generic collections and regular expresions and is heavily overengineered for the job at hand.

For example:
````csharp
    // If any of the FontSize names are contained in the line 
    else if (StylesInfo.Steam.FontSizes.Any(s=>line.Contains(s.Name)))
    {
        // The fontsize in question
        FontSize temp = StylesInfo.Steam.FontSizes.Where(s => line.Contains(s.Name)).First();

        // Read until we find the font size that isn't for OSX
        while (!this.FontSizeRegex.IsMatch(line) && !line.Contains(this.Osx))
            line = file.ReadLine();

        temp.Size = (FontSizeEnum)Enum.Parse(typeof(FontSizeEnum), this.FontSizeRegex.Match(line).ToString());
    }
    
    // Match a string between two quotes not including the quotes
    // (?<=\")  -> Positive lookbehind (?<=expr) - requires that the match be preceded by a specific 
    //             expression - a quote (") (escaped: \")
    // [^\"]    -> Match any character that is not a single quote
    // *        -> Zero or more times
    this.PanelPositionRegex = new Regex("(?<=\")[^\"]*");
    
    // Match a two digit number immediatly followng "font-size="
    this.FontSizeRegex = new Regex(@"(?<=font-size=)\d+");
````

Known Bugs:
* Steam seems to change these values back to default. I haven't figured out when and why so this needs to be run after reboot to keep changes consistent.
