using System;
using System.IO;
using UnityEngine;

namespace BankruptVtuber
{
    /// <summary>
    /// Atomic JSON save of a full run. Not written mid-stream.
    /// </summary>
    public static class RunSave
    {
        public const int Version = 1;
        public const string FileName = "bankrupt-vtuber-run.json";

        public static string FilePath =>
            Path.Combine(Application.persistentDataPath, FileName);

        public static void Write(GameRunState run)
        {
            if (run == null)
                return;
            try
            {
                string dir = Application.persistentDataPath;
                if (string.IsNullOrEmpty(dir))
                    return;
                Directory.CreateDirectory(dir);
                string json = ToJson(run);
                string path = FilePath;
                string tmp = path + ".tmp";
                File.WriteAllText(tmp, json);
                if (File.Exists(path))
                {
                    string bak = path + ".bak";
                    File.Replace(tmp, path, bak);
                    if (File.Exists(bak))
                        File.Delete(bak);
                }
                else
                    File.Move(tmp, path);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[파산 버튜버] save failed: " + e.Message);
            }
        }

        public static bool Exists()
        {
            try
            {
                return File.Exists(FilePath);
            }
            catch
            {
                return false;
            }
        }

        public static bool HasValidSave()
        {
            if (!Exists())
                return false;
            var probe = new GameRunState();
            if (TryLoad(probe))
                return true;
            Delete();
            return false;
        }

        public static bool TryLoad(GameRunState dest)
        {
            try
            {
                if (dest == null || !File.Exists(FilePath))
                    return false;
                string json = File.ReadAllText(FilePath);
                return TryApplyJson(json, dest);
            }
            catch
            {
                return false;
            }
        }

        public static void Delete()
        {
            try
            {
                string path = FilePath;
                if (File.Exists(path))
                    File.Delete(path);
                if (File.Exists(path + ".tmp"))
                    File.Delete(path + ".tmp");
                if (File.Exists(path + ".bak"))
                    File.Delete(path + ".bak");
            }
            catch
            {
            }
        }

        public static string ToJson(GameRunState run) =>
            JsonUtility.ToJson(Capture(run), false);

        public static bool TryApplyJson(string json, GameRunState dest)
        {
            try
            {
                if (dest == null || string.IsNullOrWhiteSpace(json))
                    return false;
                json = json.Trim();
                if (json.Length < 2 || json[0] != '{')
                    return false;
                var data = JsonUtility.FromJson<RunSaveData>(json);
                if (!IsValid(data))
                    return false;
                Apply(data, dest);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool DummyRoundTrip()
        {
            var src = MakeDummy();
            string json = ToJson(src);
            var dst = new GameRunState();
            if (!TryApplyJson(json, dst))
                return false;
            return dst.day == 11
                && dst.cash == 88000
                && dst.debt == 21000
                && dst.mental == 72
                && dst.loyalty == 62
                && dst.tier0 == 18
                && dst.tier1 == 4
                && dst.tier2 == 2
                && dst.tier3 == 8
                && dst.tier4 == 2
                && dst.membershipUnlocked
                && dst.membershipCount == 8
                && dst.goodsUnlocked
                && dst.goodsStock == 14
                && dst.minjunPresent
                && dst.haeunPresent
                && dst.conflictPending
                && !dst.billsAppliedThisDay
                && !dst.streamDoneThisDay
                && dst.runSeed == 4242
                && dst.extraRolls.Count == 1
                && dst.extraRolls[0].Amount == 7000
                && dst.npcRankingScore != null
                && dst.npcRankingScore.Length == 3
                && dst.npcRankingScore[1] == 360
                && dst.contentPicked == StreamContentType.Game;
        }

        public static GameRunState MakeDummy()
        {
            var run = new GameRunState();
            run.day = 11;
            run.cash = 88000;
            run.debt = 21000;
            run.mental = 72;
            run.billsAppliedThisDay = false;
            run.streamDoneThisDay = false;
            run.runSeed = 4242;
            run.membershipUnlocked = true;
            run.membershipCount = 8;
            run.viewerBonus = 6;
            run.peakViewersEver = 48f;
            run.successfulStreams = 7;
            run.week2EntryApplied = true;
            run.goodsUnlocked = true;
            run.goodsStock = 14;
            run.agencyFounded = false;
            run.juniorScouted = false;
            run.tier0 = 18;
            run.tier1 = 4;
            run.tier2 = 2;
            run.tier3 = 8;
            run.tier4 = 2;
            run.loyalty = 62;
            run.minjunPresent = true;
            run.minjunEver = true;
            run.haeunPresent = true;
            run.haeunEver = true;
            run.conflictPending = true;
            run.conflictResolved = false;
            run.playerRankingScore = 0;
            run.npcRankingScore = new[] { 420, 360, 300 };
            run.lastNpcScore = new[] { 0, 0, 0 };
            run.concertPlayed = false;
            run.lastEnding = EndingKind.None;
            run.lastOutcome = WeekOutcome.Continue;
            run.contentPicked = StreamContentType.Game;
            run.contentMentalAppliedThisDay = false;
            run.ApplyExtraRolls(new[]
            {
                new ExtraThreatRoll("gear_break", "장비 고장", 7000, "Art/bill_gear", new Color(1f, 0.42f, 0.42f))
            });
            return run;
        }

        static bool IsValid(RunSaveData data)
        {
            if (data == null || data.version != Version)
                return false;
            if (data.day < 1 || data.day > 25)
                return false;
            if (data.lastOutcome < 0 || data.lastOutcome > 7)
                return false;
            if (data.lastEnding < 0 || data.lastEnding > 6)
                return false;
            return true;
        }

        static RunSaveData Capture(GameRunState run)
        {
            var data = new RunSaveData
            {
                version = Version,
                day = run.day,
                cash = run.cash,
                debt = run.debt,
                mental = run.mental,
                billsAppliedThisDay = run.billsAppliedThisDay,
                streamDoneThisDay = run.streamDoneThisDay,
                lastStreamIncome = run.lastStreamIncome,
                lastSuperchatIncome = run.lastSuperchatIncome,
                lastTickIncome = run.lastTickIncome,
                lastStreamForceEnded = run.lastStreamForceEnded,
                lastPerfects = run.lastPerfects,
                lastGreats = run.lastGreats,
                lastGoods = run.lastGoods,
                lastMisses = run.lastMisses,
                lastPeakCombo = run.lastPeakCombo,
                lastHadHype = run.lastHadHype,
                lastStreamEventHappened = run.lastStreamEventHappened,
                lastStreamEventName = run.lastStreamEventName ?? "",
                lastStreamEventSuccess = run.lastStreamEventSuccess,
                lastBills = run.lastBills,
                lastRepaid = run.lastRepaid,
                lastOutcome = (int)run.lastOutcome,
                runSeed = run.runSeed,
                extraThreatRolled = run.extraThreatRolled,
                extraThreatId = run.extraThreatId ?? "",
                extraThreatName = run.extraThreatName ?? "",
                extraThreatAmount = run.extraThreatAmount,
                extraThreatArt = run.extraThreatArt ?? "",
                extraRolls = CaptureRolls(run),
                membershipUnlocked = run.membershipUnlocked,
                membershipJustUnlocked = run.membershipJustUnlocked,
                membershipCount = run.membershipCount,
                viewerBonus = run.viewerBonus,
                peakViewersEver = run.peakViewersEver,
                successfulStreams = run.successfulStreams,
                membershipHypeGainedToday = run.membershipHypeGainedToday,
                lastMembershipFromHype = run.lastMembershipFromHype,
                lastMembershipFromMiss = run.lastMembershipFromMiss,
                lastMembershipPassive = run.lastMembershipPassive,
                membershipPassiveAppliedThisDay = run.membershipPassiveAppliedThisDay,
                week2EntryApplied = run.week2EntryApplied,
                clipAttemptedThisDay = run.clipAttemptedThisDay,
                lastClipAttempted = run.lastClipAttempted,
                lastClipSuccess = run.lastClipSuccess,
                lastClipCash = run.lastClipCash,
                rivalMatchHappened = run.rivalMatchHappened,
                lastRivalMatch = run.lastRivalMatch,
                lastRivalWon = run.lastRivalWon,
                lastRivalCash = run.lastRivalCash,
                goodsUnlocked = run.goodsUnlocked,
                goodsJustUnlocked = run.goodsJustUnlocked,
                goodsStock = run.goodsStock,
                goodsSoldAppliedThisDay = run.goodsSoldAppliedThisDay,
                lastGoodsSold = run.lastGoodsSold,
                lastGoodsRevenue = run.lastGoodsRevenue,
                lastGoodsPromoSuccess = run.lastGoodsPromoSuccess,
                lastStreamPeakViewers = run.lastStreamPeakViewers,
                agencyFounded = run.agencyFounded,
                agencyJustFounded = run.agencyJustFounded,
                juniorScouted = run.juniorScouted,
                sponsorEverSigned = run.sponsorEverSigned,
                sponsorActive = run.sponsorActive,
                sponsorDaysLeft = run.sponsorDaysLeft,
                juniorAppliedThisDay = run.juniorAppliedThisDay,
                sponsorDailyAppliedThisDay = run.sponsorDailyAppliedThisDay,
                sponsorLineAppliedThisDay = run.sponsorLineAppliedThisDay,
                lastAgencyFoundCost = run.lastAgencyFoundCost,
                lastJuniorScoutCost = run.lastJuniorScoutCost,
                lastJuniorPay = run.lastJuniorPay,
                lastJuniorTrainFail = run.lastJuniorTrainFail,
                lastSponsorDaily = run.lastSponsorDaily,
                lastSponsorLineBonus = run.lastSponsorLineBonus,
                lastSponsorLineSuccess = run.lastSponsorLineSuccess,
                lastSponsorBroke = run.lastSponsorBroke,
                playerRankingScore = run.playerRankingScore,
                npcRankingScore = Copy3(run.npcRankingScore),
                lastNpcScore = Copy3(run.lastNpcScore),
                lastRankingScore = run.lastRankingScore,
                lastDailyRank = run.lastDailyRank,
                finalRank = run.finalRank,
                lastRankingFirstPay = run.lastRankingFirstPay,
                rankingAppliedThisDay = run.rankingAppliedThisDay,
                concertBooked = run.concertBooked,
                concertPending = run.concertPending,
                concertPlayed = run.concertPlayed,
                lastStreamWasConcert = run.lastStreamWasConcert,
                lastConcertCost = run.lastConcertCost,
                lastConcertPayout = run.lastConcertPayout,
                lastConcertFailed = run.lastConcertFailed,
                lastConcertPerformanceSuccess = run.lastConcertPerformanceSuccess,
                concertResultApplied = run.concertResultApplied,
                zeroMentalDays = run.zeroMentalDays,
                zeroMentalCountedThisDay = run.zeroMentalCountedThisDay,
                lastEnding = (int)run.lastEnding,
                retirePicked = run.retirePicked,
                tier0 = run.tier0,
                tier1 = run.tier1,
                tier2 = run.tier2,
                tier3 = run.tier3,
                tier4 = run.tier4,
                loyalty = run.loyalty,
                minjunPresent = run.minjunPresent,
                minjunEver = run.minjunEver,
                minjunIgnoreSettlements = run.minjunIgnoreSettlements,
                minjunBonusPending = run.minjunBonusPending,
                lostSuperchatBonusDay = run.lostSuperchatBonusDay,
                haeunPresent = run.haeunPresent,
                haeunEver = run.haeunEver,
                haeunHurtThisDay = run.haeunHurtThisDay,
                fanLetterSentThisDay = run.fanLetterSentThisDay,
                lastFanLetter = run.lastFanLetter,
                lastFanSupport = run.lastFanSupport,
                lastAutoCost = run.lastAutoCost,
                lastConflictSurcharge = run.lastConflictSurcharge,
                pendingExtraSurcharge = run.pendingExtraSurcharge,
                conflictResolved = run.conflictResolved,
                conflictPending = run.conflictPending,
                autoReplyOn = run.autoReplyOn,
                autoReplyPrompted = run.autoReplyPrompted,
                lastMinjunLeft = run.lastMinjunLeft,
                lastHaeunLeft = run.lastHaeunLeft,
                lastHadSuccessfulSuperchat = run.lastHadSuccessfulSuperchat,
                lastMissStreak = run.lastMissStreak,
                contentPicked = (int)run.contentPicked,
                contentMentalAppliedThisDay = run.contentMentalAppliedThisDay
            };
            return data;
        }

        static void Apply(RunSaveData data, GameRunState run)
        {
            run.day = data.day;
            run.cash = data.cash;
            run.debt = data.debt;
            run.mental = data.mental;
            run.billsAppliedThisDay = data.billsAppliedThisDay;
            run.streamDoneThisDay = data.streamDoneThisDay;
            run.lastStreamIncome = data.lastStreamIncome;
            run.lastSuperchatIncome = data.lastSuperchatIncome;
            run.lastTickIncome = data.lastTickIncome;
            run.lastStreamForceEnded = data.lastStreamForceEnded;
            run.lastPerfects = data.lastPerfects;
            run.lastGreats = data.lastGreats;
            run.lastGoods = data.lastGoods;
            run.lastMisses = data.lastMisses;
            run.lastPeakCombo = data.lastPeakCombo;
            run.lastHadHype = data.lastHadHype;
            run.lastStreamEventHappened = data.lastStreamEventHappened;
            run.lastStreamEventName = data.lastStreamEventName ?? "";
            run.lastStreamEventSuccess = data.lastStreamEventSuccess;
            run.lastBills = data.lastBills;
            run.lastRepaid = data.lastRepaid;
            run.lastOutcome = (WeekOutcome)data.lastOutcome;
            run.runSeed = data.runSeed;
            run.membershipUnlocked = data.membershipUnlocked;
            run.membershipJustUnlocked = data.membershipJustUnlocked;
            run.membershipCount = data.membershipCount;
            run.viewerBonus = data.viewerBonus;
            run.peakViewersEver = data.peakViewersEver;
            run.successfulStreams = data.successfulStreams;
            run.membershipHypeGainedToday = data.membershipHypeGainedToday;
            run.lastMembershipFromHype = data.lastMembershipFromHype;
            run.lastMembershipFromMiss = data.lastMembershipFromMiss;
            run.lastMembershipPassive = data.lastMembershipPassive;
            run.membershipPassiveAppliedThisDay = data.membershipPassiveAppliedThisDay;
            run.week2EntryApplied = data.week2EntryApplied;
            run.clipAttemptedThisDay = data.clipAttemptedThisDay;
            run.lastClipAttempted = data.lastClipAttempted;
            run.lastClipSuccess = data.lastClipSuccess;
            run.lastClipCash = data.lastClipCash;
            run.rivalMatchHappened = data.rivalMatchHappened;
            run.lastRivalMatch = data.lastRivalMatch;
            run.lastRivalWon = data.lastRivalWon;
            run.lastRivalCash = data.lastRivalCash;
            run.goodsUnlocked = data.goodsUnlocked;
            run.goodsJustUnlocked = data.goodsJustUnlocked;
            run.goodsStock = data.goodsStock;
            run.goodsSoldAppliedThisDay = data.goodsSoldAppliedThisDay;
            run.lastGoodsSold = data.lastGoodsSold;
            run.lastGoodsRevenue = data.lastGoodsRevenue;
            run.lastGoodsPromoSuccess = data.lastGoodsPromoSuccess;
            run.lastStreamPeakViewers = data.lastStreamPeakViewers;
            run.agencyFounded = data.agencyFounded;
            run.agencyJustFounded = data.agencyJustFounded;
            run.juniorScouted = data.juniorScouted;
            run.sponsorEverSigned = data.sponsorEverSigned;
            run.sponsorActive = data.sponsorActive;
            run.sponsorDaysLeft = data.sponsorDaysLeft;
            run.juniorAppliedThisDay = data.juniorAppliedThisDay;
            run.sponsorDailyAppliedThisDay = data.sponsorDailyAppliedThisDay;
            run.sponsorLineAppliedThisDay = data.sponsorLineAppliedThisDay;
            run.lastAgencyFoundCost = data.lastAgencyFoundCost;
            run.lastJuniorScoutCost = data.lastJuniorScoutCost;
            run.lastJuniorPay = data.lastJuniorPay;
            run.lastJuniorTrainFail = data.lastJuniorTrainFail;
            run.lastSponsorDaily = data.lastSponsorDaily;
            run.lastSponsorLineBonus = data.lastSponsorLineBonus;
            run.lastSponsorLineSuccess = data.lastSponsorLineSuccess;
            run.lastSponsorBroke = data.lastSponsorBroke;
            run.playerRankingScore = data.playerRankingScore;
            run.npcRankingScore = Copy3(data.npcRankingScore);
            run.lastNpcScore = Copy3(data.lastNpcScore);
            run.lastRankingScore = data.lastRankingScore;
            run.lastDailyRank = data.lastDailyRank;
            run.finalRank = data.finalRank;
            run.lastRankingFirstPay = data.lastRankingFirstPay;
            run.rankingAppliedThisDay = data.rankingAppliedThisDay;
            run.concertBooked = data.concertBooked;
            run.concertPending = data.concertPending;
            run.concertPlayed = data.concertPlayed;
            run.lastStreamWasConcert = data.lastStreamWasConcert;
            run.lastConcertCost = data.lastConcertCost;
            run.lastConcertPayout = data.lastConcertPayout;
            run.lastConcertFailed = data.lastConcertFailed;
            run.lastConcertPerformanceSuccess = data.lastConcertPerformanceSuccess;
            run.concertResultApplied = data.concertResultApplied;
            run.zeroMentalDays = data.zeroMentalDays;
            run.zeroMentalCountedThisDay = data.zeroMentalCountedThisDay;
            run.lastEnding = (EndingKind)data.lastEnding;
            run.retirePicked = data.retirePicked;
            run.tier0 = data.tier0;
            run.tier1 = data.tier1;
            run.tier2 = data.tier2;
            run.tier3 = data.tier3;
            run.tier4 = data.tier4;
            run.loyalty = data.loyalty;
            run.minjunPresent = data.minjunPresent;
            run.minjunEver = data.minjunEver;
            run.minjunIgnoreSettlements = data.minjunIgnoreSettlements;
            run.minjunBonusPending = data.minjunBonusPending;
            run.lostSuperchatBonusDay = data.lostSuperchatBonusDay;
            run.haeunPresent = data.haeunPresent;
            run.haeunEver = data.haeunEver;
            run.haeunHurtThisDay = data.haeunHurtThisDay;
            run.fanLetterSentThisDay = data.fanLetterSentThisDay;
            run.lastFanLetter = data.lastFanLetter;
            run.lastFanSupport = data.lastFanSupport;
            run.lastAutoCost = data.lastAutoCost;
            run.lastConflictSurcharge = data.lastConflictSurcharge;
            run.pendingExtraSurcharge = data.pendingExtraSurcharge;
            run.conflictResolved = data.conflictResolved;
            run.conflictPending = data.conflictPending;
            run.autoReplyOn = data.autoReplyOn;
            run.autoReplyPrompted = data.autoReplyPrompted;
            run.lastMinjunLeft = data.lastMinjunLeft;
            run.lastHaeunLeft = data.lastHaeunLeft;
            run.lastHadSuccessfulSuperchat = data.lastHadSuccessfulSuperchat;
            run.lastMissStreak = data.lastMissStreak;
            run.contentPicked = (StreamContentType)data.contentPicked;
            run.contentMentalAppliedThisDay = data.contentMentalAppliedThisDay;
            ApplyRolls(data, run);
        }

        static ExtraRollSave[] CaptureRolls(GameRunState run)
        {
            int n = run.extraRolls != null ? run.extraRolls.Count : 0;
            var rolls = new ExtraRollSave[n];
            for (int i = 0; i < n; i++)
            {
                var r = run.extraRolls[i];
                rolls[i] = new ExtraRollSave
                {
                    id = r.Id ?? "",
                    displayName = r.DisplayName ?? "",
                    amount = r.Amount,
                    artPath = r.ArtPath ?? "",
                    tintHex = ColorUtility.ToHtmlStringRGB(r.Tint)
                };
            }
            return rolls;
        }

        static void ApplyRolls(RunSaveData data, GameRunState run)
        {
            var src = data.extraRolls;
            if (src == null || src.Length == 0)
            {
                if (data.extraThreatRolled)
                    run.ApplyExtraRolls(Array.Empty<ExtraThreatRoll>());
                else
                    run.ClearExtraThreat();
                run.extraThreatId = data.extraThreatId ?? "";
                run.extraThreatName = data.extraThreatName ?? "";
                run.extraThreatAmount = data.extraThreatAmount;
                run.extraThreatArt = data.extraThreatArt ?? "";
                return;
            }

            var rolls = new ExtraThreatRoll[src.Length];
            for (int i = 0; i < src.Length; i++)
            {
                var s = src[i] ?? new ExtraRollSave();
                string hex = s.tintHex;
                if (string.IsNullOrEmpty(hex))
                    hex = "FF6A6A";
                if (hex[0] != '#')
                    hex = "#" + hex;
                Color tint = ColorUtility.TryParseHtmlString(hex, out var c)
                    ? c
                    : new Color(1f, 0.42f, 0.42f);
                rolls[i] = new ExtraThreatRoll(
                    s.id ?? "",
                    s.displayName ?? "",
                    s.amount,
                    s.artPath ?? "",
                    tint);
            }
            run.ApplyExtraRolls(rolls);
        }

        static int[] Copy3(int[] src)
        {
            var dst = new int[3];
            if (src == null)
                return dst;
            int n = src.Length < 3 ? src.Length : 3;
            for (int i = 0; i < n; i++)
                dst[i] = src[i];
            return dst;
        }

        [Serializable]
        public class ExtraRollSave
        {
            public string id;
            public string displayName;
            public int amount;
            public string artPath;
            public string tintHex;
        }

        [Serializable]
        public class RunSaveData
        {
            public int version = Version;
            public int day;
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
            public int lastOutcome;
            public int runSeed;
            public bool extraThreatRolled;
            public string extraThreatId;
            public string extraThreatName;
            public int extraThreatAmount;
            public string extraThreatArt;
            public ExtraRollSave[] extraRolls;
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
            public bool agencyJustFounded;
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
            public int[] npcRankingScore;
            public int[] lastNpcScore;
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
            public int lastEnding;
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
            public bool autoReplyPrompted;
            public bool lastMinjunLeft;
            public bool lastHaeunLeft;
            public bool lastHadSuccessfulSuperchat;
            public int lastMissStreak;
            public int contentPicked;
            public bool contentMentalAppliedThisDay;
        }
    }
}
