using System;

namespace samantha_progpart3 
{
    public class ActivityLogEntry
    {
        public DateTime Timestamp { get; set; }
        public string Description { get; set; } = string.Empty; 

        public override string ToString()
        {
            return $"{Timestamp:HH:mm:ss} - {Description}";
        }
    }
}
