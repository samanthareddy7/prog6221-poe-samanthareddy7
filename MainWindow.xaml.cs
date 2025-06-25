using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Threading.Tasks;
using System.Collections.Generic; // Required for Dictionary


namespace samantha_progpart3
{
    public partial class MainWindow : Window
    {
        private string userName = "User"; // Default user name
        private Random randomGenerator = new Random();
        private DateTime lastProactiveMentionTime = DateTime.MinValue; // Used in GetBotResponse
        private string lastProactiveInterestMentioned = null; // Used in GetBotResponse

       
        public MainWindow()
        {
            InitializeComponent();
            InitializeChatbot();
            SimulateGreeting();
            UserInputTextBox.Focus();
        }

        // Simulates an initial greeting message and user name request
        private async void SimulateGreeting()
        {
            await Task.Delay(500); // Simulate a short delay before greeting
            AddBotMessage("Hello! I am Encryptonite 🤖🔊, your Cybersecurity Awareness Chatbot.");
            await Task.Delay(1000);
            RequestUserName();
        }

        private void RequestUserName()
        {
            AddBotMessage("Before we start, what's your name?");
            
            UserInputTextBox.KeyDown -= UserInputTextBox_KeyDown; // Remove default handler
            UserInputTextBox.KeyDown += GetUserName_KeyDown;      // Add specific handler for name input
        }

        // Captures user's name from input after greeting
        private async void GetUserName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string input = UserInputTextBox.Text.Trim();
                UserInputTextBox.Clear();

                if (!string.IsNullOrWhiteSpace(input))
                {
                    userName = input;
                    AddUserMessage(input);
                    AddBotMessage($"Nice to meet you, {userName}! How can I help you stay safe online today? 🛡️");
                    AddBotMessage("You can ask me about 'phishing', 'passwords', 'safe browse', or 'malware'.");
                    // Removed mentions of quiz, tasks, activity log as they are not yet implemented.

                    // Restore normal input processing
                    UserInputTextBox.KeyDown -= GetUserName_KeyDown; // Remove name handler
                    UserInputTextBox.KeyDown += UserInputTextBox_KeyDown; // Add back default handler
                }
                else
                {
                    AddBotMessage("Please tell me your name.");
                }
            }
        }

        // Initializes the chatbot, including displaying ASCII art
        private void InitializeChatbot()
        {
            DisplayAsciiArt();
           // Removed PlayVoiceGreeting(); 
        }

        // Displays ASCII art in the chat panel
        private void DisplayAsciiArt()
        {
            // Using AddAsciiArtMessage for proper monospace rendering and no wrapping
            AddAsciiArtMessage("╔═════════════════════════════════════════════════════════════╗");
            AddAsciiArtMessage("║     CYBERSECURITY AWARENESS BOT - 'Encryptonite'            ║");
            AddAsciiArtMessage("╠═════════════════════════════════════════════════════════════╣");
            AddAsciiArtMessage(@"
  _____                                                                 _____ 
 ( ___ )------------------------------------------------------------( ___ )
  |   |                                                               |   | 
  |   |  _____                                _           _ _         |   |         |.-----.|
  |   | | ____|_ __   ___ _ __ _   _ _ __ | |_ ___  _ __ (_) |_ ___  |   |         ||x . x||
  |   | |  _| | '_ \ / __| '__| | | | '_ \| __/ _ \| '_ \| | __/ _ \ |   |         ||_.-._||
  |   | | |___| | | | (__| |  | |_| | |_) | || (_) | | | | | ||  __/ |   |         `--)-(--`       
  |   | |_____|_| |_|\___|_|   \__, | .__/ \__\___/|_| |_|_|\__\___| |   |         __[=== o]___
  |   |                            |___/|_|                           |   |        |:::::::::::|\
  |___|                                                               |___|        `-=========-`()
 (_____)------------------------------------------------------------(_____)
");
            AddAsciiArtMessage("╚═════════════════════════════════════════════════════════════╝");

            AddBotMessage("Ready to help you secure your digital life!"); // Normal bot message
        }

        // Removed PlayVoiceGreeting() method as it's no longer needed.
        /*
        private void PlayVoiceGreeting()
        {
            try
            {
                string filePath = "ProgSound.wav";
                if (File.Exists(filePath))
                {
                    SoundPlayer player = new SoundPlayer(filePath);
                    player.Play();
                }
                else
                {
                    AddBotMessage("[!] Voice greeting file not found! Please check the path is correct.");
                }
            }
            catch (Exception ex)
            {
                AddBotMessage($"[!] Failed to play voice greeting: {ex.Message}");
            }
        }
        */

        // Handles Enter key press in the user input textbox for general chat
        private void UserInputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendMessage_Click(sender, e);
            }
        }

        // Handles the Send button click for general chat
        private void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            string input = UserInputTextBox.Text.Trim();
            UserInputTextBox.Clear();

            if (string.IsNullOrWhiteSpace(input))
            {
                AddBotMessage("Encryptonite : I did not quite understand that, can you please rephrase?");
                return;
            }

            AddUserMessage(input);
            ProcessUserInput(input);
        }

        // Adds a message from the bot to the chat display
        private void AddBotMessage(string message)
        {
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(224, 224, 224)), // #E0E0E0
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(8),
                Margin = new Thickness(5, 2, 5, 2),
                HorizontalAlignment = HorizontalAlignment.Left,
                MaxWidth = 700
            };

            var textBlock = new TextBlock
            {
                Text = message,
                TextWrapping = TextWrapping.Wrap,
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51)) // #333333
            };

            border.Child = textBlock;
            ChatDisplayPanel.Children.Add(border);
            // Scroll to the bottom
            var scrollViewer = VisualTreeHelper.GetParent(ChatDisplayPanel) as ScrollViewer;
            if (scrollViewer != null)
            {
                scrollViewer.ScrollToEnd();
            }
        }

        // Adds a message from the user to the chat display
        private void AddUserMessage(string message)
        {
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(220, 248, 198)), // #DCF8C6
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(8),
                Margin = new Thickness(5, 2, 5, 2),
                HorizontalAlignment = HorizontalAlignment.Right,
                MaxWidth = 700
            };

            var textBlock = new TextBlock
            {
                Text = $"{userName}: {message}",
                TextWrapping = TextWrapping.Wrap,
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51)) // #333333
            };

            border.Child = textBlock;
            ChatDisplayPanel.Children.Add(border);
            // Scroll to the bottom
            var scrollViewer = VisualTreeHelper.GetParent(ChatDisplayPanel) as ScrollViewer;
            if (scrollViewer != null)
            {
                scrollViewer.ScrollToEnd();
            }
        }

        private void AddAsciiArtMessage(string message)
        {
            var textBlock = new TextBlock
            {
                Text = message,
                FontFamily = new FontFamily("Consolas"), // Monospace font for ASCII art
                TextWrapping = TextWrapping.NoWrap,     // Crucial for preserving ASCII art layout
                HorizontalAlignment = HorizontalAlignment.Center, // Center the ASCII art
                FontSize = 12, // Slightly smaller font can sometimes fit better
                Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 255)), // Magenta color
                Margin = new Thickness(5, 5, 5, 5) // Add some margin around the art
            };
            ChatDisplayPanel.Children.Add(textBlock);
            // Scroll to the bottom
            var scrollViewer = VisualTreeHelper.GetParent(ChatDisplayPanel) as ScrollViewer;
            if (scrollViewer != null)
            {
                scrollViewer.ScrollToEnd();
            }
        }

        // Processes user input to determine the chatbot's response or action
        private void ProcessUserInput(string input)
        {
            string lowerInput = input.ToLower();

            // Exit commands
            if (lowerInput == "exit" || lowerInput == "quit" || lowerInput == "done" || lowerInput == "bye")
            {
                AddBotMessage("Encryptonite : Stay safe online, thank you for being here. Goodbye!");
                // Application.Current.Shutdown(); //autoshutdown
                return;
            }

            // Simplified standard bot response for this minimal version
            string botResponse = GetBotResponse(lowerInput);
            AddBotMessage($"Encryptonite : {botResponse}");
        }

       
        private string GetBotResponse(string lowerInput)
        {
            // Cybersecurity Topic Responses (simplified for this version)
            if (lowerInput.Contains("phishing"))
            {
                return "Phishing is a cybercrime where scammers use deceptive emails or messages to trick you into revealing personal information. Be careful!";
            }
            else if (lowerInput.Contains("passwords"))
            {
                return "Strong passwords are crucial! Use a mix of characters and make them unique.";
            }
            else if (lowerInput.Contains("safe browse") || lowerInput.Contains("safe browsing"))
            {
                return "Always check for HTTPS in the address bar and be cautious of suspicious websites.";
            }
            else if (lowerInput.Contains("malware"))
            {
                return "Malware is malicious software designed to harm your devices. Keep your software updated and use antivirus.";
            }

            // Proactive suggestions based on time or history (simple simulation)
            if ((DateTime.Now - lastProactiveMentionTime).TotalMinutes > 5 || lastProactiveInterestMentioned == null)
            {
                string[] proactiveSuggestions = {
                    "Have you checked your privacy settings lately?",
                    "Do you know the importance of regularly backing up your data?"
                };
                string suggestion = proactiveSuggestions[randomGenerator.Next(proactiveSuggestions.Length)];
                lastProactiveInterestMentioned = suggestion;
                lastProactiveMentionTime = DateTime.Now;
                return $"By the way, {suggestion}";
            }

            // Default fallback response
            string[] defaultResponses = {
                "I'm not sure I understand. Could you please rephrase?",
                "Could you tell me more about what you're looking for?",
                "Hmm, I need a little more information.",
                "Please ask me a question related to cyber safety."
            };
            return defaultResponses[randomGenerator.Next(defaultResponses.Length)];
        }
    }
}
