using UnityEngine;
using UnityEngine.UI;

namespace BankruptVtuber
{
    public class SettlementDirector : MonoBehaviour
    {
        Text _body;
        Text _result;
        Button _next;
        Button _repay;
        Button _restart;
        Button _clipYes;
        Button _clipNo;
        Text _clipNote;
        Button _produce;

        void Awake()
        {
            UiKit.EnsureCamera(Palette.Studio);
            UiKit.EnsureEventSystem();
            Build();
        }

        void Start()
        {
            var gm = GameManager.Instance;
            if (gm != null)
            {
                Week2Rules.ApplyMembershipPassive(gm.Run, gm.Week2);
                Week3Rules.TryUnlockGoods(gm.Run, gm.Week3);
                Week3Rules.ApplyGoodsSales(gm.Run, gm.Week3);
            }
            Render();
        }

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null)
                return;
            if (CanAdvance(gm.Run) && StreamBindings.Confirm)
                gm.NextMorning();
        }

        static bool CanAdvance(GameRunState run) =>
            run.lastOutcome == WeekOutcome.Continue ||
            WeekSchedule.CanEnterWeek2(run) ||
            WeekSchedule.CanEnterWeek3(run);

        void Build()
        {
            var canvas = UiKit.CreateCanvas("SettlementCanvas", transform);
            var root = canvas.transform;
            UiKit.Image(root, "Wash", Palette.Studio);
            UiKit.Stretch(root.Find("Wash") as RectTransform);

            var title = UiKit.Label(root, "Title", "정산", 54, Palette.Pastel, TextAnchor.UpperLeft, FontStyle.Bold);
            UiKit.Layout(title.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(48, -28), new Vector2(400, 70));

            var panel = UiKit.Panel(root, "Sheet", new Color(1, 1, 1, 0.07f));
            UiKit.Layout(panel, new Vector2(0.5f, 0.56f), new Vector2(0.5f, 0.56f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(920, 500));
            _body = UiKit.Label(panel, "Body", "", 24, Palette.Pastel, TextAnchor.UpperLeft);
            UiKit.Stretch(_body.rectTransform, 36, 36, 28, 28);
            _body.horizontalOverflow = HorizontalWrapMode.Wrap;
            _body.verticalOverflow = VerticalWrapMode.Overflow;
            _body.lineSpacing = 1.12f;

            _result = UiKit.Label(root, "Result", "", 30, Palette.Gold, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(_result.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 168), new Vector2(1200, 50));

            _clipYes = UiKit.Button(root, "ClipYes", "클립 업로드", OnClipYes, Palette.Gold, Palette.Ink);
            UiKit.Layout(_clipYes.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-190, 118), new Vector2(320, 56));
            _clipNo = UiKit.Button(root, "ClipNo", "올리지 않기", OnClipNo, Palette.StudioHi, Palette.Pastel);
            UiKit.Layout(_clipNo.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(190, 118), new Vector2(320, 56));
            _clipNote = UiKit.Label(root, "ClipNote", "", 18, Palette.Gold, TextAnchor.MiddleCenter);
            UiKit.Layout(_clipNote.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 118), new Vector2(720, 28));

            _produce = UiKit.Button(root, "Produce", "아크릴 1개 생산  ₩2,500", OnProduce, Palette.Gold, Palette.Ink);
            UiKit.Layout(_produce.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 118), new Vector2(360, 56));
            _produce.gameObject.SetActive(false);

            _repay = UiKit.Button(root, "Repay", "남은 현금으로 빚 갚기", OnRepay, Palette.Gold, Palette.Ink);
            UiKit.Layout(_repay.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-210, 52), new Vector2(360, 60));

            _next = UiKit.Button(root, "Next", "다음날  (Space)", () => GameManager.Instance.NextMorning(), Palette.PinkDeep, Color.white);
            UiKit.Layout(_next.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(210, 52), new Vector2(360, 60));

            _restart = UiKit.Button(root, "Restart", "처음부터", () => GameManager.Instance.RestartRun(), Palette.Troll, Color.white);
            UiKit.Layout(_restart.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 52), new Vector2(360, 60));
        }

        void Render()
        {
            var gm = GameManager.Instance;
            var run = gm.Run;
            var b = gm.Balance;
            var w2 = gm.Week2;
            var w3 = gm.Week3;

            string extras = "";
            if (run.extraRolls.Count > 0)
            {
                for (int i = 0; i < run.extraRolls.Count; i++)
                    extras += $"위협 {run.extraRolls[i].DisplayName,-10} -{EconomyRules.FormatWon(run.extraRolls[i].Amount)}\n";
            }
            else if (run.extraThreatAmount > 0)
                extras = $"위협 {run.extraThreatName,-10} -{EconomyRules.FormatWon(run.extraThreatAmount)}\n";

            string memberDelta = "";
            if (run.lastMembershipFromHype > 0)
                memberDelta += $"   (+{run.lastMembershipFromHype} 하이프)";
            if (run.lastMembershipFromMiss > 0)
                memberDelta += $"   (-{run.lastMembershipFromMiss} 미스)";

            string force = run.lastStreamForceEnded ? "멘탈 붕괴로 강제 종료 · 수입 50%\n" : "";
            string weekTag = WeekSchedule.WeekNumber(run) + "주차";
            string rivalLine = "";
            if (run.lastRivalMatch)
                rivalLine = run.lastRivalWon
                    ? $"라이벌전 승리       {EconomyRules.FormatWon(run.lastRivalCash)} · 시작 시청자 +6\n"
                    : "라이벌전 패배       시작 시청자 −5 · 멘탈 −12\n";
            string goodsLine = "";
            if (run.goodsUnlocked)
            {
                goodsLine =
                    (run.lastGoodsSold > 0
                        ? $"아크릴 판매         {run.lastGoodsSold}개  {EconomyRules.FormatWon(run.lastGoodsRevenue)}" +
                          (run.lastGoodsPromoSuccess ? "  · 홍보 1.5x\n" : "\n")
                        : "") +
                    $"아크릴 재고         {run.goodsStock}개\n";
            }
            _body.text =
                $"{weekTag}  {run.day}일차 정산\n\n" +
                force +
                $"방송 수익(초당)     {EconomyRules.FormatWon(run.lastTickIncome)}\n" +
                $"슈퍼챗              {EconomyRules.FormatWon(run.lastSuperchatIncome)}\n" +
                $"실지급              {EconomyRules.FormatWon(run.lastStreamIncome)}\n" +
                $"오늘 고정비         -{EconomyRules.FormatWon(run.lastBills)}\n" +
                extras +
                (run.lastMembershipPassive > 0
                    ? $"멤버십 수익         {EconomyRules.FormatWon(run.lastMembershipPassive)}\n"
                    : "") +
                (run.lastClipCash > 0
                    ? $"클립 성공           {EconomyRules.FormatWon(run.lastClipCash)}\n"
                    : "") +
                rivalLine +
                goodsLine +
                (run.lastRepaid > 0 ? $"부채 상환           -{EconomyRules.FormatWon(run.lastRepaid)}\n" : "") +
                $"\n판정  P {run.lastPerfects}  G {run.lastGreats}  Good {run.lastGoods}  Miss {run.lastMisses}" +
                (run.lastHadHype ? "   · 하이프 달성" : "") +
                (run.lastStreamEventHappened
                    ? $"\n이벤트 {run.lastStreamEventName}   {(run.lastStreamEventSuccess ? "성공" : "실패")}"
                    : "") +
                (run.membershipUnlocked
                    ? $"\n멤버십 {run.membershipCount}{memberDelta}"
                    : "") +
                $"\n\n현금 {EconomyRules.FormatWon(run.cash)}     부채 {EconomyRules.FormatWon(run.debt)}     멘탈 {run.mental}";

            run.lastOutcome = EconomyRules.Evaluate(run, b, w2, w3);
            bool offerClip = Week2Rules.CanOfferClip(run, w2);
            _clipYes.gameObject.SetActive(offerClip);
            _clipNo.gameObject.SetActive(offerClip);
            _produce.gameObject.SetActive(run.goodsUnlocked && !offerClip && run.cash >= (w3 != null ? w3.goodsProduceCost : 2500));
            if (run.lastClipAttempted)
                _clipNote.text = run.lastClipSuccess
                    ? "클립 성공 — ₩30,000 · 시작 시청자 +10"
                    : "클립 없음";
            else
                _clipNote.text = "";
            _clipNote.gameObject.SetActive(run.lastClipAttempted && !offerClip);

            switch (run.lastOutcome)
            {
                case WeekOutcome.Bankrupt:
                    _result.text = WeekSchedule.InWeek3(run)
                        ? "파산. 부채가 ₩260,000을 넘었습니다."
                        : WeekSchedule.InWeek2(run)
                        ? "파산. 부채가 ₩220,000을 넘었습니다."
                        : "파산. 부채가 ₩180,000을 넘었습니다.";
                    _result.color = Palette.MoneyRed;
                    _next.gameObject.SetActive(false);
                    _repay.gameObject.SetActive(false);
                    _restart.gameObject.SetActive(true);
                    break;
                case WeekOutcome.Win:
                    _result.text = "1주차 생존 성공. 2주차를 이어갈 수 있습니다.";
                    _result.color = Palette.CashGreen;
                    _next.GetComponentInChildren<Text>().text = "2주차 시작  (Space)";
                    _next.gameObject.SetActive(true);
                    _repay.gameObject.SetActive(run.cash > 0 && run.debt > 0);
                    _restart.gameObject.SetActive(true);
                    PlaceTripleButtons();
                    break;
                case WeekOutcome.Week2Win:
                    _result.text = "2주차 클리어. 빚 ≤ 2만 또는 현금 ≥ 11만, 그리고 멤버십 해금.";
                    _result.color = Palette.CashGreen;
                    _next.GetComponentInChildren<Text>().text = "3주차 시작  (Space)";
                    _next.gameObject.SetActive(true);
                    _repay.gameObject.SetActive(run.cash > 0 && run.debt > 0);
                    _restart.gameObject.SetActive(true);
                    PlaceTripleButtons();
                    break;
                case WeekOutcome.Week3Win:
                    _result.text = "3주차 클리어. 빚 ≤ 1.5만 또는 현금 ≥ 14만, 그리고 아크릴 스탠드 해금.";
                    _result.color = Palette.CashGreen;
                    _next.gameObject.SetActive(false);
                    _repay.gameObject.SetActive(run.cash > 0 && run.debt > 0);
                    _restart.gameObject.SetActive(true);
                    break;
                case WeekOutcome.WeekFailed:
                    _result.text = WeekSchedule.InWeek3(run)
                        ? "3주차 목표 미달 (부채 1.5만 이하 또는 현금 14만, 그리고 아크릴 스탠드 해금)."
                        : WeekSchedule.InWeek2(run)
                        ? "2주차 목표 미달 (부채 2만 이하 또는 현금 11만, 그리고 멤버십 해금)."
                        : "5일은 버텼지만 목표 미달 (부채 3만 이하 또는 현금 7만). 남은 현금으로 빚을 갚을 수 있습니다.";
                    _result.color = Palette.Gold;
                    _next.gameObject.SetActive(false);
                    _repay.gameObject.SetActive(run.cash > 0 && run.debt > 0);
                    _restart.gameObject.SetActive(true);
                    break;
                default:
                    _result.text = $"{run.day}일차 종료. 남은 날 {WeekSchedule.DaysLeftInWeek(run)}일.";
                    _result.color = Palette.Pastel;
                    _next.GetComponentInChildren<Text>().text = "다음날  (Space)";
                    _next.gameObject.SetActive(true);
                    _repay.gameObject.SetActive(run.cash > 0 && run.debt > 0);
                    _restart.gameObject.SetActive(false);
                    break;
            }
        }

        void PlaceTripleButtons()
        {
            var run = GameManager.Instance.Run;
            if (run.cash > 0 && run.debt > 0)
            {
                UiKit.Layout(_repay.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-360, 52), new Vector2(300, 60));
                UiKit.Layout(_next.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 52), new Vector2(300, 60));
                UiKit.Layout(_restart.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(360, 52), new Vector2(300, 60));
            }
            else
            {
                UiKit.Layout(_next.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-210, 52), new Vector2(360, 60));
                UiKit.Layout(_restart.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(210, 52), new Vector2(360, 60));
            }
        }

        void OnRepay()
        {
            var run = GameManager.Instance.Run;
            EconomyRules.RepayDebt(run, run.cash);
            Render();
        }

        void OnClipYes()
        {
            var gm = GameManager.Instance;
            Week2Rules.AttemptClip(gm.Run, gm.Week2);
            Render();
        }

        void OnClipNo()
        {
            Week2Rules.DeclineClip(GameManager.Instance.Run);
            Render();
        }

        void OnProduce()
        {
            var gm = GameManager.Instance;
            Week3Rules.ProduceGoods(gm.Run, gm.Week3);
            Render();
        }
    }
}
