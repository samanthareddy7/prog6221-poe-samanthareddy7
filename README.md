Encryptonite – Cybersecurity Awareness Chatbot (WPF Application)
Overview
Encryptonite is an interactive Windows Presentation Foundation (WPF) desktop application developed in C# that serves as a cybersecurity awareness chatbot. It provides users with a rich, conversational experience, offering detailed answers on cybersecurity topics like phishing, malware, password security, network security, and more. Beyond just answering questions, Encryptonite simulates real conversation with typing effects, remembers key facts shared by the user, detects emotional cues, helps manage cybersecurity-related tasks, and includes a fun quiz to test user knowledge.
Features:
Interactive Chat Interface
* Personalized Greeting: Prompts the user for their name upon startup and responds with a personalized greeting.
* Typing Simulation: Replies are "typed" out character by character with a small delay, enhancing the feeling of a natural, real-time conversation.
* Voice Greeting: Plays an introductory voice greeting (requires Progsound.wav file in the correct directory).
Memory and Recall System
* User Memory: Stores facts shared by the user using natural phrasing (e.g., "My hobby is hiking").
* Information Recall: Can recall previously stored facts on request (e.g., "What is my hobby?", "Recall hobby").
* Contextual Understanding: Utilizes a dictionary to store keywords and associated responses, enabling personalized and context-sensitive conversations.
Cybersecurity Knowledge Base
* Comprehensive Topics: Responds to a wide range of cybersecurity-related keywords and phrases, including:
    * Scams and Phishing
    * Password Creation and Management
    * Types of Malware
    * Safe Browsing Practices
    * Network Security Concepts
    * General Online Safety Tips
    * Two-Factor Authentication (2FA)
* Help Functionality: Users can type "help" to receive a list of topics the chatbot can discuss.
Sentiment and Emotion Detection
* Emotional Responses: Detects words indicating positive or negative emotions in user input.
* Empathetic Interaction: Responds empathetically to user emotions, providing reassurance or encouragement.
Task Assistant
* Task Management: Allows users to add, view, and manage cybersecurity-related tasks (e.g., "Add task - Enable 2FA").
* Task Description Defaults: Provides default descriptions for common tasks if not specified.
* Reminders: Supports setting reminders for tasks using natural language (e.g., "remind me to update antivirus tomorrow").
* Completion Tracking: Users can mark tasks as completed directly within the application.
Cybersecurity Quiz Game
* Knowledge Assessment: Offers a 10-question quiz to test the user's cybersecurity knowledge.
* Question Variety: Includes a mix of 3 True/False questions and 7 Multiple Choice questions.
* Score-Based Feedback: Provides tailored feedback at the end of the quiz based on the user's score:
    * Scores of 5 and below: "Keep learning to stay safe online."
    * Scores of 6 and above: "Well done!"
* Interactive Play: The quiz can be played directly within the chat interface or on a dedicated "Quiz" tab.
Activity Log
* Action Tracking: Maintains a log of recent chatbot activities and user interactions.
* Transparency: Users can view the full activity log or recent entries directly in the chat or on a dedicated tab.
Command Handling
* Session Control: Recognizes exit commands (e.g., "exit", "quit", "done", "bye") to gracefully end the chatbot session.
* Numerical Input: Can process numerical inputs and relate them to password security tips.
Requirements
To run this WPF application, you will need:
1. Windows Operating System: This is a WPF application, designed for Windows.
2. .NET SDK 9.0:
    * Ensure you have the .NET 9.0 SDK installed. You can download it from the official Microsoft .NET website: https://dotnet.microsoft.com/download
    * Verify your installation by opening a Command Prompt or PowerShell and typing: dotnet --list-sdks
3. Visual Studio 2022 (Recommended IDE):
    * Visual Studio is the recommended Integrated Development Environment (IDE) for C# WPF applications.
    * Ensure you have the ".NET desktop development" workload installed during Visual Studio setup.
    * Download from: https://visualstudio.microsoft.com/downloads/
4. (Optional) Rider (Alternative IDE): If you prefer JetBrains Rider, ensure it's up to date and configured for .NET development.
5. Progsound.wav Audio File (Optional for voice greeting):
    * A .wav audio file named Progsound.wav is required for the initial voice greeting.
    * This file MUST be placed in the bin/Debug/net9.0-windows folder (or bin/Release/net9.0-windows for release builds) within your project's output directory. When you build your project, Visual Studio will create these folders.
Setup Instructions (Detailed Guide)
Follow these steps to set up and run the Encryptonite chatbot on your machine:
1. Clone the Repository:
    * Open Git Bash, Command Prompt, or PowerShell.
    * Navigate to the directory where you want to store the project.
    * Execute the following command to clone the project: git clone <your-repository-url-here>
    * 
    * 
    * Replace <your-repository-url-here> with the actual URL of your Git repository.
2. Open the Project in Visual Studio:
    * Navigate to the cloned project directory (samantha_progpart3).
    * Locate the solution file (e.g., samantha_progpart3.sln) and double-click it. This will open the project in Visual Studio.
3. Restore NuGet Packages:
    * Once the project is open in Visual Studio, it should automatically prompt you to restore any missing NuGet packages. If not, you can manually restore them:
        * Go to Solution Explorer (usually on the right side).
        * Right-click on your solution (samantha_progpart3 solution).
        * Select "Restore NuGet Packages" or "Manage NuGet Packages for Solution..." and then click "Restore" in the top right.
4. Place the Progsound.wav File:
    * Locate your Progsound.wav file.
    * Crucially, place this file into the build output directory. After you build your project (see step 5), navigate to: samantha_progpart3\bin\Debug\net9.0-windows\ (for Debug builds) or samantha_progpart3\bin\Release\net9.0-windows\ (for Release builds)
    * Copy Progsound.wav directly into one of these folders. The greeting will only play if the file is in the executable's directory.
5. Build the Project:
    * In Visual Studio, go to the "Build" menu.
    * Select "Build Solution" (or press Ctrl+Shift+B).
    * This compiles the C# code and creates the executable files in the bin/Debug (or bin/Release) folder. Look for "Build Succeeded" in the Output window at the bottom of Visual Studio.
6. Run the Application:
    * In Visual Studio, go to the "Debug" menu.
    * Select "Start Debugging" (or press F5).
    * Alternatively, you can navigate to the bin/Debug/net9.0-windows (or bin/Release/net9.0-windows) folder and double-click the samantha_progpart3.exe file directly.
Usage Instructions
Once the application is running:
* Starting the Chatbot: The bot will automatically initiate a greeting and prompt you for your name.
* Asking Questions: Type your cybersecurity questions or keywords (e.g., "phishing", "malware", "password tips", "network security", "cybersecurity tips"). You can also type "help" to get ideas.
* Sharing Facts (Memory): Tell the bot facts about yourself using sentences like:
    * "My hobby is reading."
    * "My favorite color is blue."
    * The bot will remember these details using the "my ... is ..." pattern.
* Recalling Information: Ask the bot to recall facts using commands like:
    * "What is my hobby?"
    * "Recall hobby"
* Numerical Input: Enter a number, and the bot will explain how numbers can contribute to cybersecurity (e.g., in password strength).
* Task Management:
    * Type "add task" followed by the task title (e.g., "add task - Enable 2FA"). You can also add a description ("add task - Enable 2FA - description on all social media") or a reminder date ("add task - Review privacy settings - reminder 2025-12-31").
    * Navigate to the "Tasks" tab to view, mark as completed, or delete tasks.
* Quiz Game:
    * Type "start quiz" in the chat to begin the cybersecurity quiz.
    * Respond to True/False questions by typing "True" or "False".
    * Respond to Multiple Choice questions by typing the letter of the correct option (A, B, C, or D).
    * You can also start the quiz directly from the "Quiz" tab.
* Activity Log:
    * Type "show activity log" to view a summary of recent actions in the chat and on the "Activity Log" tab.
    * Type "show recent activity" to see only the recent entries in the chat.
* Exiting the Chat: Type "exit", "quit", "done", or "bye" to end the session politely.
Future Improvements
* Enhanced Natural Language Understanding: Implement more advanced NLP techniques for broader understanding and more nuanced conversations.
* Persistent Storage: Integrate a database (e.g., SQLite, Firestore) to store user memory and tasks across different sessions.
* Cross-Platform Audio: Improve audio playback support for non-Windows operating systems if the application were to be adapted for cross-platform use.
* More Varied Emotional Responses: Expand the range and depth of sentiment-based responses.
* User Profiles: Allow multiple user profiles with separate memories and tasks.
* Rich Media Integration: Potentially incorporate images or external links for explanations.
Troubleshooting Guide
If the voice greeting sound is not working:
* File Location: Ensure the Progsound.wav file is located in the executable directory (e.g., samantha_progpart3\bin\Debug\net9.0-windows\). It needs to be in the same folder as the .exe file.
* File Name: Double-check that the file is named exactly Progsound.wav (case-sensitive on some systems, though Windows is usually flexible).
* File Integrity: Verify that the .wav file is not corrupted and plays correctly using a standard media player outside the application.
* Windows-Specific: System.Media.SoundPlayer is a Windows-specific class. If you were attempting to run this on a non-Windows environment (e.g., via Wine on Linux), it might not work. Ensure you are running on Windows.
If your program is crashing or not starting:
* Build Output: Check the "Output" window in Visual Studio (or your IDE's console/terminal output) immediately after attempting to build or run. This window will display detailed error messages and warnings that can pinpoint the problem.
* .NET SDK Version: Confirm that you have .NET SDK 9.0 installed and that your project is targeting this version. You can check your project's target framework in Visual Studio by right-clicking the project in Solution Explorer, selecting "Properties," and looking under "Target framework."
* Included Files: Ensure that all necessary .cs files (like MainWindow.xaml.cs, TaskItem.cs, QuizQuestion.cs, ActivityLogEntry.cs, and Greeting.cs) are part of your project and are correctly compiled. If any are missing or excluded from the project, the build will fail.
* XAML Errors: Check your MainWindow.xaml file for any syntax errors or missing UI elements that are referenced in the C# code. XAML errors can often prevent the application from starting.
* Clean and Rebuild: Sometimes, remnants of previous builds can cause issues. In Visual Studio, go to "Build" > "Clean Solution", then "Build" > "Rebuild Solution".
* Dependencies: Confirm all necessary using statements are present at the top of your C# files (e.g., System.Windows.Controls, System.Collections.ObjectModel).
Developer: Samantha Reddy
