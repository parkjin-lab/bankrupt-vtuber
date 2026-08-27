#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BankruptVtuber
{
    /// <summary>
    /// Editor / DEVELOPMENT playtest skip. Not a production cheat.
    /// Shows a small DEBUG badge whenever it is armed.
    /// </summary>
    public class PlaytestDebug : MonoBehaviour
    {
        /// <summary>
        /// Midpoint of the documented Week 1 average take band 24000-32000.
        /// Not AFK, not a hype-exploit skilled take.
        /// </summary>
        public const int AverageStreamTake = 28000;

        const int AverageTick = 22000;
        const int AverageSuperchat = 6000;
        const float AveragePeak = 22f;
        const int AveragePerfects = 12;
        const int AverageGreats = 16;
        const int AverageGoods = 10;
        const int AverageMisses = 5;

        public static bool Enabled
        {
            get
            {
#if UNITY_EDITOR
                return true;
#elif DEVELOPMENT_BUILD
                return true;
#else
                return false;
#endif
            }
        }

        public static void Attach(GameManager host)
        {
            if (host == null || !Enabled)
                return;
            if (host.GetComponent<PlaytestDebug>() == null)
                host.gameObject.AddComponent<PlaytestDebug>();
        }

        void Start()
        {
            if (Enabled)
                EnsureBadge();
        }

        void Update()
        {
            if (!Enabled)
                return;
            if (Input.GetKeyDown(KeyCode.F10))
                SkipRestOfDay();
            if (Input.GetKeyDown(KeyCode.F9))
                SkipToNextWeek();
        }

        public static void SkipRestOfDay()
        {
            if (!Enabled)
                return;
            var gm = GameManager.Instance;
            if (gm == null || gm.Run == null)
                return;

            if (FandomRules.MustResolveConflict(gm.Run))
            {
                Debug.Log("[파산 버튜버] DEBUG F10 ignored — 콘텐츠 편중 갈등 first.");
                return;
            }
            if (ContentRules.MustPick(gm.Run))
                ContentRules.Pick(gm.Run, StreamContentType.Talk);

            string scene = SceneManager.GetActiveScene().name;
            if (scene == SceneFlow.Settlement)
            {
                if (!CanAdvanceMorning(gm.Run))
                {
                    Debug.Log("[파산 버튜버] DEBUG F10 ignored — ending/bankrupt already up.");
                    return;
                }

                gm.NextMorning();
                Debug.Log("[파산 버튜버] DEBUG F10 next morning day=" + gm.Run.day);
                return;
            }

            if (!gm.Run.streamDoneThisDay)
                ApplyAverageStream(gm);
            DayHeadline.Remember(gm.Run);
            gm.MarkPrologueSeen();
            gm.GoSettlement();
            Debug.Log("[파산 버튜버] DEBUG F10 average stream ₩" + AverageStreamTake + " → settlement");
        }

        public static void SkipToNextWeek()
        {
            if (!Enabled)
                return;
            var gm = GameManager.Instance;
            if (gm == null || gm.Run == null)
                return;

            int current = WeekSchedule.WeekNumber(gm.Run);
            int next = current + 1;
            if (next > 5)
            {
                Debug.Log("[파산 버튜버] DEBUG F9 ignored — already Week 5.");
                return;
            }

            int targetDay = FirstDayOf(next, gm);
            bool alreadyPast = gm.Run.day >= targetDay;
            int cashKeep = gm.Run.cash;
            int debtKeep = gm.Run.debt;

            DayHeadline.Remember(gm.Run, false);
            GrantClearedUnlocks(gm, next);
            PrepareJumpedMorning(gm, targetDay);

            if (next == 2)
            {
                if (alreadyPast)
                {
                    gm.Run.cash = cashKeep;
                    gm.Run.debt = debtKeep;
                }
                else
                    Week2Rules.ApplyWeek2Entry(gm.Run, gm.Week2);
            }
            else if (alreadyPast)
            {
                gm.Run.cash = cashKeep;
                gm.Run.debt = debtKeep;
            }

            WeekSchedule.TryUnlockMembership(gm.Run, gm.Week2);
            Week3Rules.TryUnlockGoods(gm.Run, gm.Week3);
            gm.MarkPrologueSeen();
            gm.SaveRun();
            gm.GoWeekStart();
            Debug.Log("[파산 버튜버] DEBUG F9 → week " + next + " day " + gm.Run.day +
                      " cash=" + gm.Run.cash + " debt=" + gm.Run.debt);
        }

        static bool CanAdvanceMorning(GameRunState run)
        {
            if (FandomRules.MustResolveConflict(run))
                return false;
            if (run.lastOutcome == WeekOutcome.Bankrupt || run.lastOutcome == WeekOutcome.Ending)
                return false;
            if (WeekSchedule.InWeek5(run) && run.day >= WeekSchedule.Week5LastDay)
                return false;
            return true;
        }

        static int FirstDayOf(int week, GameManager gm)
        {
            if (week == 2 && gm.Week2 != null)
                return gm.Week2.firstDay;
            if (week == 3 && gm.Week3 != null)
                return gm.Week3.firstDay;
            if (week == 4 && gm.Week4 != null)
                return gm.Week4.firstDay;
            if (week == 5 && gm.Week5 != null)
                return gm.Week5.firstDay;
            return 1;
        }

        static void GrantClearedUnlocks(GameManager gm, int nextWeek)
        {
            var run = gm.Run;
            if (nextWeek >= 3)
                WeekSchedule.UnlockMembership(run, gm.Week2);
            if (nextWeek >= 4 && !run.goodsUnlocked)
            {
                run.goodsUnlocked = true;
                run.goodsStock = gm.Week3 != null ? gm.Week3.goodsUnlockStock : 20;
            }
            if (nextWeek >= 5)
                run.agencyFounded = true;
        }

        static void PrepareJumpedMorning(GameManager gm, int targetDay)
        {
            var run = gm.Run;
            var b = gm.Balance;
            run.day = targetDay;
            run.mental += b.mentalRestoreEachMorning;
            if (run.mental < 0)
                run.mental = 0;
            if (run.mental > b.maxMental)
                run.mental = b.maxMental;
            run.billsAppliedThisDay = false;
            run.streamDoneThisDay = false;
            run.lastStreamIncome = run.lastSuperchatIncome = run.lastTickIncome = 0;
            run.lastStreamForceEnded = false;
            run.lastPerfects = run.lastGreats = run.lastGoods = run.lastMisses = 0;
            run.lastPeakCombo = 0;
            run.lastHadHype = false;
            run.lastStreamEventHappened = false;
            run.lastStreamEventName = "";
            run.lastStreamEventSuccess = false;
            run.lastBills = 0;
            run.lastRepaid = 0;
            run.lastOutcome = WeekOutcome.Continue;
            run.lastMembershipFromHype = 0;
            run.lastMembershipFromMiss = 0;
            run.lastMembershipPassive = 0;
            run.membershipPassiveAppliedThisDay = false;
            run.membershipHypeGainedToday = 0;
            run.clipAttemptedThisDay = false;
            run.lastClipAttempted = false;
            run.lastClipSuccess = false;
            run.lastClipCash = 0;
            run.lastRivalMatch = false;
            run.lastRivalWon = false;
            run.lastRivalCash = 0;
            run.goodsSoldAppliedThisDay = false;
            run.lastGoodsSold = 0;
            run.lastGoodsRevenue = 0;
            run.lastGoodsPromoSuccess = false;
            run.lastStreamPeakViewers = 0f;
            run.juniorAppliedThisDay = false;
            run.sponsorDailyAppliedThisDay = false;
            run.sponsorLineAppliedThisDay = false;
            run.lastAgencyFoundCost = 0;
            run.lastJuniorScoutCost = 0;
            run.lastJuniorPay = 0;
            run.lastJuniorTrainFail = false;
            run.lastSponsorDaily = 0;
            run.lastSponsorLineBonus = 0;
            run.lastSponsorLineSuccess = false;
            run.lastSponsorBroke = false;
            run.lastRankingScore = 0;
            run.lastDailyRank = 0;
            run.lastRankingFirstPay = 0;
            run.rankingAppliedThisDay = false;
            run.lastStreamWasConcert = false;
            run.lastConcertPayout = 0;
            run.lastConcertFailed = false;
            run.lastConcertPerformanceSuccess = false;
            run.concertResultApplied = false;
            run.zeroMentalCountedThisDay = false;
            run.retirePicked = false;
            run.ClearExtraThreat();
            FandomRules.ResetDaily(run);
            ContentRules.ResetDaily(run);
            FandomRules.OnMorning(run, b, gm.Fandom);
        }

        static void ApplyAverageStream(GameManager gm)
        {
            var run = gm.Run;
            ExtraThreatRules.EnsureRolled(run, gm.Balance, gm.Week2, gm.Week3, gm.Week4, gm.Week5);
            Week3Rules.TryUnlockGoods(run, gm.Week3);
            if (!run.billsAppliedThisDay)
            {
                EconomyRules.ApplyDailyBills(run, gm.Balance, gm.Week2, gm.Week3, gm.Week4, gm.Week5, gm.Fandom);
                gm.SaveRun();
            }

            bool concert = Week5Rules.ConcertStreamReady(run);
            if (concert)
                Week5Rules.MarkConcertStarted(run);

            EconomyRules.ApplyStreamPayout(run, AverageTick, AverageSuperchat, false, gm.Balance);
            run.lastPerfects = AveragePerfects;
            run.lastGreats = AverageGreats;
            run.lastGoods = AverageGoods;
            run.lastMisses = AverageMisses;
            run.lastPeakCombo = 4;
            run.lastHadHype = false;
            run.lastStreamForceEnded = false;
            run.lastStreamPeakViewers = AveragePeak;
            run.lastGoodsPromoSuccess = false;
            run.lastConcertPerformanceSuccess = concert;
            if (run.mental < 60)
                run.mental = 60;
            run.lastHadSuccessfulSuperchat = AverageSuperchat > 0;
            run.lastMissStreak = 0;
            Week2Rules.AfterStream(run, AveragePeak, false, false, AverageMisses, gm.Week2);
            FandomRules.AfterStream(run, gm.Balance, gm.Fandom);
            ContentRules.ApplyStartMental(run, gm.Content, gm.Balance);
            ContentRules.AfterStream(run, gm.Content, gm.Fandom);
        }

        void EnsureBadge()
        {
            if (transform.Find("DebugBadge") != null)
                return;
            var canvas = UiKit.CreateCanvas("DebugBadge", transform);
            canvas.sortingOrder = 200;
            var label = UiKit.Label(canvas.transform, "DEBUG", "DEBUG  F9 다음 주  F10 오늘 스킵", 16, Palette.Gold, TextAnchor.UpperRight, FontStyle.Bold);
            UiKit.Layout(label.rectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(-16, -10), new Vector2(420, 28));
        }
    }
}
#endif
