using System;
using System.ComponentModel; // For INotifyPropertyChanged

namespace samantha_progpart3 // Ensure this matches your project's namespace
{
    public class TaskItem : INotifyPropertyChanged
    {
        private string _title;
        public string Title
        {
            get { return _title; }
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged(nameof(Title));
                }
            }
        }

        private string _description;
        public string Description
        {
            get { return _description; }
            set
            {
                if (_description != value)
                {
                    _description = value;
                    OnPropertyChanged(nameof(Description));
                }
            }
        }

        private DateTime? _reminderDate;
        public DateTime? ReminderDate
        {
            get { return _reminderDate; }
            set
            {
                if (_reminderDate != value)
                {
                    _reminderDate = value;
                    OnPropertyChanged(nameof(ReminderDate));
                    OnPropertyChanged(nameof(ReminderDateFormatted));
                }
            }
        }

        private bool _isCompleted;
        public bool IsCompleted
        {
            get { return _isCompleted; }
            set
            {
                if (_isCompleted != value)
                {
                    _isCompleted = value;
                    OnPropertyChanged(nameof(IsCompleted));
                }
            }
        }

        // Helper property for display in ListBox
        public string ReminderDateFormatted
        {
            get { return ReminderDate?.ToShortDateString() ?? "No reminder"; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
