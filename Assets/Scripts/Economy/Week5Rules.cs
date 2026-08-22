using System;

namespace BankruptVtuber
{
    public static class Week5Rules
    {
        public static int AgencySurcharge(GameRunState run, Week5Balance w5)
        {
            if (run == null || w5 == null || !run.agencyFounded)
                return 0;
            return w5.agencyDailyCost;
        }

        public static bool RankingUnlocked(GameRunState run, Week5Balance w5)
        {
            if (run == null || w5 == null)
                return false;
            return run.day >= w5.rankingDay && run.peakViewersEver >= w5.rankingPeakViewers;
        }

        /// <summary>
        /// Daily score = (peakViewers * 3) + (members * 8) + (goodsSoldToday * 4) + (perfects * 2).
        /// </summary>
        public static int DailyScore(GameRunState run, Week5Balance w5)
        {
            if (run == null || w5 == null)
                return 0;
            float peak = run.lastStreamPeakViewers > 0f ? run.lastStreamPeakViewers : 0f;
            return (int)Math.Floor(peak) * w5.rankingPeakFactor
                + run.membershipCount * w5.rankingMembersFactor
                + run.lastGoodsSold * w5.rankingGoodsFactor
                + run.lastPerfects * w5.rankingPerfectsFactor;
        }

        public static int NpcDailyScore(GameRunState run, Week5Balance w5, int npcIndex)
        {
            if (run == null || w5 == null || npcIndex < 0 || npcIndex > 2)
                return 0;
            int basis = npcIndex == 0 ? w5.npcBase0 : npcIndex == 1 ? w5.npcBase1 : w5.npcBase2;
            var rng = new Random(ExtraThreatRules.MixSeed(run.runSeed, run.day * 31 + (npcIndex + 1) * 131 + 25000));
            int lo = Math.Min(w5.npcVarianceLo, w5.npcVarianceHi);
            int hi = Math.Max(w5.npcVarianceLo, w5.npcVarianceHi);
            int delta = rng.Next(lo, hi + 1);
            int score = basis + delta;
            return score < 80 ? 80 : score;
        }

        static void EnsureRankingArrays(GameRunState run)
        {
            if (run.npcRankingScore == null || run.npcRankingScore.Length < 3)
                run.npcRankingScore = new int[3];
            if (run.lastNpcScore == null || run.lastNpcScore.Length < 3)
                run.lastNpcScore = new int[3];
        }

        public static void ApplyRanking(GameRunState run, Week5Balance w5)
        {
            if (run == null || w5 == null || run.rankingAppliedThisDay)
                return;
            EnsureRankingArrays(run);
            run.rankingAppliedThisDay = true;
            run.lastRankingFirstPay = 0;
            run.lastRankingScore = 0;
            run.lastDailyRank = 0;
            if (!RankingUnlocked(run, w5))
                return;

            int player = DailyScore(run, w5);
            run.lastRankingScore = player;
            run.playerRankingScore += player;
            for (int i = 0; i < 3; i++)
            {
                int npc = NpcDailyScore(run, w5, i);
                run.lastNpcScore[i] = npc;
                run.npcRankingScore[i] += npc;
            }

            run.lastDailyRank = RankOf(player, run.lastNpcScore);
            run.finalRank = RankOf(run.playerRankingScore, run.npcRankingScore);
            if (run.lastDailyRank == 1)
            {
                run.lastRankingFirstPay = w5.rankingDailyFirstCash;
                run.cash += w5.rankingDailyFirstCash;
                EconomyRules.ConvertNegativeCashToDebt(run);
            }
        }

        public static int RankOf(int player, int[] rivals)
        {
            int rank = 1;
            if (rivals == null)
                return rank;
            for (int i = 0; i < rivals.Length; i++)
            {
                if (rivals[i] > player)
                    rank += 1;
            }

            return rank;
        }

        public static string RankingBoard(GameRunState run)
        {
            if (run == null)
                return "";
            EnsureRankingArrays(run);
            var names = new[] { "나", Week5Balance.NpcNames[0], Week5Balance.NpcNames[1], Week5Balance.NpcNames[2] };
            var scores = new[]
            {
                run.playerRankingScore,
                run.npcRankingScore[0],
                run.npcRankingScore[1],
                run.npcRankingScore[2]
            };
            var order = new[] { 0, 1, 2, 3 };
            for (int i = 0; i < 4; i++)
            {
                for (int j = i + 1; j < 4; j++)
                {
                    if (scores[order[j]] > scores[order[i]])
                    {
                        int tmp = order[i];
                        order[i] = order[j];
                        order[j] = tmp;
                    }
                }
            }

            string s = "";
            for (int r = 0; r < 4; r++)
            {
                int idx = order[r];
                s += $"{r + 1}위  {names[idx]}  {scores[idx]}\n";
            }

            return s.TrimEnd();
        }

        public static bool CanBookConcert(GameRunState run, Week5Balance w5)
        {
            if (run == null || w5 == null || run.concertBooked)
                return false;
            if (run.day < w5.concertUnlockDay)
                return false;
            return run.cash >= w5.concertUnlockCash && run.peakViewersEver >= w5.concertUnlockPeak;
        }

        public static bool BookConcert(GameRunState run, Week5Balance w5)
        {
            if (!CanBookConcert(run, w5))
                return false;
            run.cash -= w5.concertCost;
            run.concertBooked = true;
            run.concertPending = true;
            run.lastConcertCost = w5.concertCost;
            EconomyRules.ConvertNegativeCashToDebt(run);
            return true;
        }

        public static bool ConcertStreamReady(GameRunState run) =>
            run != null && run.concertPending && !run.concertPlayed;

        public static void MarkConcertStarted(GameRunState run)
        {
            if (run == null || !run.concertPending)
                return;
            run.concertPending = false;
            run.lastStreamWasConcert = true;
        }

        public static void ApplyConcertResult(GameRunState run, Week1Balance w1, Week5Balance w5)
        {
            if (run == null || w5 == null || run.concertResultApplied || !run.lastStreamWasConcert)
                return;
            run.concertResultApplied = true;
            run.concertPlayed = true;
            run.lastConcertPayout = 0;
            run.lastConcertFailed = false;

            bool failed = run.lastStreamForceEnded
                || run.lastMisses >= w5.concertFailMisses
                || (!run.lastConcertPerformanceSuccess && run.mental <= w5.concertLowMental);
            if (failed)
            {
                run.lastConcertFailed = true;
                run.mental -= w5.concertFailMental;
                if (run.mental < 0)
                    run.mental = 0;
                Week3Rules.ApplyViewerBonusFloor(run, w1, -w5.concertFailViewers);
                return;
            }

            int pay = w5.concertBasePayout + run.lastRankingScore + w5.concertRankBonus;
            if (run.lastConcertPerformanceSuccess)
                pay = (int)Math.Floor(pay * w5.concertSuccessMultiplier);
            run.lastConcertPayout = pay;
            run.cash += pay;
            EconomyRules.ConvertNegativeCashToDebt(run);
        }

        public static void NoteZeroMentalDay(GameRunState run)
        {
            if (run == null || run.mental > 0 || run.zeroMentalCountedThisDay)
                return;
            run.zeroMentalCountedThisDay = true;
            run.zeroMentalDays += 1;
        }

        public static EndingKind ResolveEnding(GameRunState run, Week5Balance w5, bool retirePicked)
        {
            if (run == null || w5 == null)
                return EndingKind.Nameless;
            // Priority: 파산 > 번아웃 > 솔로 전설 > 에이전시 제국 > 은퇴 프로듀서.
            if (run.debt >= w5.bankruptDebt)
                return EndingKind.Bankrupt;
            if (run.zeroMentalDays >= w5.burnoutZeroMentalDays)
                return EndingKind.Burnout;
            if (!run.agencyFounded && run.finalRank == 1 && run.debt <= 0 && run.mental >= w5.endingSoloMental)
                return EndingKind.SoloLegend;
            if (run.agencyFounded && run.juniorScouted && run.cash >= w5.endingEmpireCash)
                return EndingKind.AgencyEmpire;
            if (run.agencyFounded && retirePicked)
                return EndingKind.RetireProducer;
            return EndingKind.Nameless;
        }

        public static bool CanOfferRetire(GameRunState run, Week5Balance w5)
        {
            if (run == null || w5 == null || !run.agencyFounded)
                return false;
            return ResolveEnding(run, w5, false) == EndingKind.Nameless;
        }

        public static string EndingTitle(EndingKind kind) => kind switch
        {
            EndingKind.Bankrupt => "파산",
            EndingKind.Burnout => "번아웃",
            EndingKind.SoloLegend => "솔로 전설",
            EndingKind.AgencyEmpire => "에이전시 제국",
            EndingKind.RetireProducer => "은퇴 프로듀서",
            _ => "무명 생존"
        };

        public static string EndingBody(EndingKind kind) => kind switch
        {
            EndingKind.Bankrupt => "부채가 ₩350,000을 넘었습니다. 채널은 여기서 멈춥니다.",
            EndingKind.Burnout => "멘탈이 0이 된 날이 두 번. 더 이상 방송을 켤 수 없습니다.",
            EndingKind.SoloLegend => "에이전시 없이 랭킹 1위. 빚을 갚고 멘탈을 지킨 솔로 전설입니다.",
            EndingKind.AgencyEmpire => "에이전시와 주니어, 그리고 충분한 현금. 제국이 되었습니다.",
            EndingKind.RetireProducer => "메인 자리를 후배에게 넘겼습니다. 당신은 프로듀서로 남습니다.",
            _ => "파산도 전설도 아닌, 계속 활동하는 무명 생존. 내일의 방송은 아직 남아 있습니다."
        };
    }
}
