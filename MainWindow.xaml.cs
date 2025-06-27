using System;
using System.Collections.Generic;
using System.IO;
using System.Media; 
using System.Linq; 
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Threading.Tasks;
using System.Collections.ObjectModel; 
using System.Windows.Threading; 
using System.ComponentModel; 
using System.Runtime.CompilerServices;


namespace samantha_progpart3
{
    public partial class MainWindow : Window
    {
        private string userName = "User"; // Default user name
        private Dictionary<string, string> userMemory = new Dictionary<string, string>(); 
        private string lastProactiveInterestMentioned = null;
        private DateTime lastProactiveMentionTime = DateTime.MinValue;
        private Random randomGenerator = new Random();
        private Greeting greeter;
        private bool _awaitingUserNameInput = false; 

        // Task Assistant State Management
        private bool _awaitingReminderConfirmation = false; // If bot asked "Would you like a reminder?"
        private bool _awaitingReminderDate = false;        // If bot asked "On what date?"
        private TaskItem _lastCreatedTask = null;          // To link reminder date to the task
        private bool _awaitingTaskForReminderDate = false; // If user said "remind me on [date]" first
        private DateTime? _tempReminderDate = null;        // Stores date for _awaitingTaskForReminderDate state


        // Task Assistant
        private ObservableCollection<TaskItem> tasks = new ObservableCollection<TaskItem>();

        // Quiz Game
        private List<QuizQuestion> quizQuestions;
        private int currentQuestionIndex = 0;
        private int correctAnswersCount = 0;
        private List<RadioButton> currentQuizOptionRadios;
        private bool isQuizActiveInChat = false;
        private bool awaitingQuizAnswerInChat = false; 

        // Activity Log - Removed MaxLogEntries constant, the collection will now store all.
        private ObservableCollection<ActivityLogEntry> activityLog = new ObservableCollection<ActivityLogEntry>();

        // Dictionary for cybersecurity responses - RANDOM RESPONSES
        private Dictionary<string, List<string>> _cybersecurityResponses;

        public MainWindow()
        {
            InitializeComponent();
            greeter = new Greeting(AddAsciiArtMessage, AddBotMessage, LogActivity);
            InitializeChatbot(); // This method now calls greeter.DisplayAsciiArt()
            LoadQuizQuestions(); // Load quiz questions at startup
            TasksListBox.ItemsSource = tasks; 
            ActivityLogListBox.ItemsSource = activityLog; 
            RefreshTasksDisplay();
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
                textBox.Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51)); 
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
            await Task.Delay(500); 
            AddBotMessage("Hello! I am Encryptonite 🤖🔊, your Cybersecurity Awareness Chatbot.");
            await Task.Delay(1000);
            RequestUserName();
        }

        // Prompts the user for their name
        private void RequestUserName()
        {
            AddBotMessage("Before we start, what's your name?");
            _awaitingUserNameInput = true; 
            UserInputTextBox.KeyDown -= UserInputTextBox_KeyDown; 
            UserInputTextBox.KeyDown += GetUserName_KeyDown;      
        }

        // Captures user's name from input after greeting 
        private async void GetUserName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // Call the common handler for username processing
                ProcessUserNameInput();
            }
        }

        // New common method to process username input
        private void ProcessUserNameInput()
        {
            string input = UserInputTextBox.Text.Trim();

            if (!string.IsNullOrWhiteSpace(input))
            {
                userName = input;
                AddUserMessage(input); // This will now correctly show "UserName: input"
                AddBotMessage($"Nice to meet you, {userName}! How can I help you stay safe online today? 🛡️");

                // Display the help menu immediately after the greeting
                greeter.DisplayHelpMenu();

                // Restore normal input processing
                _awaitingUserNameInput = false; 
                UserInputTextBox.KeyDown -= GetUserName_KeyDown;
                UserInputTextBox.KeyDown += UserInputTextBox_KeyDown; 
                LogActivity($"User name set to '{userName}'.");
                UserInputTextBox.Clear(); // Clear the text box AFTER name is processed and added to chat
            }
            else
            {
                AddBotMessage("Please tell me my name."); 
                LogActivity("Empty user name input during greeting.");
            }
        }

        private void InitializeChatbot()
        {
            // Call the Greeting instance to display ASCII art
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
                    "Use HTTPS sites, avoid popups, and never download shady files �🚫🕷️."
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

        // Plays a voice greeting sound
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

            if (string.IsNullOrWhiteSpace(input))
            {
                AddBotMessage("Encryptonite : I did not quite understand that, can you please rephrase?");
                LogActivity("Empty user input."); 
                return;
            }

            AddUserMessage(input); 
            if (_awaitingUserNameInput)
            {
                ProcessUserNameInput();
            }
            else if (_awaitingReminderConfirmation)
            {
                HandleReminderConfirmation(input);
            }
            else if (_awaitingReminderDate)
            {
                HandleReminderDateInput(input);
            }
            else if (_awaitingTaskForReminderDate)
            {
                HandleTaskDescriptionForReminder(input);
            }
            else if (isQuizActiveInChat && awaitingQuizAnswerInChat)
            {
                ProcessChatQuizAnswer(input.ToLower()); 
            }
            else 
            {
                ProcessUserInput(input);
            }
            // Clear input box after it has been fully processed by the appropriate handler
            
            UserInputTextBox.Clear();
        }

        private void AddBotMessage(string message)
        {
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(224, 224, 224)), 
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
            var scrollViewer = VisualTreeHelper.GetParent(ChatDisplayPanel) as ScrollViewer;
            if (scrollViewer != null)
            {
                scrollViewer.ScrollToEnd();
            }
        }

        private void AddUserMessage(string message)
        {
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(220, 248, 198)), 
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
                Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51))
            };

            border.Child = textBlock;
            ChatDisplayPanel.Children.Add(border);
            var scrollViewer = VisualTreeHelper.GetParent(ChatDisplayPanel) as ScrollViewer;
            if (scrollViewer != null)
            {
                scrollViewer.ScrollToEnd();
            }
        }

        // Adds a message specifically for ASCII art
        private void AddAsciiArtMessage(string message)
        {
            var textBlock = new TextBlock
            {
                Text = message,
                FontFamily = new FontFamily("Consolas"),
                TextWrapping = TextWrapping.NoWrap,     
                HorizontalAlignment = HorizontalAlignment.Center, 
                FontSize = 12, 
                Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 255)), // Magenta color
                Margin = new Thickness(5, 5, 5, 5) // Add some margin around the art
            };
            ChatDisplayPanel.Children.Add(textBlock);
            var scrollViewer = VisualTreeHelper.GetParent(ChatDisplayPanel) as ScrollViewer;
            if (scrollViewer != null)
            {
                scrollViewer.ScrollToEnd();
            }
            LogActivity($"Displayed ASCII art."); //  logs the action of displaying ASCII art itself.
        }


        // Processes user input to determine the chatbot's response or action
        private void ProcessUserInput(string input)
        {
            string lowerInput = input.ToLower();

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
                return;
            }

            // --- NLP Simulation / Keyword Detection ---
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

            // Modified to prioritize "remind me on [date]" separately if no explicit task is given
            if (lowerInput.Contains("remind me on") && !lowerInput.Contains("remind me to"))
            {
                HandleStandaloneReminderCommand(input);
                return;
            }
            else if (lowerInput.Contains("remind me to")) // Handles "remind me to [task] on [date]"
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
                RefreshActivityLogDisplay(); 
                AddBotMessage("Here's a summary of recent actions:"); 
                DisplayRecentActivityInChat(); // Now display in chat as well (limited to 5 for conciseness)
                TabControl mainTabControl = (TabControl)FindName("mainTabControl");
                if (mainTabControl != null) mainTabControl.SelectedItem = FindName("ActivityLogTabItem");
                LogActivity("User requested to view full activity log and recent log in chat."); // Log this specific event
                return;
            }
            // Keeping "show recent activity" as a separate command if the user just wants the chat output
            if (lowerInput.Contains("show recent activity") || lowerInput.Contains("recent log"))
            {
                DisplayRecentActivityInChat(); 
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

                    //  Check if the 'value' itself is a cybersecurity keyword
                    if (_cybersecurityResponses.ContainsKey(value.ToLower()))
                    {
                        List<string> responses = _cybersecurityResponses[value.ToLower()];
                        if (responses != null && responses.Count > 0)
                        {
                            string relatedResponse = responses[randomGenerator.Next(responses.Count)];
                            AddBotMessage($"Encryptonite : And speaking of '{value}', did you know? {relatedResponse}");
                            LogActivity($"Provided related info for '{value}' after memory recall."); // Log this specific event
                        }
                    }
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
        /// <param name="input">The user's response (e.g., "yes", "no").</param>
        private void HandleReminderConfirmation(string input)
        {
            string lowerInput = input.ToLower();
            _awaitingReminderConfirmation = false; // Reset the flag immediately

            if (lowerInput.Contains("yes"))
            {
                _awaitingReminderDate = true; // Set flag to expect a date next
                AddBotMessage("Encryptonite : On what date should I remind you? (e.g., '2025-07-20', 'tomorrow', or 'in 3 days')");
                LogActivity("User confirmed reminder, awaiting date.");
            }
            else if (lowerInput.Contains("no"))
            {
                _lastCreatedTask = null; // Clear the reference to the task
                AddBotMessage("Encryptonite : Okay, no reminder for this task.");
                LogActivity("User declined reminder.");
            }
            else
            {
                AddBotMessage("Encryptonite : I didn't understand that. Please say 'yes' or 'no' if you want a reminder, or ignore if you wish to continue.");
                _awaitingReminderConfirmation = true; // Keep the flag true if input wasn't clear
                LogActivity("Invalid reminder confirmation input.");
            }
        }

        /// <param name="input">The user's date string.</param>
        private void HandleReminderDateInput(string input)
        {
            _awaitingReminderDate = false; // Reset the flag immediately
            if (_lastCreatedTask == null)
            {
                AddBotMessage("Encryptonite : Hmm, I lost track of which task this reminder is for. Please try adding the task and reminder again.");
                LogActivity("Reminder date input received, but no last created task found.");
                return;
            }

            DateTime? parsedDate = ParseDateInput(input);

            if (parsedDate.HasValue)
            {
                _lastCreatedTask.ReminderDate = parsedDate;
                RefreshTasksDisplay();
                AddBotMessage($"Encryptonite : Reminder set for '{_lastCreatedTask.Title}' on {parsedDate.Value.ToShortDateString()}.");
                LogActivity($"Reminder set for task '{_lastCreatedTask.Title}' on {parsedDate.Value.ToShortDateString()}.");
                _lastCreatedTask = null; // Clear the reference
            }
            else
            {
                AddBotMessage("Encryptonite : I couldn't understand that date. Please try again using a clear format (e.g., '2025-07-20', 'tomorrow', or 'in 3 days').");
                _awaitingReminderDate = true; // Keep the flag true to re-prompt for date
                LogActivity("Invalid reminder date input.");
            }
        }

        /// <param name="input">The full user input string.</param>
        private void HandleStandaloneReminderCommand(string input)
        {
            string lowerInput = input.ToLower();
            // Extract date part after "remind me on"
            int dateStartIndex = lowerInput.IndexOf("remind me on") + "remind me on".Length;
            string datePart = input.Substring(dateStartIndex).Trim();

            DateTime? parsedDate = ParseDateInput(datePart);

            if (parsedDate.HasValue)
            {
                _tempReminderDate = parsedDate;
                _awaitingTaskForReminderDate = true; // Set flag to expect task description next
                AddBotMessage($"Encryptonite : I can set a reminder for {parsedDate.Value.ToShortDateString()}. What task should I remind you about?");
                LogActivity($"Standalone reminder initiated for {parsedDate.Value.ToShortDateString()}, awaiting task description.");
            }
            else
            {
                AddBotMessage("Encryptonite : I couldn't understand the date for the reminder. Please try again using a clear format (e.g., 'remind me on 2025-07-20').");
                LogActivity("Invalid date input for standalone reminder.");
            }
        }

        /// <param name="input">The user's task description.</param>
        private void HandleTaskDescriptionForReminder(string input)
        {
            _awaitingTaskForReminderDate = false; // Reset the flag immediately

            if (string.IsNullOrWhiteSpace(input))
            {
                AddBotMessage("Encryptonite : You need to tell me what task to remind you about.");
                _awaitingTaskForReminderDate = true; // Re-prompt if input is empty
                LogActivity("Empty task description for standalone reminder.");
                return;
            }

            string taskTitle = input.Trim();
            string description = GetDefaultTaskDescription(taskTitle); // Get a default description or use taskTitle
            DateTime? reminderDate = _tempReminderDate; // Use the stored date

            if (reminderDate.HasValue)
            {
                AddTask(taskTitle, description, reminderDate); // Create the task with the reminder
                AddBotMessage($"Encryptonite : Task '{taskTitle}' added with a reminder for {reminderDate.Value.ToShortDateString()}.");
                LogActivity($"Task '{taskTitle}' added with reminder via standalone flow.");
            }
            else
            {
                AddBotMessage("Encryptonite : Something went wrong. I have the task, but lost the reminder date. Please try again.");
                LogActivity("Error: Task description received for standalone reminder, but reminder date was null.");
            }

            _tempReminderDate = null; // Clear temporary date
        }

        /// <param name="dateInput">The string to parse.</param>
        /// <returns>A nullable DateTime if parsing is successful.</returns>
        private DateTime? ParseDateInput(string dateInput)
        {
            dateInput = dateInput.ToLower();
            DateTime parsedDate;

            // Try to parse absolute date formats first
            if (DateTime.TryParse(dateInput, out parsedDate))
            {
                return parsedDate;
            }

            // Handle relative dates
            if (dateInput.Contains("tomorrow"))
            {
                return DateTime.Today.AddDays(1);
            }
            else if (dateInput.Contains("day")) // "in X days"
            {
                int days;
                // Extract digits from the string (e.g., "in 3 days" -> "3")
                string daysPart = new string(dateInput.Where(char.IsDigit).ToArray());
                if (int.TryParse(daysPart, out days))
                {
                    return DateTime.Today.AddDays(days);
                }
            }
            else if (dateInput.Contains("week")) // "in X weeks"
            {
                int weeks;
                string weeksPart = new string(dateInput.Where(char.IsDigit).ToArray());
                if (int.TryParse(weeksPart, out weeks))
                {
                    return DateTime.Today.AddDays(weeks * 7);
                }
            }
            else if (dateInput.Contains("month")) // "in X months"
            {
                int months;
                string monthsPart = new string(dateInput.Where(char.IsDigit).ToArray());
                if (int.TryParse(monthsPart, out months))
                {
                    return DateTime.Today.AddMonths(months);
                }
            }


            return null; // Return null if date cannot be parsed
        }

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

                // Determine taskTitle and taskDescription
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

                // Try to parse reminder date (using new ParseDateInput method)
                if (reminderIndex != -1)
                {
                    string datePart = remainingInput.Substring(reminderIndex + "reminder".Length).Trim();
                    reminderDate = ParseDateInput(datePart);

                    if (!reminderDate.HasValue && !string.IsNullOrWhiteSpace(datePart))
                    {
                        AddBotMessage($"Encryptonite : I couldn't understand the reminder date '{datePart}'. Task added without reminder for now.");
                        LogActivity($"Failed to add task via NLP: invalid reminder date '{datePart}'. Task added without reminder.");
                        // Continue to add task without reminder, then set state for follow-up
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

            // Add the task
            TaskItem newTask = new TaskItem
            {
                Title = taskTitle,
                Description = taskDescription,
                ReminderDate = reminderDate, // Will be null if not parsed from initial command
                IsCompleted = false
            };
            tasks.Add(newTask);
            RefreshTasksDisplay();
            LogActivity($"Task '{taskTitle}' added. Reminder: {reminderDate?.ToShortDateString() ?? "None"}."); // Log this specific event


            // If no reminder was set in the initial command, ask for confirmation
            if (!reminderDate.HasValue)
            {
                _lastCreatedTask = newTask; // Store reference for follow-up
                _awaitingReminderConfirmation = true;
                AddBotMessage("Encryptonite : Task added. Would you like to add a reminder for this task?");
            }
            else
            {
                AddBotMessage($"Encryptonite : Task added with the description \"{taskDescription}\". I'll remind you on {reminderDate.Value.ToShortDateString()}.");
            }
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

            // Directly add the task with the GUI-provided reminder
            TaskItem newTask = new TaskItem
            {
                Title = title,
                Description = description,
                ReminderDate = reminder,
                IsCompleted = false
            };
            tasks.Add(newTask);
            RefreshTasksDisplay(); // Update display
            AddBotMessage($"Task '{title}' added! {(reminder.HasValue ? $"I'll remind you on {reminder.Value.ToShortDateString()}." : "")}");
            LogActivity($"Task '{title}' added via GUI. Reminder: {reminder?.ToShortDateString() ?? "None"}."); // Log this specific event

            // Clear input fields and reset placeholder text
            TaskTitleTextBox.Text = "Task Title (e.g., Enable 2FA)";
            TaskTitleTextBox.Foreground = new SolidColorBrush(Color.FromRgb(153, 153, 153));
            TaskDescriptionTextBox.Text = "Description (optional)";
            TaskDescriptionTextBox.Foreground = new SolidColorBrush(Color.FromRgb(153, 153, 153));
            ReminderDatePicker.SelectedDate = null;
        }

        private void AddTask(string title, string description, DateTime? reminderDate)
        {
            // Ensure title is not empty or placeholder
            if (string.IsNullOrWhiteSpace(title) || title == "Task Title (e.g., Enable 2FA)")
            {
                LogActivity("AddTask method called with empty/placeholder title (internal call)."); // Log this specific event
                return; // Do not add task if title is invalid from internal call
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
            LogActivity($"New task added (internal): '{title}'."); // Log this specific event
        }
        private void HandleSetReminderCommand(string input)
        {
            string lowerInput = input.ToLower();
            int reminderIndex = lowerInput.IndexOf("remind me to");
            string taskTitle = "";
            DateTime? reminderDate = null;

            if (reminderIndex != -1)
            {
                string remainder = input.Substring(reminderIndex + "remind me to".Length).Trim();

                // Look for "on [date]" in the remainder
                int onIndex = remainder.IndexOf(" on ");
                if (onIndex != -1)
                {
                    taskTitle = remainder.Substring(0, onIndex).Trim();
                    string datePart = remainder.Substring(onIndex + " on ".Length).Trim();
                    reminderDate = ParseDateInput(datePart);
                }
                else
                {
                    taskTitle = remainder; // No date specified in "remind me to [task]"
                }
            }

            if (string.IsNullOrWhiteSpace(taskTitle))
            {
                AddBotMessage("Encryptonite : What should I remind you to do?");
                LogActivity("Failed to set reminder: no task specified.");
                return;
            }

            // If a task title was found, proceed to add the task
            AddTask(taskTitle, GetDefaultTaskDescription(taskTitle), reminderDate); // Re-use the existing AddTask logic
            if (reminderDate.HasValue)
            {
                AddBotMessage($"Encryptonite : Okay, I've set a reminder for '{taskTitle}' on {reminderDate.Value.ToShortDateString()}.");
                LogActivity($"Reminder set for '{taskTitle}'. Date: {reminderDate.Value.ToShortDateString()}.");
            }
            else
            {
                // This case handles "remind me to [task]" without a date,
                // and it should then follow the flow to ask for confirmation
                // of a reminder date, similar to "add task"
                _lastCreatedTask = tasks.LastOrDefault(t => t.Title == taskTitle); // Try to get the newly added task
                if (_lastCreatedTask != null)
                {
                    _awaitingReminderConfirmation = true;
                    AddBotMessage($"Encryptonite : Task '{taskTitle}' added. Would you like to add a reminder for it?");
                    LogActivity($"Task '{taskTitle}' added, awaiting reminder confirmation.");
                }
                else
                {
                    AddBotMessage($"Encryptonite : Task '{taskTitle}' added, but I couldn't confirm a reminder date yet.");
                    LogActivity($"Task '{taskTitle}' added, but last created task not found for reminder.");
                }
            }
        }


        private void RefreshTasksDisplay()
        {
            TasksListBox.Items.Refresh(); //  TaskItem change (e.g., IsCompleted)
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
                    Options = new List<string> { "A. A malicious attempt to obtain sensitive information by disguising as a trustworthy entity", "B. A type of fishing", "C. A cybersecurity software", "D. A strong password" },
                    CorrectAnswerIndex = 0 // Corrected based on the question
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
            DisplayQuizQuestion(); 
        }

        // Method to process quiz answers typed in chat
        private void ProcessChatQuizAnswer(string userInput)
        {
            if (!isQuizActiveInChat || !awaitingQuizAnswerInChat)
            {
                return; 
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

        // Method to display quiz question in the main chat area
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
            // Or it can be used to restart/start the quiz directly from the tab.
            StartChatQuiz(); 
        }

        private void DisplayQuizQuestion()
        {
            // This method updates the visual elements on the Quiz tab 
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
                // This branch handles ending the GUI quiz
                if (isQuizActiveInChat) { EndChatQuiz(); }
                else { EndQuiz(); } // For cases where quiz might have started only from GUI
            }
        }

        private void SubmitQuizAnswer_Click(object sender, RoutedEventArgs e)
        {
            // This handles submission from the GUI
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
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
            else
            {
                QuizFeedbackTextBlock.Text = "Please select an answer.";
                QuizFeedbackTextBlock.Foreground = new SolidColorBrush(Colors.Orange);
                LogActivity("GUI Quiz answer: No selection."); // Log this specific event
            }
        }

        // Ends the quiz and displays the final score 
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

        // Logs an activity with a timestamp 
        private void LogActivity(string description)
        {
            //  activityLog will now store all.
            activityLog.Add(new ActivityLogEntry { Timestamp = DateTime.Now, Description = description });
            RefreshActivityLogDisplay();
        }

        // Refreshes the display of the activity log 
        private void RefreshActivityLogDisplay()
        {
            ActivityLogListBox.Items.Refresh(); 
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

            // Proactive suggestions based on time or history 
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
