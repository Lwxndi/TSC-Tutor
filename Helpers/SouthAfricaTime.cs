// Helpers/SouthAfricaTime.cs
namespace Tutor_Manager.Helpers
{
    public static class SouthAfricaTime
    {
        // SAST is a fixed UTC+2 offset — South Africa does not observe daylight saving.
        private static readonly TimeSpan Offset = TimeSpan.FromHours(2);

        public static DateTime ToUtc(DateTime sastWallClock) =>
            DateTime.SpecifyKind(sastWallClock - Offset, DateTimeKind.Utc);

        public static DateTime ToSast(DateTime utc) =>
            DateTime.SpecifyKind(utc + Offset, DateTimeKind.Unspecified);
    }
}