using System.Collections.Generic;
using UnityEngine;

namespace BankruptVtuber
{
    /// <summary>
    /// One-line memory of today from existing run numbers. Presentation only.
    /// </summary>
    public static class DayHeadline
    {
        public static string Build(GameRunState run)
        {
            if (run == null)
                return "";
            var facts = new List<string>();
            var gm = GameManager.Instance;
            int charges = EconomyRules.TonightBills(run);
            int income = run.lastStreamIncome;
            int gap = charges - income;
            int cap = EconomyRules.BankruptDebt(
                run,
                gm != null ? gm.Balance : null,
                gm != null ? gm.Week2 : null,
                gm != null ? gm.Week3 : null,
                gm != null ? gm.Week4 : null,
                gm != null ? gm.Week5 : null);
            bool bankrupt = run.lastOutcome == WeekOutcome.Bankrupt || run.lastEnding == EndingKind.Bankrupt;
            int room = cap - run.debt;

            if (bankrupt)
                facts.Add("파산");
            else if (run.debt > 0 && room <= 20000)
                facts.Add("파산 직전");

            if (gap > 0)
                facts.Add("청구 미달 " + EconomyRules.FormatWon(gap));
            else
                facts.Add("청구 커버");

            if (run.lastRivalMatch)
                facts.Add(run.lastRivalWon ? "라이벌 승" : "라이벌 패");

            if (run.lastClipAttempted)
                facts.Add(run.lastClipSuccess ? "클립 성공" : "클립 없음");
            if (run.lastConcertFailed)
                facts.Add("콘서트 실패");
            else if (run.lastConcertPerformanceSuccess && run.lastConcertPayout > 0)
                facts.Add("퍼포먼스 1.3x");
            if (run.lastGoodsPromoSuccess)
                facts.Add("홍보 1.5x");
            if (run.lastSponsorBroke)
                facts.Add("계약 파기");
            else if (run.lastSponsorLineSuccess)
                facts.Add("스폰서 멘트");

            if (run.lastFanLetter)
            {
                if (run.minjunPresent)
                    facts.Add("민준 답장");
                else if (run.haeunPresent)
                    facts.Add("하은 답장");
                else
                    facts.Add("팬레터 답장");
            }

            if (run.lastHadHype)
                facts.Add("하이프");
            else if (run.lastMisses >= 8 || run.lastStreamForceEnded)
                facts.Add("하이프 실패");

            if (run.lastMisses >= 10)
                facts.Add("Miss " + run.lastMisses);

            int peak = Mathf.RoundToInt(run.lastStreamPeakViewers);
            if (peak > 0)
                facts.Add("시청 " + peak);

            if (facts.Count == 0)
                return "오늘 방송";
            int take = facts.Count < 3 ? facts.Count : 3;
            return string.Join(" · ", facts.GetRange(0, take));
        }
    }
}
