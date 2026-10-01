namespace Tutor_Manager.Helpers
{
    public static class MoneyFormat
    {
        public static string ZAR(decimal amount) => $"R{amount:N2}";
    }
}