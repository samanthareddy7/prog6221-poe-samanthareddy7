using System;
using System.ComponentModel;
using System.Runtime.CompilerServices; 

namespace samantha_progpart3 
{
    public class TaskItem : INotifyPropertyChanged
    {
        private string _title = string.Empty;
        public string Title
        {
            get { return _title; }
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged(); 
                }
            }
        }

        private string _description = string.Empty; 
        public string Description
        {
            get { return _description; }
            set
            {
                if (_description != value)
                {
                    _description = value;
                    OnPropertyChanged(); 
                }
            }
        }

        public DateTime? ReminderDate { get; set; } // Nullable DateTime

        private bool _isCompleted;
        public bool IsCompleted
        {
            get { return _isCompleted; }
            set
            {
                if (_isCompleted != value)
                {
                    _isCompleted = value;
                    OnPropertyChanged(); 
                }
            }
        }

        // Helper property for display in ListBox
        public string ReminderDateFormatted
        {
            get { return ReminderDate?.ToShortDateString() ?? "No reminder"; }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
