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

        void Awake()
        {
            UiKit.EnsureCamera(Palette.Studio);
            UiKit.EnsureEventSystem();
            Build();
        }

        void Start()
        {
            Render();
        }

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null)
                return;
            if (gm.Run.lastOutcome == WeekOutcome.Continue && StreamBindings.Confirm)
                gm.NextMorning();
        }

        void Build()
        {
            var canvas = UiKit.CreateCanvas("SettlementCanvas", transform);
            var root = canvas.transform;
            UiKit.Image(root, "Wash", Palette.Studio);
            UiKit.Stretch(root.Find("Wash") as RectTransform);

            var title = UiKit.Label(root, "Title", "정산", 54, Palette.Pastel, TextAnchor.UpperLeft, FontStyle.Bold);
            UiKit.Layout(title.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(48, -28), new Vector2(400, 70));

            var panel = UiKit.Panel(root, "Sheet", new Color(1, 1, 1, 0.07f));
            UiKit.Layout(panel, new Vector2(0.5f, 0.54f), new Vector2(0.5f, 0.54f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(920, 460));
            _body = UiKit.Label(panel, "Body", "", 26, Palette.Pastel, TextAnchor.UpperLeft);
            UiKit.Stretch(_body.rectTransform, 36, 36, 28, 28);
            _body.horizontalOverflow = HorizontalWrapMode.Wrap;
            _body.verticalOverflow = VerticalWrapMode.Overflow;
            _body.lineSpacing = 1.15f;

            _result = UiKit.Label(root, "Result", "", 32, Palette.Gold, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(_result.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 150), new Vector2(1200, 50));

            _repay = UiKit.Button(root, "Repay", "남은 현금으로 빚 갚기", OnRepay, Palette.Gold, Palette.Ink);
            UiKit.Layout(_repay.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-210, 70), new Vector2(360, 64));

            _next = UiKit.Button(root, "Next", "다음날  (Space)", () => GameManager.Instance.NextMorning(), Palette.PinkDeep, Color.white);
            UiKit.Layout(_next.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(210, 70), new Vector2(360, 64));

            _restart = UiKit.Button(root, "Restart", "처음부터", () => GameManager.Instance.RestartRun(), Palette.Troll, Color.white);
            UiKit.Layout(_restart.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 70), new Vector2(360, 64));
        }

        void Render()
        {
            var gm = GameManager.Instance;
            var run = gm.Run;
            var b = gm.Balance;

            string force = run.lastStreamForceEnded ? "멘탈 붕괴로 강제 종료 · 수입 50%\n" : "";
            _body.text =
                $"{run.day}일차 정산\n\n" +
                force +
                $"방송 수익(초당)     {EconomyRules.FormatWon(run.lastTickIncome)}\n" +
                $"슈퍼챗              {EconomyRules.FormatWon(run.lastSuperchatIncome)}\n" +
                $"실지급              {EconomyRules.FormatWon(run.lastStreamIncome)}\n" +
                $"오늘 고정비         -{EconomyRules.FormatWon(run.lastBills)}\n" +
                (run.extraThreatAmount > 0
                    ? $"위협 {run.extraThreatName,-10} -{EconomyRules.FormatWon(run.extraThreatAmount)}\n"
                    : "") +
                (run.lastRepaid > 0 ? $"부채 상환           -{EconomyRules.FormatWon(run.lastRepaid)}\n" : "") +
                $"\n판정  P {run.lastPerfects}  G {run.lastGreats}  Good {run.lastGoods}  Miss {run.lastMisses}" +
                (run.lastHadHype ? "   · 하이프 달성" : "") +
                $"\n\n현금 {EconomyRules.FormatWon(run.cash)}     부채 {EconomyRules.FormatWon(run.debt)}     멘탈 {run.mental}";

            run.lastOutcome = EconomyRules.Evaluate(run, b);
            switch (run.lastOutcome)
            {
                case WeekOutcome.Bankrupt:
                    _result.text = "파산. 부채가 ₩180,000을 넘었습니다.";
                    _result.color = Palette.MoneyRed;
                    _next.gameObject.SetActive(false);
                    _repay.gameObject.SetActive(false);
                    _restart.gameObject.SetActive(true);
                    break;
                case WeekOutcome.Win:
                    _result.text = "1주차 생존 성공. 아직 파산하지 않았습니다.";
                    _result.color = Palette.CashGreen;
                    _next.gameObject.SetActive(false);
                    _repay.gameObject.SetActive(run.cash > 0 && run.debt > 0);
                    _restart.gameObject.SetActive(true);
                    break;
                case WeekOutcome.WeekFailed:
                    _result.text = "5일은 버텼지만 목표 미달 (부채 3만 이하 또는 현금 7만). 남은 현금으로 빚을 갚을 수 있습니다.";
                    _result.color = Palette.Gold;
                    _next.gameObject.SetActive(false);
                    _repay.gameObject.SetActive(run.cash > 0 && run.debt > 0);
                    _restart.gameObject.SetActive(true);
                    break;
                default:
                    _result.text = $"{run.day}일차 종료. 남은 날 {b.daysInWeek - run.day}일.";
                    _result.color = Palette.Pastel;
                    _next.gameObject.SetActive(true);
                    _repay.gameObject.SetActive(run.cash > 0 && run.debt > 0);
                    _restart.gameObject.SetActive(false);
                    break;
            }
        }

        void OnRepay()
        {
            var run = GameManager.Instance.Run;
            EconomyRules.RepayDebt(run, run.cash);
            Render();
        }
    }
}
