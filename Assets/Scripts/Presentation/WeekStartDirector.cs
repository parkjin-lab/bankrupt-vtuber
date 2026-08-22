using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace BankruptVtuber
{
    public class WeekStartDirector : MonoBehaviour
    {
        Text _cash;
        Text _debt;
        Text _mental;
        Text _log;
        Text _day;
        RectTransform _stack;
        Button _goLive;
        bool _ready;

        struct Bill
        {
            public string Name;
            public string Art;
            public int Amount;
        }

        void Awake()
        {
            UiKit.EnsureCamera(Palette.Studio);
            UiKit.EnsureEventSystem();
            Build();
        }

        void Start()
        {
            var gm = GameManager.Instance;
            if (gm == null)
                return;
            RefreshHud();
            StartCoroutine(BillWave(gm));
        }

        void Update()
        {
            if (_ready && StreamBindings.Confirm)
                GameManager.Instance.GoLive();
        }

        void Build()
        {
            var canvas = UiKit.CreateCanvas("WeekStartCanvas", transform);
            var root = canvas.transform;

            UiKit.Image(root, "Wash", Palette.Studio);
            UiKit.Stretch(root.Find("Wash") as RectTransform);

            var title = UiKit.Label(root, "Title", "파산 버튜버", 56, Palette.Pastel, TextAnchor.UpperLeft, FontStyle.Bold);
            UiKit.Layout(title.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(48, -28), new Vector2(640, 70));
            _day = UiKit.Label(root, "DayLabel", "", 28, Palette.Pink, TextAnchor.UpperLeft);
            UiKit.Layout(_day.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(52, -96), new Vector2(720, 40));

            _cash = MoneyChip(root, "CashChip", "현금", Palette.CashGreen, new Vector2(-520, 250));
            _debt = MoneyChip(root, "DebtChip", "부채", Palette.MoneyRed, new Vector2(-180, 250));
            _mental = MoneyChip(root, "MentalChip", "멘탈", Palette.Pink, new Vector2(160, 250));

            var wavePanel = UiKit.Panel(root, "WavePanel", new Color(1, 1, 1, 0.06f));
            UiKit.Layout(wavePanel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -20), new Vector2(1100, 420));
            UiKit.Label(wavePanel, "WaveTitle", "오늘의 고정비 — 방어 웨이브", 26, Palette.Pastel, TextAnchor.UpperLeft, FontStyle.Bold);
            var wt = wavePanel.Find("WaveTitle") as RectTransform;
            UiKit.Layout(wt, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -12), new Vector2(0, 36));

            _stack = UiKit.Panel(wavePanel, "Stack", new Color(0, 0, 0, 0));
            UiKit.Layout(_stack, new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1000, 220));

            _log = UiKit.Label(wavePanel, "Log", "청구서가 몰려옵니다…", 24, Palette.PastelDim, TextAnchor.LowerLeft);
            UiKit.Layout(_log.rectTransform, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), new Vector2(0, 16), new Vector2(-40, 48));

            _goLive = UiKit.Button(root, "GoLive", "방송 켜기  (Space)", () => GameManager.Instance.GoLive(), Palette.PinkDeep, Color.white);
            UiKit.Layout(_goLive.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 36), new Vector2(360, 70));
            _goLive.gameObject.SetActive(false);

            var hint = UiKit.Label(root, "Hint", "A 긍정   S 공감   D 웃음   F 감사   Space 슈퍼챗(떼면 판정)", 18, Palette.Muted, TextAnchor.LowerRight);
            UiKit.Layout(hint.rectTransform, new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0), new Vector2(-24, 16), new Vector2(620, 28));
        }

        Text MoneyChip(Transform root, string name, string label, Color accent, Vector2 pos)
        {
            var panel = UiKit.Panel(root, name, new Color(0.08f, 0.05f, 0.1f, 0.86f));
            UiKit.Layout(panel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, new Vector2(300, 88));
            var bar = UiKit.Image(panel, "Accent", accent);
            UiKit.Layout(bar.rectTransform, new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0.5f), Vector2.zero, new Vector2(8, 0));
            UiKit.Label(panel, "L", label, 18, Palette.Muted, TextAnchor.UpperLeft);
            var l = panel.Find("L") as RectTransform;
            UiKit.Layout(l, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1), new Vector2(20, -8), new Vector2(-28, 24));
            var v = UiKit.Label(panel, "V", "₩0", 32, accent, TextAnchor.LowerLeft, FontStyle.Bold);
            UiKit.Layout(v.rectTransform, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 0), new Vector2(20, 10), new Vector2(-28, -36));
            return v;
        }

        void RefreshHud()
        {
            var run = GameManager.Instance.Run;
            _day.text = $"1주차  ·  {run.day}일차   /   5일 생존";
            _cash.text = EconomyRules.FormatWon(run.cash);
            _debt.text = EconomyRules.FormatWon(run.debt);
            _mental.text = $"{run.mental}/100";
        }

        IEnumerator BillWave(GameManager gm)
        {
            var b = gm.Balance;
            var bills = new[]
            {
                new Bill { Name = "월세", Art = ArtSprites.BillRent, Amount = b.billRent },
                new Bill { Name = "전기+인터넷", Art = ArtSprites.BillElectric, Amount = b.billElectricNet },
                new Bill { Name = "아바타 라이선스", Art = ArtSprites.BillLicense, Amount = b.billAvatarLicense },
                new Bill { Name = "식비", Art = ArtSprites.BillFood, Amount = b.billFood },
                new Bill { Name = "장비 할부", Art = ArtSprites.BillGear, Amount = b.billGear }
            };

            _log.text = "오늘의 고정비가 한꺼번에 들이닥칩니다.";
            yield return new WaitForSeconds(0.45f);

            for (int i = 0; i < bills.Length; i++)
            {
                SpawnIncoming(bills[i], i, bills.Length);
                yield return new WaitForSeconds(0.28f);
            }

            yield return new WaitForSeconds(0.35f);
            EconomyRules.ApplyDailyBills(gm.Run, b);
            RefreshHud();
            _cash.color = Palette.MoneyRed;
            _log.text = $"오늘 청구 {EconomyRules.FormatWon(b.TotalDailyBills)} 차감. 방송으로 메우세요.";
            yield return new WaitForSeconds(0.2f);
            _ready = true;
            _goLive.gameObject.SetActive(true);
        }

        void SpawnIncoming(Bill bill, int index, int total)
        {
            float x = (index - (total - 1) * 0.5f) * 190f;
            var card = UiKit.Panel(_stack, "Bill" + index, new Color(0.95f, 0.93f, 0.96f, 0.96f));
            UiKit.Layout(card, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(x, 220), new Vector2(176, 210));
            var icon = UiKit.Image(card, "Icon", Color.white);
            UiKit.Layout(icon.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -10), new Vector2(108, 108));
            ArtSprites.Apply(icon, bill.Art, Palette.PinkDeep);
            UiKit.Label(card, "N", bill.Name, 18, Palette.Ink, TextAnchor.MiddleCenter, FontStyle.Bold);
            var n = card.Find("N") as RectTransform;
            UiKit.Layout(n, new Vector2(0, 0.22f), new Vector2(1, 0.40f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            var amt = UiKit.Label(card, "A", "-" + EconomyRules.FormatWon(bill.Amount), 20, Palette.MoneyRed, TextAnchor.LowerCenter, FontStyle.Bold);
            UiKit.Layout(amt.rectTransform, new Vector2(0, 0), new Vector2(1, 0.24f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            StartCoroutine(Slam(card, x));
        }

        IEnumerator Slam(RectTransform card, float x)
        {
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * 2.6f;
                float e = 1f - Mathf.Pow(1f - Mathf.Clamp01(t), 3f);
                card.anchoredPosition = new Vector2(x, Mathf.Lerp(220f, 0f, e));
                card.localScale = Vector3.one * Mathf.Lerp(0.7f, 1f, e);
                yield return null;
            }
            card.anchoredPosition = new Vector2(x, 0);
        }
    }
}
