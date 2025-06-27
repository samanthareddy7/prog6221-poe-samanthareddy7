using System;
using System.Text; 
using System.Threading.Tasks; 
using System.Windows.Media; 

namespace samantha_progpart3
{
    public class Greeting
    {
        // Actions (delegates) to communicate back UI updates and logging
        private Action<string> _addAsciiArtMessage;
        private Action<string> _addBotMessage; // Added to store the AddBotMessage action
        private Action<string> _logActivity;   // Added to store the LogActivity action

       
        /// <param name="addAsciiArtMessage">Action to add an ASCII art message to the chat display.</param>
        /// <param name="addBotMessage">Action to add a regular bot message to the chat display.</param>
        /// <param name="logActivity">Action to log an activity.</param>
        public Greeting(Action<string> addAsciiArtMessage, Action<string> addBotMessage, Action<string> logActivity)
        {
            _addAsciiArtMessage = addAsciiArtMessage;
            _addBotMessage = addBotMessage;
            _logActivity = logActivity;
        }

        
        public async void DisplayAsciiArt()
        {
            // Original ASCII Art for Encryptonite as provided by the user
            string asciiArt = @"
      ╔═════════════════════════════════════════════════════════════╗
      ║     CYBERSECURITY AWARENESS BOT - 'Encryptonite'            ║
      ╠═════════════════════════════════════════════════════════════╣
 _____                                                              _____ 
( ___ )------------------------------------------------------------( ___ )
 |   |                                                              |   | 
 |   | _____                               _           _ _          |   |           |.-----.|
 |   | | ____|_ __   ___ _ __ _   _ _ __ | |_ ___  _ __ (_) |_ ___  |   |           ||x . x||
 |   | |  _| | '_ \ / __| '__| | | | '_ \| __/ _ \| '_ \| | __/ _ \ |   |           ||_.-._||
 |   | | |___| | | | (__| |  | |_| | |_) | || (_) | | | | | ||  __/ |   |           `--)-(--`       
 |   | |_____|_| |_|\___|_|   \__, | .__/ \__\___/|_| |_|_|\__\___| |   |          __[=== o]___
 |   |                            |___/|_|                          |   |         |:::::::::::|\
 |___|                                                              |___|         `-=========-`()
(_____)------------------------------------------------------------(_____)
";
            _addAsciiArtMessage(asciiArt);
            _logActivity("Displayed Encryptonite ASCII art.");
            await Task.Delay(1000); // Small delay after displaying ASCII art
        }

       
        public void DisplayHelpMenu()
        {
            StringBuilder menu = new StringBuilder();

menu.AppendLine("");
menu.AppendLine("╭────•───────────────────────────────────────────────────────────────•────╮");
menu.AppendLine("│ ╭─╮                                                                  ╭─╮ │");
menu.AppendLine("│ │          ✦✦✦✦✦✦✦✦  Topics I Can Help With  ✦✦✦✦✦✦✦✦       │ │");
menu.AppendLine("│ ╰─╯                                                                  ╰─╯ │");
menu.AppendLine("├────────────────────────────────────────────────────────────────────────┤");
menu.AppendLine("│                                                                        │");
menu.AppendLine("│  • Numbers            → Importance of numbers                          │");
menu.AppendLine("│  • Purpose            → Why can I do                                   │");
menu.AppendLine("│  • Scam               → Learn how to spot scams                        │");
menu.AppendLine("│  • What can you do    → See what I’m capable of                        │");
menu.AppendLine("│  • Phishing           → Avoid phishing traps                           │");
menu.AppendLine("│  • Password           → Tips for stronger passwords                    │");
menu.AppendLine("│  • Safe Browse        → Skim safely without fear                       │");
menu.AppendLine("│  • Malware            → Understand and block malware                   │");
menu.AppendLine("│  • Network Security   → Keep your connection strong                    │");
menu.AppendLine("│  • Cybersecurity      → Get the big picture on staying safe            │");
menu.AppendLine("│  • Help               → A reminder of what you can ask                 │");
menu.AppendLine("│  • Add Task           → Add a task with reminders and check when done  │");
menu.AppendLine("│  • Start quiz         → Play a quiz to test your cybersecurity skills  │");
menu.AppendLine("│  • Show Activity log  → View recent chat and side activity history     │");
menu.AppendLine("│                                                                        │");
menu.AppendLine("└────•───────────────────────────────────────────────────────────────•────┘");
menu.AppendLine("");


            _addAsciiArtMessage(menu.ToString());
            _logActivity("Displayed help menu in chat.");
        }
    }
}
