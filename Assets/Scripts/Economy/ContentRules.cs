using System;

namespace BankruptVtuber
{
    public readonly struct ContentTuning
    {
        public readonly StreamContentType Type;
        public readonly string Name;
        public readonly float IncomeMul;
        public readonly int MentalCost;
        public readonly int PositiveWeight;
        public readonly int EmpathyWeight;
        public readonly int LaughWeight;
        public readonly float ChatSpawnMul;
        public readonly float PerfectWindowMul;
        public readonly float PerfectViewerMul;
        public readonly float MissViewerMul;
        public readonly float SuperchatIntervalMul;
        public readonly int ExtraSuperchat;
        public readonly int ExtraT0toT1;
        public readonly int ExtraT1toT2;
        public readonly int FinishLoyalty;
        public readonly int FinishLoyaltyMissMax;

        public ContentTuning(
            StreamContentType type,
            string name,
            float incomeMul,
            int mentalCost,
            int positiveWeight,
            int empathyWeight,
            int laughWeight,
            float chatSpawnMul,
            float perfectWindowMul,
            float perfectViewerMul,
            float missViewerMul,
            float superchatIntervalMul,
            int extraSuperchat,
            int extraT0toT1,
            int extraT1toT2,
            int finishLoyalty,
            int finishLoyaltyMissMax)
        {
            Type = type;
            Name = name;
            IncomeMul = incomeMul;
            MentalCost = mentalCost;
            PositiveWeight = positiveWeight;
            EmpathyWeight = empathyWeight;
            LaughWeight = laughWeight;
            ChatSpawnMul = chatSpawnMul;
            PerfectWindowMul = perfectWindowMul;
            PerfectViewerMul = perfectViewerMul;
            MissViewerMul = missViewerMul;
            SuperchatIntervalMul = superchatIntervalMul;
            ExtraSuperchat = extraSuperchat;
            ExtraT0toT1 = extraT0toT1;
            ExtraT1toT2 = extraT1toT2;
            FinishLoyalty = finishLoyalty;
            FinishLoyaltyMissMax = finishLoyaltyMissMax;
        }
    }

    public static class ContentRules
    {
        public static bool HasPick(GameRunState run) =>
            run != null && run.contentPicked != StreamContentType.None;

        public static bool MustPick(GameRunState run) => !HasPick(run);

        public static bool Pick(GameRunState run, StreamContentType type)
        {
            if (run == null || type == StreamContentType.None)
                return false;
            run.contentPicked = type;
            return true;
        }

        public static void ResetDaily(GameRunState run)
        {
            if (run == null)
                return;
            run.contentPicked = StreamContentType.None;
            run.contentMentalAppliedThisDay = false;
        }

        public static ContentTuning Tuning(ContentBalance b, StreamContentType type)
        {
            if (b == null)
                b = ContentBalance.Load();
            switch (type)
            {
                case StreamContentType.Talk:
                    return new ContentTuning(type, b.talkName, b.talkIncomeMultiplier, b.talkMentalCost,
                        b.talkPositiveWeight, b.talkEmpathyWeight, b.talkLaughWeight,
                        1f, 1f, 1f, 1f, 1f, 0, b.talkT0toT1, b.talkT1toT2, 0, 0);
                case StreamContentType.Game:
                    return new ContentTuning(type, b.gameName, b.gameIncomeMultiplier, b.gameMentalCost,
                        b.gamePositiveWeight, b.gameEmpathyWeight, b.gameLaughWeight,
                        1f, 1f, b.gamePerfectViewerMul, b.gameMissViewerMul, 1f, 0, 0, 0, 0, 0);
                case StreamContentType.Song:
                    return new ContentTuning(type, b.songName, b.songIncomeMultiplier, b.songMentalCost,
                        b.songPositiveWeight, b.songEmpathyWeight, b.songLaughWeight,
                        1f, b.songPerfectWindowMul, 1f, 1f, b.songSuperchatIntervalMul,
                        b.songExtraSuperchat, 0, 0, 0, 0);
                case StreamContentType.Reaction:
                    return new ContentTuning(type, b.reactionName, b.reactionIncomeMultiplier, b.reactionMentalCost,
                        b.reactionPositiveWeight, b.reactionEmpathyWeight, b.reactionLaughWeight,
                        b.reactionChatSpawnMul, 1f, 1f, 1f, 1f, 0, 0, 0,
                        b.reactionLoyalty, b.reactionMissMax);
                default:
                    return new ContentTuning(StreamContentType.None, "", 1f, 0,
                        50, 28, 22, 1f, 1f, 1f, 1f, 1f, 0, 0, 0, 0, 0);
            }
        }

        public static string DisplayName(ContentBalance b, StreamContentType type) =>
            Tuning(b, type).Name;

        public static string HudLine(ContentBalance b, GameRunState run)
        {
            if (!HasPick(run))
                return "";
            var t = Tuning(b, run.contentPicked);
            return $"콘텐츠 {t.Name}   ·   수입 ×{t.IncomeMul:0.##}   ·   멘탈 −{t.MentalCost}";
        }

        public static void ApplyStartMental(GameRunState run, ContentBalance b, Week1Balance w1)
        {
            if (run == null || run.contentMentalAppliedThisDay || !HasPick(run))
                return;
            var t = Tuning(b, run.contentPicked);
            run.mental -= t.MentalCost;
            if (run.mental < 0)
                run.mental = 0;
            int max = w1 != null ? w1.maxMental : 100;
            if (run.mental > max)
                run.mental = max;
            run.contentMentalAppliedThisDay = true;
        }

        public static void AfterStream(GameRunState run, ContentBalance b, FandomBalance f)
        {
            if (run == null || !HasPick(run))
                return;
            var t = Tuning(b, run.contentPicked);
            if (t.ExtraT0toT1 > 0 || t.ExtraT1toT2 > 0)
            {
                int move0 = Math.Min(t.ExtraT0toT1, run.tier0);
                run.tier0 -= move0;
                run.tier1 += move0;
                int move1 = Math.Min(t.ExtraT1toT2, run.tier1);
                run.tier1 -= move1;
                run.tier2 += move1;
            }

            if (t.FinishLoyalty > 0 && run.lastMisses < t.FinishLoyaltyMissMax)
            {
                run.loyalty += t.FinishLoyalty;
                FandomRules.ClampLoyalty(run, f);
            }
        }

        public static ChatKind RollRegularKind(ContentTuning t, Random rng)
        {
            int pos = Math.Max(0, t.PositiveWeight);
            int emp = Math.Max(0, t.EmpathyWeight);
            int laugh = Math.Max(0, t.LaughWeight);
            int total = pos + emp + laugh;
            if (total <= 0)
                return ChatKind.Positive;
            int roll = rng.Next(total);
            if (roll < pos)
                return ChatKind.Positive;
            if (roll < pos + emp)
                return ChatKind.Empathy;
            return ChatKind.Laugh;
        }
    }
}
