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
        public bool lastStreamEventHappened;
        public string lastStreamEventName;
        public bool lastStreamEventSuccess;

        public int lastBills;
        public int lastRepaid;
        public WeekOutcome lastOutcome = WeekOutcome.Continue;

        public int runSeed;
        public bool extraThreatRolled;
        public string extraThreatId;
        public string extraThreatName;
        public int extraThreatAmount;
        public string extraThreatArt;

        public bool membershipUnlocked;
        public int membershipCount;
        public int viewerBonus;
        public int lastMembershipFromPerfects;
        public int lastMembershipFromPitch;
        public bool lastMembershipPitchHappened;
        public bool lastMembershipPitchSuccess;
        public int lastMembershipPassive;
        public bool membershipPassiveAppliedThisDay;
        public bool clipAttemptedThisDay;
        public bool lastClipAttempted;
        public bool lastClipSuccess;
        public int lastClipCash;

        public void ResetNewRun(Week1Balance b, int? seed = null)
        {
            day = 1;
            runSeed = seed ?? unchecked((int)(System.DateTime.UtcNow.Ticks & 0x7fffffff));
            cash = b.startingCash;
            debt = b.startingDebt;
            mental = b.startingMental;
            billsAppliedThisDay = false;
            streamDoneThisDay = false;
            ClearExtraThreat();
            lastStreamIncome = 0;
            lastSuperchatIncome = 0;
            lastTickIncome = 0;
            lastStreamForceEnded = false;
            lastPerfects = lastGreats = lastGoods = lastMisses = 0;
            lastPeakCombo = 0;
            lastHadHype = false;
            lastStreamEventHappened = false;
            lastStreamEventName = "";
            lastStreamEventSuccess = false;
            lastBills = 0;
            lastRepaid = 0;
            lastOutcome = WeekOutcome.Continue;
            ClearWeek2Progress();
        }

        void ClearWeek2Progress()
        {
            membershipUnlocked = false;
            membershipCount = 0;
            viewerBonus = 0;
            lastMembershipFromPerfects = 0;
            lastMembershipFromPitch = 0;
            lastMembershipPitchHappened = false;
            lastMembershipPitchSuccess = false;
            lastMembershipPassive = 0;
            membershipPassiveAppliedThisDay = false;
            clipAttemptedThisDay = false;
            lastClipAttempted = false;
            lastClipSuccess = false;
            lastClipCash = 0;
        }

        public void ApplyExtraThreat(ExtraThreatRoll roll)
        {
            extraThreatRolled = true;
            extraThreatId = roll.Id;
            extraThreatName = roll.DisplayName;
            extraThreatAmount = roll.Amount;
            extraThreatArt = roll.ArtPath;
        }

        public void ClearExtraThreat()
        {
            extraThreatRolled = false;
            extraThreatId = "";
            extraThreatName = "";
            extraThreatAmount = 0;
            extraThreatArt = "";
        }

        public void BeginNextDay(Week1Balance b, Week2Balance w2 = null)
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
            lastStreamEventHappened = false;
            lastStreamEventName = "";
            lastStreamEventSuccess = false;
            lastBills = 0;
            lastRepaid = 0;
            lastOutcome = WeekOutcome.Continue;
            lastMembershipFromPerfects = 0;
            lastMembershipFromPitch = 0;
            lastMembershipPitchHappened = false;
            lastMembershipPitchSuccess = false;
            lastMembershipPassive = 0;
            membershipPassiveAppliedThisDay = false;
            clipAttemptedThisDay = false;
            lastClipAttempted = false;
            lastClipSuccess = false;
            lastClipCash = 0;
            ClearExtraThreat();
            WeekSchedule.TryUnlockMembership(this, w2);
        }
    }
}
