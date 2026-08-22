using System;

namespace BankruptVtuber
{
    public static class Week3Rules
    {
        public static bool ShouldStartRival(GameRunState run, Week3Balance w3)
        {
            if (run == null || w3 == null || !WeekSchedule.InWeek3(run) || run.rivalMatchHappened)
                return false;
            return run.day == w3.rivalDay || run.peakViewersEver >= w3.rivalPeakViewers;
        }

        public static void MarkRivalStarted(GameRunState run)
        {
            if (run == null)
                return;
            run.rivalMatchHappened = true;
            run.lastRivalMatch = true;
        }

        public static void ApplyRivalResult(GameRunState run, Week1Balance w1, Week3Balance w3, float playerViewers, float rivalViewers, bool rivalActive)
        {
            if (run == null)
                return;
            run.lastRivalMatch = rivalActive;
            run.lastRivalWon = false;
            run.lastRivalCash = 0;
            if (!rivalActive || w3 == null)
                return;

            bool won = playerViewers > rivalViewers;
            run.lastRivalWon = won;
            if (won)
            {
                run.lastRivalCash = w3.rivalWinCash;
                run.cash += w3.rivalWinCash;
                run.viewerBonus += w3.rivalWinViewerBonus;
                EconomyRules.ConvertNegativeCashToDebt(run);
                return;
            }

            run.mental -= w3.rivalLoseMental;
            if (run.mental < 0)
                run.mental = 0;
            ApplyViewerBonusFloor(run, w1, -w3.rivalLoseViewerPenalty);
        }

        public static void ApplyViewerBonusFloor(GameRunState run, Week1Balance w1, int delta)
        {
            if (run == null)
                return;
            float start = w1 != null ? w1.startingViewers : 12f;
            float floor = Math.Max(1f, start);
            float next = start + run.viewerBonus + delta;
            if (next < floor)
                next = floor;
            run.viewerBonus = (int)Math.Round(next - start);
        }

        public static void TryUnlockGoods(GameRunState run, Week3Balance w3)
        {
            if (run == null || w3 == null || run.goodsUnlocked || !WeekSchedule.InWeek3(run))
                return;
            if (!run.membershipUnlocked || run.cash < w3.goodsUnlockCash)
                return;
            run.goodsUnlocked = true;
            run.goodsStock = w3.goodsUnlockStock;
        }

        public static bool ProduceGoods(GameRunState run, Week3Balance w3, int count = 1)
        {
            if (run == null || w3 == null || !run.goodsUnlocked || count <= 0)
                return false;
            int cost = w3.goodsProduceCost * count;
            if (run.cash < cost)
                return false;
            run.cash -= cost;
            run.goodsStock += count;
            return true;
        }

        public static int ApplyGoodsSales(GameRunState run, Week3Balance w3)
        {
            if (run == null || w3 == null || run.goodsSoldAppliedThisDay)
                return 0;
            run.goodsSoldAppliedThisDay = true;
            run.lastGoodsSold = 0;
            run.lastGoodsRevenue = 0;
            if (!run.goodsUnlocked || run.goodsStock <= 0)
                return 0;

            float peak = run.lastStreamPeakViewers > 0f ? run.lastStreamPeakViewers : run.peakViewersEver;
            int sold = (int)Math.Floor(run.membershipCount * w3.goodsSoldMembersFactor + peak * w3.goodsSoldPeakFactor);
            if (sold < w3.goodsSoldMin)
                sold = w3.goodsSoldMin;
            if (run.lastGoodsPromoSuccess)
                sold = (int)Math.Floor(sold * w3.goodsPromoMultiplier);
            if (sold > run.goodsStock)
                sold = run.goodsStock;

            int revenue = sold * w3.goodsPrice;
            run.goodsStock -= sold;
            run.lastGoodsSold = sold;
            run.lastGoodsRevenue = revenue;
            run.cash += revenue;
            EconomyRules.ConvertNegativeCashToDebt(run);
            return revenue;
        }
    }
}
