namespace BankruptVtuber
{
    public class GameRunState
    {
        public int day = 1;
        public int cash;
        public int debt;
        public int mental;
        public bool billsAppliedThisDay;
        public bool streamDoneThisDay;

        public int lastStreamIncome;
        public int lastSuperchatIncome;
        public int lastTickIncome;
        public bool lastStreamForceEnded;
        public int lastPerfects;
        public int lastGreats;
        public int lastGoods;
        public int lastMisses;
        public int lastPeakCombo;
        public bool lastHadHype;

        public int lastBills;
        public int lastRepaid;
        public WeekOutcome lastOutcome = WeekOutcome.Continue;

        public void ResetNewRun(Week1Balance b)
        {
            day = 1;
            cash = b.startingCash;
            debt = b.startingDebt;
            mental = b.startingMental;
            billsAppliedThisDay = false;
            streamDoneThisDay = false;
            lastStreamIncome = 0;
            lastSuperchatIncome = 0;
            lastTickIncome = 0;
            lastStreamForceEnded = false;
            lastPerfects = lastGreats = lastGoods = lastMisses = 0;
            lastPeakCombo = 0;
            lastHadHype = false;
            lastBills = 0;
            lastRepaid = 0;
            lastOutcome = WeekOutcome.Continue;
        }

        public void BeginNextDay(Week1Balance b)
        {
            day += 1;
            mental += b.mentalRestoreEachMorning;
            if (mental < 0)
                mental = 0;
            if (mental > b.maxMental)
                mental = b.maxMental;
            billsAppliedThisDay = false;
            streamDoneThisDay = false;
            lastStreamIncome = lastSuperchatIncome = lastTickIncome = 0;
            lastStreamForceEnded = false;
            lastPerfects = lastGreats = lastGoods = lastMisses = 0;
            lastPeakCombo = 0;
            lastHadHype = false;
            lastBills = 0;
            lastRepaid = 0;
            lastOutcome = WeekOutcome.Continue;
        }
    }
}
