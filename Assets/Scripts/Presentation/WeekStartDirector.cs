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
        Text _fandom;
        Text _superfans;
        RectTransform _stack;
        RectTransform _conflictRoot;
        RectTransform _contentRoot;
        Text _contentHud;
        Button _goLive;
        StudioPortrait _portrait;
        RectTransform _fanMinjun;
        RectTransform _fanHaeun;
        bool _ready;
        float _cashSlam;

        struct Bill
        {
            public string Id;
            public string Name;
            public string Art;
            public int Amount;
            public bool Extra;
            public bool Gain;
            public bool Threat;
            public Color Tint;
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
            _portrait?.Tick(Time.deltaTime);
            if (_cashSlam > 0f)
            {
                _cashSlam = Mathf.MoveTowards(_cashSlam, 0f, Time.deltaTime);
                float u = _cashSlam;
                _cash.rectTransform.localScale = Vector3.one * (1f + 0.18f * u);
                _debt.rectTransform.localScale = Vector3.one * (1f + 0.22f * u);
            }

            if (_ready
                && !FandomRules.MustResolveConflict(GameManager.Instance.Run)
                && !ContentRules.MustPick(GameManager.Instance.Run)
                && StreamBindings.Confirm)
                GameManager.Instance.GoLive();
        }

        void Build()
        {
            var canvas = UiKit.CreateCanvas("WeekStartCanvas", transform);
            var root = canvas.transform;
            StudioChrome.Wash(root);
            _portrait = new StudioPortrait(root, new Vector2(0.90f, 0.82f), new Vector2(220, 280), true);

            var title = UiKit.Label(root, "Title", "파산 버튜버", 48, Palette.Pastel, TextAnchor.UpperLeft, FontStyle.Bold);
            UiKit.Layout(title.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(36, -16), new Vector2(640, 54));
            _day = UiKit.Label(root, "DayLabel", "", 24, Palette.Pink, TextAnchor.UpperLeft);
            UiKit.Layout(_day.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(40, -68), new Vector2(720, 32));
            _fandom = UiKit.Label(root, "FandomHud", "", 18, Palette.Pastel, TextAnchor.UpperLeft);
            UiKit.Layout(_fandom.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(40, -98), new Vector2(900, 24));
            _superfans = UiKit.Label(root, "SuperfanHud", "", 16, Palette.Gold, TextAnchor.UpperLeft);
            UiKit.Layout(_superfans.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(40, -122), new Vector2(720, 22));
            _contentHud = UiKit.Label(root, "ContentHud", "", 16, Palette.Gold, TextAnchor.UpperLeft);
            UiKit.Layout(_contentHud.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(40, -144), new Vector2(720, 22));

            _fanMinjun = StudioChrome.FanChip(root, "FanMinjun", "민준", "첫 도네", 40);
            UiKit.Layout(_fanMinjun, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(40, -168), new Vector2(220, 48));
            _fanHaeun = StudioChrome.FanChip(root, "FanHaeun", "하은", "매일 오는 야간", 270);
            UiKit.Layout(_fanHaeun, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(272, -168), new Vector2(240, 48));
            _fanMinjun.gameObject.SetActive(false);
            _fanHaeun.gameObject.SetActive(false);

            var moneyBar = UiKit.Panel(root, "MoneyBar", new Color(0, 0, 0, 0));
            UiKit.Layout(moneyBar, new Vector2(0, 1), new Vector2(0.72f, 1), new Vector2(0, 1), new Vector2(24, -220), new Vector2(0, 88));
            _cash = MoneyChip(moneyBar, "CashChip", "현금", Palette.CashGreen, 0f, 0.33f);
            _debt = MoneyChip(moneyBar, "DebtChip", "부채", Palette.MoneyRed, 0.33f, 0.66f);
            _mental = MoneyChip(moneyBar, "MentalChip", "멘탈", Palette.Pink, 0.66f, 1f);

            var wavePanel = UiKit.Panel(root, "WavePanel", new Color(1, 1, 1, 0.06f));
            UiKit.Layout(wavePanel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -20), new Vector2(1320, 480));
            UiKit.Label(wavePanel, "WaveTitle", "오늘의 고정비 + 위협 — 방어 웨이브", 26, Palette.Pastel, TextAnchor.UpperLeft, FontStyle.Bold);
            var wt = wavePanel.Find("WaveTitle") as RectTransform;
            UiKit.Layout(wt, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -12), new Vector2(0, 36));

            _stack = UiKit.Panel(wavePanel, "Stack", new Color(0, 0, 0, 0));
            UiKit.Layout(_stack, new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1260, 268));

            _log = UiKit.Label(wavePanel, "Log", "청구서가 몰려옵니다…", 24, Palette.PastelDim, TextAnchor.LowerLeft);
            UiKit.Layout(_log.rectTransform, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), new Vector2(0, 16), new Vector2(-40, 48));

            _goLive = UiKit.Button(root, "GoLive", "방송 켜기  (Space)", () => GameManager.Instance.GoLive(), Palette.PinkDeep, Color.white);
            UiKit.Layout(_goLive.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 36), new Vector2(360, 70));
            _goLive.gameObject.SetActive(false);

            _conflictRoot = UiKit.Panel(root, "ConflictCard", new Color(0.16f, 0.07f, 0.12f, 0.97f));
            UiKit.Layout(_conflictRoot, new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(720, 280));
            var cTitle = UiKit.Label(_conflictRoot, "CTitle", "콘텐츠 편중 갈등", 34, Palette.Gold, TextAnchor.UpperCenter, FontStyle.Bold);
            UiKit.Layout(cTitle.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -20), new Vector2(-32, 48));
            var cBody = UiKit.Label(_conflictRoot, "CBody", "오늘 안에 고르세요.", 22, Palette.Pastel, TextAnchor.MiddleCenter);
            UiKit.Layout(cBody.rectTransform, new Vector2(0, 0.42f), new Vector2(1, 0.72f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(-40, 0));
            var soothe = UiKit.Button(_conflictRoot, "Soothe", "특별방송으로 달래기", OnSootheConflict, Palette.PinkDeep, Color.white);
            UiKit.Layout(soothe.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-170, 28), new Vector2(300, 56));
            var style = UiKit.Button(_conflictRoot, "Style", "내 스타일대로", OnStyleConflict, Palette.Troll, Color.white);
            UiKit.Layout(style.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(170, 28), new Vector2(300, 56));
            _conflictRoot.gameObject.SetActive(false);

            _contentRoot = UiKit.Panel(root, "ContentPick", Color.white);
            UiKit.Layout(_contentRoot, new Vector2(0.5f, 0.18f), new Vector2(0.5f, 0.18f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1280, 280));
            ArtSprites.ApplySliced(_contentRoot.GetComponent<Image>(), ArtSprites.PanelDark, new Color(1f, 1f, 1f, 0.96f));
            var pTitle = UiKit.Label(_contentRoot, "PTitle", "오늘 콘텐츠", 28, Palette.Gold, TextAnchor.UpperCenter, FontStyle.Bold);
            UiKit.Layout(pTitle.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -10), new Vector2(-24, 34));
            var pHint = UiKit.Label(_contentRoot, "PHint", "방송 전에 반드시 고르세요. 채팅 QTE는 그대로입니다.", 16, Palette.PastelDim, TextAnchor.UpperCenter);
            UiKit.Layout(pHint.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -42), new Vector2(-24, 22));
            AddContentButton(StreamContentType.Talk, 0);
            AddContentButton(StreamContentType.Game, 1);
            AddContentButton(StreamContentType.Song, 2);
            AddContentButton(StreamContentType.Reaction, 3);
            _contentRoot.gameObject.SetActive(false);

            var hint = UiKit.Label(root, "Hint", "← 긍정   ↓ 공감   → 웃음   ↑ 감사   Space 슈퍼챗(떼면 판정)", 18, Palette.Muted, TextAnchor.LowerRight);
            UiKit.Layout(hint.rectTransform, new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0), new Vector2(-24, 16), new Vector2(620, 28));
        }

        Text MoneyChip(Transform root, string name, string label, Color accent, float x0, float x1)
        {
            var panel = UiKit.Panel(root, name, Color.white);
            panel.anchorMin = new Vector2(x0, 0f);
            panel.anchorMax = new Vector2(x1, 1f);
            panel.offsetMin = new Vector2(6f, 0f);
            panel.offsetMax = new Vector2(-6f, 0f);
            bool cash = name.Contains("Cash");
            bool debt = name.Contains("Debt");
            ArtSprites.ApplySliced(
                panel.GetComponent<Image>(),
                debt ? ArtSprites.ThreatBanner : cash ? ArtSprites.CashBanner : ArtSprites.PanelDark,
                debt ? Palette.MoneyRed : cash ? Palette.CashGreen : new Color(0.92f, 0.45f, 0.62f, 1f));
            UiKit.Label(panel, "L", label, 16, Color.white, TextAnchor.UpperLeft, FontStyle.Bold);
            var l = panel.Find("L") as RectTransform;
            UiKit.Layout(l, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1), new Vector2(16, -6), new Vector2(-20, 20));
            var v = UiKit.Label(panel, "V", "₩0", 28, Color.white, TextAnchor.LowerLeft, FontStyle.Bold);
            UiKit.Layout(v.rectTransform, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 0), new Vector2(16, 8), new Vector2(-20, -24));
            return v;
        }

        void RefreshHud()
        {
            var run = GameManager.Instance.Run;
            int week = WeekSchedule.WeekNumber(run);
            int last = WeekSchedule.LastDayOfCurrentWeek(run);
            string members = run.membershipUnlocked ? $"   ·   멤버십 {run.membershipCount}" : "";
            string goods = run.goodsUnlocked ? $"   ·   아크릴 {run.goodsStock}" : "";
            string agency = run.agencyFounded ? "   ·   에이전시" : "";
            string junior = run.juniorScouted ? "   ·   주니어" : "";
            string sponsor = run.sponsorActive ? $"   ·   스폰서 {run.sponsorDaysLeft}일" : "";
            string rank = run.finalRank > 0 ? $"   ·   랭킹 {run.finalRank}위" : "";
            string concert = run.concertPending ? "   ·   콘서트 대기" : "";
            _day.text = $"{week}주차  ·  {run.day}일차   /   {last}일{members}{goods}{agency}{junior}{sponsor}{rank}{concert}";
            _cash.text = EconomyRules.FormatWon(run.cash);
            _debt.text = EconomyRules.FormatWon(run.debt);
            _mental.text = $"{run.mental}/100";
            _fandom.text = FandomRules.HudLine(run);
            string fans = FandomRules.SuperfanLine(run, GameManager.Instance.Fandom);
            _superfans.text = fans;
            _superfans.gameObject.SetActive(!string.IsNullOrEmpty(fans));
            if (_fanMinjun != null)
                _fanMinjun.gameObject.SetActive(run.minjunPresent);
            if (_fanHaeun != null)
                _fanHaeun.gameObject.SetActive(run.haeunPresent);
            string content = ContentRules.HudLine(GameManager.Instance.Content, run);
            _contentHud.text = content;
            _contentHud.gameObject.SetActive(!string.IsNullOrEmpty(content));
        }

        static string ContentPickName(StreamContentType type) => type switch
        {
            StreamContentType.Talk => "토크",
            StreamContentType.Game => "게임",
            StreamContentType.Song => "노래",
            StreamContentType.Reaction => "리액션",
            _ => ""
        };

        void AddContentButton(StreamContentType type, int index)
        {
            var look = ContentShowLook.For(type);
            var t = ContentRules.Tuning(GameManager.Instance != null ? GameManager.Instance.Content : null, type);
            string name = ContentPickName(type);
            if (string.IsNullOrEmpty(name))
                name = t.Name;
            string caption = $"{name}\n수입 ×{t.IncomeMul:0.##}  멘탈 −{t.MentalCost}";
            var btn = UiKit.Button(_contentRoot, type.ToString(), caption, () => OnPickContent(type), look.Card, look.CardInk);
            float a = index / 4f;
            float b = (index + 1) / 4f;
            var rt = btn.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(a, 0f);
            rt.anchorMax = new Vector2(b, 0.70f);
            rt.offsetMin = new Vector2(10f, 16f);
            rt.offsetMax = new Vector2(-10f, -8f);
            var img = btn.GetComponent<Image>();
            ArtSprites.ApplySliced(img, ArtSprites.BubblePill, look.Card);
            img.raycastTarget = true;
            var wash = UiKit.Image(btn.transform, "ShowWash", look.Wash);
            UiKit.Layout(wash.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -6), new Vector2(-20, 10));
            var veil = UiKit.Image(btn.transform, "ShowVeil", look.WashVeil);
            UiKit.Layout(veil.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -6), new Vector2(-20, 10));
            AddCardChip(btn.transform, 0, look.Type == StreamContentType.Talk ? Palette.Blue : look.Type == StreamContentType.Game ? Palette.Troll : look.Type == StreamContentType.Song ? Palette.Gold : Palette.Muted);
            AddCardChip(btn.transform, 1, look.Type == StreamContentType.Talk ? Palette.Green : look.CamFrame);
            var cap = btn.GetComponentInChildren<Text>();
            if (cap != null)
            {
                cap.fontSize = 26;
                cap.lineSpacing = 1.15f;
                cap.color = look.CardInk;
                cap.rectTransform.offsetMin = new Vector2(8f, 8f);
                cap.rectTransform.offsetMax = new Vector2(-8f, -18f);
            }
        }

        static void AddCardChip(Transform parent, int index, Color color)
        {
            var chip = UiKit.Image(parent, "Chip" + index, color);
            UiKit.Layout(chip.rectTransform, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(14 + index * 22, 10), new Vector2(16, 16));
        }

        void OnPickContent(StreamContentType type)
        {
            var gm = GameManager.Instance;
            if (gm == null || !ContentRules.Pick(gm.Run, type))
                return;
            gm.SaveRun();
            _contentRoot.gameObject.SetActive(false);
            RefreshHud();
            _ready = true;
            if (Week5Rules.ConcertStreamReady(gm.Run))
                _goLive.GetComponentInChildren<Text>().text = "콘서트 방송  (Space)";
            _goLive.gameObject.SetActive(true);
        }

        void OnSootheConflict()
        {
            var gm = GameManager.Instance;
            FandomRules.SootheConflict(gm.Run, gm.Fandom);
            RefreshHud();
        }

        void OnStyleConflict()
        {
            var gm = GameManager.Instance;
            FandomRules.StyleConflict(gm.Run, gm.Fandom);
            RefreshHud();
        }

        IEnumerator BillWave(GameManager gm)
        {
            var b = gm.Balance;
            ExtraThreatRules.EnsureRolled(gm.Run, b, gm.Week2, gm.Week3, gm.Week4, gm.Week5);
            Week3Rules.TryUnlockGoods(gm.Run, gm.Week3);

            if (FandomRules.MustResolveConflict(gm.Run))
            {
                _conflictRoot.gameObject.SetActive(true);
                _goLive.gameObject.SetActive(false);
                _log.text = "콘텐츠 편중 갈등 — 오늘 안에 고르세요.";
                while (FandomRules.MustResolveConflict(gm.Run))
                    yield return null;
                _conflictRoot.gameObject.SetActive(false);
                RefreshHud();
            }

            var fixedBills = WeekSchedule.FixedBills(gm.Run, b, gm.Week2, gm.Week3, gm.Week4, gm.Week5);
            var bills = new System.Collections.Generic.List<Bill>
            {
                new Bill { Name = "월세", Art = ArtSprites.BillRent, Amount = fixedBills.Rent },
                new Bill { Name = "전기+인터넷", Art = ArtSprites.BillElectric, Amount = fixedBills.Electric },
                new Bill { Name = "아바타 라이선스", Art = ArtSprites.BillLicense, Amount = fixedBills.License },
                new Bill { Name = "식비", Art = ArtSprites.BillFood, Amount = fixedBills.Food },
                new Bill { Name = "장비 할부", Art = ArtSprites.BillGear, Amount = fixedBills.Gear }
            };
            int agencyCost = WeekSchedule.AgencyOps(gm.Run, gm.Week4, gm.Week5);
            if (agencyCost > 0)
            {
                bills.Add(new Bill
                {
                    Name = "에이전시 운영",
                    Art = ArtSprites.BillLicense,
                    Amount = agencyCost,
                    Extra = true,
                    Tint = Palette.Gold
                });
            }
            for (int e = 0; e < gm.Run.extraRolls.Count; e++)
            {
                var extra = gm.Run.extraRolls[e];
                bills.Add(new Bill
                {
                    Id = extra.Id,
                    Name = extra.DisplayName,
                    Art = extra.ArtPath,
                    Amount = extra.Amount,
                    Extra = true,
                    Threat = true,
                    Tint = extra.Tint
                });
            }
            if (gm.Run.pendingExtraSurcharge > 0)
            {
                bills.Add(new Bill
                {
                    Name = "갈등 할증",
                    Art = ArtSprites.BillLicense,
                    Amount = gm.Run.pendingExtraSurcharge,
                    Extra = true,
                    Threat = true,
                    Tint = Palette.Troll
                });
            }
            int autoCost = FandomRules.AutoCostToday(gm.Run, gm.Fandom);
            if (autoCost > 0)
            {
                bills.Add(new Bill
                {
                    Name = "자동응답",
                    Art = ArtSprites.BillElectric,
                    Amount = autoCost,
                    Extra = true,
                    Tint = Palette.Gold
                });
            }

            _log.text = gm.Run.extraRolls.Count == 0
                ? "오늘은 추가 위협이 없습니다."
                : $"오늘의 위협 — {gm.Run.extraThreatName}  {EconomyRules.FormatWon(gm.Run.extraThreatAmount)}";
            yield return new WaitForSeconds(0.45f);

            for (int i = 0; i < bills.Count; i++)
            {
                SpawnIncoming(bills[i], i, bills.Count);
                yield return new WaitForSeconds(0.28f);
            }

            yield return new WaitForSeconds(0.35f);
            EconomyRules.ApplyDailyBills(gm.Run, b, gm.Week2, gm.Week3, gm.Week4, gm.Week5, gm.Fandom);
            gm.SaveRun();
            RefreshHud();
            _cash.color = Color.white;
            _cashSlam = 1f;
            int today = gm.Run.lastBills + gm.Run.extraThreatAmount + gm.Run.lastConflictSurcharge + gm.Run.lastAutoCost;
            string support = gm.Run.lastFanSupport > 0
                ? $"   ·   팬 지원금 {EconomyRules.FormatWon(gm.Run.lastFanSupport)}"
                : "";
            string left = "";
            if (gm.Run.lastMinjunLeft)
                left += "   ·   민준이 떠났습니다.";
            if (gm.Run.lastHaeunLeft)
                left += "   ·   하은이 떠났습니다.";
            _log.text = gm.Run.extraRolls.Count == 0
                ? $"오늘 고정비 {EconomyRules.FormatWon(today)} 차감.{support}{left} 방송으로 메우세요."
                : $"{gm.Run.extraThreatName} 때문에 오늘 {EconomyRules.FormatWon(today)} 차감.{support}{left} 방송으로 메우세요.";
            if (gm.Run.lastFanSupport > 0)
            {
                SpawnIncoming(new Bill
                {
                    Name = "팬 지원금",
                    Art = ArtSprites.Superchat,
                    Amount = gm.Run.lastFanSupport,
                    Extra = true,
                    Gain = true,
                    Tint = Palette.CashGreen
                }, bills.Count, bills.Count + 1);
            }
            yield return new WaitForSeconds(0.2f);
            if (ContentRules.MustPick(gm.Run))
            {
                _ready = false;
                _goLive.gameObject.SetActive(false);
                _contentRoot.gameObject.SetActive(true);
                _log.text += "   ·   오늘 콘텐츠를 고르세요.";
            }
            else
            {
                _ready = true;
                if (Week5Rules.ConcertStreamReady(gm.Run))
                    _goLive.GetComponentInChildren<Text>().text = "콘서트 방송  (Space)";
                _goLive.gameObject.SetActive(true);
            }
        }

        void SpawnIncoming(Bill bill, int index, int total)
        {
            float step = total > 6 ? 164f : 192f;
            float x = (index - (total - 1) * 0.5f) * step;
            bool threat = bill.Threat;
            var bg = threat
                ? Palette.MoneyRed
                : bill.Gain
                    ? new Color(0.72f, 0.96f, 0.82f, 0.98f)
                    : bill.Extra
                        ? new Color(1f, 0.86f, 0.88f, 0.98f)
                        : new Color(0.95f, 0.93f, 0.96f, 0.96f);
            var card = UiKit.Panel(_stack, "Bill" + index, bg);
            UiKit.Layout(card, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(x, 280), new Vector2(176, 248));
            ExtraThreatLook threatLook = default;
            if (threat && !string.IsNullOrEmpty(bill.Id))
                threatLook = ExtraThreatLook.For(bill.Id, bill.Name, bill.Tint, bill.Art);
            if (threat)
                ArtSprites.ApplySliced(
                    card.GetComponent<Image>(),
                    ArtSprites.ThreatBanner,
                    threatLook.Fx != ExtraThreatFx.None ? threatLook.Tint : Palette.MoneyRed,
                    new Vector4(28f, 24f, 28f, 24f));
            else if (bill.Gain)
                ArtSprites.ApplySliced(card.GetComponent<Image>(), ArtSprites.CashBanner, Palette.CashGreen, new Vector4(28f, 24f, 28f, 24f));
            if (bill.Extra)
            {
                string tagText = bill.Gain ? "팬 지원" : "오늘의 위협";
                var tagCol = bill.Gain || threat ? Color.white : Palette.MoneyRed;
                var tag = UiKit.Label(card, "Tag", tagText, 13, tagCol, TextAnchor.UpperCenter, FontStyle.Bold);
                UiKit.Layout(tag.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -4), new Vector2(0, 18));
            }
            var icon = UiKit.Image(card, "Icon", Color.white);
            UiKit.Layout(icon.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, bill.Extra ? -28 : -12), new Vector2(128, 128));
            string iconArt = threatLook.Fx != ExtraThreatFx.None ? threatLook.Art : bill.Art;
            Color iconTint = threatLook.Fx != ExtraThreatFx.None ? threatLook.Tint : (bill.Extra ? bill.Tint : Palette.PinkDeep);
            ArtSprites.Apply(icon, iconArt, iconTint, bill.Extra || threatLook.Fx != ExtraThreatFx.None ? iconTint : (Color?)null);
            var nameCol = threat || bill.Gain ? Color.white : Palette.Ink;
            UiKit.Label(card, "N", bill.Name, 16, nameCol, TextAnchor.MiddleCenter, FontStyle.Bold);
            var n = card.Find("N") as RectTransform;
            UiKit.Layout(n, new Vector2(0, 0.20f), new Vector2(1, 0.38f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            var amtCol = bill.Gain ? Color.white : threat ? Palette.Gold : Palette.MoneyRed;
            var amt = UiKit.Label(card, "A", (bill.Gain ? "+" : "-") + EconomyRules.FormatWon(bill.Amount), 26, amtCol, TextAnchor.LowerCenter, FontStyle.Bold);
            UiKit.Layout(amt.rectTransform, new Vector2(0, 0), new Vector2(1, 0.24f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            StartCoroutine(Slam(card, x, threat));
        }

        IEnumerator Slam(RectTransform card, float x, bool threat)
        {
            float t = 0f;
            float rot = threat ? 12f : 6f;
            while (t < 1f)
            {
                t += Time.deltaTime * 2.4f;
                float e = 1f - Mathf.Pow(1f - Mathf.Clamp01(t), 3f);
                card.anchoredPosition = new Vector2(x, Mathf.Lerp(280f, 0f, e));
                card.localScale = Vector3.one * Mathf.Lerp(threat ? 1.45f : 0.7f, 1f, e);
                card.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(rot, 0f, e));
                yield return null;
            }
            card.anchoredPosition = new Vector2(x, 0);
            card.localRotation = Quaternion.identity;
        }
    }
}
