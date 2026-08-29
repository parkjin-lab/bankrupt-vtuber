using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace BankruptVtuber
{
    public class WeekStartDirector : MonoBehaviour
    {
        Text _cash;
        Text _debt;
        Text _billToday;
        Text _mental;
        Text _log;
        Text _day;
        Text _dayHead;
        RectTransform _dayTab;
        float _daySlam;
        Text _fandom;
        Text _superfans;
        RectTransform _stack;
        RectTransform _conflictRoot;
        Button _sootheCard;
        Button _styleCard;
        Text _conflictResult;
        string _conflictLine;
        RectTransform _supportRoot;
        Text _supportAmount;
        bool _supportOpen;
        bool _supportAcked;
        RectTransform _contentRoot;
        Text _contentHud;
        Text _yesterday;
        Image _yesterdayClip;
        Image _day1Tab;
        Image _day1Headline;
        Image _weekStartTab;
        Image _weekStartHeadline;
        Text _weekStartLabel;
        Image _midDayTab;
        RectTransform _lastDayRoot;
        Image _lastDayHeadline;
        Text _lastDayWeek;
        Text _lastDayNeed;
        Button _goLive;
        RectTransform _goLiveRt;
        Image _goLivePip;
        StudioPortrait _portrait;
        RectTransform _fanMinjun;
        RectTransform _fanHaeun;
        bool _ready;
        float _cashSlam;
        RectTransform _billTile;
        float _billSlam;
        Image _cashImg;
        Text _cashShort;
        Image _cashShortStamp;
        AudioSource _morningBgm;
        AudioSource _sfx;
        AudioClip _goLiveCue;
        AudioClip _pickCue;
        AudioClip _threatCue;
        bool _leavingMorning;
        bool _threatSfxPlayed;

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
            UiKit.UnlockUiInputForStream();
            Build();
            _sfx = gameObject.AddComponent<AudioSource>();
            _sfx.playOnAwake = false;
            _goLiveCue = Resources.Load<AudioClip>("Audio/sfx_golive");
            _pickCue = Resources.Load<AudioClip>("Audio/sfx_pick");
            _threatCue = Resources.Load<AudioClip>("Audio/sfx_threat");
            StartMorningBgm();
        }

        void OnDestroy()
        {
            if (_morningBgm != null)
                _morningBgm.Stop();
        }

        void Start()
        {
            var gm = GameManager.Instance;
            if (gm == null)
                return;
            ExtraThreatRules.EnsureRolled(gm.Run, gm.Balance, gm.Week2, gm.Week3, gm.Week4, gm.Week5);
            RefreshHud();
            _daySlam = 0.25f;
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
            _billSlam = Mathf.MoveTowards(_billSlam, 0f, Time.deltaTime);
            if (_billTile != null)
            {
                float u = _billSlam / 0.25f;
                _billTile.localScale = Vector3.one * (1f + 0.24f * u);
            }
            _daySlam = Mathf.MoveTowards(_daySlam, 0f, Time.deltaTime);
            if (_dayTab != null)
            {
                float u = _daySlam / 0.25f;
                _dayTab.localScale = Vector3.one * (1f + 0.28f * u);
            }
            else if (_dayHead != null)
            {
                float u = _daySlam / 0.25f;
                _dayHead.rectTransform.localScale = Vector3.one * (1f + 0.28f * u);
            }
            TickGoLivePulse();

            if (_supportOpen)
            {
                if (StreamBindings.Confirm)
                    OnSupportAck();
                return;
            }

            if (_ready
                && !_leavingMorning
                && !FandomRules.MustResolveConflict(GameManager.Instance.Run)
                && !ContentRules.MustPick(GameManager.Instance.Run)
                && StreamBindings.Confirm)
                LeaveMorning(() => GameManager.Instance.GoLive());
        }

        void TickGoLivePulse()
        {
            if (_goLiveRt == null)
                return;
            if (_goLive == null || !_goLive.gameObject.activeInHierarchy)
            {
                _goLiveRt.localScale = Vector3.one;
                return;
            }
            float u = 0.5f + 0.5f * Mathf.Sin(Time.time * 2.4f);
            _goLiveRt.localScale = Vector3.one * (1f + 0.04f * u);
            if (_goLivePip == null)
                return;
            var c = Palette.MoneyRed;
            _goLivePip.color = new Color(c.r, c.g, c.b, 0.45f + 0.55f * Mathf.Abs(Mathf.Sin(Time.time * 6f)));
        }

        void Build()
        {
            var canvas = UiKit.CreateCanvas("WeekStartCanvas", transform);
            StudioChrome.Wash(canvas.transform);
            var backdrop = UiKit.Image(canvas.transform, "MorningBackdrop", Color.white);
            UiKit.Stretch(backdrop.rectTransform);
            ArtSprites.Apply(backdrop, ArtSprites.MorningRoom, Palette.Studio, Color.white);
            backdrop.preserveAspect = false;
            backdrop.raycastTarget = false;
            var root = StreamSafeArea.Attach(canvas.transform);
            _portrait = new StudioPortrait(root, new Vector2(0.90f, 0.82f), new Vector2(220, 280), true);

            var title = UiKit.Label(root, "Title", "파산 버튜버", 48, Palette.Pastel, TextAnchor.UpperLeft, FontStyle.Bold);
            UiKit.Layout(title.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(36, -16), new Vector2(400, 54));
            var dayTabImg = UiKit.Image(root, "DayTab", Color.white);
            _dayTab = dayTabImg.rectTransform;
            UiKit.Layout(_dayTab, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(430, -8), new Vector2(300, 72));
            ArtSprites.Apply(dayTabImg, ArtSprites.DayTab, new Color(1f, 0.92f, 0.55f, 0.98f), Color.white);
            dayTabImg.preserveAspect = false;
            dayTabImg.raycastTarget = false;
            _dayHead = UiKit.Label(_dayTab, "DayHead", "", 44, Palette.Gold, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(_dayHead.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0f, -4f), new Vector2(-28f, -18f));
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
            _billToday = MoneyChip(moneyBar, "BillChip", "오늘 청구", Palette.MoneyRed, 0f, 0.28f);
            _billTile = moneyBar.Find("BillChip") as RectTransform;
            _cash = MoneyChip(moneyBar, "CashChip", "현금", Palette.CashGreen, 0.28f, 0.52f);
            var cashTile = moneyBar.Find("CashChip") as RectTransform;
            if (cashTile != null)
            {
                _cashImg = cashTile.GetComponent<Image>();
                _cashShortStamp = UiKit.Image(cashTile, "CashShortStamp", Color.white);
                UiKit.Layout(_cashShortStamp.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 1f), new Vector2(0f, -2f), new Vector2(200f, 48f));
                ArtSprites.Apply(_cashShortStamp, ArtSprites.BillShort, Palette.MoneyRed, Color.white);
                _cashShortStamp.preserveAspect = false;
                _cashShortStamp.raycastTarget = false;
                _cashShortStamp.gameObject.SetActive(false);
                _cashShort = UiKit.Label(_cashShortStamp.transform, "CashShort", "청구보다 부족", 15, Palette.MoneyRed, TextAnchor.MiddleCenter, FontStyle.Bold);
                UiKit.Layout(_cashShort.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0f, -1f), new Vector2(-10f, -6f));
                _cashShort.gameObject.SetActive(false);
            }
            _debt = MoneyChip(moneyBar, "DebtChip", "부채", Palette.MoneyRed, 0.52f, 0.76f);
            _mental = MoneyChip(moneyBar, "MentalChip", "멘탈", Palette.Pink, 0.76f, 1f);

            _midDayTab = UiKit.Image(root, "MorningMidDay", Color.white);
            UiKit.Layout(_midDayTab.rectTransform, new Vector2(0.74f, 1f), new Vector2(0.74f, 1f), new Vector2(0f, 1f), new Vector2(8f, -220f), new Vector2(180f, 56f));
            ArtSprites.Apply(_midDayTab, ArtSprites.DayTab, new Color(1f, 0.92f, 0.55f, 0.98f), Color.white);
            _midDayTab.preserveAspect = true;
            _midDayTab.raycastTarget = false;
            var midDayT = UiKit.Label(_midDayTab.transform, "T", "날짜", 18, Palette.Gold, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(midDayT.rectTransform, new Vector2(0.10f, 0.16f), new Vector2(0.90f, 0.84f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            _midDayTab.gameObject.SetActive(false);

            _weekStartTab = UiKit.Image(root, "MorningWeekStart", Color.white);
            UiKit.Layout(_weekStartTab.rectTransform, new Vector2(0.74f, 1f), new Vector2(0.74f, 1f), new Vector2(0f, 1f), new Vector2(8f, -220f), new Vector2(180f, 56f));
            ArtSprites.Apply(_weekStartTab, ArtSprites.DayTab, new Color(1f, 0.92f, 0.55f, 0.98f), Color.white);
            _weekStartTab.preserveAspect = true;
            _weekStartTab.raycastTarget = false;
            _weekStartLabel = UiKit.Label(_weekStartTab.transform, "T", "2주차", 18, Palette.Gold, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(_weekStartLabel.rectTransform, new Vector2(0.10f, 0.16f), new Vector2(0.90f, 0.84f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            _weekStartTab.gameObject.SetActive(false);
            _weekStartHeadline = UiKit.Image(root, "MorningWeekHeadline", Color.white);
            UiKit.Layout(_weekStartHeadline.rectTransform, new Vector2(0.74f, 1f), new Vector2(0.74f, 1f), new Vector2(0f, 1f), new Vector2(8f, -284f), new Vector2(228f, 92f));
            ArtSprites.Apply(_weekStartHeadline, ArtSprites.HeadlineClip, new Color(0.93f, 0.88f, 0.74f, 0.98f), Color.white);
            _weekStartHeadline.preserveAspect = true;
            _weekStartHeadline.raycastTarget = false;
            var weekHeadT = UiKit.Label(_weekStartHeadline.transform, "T", "헤드라인", 18, Palette.Ink, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(weekHeadT.rectTransform, new Vector2(0.10f, 0.16f), new Vector2(0.90f, 0.84f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            _weekStartHeadline.gameObject.SetActive(false);

            _day1Tab = UiKit.Image(root, "MorningDay1", Color.white);
            UiKit.Layout(_day1Tab.rectTransform, new Vector2(0.74f, 1f), new Vector2(0.74f, 1f), new Vector2(0f, 1f), new Vector2(8f, -220f), new Vector2(180f, 56f));
            ArtSprites.Apply(_day1Tab, ArtSprites.DayTab, new Color(1f, 0.92f, 0.55f, 0.98f), Color.white);
            _day1Tab.preserveAspect = true;
            _day1Tab.raycastTarget = false;
            var day1T = UiKit.Label(_day1Tab.transform, "T", "1일차", 18, Palette.Gold, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(day1T.rectTransform, new Vector2(0.10f, 0.16f), new Vector2(0.90f, 0.84f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            _day1Tab.gameObject.SetActive(false);
            _day1Headline = UiKit.Image(root, "MorningHeadline", Color.white);
            UiKit.Layout(_day1Headline.rectTransform, new Vector2(0.74f, 1f), new Vector2(0.74f, 1f), new Vector2(0f, 1f), new Vector2(8f, -284f), new Vector2(228f, 92f));
            ArtSprites.Apply(_day1Headline, ArtSprites.HeadlineClip, new Color(0.93f, 0.88f, 0.74f, 0.98f), Color.white);
            _day1Headline.preserveAspect = true;
            _day1Headline.raycastTarget = false;
            var day1HeadT = UiKit.Label(_day1Headline.transform, "T", "헤드라인", 18, Palette.Ink, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(day1HeadT.rectTransform, new Vector2(0.10f, 0.16f), new Vector2(0.90f, 0.84f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            _day1Headline.gameObject.SetActive(false);

            _lastDayRoot = UiKit.Panel(root, "LastDayBanner", Color.white);
            UiKit.Layout(_lastDayRoot, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(744, -8), new Vector2(312, 108));
            var lastTabImg = _lastDayRoot.GetComponent<Image>();
            ArtSprites.Apply(lastTabImg, ArtSprites.DayTab, new Color(1f, 0.92f, 0.55f, 0.98f), Color.white);
            lastTabImg.preserveAspect = false;
            lastTabImg.raycastTarget = false;
            var lastTitle = UiKit.Label(_lastDayRoot, "LastDayTitle", "마지막 날", 26, Palette.Gold, TextAnchor.MiddleLeft, FontStyle.Bold);
            UiKit.Layout(lastTitle.rectTransform, new Vector2(0, 0.58f), new Vector2(1, 1), new Vector2(0, 1), new Vector2(16, -4), new Vector2(-28, 0));
            _lastDayWeek = UiKit.Label(_lastDayRoot, "LastDayWeek", "1주차 마지막", 18, Palette.Gold, TextAnchor.MiddleLeft, FontStyle.Bold);
            UiKit.Layout(_lastDayWeek.rectTransform, new Vector2(0, 0.34f), new Vector2(1, 0.62f), new Vector2(0, 1), new Vector2(16, 0), new Vector2(-28, 0));
            _lastDayNeed = UiKit.Label(_lastDayRoot, "LastDayNeed", "", 13, Palette.Gold, TextAnchor.MiddleLeft, FontStyle.Bold);
            UiKit.Layout(_lastDayNeed.rectTransform, new Vector2(0, 0), new Vector2(1, 0.38f), new Vector2(0, 0), new Vector2(16, 6), new Vector2(-28, 0));
            UiKit.Wrap(_lastDayNeed);
            _lastDayRoot.gameObject.SetActive(false);
            _lastDayHeadline = UiKit.Image(root, "MorningLastHeadline", Color.white);
            UiKit.Layout(_lastDayHeadline.rectTransform, new Vector2(0.74f, 1f), new Vector2(0.74f, 1f), new Vector2(0f, 1f), new Vector2(8f, -284f), new Vector2(228f, 92f));
            ArtSprites.Apply(_lastDayHeadline, ArtSprites.HeadlineClip, new Color(0.93f, 0.88f, 0.74f, 0.98f), Color.white);
            _lastDayHeadline.preserveAspect = true;
            _lastDayHeadline.raycastTarget = false;
            var lastHeadT = UiKit.Label(_lastDayHeadline.transform, "T", "헤드라인", 18, Palette.Ink, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(lastHeadT.rectTransform, new Vector2(0.10f, 0.16f), new Vector2(0.90f, 0.84f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            _lastDayHeadline.gameObject.SetActive(false);

            var wavePanel = UiKit.Panel(root, "WavePanel", new Color(1, 1, 1, 0.06f));
            UiKit.Layout(wavePanel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -20), new Vector2(1320, 480));
            SafeFitCard.Bind(wavePanel, 1320f, 480f, 16f);
            UiKit.Label(wavePanel, "WaveTitle", "오늘의 고정비 + 위협 — 방어 웨이브", 26, Palette.Pastel, TextAnchor.UpperLeft, FontStyle.Bold);
            var wt = wavePanel.Find("WaveTitle") as RectTransform;
            UiKit.Layout(wt, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -12), new Vector2(0, 36));
            _yesterdayClip = UiKit.Image(wavePanel, "YesterdayClip", Color.white);
            UiKit.Layout(_yesterdayClip.rectTransform, new Vector2(0.03f, 1), new Vector2(0.97f, 1), new Vector2(0.5f, 1), new Vector2(0, -42), new Vector2(0, 78));
            ArtSprites.Apply(_yesterdayClip, ArtSprites.HeadlineClip, new Color(0.93f, 0.88f, 0.74f, 0.98f), Color.white);
            _yesterdayClip.preserveAspect = false;
            _yesterdayClip.raycastTarget = false;
            _yesterday = UiKit.Label(_yesterdayClip.transform, "Yesterday", "", 22, Palette.Ink, TextAnchor.MiddleLeft, FontStyle.Bold);
            UiKit.Layout(_yesterday.rectTransform, new Vector2(0.07f, 0.16f), new Vector2(0.93f, 0.84f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            UiKit.Wrap(_yesterday);
            _yesterdayClip.gameObject.SetActive(false);
            _yesterday.gameObject.SetActive(false);

            _stack = UiKit.Panel(wavePanel, "Stack", new Color(0, 0, 0, 0));
            UiKit.Layout(_stack, new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1260, 268));

            _log = UiKit.Label(wavePanel, "Log", "청구서가 몰려옵니다…", 24, Palette.PastelDim, TextAnchor.LowerLeft);
            UiKit.Layout(_log.rectTransform, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), new Vector2(0, 16), new Vector2(-40, 48));

            _goLive = UiKit.Button(root, "GoLive", "방송 켜기  (Space)", () => LeaveMorning(() => GameManager.Instance.GoLive()), Palette.PinkDeep, Color.white);
            _goLiveRt = _goLive.GetComponent<RectTransform>();
            UiKit.Layout(_goLiveRt, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 36), new Vector2(360, 70));
            var goLiveImg = _goLive.GetComponent<Image>();
            if (goLiveImg != null)
            {
                ArtSprites.ApplySliced(goLiveImg, ArtSprites.GoLiveKey, Color.white, new Vector4(48f, 36f, 48f, 36f));
                goLiveImg.raycastTarget = true;
            }
            _goLivePip = UiKit.Image(_goLive.transform, "LivePip", Palette.MoneyRed);
            UiKit.Layout(_goLivePip.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(32f, 0f), new Vector2(56f, 16f));
            ArtSprites.Apply(_goLivePip, ArtSprites.OnAirLed, Palette.MoneyRed, Color.white);
            _goLivePip.raycastTarget = false;
            var goCap = _goLive.transform.Find("Caption") as RectTransform;
            if (goCap != null)
                goCap.offsetMin = new Vector2(62f, 0f);
            _goLive.gameObject.SetActive(false);

            var conflictGo = new GameObject("ConflictRoot", typeof(RectTransform));
            conflictGo.transform.SetParent(root, false);
            _conflictRoot = conflictGo.GetComponent<RectTransform>();
            UiKit.Stretch(_conflictRoot);
            var conflictWash = UiKit.Image(_conflictRoot, "ConflictWash", new Color(0.08f, 0.03f, 0.08f, 0.84f));
            UiKit.Stretch(conflictWash.rectTransform);
            conflictWash.raycastTarget = true;
            var cTitle = UiKit.Label(_conflictRoot, "CTitle", "콘텐츠 편중 갈등", 42, Palette.Gold, TextAnchor.UpperCenter, FontStyle.Bold);
            UiKit.Layout(cTitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -16), new Vector2(920, 52));
            var cHint = UiKit.Label(_conflictRoot, "CBody", "오늘 안에 고르세요.", 22, Palette.Pastel, TextAnchor.UpperCenter);
            UiKit.Layout(cHint.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -64), new Vector2(720, 32));
            var conflictPair = UiKit.Panel(_conflictRoot, "ConflictPair", new Color(0, 0, 0, 0));
            UiKit.Layout(conflictPair, new Vector2(0.04f, 0.14f), new Vector2(0.96f, 0.86f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            _sootheCard = UiKit.Button(conflictPair, "Soothe", "특별방송으로 달래기", OnSootheConflict, Palette.PinkDeep, Color.white);
            UiKit.Layout(_sootheCard.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-300, 8), new Vector2(500, 340));
            ArtSprites.ApplySliced(_sootheCard.GetComponent<Image>(), ArtSprites.PanelDark, new Color(1f, 0.78f, 0.88f, 0.98f));
            StyleConflictCard(_sootheCard);
            _styleCard = UiKit.Button(conflictPair, "Style", "내 스타일대로", OnStyleConflict, Palette.Troll, Color.white);
            UiKit.Layout(_styleCard.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(300, 8), new Vector2(500, 340));
            ArtSprites.ApplySliced(_styleCard.GetComponent<Image>(), ArtSprites.PanelDark, new Color(0.92f, 0.42f, 0.48f, 0.98f));
            StyleConflictCard(_styleCard);
            var conflictPairLayout = SafePairLayout.Bind(conflictPair, _sootheCard.GetComponent<RectTransform>(), _styleCard.GetComponent<RectTransform>(), true, false);
            conflictPairLayout.MinEach = 480f;
            _conflictResult = UiKit.Label(_conflictRoot, "CResult", "", 30, Palette.Gold, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(_conflictResult.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 16), new Vector2(1100, 48));
            UiKit.Wrap(_conflictResult);
            _conflictRoot.gameObject.SetActive(false);

            var supportGo = new GameObject("SupportRoot", typeof(RectTransform));
            supportGo.transform.SetParent(root, false);
            _supportRoot = supportGo.GetComponent<RectTransform>();
            UiKit.Stretch(_supportRoot);
            var supportWash = UiKit.Image(_supportRoot, "SupportWash", new Color(0.10f, 0.07f, 0.02f, 0.80f));
            UiKit.Stretch(supportWash.rectTransform);
            supportWash.raycastTarget = true;
            var supportCard = UiKit.Panel(_supportRoot, "SupportCard", Color.white);
            UiKit.Layout(supportCard, new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(720, 380));
            ArtSprites.ApplySliced(supportCard.GetComponent<Image>(), ArtSprites.PanelDark, new Color(1f, 0.90f, 0.45f, 0.98f));
            SafeFitCard.Bind(supportCard, 720f, 380f);
            var supportTitle = UiKit.Label(supportCard, "SupportTitle", "팬 지원금", 52, Palette.Gold, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(supportTitle.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -28), new Vector2(-40, 70));
            _supportAmount = UiKit.Label(supportCard, "SupportAmt", "₩0", 48, Palette.Ink, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(_supportAmount.rectTransform, new Vector2(0.08f, 0.28f), new Vector2(0.92f, 0.72f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            var supportAck = UiKit.Button(supportCard, "SupportAck", "확인", OnSupportAck, Palette.Gold, Palette.Ink);
            UiKit.Layout(supportAck.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 28), new Vector2(320, 72));
            _supportRoot.gameObject.SetActive(false);

            _contentRoot = UiKit.Panel(root, "ContentPick", Color.white);
            UiKit.Layout(_contentRoot, new Vector2(0.04f, 0), new Vector2(0.96f, 0), new Vector2(0.5f, 0), new Vector2(0, 120), new Vector2(0, 280));
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
            bool bill = name.Contains("Bill");
            bool mental = name.Contains("Mental");
            var img = panel.GetComponent<Image>();
            if (cash)
            {
                ArtSprites.Apply(img, ArtSprites.CashSlip, new Color(0.98f, 0.94f, 0.86f, 0.98f), Color.white);
                img.preserveAspect = false;
                img.raycastTarget = false;
            }
            else if (mental)
            {
                ArtSprites.Apply(img, ArtSprites.MentalNote, new Color(1f, 0.95f, 0.72f, 0.98f), Color.white);
                img.preserveAspect = false;
                img.raycastTarget = false;
            }
            else
            {
                ArtSprites.ApplySliced(
                    img,
                    debt ? ArtSprites.ThreatBanner : bill ? ArtSprites.BillNotice : ArtSprites.PanelDark,
                    debt ? Palette.MoneyRed : bill ? Color.white : new Color(0.92f, 0.45f, 0.62f, 1f));
            }
            Color caption = cash || mental ? Palette.Ink : Color.white;
            UiKit.Label(panel, "L", label, 16, caption, TextAnchor.UpperLeft, FontStyle.Bold);
            var l = panel.Find("L") as RectTransform;
            UiKit.Layout(l, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1), new Vector2(16, -6), new Vector2(-20, 20));
            var v = UiKit.Label(panel, "V", "₩0", 28, caption, TextAnchor.LowerLeft, FontStyle.Bold);
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
            if (_dayHead != null)
                _dayHead.text = run.day + "일차";
            _day.text = $"{week}주차  ·  {run.day}일차   /   {last}일{members}{goods}{agency}{junior}{sponsor}{rank}{concert}";
            _cash.text = EconomyRules.FormatWon(run.cash);
            _debt.text = EconomyRules.FormatWon(run.debt);
            if (_billToday != null)
                _billToday.text = EconomyRules.FormatWon(PeekTodayBills(GameManager.Instance));
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
            RefreshYesterday(run);
            RefreshWeekStart(run);
            RefreshDay1(run);
            RefreshLastDay(run);
            RefreshMidDay(run);
            RefreshCashShort();
        }

        void RefreshCashShort()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.Run == null)
                return;
            var run = gm.Run;
            int bills = PeekTodayBills(gm);
            bool shortfall;
            if (!run.billsAppliedThisDay)
                shortfall = run.cash < bills;
            else
            {
                int debtGrew = Mathf.Max(0, run.debt - run.debtAtDayStart);
                int wallet = run.cash - debtGrew + bills - run.lastFanSupport;
                shortfall = wallet < bills;
            }
            if (_cashShortStamp != null)
            {
                if (shortfall)
                {
                    ArtSprites.Apply(_cashShortStamp, ArtSprites.BillShort, Palette.MoneyRed, Color.white);
                    _cashShortStamp.preserveAspect = false;
                }
                _cashShortStamp.gameObject.SetActive(shortfall);
            }
            if (_cashShort != null)
            {
                if (shortfall)
                {
                    _cashShort.text = "청구보다 부족";
                    _cashShort.color = Palette.MoneyRed;
                }
                _cashShort.gameObject.SetActive(shortfall);
            }
            if (shortfall)
            {
                if (_cash != null)
                    _cash.color = Palette.MoneyRed;
                if (_cashImg != null)
                    _cashImg.color = Color.white;
            }
            else
            {
                if (_cash != null)
                    _cash.color = Palette.Ink;
                if (_cashImg != null)
                    _cashImg.color = Color.white;
            }
        }

        static int PeekTodayBills(GameManager gm)
        {
            if (gm == null || gm.Run == null)
                return 0;
            var run = gm.Run;
            if (run.billsAppliedThisDay)
                return EconomyRules.TonightBills(run);
            int extra = Mathf.Max(0, run.extraThreatAmount);
            int surcharge = Mathf.Max(0, run.pendingExtraSurcharge);
            int auto = FandomRules.AutoCostToday(run, gm.Fandom);
            return WeekSchedule.TotalFixedBills(run, gm.Balance, gm.Week2, gm.Week3, gm.Week4, gm.Week5)
                + extra + surcharge + auto;
        }

        void RefreshMidDay(GameRunState run)
        {
            if (_midDayTab == null)
                return;
            _midDayTab.gameObject.SetActive(run != null && MorningMidWeekDay(run.day));
        }

        static bool MorningMidWeekDay(int day) =>
            day == 2 || day == 3 || day == 4
            || day == 7 || day == 8 || day == 9
            || day == 12 || day == 13 || day == 14
            || day == 17 || day == 18 || day == 19
            || day == 22 || day == 23 || day == 24;

        void RefreshYesterday(GameRunState run)
        {
            if (_yesterday == null)
                return;
            string line = DayHeadline.YesterdayLine(run);
            _yesterday.text = line;
            bool on = !string.IsNullOrEmpty(line);
            if (_yesterdayClip != null)
                _yesterdayClip.gameObject.SetActive(on);
            _yesterday.gameObject.SetActive(on);
        }

        void RefreshWeekStart(GameRunState run)
        {
            if (_weekStartTab == null)
                return;
            bool on = run != null && (run.day == 6 || run.day == 11 || run.day == 16 || run.day == 21);
            _weekStartTab.gameObject.SetActive(on);
            if (_weekStartHeadline != null)
                _weekStartHeadline.gameObject.SetActive(on);
            if (!on || _weekStartLabel == null)
                return;
            _weekStartLabel.text = WeekSchedule.WeekNumber(run) + "주차";
        }

        void RefreshDay1(GameRunState run)
        {
            bool day1 = run != null && run.day == 1;
            if (_day1Tab != null)
                _day1Tab.gameObject.SetActive(day1);
            if (_day1Headline != null)
                _day1Headline.gameObject.SetActive(day1);
        }

        void RefreshLastDay(GameRunState run)
        {
            bool last = run != null && run.day == WeekSchedule.LastDayOfCurrentWeek(run);
            if (_lastDayRoot != null)
                _lastDayRoot.gameObject.SetActive(last);
            if (_lastDayHeadline != null)
                _lastDayHeadline.gameObject.SetActive(last);
            if (!last)
                return;
            int week = WeekSchedule.WeekNumber(run);
            if (_lastDayWeek != null)
                _lastDayWeek.text = week + "주차 마지막";
            if (_lastDayNeed != null)
                _lastDayNeed.text = LastDayClearReminder(run);
        }

        static string LastDayClearReminder(GameRunState run)
        {
            var gm = GameManager.Instance;
            string now = "지금 현금 " + EconomyRules.FormatWon(run.cash) +
                         " · 부채 " + EconomyRules.FormatWon(run.debt);
            int week = WeekSchedule.WeekNumber(run);
            if (week == 2 && gm != null && gm.Week2 != null)
            {
                return now + "   클리어 부채 ≤ " + EconomyRules.FormatWon(gm.Week2.winDebtMax) +
                       " 또는 현금 ≥ " + EconomyRules.FormatWon(gm.Week2.winCashMin) + " · 멤버십";
            }
            if (week == 3 && gm != null && gm.Week3 != null)
            {
                return now + "   클리어 부채 ≤ " + EconomyRules.FormatWon(gm.Week3.winDebtMax) +
                       " 또는 현금 ≥ " + EconomyRules.FormatWon(gm.Week3.winCashMin) + " · 아크릴";
            }
            if (week == 4 && gm != null && gm.Week4 != null)
            {
                return now + "   클리어 에이전시, 그리고 부채 ≤ " + EconomyRules.FormatWon(gm.Week4.winDebtMax) +
                       " 또는 현금 ≥ " + EconomyRules.FormatWon(gm.Week4.winCashMin);
            }
            if (week == 5 && gm != null && gm.Week5 != null)
            {
                return now + "   오늘 끝나면 엔딩 · 파산 부채 ≥ " + EconomyRules.FormatWon(gm.Week5.bankruptDebt);
            }
            var w1 = gm != null ? gm.Balance : null;
            if (w1 == null)
                return now;
            return now + "   클리어 부채 ≤ " + EconomyRules.FormatWon(w1.winDebtMax) +
                   " 또는 현금 ≥ " + EconomyRules.FormatWon(w1.winCashMin);
        }

        static string ContentPickName(StreamContentType type) => type switch
        {
            StreamContentType.Talk => "토크",
            StreamContentType.Game => "게임",
            StreamContentType.Song => "노래",
            StreamContentType.Reaction => "리액션",
            _ => ""
        };

        static string ContentPickVibe(StreamContentType type) => type switch
        {
            StreamContentType.Talk => "편하게 잡담",
            StreamContentType.Game => "같이 깨자",
            StreamContentType.Song => "고음 승부",
            StreamContentType.Reaction => "같이 보자",
            _ => ""
        };

        static string ContentPickIcon(StreamContentType type)
        {
            return ArtSprites.ForContent(type) ?? ArtSprites.BubblePill;
        }

        static Color ContentPickAccent(StreamContentType type) => type switch
        {
            StreamContentType.Talk => Palette.Pink,
            StreamContentType.Game => Palette.Troll,
            StreamContentType.Song => Palette.Gold,
            StreamContentType.Reaction => Palette.PastelDim,
            _ => Palette.Muted
        };

        void AddContentButton(StreamContentType type, int index)
        {
            var look = ContentShowLook.For(type);
            var t = ContentRules.Tuning(GameManager.Instance != null ? GameManager.Instance.Content : null, type);
            string name = ContentPickName(type);
            if (string.IsNullOrEmpty(name))
                name = t.Name;
            string vibe = ContentPickVibe(type);
            string caption = $"{name}\n{vibe}\n수입 ×{t.IncomeMul:0.##}  멘탈 −{t.MentalCost}";
            var btn = UiKit.Button(_contentRoot, type.ToString(), caption, () => OnPickContent(type), look.Card, look.CardInk);
            float a = index / 4f;
            float b = (index + 1) / 4f;
            var rt = btn.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(a, 0f);
            rt.anchorMax = new Vector2(b, 0.70f);
            rt.offsetMin = new Vector2(10f, 16f);
            rt.offsetMax = new Vector2(-10f, -8f);
            var img = btn.GetComponent<Image>();
            ArtSprites.ApplySliced(img, ArtSprites.ContentPlate, look.Card, new Vector4(40f, 48f, 40f, 48f));
            img.raycastTarget = true;
            var wash = UiKit.Image(btn.transform, "ShowWash", look.Wash);
            UiKit.Layout(wash.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -6), new Vector2(-20, 10));
            var veil = UiKit.Image(btn.transform, "ShowVeil", look.WashVeil);
            UiKit.Layout(veil.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -6), new Vector2(-20, 10));
            var accent = UiKit.Image(btn.transform, "Accent", ContentPickAccent(type));
            UiKit.Layout(accent.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f), new Vector2(6f, 8f), new Vector2(12f, -16f));
            var icon = UiKit.Image(btn.transform, "Icon", Color.white);
            UiKit.Layout(icon.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -8f), new Vector2(72f, 72f));
            ArtSprites.Apply(icon, ContentPickIcon(type), Color.white, Color.white);
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            AddCardChip(btn.transform, 0, look.Type == StreamContentType.Talk ? Palette.Blue : look.Type == StreamContentType.Game ? Palette.Troll : look.Type == StreamContentType.Song ? Palette.Gold : Palette.Muted);
            AddCardChip(btn.transform, 1, look.Type == StreamContentType.Talk ? Palette.Green : look.CamFrame);
            var cap = btn.GetComponentInChildren<Text>();
            if (cap != null)
            {
                cap.fontSize = 22;
                cap.lineSpacing = 1.12f;
                cap.color = look.CardInk;
                cap.rectTransform.offsetMin = new Vector2(18f, 8f);
                cap.rectTransform.offsetMax = new Vector2(-8f, -82f);
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
            PlayPickSfx();
            gm.SaveRun();
            _contentRoot.gameObject.SetActive(false);
            RefreshHud();
            _ready = true;
            if (Week5Rules.ConcertStreamReady(gm.Run))
                _goLive.GetComponentInChildren<Text>().text = "콘서트 방송  (Space)";
            _goLive.gameObject.SetActive(true);
        }

        static void StyleConflictCard(Button btn)
        {
            var cap = btn.GetComponentInChildren<Text>();
            if (cap == null)
                return;
            cap.fontSize = 30;
            cap.lineSpacing = 1.2f;
            cap.horizontalOverflow = HorizontalWrapMode.Wrap;
            cap.rectTransform.offsetMin = new Vector2(24f, 20f);
            cap.rectTransform.offsetMax = new Vector2(-24f, -20f);
        }

        void FillConflictCards()
        {
            var f = GameManager.Instance != null ? GameManager.Instance.Fandom : null;
            int mental = f != null ? f.conflictSootheMental : 10;
            int sootheLoy = f != null ? f.conflictSootheLoyalty : 8;
            int t2 = f != null ? f.conflictStyleT2 : 2;
            int styleLoy = f != null ? f.conflictStyleLoyalty : 10;
            int extra = f != null ? f.conflictExtraSurcharge : 2000;
            var sootheCap = _sootheCard != null ? _sootheCard.GetComponentInChildren<Text>() : null;
            if (sootheCap != null)
                sootheCap.text = $"특별방송으로 달래기\n멘탈 −{mental}\n충성 +{sootheLoy}";
            var styleCap = _styleCard != null ? _styleCard.GetComponentInChildren<Text>() : null;
            if (styleCap != null)
                styleCap.text = $"내 스타일대로\nT2 −{t2}\n충성 −{styleLoy}\n다음 위협 +{EconomyRules.FormatWon(extra)}";
            if (_conflictResult != null)
                _conflictResult.text = "";
            if (_sootheCard != null)
                _sootheCard.gameObject.SetActive(true);
            if (_styleCard != null)
                _styleCard.gameObject.SetActive(true);
        }

        void OnSootheConflict()
        {
            var gm = GameManager.Instance;
            if (gm == null || !FandomRules.SootheConflict(gm.Run, gm.Fandom))
                return;
            var f = gm.Fandom;
            int mental = f != null ? f.conflictSootheMental : 10;
            int loy = f != null ? f.conflictSootheLoyalty : 8;
            _conflictLine = $"달랬다 멘탈 −{mental} · 충성 +{loy}";
            ShowConflictResult();
            gm.SaveRun();
            RefreshHud();
        }

        void OnStyleConflict()
        {
            var gm = GameManager.Instance;
            if (gm == null || !FandomRules.StyleConflict(gm.Run, gm.Fandom))
                return;
            var f = gm.Fandom;
            int t2 = f != null ? f.conflictStyleT2 : 2;
            int loy = f != null ? f.conflictStyleLoyalty : 10;
            int extra = f != null ? f.conflictExtraSurcharge : 2000;
            _conflictLine = $"내 스타일대로 T2 −{t2} · 충성 −{loy} · 다음 위협 +{EconomyRules.FormatWon(extra)}";
            ShowConflictResult();
            gm.SaveRun();
            RefreshHud();
        }

        void ShowConflictResult()
        {
            if (_sootheCard != null)
                _sootheCard.gameObject.SetActive(false);
            if (_styleCard != null)
                _styleCard.gameObject.SetActive(false);
            if (_conflictResult != null)
                _conflictResult.text = _conflictLine;
            _log.text = _conflictLine;
        }

        void OnSupportAck()
        {
            _supportAcked = true;
            _supportOpen = false;
            if (_supportRoot != null)
                _supportRoot.gameObject.SetActive(false);
        }

        IEnumerator BillWave(GameManager gm)
        {
            _threatSfxPlayed = false;
            var b = gm.Balance;
            ExtraThreatRules.EnsureRolled(gm.Run, b, gm.Week2, gm.Week3, gm.Week4, gm.Week5);
            Week3Rules.TryUnlockGoods(gm.Run, gm.Week3);

            if (FandomRules.MustResolveConflict(gm.Run))
            {
                FillConflictCards();
                _conflictRoot.gameObject.SetActive(true);
                _goLive.gameObject.SetActive(false);
                _log.text = "콘텐츠 편중 갈등 — 오늘 안에 고르세요.";
                while (FandomRules.MustResolveConflict(gm.Run))
                    yield return null;
                if (!string.IsNullOrEmpty(_conflictLine))
                    _log.text = _conflictLine;
                yield return new WaitForSeconds(0.85f);
                _conflictRoot.gameObject.SetActive(false);
                RefreshHud();
            }

            if (!gm.Run.billsAppliedThisDay)
            {
                int peek = FandomRules.RollSupport(gm.Run, gm.Fandom);
                if (peek > 0)
                {
                    _supportAmount.text = EconomyRules.FormatWon(peek);
                    _supportAcked = false;
                    _supportOpen = true;
                    _supportRoot.gameObject.SetActive(true);
                    _supportRoot.SetAsLastSibling();
                    while (!_supportAcked)
                        yield return null;
                    _supportOpen = false;
                    _supportRoot.gameObject.SetActive(false);
                }
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

            RefreshYesterday(gm.Run);
            RefreshLastDay(gm.Run);
            RefreshHud();
            _billSlam = 0.25f;
            if (_yesterday != null && _yesterday.gameObject.activeSelf)
                yield return new WaitForSeconds(0.55f);
            if (_lastDayRoot != null && _lastDayRoot.gameObject.activeSelf)
                yield return new WaitForSeconds(0.45f);

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
                    ArtSprites.EventWarn,
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
            if (threat)
                PlayThreatSfx();
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

        void StartMorningBgm()
        {
            var clip = Resources.Load<AudioClip>("Audio/bgm_morning");
            if (clip == null)
                return;
            _morningBgm = gameObject.AddComponent<AudioSource>();
            _morningBgm.clip = clip;
            _morningBgm.loop = true;
            _morningBgm.playOnAwake = false;
            _morningBgm.volume = 0.20f;
            _morningBgm.Play();
        }

        void LeaveMorning(System.Action next)
        {
            if (_leavingMorning)
                return;
            _leavingMorning = true;
            PlayGoLiveSfx();
            StartCoroutine(FadeMorningBgmThen(next));
        }

        void PlayGoLiveSfx()
        {
            if (_sfx != null && _goLiveCue != null)
                _sfx.PlayOneShot(_goLiveCue, 0.48f);
        }

        void PlayPickSfx()
        {
            if (_sfx != null && _pickCue != null)
                _sfx.PlayOneShot(_pickCue, 0.42f);
        }

        IEnumerator FadeMorningBgmThen(System.Action next)
        {
            if (_morningBgm != null && _morningBgm.isPlaying)
            {
                float start = _morningBgm.volume;
                float t = 0f;
                const float fade = 0.2f;
                while (t < fade)
                {
                    t += Time.deltaTime;
                    _morningBgm.volume = Mathf.Lerp(start, 0f, t / fade);
                    yield return null;
                }
                _morningBgm.Stop();
            }
            next?.Invoke();
        }

        void PlayThreatSfx()
        {
            if (_threatSfxPlayed)
                return;
            _threatSfxPlayed = true;
            if (_sfx != null && _threatCue != null)
                _sfx.PlayOneShot(_threatCue, 0.46f);
        }
    }
}
