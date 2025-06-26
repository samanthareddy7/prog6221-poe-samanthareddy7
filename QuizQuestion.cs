using System.Collections.Generic;

namespace samantha_progpart3 
{
    public class QuizQuestion
    {
        public string QuestionText { get; set; } = string.Empty; // Initialize to empty string
        public List<string> Options { get; set; } = new List<string>(); // Initialize to empty list
        public int CorrectAnswerIndex { get; set; }
    }
}
