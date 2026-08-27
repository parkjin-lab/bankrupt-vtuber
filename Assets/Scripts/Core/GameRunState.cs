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
        public bool membershipJustUnlocked;
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

        public bool rivalMatchHappened;
        public bool lastRivalMatch;
        public bool lastRivalWon;
        public int lastRivalCash;
        public bool goodsUnlocked;
        public bool goodsJustUnlocked;
        public int goodsStock;
        public bool goodsSoldAppliedThisDay;
        public int lastGoodsSold;
        public int lastGoodsRevenue;
        public bool lastGoodsPromoSuccess;
        public float lastStreamPeakViewers;

        public bool agencyFounded;
        public bool juniorScouted;
        public bool sponsorEverSigned;
        public bool sponsorActive;
        public int sponsorDaysLeft;
        public bool juniorAppliedThisDay;
        public bool sponsorDailyAppliedThisDay;
        public bool sponsorLineAppliedThisDay;
        public int lastAgencyFoundCost;
        public int lastJuniorScoutCost;
        public int lastJuniorPay;
        public bool lastJuniorTrainFail;
        public int lastSponsorDaily;
        public int lastSponsorLineBonus;
        public bool lastSponsorLineSuccess;
        public bool lastSponsorBroke;

        public int playerRankingScore;
        public int[] npcRankingScore = new int[3];
        public int[] lastNpcScore = new int[3];
        public int lastRankingScore;
        public int lastDailyRank;
        public int finalRank;
        public int lastRankingFirstPay;
        public bool rankingAppliedThisDay;

        public bool concertBooked;
        public bool concertPending;
        public bool concertPlayed;
        public bool lastStreamWasConcert;
        public int lastConcertCost;
        public int lastConcertPayout;
        public bool lastConcertFailed;
        public bool lastConcertPerformanceSuccess;
        public bool concertResultApplied;

        public int zeroMentalDays;
        public bool zeroMentalCountedThisDay;
        public EndingKind lastEnding;
        public bool retirePicked;

        public int tier0;
        public int tier1;
        public int tier2;
        public int tier3;
        public int tier4;
        public int loyalty;
        public bool minjunPresent;
        public bool minjunEver;
        public int minjunIgnoreSettlements;
        public bool minjunBonusPending;
        public bool lostSuperchatBonusDay;
        public bool haeunPresent;
        public bool haeunEver;
        public bool haeunHurtThisDay;
        public bool fanLetterSentThisDay;
        public bool lastFanLetter;
        public int lastFanSupport;
        public int lastAutoCost;
        public int lastConflictSurcharge;
        public int pendingExtraSurcharge;
        public bool conflictResolved;
        public bool conflictPending;
        public bool autoReplyOn;
        public bool lastMinjunLeft;
        public bool lastHaeunLeft;
        public bool lastHadSuccessfulSuperchat;
        public int lastMissStreak;
        public StreamContentType contentPicked;
        public bool contentMentalAppliedThisDay;

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
            ClearWeek3Progress();
            ClearWeek4Progress();
            ClearWeek5Progress();
            FandomRules.Reset(this, b, null);
            ContentRules.ResetDaily(this);
        }

        void ClearWeek2Progress()
        {
            membershipUnlocked = false;
            membershipJustUnlocked = false;
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

        void ClearWeek3Progress()
        {
            rivalMatchHappened = false;
            lastRivalMatch = false;
            lastRivalWon = false;
            lastRivalCash = 0;
            goodsUnlocked = false;
            goodsJustUnlocked = false;
            goodsStock = 0;
            goodsSoldAppliedThisDay = false;
            lastGoodsSold = 0;
            lastGoodsRevenue = 0;
            lastGoodsPromoSuccess = false;
            lastStreamPeakViewers = 0f;
        }

        void ClearWeek4Progress()
        {
            agencyFounded = false;
            juniorScouted = false;
            sponsorEverSigned = false;
            sponsorActive = false;
            sponsorDaysLeft = 0;
            juniorAppliedThisDay = false;
            sponsorDailyAppliedThisDay = false;
            sponsorLineAppliedThisDay = false;
            lastAgencyFoundCost = 0;
            lastJuniorScoutCost = 0;
            lastJuniorPay = 0;
            lastJuniorTrainFail = false;
            lastSponsorDaily = 0;
            lastSponsorLineBonus = 0;
            lastSponsorLineSuccess = false;
            lastSponsorBroke = false;
        }

        void ClearWeek5Progress()
        {
            playerRankingScore = 0;
            npcRankingScore = new int[3];
            lastNpcScore = new int[3];
            lastRankingScore = 0;
            lastDailyRank = 0;
            finalRank = 0;
            lastRankingFirstPay = 0;
            rankingAppliedThisDay = false;
            concertBooked = false;
            concertPending = false;
            concertPlayed = false;
            lastStreamWasConcert = false;
            lastConcertCost = 0;
            lastConcertPayout = 0;
            lastConcertFailed = false;
            lastConcertPerformanceSuccess = false;
            concertResultApplied = false;
            zeroMentalDays = 0;
            zeroMentalCountedThisDay = false;
            lastEnding = EndingKind.None;
            retirePicked = false;
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

        public void BeginNextDay(Week1Balance b, Week2Balance w2 = null, Week3Balance w3 = null, Week4Balance w4 = null, Week5Balance w5 = null, FandomBalance fandom = null)
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
            lastRivalMatch = false;
            lastRivalWon = false;
            lastRivalCash = 0;
            goodsSoldAppliedThisDay = false;
            lastGoodsSold = 0;
            lastGoodsRevenue = 0;
            lastGoodsPromoSuccess = false;
            lastStreamPeakViewers = 0f;
            juniorAppliedThisDay = false;
            sponsorDailyAppliedThisDay = false;
            sponsorLineAppliedThisDay = false;
            lastAgencyFoundCost = 0;
            lastJuniorScoutCost = 0;
            lastJuniorPay = 0;
            lastJuniorTrainFail = false;
            lastSponsorDaily = 0;
            lastSponsorLineBonus = 0;
            lastSponsorLineSuccess = false;
            lastSponsorBroke = false;
            lastRankingScore = 0;
            lastDailyRank = 0;
            lastRankingFirstPay = 0;
            rankingAppliedThisDay = false;
            lastStreamWasConcert = false;
            lastConcertPayout = 0;
            lastConcertFailed = false;
            lastConcertPerformanceSuccess = false;
            concertResultApplied = false;
            zeroMentalCountedThisDay = false;
            retirePicked = false;
            ClearExtraThreat();
            FandomRules.ResetDaily(this);
            ContentRules.ResetDaily(this);
            Week2Rules.ApplyWeek2Entry(this, w2);
            WeekSchedule.TryUnlockMembership(this, w2);
            Week3Rules.TryUnlockGoods(this, w3);
            FandomRules.OnMorning(this, b, fandom);
        }
    }
}
