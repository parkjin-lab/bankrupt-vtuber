using System;

namespace BankruptVtuber
{
    public readonly struct StreamHitResult
    {
        public readonly Judgement Judgement;
        public readonly float ViewerDelta;
        public readonly int MentalDelta;
        public readonly int ExtraViewerLoss;
        public readonly bool ResetCombo;
        public readonly bool StartedHype;
        public readonly bool TriggeredTotalMissPenalty;

        public StreamHitResult(
            Judgement judgement,
            float viewerDelta,
            int mentalDelta,
            int extraViewerLoss,
            bool resetCombo,
            bool startedHype,
            bool triggeredTotalMissPenalty)
        {
            Judgement = judgement;
            ViewerDelta = viewerDelta;
            MentalDelta = mentalDelta;
            ExtraViewerLoss = extraViewerLoss;
            ResetCombo = resetCombo;
            StartedHype = startedHype;
            TriggeredTotalMissPenalty = triggeredTotalMissPenalty;
        }
    }

    /// <summary>Pure Week 1 stream rules. Safe to call from a headless simulator.</summary>
    public static class StreamRules
    {
        public static Judgement Judge(float absDeltaSeconds, Week1Balance b) =>
            Judge(absDeltaSeconds, b, 1f);

        public static Judgement Judge(float absDeltaSeconds, Week1Balance b, float perfectWindowMul)
        {
            float perfect = b.perfectWindow * (perfectWindowMul > 0f ? perfectWindowMul : 1f);
            if (absDeltaSeconds <= perfect)
                return Judgement.Perfect;
            if (absDeltaSeconds <= b.greatWindow)
                return Judgement.Great;
            if (absDeltaSeconds <= b.goodWindow)
                return Judgement.Good;
            return Judgement.Miss;
        }

        public static float ViewerDeltaFor(Judgement j, Week1Balance b) => j switch
        {
            Judgement.Perfect => b.perfectViewerDelta,
            Judgement.Great => b.greatViewerDelta,
            Judgement.Good => b.goodViewerDelta,
            _ => b.missViewerDelta
        };

        public static float IncomeMultiplier(int perfectCombo, bool hypeActive, Week1Balance b)
        {
            if (hypeActive)
                return b.hypeIncomeMultiplier;
            if (perfectCombo >= b.comboIncomeThreshold)
                return b.comboIncomeMultiplier;
            return 1f;
        }

        public static int TickIncome(float viewers, float dt, float multiplier, Week1Balance b)
        {
            int perSec = (int)Math.Floor(viewers) * b.incomePerViewerPerSec;
            return (int)Math.Floor(perSec * multiplier * dt);
        }

        public static int SuperchatAmount(bool hypeActive, Random rng, Week1Balance b)
        {
            // Base roll 1000–6000; hype 2.0x → 2000–12000.
            int raw = rng.Next(b.superchatMinWon, b.superchatMaxWon + 1);
            if (hypeActive)
                raw = (int)Math.Floor(raw * b.hypeSuperchatMultiplier);
            return raw;
        }

        public static StreamHitResult ApplyJudgement(
            Judgement judgement,
            ref int perfectCombo,
            ref int missStreak,
            ref int totalMiss,
            ref bool totalMissPenaltyUsed,
            Week1Balance b)
        {
            float viewers = ViewerDeltaFor(judgement, b);
            int mental = 0;
            int extraViewerLoss = 0;
            bool reset = false;
            bool hype = false;
            bool totalPenalty = false;

            if (judgement == Judgement.Miss)
            {
                perfectCombo = 0;
                missStreak += 1;
                totalMiss += 1;
                reset = true;

                if (missStreak >= b.missStreakMental)
                {
                    mental -= b.missStreakMentalPenalty;
                    extraViewerLoss += b.missStreakViewerPenalty;
                    missStreak = 0;
                }

                if (!totalMissPenaltyUsed && totalMiss >= b.totalMissMentalTrigger)
                {
                    mental -= b.totalMissMentalPenalty;
                    totalMissPenaltyUsed = true;
                    totalPenalty = true;
                }
            }
            else
            {
                missStreak = 0;
                if (judgement == Judgement.Perfect)
                {
                    perfectCombo += 1;
                    if (perfectCombo == b.hypePerfectCombo)
                        hype = true;
                }
                else
                {
                    perfectCombo = 0;
                }
            }

            return new StreamHitResult(judgement, viewers, mental, extraViewerLoss, reset, hype, totalPenalty);
        }

        public static float ClampViewers(float viewers, Week1Balance b) =>
            viewers < b.minViewers ? b.minViewers : viewers;
    }
}
