using System;
using System.Windows.Controls; 
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

       
        public void DisplayAsciiArt()
        {
            // Using the delegate to send ASCII art to the UI
            _addAsciiArtMessage("╔═════════════════════════════════════════════════════════════╗");
            _addAsciiArtMessage("║     CYBERSECURITY AWARENESS BOT - 'Encryptonite'            ║");
            _addAsciiArtMessage("╠═════════════════════════════════════════════════════════════╣");
            _addAsciiArtMessage(@"
  _____                                                              _____ 
 ( ___ )------------------------------------------------------------( ___ )
  |   |                                                              |   | 
  |   |  _____                                _           _ _        |   |           |.-----.|
  |   | | ____|_ __   ___ _ __ _   _ _ __ | |_ ___  _ __ (_) |_ ___  |   |           ||x . x||
  |   | |  _| | '_ \ / __| '__| | | | '_ \| __/ _ \| '_ \| | __/ _ \ |   |           ||_.-._||
  |   | | |___| | | | (__| |  | |_| | |_) | || (_) | | | | | ||  __/ |   |           `--)-(--`       
  |   | |_____|_| |_|\___|_|   \__, | .__/ \__\___/|_| |_|_|\__\___| |   |          __[=== o]___
  |   |                            |___/|_|                           |   |        |:::::::::::|\
  |___|                                                               |___|        `-=========-`()
 (_____)------------------------------------------------------------(_____)
");
            _addAsciiArtMessage("╚═════════════════════════════════════════════════════════════╝");

            // Using the delegate to send bot message
            _addBotMessage("Ready to help you secure your digital life!");

            // Using the delegate to log  activity
            _logActivity($"Displayed ASCII art.");
        }
    }
}
