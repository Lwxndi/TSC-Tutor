namespace Tutor_Manager.Models
{
    public class TscSettings
    {
        public int TscSettingsId { get; set; }

        public TimeSpan EarliestTeachingTime { get; set; } = new TimeSpan(7, 0, 0);
        public TimeSpan LatestTeachingTime { get; set; } = new TimeSpan(21, 0, 0);

        public int MinSessionDurationMinutes { get; set; } = 60;
        public int MaxSessionDurationMinutes { get; set; } = 150;
        public int StandardSessionDurationMinutes { get; set; } = 120;

        public int MaxSessionsPerTutorPerDay { get; set; } = 4;
        public int MaxTutoringHoursPerTutorPerDay { get; set; } = 8;
        public int MaxSubjectsPerTutorPerDay { get; set; } = 2;
        public int MaxSubjectsPerTutorPerWeek { get; set; } = 2;

        public int MinBreakBetweenSessionsMinutes { get; set; } = 30;
        public int MaxConsecutiveSessionsBeforeLongerBreak { get; set; } = 2;
        public int LongerBreakMinutes { get; set; } = 60;

        public int MaxWeeklyTutoringHours { get; set; } = 40;
    }
}