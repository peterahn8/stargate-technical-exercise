namespace StargateAPI.Business.Data
{
    /// <summary>
    /// Seeds demo data for local development. Runs only against an empty database, with fixed
    /// dates so every fresh clone sees the same data. The records follow RULE-1 to RULE-7.
    /// </summary>
    public static class DevelopmentSeeder
    {
        public static void Seed(StargateContext context)
        {
            if (context.People.Any()) return;

            // Never an astronaut: no AstronautDetail or AstronautDuty rows (RULE-2).
            var jane = new Person { Name = "Jane Doe" };

            // Active astronaut with history: each previous duty ends the day before the next starts (RULE-5),
            // and only the current duty has no end date (RULE-4).
            var john = new Person
            {
                Name = "John Doe",
                AstronautDetail = new AstronautDetail
                {
                    CurrentRank = "CPT",
                    CurrentDutyTitle = "Commander",
                    CareerStartDate = new DateTime(2018, 1, 15)
                },
                AstronautDuties =
                {
                    new AstronautDuty { Rank = "2LT", DutyTitle = "Pilot", DutyStartDate = new DateTime(2018, 1, 15), DutyEndDate = new DateTime(2020, 6, 30) },
                    new AstronautDuty { Rank = "1LT", DutyTitle = "Pilot", DutyStartDate = new DateTime(2020, 7, 1), DutyEndDate = new DateTime(2022, 12, 31) },
                    new AstronautDuty { Rank = "CPT", DutyTitle = "Commander", DutyStartDate = new DateTime(2023, 1, 1) }
                }
            };

            // Retired: current duty is RETIRED (RULE-6); career ends the day before it starts (RULE-7).
            // The apostrophe also exercises name handling in the by-name endpoints.
            var jack = new Person
            {
                Name = "Jack O'Neill",
                AstronautDetail = new AstronautDetail
                {
                    CurrentRank = "COL",
                    CurrentDutyTitle = "RETIRED",
                    CareerStartDate = new DateTime(2010, 3, 1),
                    CareerEndDate = new DateTime(2021, 12, 31)
                },
                AstronautDuties =
                {
                    new AstronautDuty { Rank = "MAJ", DutyTitle = "Pilot", DutyStartDate = new DateTime(2010, 3, 1), DutyEndDate = new DateTime(2015, 8, 31) },
                    new AstronautDuty { Rank = "COL", DutyTitle = "Commander", DutyStartDate = new DateTime(2015, 9, 1), DutyEndDate = new DateTime(2021, 12, 31) },
                    new AstronautDuty { Rank = "COL", DutyTitle = "RETIRED", DutyStartDate = new DateTime(2022, 1, 1) }
                }
            };

            context.People.AddRange(jane, john, jack);
            context.SaveChanges();
        }
    }
}
