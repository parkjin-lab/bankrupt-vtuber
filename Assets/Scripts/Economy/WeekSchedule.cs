namespace BankruptVtuber
{
    public readonly struct DailyBillSet
    {
        public readonly int Rent;
        public readonly int Electric;
        public readonly int License;
        public readonly int Food;
        public readonly int Gear;

        public DailyBillSet(int rent, int electric, int license, int food, int gear)
        {
            Rent = rent;
            Electric = electric;
            License = license;
            Food = food;
            Gear = gear;
        }

        public int Total => Rent + Electric + License + Food + Gear;
    }

    /// <summary>
    /// Week 1 is days 1–5. Week 2 numbers apply only on days 6–10.
    /// </summary>
    public static class WeekSchedule
    {
        public const int Week1LastDay = 5;
        public const int Week2LastDay = 10;

        public static bool InWeek2(GameRunState run) =>
            run != null && run.day > Week1LastDay;

        public static int WeekNumber(GameRunState run) => InWeek2(run) ? 2 : 1;

        public static int LastDayOfCurrentWeek(GameRunState run) =>
            InWeek2(run) ? Week2LastDay : Week1LastDay;

        public static int DaysLeftInWeek(GameRunState run) =>
            LastDayOfCurrentWeek(run) - run.day;

        public static bool CanEnterWeek2(GameRunState run) =>
            run != null && run.day == Week1LastDay &&
            (run.lastOutcome == WeekOutcome.Win || run.lastOutcome == WeekOutcome.Continue);

        public static DailyBillSet FixedBills(GameRunState run, Week1Balance w1, Week2Balance w2)
        {
            if (InWeek2(run) && w2 != null)
                return new DailyBillSet(w2.billRent, w2.billElectricNet, w2.billAvatarLicense, w2.billFood, w2.billGear);
            return new DailyBillSet(w1.billRent, w1.billElectricNet, w1.billAvatarLicense, w1.billFood, w1.billGear);
        }

        public static int TotalFixedBills(GameRunState run, Week1Balance w1, Week2Balance w2) =>
            FixedBills(run, w1, w2).Total;

        public static ExtraThreatDef[] ThreatTable(GameRunState run, Week1Balance w1, Week2Balance w2)
        {
            if (InWeek2(run) && w2 != null)
                return ExtraThreatRules.TableOrDefault(w2.extraThreats, ExtraThreatRules.DefaultWeek2Table());
            return ExtraThreatRules.TableOrDefault(w1);
        }

        public static void TryUnlockMembership(GameRunState run, Week2Balance w2)
        {
            if (run == null || w2 == null || run.membershipUnlocked || !InWeek2(run))
                return;
            if (run.peakViewersEver >= w2.unlockPeakViewers ||
                run.successfulStreams >= w2.unlockSuccessfulStreams)
                UnlockMembership(run, w2);
        }

        public static void UnlockMembership(GameRunState run, Week2Balance w2)
        {
            if (run == null || w2 == null || run.membershipUnlocked)
                return;
            run.membershipUnlocked = true;
            run.membershipCount = w2.startingMembers;
        }
    }
}
