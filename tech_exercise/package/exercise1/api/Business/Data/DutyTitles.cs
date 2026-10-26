namespace StargateAPI.Business.Data
{
    public static class DutyTitles
    {
        public const string Retired = "RETIRED";

        /// <summary>
        /// Whether a duty title means retirement (RULE-6), ignoring case (D4). The single place this is
        /// decided. Titles are stored as received, so "Retired" stays "Retired".
        /// </summary>
        public static bool IsRetired(string? title) => string.Equals(title, Retired, StringComparison.OrdinalIgnoreCase);
    }
}
