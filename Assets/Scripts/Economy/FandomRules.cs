using System;

namespace BankruptVtuber
{
    public static class FandomRules
    {
        public static void Reset(GameRunState run, Week1Balance w1, FandomBalance f)
        {
            if (run == null)
                return;
            int t0 = f != null ? f.startT0 : 12;
            if (w1 != null)
                t0 = (int)Math.Floor(w1.startingViewers);
            run.tier0 = t0;
            run.tier1 = 0;
            run.tier2 = 0;
            run.tier3 = 0;
            run.tier4 = 0;
            run.loyalty = f != null ? f.startLoyalty : 40;
            run.minjunPresent = false;
            run.minjunEver = false;
            run.minjunIgnoreSettlements = 0;
            run.minjunBonusPending = false;
            run.lostSuperchatBonusDay = false;
            run.haeunPresent = false;
            run.haeunEver = false;
            run.haeunHurtThisDay = false;
            run.fanLetterSentThisDay = false;
            run.lastFanLetter = false;
            run.lastFanSupport = 0;
            run.lastAutoCost = 0;
            run.lastConflictSurcharge = 0;
            run.pendingExtraSurcharge = 0;
            run.conflictResolved = false;
            run.conflictPending = false;
            run.autoReplyOn = false;
            run.autoReplyPrompted = false;
            run.lastMinjunLeft = false;
            run.lastHaeunLeft = false;
            run.lastHadSuccessfulSuperchat = false;
            run.lastMissStreak = 0;
        }

        public static void ResetDaily(GameRunState run)
        {
            if (run == null)
                return;
            run.fanLetterSentThisDay = false;
            run.lastFanLetter = false;
            run.lastFanSupport = 0;
            run.lastAutoCost = 0;
            run.lastConflictSurcharge = 0;
            run.haeunHurtThisDay = false;
            run.lastHadSuccessfulSuperchat = false;
            run.lastMissStreak = 0;
        }

        public static void ClampLoyalty(GameRunState run, FandomBalance f)
        {
            if (run == null)
                return;
            int max = f != null ? f.maxLoyalty : 100;
            if (run.loyalty < 0)
                run.loyalty = 0;
            if (run.loyalty > max)
                run.loyalty = max;
        }

        public static void RefreshT0(GameRunState run, Week1Balance w1)
        {
            if (run == null)
                return;
            float start = w1 != null ? w1.startingViewers : 12f;
            float live = start + run.viewerBonus;
            if (run.lastStreamPeakViewers > live)
                live = run.lastStreamPeakViewers;
            int floor = (int)Math.Floor(live);
            if (floor < 0)
                floor = 0;
            int promoted = run.tier1 + run.tier2 + run.tier3 + run.tier4;
            run.tier0 = floor - promoted;
            if (run.tier0 < 0)
                run.tier0 = 0;
        }

        public static void SyncT3(GameRunState run)
        {
            if (run == null || !run.membershipUnlocked)
                return;
            if (run.tier3 < run.membershipCount)
                run.tier3 = run.membershipCount;
            run.membershipCount = run.tier3;
        }

        public static void RecountT4(GameRunState run)
        {
            if (run == null)
                return;
            int n = 0;
            if (run.minjunPresent)
                n += 1;
            if (run.haeunPresent)
                n += 1;
            run.tier4 = n;
        }

        public static void AfterStream(GameRunState run, Week1Balance w1, FandomBalance f)
        {
            if (run == null || f == null)
                return;

            run.lastMinjunLeft = false;
            run.lastHaeunLeft = false;

            RefreshT0(run, w1);
            bool minjunAlready = run.minjunPresent;
            if (run.lastHadSuccessfulSuperchat)
                MaybeSpawnMinjun(run, f);

            if (run.lastPerfects >= f.perfectHigh)
            {
                Convert(run, fromT0: f.perfectHighT0toT1, t1toT2: f.perfectHighT1toT2);
                run.loyalty += f.perfectHighLoyalty;
            }
            else if (run.lastPerfects >= f.perfectMidLo && run.lastPerfects <= f.perfectMidHi)
            {
                Convert(run, fromT0: f.perfectMidT0toT1, t1toT2: 0);
                run.loyalty += f.perfectMidLoyalty;
            }

            if (run.lastMisses >= f.missCount)
            {
                run.loyalty -= f.missLoyalty;
                run.tier2 -= f.missT2Loss;
                if (run.tier2 < 0)
                    run.tier2 = 0;
            }

            if (run.lastMissStreak >= f.haeunHurtStreak && run.haeunPresent)
                run.haeunHurtThisDay = true;

            // Bonus is a later stream, not the superchat that first spawned him.
            if (minjunAlready && run.minjunPresent && run.minjunBonusPending && run.lastHadSuccessfulSuperchat)
                run.minjunBonusPending = false;

            ClampLoyalty(run, f);
            SyncT3(run);
            RecountT4(run);
        }

        static void Convert(GameRunState run, int fromT0, int t1toT2)
        {
            int move0 = Math.Min(fromT0, run.tier0);
            run.tier0 -= move0;
            run.tier1 += move0;
            int move1 = Math.Min(t1toT2, run.tier1);
            run.tier1 -= move1;
            run.tier2 += move1;
        }

        public static void MaybeSpawnMinjun(GameRunState run, FandomBalance f)
        {
            if (run == null || run.minjunEver)
                return;
            run.minjunEver = true;
            run.minjunPresent = true;
            run.minjunIgnoreSettlements = 0;
            run.minjunBonusPending = true;
            RecountT4(run);
        }

        public static void MaybeSpawnHaeun(GameRunState run, FandomBalance f)
        {
            if (run == null || f == null || run.haeunEver)
                return;
            if (run.day < f.haeunAppearDay)
                return;
            run.haeunEver = true;
            run.haeunPresent = true;
            RecountT4(run);
        }

        public static void OnMorning(GameRunState run, Week1Balance w1, FandomBalance f)
        {
            if (run == null)
                return;
            MaybeSpawnHaeun(run, f);
            run.conflictPending = f != null && run.day == f.conflictDay && !run.conflictResolved;
            RefreshT0(run, w1);
            RecountT4(run);
        }

        public static bool CanSendLetter(GameRunState run) =>
            run != null && !run.fanLetterSentThisDay;

        public static bool ShouldOfferLetter(GameRunState run) =>
            run != null
            && !run.fanLetterSentThisDay
            && (run.minjunPresent || run.haeunPresent);

        public static bool SendLetter(GameRunState run, Week1Balance w1, FandomBalance f)
        {
            if (!CanSendLetter(run) || f == null)
                return false;
            run.fanLetterSentThisDay = true;
            run.lastFanLetter = true;
            run.loyalty += f.letterLoyalty;
            ClampLoyalty(run, f);
            run.mental += f.letterMental;
            int max = w1 != null ? w1.maxMental : 100;
            if (run.mental > max)
                run.mental = max;
            run.minjunIgnoreSettlements = 0;
            run.haeunHurtThisDay = false;
            return true;
        }

        public static void ResolveEndOfDay(GameRunState run, FandomBalance f)
        {
            if (run == null || f == null)
                return;

            run.lastMinjunLeft = false;
            run.lastHaeunLeft = false;

            if (run.minjunPresent && !run.fanLetterSentThisDay)
            {
                run.minjunIgnoreSettlements += 1;
                if (run.minjunIgnoreSettlements >= f.minjunIgnoreSettlements)
                    LeaveMinjun(run, f);
            }

            if (run.haeunPresent && run.haeunHurtThisDay && !run.fanLetterSentThisDay)
                LeaveHaeun(run, f);

            if (run.autoReplyOn && !run.fanLetterSentThisDay)
            {
                run.loyalty -= f.autoLoyaltyDrain;
                ClampLoyalty(run, f);
            }
        }

        static void LeaveMinjun(GameRunState run, FandomBalance f)
        {
            if (!run.minjunPresent)
                return;
            run.minjunPresent = false;
            run.lastMinjunLeft = true;
            run.loyalty -= f.minjunLeaveLoyalty;
            if (run.minjunBonusPending)
            {
                run.lostSuperchatBonusDay = true;
                run.minjunBonusPending = false;
            }
            ClampLoyalty(run, f);
            RecountT4(run);
        }

        static void LeaveHaeun(GameRunState run, FandomBalance f)
        {
            if (!run.haeunPresent)
                return;
            run.haeunPresent = false;
            run.lastHaeunLeft = true;
            run.loyalty -= f.haeunLeaveLoyalty;
            ClampLoyalty(run, f);
            RecountT4(run);
        }

        public static bool MustResolveConflict(GameRunState run) =>
            run != null && run.conflictPending && !run.conflictResolved;

        public static bool SootheConflict(GameRunState run, FandomBalance f)
        {
            if (!MustResolveConflict(run) || f == null)
                return false;
            run.conflictResolved = true;
            run.conflictPending = false;
            run.mental -= f.conflictSootheMental;
            if (run.mental < 0)
                run.mental = 0;
            run.loyalty += f.conflictSootheLoyalty;
            ClampLoyalty(run, f);
            return true;
        }

        public static bool StyleConflict(GameRunState run, FandomBalance f)
        {
            if (!MustResolveConflict(run) || f == null)
                return false;
            run.conflictResolved = true;
            run.conflictPending = false;
            run.tier2 -= f.conflictStyleT2;
            if (run.tier2 < 0)
                run.tier2 = 0;
            run.loyalty -= f.conflictStyleLoyalty;
            ClampLoyalty(run, f);
            run.pendingExtraSurcharge += f.conflictExtraSurcharge;
            return true;
        }

        public static bool CanToggleAuto(GameRunState run) =>
            run != null && run.agencyFounded &&
            (WeekSchedule.InWeek4(run) || WeekSchedule.InWeek5(run));

        public static void SetAutoReply(GameRunState run, bool on)
        {
            if (!CanToggleAuto(run))
                return;
            run.autoReplyOn = on;
        }

        public static int ConsumeSurcharge(GameRunState run)
        {
            if (run == null)
                return 0;
            int n = run.pendingExtraSurcharge;
            run.pendingExtraSurcharge = 0;
            run.lastConflictSurcharge = n;
            return n;
        }

        public static int AutoCostToday(GameRunState run, FandomBalance f)
        {
            if (run == null || f == null || !run.autoReplyOn || !CanToggleAuto(run))
                return 0;
            return f.autoDailyCost;
        }

        public static int RollSupport(GameRunState run, FandomBalance f)
        {
            if (run == null || f == null || run.loyalty < f.supportLoyaltyMin)
                return 0;
            int chance = run.loyalty / 2;
            var rng = new Random(ExtraThreatRules.MixSeed(run.runSeed, run.day * 17 + 7100));
            if (rng.Next(100) >= chance)
                return 0;
            int pay = f.supportBase + run.tier3 * f.supportPerT3 + run.tier4 * f.supportPerT4;
            if (pay < f.supportMin)
                pay = f.supportMin;
            if (pay > f.supportMax)
                pay = f.supportMax;
            return pay;
        }

        public static string HudLine(GameRunState run)
        {
            if (run == null)
                return "";
            return $"충성 {run.loyalty}/100   시청자 {run.tier0}  팔로워 {run.tier1}  정기 {run.tier2}  코어 {run.tier3}  슈퍼팬 {run.tier4}";
        }

        public static string SuperfanLine(GameRunState run, FandomBalance f)
        {
            if (run == null)
                return "";
            string minjun = f != null ? f.minjunName : "민준";
            string haeun = f != null ? f.haeunName : "하은";
            if (run.minjunPresent && run.haeunPresent)
                return $"{minjun} (첫 도네)   ·   {haeun} (매일 오는 야간)";
            if (run.minjunPresent)
                return $"{minjun} (첫 도네)";
            if (run.haeunPresent)
                return $"{haeun} (매일 오는 야간)";
            return "";
        }
    }
}
