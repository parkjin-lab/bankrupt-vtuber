using System;

namespace BankruptVtuber
{
    public static class Week2Rules
    {
        public static int MembersFromPerfects(int perfects, Week2Balance w2)
        {
            if (w2 == null)
                return 0;
            if (perfects >= w2.membersHighPerfects)
                return w2.membersHighGain;
            if (perfects >= w2.membersLowPerfects)
                return w2.membersLowGain;
            return 0;
        }

        public static void ApplyStreamMembership(GameRunState run, int perfects, bool pitchFired, bool pitchSuccess, Week2Balance w2)
        {
            if (run == null)
                return;
            run.lastMembershipFromPerfects = 0;
            run.lastMembershipFromPitch = 0;
            run.lastMembershipPitchHappened = pitchFired;
            run.lastMembershipPitchSuccess = pitchSuccess;
            if (!run.membershipUnlocked || w2 == null)
                return;

            int fromPerfects = MembersFromPerfects(perfects, w2);
            int fromPitch = pitchSuccess ? w2.pitchMemberBonus : 0;
            run.lastMembershipFromPerfects = fromPerfects;
            run.lastMembershipFromPitch = fromPitch;
            run.membershipCount += fromPerfects + fromPitch;
        }

        public static int ApplyMembershipPassive(GameRunState run, Week2Balance w2)
        {
            if (run == null || w2 == null || run.membershipPassiveAppliedThisDay)
                return 0;
            run.membershipPassiveAppliedThisDay = true;
            if (!run.membershipUnlocked)
            {
                run.lastMembershipPassive = 0;
                return 0;
            }

            int pay = run.membershipCount * w2.membershipPassivePerMember;
            run.lastMembershipPassive = pay;
            run.cash += pay;
            EconomyRules.ConvertNegativeCashToDebt(run);
            return pay;
        }

        public static bool CanOfferClip(GameRunState run)
        {
            if (run == null || !WeekSchedule.InWeek2(run) || run.clipAttemptedThisDay)
                return false;
            return run.lastHadHype || run.lastPerfects >= 8;
        }

        public static bool AttemptClip(GameRunState run, Week2Balance w2)
        {
            if (!CanOfferClip(run) || w2 == null)
                return false;

            run.clipAttemptedThisDay = true;
            int chance = run.lastHadHype ? w2.clipChanceWithHype : w2.clipChanceNoHype;
            var rng = new Random(ExtraThreatRules.MixSeed(run.runSeed, run.day + 4241));
            bool success = rng.Next(100) < chance;
            run.lastClipAttempted = true;
            run.lastClipSuccess = success;
            run.lastClipCash = 0;
            if (!success)
                return false;

            run.lastClipCash = w2.clipCash;
            run.cash += w2.clipCash;
            run.viewerBonus += w2.clipViewerBonus;
            WeekSchedule.UnlockMembership(run, w2);
            EconomyRules.ConvertNegativeCashToDebt(run);
            return true;
        }
    }
}
