using System.Collections.Generic;

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
        public readonly List<ExtraThreatRoll> extraRolls = new List<ExtraThreatRoll>();

        public bool membershipUnlocked;
        public int membershipCount;
        public int viewerBonus;
        public float peakViewersEver;
        public int successfulStreams;
        public int membershipHypeGainedToday;
        public int lastMembershipFromHype;
        public int lastMembershipFromMiss;
        public int lastMembershipPassive;
        public bool membershipPassiveAppliedThisDay;
        public bool week2EntryApplied;
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
            peakViewersEver = 0f;
            successfulStreams = 0;
            membershipHypeGainedToday = 0;
            lastMembershipFromHype = 0;
            lastMembershipFromMiss = 0;
            lastMembershipPassive = 0;
            membershipPassiveAppliedThisDay = false;
            week2EntryApplied = false;
            clipAttemptedThisDay = false;
            lastClipAttempted = false;
            lastClipSuccess = false;
            lastClipCash = 0;
        }

        public void ApplyExtraThreat(ExtraThreatRoll roll)
        {
            ApplyExtraRolls(new[] { roll });
        }

        public void ApplyExtraRolls(IList<ExtraThreatRoll> rolls)
        {
            extraThreatRolled = true;
            extraRolls.Clear();
            extraThreatAmount = 0;
            extraThreatId = "";
            extraThreatName = "";
            extraThreatArt = "";
            if (rolls == null || rolls.Count == 0)
            {
                extraThreatName = "없음";
                return;
            }

            var names = new List<string>(rolls.Count);
            for (int i = 0; i < rolls.Count; i++)
            {
                extraRolls.Add(rolls[i]);
                extraThreatAmount += rolls[i].Amount;
                names.Add(rolls[i].DisplayName);
            }

            extraThreatId = extraRolls[0].Id;
            extraThreatArt = extraRolls[0].ArtPath;
            extraThreatName = string.Join(" · ", names);
        }

        public void ClearExtraThreat()
        {
            extraThreatRolled = false;
            extraThreatId = "";
            extraThreatName = "";
            extraThreatAmount = 0;
            extraThreatArt = "";
            extraRolls.Clear();
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
            lastMembershipFromHype = 0;
            lastMembershipFromMiss = 0;
            lastMembershipPassive = 0;
            membershipPassiveAppliedThisDay = false;
            membershipHypeGainedToday = 0;
            clipAttemptedThisDay = false;
            lastClipAttempted = false;
            lastClipSuccess = false;
            lastClipCash = 0;
            ClearExtraThreat();
            Week2Rules.ApplyWeek2Entry(this, w2);
            WeekSchedule.TryUnlockMembership(this, w2);
        }
    }
}
