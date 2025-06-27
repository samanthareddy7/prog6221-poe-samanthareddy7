using System;
using System.Collections.Generic;
using System.IO;
using System.Media; // For SoundPlayer on Windows
using System.Linq; // Required for .Where(char.IsDigit) and .Take()
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Threading.Tasks;
using System.Collections.ObjectModel; // For ObservableCollection
using System.Windows.Threading; // Required for Dispatcher.Invoke
using System.ComponentModel; // Required for INotifyPropertyChanged in TaskItem

namespace samantha_progpart3
{
    public partial class MainWindow : Window
    {
        private string userName = "User"; // Default user name
        private Dictionary<string, string> userMemory = new Dictionary<string, string>(); // Enabled user memory
        private string lastProactiveInterestMentioned = null;
        private DateTime lastProactiveMentionTime = DateTime.MinValue;
        private Random randomGenerator = new Random();

        // Task Assistant
        private ObservableCollection<TaskItem> tasks = new ObservableCollection<TaskItem>();

        // Quiz Game
        private List<QuizQuestion> quizQuestions;
        private int currentQuestionIndex = 0;
        private int correctAnswersCount = 0;
        private List<RadioButton> currentQuizOptionRadios;
        private bool isQuizActiveInChat = false; // State for chat-based quiz
        private bool awaitingQuizAnswerInChat = false; // State for chat-based quiz answer input

        // Activity Log - Removed MaxLogEntries constant, the collection will now store all.
        private ObservableCollection<ActivityLogEntry> activityLog = new ObservableCollection<ActivityLogEntry>();
        // private const int MaxLogEntries = 10; // This constant is no longer used to limit the collection itself.

        // Dictionary for cybersecurity responses - NOW LIST OF STRINGS FOR RANDOM RESPONSES
        private Dictionary<string, List<string>> _cybersecurityResponses;

        public MainWindow()
        {
            InitializeComponent();
            InitializeChatbot();
            LoadQuizQuestions(); // Load quiz questions at startup
            TasksListBox.ItemsSource = tasks; // Bind ListBox to ObservableCollection
            ActivityLogListBox.ItemsSource = activityLog; // Bind Activity Log ListBox (now displaying all entries)
            RefreshTasksDisplay(); // Display any pre-loaded tasks (if implementing persistence)
            LogActivity("Chatbot initialized.");
            SimulateGreeting();
            UserInputTextBox.Focus();

            // Add placeholder text logic for Task text boxes
            TaskTitleTextBox.Text = "Task Title (e.g., Enable 2FA)";
            TaskTitleTextBox.Foreground = new SolidColorBrush(Color.FromRgb(153, 153, 153)); // #999999
            TaskDescriptionTextBox.Text = "Description (optional)";
            TaskDescriptionTextBox.Foreground = new SolidColorBrush(Color.FromRgb(153, 153, 153)); // #999999
        }

        // Handles focus for placeholder text in Task text boxes
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null && (textBox.Text == "Task Title (e.g., Enable 2FA)" || textBox.Text == "Description (optional)"))
            {
                textBox.Text = "";
                textBox.Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51)); // #333333
            }
        }

        // Handles lost focus for placeholder text in Task text boxes
        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null && string.IsNullOrWhiteSpace(textBox.Text))
            {
                if (textBox.Name == "TaskTitleTextBox")
                {
                    textBox.Text = "Task Title (e.g., Enable 2FA)";
                }
                else if (textBox.Name == "TaskDescriptionTextBox")
                {
                    textBox.Text = "Description (optional)";
                }
                textBox.Foreground = new SolidColorBrush(Color.FromRgb(153, 153, 153)); // #999999
            }
        }

        // Simulates an initial greeting message and user name request
        private async void SimulateGreeting()
        {
            await Task.Delay(500); // Simulate a short delay before greeting
            AddBotMessage("Hello! I am Encryptonite 🤖🔊, your Cybersecurity Awareness Chatbot.");
            await Task.Delay(1000);
            RequestUserName();
        }

        // Prompts the user for their name
        private void RequestUserName()
        {
            AddBotMessage("Before we start, what's your name?");
            // Temporarily change KeyDown handler to capture user's name
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
                    AddBotMessage("You can ask me about 'phishing?', 'passwords', 'safe Browse', 'scams', 'malware', 'network security', or 'cybersecurity tips'.");
                    AddBotMessage("You can also type 'start quiz' to test your knowledge, 'add task' to manage tasks, or 'show activity log' to see what I've been doing.");

                    // Restore normal input processing
                    UserInputTextBox.KeyDown -= GetUserName_KeyDown; // Remove name handler
                    UserInputTextBox.KeyDown += UserInputTextBox_KeyDown; // Add back default handler
                    LogActivity($"User name set to '{userName}'.");
                }
                else
                {
                    AddBotMessage("Please tell me your name.");
                }
            }
        }

        // Initializes the chatbot, including displaying ASCII art and playing greeting sound
        private void InitializeChatbot()
        {
            // Create an instance of the Greeting class and call its DisplayAsciiArt method
            // Pass the AddAsciiArtMessage, AddBotMessage, and LogActivity methods as actions
            Greeting greeter = new Greeting(AddAsciiArtMessage, AddBotMessage, LogActivity);
            greeter.DisplayAsciiArt();

            PlayVoiceGreeting(); // Enabled voice greeting

            // Initialize cybersecurity responses dictionary directly here with Lists of strings
            _cybersecurityResponses = new Dictionary<string, List<string>>
            {
                { "how are you", new List<string> {
                    "I am great, thanks for asking ☺️! I’m running smooth and securely 🔐 and ready to help!",
                    "I am doing excellent in my digital world thanks! 🔒💻",
                    "Wonderful! and ready to help with your questions!",
                    "Feeling secure and stable ready to assist you?"
                }},
                { "purpose", new List<string> {
                    "My purpose is to help you understand how to stay safe online 🤺."
                }},
                { "scam", new List<string> {
                    "Always verify a site or sender before clicking anything. Don't give scammers a chance!",
                    "Scams are like digital traps don't fall for them! 🕳️🐭",
                    "Always verify links and senders before clicking anything. 🔍📧",
                    "Scammers want your personal info don't let them have it! 🔒🚫"
                }},
                { "what can", new List<string> {
                    "You can ask me about phishing, passwords, safe browsing, scams, malware and cybersecurity tips 💻."
                }},
                { "phishing", new List<string> {
                    "Phishing tricks you into giving personal info so always check email links before opening them and be aware 🔗!",
                    "Phishing often pretends to be your biggest 'want' like a shopping desire for free trust no one and nothing without double checking it is from a reliable source.",
                    "Watch out for emails with urgent requests and dodgy links they could be harmful to your device ! 🐟🚫",
                    "If a website looks incorrect it probably is, verify first, click later!"
                }},
                { "password", new List<string> {
                    "Strong passwords = strong security make sure your password consists of 12+ characters, mix cases, numbers, symbols 🔒.",
                    "A strong password is your first defense! Make it long, unique and impossible to guess. 🔐",
                    "Use a passphrase or a mix of many symbols, numbers, uppercase and lowercase letters.",
                    "Avoid using '123456' or birthdays or simple names even a toddler can crack that one."
                }},
                {"password security", new List<string> {
                    "Strong passwords = strong security make sure your password consists of 12+ characters, mix cases, numbers, symbols 🔒.",
                    "A strong password is your first defense! Make it long, unique and impossible to guess. 🔐",
                    "Use a passphrase or a mix of many symbols, numbers, uppercase and lowercase letters.",
                    "Avoid using '123456' or birthdays or simple names even a toddler can crack that one."
                }},
                { "safe browse", new List<string> {
                    "Use HTTPS sites, avoid popups, and never download shady files 🌐🚫🕷️."
                }},
                { "safe browsing", new List<string> {
                    "Use HTTPS sites, avoid popups, and never download shady files 🌐🚫🕷️."
                }},
                { "malware", new List<string> {
                    "Malware = bad news 🦠. Protect yourself with antivirus software and updates!"
                }},
                { "network security", new List<string> {
                    "It’s like a digital firewall keeping your connection safe and hacker free 🛡️📡.",
                    "Network security keeps your data safe as it travels it's kind of like a bodyguard for your Wi-Fi.",
                    "Strong firewalls and good encryption make your network ninja-proof and safe. 🥷🛡️",
                    "Unsecured networks = hacker party, make sure you lock it down!"
                }},
                { "cybersecurity", new List<string> {
                    "It’s your digital armour against threats like hackers, scams and malware 🧠🖥️.",
                    "Cybersecurity is like hygiene but for your devices it keeps them clean,protected and free from nasty things. 🧼🖥️",
                    "It’s your armor against digital villains so stay patched and protected! So keeping your software updated and your passwords strong is like sharpening your sword in the cyber battle 🛡️",
                    "Think of cybersecurity as your online seatbelt and buckle up thus it helps protect you from crashes like viruses,phishing scams and hackers that are trying to take control!"
                }},
                {"cybersecurity tips", new List<string> {
                    "It’s your digital armour against threats like hackers, scams and malware 🧠🖥️.",
                    "Cybersecurity is like hygiene but for your devices it keeps them clean,protected and free from nasty things. 🧼🖥️",
                    "It’s your armor against digital villains so stay patched and protected! So keeping your software updated and your passwords strong is like sharpening your sword in the cyber battle 🛡️",
                    "Think of cybersecurity as your online seatbelt and buckle up thus it helps protect you from crashes like viruses,phishing scams and hackers that are trying to take control!"
                }},
                {"security advice", new List<string> {
                    "It’s your digital armour against threats like hackers, scams and malware 🧠🖥️.",
                    "Cybersecurity is like hygiene but for your devices it keeps them clean,protected and free from nasty things. 🧼🖥️",
                    "It’s your armor against digital villains so stay patched and protected! So keeping your software updated and your passwords strong is like sharpening your sword in the cyber battle 🛡️",
                    "Think of cybersecurity as your online seatbelt and buckle up thus it helps protect you from crashes like viruses,phishing scams and hackers that are trying to take control!"
                }},
                {"2fa", new List<string> {
                    "Two-factor authentication (2FA) adds an extra layer of security beyond just a password. It usually involves something you know (password) and something you have (like a code from your phone). Enable it wherever possible!"
                }},
                {"two-factor authentication", new List<string> {
                    "Two-factor authentication (2FA) adds an extra layer of security beyond just a password. It usually involves something you know (password) and something you have (like a code from your phone). Enable it wherever possible!"
                }},
                { "help", new List<string> {
                    "Ask me things about 'phishing?' or 'cybersecurity', 'How to make strong passwords?', or 'What’s safe browsing?' 🧾"
                }}
            };
            LogActivity("Chatbot initialized with cybersecurity responses.");
        }

        // Plays a voice greeting sound (Windows-specific)
        private void PlayVoiceGreeting()
        {
            try
            {
               
                string filePath = "Progsound.wav"; 
                if (File.Exists(filePath))
                {
                    System.Media.SoundPlayer player = new System.Media.SoundPlayer(filePath);
                    player.Play();
                }
                else
                {
                    AddBotMessage("[!] Voice greeting file not found! Please check the path is correct.");
                    LogActivity("Voice greeting file not found.");
                }
            }
            catch (Exception ex)
            {
                AddBotMessage($"[!] Failed to play voice greeting: {ex.Message}");
                LogActivity($"Error playing voice greeting: {ex.Message}");
            }
        }

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
                LogActivity("Empty user input."); // Log this specific event
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
            // Removed direct LogActivity call from here to prevent recursive logging
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

        // Adds a message specifically for ASCII art, using a monospace font and no wrapping
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
            LogActivity($"Displayed ASCII art."); // This specific log is fine, it logs the action of displaying ASCII art itself.
        }


        // Processes user input to determine the chatbot's response or action
        private void ProcessUserInput(string input)
        {
            string lowerInput = input.ToLower();

            // --- IMPORTANT: Check for quiz exit command FIRST ---
            if (isQuizActiveInChat && (lowerInput.Contains("end quiz") || lowerInput.Contains("stop quiz")))
            {
                EndChatQuiz();
                AddBotMessage($"Encryptonite : Okay, I've ended the quiz. Your final score was {correctAnswersCount} out of {quizQuestions.Count}.");
                LogActivity("User ended quiz prematurely."); // Log this specific event
                return;
            }

            // If quiz is active and awaiting an answer in chat
            if (isQuizActiveInChat && awaitingQuizAnswerInChat)
            {
                ProcessChatQuizAnswer(lowerInput);
                return;
            }

            // Exit commands
            if (lowerInput == "exit" || lowerInput == "quit" || lowerInput == "done" || lowerInput == "bye")
            {
                // End quiz if active before exiting
                if (isQuizActiveInChat)
                {
                    EndChatQuiz();
                }
                AddBotMessage("Encryptonite : Stay safe online, thank you for being here. Goodbye!");
                LogActivity("Chatbot session ended.");
                // Application.Current.Shutdown(); // Optionally close the application
                return;
            }

            // --- NLP Simulation / Keyword Detection ---
            // Task Management Commands
            if (lowerInput.Contains("add task") || lowerInput.Contains("create task") || lowerInput.Contains("new task"))
            {
                HandleAddTaskCommand(input);
                return;
            }
            if (lowerInput.Contains("show tasks") || lowerInput.Contains("view tasks") || lowerInput.Contains("my tasks"))
            {
                RefreshTasksDisplay();
                AddBotMessage("Here are your current tasks.");
                TabControl mainTabControl = (TabControl)FindName("mainTabControl");
                if (mainTabControl != null) mainTabControl.SelectedItem = FindName("TasksTabItem");
                LogActivity("User requested to view tasks."); // Log this specific event
                return;
            }
            if (lowerInput.Contains("remind me to") || lowerInput.Contains("set a reminder"))
            {
                HandleSetReminderCommand(input);
                return;
            }

            // Quiz Commands
            if (lowerInput.Contains("start quiz") || lowerInput.Contains("play quiz") || lowerInput.Contains("quiz me"))
            {
                StartChatQuiz(); // Call the new chat-based quiz start
                return;
            }


            // Activity Log Commands
            if (lowerInput.Contains("show activity log") || lowerInput.Contains("what have you done") || lowerInput.Contains("view log"))
            {
                RefreshActivityLogDisplay(); // Always update the log tab first
                AddBotMessage("Here's a summary of recent actions:"); // Initial message before detailed log in chat
                DisplayRecentActivityInChat(); // Now display in chat as well (limited to 5 for conciseness)
                TabControl mainTabControl = (TabControl)FindName("mainTabControl");
                if (mainTabControl != null) mainTabControl.SelectedItem = FindName("ActivityLogTabItem");
                LogActivity("User requested to view full activity log and recent log in chat."); // Log this specific event
                return;
            }
            // Keeping "show recent activity" as a separate command if the user just wants the chat output
            if (lowerInput.Contains("show recent activity") || lowerInput.Contains("recent log"))
            {
                DisplayRecentActivityInChat(); // This only displays in chat, no tab switch
                LogActivity("User requested to view recent activity in chat."); // Log this specific event
                return;
            }

            // Natural recall using 'What is my ...?'
            if (lowerInput.StartsWith("what is my "))
            {
                string key = lowerInput.Substring(11).Trim(); // Skip "what is my "
                if (userMemory.ContainsKey(key))
                {
                    AddBotMessage($"Encryptonite : Your {key} is {userMemory[key]}! 🧠✨");
                    LogActivity($"User inquired about '{key}' from memory (found)."); // Log this specific event
                }
                else
                {
                    AddBotMessage($"Encryptonite : I don't seem to know your {key} yet 🤔. Maybe tell me?");
                    LogActivity($"User inquired about '{key}' from memory (not found)."); // Log this specific event
                }
                return;
            }

            // Detects and responds to emotions
            string emotionResponse;
            if (DetectSentiment(lowerInput, out emotionResponse))
            {
                AddBotMessage($"Encryptonite : {emotionResponse}");
                LogActivity($"Sentiment detected: {emotionResponse}"); // Log this specific event
                return;
            }

            // Memory capture using natural phrasing (e.g., "my name is...", "my favorite color is...")
            if (lowerInput.StartsWith("my ") && lowerInput.Contains(" is "))
            {
                int isIndex = lowerInput.IndexOf(" is ");
                string key = lowerInput.Substring(3, isIndex - 3).Trim(); // Skip "my "
                string value = input.Substring(isIndex + 4).Trim(); // Use original input for value case

                if (!string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(value))
                {
                    userMemory[key] = value;
                    AddBotMessage($"Encryptonite : Got it! I'll remember your {key} is {value} 🧠💾.");
                    LogActivity($"Memorized '{key}': '{value}'."); // Log this specific event
                    return;
                }
            }

            // Recalls the memory using "recall [key]"
            if (lowerInput.StartsWith("recall "))
            {
                string key = lowerInput.Substring(7).Trim();
                if (userMemory.ContainsKey(key))
                {
                    AddBotMessage($"Encryptonite : You told me your '{key}' is '{userMemory[key]}' 🤖💡.");
                    LogActivity($"User requested recall for '{key}' (found)."); // Log this specific event
                }
                else
                {
                    AddBotMessage($"Encryptonite : Hmm... I don't remember anything about '{key}' 🧐.");
                    LogActivity($"User requested recall for '{key}' (not found)."); // Log this specific event
                }
                return;
            }

            // Number input processing
            if (int.TryParse(input, out int justNumber))
            {
                AddBotMessage($"Nice, you entered the number: {justNumber}. Numbers help make passwords harder to guess!");
                LogActivity($"User entered number: {justNumber}."); // Log this specific event
                return;
            }
            if (lowerInput.StartsWith("number "))
            {
                string numberPart = input.Substring(7).Trim();
                if (int.TryParse(numberPart, out int result))
                {
                    AddBotMessage($"Sweet! You entered the number: {result}. Numbers can be useful in passwords as they help to strengthen them.");
                    LogActivity($"User tried to enter 'number {numberPart}' (valid)."); // Log this specific event
                }
                else
                {
                    AddBotMessage("Hmm, that is not a valid number. Try entering a valid number.");
                    LogActivity($"User tried to enter 'number {numberPart}' (invalid)."); // Log this specific event
                }
                return;
            }

            // Standard Bot Response
            string botResponse = GetBotResponse(lowerInput);
            AddBotMessage($"Encryptonite : {botResponse}");
            LogActivity($"General bot response: '{botResponse}'"); // Log this specific event
        }


        // --- Task Assistant Logic ---
        private void HandleAddTaskCommand(string input)
        {
            string taskTitle = "";
            string taskDescription = "";
            DateTime? reminderDate = null;

            // Simple parsing: Look for "add task - [title] - description [desc] - reminder [date]"
            // Or "add task [title]"
            string lowerInput = input.ToLower();
            int titleStartIndex = lowerInput.IndexOf("add task") + "add task".Length;
            if (titleStartIndex < input.Length)
            {
                string remainingInput = input.Substring(titleStartIndex).TrimStart('-', ' ');

                int descIndex = remainingInput.IndexOf("description");
                int reminderIndex = remainingInput.IndexOf("reminder");

                if (descIndex != -1)
                {
                    taskTitle = remainingInput.Substring(0, descIndex).TrimEnd('-', ' ');
                    int descEndIndex = reminderIndex != -1 ? reminderIndex : remainingInput.Length;
                    taskDescription = remainingInput.Substring(descIndex + "description".Length, descEndIndex - (descIndex + "description".Length)).Trim();
                }
                else if (reminderIndex != -1)
                {
                    taskTitle = remainingInput.Substring(0, reminderIndex).TrimEnd('-', ' ');
                }
                else
                {
                    taskTitle = remainingInput;
                }

                // Try to parse reminder date (local implementation)
                if (reminderIndex != -1)
                {
                    string datePart = remainingInput.Substring(reminderIndex + "reminder".Length).Trim();
                    if (DateTime.TryParse(datePart, out DateTime parsedDate))
                    {
                        reminderDate = parsedDate;
                    }
                    else if (datePart.Contains("day")) // "in 3 days"
                    {
                        int days;
                        if (int.TryParse(string.Join("", datePart.Where(char.IsDigit)), out days))
                        {
                            reminderDate = DateTime.Now.AddDays(days);
                        }
                    }
                    else if (datePart.Contains("tomorrow"))
                    {
                        reminderDate = DateTime.Now.AddDays(1);
                    }
                    if (!reminderDate.HasValue && !string.IsNullOrWhiteSpace(datePart))
                    {
                        AddBotMessage($"Encryptonite : I couldn't understand the reminder date '{datePart}'. Please use a clear date format (e.g., '2025-12-31').");
                        LogActivity($"Failed to add task via NLP: invalid reminder date '{datePart}'."); // Log this specific event
                        return;
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(taskTitle))
            {
                AddBotMessage("Encryptonite : I need a title for the task. For example: 'Add task - Review privacy settings'.");
                LogActivity("Failed to add task: no title provided."); // Log this specific event
                return;
            }

            // If task description was not explicitly provided by NLP, provide a default one
            if (string.IsNullOrWhiteSpace(taskDescription))
            {
                taskDescription = GetDefaultTaskDescription(taskTitle);
            }

            AddTask(taskTitle, taskDescription, reminderDate);
            AddBotMessage($"Task added with the description \"{taskDescription}\". {(reminderDate.HasValue ? $"I'll remind you on {reminderDate.Value.ToShortDateString()}." : "Would you like a reminder?")}");
            LogActivity($"Task '{taskTitle}' added. Reminder: {reminderDate?.ToShortDateString() ?? "None"}."); // Log this specific event
        }

        // Provides a default description for common cybersecurity tasks
        private string GetDefaultTaskDescription(string title)
        {
            title = title.ToLower();
            if (title.Contains("2fa") || title.Contains("two-factor"))
                return "Enable two-factor authentication for stronger account security.";
            if (title.Contains("password") || title.Contains("update pass"))
                return "Update your passwords to strong, unique combinations.";
            if (title.Contains("privacy settings") || title.Contains("privacy"))
                return "Review account privacy settings to ensure your data is protected.";
            if (title.Contains("antivirus"))
                return "Update or install antivirus software.";
            if (title.Contains("software updates"))
                return "Install critical software updates to patch vulnerabilities.";
            if (title.Contains("phishing"))
                return "Learn to identify and report phishing attempts.";
            return "General cybersecurity task.";
        }

        // Handles the Add Task button click in the GUI
        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            string title = TaskTitleTextBox.Text.Trim();
            string description = TaskDescriptionTextBox.Text.Trim();
            DateTime? reminder = ReminderDatePicker.SelectedDate;

            if (string.IsNullOrWhiteSpace(title) || title == "Task Title (e.g., Enable 2FA)")
            {
                AddBotMessage("Encryptonite : Please enter a title for the task.");
                LogActivity("Failed to add task via GUI: no title provided."); // Log this specific event
                return;
            }

            if (string.IsNullOrWhiteSpace(description) || description == "Description (optional)")
            {
                description = GetDefaultTaskDescription(title);
            }

            AddTask(title, description, reminder);
            AddBotMessage($"Task '{title}' added! {(reminder.HasValue ? $"I'll remind you on {reminder.Value.ToShortDateString()}." : "")}");

            // Clear input fields and reset placeholder text
            TaskTitleTextBox.Text = "Task Title (e.g., Enable 2FA)";
            TaskTitleTextBox.Foreground = new SolidColorBrush(Color.FromRgb(153, 153, 153));
            TaskDescriptionTextBox.Text = "Description (optional)";
            TaskDescriptionTextBox.Foreground = new SolidColorBrush(Color.FromRgb(153, 153, 153));
            ReminderDatePicker.SelectedDate = null;
            LogActivity($"Task '{title}' added via GUI. Reminder: {reminder?.ToShortDateString() ?? "None"}."); // Log this specific event
        }

        // Adds a new TaskItem to the tasks collection (local implementation)
        private void AddTask(string title, string description, DateTime? reminderDate)
        {
            // Ensure title is not empty or placeholder
            if (string.IsNullOrWhiteSpace(title) || title == "Task Title (e.g., Enable 2FA)")
            {
                AddBotMessage("Encryptonite : Task title cannot be empty or the placeholder. Please try again.");
                LogActivity("AddTask method called with empty/placeholder title."); // Log this specific event
                return;
            }

            // Ensure description is not empty or placeholder if it's the only text
            if (string.IsNullOrWhiteSpace(description) || description == "Description (optional)")
            {
                description = GetDefaultTaskDescription(title); // Use default description if empty/placeholder
            }

            TaskItem newTask = new TaskItem
            {
                Title = title,
                Description = description,
                ReminderDate = reminderDate,
                IsCompleted = false
            };
            tasks.Add(newTask);
            RefreshTasksDisplay();
            LogActivity($"New task added: '{title}'."); // Log this specific event
        }

        // Handles NLP command for setting a reminder (local implementation)
        private void HandleSetReminderCommand(string input)
        {
            // Simplified reminder parsing for demonstration
            // "remind me to [task] on [date]"
            string lowerInput = input.ToLower();
            int reminderIndex = lowerInput.IndexOf("remind me to");
            if (reminderIndex != -1)
            {
                string remainder = input.Substring(reminderIndex + "remind me to".Length).Trim();
                string taskTitle = remainder;
                DateTime? reminderDate = null;

                // Look for "on [date]"
                int onIndex = remainder.IndexOf(" on ");
                if (onIndex != -1)
                {
                    taskTitle = remainder.Substring(0, onIndex).Trim();
                    string datePart = remainder.Substring(onIndex + " on ".Length).Trim();
                    if (DateTime.TryParse(datePart, out DateTime parsedDate))
                    {
                        reminderDate = parsedDate;
                    }
                    else if (datePart.Contains("day")) // "in 3 days"
                    {
                        int days;
                        if (int.TryParse(string.Join("", datePart.Where(char.IsDigit)), out days))
                        {
                            reminderDate = DateTime.Now.AddDays(days);
                        }
                    }
                    else if (datePart.Contains("tomorrow"))
                    {
                        reminderDate = DateTime.Now.AddDays(1);
                    }
                    if (!reminderDate.HasValue && !string.IsNullOrWhiteSpace(datePart))
                    {
                        AddBotMessage($"Encryptonite : I couldn't understand the reminder date '{datePart}'. Please use a clear date format (e.g., '2025-12-31').");
                        LogActivity($"Failed to set reminder: invalid date format '{datePart}'."); // Log this specific event
                        return;
                    }
                }

                if (!string.IsNullOrWhiteSpace(taskTitle))
                {
                    AddTask(taskTitle, GetDefaultTaskDescription(taskTitle), reminderDate); // Call local AddTask
                    AddBotMessage($"Encryptonite : Okay, I've set a reminder for '{taskTitle}' {(reminderDate.HasValue ? $"on {reminderDate.Value.ToShortDateString()}." : "without a specific date.")}");
                    LogActivity($"Reminder set for '{taskTitle}'. Date: {reminderDate?.ToShortDateString() ?? "None"}."); // Log this specific event
                }
                else
                {
                    AddBotMessage("Encryptonite : What should I remind you about?");
                    LogActivity("Failed to set reminder: no task specified."); // Log this specific event
                }
            }
        }

        private void RefreshTasksDisplay()
        {
            TasksListBox.Items.Refresh(); // Force UI update if properties within TaskItem change (e.g., IsCompleted)
            LogActivity("Tasks display refreshed."); // Log this specific event
        }

        private void TaskCompleted_Click(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox != null && checkBox.DataContext is TaskItem task)
            {
                task.IsCompleted = checkBox.IsChecked ?? false; // Direct modification
                if (task.IsCompleted)
                {
                    AddBotMessage($"Encryptonite : Great job! Task '{task.Title}' marked as completed. 🎉");
                    LogActivity($"Task '{task.Title}' marked as completed."); // Log this specific event
                }
                else
                {
                    AddBotMessage($"Encryptonite : Task '{task.Title}' marked as incomplete.");
                    LogActivity($"Task '{task.Title}' marked as incomplete."); // Log this specific event
                }
                // Removed the call to RefreshTasksDisplay()
            }
        }

        private void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null && button.DataContext is TaskItem taskToDelete)
            {
                tasks.Remove(taskToDelete); // Direct modification
                AddBotMessage($"Encryptonite : Task '{taskToDelete.Title}' deleted.");
                LogActivity($"Task '{taskToDelete.Title}' deleted."); // Log this specific event
                RefreshTasksDisplay(); // Ensure UI updates
            }
        }


        // --- Quiz Game Logic ---
        // Loads predefined quiz questions (local implementation)
        private void LoadQuizQuestions()
        {
            quizQuestions = new List<QuizQuestion>
            {
                // True/False Questions (3 total)
                new QuizQuestion
                {
                    QuestionText = "True or False: Regularly updating your software protects against known vulnerabilities.",
                    Options = new List<string> { "True", "False" },
                    CorrectAnswerIndex = 0 // True
                },
                new QuizQuestion
                {
                    QuestionText = "True or False: Using the same strong password for all your accounts is a good security practice.",
                    Options = new List<string> { "True", "False" },
                    CorrectAnswerIndex = 1 // False
                },
                new QuizQuestion
                {
                    QuestionText = "True or False: Phishing attacks always involve sophisticated malware.",
                    Options = new List<string> { "True", "False" },
                    CorrectAnswerIndex = 1 // False
                },
                // Multiple Choice Questions (7 total)
                new QuizQuestion
                {
                    QuestionText = "What is phishing?",
                    Options = new List<string> { "A. A type of fishing", "B. A malicious attempt to obtain sensitive information by disguising as a trustworthy entity", "C. A cybersecurity software", "D. A strong password" },
                    CorrectAnswerIndex = 1
                },
                new QuizQuestion
                {
                    QuestionText = "What is Two-Factor Authentication (2FA)?",
                    Options = new List<string> { "A. Using two different passwords for one account", "B. A security method that requires two forms of verification to access an account", "C. A method for encrypting data", "D. A type of malware" },
                    CorrectAnswerIndex = 1
                },
                new QuizQuestion
                {
                    QuestionText = "Which of these is a characteristic of a strong password?",
                    Options = new List<string> { "A. Short and easy to remember", "B. Contains personal information like your name", "C. At least 12 characters, including a mix of upper/lowercase letters, numbers, and symbols", "D. The same password used across multiple accounts" },
                    CorrectAnswerIndex = 2
                },
                new QuizQuestion
                {
                    QuestionText = "What should you do if you receive a suspicious email asking for your login credentials?",
                    Options = new List<string> { "A. Reply immediately asking for more details", "B. Click on all links to investigate", "C. Delete it or mark it as spam, and do not click on any links or attachments", "D. Forward it to all your contacts for awareness" },
                    CorrectAnswerIndex = 2
                },
                new QuizQuestion
                {
                    QuestionText = "What is malware?",
                    Options = new List<string> { "A. Software used for playing games", "B. Software designed to intentionally cause damage to a computer, server, client, or computer network", "C. A programming language", "D. A type of operating system" },
                    CorrectAnswerIndex = 1
                },
                new QuizQuestion
                {
                    QuestionText = "What is the primary purpose of a firewall?",
                    Options = new List<string> { "A. To speed up internet connection", "B. To filter network traffic and prevent unauthorized access", "C. To create strong passwords", "D. To store data securely" },
                    CorrectAnswerIndex = 1
                },
                new QuizQuestion
                {
                    QuestionText = "Why is it important to keep your software updated?",
                    Options = new List<string> { "A. To get new features", "B. To ensure compatibility with new hardware", "C. To patch security vulnerabilities and improve performance", "D. To increase battery life" },
                    CorrectAnswerIndex = 2
                }
            };
            LogActivity("Quiz questions loaded."); // Log this specific event
        }

        // Starts the quiz game (local implementation)
        private void StartChatQuiz()
        {
            if (quizQuestions == null || quizQuestions.Count == 0)
            {
                AddBotMessage("Encryptonite : I don't have any quiz questions loaded right now. Please check back later!");
                LogActivity("Attempted to start quiz, but no questions loaded."); // Log this specific event
                return;
            }

            isQuizActiveInChat = true;
            awaitingQuizAnswerInChat = true;
            currentQuestionIndex = 0;
            correctAnswersCount = 0;
            AddBotMessage("Encryptonite : Great! Let's start the Cybersecurity Quiz in the chat. Type True/False or A, B, C, or D for your answer."); // Updated prompt
            LogActivity("Chat quiz started."); // Log this specific event

            // Also update the visual quiz tab if it exists
            TabControl mainTabControl = (TabControl)FindName("mainTabControl");
            if (mainTabControl != null) mainTabControl.SelectedItem = FindName("QuizTabItem");
            StartQuizButton.IsEnabled = false; // Disable start button on GUI if chat quiz is active
            SubmitQuizAnswerButton.IsEnabled = true;

            DisplayQuizQuestionInChat();
            DisplayQuizQuestion(); // Also update the GUI quiz display
        }

        // Method to process quiz answers typed in chat (local implementation)
        private void ProcessChatQuizAnswer(string userInput)
        {
            if (!isQuizActiveInChat || !awaitingQuizAnswerInChat)
            {
                return; // Should not happen if state is managed correctly
            }

            int selectedAnswerIndex = -1;
            string normalizedInput = userInput.Trim().ToLower();
            QuizQuestion currentQuestion = quizQuestions[currentQuestionIndex];

            // Check if it's a True/False question based on options content
            if (currentQuestion.Options.Count == 2 &&
                currentQuestion.Options.Contains("True") && currentQuestion.Options.Contains("False"))
            {
                if (normalizedInput == "true")
                {
                    selectedAnswerIndex = 0; // Assuming "True" is at index 0
                }
                else if (normalizedInput == "false")
                {
                    selectedAnswerIndex = 1; // Assuming "False" is at index 1
                }
            }
            else // It's a multiple choice question (A, B, C, D)
            {
                if (normalizedInput == "a" || normalizedInput.Contains("answer a"))
                {
                    selectedAnswerIndex = 0;
                }
                else if (normalizedInput == "b" || normalizedInput.Contains("answer b"))
                {
                    selectedAnswerIndex = 1;
                }
                else if (normalizedInput == "c" || normalizedInput.Contains("answer c"))
                {
                    selectedAnswerIndex = 2;
                }
                else if (normalizedInput == "d" || normalizedInput.Contains("answer d"))
                {
                    selectedAnswerIndex = 3;
                }
            }


            if (selectedAnswerIndex != -1)
            {
                if (selectedAnswerIndex == currentQuestion.CorrectAnswerIndex)
                {
                    correctAnswersCount++;
                    AddBotMessage("Encryptonite : Correct! ✅");
                    LogActivity("Chat quiz answer: Correct."); // Log this specific event
                }
                else
                {
                    string correctAnswerText = currentQuestion.Options[currentQuestion.CorrectAnswerIndex];
                    // Remove "A. " or "True" / "False" prefix for display clarity
                    if (correctAnswerText.Length > 3 && (correctAnswerText.StartsWith("A.") || correctAnswerText.StartsWith("B.") || correctAnswerText.StartsWith("C.") || correctAnswerText.StartsWith("D.")))
                    {
                        correctAnswerText = correctAnswerText.Substring(3);
                    }
                    AddBotMessage($"Encryptonite : Incorrect. The correct answer was: {correctAnswerText} ❌");
                    LogActivity("Chat quiz answer: Incorrect."); // Log this specific event
                }

                currentQuestionIndex++;
                Task.Delay(1500).ContinueWith(_ =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        if (currentQuestionIndex < quizQuestions.Count)
                        {
                            DisplayQuizQuestionInChat(); // Display next question in chat
                            DisplayQuizQuestion(); // Update GUI as well
                        }
                        else
                        {
                            EndChatQuiz(); // End quiz if all questions answered
                        }
                    });
                });
            }
            else
            {
                AddBotMessage("Encryptonite : Please respond with True, False, A, B, C, or D for your answer."); // Updated prompt
                LogActivity("Invalid quiz answer format in chat."); // Log this specific event
            }
        }


        // Method to display quiz question in the main chat area (local implementation)
        private void DisplayQuizQuestionInChat()
        {
            if (currentQuestionIndex < quizQuestions.Count)
            {
                QuizQuestion currentQuestion = quizQuestions[currentQuestionIndex];
                string questionOutput = $"Encryptonite : \n\nQuestion {currentQuestionIndex + 1}: {currentQuestion.QuestionText}\n";
                // Only show options if there are more than 0 or it's not explicitly a True/False question to avoid double "True/False" display
                if (currentQuestion.Options != null && currentQuestion.Options.Any())
                {
                    foreach (string option in currentQuestion.Options)
                    {
                        questionOutput += $"{option}\n";
                    }
                }
                AddBotMessage(questionOutput);
                LogActivity($"Displayed quiz question {currentQuestionIndex + 1} in chat."); // Log this specific event
            }
            // EndQuiz is called by ProcessChatQuizAnswer if all questions are done.
        }

        // Method to end quiz gracefully in chat (local implementation)
        private void EndChatQuiz()
        {
            isQuizActiveInChat = false;
            awaitingQuizAnswerInChat = false;
            string feedbackMessage = "";
            if (correctAnswersCount >= 6)
            {
                feedbackMessage = "Well done!";
            }
            else
            {
                feedbackMessage = "Keep learning to stay safe online.";
            }

            AddBotMessage($"Encryptonite : Quiz completed in chat! You scored {correctAnswersCount} out of {quizQuestions.Count}. {feedbackMessage}");
            LogActivity($"Chat quiz ended. Final score: {correctAnswersCount}/{quizQuestions.Count}. Feedback: {feedbackMessage}"); // Log this specific event

            // Reset GUI quiz state
            QuizQuestionTextBlock.Text = "Quiz Completed!";
            QuizOptionsPanel.Children.Clear();
            QuizScoreTextBlock.Text = $"You got {correctAnswersCount} out of {quizQuestions.Count} questions correct. {feedbackMessage}";
            QuizFeedbackTextBlock.Text = "";
            SubmitQuizAnswerButton.IsEnabled = false;
            StartQuizButton.IsEnabled = true;
        }


        private void StartQuiz_Click(object sender, RoutedEventArgs e)
        {
            // This button now primarily mirrors the chat-based quiz start if not already active.
            // Or it can be used to restart/start the quiz directly from the tab.
            StartChatQuiz(); // Re-use the chat-based quiz start logic
        }

        private void DisplayQuizQuestion()
        {
            // This method updates the visual elements on the Quiz tab (local implementation)
            if (currentQuestionIndex < quizQuestions.Count)
            {
                QuizQuestion currentQuestion = quizQuestions[currentQuestionIndex];
                QuizQuestionTextBlock.Text = $"Question {currentQuestionIndex + 1}: {currentQuestion.QuestionText}";
                QuizOptionsPanel.Children.Clear();
                currentQuizOptionRadios = new List<RadioButton>();

                for (int i = 0; i < currentQuestion.Options.Count; i++)
                {
                    RadioButton optionRadio = new RadioButton
                    {
                        Content = currentQuestion.Options[i],
                        GroupName = "QuizOptions",
                        Margin = new Thickness(0, 5, 0, 5),
                        FontSize = 14
                    };
                    QuizOptionsPanel.Children.Add(optionRadio);
                    currentQuizOptionRadios.Add(optionRadio);
                }
                // Ensure submit button is enabled when a question is displayed
                SubmitQuizAnswerButton.IsEnabled = true;
            }
            else
            {
                // This branch handles ending the GUI quiz, it should also call the chat-based end
                if (isQuizActiveInChat) { EndChatQuiz(); }
                else { EndQuiz(); } // For cases where quiz might have started only from GUI
            }
        }

        private void SubmitQuizAnswer_Click(object sender, RoutedEventArgs e)
        {
            // This handles submission from the GUI buttons (local implementation)
            int selectedAnswerIndex = -1;
            for (int i = 0; i < currentQuizOptionRadios.Count; i++)
            {
                if (currentQuizOptionRadios[i].IsChecked == true)
                {
                    selectedAnswerIndex = i;
                    break;
                }
            }

            if (selectedAnswerIndex != -1)
            {
                if (selectedAnswerIndex == quizQuestions[currentQuestionIndex].CorrectAnswerIndex)
                {
                    correctAnswersCount++;
                    QuizFeedbackTextBlock.Text = "Correct! ✅";
                    QuizFeedbackTextBlock.Foreground = new SolidColorBrush(Colors.Green);
                    LogActivity("GUI Quiz answer: Correct."); // Log this specific event
                }
                else
                {
                    string correctAnswerText = quizQuestions[currentQuestionIndex].Options[quizQuestions[currentQuestionIndex].CorrectAnswerIndex];
                    if (correctAnswerText.Length > 3 && (correctAnswerText.StartsWith("A.") || correctAnswerText.StartsWith("B.") || correctAnswerText.StartsWith("C.") || correctAnswerText.StartsWith("D.")))
                    {
                        correctAnswerText = correctAnswerText.Substring(3);
                    }
                    QuizFeedbackTextBlock.Text = $"Incorrect. The correct answer was: {correctAnswerText} ❌";
                    QuizFeedbackTextBlock.Foreground = new SolidColorBrush(Colors.Red);
                    LogActivity("GUI Quiz answer: Incorrect."); // Log this specific event
                }
                currentQuestionIndex++;
                // Give a short delay for feedback before next question
                Task.Delay(1500).ContinueWith(_ =>
                {
                    Dispatcher.Invoke(() => DisplayQuizQuestion());
                }, TaskScheduler.FromCurrentSynchronizationContext()); // Ensure UI thread for Dispatcher
            }
            else
            {
                QuizFeedbackTextBlock.Text = "Please select an answer.";
                QuizFeedbackTextBlock.Foreground = new SolidColorBrush(Colors.Orange);
                LogActivity("GUI Quiz answer: No selection."); // Log this specific event
            }
        }

        // Ends the quiz and displays the final score (local implementation)
        private void EndQuiz()
        {
            // This is the original EndQuiz for GUI only.
            string feedbackMessage = "";
            if (correctAnswersCount >= 6)
            {
                feedbackMessage = "Well done!";
            }
            else
            {
                feedbackMessage = "Keep learning to stay safe online.";
            }

            QuizQuestionTextBlock.Text = "Quiz Completed!";
            QuizOptionsPanel.Children.Clear();
            QuizScoreTextBlock.Text = $"You got {correctAnswersCount} out of {quizQuestions.Count} questions correct. {feedbackMessage}";
            QuizScoreTextBlock.Foreground = new SolidColorBrush(Colors.DarkBlue);
            QuizFeedbackTextBlock.Text = "";
            SubmitQuizAnswerButton.IsEnabled = false;
            StartQuizButton.IsEnabled = true;
            LogActivity($"Quiz ended. Score: {correctAnswersCount}/{quizQuestions.Count}. Feedback: {feedbackMessage}"); // Log this specific event
            AddBotMessage($"Encryptonite : Quiz completed! You scored {correctAnswersCount} out of {quizQuestions.Count}. {feedbackMessage}");
        }

        // --- Activity Log Logic ---
        // Logs an activity with a timestamp (local implementation)
        private void LogActivity(string description)
        {
            // No longer checking for MaxLogEntries here, activityLog will now store all.
            activityLog.Add(new ActivityLogEntry { Timestamp = DateTime.Now, Description = description });
            RefreshActivityLogDisplay();
        }

        // Refreshes the display of the activity log (local implementation)
        private void RefreshActivityLogDisplay()
        {
            ActivityLogListBox.Items.Refresh(); // Force UI update
            // Scroll to the bottom of the log to show the latest entry
            if (ActivityLogListBox.Items.Count > 0)
            {
                ActivityLogListBox.ScrollIntoView(ActivityLogListBox.Items[ActivityLogListBox.Items.Count - 1]);
            }
        }

        // Displays the most recent activity log entries in the main chat area
        private void DisplayRecentActivityInChat()
        {
            if (activityLog.Count == 0)
            {
                AddBotMessage("Encryptonite : There is no recent activity to display yet.");
                LogActivity("Attempted to display recent activity in chat, but log is empty."); // Log this specific event
                return;
            }

            // Get the 5 most recent entries, ensuring the latest are displayed at the bottom of the block
            var recentEntries = activityLog.Reverse().Take(5).Reverse().ToList();

            foreach (var entry in recentEntries)
            {
                AddBotMessage($"  [{entry.Timestamp:HH:mm:ss}] {entry.Description}");
            }
            LogActivity("Displayed recent activity in chat."); // Log this specific event
        }


        // --- NLP Simulation Methods (local implementation) ---

        // Detects sentiment in user input and provides a corresponding response
        private bool DetectSentiment(string input, out string response)
        {
            input = input.ToLower();
            if (input.Contains("happy") || input.Contains("great") || input.Contains("good"))
            {
                response = "That's wonderful to hear! A positive attitude is a great foundation for learning cybersecurity. 😄";
                return true;
            }
            else if (input.Contains("sad") || input.Contains("down") || input.Contains("unhappy"))
            {
                response = "I'm sorry to hear that. Remember, taking small steps to improve your online security can give you more peace of mind. I'm here to help! 😔";
                return true;
            }
            else if (input.Contains("angry") || input.Contains("frustrated"))
            {
                response = "I understand. It can be frustrating dealing with online threats. Let's focus on solutions and how to protect yourself. 😠";
                return true;
            }
            response = "";
            return false;
        }

        // Provides a relevant chatbot response based on user input keywords (using dictionary)
        private string GetBotResponse(string lowerInput)
        {
            // Try to get response from dictionary first
            // Iterate through the dictionary to find if any key is contained in the input
            foreach (var entry in _cybersecurityResponses)
            {
                if (lowerInput.Contains(entry.Key))
                {
                    // Select a random response from the list
                    List<string> responses = entry.Value;
                    if (responses != null && responses.Count > 0)
                    {
                        return responses[randomGenerator.Next(responses.Count)];
                    }
                }
            }

            // Proactive suggestions based on time or history (simple simulation)
            if ((DateTime.Now - lastProactiveMentionTime).TotalMinutes > 5 || lastProactiveInterestMentioned == null)
            {
                string[] proactiveSuggestions = {
                    "Have you checked your privacy settings on social media lately?",
                    "Do you know the importance of regularly backing up your important data?",
                    "Are you familiar with the concept of a 'Zero-Trust' security model?",
                    "Would you like to learn about ransomware and how to protect against it?"
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
                "Hmm, I need a little more information. What topic are you interested in?",
                "I am here to help you understand cybersecurity. Please ask me a question related to cyber safety."
            };
            return defaultResponses[randomGenerator.Next(defaultResponses.Length)];
        }
    }
}