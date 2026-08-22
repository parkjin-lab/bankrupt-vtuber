using System;

namespace BankruptVtuber
{
    public static class Week2Rules
    {
        public static void ApplyWeek2Entry(GameRunState run, Week2Balance w2)
        {
            if (run == null || w2 == null || run.week2EntryApplied)
                return;
            if (run.day < w2.firstDay)
                return;

            run.week2EntryApplied = true;
            run.cash += w2.entryCash;
            run.debt -= w2.entryDebtRelief;
            if (run.debt < 0)
                run.debt = 0;
            run.mental = w2.entryMental;
        }

        public static void AfterStream(GameRunState run, float peakViewers, bool forceEnded, bool hadHype, int misses, Week2Balance w2)
        {
            if (run == null)
                return;

            if (peakViewers > run.peakViewersEver)
                run.peakViewersEver = peakViewers;
            if (!forceEnded)
                run.successfulStreams += 1;

            WeekSchedule.TryUnlockMembership(run, w2);

            run.lastMembershipFromHype = 0;
            run.lastMembershipFromMiss = 0;
            if (!run.membershipUnlocked || w2 == null)
                return;

            if (hadHype && run.membershipHypeGainedToday < w2.membersFromHypeDayMax)
            {
                int gain = w2.membersFromHype;
                int room = w2.membersFromHypeDayMax - run.membershipHypeGainedToday;
                if (gain > room)
                    gain = room;
                if (gain > 0)
                {
                    run.membershipCount += gain;
                    run.membershipHypeGainedToday += gain;
                    run.lastMembershipFromHype = gain;
                }
            }

            if (misses >= w2.membersMissPenaltyAt)
            {
                int loss = w2.membersMissLoss;
                if (loss > run.membershipCount)
                    loss = run.membershipCount;
                run.membershipCount -= loss;
                run.lastMembershipFromMiss = loss;
            }
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

        public static bool CanOfferClip(GameRunState run, Week2Balance w2)
        {
            if (run == null || w2 == null || !WeekSchedule.InWeek2(run) || run.clipAttemptedThisDay)
                return false;
            return run.lastHadHype && run.lastPerfects >= w2.clipPerfectsRequired;
        }

        public static bool AttemptClip(GameRunState run, Week2Balance w2)
        {
            if (!CanOfferClip(run, w2))
                return false;

            run.clipAttemptedThisDay = true;
            run.lastClipAttempted = true;
            var rng = new Random(ExtraThreatRules.MixSeed(run.runSeed, run.day + 4241));
            bool success = rng.Next(100) < w2.clipChance;
            run.lastClipSuccess = success;
            run.lastClipCash = 0;
            if (!success)
                return false;

            run.lastClipCash = w2.clipCash;
            run.cash += w2.clipCash;
            run.viewerBonus += w2.clipViewerBonus;
            EconomyRules.ConvertNegativeCashToDebt(run);
            return true;
        }

        public static void DeclineClip(GameRunState run)
        {
            if (run == null || run.clipAttemptedThisDay)
                return;
            run.clipAttemptedThisDay = true;
            run.lastClipAttempted = true;
            run.lastClipSuccess = false;
            run.lastClipCash = 0;
        }
    }
}
