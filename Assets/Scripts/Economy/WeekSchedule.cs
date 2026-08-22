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
    /// Week 1 is days 1–5. Week 2 is days 6–10. Week 3 is days 11–15.
    /// </summary>
    public static class WeekSchedule
    {
        public const int Week1LastDay = 5;
        public const int Week2LastDay = 10;
        public const int Week3LastDay = 15;

        public static bool InWeek2(GameRunState run) =>
            run != null && run.day > Week1LastDay && run.day <= Week2LastDay;

        public static bool InWeek3(GameRunState run) =>
            run != null && run.day > Week2LastDay && run.day <= Week3LastDay;

        public static int WeekNumber(GameRunState run)
        {
            if (InWeek3(run))
                return 3;
            if (InWeek2(run))
                return 2;
            return 1;
        }

        public static int LastDayOfCurrentWeek(GameRunState run)
        {
            if (InWeek3(run))
                return Week3LastDay;
            if (InWeek2(run))
                return Week2LastDay;
            return Week1LastDay;
        }

        public static int DaysLeftInWeek(GameRunState run) =>
            LastDayOfCurrentWeek(run) - run.day;

        public static bool CanEnterWeek2(GameRunState run) =>
            run != null && run.day == Week1LastDay &&
            (run.lastOutcome == WeekOutcome.Win || run.lastOutcome == WeekOutcome.Continue);

        public static bool CanEnterWeek3(GameRunState run) =>
            run != null && run.day == Week2LastDay &&
            (run.lastOutcome == WeekOutcome.Week2Win || run.lastOutcome == WeekOutcome.Continue);

        public static DailyBillSet FixedBills(GameRunState run, Week1Balance w1, Week2Balance w2, Week3Balance w3 = null)
        {
            if (InWeek3(run) && w3 != null)
                return new DailyBillSet(w3.billRent, w3.billElectricNet, w3.billAvatarLicense, w3.billFood, w3.billGear);
            if (InWeek2(run) && w2 != null)
                return new DailyBillSet(w2.billRent, w2.billElectricNet, w2.billAvatarLicense, w2.billFood, w2.billGear);
            return new DailyBillSet(w1.billRent, w1.billElectricNet, w1.billAvatarLicense, w1.billFood, w1.billGear);
        }

        public static int TotalFixedBills(GameRunState run, Week1Balance w1, Week2Balance w2, Week3Balance w3 = null) =>
            FixedBills(run, w1, w2, w3).Total;

        public static ExtraThreatDef[] ThreatTable(GameRunState run, Week1Balance w1, Week2Balance w2, Week3Balance w3 = null)
        {
            if (InWeek3(run) && w3 != null)
                return ExtraThreatRules.TableOrDefault(w3.extraThreats, ExtraThreatRules.DefaultWeek3Table());
            if (InWeek2(run) && w2 != null)
                return ExtraThreatRules.TableOrDefault(w2.extraThreats, ExtraThreatRules.DefaultWeek2Table());
            return ExtraThreatRules.TableOrDefault(w1);
        }

        public static void TryUnlockMembership(GameRunState run, Week2Balance w2)
        {
            if (run == null || w2 == null || run.membershipUnlocked)
                return;
            if (!InWeek2(run) && !InWeek3(run))
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
