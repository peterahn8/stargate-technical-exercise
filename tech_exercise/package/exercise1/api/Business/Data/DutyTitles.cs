namespace StargateAPI.Business.Data
{
    public static class DutyTitles
    {
        public const string Retired = "RETIRED";

        /// <summary>
        /// Whether a duty title means retirement (RULE-6). The single place this is decided, so the
        /// case-sensitivity choice (D4, T18) only has to change here.
        /// </summary>
        public static bool IsRetired(string? title) => title == Retired;
    }
}
