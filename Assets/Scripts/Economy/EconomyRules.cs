using System;

namespace BankruptVtuber
{
    public static class EconomyRules
    {
        public static int ApplyDailyBills(GameRunState state, Week1Balance b, Week2Balance w2 = null, Week3Balance w3 = null, Week4Balance w4 = null, Week5Balance w5 = null, FandomBalance fandom = null)
        {
            if (state.billsAppliedThisDay)
                return 0;

            ExtraThreatRules.EnsureRolled(state, b, w2, w3, w4, w5);
            int extra = Math.Max(0, state.extraThreatAmount);
            int surcharge = FandomRules.ConsumeSurcharge(state);
            extra += surcharge;
            int auto = FandomRules.AutoCostToday(state, fandom);
            state.lastAutoCost = auto;
            int fixedBills = WeekSchedule.TotalFixedBills(state, b, w2, w3, w4, w5);
            int total = fixedBills + extra + auto;
            state.cash -= total;
            state.lastBills = fixedBills;
            state.billsAppliedThisDay = true;
            int support = FandomRules.RollSupport(state, fandom);
            state.lastFanSupport = support;
            if (support > 0)
                state.cash += support;
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

        public static WeekOutcome Evaluate(GameRunState state, Week1Balance b, Week2Balance w2 = null, Week3Balance w3 = null, Week4Balance w4 = null, Week5Balance w5 = null)
        {
            int bankrupt = BankruptDebt(state, b, w2, w3, w4, w5);
            if (state.debt >= bankrupt)
            {
                if (WeekSchedule.InWeek5(state) && w5 != null)
                    state.lastEnding = EndingKind.Bankrupt;
                return WeekOutcome.Bankrupt;
            }

            if (WeekSchedule.InWeek5(state))
            {
                Week5Rules.NoteZeroMentalDay(state);
                if (w5 != null && state.zeroMentalDays >= w5.burnoutZeroMentalDays)
                {
                    state.lastEnding = EndingKind.Burnout;
                    return WeekOutcome.Ending;
                }

                int last5 = w5 != null ? w5.lastDay : WeekSchedule.Week5LastDay;
                if (state.day < last5)
                {
                    state.lastEnding = EndingKind.None;
                    return WeekOutcome.Continue;
                }

                state.lastEnding = Week5Rules.ResolveEnding(state, w5, state.retirePicked);
                return WeekOutcome.Ending;
            }

            if (WeekSchedule.InWeek4(state))
            {
                int last4 = w4 != null ? w4.lastDay : WeekSchedule.Week4LastDay;
                if (state.day < last4)
                    return WeekOutcome.Continue;
                if (w4 != null &&
                    state.agencyFounded &&
                    (state.debt <= w4.winDebtMax || state.cash >= w4.winCashMin))
                    return WeekOutcome.Week4Win;
                return WeekOutcome.WeekFailed;
            }

            if (WeekSchedule.InWeek3(state))
            {
                int last3 = w3 != null ? w3.lastDay : WeekSchedule.Week3LastDay;
                if (state.day < last3)
                    return WeekOutcome.Continue;
                if (w3 != null &&
                    state.goodsUnlocked &&
                    (state.debt <= w3.winDebtMax || state.cash >= w3.winCashMin))
                    return WeekOutcome.Week3Win;
                return WeekOutcome.WeekFailed;
            }

            if (!WeekSchedule.InWeek2(state))
            {
                if (state.day < WeekSchedule.Week1LastDay)
                    return WeekOutcome.Continue;
                if (state.debt <= b.winDebtMax || state.cash >= b.winCashMin)
                    return WeekOutcome.Win;
                return WeekOutcome.WeekFailed;
            }

            int last = w2 != null ? w2.lastDay : WeekSchedule.Week2LastDay;
            if (state.day < last)
                return WeekOutcome.Continue;
            if (w2 != null &&
                state.membershipUnlocked &&
                (state.debt <= w2.winDebtMax || state.cash >= w2.winCashMin))
                return WeekOutcome.Week2Win;
            return WeekOutcome.WeekFailed;
        }

        public static int BankruptDebt(GameRunState state, Week1Balance b, Week2Balance w2 = null, Week3Balance w3 = null, Week4Balance w4 = null, Week5Balance w5 = null)
        {
            if (w5 != null && WeekSchedule.InWeek5(state))
                return w5.bankruptDebt;
            if (w4 != null && WeekSchedule.InWeek4(state))
                return w4.bankruptDebt;
            if (w3 != null && WeekSchedule.InWeek3(state))
                return w3.bankruptDebt;
            if (w2 != null && WeekSchedule.InWeek2(state))
                return w2.bankruptDebt;
            return b != null ? b.bankruptDebt : 180000;
        }

        public static int TonightBills(GameRunState state)
        {
            if (state == null)
                return 0;
            return state.lastBills + state.extraThreatAmount + state.lastConflictSurcharge + state.lastAutoCost;
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
