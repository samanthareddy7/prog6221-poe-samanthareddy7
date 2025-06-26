
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

        // Activity Log
        private ObservableCollection<ActivityLogEntry> activityLog = new ObservableCollection<ActivityLogEntry>();
        private const int MaxLogEntries = 10; // Limit log display

        public MainWindow()
        {
            InitializeComponent();
            InitializeChatbot();
            LoadQuizQuestions(); // Load quiz questions at startup
            TasksListBox.ItemsSource = tasks; // Bind ListBox to ObservableCollection
            ActivityLogListBox.ItemsSource = activityLog; // Bind Activity Log ListBox
            RefreshTasksDisplay(); // Display any pre-loaded tasks (if implementing persistence)
            LogActivity("Chatbot initialized.");
            SimulateGreeting();
            UserInputTextBox.Focus();

            // Add placeholder text logic for Task text boxes
            // Set initial text and foreground color to simulate placeholder
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
            DisplayAsciiArt();
            PlayVoiceGreeting(); // Enabled voice greeting
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

        // Plays a voice greeting sound (Windows-specific)
        private void PlayVoiceGreeting()
        {
            try
            {
                // Ensure Progsound.wav is in your project's root directory and
                // its 'Copy to Output Directory' property is set to 'Copy if newer'.
                string filePath = "Progsound.wav"; // Corrected to match "Progsound.wav" (capital P)
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
                LogActivity("Empty user input.");
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
            LogActivity($"Bot response: '{message}'");
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
            LogActivity($"Displayed ASCII art.");
        }


        // Processes user input to determine the chatbot's response or action
        private void ProcessUserInput(string input)
        {
            string lowerInput = input.ToLower();

            // Exit commands
            if (lowerInput == "exit" || lowerInput == "quit" || lowerInput == "done" || lowerInput == "bye")
            {
                AddBotMessage("Encryptonite : Stay safe online, thank you for being here. Goodbye!");
                LogActivity("Chatbot session ended.");
                // Optionally close the application
                // Application.Current.Shutdown();
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
                TabControl mainTabControl = (TabControl)FindName("mainTabControl"); // Assuming TabControl is named mainTabControl in XAML
                if (mainTabControl != null) mainTabControl.SelectedItem = FindName("TasksTabItem"); // Switch to Tasks tab (ensure this TabItem has x:Name="TasksTabItem")
                LogActivity("User requested to view tasks.");
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
                StartQuiz_Click(null, null);
                AddBotMessage("Great! Let's start the Cybersecurity Quiz.");
                TabControl mainTabControl = (TabControl)FindName("mainTabControl"); // Assuming TabControl is named mainTabControl in XAML
                if (mainTabControl != null) mainTabControl.SelectedItem = FindName("QuizTabItem"); // Switch to Quiz tab (ensure this TabItem has x:Name="QuizTabItem")
                LogActivity("User started quiz.");
                return;
            }

            // Activity Log Commands
            if (lowerInput.Contains("show activity log") || lowerInput.Contains("what have you done") || lowerInput.Contains("view log"))
            {
                RefreshActivityLogDisplay();
                AddBotMessage("Here's a summary of recent actions:");
                TabControl mainTabControl = (TabControl)FindName("mainTabControl"); // Assuming TabControl is named mainTabControl in XAML
                if (mainTabControl != null) mainTabControl.SelectedItem = FindName("ActivityLogTabItem"); // Switch to Activity Log tab (ensure this TabItem has x:Name="ActivityLogTabItem")
                LogActivity("User requested to view activity log.");
                return;
            }

            // Natural recall using 'What is my ...?'
            if (lowerInput.StartsWith("what is my "))
            {
                string key = lowerInput.Substring(11).Trim(); // Skip "what is my "
                if (userMemory.ContainsKey(key))
                {
                    AddBotMessage($"Encryptonite : Your {key} is {userMemory[key]}! 🧠✨");
                }
                else
                {
                    AddBotMessage($"Encryptonite : I don't seem to know your {key} yet 🤔. Maybe tell me?");
                }
                LogActivity($"User inquired about '{key}' from memory.");
                return;
            }

            // Detects and responds to emotions
            if (DetectSentiment(lowerInput, out string emotionResponse))
            {
                AddBotMessage($"Encryptonite : {emotionResponse}");
                LogActivity($"Sentiment detected: {emotionResponse}");
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
                    LogActivity($"Memorized '{key}': '{value}'.");
                    return;
                }
            }

            // Recalls the memory using "recall [key]"
            if (lowerInput.StartsWith("recall "))
            {
                string key = lowerInput.Substring(7).Trim();
                if (userMemory.ContainsKey(key))
                {
                    AddBotMessage($"Encryptonite : You told me your '{key}' is '{userMemory[key]}'🤖💡.");
                }
                else
                {
                    AddBotMessage($"Encryptonite : Hmm... I don't remember anything about '{key}' 🧐.");
                }
                LogActivity($"User requested recall for '{key}'.");
                return;
            }

            // Number input processing
            if (int.TryParse(input, out int justNumber))
            {
                AddBotMessage($"Nice, you entered the number: {justNumber}. Numbers help make passwords harder to guess!");
                LogActivity($"User entered number: {justNumber}.");
                return;
            }
            if (lowerInput.StartsWith("number "))
            {
                string numberPart = input.Substring(7).Trim();
                if (int.TryParse(numberPart, out int result))
                {
                    AddBotMessage($"Sweet! You entered the number: {result}. Numbers can be useful in passwords as they help to strengthen them.");
                }
                else
                {
                    AddBotMessage("Hmm, that is not a valid number. Try entering a valid number.");
                }
                LogActivity($"User tried to enter 'number {numberPart}'.");
                return;
            }

            // Standard Bot Response
            string botResponse = GetBotResponse(lowerInput);
            AddBotMessage($"Encryptonite : {botResponse}");
            LogActivity($"General bot response: '{botResponse}'");
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

                // Try to parse reminder date
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
                }
            }

            if (string.IsNullOrWhiteSpace(taskTitle))
            {
                AddBotMessage("Encryptonite : I need a title for the task. For example: 'Add task - Review privacy settings'.");
                LogActivity("Failed to add task: no title provided.");
                return;
            }

            // If task description was not explicitly provided by NLP, provide a default one
            if (string.IsNullOrWhiteSpace(taskDescription))
            {
                taskDescription = GetDefaultTaskDescription(taskTitle);
            }

            AddTask(taskTitle, taskDescription, reminderDate);
            AddBotMessage($"Task added with the description \"{taskDescription}\". {(reminderDate.HasValue ? $"I'll remind you on {reminderDate.Value.ToShortDateString()}." : "Would you like a reminder?")}");
            LogActivity($"Task '{taskTitle}' added. Reminder: {reminderDate?.ToShortDateString() ?? "None"}.");
        }

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

        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            string title = TaskTitleTextBox.Text.Trim();
            string description = TaskDescriptionTextBox.Text.Trim();
            DateTime? reminder = ReminderDatePicker.SelectedDate;

            if (string.IsNullOrWhiteSpace(title) || title == "Task Title (e.g., Enable 2FA)")
            {
                AddBotMessage("Encryptonite : Please enter a title for the task.");
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
            LogActivity($"Task '{title}' added via GUI. Reminder: {reminder?.ToShortDateString() ?? "None"}.");
        }

        private void AddTask(string title, string description, DateTime? reminderDate)
        {
            // Ensure title is not empty or placeholder
            if (string.IsNullOrWhiteSpace(title) || title == "Task Title (e.g., Enable 2FA)")
            {
                AddBotMessage("Encryptonite : Task title cannot be empty or the placeholder. Please try again.");
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
            LogActivity($"New task added: '{title}'.");
        }

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
                    else
                    {
                        AddBotMessage($"Encryptonite : I couldn't understand the date '{datePart}'. Please use a clear date format (e.g., '2025-12-31').");
                        LogActivity($"Failed to set reminder: invalid date format '{datePart}'.");
                        return;
                    }
                }

                if (!string.IsNullOrWhiteSpace(taskTitle))
                {
                    AddTask(taskTitle, GetDefaultTaskDescription(taskTitle), reminderDate);
                    AddBotMessage($"Encryptonite : Okay, I've set a reminder for '{taskTitle}' {(reminderDate.HasValue ? $"on {reminderDate.Value.ToShortDateString()}." : "without a specific date.")}");
                    LogActivity($"Reminder set for '{taskTitle}'. Date: {reminderDate?.ToShortDateString() ?? "None"}.");
                }
                else
                {
                    AddBotMessage("Encryptonite : What should I remind you about?");
                    LogActivity("Failed to set reminder: no task specified.");
                }
            }
        }

        private void RefreshTasksDisplay()
        {
            TasksListBox.Items.Refresh(); // Force UI update if properties within TaskItem change (e.g., IsCompleted)
            LogActivity("Tasks display refreshed.");
        }

        private void TaskCompleted_Click(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox != null && checkBox.DataContext is TaskItem task)
            {
                task.IsCompleted = checkBox.IsChecked ?? false;
                if (task.IsCompleted)
                {
                    AddBotMessage($"Encryptonite : Great job! Task '{task.Title}' marked as completed. 🎉");
                    LogActivity($"Task '{task.Title}' marked as completed.");
                }
                else
                {
                    AddBotMessage($"Encryptonite : Task '{task.Title}' marked as incomplete.");
                    LogActivity($"Task '{task.Title}' marked as incomplete.");
                }
                RefreshTasksDisplay(); // Ensure UI updates
            }
        }

        private void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null && button.DataContext is TaskItem taskToDelete)
            {
                tasks.Remove(taskToDelete);
                AddBotMessage($"Encryptonite : Task '{taskToDelete.Title}' deleted.");
                LogActivity($"Task '{taskToDelete.Title}' deleted.");
                RefreshTasksDisplay(); // Ensure UI updates
            }
        }


        // --- Quiz Game Logic ---
        private void LoadQuizQuestions()
        {
            quizQuestions = new List<QuizQuestion>
            {
                new QuizQuestion
                {
                    QuestionText = "What is phishing?",
                    Options = new List<string> { "A type of fishing", "A malicious attempt to obtain sensitive information by disguising as a trustworthy entity", "A cybersecurity software", "A strong password" },
                    CorrectAnswerIndex = 1
                },
                new QuizQuestion
                {
                    QuestionText = "What is Two-Factor Authentication (2FA)?",
                    Options = new List<string> { "Using two different passwords for one account", "A security method that requires two forms of verification to access an account", "A method for encrypting data", "A type of malware" },
                    CorrectAnswerIndex = 1
                },
                new QuizQuestion
                {
                    QuestionText = "Which of these is a characteristic of a strong password?",
                    Options = new List<string> { "Short and easy to remember", "Contains personal information like your name", "At least 12 characters, including a mix of upper/lowercase letters, numbers, and symbols", "The same password used across multiple accounts" },
                    CorrectAnswerIndex = 2
                },
                new QuizQuestion
                {
                    QuestionText = "What should you do if you receive a suspicious email asking for your login credentials?",
                    Options = new List<string> { "Reply immediately asking for more details", "Click on all links to investigate", "Delete it or mark it as spam, and do not click on any links or attachments", "Forward it to all your contacts for awareness" },
                    CorrectAnswerIndex = 2
                },
                new QuizQuestion
                {
                    QuestionText = "What is malware?",
                    Options = new List<string> { "Software used for playing games", "Software designed to intentionally cause damage to a computer, server, client, or computer network", "A programming language", "A type of operating system" },
                    CorrectAnswerIndex = 1
                },
                new QuizQuestion
                {
                    QuestionText = "What is ransomware?",
                    Options = new List<string> { "A type of antivirus software", "Malicious software that encrypts your files and demands payment to restore them", "A tool for secure file sharing", "A method for backing up data" },
                    CorrectAnswerIndex = 1
                },
                new QuizQuestion
                {
                    QuestionText = "Which of the following is an example of social engineering?",
                    Options = new List<string> { "Using a complex firewall", "Phishing", "Implementing strong encryption", "Regular software updates" },
                    CorrectAnswerIndex = 1
                },
                new QuizQuestion
                {
                    QuestionText = "What is the primary purpose of a firewall?",
                    Options = new List<string> { "To speed up internet connection", "To filter network traffic and prevent unauthorized access", "To create strong passwords", "To store data securely" },
                    CorrectAnswerIndex = 1
                },
                new QuizQuestion
                {
                    QuestionText = "Why is it important to keep your software updated?",
                    Options = new List<string> { "To get new features", "To ensure compatibility with new hardware", "To patch security vulnerabilities and improve performance", "To increase battery life" },
                    CorrectAnswerIndex = 2
                },
                new QuizQuestion
                {
                    QuestionText = "What does 'HTTPS' in a website URL signify?",
                    Options = new List<string> { "HyperText Transfer Protocol Standard", "Highly Technical Transfer Protocol Secure", "HyperText Transfer Protocol Secure", "High-Level Text Transfer Protocol System" },
                    CorrectAnswerIndex = 2
                }
            };
            LogActivity("Quiz questions loaded.");
        }

        private void StartQuiz_Click(object sender, RoutedEventArgs e)
        {
            currentQuestionIndex = 0;
            correctAnswersCount = 0;
            QuizFeedbackTextBlock.Text = "";
            QuizScoreTextBlock.Text = "";
            SubmitQuizAnswerButton.IsEnabled = true;
            StartQuizButton.IsEnabled = false;
            DisplayQuizQuestion();
            LogActivity("Quiz started.");
        }

        private void DisplayQuizQuestion()
        {
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
                LogActivity($"Displayed quiz question {currentQuestionIndex + 1}.");
            }
            else
            {
                EndQuiz();
            }
        }

        private void SubmitQuizAnswer_Click(object sender, RoutedEventArgs e)
        {
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
                    LogActivity("Quiz answer: Correct.");
                }
                else
                {
                    QuizFeedbackTextBlock.Text = $"Incorrect. The correct answer was: {quizQuestions[currentQuestionIndex].Options[quizQuestions[currentQuestionIndex].CorrectAnswerIndex]} ❌";
                    QuizFeedbackTextBlock.Foreground = new SolidColorBrush(Colors.Red);
                    LogActivity("Quiz answer: Incorrect.");
                }
                currentQuestionIndex++;
                // Use Dispatcher.Invoke for UI updates from a non-UI thread (Task.Delay's continuation)
                Dispatcher.Invoke(() =>
                {
                    Task.Delay(1500).ContinueWith(_ =>
                    {
                        Dispatcher.Invoke(() => DisplayQuizQuestion());
                    }, TaskScheduler.FromCurrentSynchronizationContext()); // Ensure continuation runs on UI thread
                });
            }
            else
            {
                QuizFeedbackTextBlock.Text = "Please select an answer.";
                QuizFeedbackTextBlock.Foreground = new SolidColorBrush(Colors.Orange);
                LogActivity("Quiz answer: No selection.");
            }
        }

        private void EndQuiz()
        {
            QuizQuestionTextBlock.Text = "Quiz Completed!";
            QuizOptionsPanel.Children.Clear();
            QuizScoreTextBlock.Text = $"You got {correctAnswersCount} out of {quizQuestions.Count} questions correct. Well done!";
            QuizScoreTextBlock.Foreground = new SolidColorBrush(Colors.DarkBlue);
            QuizFeedbackTextBlock.Text = "";
            SubmitQuizAnswerButton.IsEnabled = false;
            StartQuizButton.IsEnabled = true;
            LogActivity($"Quiz ended. Score: {correctAnswersCount}/{quizQuestions.Count}.");
            AddBotMessage($"Encryptonite : Quiz completed! You scored {correctAnswersCount} out of {quizQuestions.Count}. You're doing great!");
        }


        // --- Activity Log Logic ---
        private void LogActivity(string description)
        {
            // Remove old entries if exceeding limit
            if (activityLog.Count >= MaxLogEntries)
            {
                // Remove from the beginning to keep the most recent entries
                activityLog.RemoveAt(0);
            }
            activityLog.Add(new ActivityLogEntry { Timestamp = DateTime.Now, Description = description });
            RefreshActivityLogDisplay();
        }

        private void RefreshActivityLogDisplay()
        {
            ActivityLogListBox.Items.Refresh(); // Force UI update
            // Scroll to the bottom of the log to show the latest entry
            if (activityLog.Count > 0)
            {
                ActivityLogListBox.ScrollIntoView(activityLog[activityLog.Count - 1]);
            }
        }


        // --- NLP Simulation Methods (from original bot logic) ---

        // Detects sentiment in user input and provides a corresponding response
        private bool DetectSentiment(string input, out string response)
        {
            input = input.ToLower();
            if (input.Contains("happy") || input.Contains("great") || input.Contains("good"))
            {
                response = "That's wonderful to hear! A positive attitude is a great foundation for learning cybersecurity. �";
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

        // Provides a relevant chatbot response based on user input keywords (full version)
        private string GetBotResponse(string lowerInput)
        {
            // Cybersecurity Topic Responses
            if (lowerInput.Contains("phishing"))
            {
                return "Phishing is a cybercrime where scammers use deceptive emails, messages, or websites to trick you into revealing personal information like passwords or credit card numbers. Always check the sender and look for red flags!";
            }
            else if (lowerInput.Contains("passwords") || lowerInput.Contains("password security"))
            {
                return "Strong passwords are crucial! Use a mix of uppercase and lowercase letters, numbers, and symbols. Make them at least 12 characters long and unique for each account. Consider using a password manager.";
            }
            else if (lowerInput.Contains("safe browse") || lowerInput.Contains("safe browsing"))
            {
                return "Always check if a website uses HTTPS (look for the padlock icon in the address bar). Be cautious of pop-ups and unsolicited downloads. Use a reputable antivirus and keep your browser updated.";
            }
            else if (lowerInput.Contains("scams"))
            {
                return "Online scams come in many forms, from fake lottery wins to tech support scams. If something sounds too good to be true, it probably is. Verify offers independently and never give out personal info to unverified sources.";
            }
            else if (lowerInput.Contains("malware"))
            {
                return "Malware is malicious software like viruses, worms, and ransomware. It can infect your devices, steal data, or disrupt operations. Keep your software updated, use antivirus, and be careful about what you download or open.";
            }
            else if (lowerInput.Contains("network security"))
            {
                return "Network security involves protecting your computer network from unauthorized access. Use strong Wi-Fi passwords, enable WPA2/WPA3 encryption, and consider using a VPN, especially on public Wi-Fi.";
            }
            else if (lowerInput.Contains("cybersecurity tips") || lowerInput.Contains("security advice"))
            {
                return "Here are some quick tips: Use strong, unique passwords; enable 2FA; be wary of suspicious links; keep software updated; back up your data; and learn to recognize phishing attempts.";
            }
            else if (lowerInput.Contains("2fa") || lowerInput.Contains("two-factor authentication"))
            {
                return "Two-factor authentication (2FA) adds an extra layer of security beyond just a password. It usually involves something you know (password) and something you have (like a code from your phone). Enable it wherever possible!";
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
                "Please ask me a question related to cyber safety."
            };
            return defaultResponses[randomGenerator.Next(defaultResponses.Length)];
        }
    }
}
