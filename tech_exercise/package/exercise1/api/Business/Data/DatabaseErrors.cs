using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace StargateAPI.Business.Data
{
    public static class DatabaseErrors
    {
        private const int SqliteConstraintUnique = 2067;

        /// <summary>
        /// Whether a save failed on a unique index, e.g. a duplicate name (T08) or a second current duty
        /// (T20). These are conflicts with existing data rather than bugs.
        /// </summary>
        public static bool IsUniqueConflict(Exception exception) =>
            exception is DbUpdateException { InnerException: SqliteException { SqliteExtendedErrorCode: SqliteConstraintUnique } };
    }
}
