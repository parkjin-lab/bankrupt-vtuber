using System;

namespace BankruptVtuber
{
    public static class EconomyRules
    {
        public static int ApplyDailyBills(GameRunState state, Week1Balance b)
        {
            if (state.billsAppliedThisDay)
                return 0;

            ExtraThreatRules.EnsureRolled(state, b);
            int extra = Math.Max(0, state.extraThreatAmount);
            int total = b.TotalDailyBills + extra;
            state.cash -= total;
            state.lastBills = b.TotalDailyBills;
            state.billsAppliedThisDay = true;
            ConvertNegativeCashToDebt(state);
            return total;
        }

        public static int ApplyStreamPayout(GameRunState state, int tickIncome, int superchatIncome, bool forceEnded, Week1Balance b)
        {
            int raw = tickIncome + superchatIncome;
            int paid = forceEnded
                ? raw * b.forceEndIncomeNumerator / Math.Max(1, b.forceEndIncomeDenominator)
                : raw;

            state.lastTickIncome = tickIncome;
            state.lastSuperchatIncome = superchatIncome;
            state.lastStreamIncome = paid;
            state.lastStreamForceEnded = forceEnded;
            state.streamDoneThisDay = true;
            state.cash += paid;
            ConvertNegativeCashToDebt(state);
            return paid;
        }

        public static void ConvertNegativeCashToDebt(GameRunState state)
        {
            if (state.cash >= 0)
                return;
            state.debt += -state.cash;
            state.cash = 0;
        }

        public static int RepayDebt(GameRunState state, int requested)
        {
            int amount = Math.Max(0, Math.Min(requested, Math.Min(state.cash, state.debt)));
            state.cash -= amount;
            state.debt -= amount;
            state.lastRepaid += amount;
            return amount;
        }

        public static WeekOutcome Evaluate(GameRunState state, Week1Balance b)
        {
            if (state.debt >= b.bankruptDebt)
                return WeekOutcome.Bankrupt;
            if (state.day < b.daysInWeek)
                return WeekOutcome.Continue;
            if (state.debt <= b.winDebtMax || state.cash >= b.winCashMin)
                return WeekOutcome.Win;
            return WeekOutcome.WeekFailed;
        }

        public static string FormatWon(int amount)
        {
            bool neg = amount < 0;
            amount = Math.Abs(amount);
            string s = amount.ToString();
            for (int i = s.Length - 3; i > 0; i -= 3)
                s = s.Insert(i, ",");
            return neg ? $"-₩{s}" : $"₩{s}";
        }
    }
}
