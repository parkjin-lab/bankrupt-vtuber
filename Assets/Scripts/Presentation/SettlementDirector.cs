using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace BankruptVtuber
{
    public class SettlementDirector : MonoBehaviour
    {
        Text _body;
        Text _result;
        Button _next;
        RectTransform _nextRt;
        RectTransform _nextChip;
        Button _repay;
        Button _restart;
        Button _clipYes;
        Button _clipNo;
        Text _clipNote;
        Button _produce;
        Button _foundAgency;
        Button _scout;
        Button _signSponsor;
        Button _bookConcert;
        Button _concertLive;
        Button _letter;
        Button _auto;
        Button _soothe;
        Button _style;
        Button _retire;
        RectTransform _rankBox;
        Text _rankPanel;
        GameObject _endingRoot;
        Text _endingTitle;
        Text _endingBody;
        Text _headlineTag;
        Text _headline;
        Image _headlineClip;
        Text _showLine;
        Image _showLineIcon;
        Text _clearHeadline;
        Text _stampHeadline;
        Text _endingHeadline;
        Text _tileIncome;
        Text _tileBills;
        Text _tileCash;
        Text _tileDebt;
        Text _tilePerfect;
        Text _tileMiss;
        Text _tileViewers;
        Text _tileMental;
        RectTransform _cashTile;
        RectTransform _debtTile;
        Image _leftCashSlip;
        Image _leftCashShortStamp;
        Text _leftCashShort;
        StudioPortrait _portrait;
        StudioPortrait _endingPortrait;
        StudioPortrait _clearPortrait;
        StudioPortrait _stampPortrait;
        GameObject _clearRoot;
        Text _clearTitle;
        Text _clearCash;
        Text _clearDebt;
        GameObject _stampRoot;
        Image _stampWash;
        Text _stampMark;
        Text _stampDebt;
        Text _stampEpitaph;
        float _mood;
        bool _cashUp;
        bool _incomeCountStarted;
        bool _incomeCounting;
        int _incomeTarget;
        int _incomeBill;
        float _incomeCountT;
        bool _coverCrossed;
        float _incomeCoverFlash;
        bool _debtCountStarted;
        bool _debtCounting;
        int _debtFrom;
        int _debtTo;
        float _debtCountT;
        float _debtDip;
        bool _mentalCountStarted;
        bool _mentalCounting;
        int _mentalFrom;
        int _mentalTo;
        float _mentalCountT;
        float _mentalTick;
        string _bodyLead;
        Text _leftCash;
        bool _leftCashShown;
        float _leftCashSnap;
        RectTransform _billsTile;
        Image _billsImg;
        Text _billsCap;
        Text _shortChip;
        Image _shortStamp;
        float _shortFlash;
        bool _shortFired;
        GameObject _letterRoot;
        Text _letterFrom;
        Text _letterTag;
        Text _letterBody;
        Text _letterHeart;
        float _letterHeartFlash;
        bool _letterOpen;
        bool _letterDismissed;
        GameObject _memberRoot;
        Text _memberBody;
        bool _memberOpen;
        GameObject _clipRoot;
        Text _clipSlam;
        float _clipSlamFlash;
        bool _clipOpen;
        GameObject _goodsRoot;
        Text _goodsBody;
        bool _goodsOpen;
        GameObject _agencyRoot;
        Text _agencyBody;
        bool _agencyOpen;
        bool _agencyDismissed;
        GameObject _agencySplashRoot;
        Text _agencySplashBody;
        bool _agencySplashOpen;
        GameObject _juniorRoot;
        Text _juniorBody;
        bool _juniorOpen;
        bool _juniorDismissed;
        GameObject _concertRoot;
        Text _concertBody;
        bool _concertOpen;
        bool _concertDismissed;
        GameObject _concertResultRoot;
        Image _concertResultPanel;
        Text _concertResultTitle;
        Text _concertResultSub;
        bool _concertResultOpen;
        bool _concertResultDismissed;
        GameObject _conflictOverlay;
        Button _conflictSoothe;
        Button _conflictStyle;
        Text _conflictOverlayResult;
        bool _conflictOpen;
        GameObject _autoRoot;
        Text _autoBody;
        bool _autoOpen;
        RectTransform _offerRow;
        RectTransform _week5Row;
        RectTransform _actionRow;
        AudioSource _settleBgm;
        AudioSource _settleSfx;
        AudioClip _clearCue;
        AudioClip _bankruptCue;
        AudioClip _nextDayCue;
        AudioClip _letterCue;
        AudioClip _memberCue;
        AudioClip _clipCue;
        AudioClip _goodsCue;
        AudioClip _agencyCue;
        AudioClip _rankingCue;
        AudioClip _concertBookCue;
        bool _rankHeard;
        bool _leavingSettle;
        bool _resultStingPlayed;

        void Awake()
        {
            UiKit.EnsureCamera(Palette.Studio);
            UiKit.EnsureEventSystem();
            UiKit.UnlockUiInputForStream();
            Build();
            StartSettleBgm();
        }

        void OnDestroy()
        {
            if (_settleBgm != null)
                _settleBgm.Stop();
        }

        void Start()
        {
            var gm = GameManager.Instance;
            if (gm != null)
            {
                Week2Rules.ApplyMembershipPassive(gm.Run, gm.Week2);
                Week3Rules.TryUnlockGoods(gm.Run, gm.Week3);
                Week3Rules.ApplyGoodsSales(gm.Run, gm.Week3);
                Week4Rules.ApplyJuniorDaily(gm.Run, gm.Week4);
                Week4Rules.ApplySponsorDaily(gm.Run, gm.Week4);
                Week5Rules.ApplyRanking(gm.Run, gm.Week5);
                Week5Rules.ApplyConcertResult(gm.Run, gm.Balance, gm.Week5);
                Week5Rules.NoteZeroMentalDay(gm.Run);
            }
            Render();
            AdvanceBeats();
        }

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null)
                return;
            _portrait?.Tick(Time.deltaTime);
            _endingPortrait?.Tick(Time.deltaTime);
            _clearPortrait?.Tick(Time.deltaTime);
            _stampPortrait?.Tick(Time.deltaTime);
            _mood = Mathf.MoveTowards(_mood, 0f, Time.deltaTime * 0.55f);
            TickIncomeCount(Time.deltaTime);
            TickDebtCount(Time.deltaTime);
            TickMentalCount(Time.deltaTime);
            TickLeftCash(Time.deltaTime);
            if (_cashTile != null && _debtTile != null)
            {
                float pulse = 1f + 0.12f * Mathf.Abs(Mathf.Sin(Time.time * 6f)) * (0.35f + _mood);
                if (_cashUp)
                {
                    _cashTile.localScale = Vector3.one * pulse;
                    _debtTile.localScale = Vector3.one;
                }
                else
                {
                    _cashTile.localScale = Vector3.one;
                    _debtTile.anchoredPosition = new Vector2(_debtTile.anchoredPosition.x, Mathf.Sin(Time.time * 28f) * 5f * (0.4f + _mood));
                    _debtTile.localScale = Vector3.one * (1f + 0.08f * _mood);
                }
            }
            _letterHeartFlash = Mathf.MoveTowards(_letterHeartFlash, 0f, Time.deltaTime * 0.85f);
            if (_letterHeart != null)
            {
                var hc = _letterHeart.color;
                hc.a = _letterHeartFlash;
                _letterHeart.color = hc;
                _letterHeart.rectTransform.localScale = Vector3.one * (1f + 0.18f * _letterHeartFlash);
            }
            TickShortfall(Time.deltaTime);
            if (_tileDebt != null && _debtCounting)
            {
                _tileDebt.color = Palette.MoneyRed;
                _tileDebt.rectTransform.localScale = Vector3.one * (1f + 0.12f * (1f - Mathf.Clamp01(_debtCountT / 0.4f)));
            }
            else if (_tileDebt != null && _debtDip > 0.02f)
            {
                _debtDip = Mathf.MoveTowards(_debtDip, 0f, Time.deltaTime * 3.2f);
                _tileDebt.color = Color.Lerp(Color.white, Palette.CashGreen, _debtDip);
                _tileDebt.rectTransform.localScale = Vector3.one * (1f - 0.06f * _debtDip);
            }
            else if (_tileDebt != null)
            {
                _tileDebt.color = Color.white;
                _tileDebt.rectTransform.localScale = Vector3.one;
            }
            if (_tileMental != null && _mentalCounting)
            {
                _tileMental.color = Palette.MoneyRed;
                _tileMental.rectTransform.localScale = Vector3.one * (1f + 0.12f * (1f - Mathf.Clamp01(_mentalCountT / 0.35f)));
            }
            else if (_tileMental != null && _mentalTick > 0.02f)
            {
                _mentalTick = Mathf.MoveTowards(_mentalTick, 0f, Time.deltaTime * 4f);
                _tileMental.color = Color.Lerp(Palette.Ink, Palette.CashGreen, _mentalTick);
                _tileMental.rectTransform.localScale = Vector3.one * (1f + 0.10f * _mentalTick);
            }
            else if (_tileMental != null)
            {
                _tileMental.color = Palette.Ink;
                _tileMental.rectTransform.localScale = Vector3.one;
            }
            _incomeCoverFlash = Mathf.MoveTowards(_incomeCoverFlash, 0f, Time.deltaTime * 2.2f);
            if (_tileIncome != null && _incomeCoverFlash > 0.02f)
            {
                _tileIncome.color = Color.Lerp(Palette.Ink, Palette.Gold, _incomeCoverFlash);
                _tileIncome.rectTransform.localScale = Vector3.one * (1f + 0.16f * _incomeCoverFlash);
            }
            else if (_tileIncome != null && !_incomeCounting)
            {
                _tileIncome.color = Palette.Ink;
                _tileIncome.rectTransform.localScale = Vector3.one;
            }
            _clipSlamFlash = Mathf.MoveTowards(_clipSlamFlash, 0f, Time.deltaTime * 0.7f);
            if (_clipSlam != null)
            {
                var sc = _clipSlam.color;
                sc.a = _clipSlamFlash;
                _clipSlam.color = sc;
                _clipSlam.rectTransform.localScale = Vector3.one * (1f + 0.35f * _clipSlamFlash);
            }
            TickNextPulse();
            if (_letterOpen || _memberOpen || _clipOpen || _goodsOpen || _agencyOpen || _agencySplashOpen || _juniorOpen || _concertOpen || _concertResultOpen || _conflictOpen || _autoOpen)
                return;
            if (!_leavingSettle && CanAdvance(gm.Run) && StreamBindings.Confirm)
            {
                PlayNextDaySfx();
                LeaveSettle(() => gm.NextMorning());
            }
        }

        void TickNextPulse()
        {
            if (_nextRt == null)
                return;
            if (_next == null || !_next.gameObject.activeInHierarchy)
            {
                _nextRt.localScale = Vector3.one;
                return;
            }
            float u = 0.5f + 0.5f * Mathf.Sin(Time.time * 2.2f);
            _nextRt.localScale = Vector3.one * (1f + 0.03f * u);
            if (_nextChip != null)
                _nextChip.localScale = Vector3.one * (1f + 0.08f * u);
        }

        static bool CanAdvance(GameRunState run) =>
            !FandomRules.MustResolveConflict(run) &&
            (run.lastOutcome == WeekOutcome.Continue ||
            WeekSchedule.CanEnterWeek2(run) ||
            WeekSchedule.CanEnterWeek3(run) ||
            WeekSchedule.CanEnterWeek4(run) ||
            WeekSchedule.CanEnterWeek5(run));

        void Build()
        {
            var canvas = UiKit.CreateCanvas("SettlementCanvas", transform);
            StudioChrome.Wash(canvas.transform);
            var backdrop = UiKit.Image(canvas.transform, "SettlementBackdrop", Color.white);
            UiKit.Stretch(backdrop.rectTransform);
            ArtSprites.Apply(backdrop, ArtSprites.SettlementDesk, Palette.Studio, Color.white);
            backdrop.preserveAspect = false;
            backdrop.raycastTarget = false;
            var root = StreamSafeArea.Attach(canvas.transform);
            _portrait = new StudioPortrait(root, new Vector2(0.90f, 0.82f), new Vector2(210, 268), false);

            var title = UiKit.Label(root, "Title", "정산", 48, Palette.Pastel, TextAnchor.UpperLeft, FontStyle.Bold);
            UiKit.Layout(title.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(36, -16), new Vector2(400, 56));
            _headlineClip = UiKit.Image(root, "HeadlineClip", Color.white);
            UiKit.Layout(_headlineClip.rectTransform, new Vector2(0, 1), new Vector2(0.78f, 1), new Vector2(0, 1), new Vector2(36, -66), new Vector2(0, 80));
            ArtSprites.Apply(_headlineClip, ArtSprites.HeadlineClip, new Color(0.93f, 0.88f, 0.74f, 0.98f), Color.white);
            _headlineClip.preserveAspect = false;
            _headlineClip.raycastTarget = false;
            _headlineTag = UiKit.Label(_headlineClip.transform, "HeadlineTag", "오늘 헤드라인", 16, Palette.Ink, TextAnchor.UpperLeft, FontStyle.Bold);
            UiKit.Layout(_headlineTag.rectTransform, new Vector2(0.07f, 0.62f), new Vector2(0.93f, 0.96f), new Vector2(0, 1), Vector2.zero, Vector2.zero);
            var showLineRow = UiKit.Panel(root, "ShowLineRow", new Color(0, 0, 0, 0));
            UiKit.Layout(showLineRow, new Vector2(0.78f, 1), new Vector2(0.78f, 1), new Vector2(1, 1), new Vector2(-8, -68), new Vector2(168, 22));
            _showLineIcon = UiKit.Image(showLineRow, "ShowLineIcon", Color.white);
            UiKit.Layout(_showLineIcon.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(12f, 0f), new Vector2(20f, 20f));
            _showLineIcon.raycastTarget = false;
            _showLine = UiKit.Label(showLineRow, "ShowLine", "", 18, Palette.Pink, TextAnchor.MiddleLeft, FontStyle.Bold);
            UiKit.Layout(_showLine.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0f, 0.5f), new Vector2(28f, 0f), new Vector2(-4f, 0f));
            _headline = UiKit.Label(_headlineClip.transform, "Headline", "", 26, Palette.Ink, TextAnchor.MiddleLeft, FontStyle.Bold);
            UiKit.Layout(_headline.rectTransform, new Vector2(0.07f, 0.10f), new Vector2(0.93f, 0.62f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            UiKit.Wrap(_headline);
            _headline.lineSpacing = 1.1f;
            _headlineClip.gameObject.SetActive(false);

            var recap = UiKit.Panel(root, "Recap", new Color(0, 0, 0, 0));
            UiKit.Layout(recap, new Vector2(0, 1), new Vector2(0.78f, 1), new Vector2(0, 1), new Vector2(20, -148), new Vector2(0, 190));
            _tileIncome = StudioChrome.RecapTile(recap, "Income", "오늘 수입", Palette.CashGreen, 0f, 0.25f, 0.48f, 0.52f, true);
            var incomeTile = recap.Find("Income") as RectTransform;
            if (incomeTile != null)
            {
                var incomeImg = incomeTile.GetComponent<Image>();
                if (incomeImg != null)
                {
                    ArtSprites.Apply(incomeImg, ArtSprites.CashSlip, new Color(0.98f, 0.94f, 0.86f, 0.98f), Color.white);
                    incomeImg.preserveAspect = false;
                    incomeImg.raycastTarget = false;
                }
                var incomeCap = incomeTile.Find("L");
                if (incomeCap != null)
                {
                    var incomeCapT = incomeCap.GetComponent<Text>();
                    if (incomeCapT != null)
                        incomeCapT.color = Palette.Ink;
                }
            }
            if (_tileIncome != null)
                _tileIncome.color = Palette.Ink;
            _tileBills = StudioChrome.RecapTile(recap, "Bills", "청구", Palette.MoneyRed, 0.25f, 0.50f, 0.48f, 0.52f, false);
            _tileCash = StudioChrome.RecapTile(recap, "Cash", "현금", Palette.CashGreen, 0.50f, 0.75f, 0.48f, 0.52f, true);
            _tileDebt = StudioChrome.RecapTile(recap, "Debt", "부채", Palette.MoneyRed, 0.75f, 1f, 0.48f, 0.52f, false);
            _cashTile = recap.Find("Cash") as RectTransform;
            _debtTile = recap.Find("Debt") as RectTransform;
            if (_debtTile != null)
            {
                var debtImg = _debtTile.GetComponent<Image>();
                if (debtImg != null)
                    ArtSprites.ApplySliced(debtImg, ArtSprites.BillNotice, Color.white, new Vector4(28f, 24f, 28f, 24f));
            }
            _billsTile = recap.Find("Bills") as RectTransform;
            if (_billsTile != null)
            {
                _billsImg = _billsTile.GetComponent<Image>();
                var capT = _billsTile.Find("L");
                if (capT != null)
                    _billsCap = capT.GetComponent<Text>();
            }
            var shortHost = _billsTile != null ? _billsTile : recap;
            _shortStamp = UiKit.Image(shortHost, "ShortStamp", Color.white);
            UiKit.Layout(_shortStamp.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 0f), new Vector2(0f, 18f), new Vector2(240f, 72f));
            ArtSprites.Apply(_shortStamp, ArtSprites.BillShort, Palette.MoneyRed, Color.white);
            _shortStamp.preserveAspect = false;
            _shortStamp.raycastTarget = false;
            _shortStamp.gameObject.SetActive(false);
            _shortChip = UiKit.Label(_shortStamp.transform, "ShortChip", "", 22, Palette.MoneyRed, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(_shortChip.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0f, -1f), new Vector2(-12f, -8f));
            _shortChip.gameObject.SetActive(false);
            _tilePerfect = StudioChrome.RecapTile(recap, "Perfect", "PERFECT", Palette.Gold, 0f, 0.25f, 0f, 0.48f, true);
            _tileMiss = StudioChrome.RecapTile(recap, "Miss", "MISS", Palette.MoneyRed, 0.25f, 0.50f, 0f, 0.48f, false);
            _tileViewers = StudioChrome.RecapTile(recap, "Viewers", "시청자", Palette.Pink, 0.50f, 0.75f, 0f, 0.48f, true);
            _tileMental = StudioChrome.RecapTile(recap, "Mental", "멘탈", Palette.Pink, 0.75f, 1f, 0f, 0.48f, false);
            var mentalTile = recap.Find("Mental") as RectTransform;
            if (mentalTile != null)
            {
                var mentalImg = mentalTile.GetComponent<Image>();
                if (mentalImg != null)
                {
                    ArtSprites.Apply(mentalImg, ArtSprites.MentalNote, new Color(1f, 0.95f, 0.72f, 0.98f), Color.white);
                    mentalImg.preserveAspect = false;
                    mentalImg.raycastTarget = false;
                }
                var mentalCap = mentalTile.Find("L");
                if (mentalCap != null)
                {
                    var mentalCapT = mentalCap.GetComponent<Text>();
                    if (mentalCapT != null)
                        mentalCapT.color = Palette.Ink;
                }
            }
            if (_tileMental != null)
                _tileMental.color = Palette.Ink;
            _leftCashSlip = UiKit.Image(root, "LeftCashSlip", Color.white);
            UiKit.Layout(_leftCashSlip.rectTransform, new Vector2(0f, 1f), new Vector2(0.78f, 1f), new Vector2(0f, 1f), new Vector2(36f, -338f), new Vector2(0f, 52f));
            ArtSprites.Apply(_leftCashSlip, ArtSprites.CashSlip, new Color(0.98f, 0.94f, 0.86f, 0.98f), Color.white);
            _leftCashSlip.preserveAspect = false;
            _leftCashSlip.raycastTarget = false;
            _leftCash = UiKit.Label(_leftCashSlip.transform, "LeftCash", "남은 현금", 24, Palette.Pastel, TextAnchor.MiddleLeft, FontStyle.Bold);
            UiKit.Layout(_leftCash.rectTransform, new Vector2(0.08f, 0.14f), new Vector2(0.90f, 0.86f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            _leftCashShortStamp = UiKit.Image(_leftCashSlip.transform, "LeftCashShortStamp", Color.white);
            UiKit.Layout(_leftCashShortStamp.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 1f), new Vector2(0f, -2f), new Vector2(220f, 48f));
            ArtSprites.Apply(_leftCashShortStamp, ArtSprites.BillShort, Palette.MoneyRed, Color.white);
            _leftCashShortStamp.preserveAspect = false;
            _leftCashShortStamp.raycastTarget = false;
            _leftCashShortStamp.gameObject.SetActive(false);
            _leftCashShort = UiKit.Label(_leftCashShortStamp.transform, "LeftCashShort", "청구보다 부족", 15, Palette.MoneyRed, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(_leftCashShort.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0f, -1f), new Vector2(-10f, -6f));
            _leftCashShort.gameObject.SetActive(false);
            _leftCashSlip.gameObject.SetActive(false);

            var panel = UiKit.Panel(root, "Sheet", Color.white);
            UiKit.Layout(panel, new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(980, 200));
            SafeFitCard.Bind(panel, 980f, 220f, 16f);
            ArtSprites.ApplySliced(panel.GetComponent<Image>(), ArtSprites.PanelDark, new Color(1f, 1f, 1f, 0.92f));
            _body = UiKit.Label(panel, "Body", "", 18, Palette.Pastel, TextAnchor.UpperLeft);
            UiKit.Stretch(_body.rectTransform, 22, 22, 16, 16);
            _body.horizontalOverflow = HorizontalWrapMode.Wrap;
            _body.verticalOverflow = VerticalWrapMode.Overflow;
            _body.lineSpacing = 1.08f;

            _result = UiKit.Label(root, "Result", "", 30, Palette.Gold, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(_result.rectTransform, new Vector2(0.04f, 0), new Vector2(0.96f, 0), new Vector2(0.5f, 0), new Vector2(0, 168), new Vector2(0, 50));
            UiKit.Wrap(_result);

            _offerRow = UiKit.Panel(root, "OfferRow", new Color(0, 0, 0, 0));
            UiKit.Layout(_offerRow, new Vector2(0.04f, 0), new Vector2(0.96f, 0), new Vector2(0.5f, 0), new Vector2(0, 118), new Vector2(0, 72));
            _clipYes = UiKit.Button(_offerRow, "ClipYes", "클립 업로드", OnClipYes, Palette.Gold, Palette.Ink);
            UiKit.Layout(_clipYes.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-190, 0), new Vector2(320, 56));
            _clipNo = UiKit.Button(_offerRow, "ClipNo", "올리지 않기", OnClipNo, Palette.StudioHi, Palette.Pastel);
            UiKit.Layout(_clipNo.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(190, 0), new Vector2(320, 56));
            _clipNote = UiKit.Label(root, "ClipNote", "", 18, Palette.Gold, TextAnchor.MiddleCenter);
            UiKit.Layout(_clipNote.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 118), new Vector2(720, 28));
            UiKit.Wrap(_clipNote);

            _produce = UiKit.Button(_offerRow, "Produce", "아크릴 1개 생산  ₩2,500", OnProduce, Palette.Gold, Palette.Ink);
            UiKit.Layout(_produce.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 0), new Vector2(360, 56));
            _produce.gameObject.SetActive(false);

            _foundAgency = UiKit.Button(_offerRow, "FoundAgency", "에이전시 설립  ₩40,000", OnFoundAgency, Palette.Gold, Palette.Ink);
            UiKit.Layout(_foundAgency.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-360, 0), new Vector2(300, 56));
            _foundAgency.gameObject.SetActive(false);
            _scout = UiKit.Button(_offerRow, "Scout", "주니어 스카우트  ₩25,000", OnScout, Palette.PinkDeep, Color.white);
            UiKit.Layout(_scout.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 0), new Vector2(300, 56));
            _scout.gameObject.SetActive(false);
            _signSponsor = UiKit.Button(_offerRow, "Sponsor", "스폰서 계약", OnSignSponsor, Palette.Gold, Palette.Ink);
            UiKit.Layout(_signSponsor.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(360, 0), new Vector2(300, 56));
            _signSponsor.gameObject.SetActive(false);
            SafePairLayout.BindMany(_offerRow, false, true,
                _clipYes.GetComponent<RectTransform>(),
                _clipNo.GetComponent<RectTransform>(),
                _produce.GetComponent<RectTransform>(),
                _foundAgency.GetComponent<RectTransform>(),
                _scout.GetComponent<RectTransform>(),
                _signSponsor.GetComponent<RectTransform>());

            _week5Row = UiKit.Panel(root, "Week5Row", new Color(0, 0, 0, 0));
            UiKit.Layout(_week5Row, new Vector2(0.04f, 0), new Vector2(0.96f, 0), new Vector2(0.5f, 0), new Vector2(0, 200), new Vector2(0, 72));
            _bookConcert = UiKit.Button(_week5Row, "BookConcert", "콘서트 개최  ₩80,000", OnBookConcert, Palette.Gold, Palette.Ink);
            UiKit.Layout(_bookConcert.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-210, 0), new Vector2(360, 56));
            _bookConcert.gameObject.SetActive(false);
            _concertLive = UiKit.Button(_week5Row, "ConcertLive", "콘서트 방송", OnConcertLive, Palette.PinkDeep, Color.white);
            UiKit.Layout(_concertLive.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(210, 0), new Vector2(360, 56));
            _concertLive.gameObject.SetActive(false);
            SafePairLayout.Bind(_week5Row, _bookConcert.GetComponent<RectTransform>(), _concertLive.GetComponent<RectTransform>());

            _letter = UiKit.Button(root, "FanLetter", "팬레터 답장", OnLetter, Palette.PinkDeep, Color.white);
            UiKit.Layout(_letter.GetComponent<RectTransform>(), new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(48, 52), new Vector2(240, 56));
            _letter.gameObject.SetActive(false);
            _auto = UiKit.Button(root, "AutoReply", "기본 자동응답", OnToggleAuto, Palette.Gold, Palette.Ink);
            UiKit.Layout(_auto.GetComponent<RectTransform>(), new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0), new Vector2(-48, 52), new Vector2(240, 56));
            _soothe = UiKit.Button(root, "Soothe", "특별방송으로 달래기", OnSootheConflict, Palette.PinkDeep, Color.white);
            UiKit.Layout(_soothe.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-190, 184), new Vector2(320, 56));
            _style = UiKit.Button(root, "Style", "내 스타일대로", OnStyleConflict, Palette.Troll, Color.white);
            UiKit.Layout(_style.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(190, 184), new Vector2(320, 56));
            _soothe.gameObject.SetActive(false);
            _style.gameObject.SetActive(false);

            _conflictOverlay = new GameObject("ConflictRoot", typeof(RectTransform));
            _conflictOverlay.transform.SetParent(root, false);
            UiKit.Stretch(_conflictOverlay.GetComponent<RectTransform>());
            var conflictWash = UiKit.Image(_conflictOverlay.transform, "ConflictWash", new Color(0.08f, 0.03f, 0.08f, 0.84f));
            UiKit.Stretch(conflictWash.rectTransform);
            conflictWash.raycastTarget = true;
            var conflictTitle = UiKit.Label(_conflictOverlay.transform, "CTitle", "콘텐츠 편중 갈등", 42, Palette.Gold, TextAnchor.UpperCenter, FontStyle.Bold);
            UiKit.Layout(conflictTitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -16), new Vector2(920, 52));
            var conflictHint = UiKit.Label(_conflictOverlay.transform, "CBody", "오늘 안에 고르세요.", 22, Palette.Pastel, TextAnchor.UpperCenter);
            UiKit.Layout(conflictHint.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -64), new Vector2(720, 32));
            var conflictPair = UiKit.Panel(_conflictOverlay.transform, "ConflictPair", new Color(0, 0, 0, 0));
            UiKit.Layout(conflictPair, new Vector2(0.04f, 0.14f), new Vector2(0.96f, 0.86f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            _conflictSoothe = UiKit.Button(conflictPair, "ConflictSoothe", "특별방송으로 달래기", OnSootheConflict, Palette.PinkDeep, Color.white);
            UiKit.Layout(_conflictSoothe.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-300, 8), new Vector2(500, 340));
            ArtSprites.ApplySliced(_conflictSoothe.GetComponent<Image>(), ArtSprites.PanelDark, new Color(1f, 0.78f, 0.88f, 0.98f));
            StyleConflictCard(_conflictSoothe);
            _conflictStyle = UiKit.Button(conflictPair, "ConflictStyle", "내 스타일대로", OnStyleConflict, Palette.Troll, Color.white);
            UiKit.Layout(_conflictStyle.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(300, 8), new Vector2(500, 340));
            ArtSprites.ApplySliced(_conflictStyle.GetComponent<Image>(), ArtSprites.PanelDark, new Color(0.92f, 0.42f, 0.48f, 0.98f));
            StyleConflictCard(_conflictStyle);
            var settleConflictPair = SafePairLayout.Bind(conflictPair, _conflictSoothe.GetComponent<RectTransform>(), _conflictStyle.GetComponent<RectTransform>(), true, false);
            settleConflictPair.MinEach = 480f;
            _conflictOverlayResult = UiKit.Label(_conflictOverlay.transform, "CResult", "", 30, Palette.Gold, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(_conflictOverlayResult.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 16), new Vector2(1100, 48));
            UiKit.Wrap(_conflictOverlayResult);
            _conflictOverlay.SetActive(false);

            _autoRoot = new GameObject("AutoRoot", typeof(RectTransform));
            _autoRoot.transform.SetParent(root, false);
            UiKit.Stretch(_autoRoot.GetComponent<RectTransform>());
            var autoWash = UiKit.Image(_autoRoot.transform, "AutoWash", new Color(0.08f, 0.05f, 0.02f, 0.78f));
            UiKit.Stretch(autoWash.rectTransform);
            autoWash.raycastTarget = true;
            var autoCard = UiKit.Panel(_autoRoot.transform, "AutoCard", Color.white);
            UiKit.Layout(autoCard, new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(720, 380));
            ArtSprites.ApplySliced(autoCard.GetComponent<Image>(), ArtSprites.PanelDark, new Color(1f, 0.92f, 0.55f, 0.98f));
            SafeFitCard.Bind(autoCard, 720f, 380f);
            var autoTitle = UiKit.Label(autoCard, "AutoTitle", "기본 자동응답", 46, Palette.Gold, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(autoTitle.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -28), new Vector2(-40, 70));
            _autoBody = UiKit.Label(autoCard, "AutoBody", "", 26, Palette.Ink, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(_autoBody.rectTransform, new Vector2(0.08f, 0.28f), new Vector2(0.92f, 0.72f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            UiKit.Wrap(_autoBody);
            var autoOn = UiKit.Button(autoCard, "AutoOn", "켜기", OnAutoOn, Palette.Gold, Palette.Ink);
            UiKit.Layout(autoOn.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-170, 28), new Vector2(300, 72));
            var autoOff = UiKit.Button(autoCard, "AutoOff", "끄기", OnAutoOff, Palette.StudioHi, Palette.Pastel);
            UiKit.Layout(autoOff.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(170, 28), new Vector2(300, 72));
            SafePairLayout.Bind(autoCard, autoOn.GetComponent<RectTransform>(), autoOff.GetComponent<RectTransform>());
            _autoRoot.SetActive(false);

            _rankBox = UiKit.Panel(root, "RankPanel", Color.white);
            UiKit.Layout(_rankBox, new Vector2(1, 0.58f), new Vector2(1, 0.58f), new Vector2(1, 0.5f), new Vector2(-16, 0), new Vector2(360, 340));
            var rankImg = _rankBox.GetComponent<Image>();
            ArtSprites.Apply(rankImg, ArtSprites.RankingBoard, new Color(0.10f, 0.05f, 0.12f, 0.94f), Color.white);
            rankImg.preserveAspect = false;
            _rankPanel = UiKit.Label(_rankBox, "RankBody", "", 20, Palette.Pastel, TextAnchor.UpperLeft, FontStyle.Bold);
            UiKit.Stretch(_rankPanel.rectTransform, 16, 16, 14, 14);
            _rankPanel.lineSpacing = 1.2f;
            UiKit.Wrap(_rankPanel);
            _rankBox.gameObject.SetActive(false);

            _actionRow = UiKit.Panel(root, "ActionRow", new Color(0, 0, 0, 0));
            UiKit.Layout(_actionRow, new Vector2(0.04f, 0), new Vector2(0.96f, 0), new Vector2(0.5f, 0), new Vector2(0, 16), new Vector2(0, 68));
            _repay = UiKit.Button(_actionRow, "Repay", "남은 현금으로 빚 갚기", OnRepay, Palette.Gold, Palette.Ink);
            UiKit.Layout(_repay.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-210, 0), new Vector2(360, 60));

            _next = UiKit.Button(_actionRow, "Next", "다음날  (Space)", () => { PlayNextDaySfx(); LeaveSettle(() => GameManager.Instance.NextMorning()); }, Palette.PinkDeep, Color.white);
            _nextRt = _next.GetComponent<RectTransform>();
            UiKit.Layout(_nextRt, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(210, 0), new Vector2(360, 60));
            var nextImg = _next.GetComponent<Image>();
            if (nextImg != null)
            {
                ArtSprites.ApplySliced(nextImg, ArtSprites.NextDayKey, Color.white, new Vector4(48f, 36f, 48f, 36f));
                nextImg.raycastTarget = true;
            }
            _nextChip = UiKit.Panel(_next.transform, "NextChip", Palette.Gold);
            UiKit.Layout(_nextChip, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(28f, 0f), new Vector2(52f, 26f));
            var chipImg = _nextChip.GetComponent<Image>();
            if (chipImg != null)
                chipImg.raycastTarget = false;
            var chipT = UiKit.Label(_nextChip, "T", "다음", 14, Palette.Ink, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Stretch(chipT.rectTransform);
            var nextCap = _next.transform.Find("Caption") as RectTransform;
            if (nextCap != null)
                nextCap.offsetMin = new Vector2(56f, 0f);

            _restart = UiKit.Button(_actionRow, "Restart", "처음부터", () => LeaveSettle(() => GameManager.Instance.RestartRun()), Palette.Troll, Color.white);
            UiKit.Layout(_restart.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 0), new Vector2(360, 60));
            SafePairLayout.BindMany(_actionRow, false, true, _repay.GetComponent<RectTransform>(), _next.GetComponent<RectTransform>(), _restart.GetComponent<RectTransform>());

            _endingRoot = new GameObject("EndingRoot", typeof(RectTransform));
            _endingRoot.transform.SetParent(root, false);
            UiKit.Stretch(_endingRoot.GetComponent<RectTransform>());
            var endingWash = UiKit.Image(_endingRoot.transform, "EndingWash", new Color(0.06f, 0.03f, 0.08f, 0.94f));
            UiKit.Stretch(endingWash.rectTransform);
            var endingCard = UiKit.Panel(_endingRoot.transform, "EndingCard", Color.white);
            UiKit.Layout(endingCard, new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1080, 460));
            ArtSprites.ApplySliced(endingCard.GetComponent<Image>(), ArtSprites.PanelDark, new Color(1f, 1f, 1f, 0.98f));
            SafeFitCard.Bind(endingCard, 1080f, 520f);
            _endingPortrait = new StudioPortrait(endingCard, new Vector2(0.18f, 0.52f), new Vector2(320, 400), false);
            _endingTitle = UiKit.Label(endingCard, "ETitle", "", 52, Palette.Gold, TextAnchor.UpperLeft, FontStyle.Bold);
            UiKit.Layout(_endingTitle.rectTransform, new Vector2(0.38f, 1), new Vector2(1, 1), new Vector2(0, 1), new Vector2(12, -36), new Vector2(-40, 72));
            _endingHeadline = UiKit.Label(endingCard, "EHeadline", "", 22, Palette.Gold, TextAnchor.UpperLeft, FontStyle.Bold);
            UiKit.Layout(_endingHeadline.rectTransform, new Vector2(0.38f, 1), new Vector2(1, 1), new Vector2(0, 1), new Vector2(12, -108), new Vector2(-48, 44));
            UiKit.Wrap(_endingHeadline);
            _endingBody = UiKit.Label(endingCard, "EBody", "", 24, Palette.Pastel, TextAnchor.UpperLeft);
            UiKit.Layout(_endingBody.rectTransform, new Vector2(0.38f, 0.22f), new Vector2(1, 0.62f), new Vector2(0, 1), new Vector2(12, 0), new Vector2(-48, 0));
            _endingBody.horizontalOverflow = HorizontalWrapMode.Wrap;
            _endingBody.lineSpacing = 1.25f;
            _retire = UiKit.Button(endingCard, "Retire", "후배에게 메인 양도", OnRetire, Palette.Gold, Palette.Ink);
            UiKit.Layout(_retire.GetComponent<RectTransform>(), new Vector2(0.68f, 0), new Vector2(0.68f, 0), new Vector2(0.5f, 0), new Vector2(-170, 28), new Vector2(300, 56));
            var endingRestart = UiKit.Button(endingCard, "EndingRestart", "처음부터", () => LeaveSettle(() => GameManager.Instance.RestartRun()), Palette.PinkDeep, Color.white);
            UiKit.Layout(endingRestart.GetComponent<RectTransform>(), new Vector2(0.68f, 0), new Vector2(0.68f, 0), new Vector2(0.5f, 0), new Vector2(150, 28), new Vector2(300, 56));
            SafePairLayout.Bind(endingCard, _retire.GetComponent<RectTransform>(), endingRestart.GetComponent<RectTransform>());
            _endingRoot.SetActive(false);

            _clearRoot = new GameObject("ClearRoot", typeof(RectTransform));
            _clearRoot.transform.SetParent(root, false);
            UiKit.Stretch(_clearRoot.GetComponent<RectTransform>());
            var clearWash = UiKit.Image(_clearRoot.transform, "ClearWash", Color.white);
            UiKit.Stretch(clearWash.rectTransform);
            ArtSprites.Apply(clearWash, ArtSprites.EndingClear, new Color(0.08f, 0.16f, 0.12f, 0.96f), Color.white);
            clearWash.preserveAspect = false;
            clearWash.raycastTarget = false;
            var clearGlow = UiKit.Image(_clearRoot.transform, "ClearGlow", new Color(1f, 0.82f, 0.25f, 0.16f));
            UiKit.Layout(clearGlow.rectTransform, new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(900, 900));
            var clearTag = UiKit.Label(_clearRoot.transform, "ClearTag", "주차 클리어", 28, Palette.Gold, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(clearTag.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -36), new Vector2(480, 40));
            _clearTitle = UiKit.Label(_clearRoot.transform, "ClearTitle", "1주차 생존", 72, Palette.Pastel, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(_clearTitle.rectTransform, new Vector2(0.04f, 1), new Vector2(0.96f, 1), new Vector2(0.5f, 1), new Vector2(0, -100), new Vector2(0, 90));
            UiKit.Wrap(_clearTitle);
            _clearHeadline = UiKit.Label(_clearRoot.transform, "ClearHeadline", "", 28, Palette.Gold, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(_clearHeadline.rectTransform, new Vector2(0.06f, 1), new Vector2(0.94f, 1), new Vector2(0.5f, 1), new Vector2(0, -188), new Vector2(0, 48));
            UiKit.Wrap(_clearHeadline);
            _clearPortrait = new StudioPortrait(_clearRoot.transform, new Vector2(0.5f, 0.46f), new Vector2(340, 420), false);
            var snap = UiKit.Panel(_clearRoot.transform, "ClearSnap", Color.white);
            UiKit.Layout(snap, new Vector2(0.08f, 0), new Vector2(0.92f, 0), new Vector2(0.5f, 0), new Vector2(0, 156), new Vector2(0, 88));
            ArtSprites.ApplySliced(snap.GetComponent<Image>(), ArtSprites.PanelDark, new Color(1f, 1f, 1f, 0.94f));
            _clearCash = UiKit.Label(snap, "C", "현금 ₩0", 28, Palette.CashGreen, TextAnchor.MiddleLeft, FontStyle.Bold);
            UiKit.Layout(_clearCash.rectTransform, new Vector2(0, 0), new Vector2(0.5f, 1), new Vector2(0, 0.5f), new Vector2(24, 0), new Vector2(-16, 0));
            _clearDebt = UiKit.Label(snap, "D", "부채 ₩0", 28, Palette.MoneyRed, TextAnchor.MiddleRight, FontStyle.Bold);
            UiKit.Layout(_clearDebt.rectTransform, new Vector2(0.5f, 0), new Vector2(1, 1), new Vector2(1, 0.5f), new Vector2(-24, 0), new Vector2(-16, 0));
            var clearGo = UiKit.Button(_clearRoot.transform, "ClearGo", "다음 주차 시작", () => LeaveSettle(() => GameManager.Instance.NextMorning()), Palette.Gold, Palette.Ink);
            UiKit.Layout(clearGo.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 48), new Vector2(420, 72));
            _clearRoot.SetActive(false);

            _stampRoot = new GameObject("StampRoot", typeof(RectTransform));
            _stampRoot.transform.SetParent(root, false);
            UiKit.Stretch(_stampRoot.GetComponent<RectTransform>());
            _stampWash = UiKit.Image(_stampRoot.transform, "StampWash", Color.white);
            UiKit.Stretch(_stampWash.rectTransform);
            ArtSprites.Apply(_stampWash, ArtSprites.EndingBankrupt, new Color(0.42f, 0.04f, 0.10f, 0.97f), Color.white);
            _stampWash.preserveAspect = false;
            _stampWash.raycastTarget = false;
            _stampPortrait = new StudioPortrait(_stampRoot.transform, new Vector2(0.18f, 0.50f), new Vector2(320, 400), false);
            _stampMark = UiKit.Label(_stampRoot.transform, "StampMark", "파산", 120, Palette.MoneyRed, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(_stampMark.rectTransform, new Vector2(0.58f, 0.62f), new Vector2(0.58f, 0.62f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(720, 160));
            _stampMark.rectTransform.localEulerAngles = new Vector3(0f, 0f, -8f);
            _stampDebt = UiKit.Label(_stampRoot.transform, "StampDebt", "", 36, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(_stampDebt.rectTransform, new Vector2(0.58f, 0.38f), new Vector2(0.58f, 0.38f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(720, 48));
            _stampEpitaph = UiKit.Label(_stampRoot.transform, "StampEpitaph", "", 22, Palette.Pastel, TextAnchor.MiddleCenter);
            UiKit.Layout(_stampEpitaph.rectTransform, new Vector2(0.58f, 0.28f), new Vector2(0.58f, 0.28f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(760, 64));
            _stampEpitaph.horizontalOverflow = HorizontalWrapMode.Wrap;
            _stampHeadline = UiKit.Label(_stampRoot.transform, "StampHeadline", "", 24, Palette.Gold, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(_stampHeadline.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 132), new Vector2(720, 44));
            UiKit.Wrap(_stampHeadline);
            var stampRestart = UiKit.Button(_stampRoot.transform, "StampRestart", "처음부터", () => LeaveSettle(() => GameManager.Instance.RestartRun()), Palette.Ink, Palette.Pastel);
            UiKit.Layout(stampRestart.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 48), new Vector2(360, 72));
            _stampRoot.SetActive(false);

            _letterRoot = new GameObject("LetterRoot", typeof(RectTransform));
            _letterRoot.transform.SetParent(root, false);
            UiKit.Stretch(_letterRoot.GetComponent<RectTransform>());
            var letterWash = UiKit.Image(_letterRoot.transform, "LetterWash", new Color(0.08f, 0.04f, 0.1f, 0.72f));
            UiKit.Stretch(letterWash.rectTransform);
            letterWash.raycastTarget = true;
            var paper = UiKit.Panel(_letterRoot.transform, "LetterCard", Color.white);
            UiKit.Layout(paper, new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(720, 440));
            var paperImg = paper.GetComponent<Image>();
            ArtSprites.Apply(paperImg, ArtSprites.LetterCard, new Color(1f, 0.92f, 0.94f, 0.98f), Color.white);
            paperImg.preserveAspect = false;
            SafeFitCard.Bind(paper, 720f, 440f);
            var letterTitle = UiKit.Label(paper, "LetterTitle", "팬레터", 22, Palette.Pink, TextAnchor.UpperLeft, FontStyle.Bold);
            UiKit.Layout(letterTitle.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1), new Vector2(28, -16), new Vector2(-56, 28));
            _letterFrom = UiKit.Label(paper, "LetterFrom", "", 40, Palette.Ink, TextAnchor.UpperLeft, FontStyle.Bold);
            UiKit.Layout(_letterFrom.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1), new Vector2(28, -48), new Vector2(-56, 48));
            _letterTag = UiKit.Label(paper, "LetterTag", "", 18, Palette.PinkDeep, TextAnchor.UpperLeft, FontStyle.Bold);
            UiKit.Layout(_letterTag.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1), new Vector2(28, -96), new Vector2(-56, 24));
            _letterBody = UiKit.Label(paper, "LetterBody", "", 22, Palette.Ink, TextAnchor.UpperLeft);
            UiKit.Layout(_letterBody.rectTransform, new Vector2(0, 0.28f), new Vector2(1, 0.72f), new Vector2(0, 1), new Vector2(28, 0), new Vector2(-56, 0));
            _letterBody.horizontalOverflow = HorizontalWrapMode.Wrap;
            _letterBody.lineSpacing = 1.22f;
            var reply = UiKit.Button(paper, "Reply", "답장하기", OnLetter, Palette.PinkDeep, Color.white);
            UiKit.Layout(reply.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-170, 28), new Vector2(300, 72));
            var replyImg = reply.GetComponent<Image>();
            if (replyImg != null)
            {
                ArtSprites.ApplySliced(replyImg, ArtSprites.LetterReply, Color.white, new Vector4(48f, 36f, 48f, 36f));
                replyImg.raycastTarget = true;
            }
            var later = UiKit.Button(paper, "Later", "나중에", OnLetterLater, Palette.StudioHi, Palette.Pastel);
            UiKit.Layout(later.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(170, 28), new Vector2(300, 72));
            var laterImg = later.GetComponent<Image>();
            if (laterImg != null)
            {
                ArtSprites.ApplySliced(laterImg, ArtSprites.LetterIgnore, Color.white, new Vector4(48f, 36f, 48f, 36f));
                laterImg.raycastTarget = true;
            }
            SafePairLayout.Bind(paper, reply.GetComponent<RectTransform>(), later.GetComponent<RectTransform>());
            _letterHeart = UiKit.Label(root, "LetterHeart", "", 36, Palette.Pink, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(_letterHeart.rectTransform, new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(640, 56));
            var heartC = _letterHeart.color;
            heartC.a = 0f;
            _letterHeart.color = heartC;
            _letterRoot.SetActive(false);

            _memberRoot = new GameObject("MemberRoot", typeof(RectTransform));
            _memberRoot.transform.SetParent(root, false);
            UiKit.Stretch(_memberRoot.GetComponent<RectTransform>());
            var memberWash = UiKit.Image(_memberRoot.transform, "MemberWash", new Color(0.08f, 0.05f, 0.02f, 0.78f));
            UiKit.Stretch(memberWash.rectTransform);
            memberWash.raycastTarget = true;
            var memberCard = UiKit.Panel(_memberRoot.transform, "MemberCard", Color.white);
            UiKit.Layout(memberCard, new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(720, 380));
            var memberImg = memberCard.GetComponent<Image>();
            ArtSprites.Apply(memberImg, ArtSprites.MembershipCard, new Color(1f, 0.92f, 0.55f, 0.98f), Color.white);
            memberImg.preserveAspect = false;
            SafeFitCard.Bind(memberCard, 720f, 380f);
            var memberTitle = UiKit.Label(memberCard, "MemberTitle", "멤버십 해금", 52, Palette.Gold, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(memberTitle.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -28), new Vector2(-40, 70));
            _memberBody = UiKit.Label(memberCard, "MemberBody", "", 26, Palette.Ink, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(_memberBody.rectTransform, new Vector2(0.08f, 0.28f), new Vector2(0.92f, 0.72f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            _memberBody.horizontalOverflow = HorizontalWrapMode.Wrap;
            _memberBody.lineSpacing = 1.25f;
            var memberGo = UiKit.Button(memberCard, "MemberAck", "정산으로", OnMemberAck, Palette.Gold, Palette.Ink);
            UiKit.Layout(memberGo.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 28), new Vector2(320, 72));
            _memberRoot.SetActive(false);

            _clipRoot = new GameObject("ClipRoot", typeof(RectTransform));
            _clipRoot.transform.SetParent(root, false);
            UiKit.Stretch(_clipRoot.GetComponent<RectTransform>());
            var clipWash = UiKit.Image(_clipRoot.transform, "ClipWash", new Color(0.06f, 0.04f, 0.1f, 0.76f));
            UiKit.Stretch(clipWash.rectTransform);
            clipWash.raycastTarget = true;
            var clipCard = UiKit.Panel(_clipRoot.transform, "ClipCard", Color.white);
            UiKit.Layout(clipCard, new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(720, 360));
            var clipImg = clipCard.GetComponent<Image>();
            ArtSprites.Apply(clipImg, ArtSprites.ClipCard, new Color(1f, 0.94f, 0.72f, 0.98f), Color.white);
            clipImg.preserveAspect = false;
            SafeFitCard.Bind(clipCard, 720f, 360f);
            var clipTag = UiKit.Label(clipCard, "ClipTag", "클립 업로드", 22, Palette.Gold, TextAnchor.UpperCenter, FontStyle.Bold);
            UiKit.Layout(clipTag.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -20), new Vector2(-40, 28));
            var clipAsk = UiKit.Label(clipCard, "ClipAsk", "오늘 클립 올릴까?", 44, Palette.Ink, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(clipAsk.rectTransform, new Vector2(0.08f, 0.38f), new Vector2(0.92f, 0.78f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            var clipYes = UiKit.Button(clipCard, "ClipGo", "올린다", OnClipYes, Palette.Gold, Palette.Ink);
            UiKit.Layout(clipYes.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-170, 28), new Vector2(300, 72));
            var clipNo = UiKit.Button(clipCard, "ClipPass", "패스", OnClipNo, Palette.StudioHi, Palette.Pastel);
            UiKit.Layout(clipNo.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(170, 28), new Vector2(300, 72));
            SafePairLayout.Bind(clipCard, clipYes.GetComponent<RectTransform>(), clipNo.GetComponent<RectTransform>());
            _clipSlam = UiKit.Label(root, "ClipSlam", "", 56, Palette.Gold, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(_clipSlam.rectTransform, new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(820, 80));
            var slamC = _clipSlam.color;
            slamC.a = 0f;
            _clipSlam.color = slamC;
            _clipRoot.SetActive(false);

            _goodsRoot = new GameObject("GoodsRoot", typeof(RectTransform));
            _goodsRoot.transform.SetParent(root, false);
            UiKit.Stretch(_goodsRoot.GetComponent<RectTransform>());
            var goodsWash = UiKit.Image(_goodsRoot.transform, "GoodsWash", new Color(0.08f, 0.04f, 0.1f, 0.78f));
            UiKit.Stretch(goodsWash.rectTransform);
            goodsWash.raycastTarget = true;
            var goodsCard = UiKit.Panel(_goodsRoot.transform, "GoodsCard", Color.white);
            UiKit.Layout(goodsCard, new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(720, 400));
            ArtSprites.ApplySliced(goodsCard.GetComponent<Image>(), ArtSprites.PanelDark, new Color(1f, 0.86f, 0.94f, 0.98f));
            SafeFitCard.Bind(goodsCard, 720f, 400f);
            var goodsTitle = UiKit.Label(goodsCard, "GoodsTitle", "아크릴 스탠드 해금", 46, Palette.Gold, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(goodsTitle.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -28), new Vector2(-40, 70));
            var goodsIcon = UiKit.Image(goodsCard, "GoodsStand", Color.white);
            UiKit.Layout(goodsIcon.rectTransform, new Vector2(0.5f, 0.62f), new Vector2(0.5f, 0.62f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(148, 148));
            ArtSprites.Apply(goodsIcon, ArtSprites.GoodsStand, Color.white, Color.white);
            goodsIcon.preserveAspect = true;
            goodsIcon.raycastTarget = false;
            _goodsBody = UiKit.Label(goodsCard, "GoodsBody", "", 26, Palette.Ink, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(_goodsBody.rectTransform, new Vector2(0.08f, 0.22f), new Vector2(0.92f, 0.48f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            _goodsBody.horizontalOverflow = HorizontalWrapMode.Wrap;
            _goodsBody.lineSpacing = 1.25f;
            var goodsGo = UiKit.Button(goodsCard, "GoodsAck", "정산으로", OnGoodsAck, Palette.Gold, Palette.Ink);
            UiKit.Layout(goodsGo.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 28), new Vector2(320, 72));
            _goodsRoot.SetActive(false);

            _agencyRoot = new GameObject("AgencyRoot", typeof(RectTransform));
            _agencyRoot.transform.SetParent(root, false);
            UiKit.Stretch(_agencyRoot.GetComponent<RectTransform>());
            var agencyWash = UiKit.Image(_agencyRoot.transform, "AgencyWash", new Color(0.08f, 0.05f, 0.02f, 0.78f));
            UiKit.Stretch(agencyWash.rectTransform);
            agencyWash.raycastTarget = true;
            var agencyCard = UiKit.Panel(_agencyRoot.transform, "AgencyCard", Color.white);
            UiKit.Layout(agencyCard, new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(720, 400));
            var agencyImg = agencyCard.GetComponent<Image>();
            ArtSprites.Apply(agencyImg, ArtSprites.AgencyCard, new Color(1f, 0.92f, 0.55f, 0.98f), Color.white);
            agencyImg.preserveAspect = false;
            SafeFitCard.Bind(agencyCard, 720f, 400f);
            var agencyTitle = UiKit.Label(agencyCard, "AgencyTitle", "에이전시 설립", 46, Palette.Gold, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(agencyTitle.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -28), new Vector2(-40, 70));
            _agencyBody = UiKit.Label(agencyCard, "AgencyBody", "", 26, Palette.Ink, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(_agencyBody.rectTransform, new Vector2(0.08f, 0.28f), new Vector2(0.92f, 0.72f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            _agencyBody.horizontalOverflow = HorizontalWrapMode.Wrap;
            _agencyBody.lineSpacing = 1.25f;
            var agencyYes = UiKit.Button(agencyCard, "AgencyGo", "설립", OnAgencyYes, Palette.Gold, Palette.Ink);
            UiKit.Layout(agencyYes.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-170, 28), new Vector2(300, 72));
            var agencyNo = UiKit.Button(agencyCard, "AgencyLater", "나중에", OnAgencyLater, Palette.StudioHi, Palette.Pastel);
            UiKit.Layout(agencyNo.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(170, 28), new Vector2(300, 72));
            SafePairLayout.Bind(agencyCard, agencyYes.GetComponent<RectTransform>(), agencyNo.GetComponent<RectTransform>());
            _agencyRoot.SetActive(false);

            _agencySplashRoot = new GameObject("AgencySplashRoot", typeof(RectTransform));
            _agencySplashRoot.transform.SetParent(root, false);
            UiKit.Stretch(_agencySplashRoot.GetComponent<RectTransform>());
            var agencyOpenWash = UiKit.Image(_agencySplashRoot.transform, "AgencyOpenWash", new Color(0.08f, 0.05f, 0.02f, 0.78f));
            UiKit.Stretch(agencyOpenWash.rectTransform);
            agencyOpenWash.raycastTarget = true;
            var agencyOpenCard = UiKit.Panel(_agencySplashRoot.transform, "AgencyOpenCard", Color.white);
            UiKit.Layout(agencyOpenCard, new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(720, 380));
            var agencyOpenImg = agencyOpenCard.GetComponent<Image>();
            ArtSprites.Apply(agencyOpenImg, ArtSprites.AgencyCard, new Color(1f, 0.9f, 0.5f, 0.98f), Color.white);
            agencyOpenImg.preserveAspect = false;
            SafeFitCard.Bind(agencyOpenCard, 720f, 380f);
            var agencyOpenTitle = UiKit.Label(agencyOpenCard, "AgencyOpenTitle", "에이전시 오픈", 52, Palette.Gold, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(agencyOpenTitle.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -28), new Vector2(-40, 70));
            _agencySplashBody = UiKit.Label(agencyOpenCard, "AgencyOpenBody", "", 26, Palette.Ink, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(_agencySplashBody.rectTransform, new Vector2(0.08f, 0.28f), new Vector2(0.92f, 0.72f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            _agencySplashBody.horizontalOverflow = HorizontalWrapMode.Wrap;
            _agencySplashBody.lineSpacing = 1.25f;
            var agencyOpenGo = UiKit.Button(agencyOpenCard, "AgencyOpenAck", "정산으로", OnAgencySplashAck, Palette.Gold, Palette.Ink);
            UiKit.Layout(agencyOpenGo.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 28), new Vector2(320, 72));
            _agencySplashRoot.SetActive(false);

            _juniorRoot = new GameObject("JuniorRoot", typeof(RectTransform));
            _juniorRoot.transform.SetParent(root, false);
            UiKit.Stretch(_juniorRoot.GetComponent<RectTransform>());
            var juniorWash = UiKit.Image(_juniorRoot.transform, "JuniorWash", new Color(0.08f, 0.04f, 0.1f, 0.76f));
            UiKit.Stretch(juniorWash.rectTransform);
            juniorWash.raycastTarget = true;
            var juniorCard = UiKit.Panel(_juniorRoot.transform, "JuniorCard", Color.white);
            UiKit.Layout(juniorCard, new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(720, 360));
            var juniorImg = juniorCard.GetComponent<Image>();
            ArtSprites.Apply(juniorImg, ArtSprites.AgencyCard, new Color(1f, 0.86f, 0.94f, 0.98f), Color.white);
            juniorImg.preserveAspect = false;
            SafeFitCard.Bind(juniorCard, 720f, 360f);
            var juniorTitle = UiKit.Label(juniorCard, "JuniorTitle", "후배 스카우트", 46, Palette.Gold, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(juniorTitle.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -28), new Vector2(-40, 70));
            _juniorBody = UiKit.Label(juniorCard, "JuniorBody", "", 26, Palette.Ink, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(_juniorBody.rectTransform, new Vector2(0.08f, 0.28f), new Vector2(0.92f, 0.72f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            var juniorYes = UiKit.Button(juniorCard, "JuniorGo", "스카우트", OnJuniorYes, Palette.PinkDeep, Color.white);
            UiKit.Layout(juniorYes.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-170, 28), new Vector2(300, 72));
            var juniorNo = UiKit.Button(juniorCard, "JuniorLater", "나중에", OnJuniorLater, Palette.StudioHi, Palette.Pastel);
            UiKit.Layout(juniorNo.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(170, 28), new Vector2(300, 72));
            SafePairLayout.Bind(juniorCard, juniorYes.GetComponent<RectTransform>(), juniorNo.GetComponent<RectTransform>());
            _juniorRoot.SetActive(false);

            _concertRoot = new GameObject("ConcertBookRoot", typeof(RectTransform));
            _concertRoot.transform.SetParent(root, false);
            UiKit.Stretch(_concertRoot.GetComponent<RectTransform>());
            var concertWash = UiKit.Image(_concertRoot.transform, "ConcertWash", new Color(0.08f, 0.04f, 0.1f, 0.78f));
            UiKit.Stretch(concertWash.rectTransform);
            concertWash.raycastTarget = true;
            var concertCard = UiKit.Panel(_concertRoot.transform, "ConcertBookCard", Color.white);
            UiKit.Layout(concertCard, new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(720, 380));
            var concertBookImg = concertCard.GetComponent<Image>();
            ArtSprites.Apply(concertBookImg, ArtSprites.ConcertStage, new Color(1f, 0.86f, 0.94f, 0.98f), Color.white);
            concertBookImg.preserveAspect = false;
            SafeFitCard.Bind(concertCard, 720f, 380f);
            var concertTitle = UiKit.Label(concertCard, "ConcertTitle", "콘서트 개최", 46, Palette.Gold, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(concertTitle.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -28), new Vector2(-40, 70));
            _concertBody = UiKit.Label(concertCard, "ConcertBody", "", 28, Palette.Pastel, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(_concertBody.rectTransform, new Vector2(0.08f, 0.28f), new Vector2(0.92f, 0.72f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            var concertYes = UiKit.Button(concertCard, "ConcertGo", "개최", OnConcertYes, Palette.Gold, Palette.Ink);
            UiKit.Layout(concertYes.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-170, 28), new Vector2(300, 72));
            var concertNo = UiKit.Button(concertCard, "ConcertLater", "나중에", OnConcertLater, Palette.StudioHi, Palette.Pastel);
            UiKit.Layout(concertNo.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(170, 28), new Vector2(300, 72));
            SafePairLayout.Bind(concertCard, concertYes.GetComponent<RectTransform>(), concertNo.GetComponent<RectTransform>());
            _concertRoot.SetActive(false);

            _concertResultRoot = new GameObject("ConcertResultRoot", typeof(RectTransform));
            _concertResultRoot.transform.SetParent(root, false);
            UiKit.Stretch(_concertResultRoot.GetComponent<RectTransform>());
            var resultWash = UiKit.Image(_concertResultRoot.transform, "ConcertResultWash", new Color(0.08f, 0.04f, 0.1f, 0.78f));
            UiKit.Stretch(resultWash.rectTransform);
            resultWash.raycastTarget = true;
            var resultCard = UiKit.Panel(_concertResultRoot.transform, "ConcertResultCard", Color.white);
            UiKit.Layout(resultCard, new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(720, 360));
            SafeFitCard.Bind(resultCard, 720f, 360f);
            _concertResultPanel = resultCard.GetComponent<Image>();
            ArtSprites.Apply(_concertResultPanel, ArtSprites.ConcertStage, new Color(1f, 0.9f, 0.5f, 0.98f), Color.white);
            _concertResultPanel.preserveAspect = false;
            _concertResultTitle = UiKit.Label(resultCard, "ConcertResultTitle", "", 48, Palette.Gold, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(_concertResultTitle.rectTransform, new Vector2(0, 0.42f), new Vector2(1, 1), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            _concertResultSub = UiKit.Label(resultCard, "ConcertResultSub", "", 24, Palette.Pastel, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(_concertResultSub.rectTransform, new Vector2(0, 0.18f), new Vector2(1, 0.48f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            var resultGo = UiKit.Button(resultCard, "ConcertResultAck", "정산으로", OnConcertResultAck, Palette.Gold, Palette.Ink);
            UiKit.Layout(resultGo.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 28), new Vector2(320, 72));
            _concertResultRoot.SetActive(false);
        }

        void TickDebtCount(float dt)
        {
            if (!_debtCounting || _tileDebt == null)
                return;
            _debtCountT += dt;
            float u = Mathf.Clamp01(_debtCountT / 0.4f);
            int shown = Mathf.RoundToInt(Mathf.Lerp(_debtFrom, _debtTo, u));
            if (u >= 1f)
            {
                shown = _debtTo;
                _debtCounting = false;
            }
            _tileDebt.text = EconomyRules.FormatWon(shown);
        }

        void TickMentalCount(float dt)
        {
            if (!_mentalCounting)
                return;
            _mentalCountT += dt;
            float u = Mathf.Clamp01(_mentalCountT / 0.35f);
            int shown = Mathf.RoundToInt(Mathf.Lerp(_mentalFrom, _mentalTo, u));
            if (u >= 1f)
            {
                shown = _mentalTo;
                _mentalCounting = false;
            }
            ApplyMentalShown(shown);
        }

        void ApplyMentalShown(int shown)
        {
            if (_tileMental != null)
                _tileMental.text = shown.ToString();
            if (_body != null && _bodyLead != null)
                _body.text = _bodyLead + shown;
        }

        void TickIncomeCount(float dt)
        {
            if (!_incomeCounting || _tileIncome == null)
                return;
            _incomeCountT += dt;
            float u = Mathf.Clamp01(_incomeCountT / 0.6f);
            int shown = Mathf.RoundToInt(Mathf.Lerp(0f, _incomeTarget, u));
            if (u >= 1f)
            {
                shown = _incomeTarget;
                _incomeCounting = false;
                if (!_shortFired && _incomeTarget < _incomeBill)
                    ShowShortfall();
            }
            _tileIncome.text = EconomyRules.FormatWon(shown);
            if (!_coverCrossed && _incomeTarget >= _incomeBill && shown >= _incomeBill)
            {
                _coverCrossed = true;
                _cashUp = true;
                _mood = 1f;
                _incomeCoverFlash = 1f;
            }
        }

        void TickLeftCash(float dt)
        {
            if (!_leftCashShown && !_incomeCounting && !_debtCounting)
                ShowLeftCash();
            if (!_leftCashShown || _leftCash == null)
                return;
            _leftCashSnap = Mathf.MoveTowards(_leftCashSnap, 0f, dt * 4f);
            float u = _leftCashSnap;
            var rt = _leftCashSlip != null ? _leftCashSlip.rectTransform : _leftCash.rectTransform;
            rt.localScale = Vector3.one * (1f + 0.10f * u);
        }

        void ShowLeftCash()
        {
            _leftCashShown = true;
            _leftCashSnap = 1f;
            ApplyLeftCash();
            if (_leftCashSlip != null)
                _leftCashSlip.gameObject.SetActive(true);
            if (_leftCash != null)
                _leftCash.gameObject.SetActive(true);
        }

        void ApplyLeftCash()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.Run == null || _leftCash == null)
                return;
            int cash = gm.Run.cash;
            _leftCash.text = "남은 현금  " + EconomyRules.FormatWon(cash);
            int typical = PeekTomorrowTypical(gm);
            bool shortfall = typical > 0 && cash < typical;
            _leftCash.color = shortfall ? Palette.MoneyRed : Palette.Pastel;
            if (_leftCashShortStamp != null)
            {
                if (shortfall)
                {
                    ArtSprites.Apply(_leftCashShortStamp, ArtSprites.BillShort, Palette.MoneyRed, Color.white);
                    _leftCashShortStamp.preserveAspect = false;
                }
                _leftCashShortStamp.gameObject.SetActive(shortfall);
            }
            if (_leftCashShort != null)
            {
                if (shortfall)
                {
                    _leftCashShort.text = "청구보다 부족";
                    _leftCashShort.color = Palette.MoneyRed;
                }
                _leftCashShort.gameObject.SetActive(shortfall);
            }
        }

        static int PeekTomorrowTypical(GameManager gm)
        {
            if (gm == null || gm.Run == null || gm.Balance == null)
                return 0;
            if (WeekSchedule.DaysLeftInWeek(gm.Run) <= 0)
                return 0;
            return WeekSchedule.TotalFixedBills(gm.Run, gm.Balance, gm.Week2, gm.Week3, gm.Week4, gm.Week5);
        }

        void ShowShortfall()
        {
            _shortFired = true;
            _shortFlash = 0.35f;
            if (_shortStamp != null)
            {
                ArtSprites.Apply(_shortStamp, ArtSprites.BillShort, Palette.MoneyRed, Color.white);
                _shortStamp.preserveAspect = false;
                _shortStamp.gameObject.SetActive(true);
                var sc = _shortStamp.color;
                sc.a = 1f;
                _shortStamp.color = sc;
                _shortStamp.rectTransform.localScale = Vector3.one * 1.22f;
            }
            if (_shortChip != null)
            {
                _shortChip.text = "청구 미달";
                _shortChip.gameObject.SetActive(true);
                var c = Palette.MoneyRed;
                c.a = 1f;
                _shortChip.color = c;
            }
        }

        void TickShortfall(float dt)
        {
            _shortFlash = Mathf.MoveTowards(_shortFlash, 0f, dt);
            bool on = _shortFlash > 0.001f;
            float u = on ? _shortFlash / 0.35f : 0f;
            var shortScale = Vector3.one * (1f + 0.22f * u);
            if (_shortStamp != null)
            {
                _shortStamp.gameObject.SetActive(on);
                if (on)
                {
                    var sc = _shortStamp.color;
                    sc.a = Mathf.Clamp01(u);
                    _shortStamp.color = sc;
                    _shortStamp.rectTransform.localScale = shortScale;
                }
            }
            if (_shortChip != null)
            {
                _shortChip.gameObject.SetActive(on);
                if (on)
                {
                    var c = Palette.MoneyRed;
                    c.a = Mathf.Clamp01(u);
                    _shortChip.color = c;
                }
            }
            if (_tileBills == null)
                return;
            if (on)
            {
                _tileBills.color = Color.Lerp(Color.white, Palette.MoneyRed, u);
                if (_billsCap != null)
                    _billsCap.color = Color.Lerp(Color.white, Palette.MoneyRed, u);
                if (_billsImg != null)
                    _billsImg.color = Color.Lerp(Palette.MoneyRed, new Color(1f, 0.12f, 0.22f, 1f), u);
                if (_billsTile != null)
                    _billsTile.localScale = Vector3.one * (1f + 0.16f * u);
            }
            else if (_billsTile != null)
            {
                _tileBills.color = Color.white;
                if (_billsCap != null)
                    _billsCap.color = Color.white;
                if (_billsImg != null)
                    _billsImg.color = Palette.MoneyRed;
                _billsTile.localScale = Vector3.one;
            }
        }

        void Render()
        {
            var gm = GameManager.Instance;
            var run = gm.Run;
            var b = gm.Balance;
            var w2 = gm.Week2;
            var w3 = gm.Week3;
            var w4 = gm.Week4;
            var w5 = gm.Week5;

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
                        ? $"아크릴 {run.lastGoodsSold}개 팔림   {EconomyRules.FormatWon(run.lastGoodsRevenue)}" +
                          (run.lastGoodsPromoSuccess ? "  · 홍보 1.5x\n" : "\n")
                        : "") +
                    $"아크릴 재고         {run.goodsStock}개\n";
            }
            int charges = run.lastBills + run.extraThreatAmount + run.lastConflictSurcharge + run.lastAutoCost;
            _incomeTarget = run.lastStreamIncome;
            _incomeBill = run.lastBills;
            if (!_incomeCountStarted)
            {
                _incomeCountStarted = true;
                _incomeCounting = true;
                _incomeCountT = 0f;
                _coverCrossed = run.lastStreamIncome < run.lastBills;
                _cashUp = false;
                if (_tileIncome != null)
                    _tileIncome.text = EconomyRules.FormatWon(0);
            }
            else if (!_incomeCounting && _tileIncome != null)
            {
                _tileIncome.text = EconomyRules.FormatWon(run.lastStreamIncome);
                _cashUp = run.lastStreamIncome >= run.lastBills;
            }
            _tileBills.text = "-" + EconomyRules.FormatWon(charges);
            _tileCash.text = EconomyRules.FormatWon(run.cash);
            if (_leftCashShown)
                ApplyLeftCash();
            _debtTo = run.debt;
            _debtFrom = run.debtAtDayStart;
            if (!_debtCountStarted)
            {
                _debtCountStarted = true;
                if (_debtTo > _debtFrom)
                {
                    _debtCounting = true;
                    _debtCountT = 0f;
                    if (_tileDebt != null)
                        _tileDebt.text = EconomyRules.FormatWon(_debtFrom);
                }
                else if (_tileDebt != null)
                {
                    _tileDebt.text = EconomyRules.FormatWon(_debtTo);
                    if (_debtTo < _debtFrom)
                        _debtDip = 1f;
                }
            }
            else if (!_debtCounting && _tileDebt != null)
                _tileDebt.text = EconomyRules.FormatWon(run.debt);
            _mentalFrom = run.mentalAtDayStart;
            int nextMental = run.mental;
            if (!_mentalCountStarted)
            {
                _mentalCountStarted = true;
                _mentalTo = nextMental;
                if (_mentalTo < _mentalFrom)
                {
                    _mentalCounting = true;
                    _mentalCountT = 0f;
                    if (_tileMental != null)
                        _tileMental.text = _mentalFrom.ToString();
                }
                else
                {
                    if (_tileMental != null)
                        _tileMental.text = _mentalTo.ToString();
                    if (_mentalTo > _mentalFrom)
                        _mentalTick = 1f;
                }
            }
            else if (!_mentalCounting)
            {
                if (nextMental > _mentalTo)
                    _mentalTick = 1f;
                _mentalTo = nextMental;
                if (_tileMental != null)
                    _tileMental.text = _mentalTo.ToString();
            }
            else
                _mentalTo = nextMental;
            _tilePerfect.text = run.lastPerfects.ToString();
            _tileMiss.text = run.lastMisses.ToString();
            _tileViewers.text = Mathf.RoundToInt(run.lastStreamPeakViewers).ToString();
            _mood = 1f;
            if (_portrait != null)
                _portrait.PoseEnding(_cashUp ? EndingKind.SoloLegend : EndingKind.Bankrupt);

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
                (run.lastAgencyFoundCost > 0
                    ? $"에이전시 설립      -{EconomyRules.FormatWon(run.lastAgencyFoundCost)}\n"
                    : "") +
                (run.lastJuniorScoutCost > 0
                    ? $"주니어 스카우트    -{EconomyRules.FormatWon(run.lastJuniorScoutCost)}\n"
                    : "") +
                (run.lastJuniorPay > 0
                    ? $"후배 방송           +{EconomyRules.FormatWon(run.lastJuniorPay)}\n"
                    : "") +
                (run.lastJuniorTrainFail ? "주니어 훈련 실패   멘탈 −8\n" : "") +
                (run.lastSponsorDaily > 0
                    ? $"스폰서 일급         {EconomyRules.FormatWon(run.lastSponsorDaily)}\n"
                    : "") +
                (run.lastSponsorLineBonus > 0
                    ? $"스폰서 멘트         {EconomyRules.FormatWon(run.lastSponsorLineBonus)}\n"
                    : "") +
                (run.lastSponsorBroke ? "스폰서 계약 종료   −₩15,000 · 멘탈 −12\n" : "") +
                (run.lastRankingFirstPay > 0
                    ? $"랭킹 1위            {EconomyRules.FormatWon(run.lastRankingFirstPay)}\n"
                    : "") +
                (run.lastConcertCost > 0 && run.concertBooked
                    ? $"콘서트 개최        -{EconomyRules.FormatWon(run.lastConcertCost)}\n"
                    : "") +
                (run.lastConcertFailed ? "콘서트 실패         개최비만 날림 · 멘탈 −25 · 시작 시청자 −10\n" : "") +
                (run.lastConcertPayout > 0
                    ? $"콘서트 정산         {EconomyRules.FormatWon(run.lastConcertPayout)}" +
                      (run.lastConcertPerformanceSuccess ? "  · 퍼포먼스 1.3x\n" : "\n")
                    : "") +
                (run.lastRepaid > 0 ? $"부채 상환           -{EconomyRules.FormatWon(run.lastRepaid)}\n" : "") +
                (run.lastAutoCost > 0
                    ? $"자동응답           -{EconomyRules.FormatWon(run.lastAutoCost)}\n"
                    : "") +
                (run.lastConflictSurcharge > 0
                    ? $"갈등 할증           -{EconomyRules.FormatWon(run.lastConflictSurcharge)}\n"
                    : "") +
                (run.lastFanSupport > 0
                    ? $"팬 지원금           {EconomyRules.FormatWon(run.lastFanSupport)}\n"
                    : "") +
                (run.lastFanLetter ? "팬레터 답장         충성 +4 · 멘탈 +8\n" : "") +
                (run.lastMinjunLeft ? "민준이 떠났습니다   충성 −12\n" : "") +
                (run.lastHaeunLeft ? "하은이 떠났습니다   충성 −15\n" : "") +
                (run.lostSuperchatBonusDay ? "슈퍼챗 보너스 1일 소멸\n" : "") +
                $"\n판정  P {run.lastPerfects}  G {run.lastGreats}  Good {run.lastGoods}  Miss {run.lastMisses}" +
                (run.lastHadHype ? "   · 하이프 달성" : "") +
                (run.lastStreamEventHappened
                    ? $"\n이벤트 {run.lastStreamEventName}   {(run.lastStreamEventSuccess ? "성공" : "실패")}"
                    : "") +
                (run.membershipUnlocked
                    ? $"\n멤버십 {run.membershipCount}{memberDelta}"
                    : "") +
                (run.agencyFounded ? "\n에이전시 설립됨" : "") +
                (run.juniorScouted ? "   ·   주니어 1" : "") +
                (run.sponsorActive ? $"   ·   스폰서 남은 {run.sponsorDaysLeft}일" : "") +
                $"\n{FandomRules.HudLine(run)}" +
                (string.IsNullOrEmpty(FandomRules.SuperfanLine(run, gm.Fandom))
                    ? ""
                    : $"\n{FandomRules.SuperfanLine(run, gm.Fandom)}") +
                (FandomRules.MustResolveConflict(run) ? "\n콘텐츠 편중 갈등 — 오늘 안에 고르세요." : "") +
                (ContentRules.HasPick(run)
                    ? $"\n콘텐츠 {ContentRules.DisplayName(gm.Content, run.contentPicked)}"
                    : "") +
                $"\n\n현금 {EconomyRules.FormatWon(run.cash)}     부채 {EconomyRules.FormatWon(run.debt)}     멘탈 ";
            _bodyLead = _body.text;
            int mentalShown = _mentalCounting ? _mentalFrom : _mentalTo;
            _body.text = _bodyLead + mentalShown;
            if (_tileMental != null && !_mentalCounting)
                _tileMental.text = _mentalTo.ToString();

            run.lastOutcome = EconomyRules.Evaluate(run, b, w2, w3, w4, w5);
            ApplyHeadline(run);
            PaintShowLine(run);
            bool offerClip = Week2Rules.CanOfferClip(run, w2);
            bool offerFound = Week4Rules.CanFoundAgency(run, w4);
            bool offerScout = Week4Rules.CanScoutJunior(run, w4);
            bool offerSponsor = Week4Rules.CanOfferSponsor(run, w4);
            bool offerConcert = Week5Rules.CanBookConcert(run, w5);
            bool concertReady = Week5Rules.ConcertStreamReady(run);
            bool week4Offer = offerFound || offerScout || offerSponsor;
            bool week5Offer = offerConcert || concertReady;
            _clipYes.gameObject.SetActive(false);
            _clipNo.gameObject.SetActive(false);
            _foundAgency.gameObject.SetActive(offerFound && !_agencyOpen && !_agencySplashOpen);
            _scout.gameObject.SetActive(offerScout && !_juniorOpen && !_agencyOpen && !_agencySplashOpen);
            _signSponsor.gameObject.SetActive(offerSponsor && !_agencyOpen && !_agencySplashOpen && !_juniorOpen);
            _bookConcert.gameObject.SetActive(offerConcert && !_concertOpen && !_concertResultOpen);
            _concertLive.gameObject.SetActive(concertReady && !_concertOpen && !_concertResultOpen);
            if (_produce != null)
            {
                var produceCap = _produce.GetComponentInChildren<Text>();
                if (produceCap != null && w3 != null)
                    produceCap.text = $"아크릴 1개 생산  {EconomyRules.FormatWon(w3.goodsProduceCost)}  ·  판매 {EconomyRules.FormatWon(w3.goodsPrice)}";
            }
            _produce.gameObject.SetActive(run.goodsUnlocked && !offerClip && !week4Offer && !week5Offer && !_goodsOpen && !_agencyOpen && !_agencySplashOpen && !_juniorOpen && !_concertOpen && !_concertResultOpen && run.cash >= (w3 != null ? w3.goodsProduceCost : 2500));
            bool rankOn = Week5Rules.RankingUnlocked(run, w5);
            if (rankOn && !_rankHeard)
            {
                PlayRankingSfx();
                _rankHeard = true;
            }
            _rankBox.gameObject.SetActive(rankOn);
            if (rankOn)
                FillRankPanel(run, w5);
            if (run.lastClipAttempted)
                _clipNote.text = run.lastClipSuccess
                    ? "클립 성공 — ₩30,000 · 시작 시청자 +10"
                    : "클립 없음";
            else
                _clipNote.text = "";
            _clipNote.gameObject.SetActive(run.lastClipAttempted && !offerClip);

            bool ending = ShouldShowEnding(run, w5);
            bool conflict = FandomRules.MustResolveConflict(run);
            _letter.gameObject.SetActive(false);
            _letter.interactable = FandomRules.CanSendLetter(run);
            _letter.GetComponentInChildren<Text>().text = run.fanLetterSentThisDay ? "팬레터 완료" : "팬레터 답장";
            _auto.gameObject.SetActive(!ending && FandomRules.CanToggleAuto(run) && !_autoOpen && !_conflictOpen);
            if (_auto.gameObject.activeSelf)
                _auto.GetComponentInChildren<Text>().text = run.autoReplyOn ? "기본 자동응답 끄기" : "기본 자동응답 켜기";
            _soothe.gameObject.SetActive(false);
            _style.gameObject.SetActive(false);
            if (conflict)
            {
                _clipYes.gameObject.SetActive(false);
                _clipNo.gameObject.SetActive(false);
                _produce.gameObject.SetActive(false);
                _foundAgency.gameObject.SetActive(false);
                _scout.gameObject.SetActive(false);
                _signSponsor.gameObject.SetActive(false);
                _bookConcert.gameObject.SetActive(false);
                _concertLive.gameObject.SetActive(false);
            }

            switch (run.lastOutcome)
            {
                case WeekOutcome.Bankrupt:
                    _result.text = WeekSchedule.InWeek5(run)
                        ? "파산. 부채가 ₩350,000을 넘었습니다."
                        : WeekSchedule.InWeek4(run)
                        ? "파산. 부채가 ₩300,000을 넘었습니다."
                        : WeekSchedule.InWeek3(run)
                        ? "파산. 부채가 ₩260,000을 넘었습니다."
                        : WeekSchedule.InWeek2(run)
                        ? "파산. 부채가 ₩220,000을 넘었습니다."
                        : "파산. 부채가 ₩180,000을 넘었습니다.";
                    _result.color = Palette.MoneyRed;
                    _next.gameObject.SetActive(false);
                    _repay.gameObject.SetActive(false);
                    _restart.gameObject.SetActive(true);
                    _letter.gameObject.SetActive(false);
                    _auto.gameObject.SetActive(false);
                    _soothe.gameObject.SetActive(false);
                    _style.gameObject.SetActive(false);
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
                    _next.GetComponentInChildren<Text>().text = "4주차 시작  (Space)";
                    _next.gameObject.SetActive(true);
                    _repay.gameObject.SetActive(run.cash > 0 && run.debt > 0);
                    _restart.gameObject.SetActive(true);
                    PlaceTripleButtons();
                    break;
                case WeekOutcome.Week4Win:
                    _result.text = "4주차 클리어. 에이전시 설립, 그리고 빚 ≤ 1만 또는 현금 ≥ 18만.";
                    _result.color = Palette.CashGreen;
                    _next.GetComponentInChildren<Text>().text = "5주차 시작  (Space)";
                    _next.gameObject.SetActive(true);
                    _repay.gameObject.SetActive(run.cash > 0 && run.debt > 0);
                    _restart.gameObject.SetActive(true);
                    PlaceTripleButtons();
                    break;
                case WeekOutcome.Ending:
                    _result.text = "5주차 정산. 엔딩이 열립니다.";
                    _result.color = Palette.Gold;
                    _next.gameObject.SetActive(false);
                    _repay.gameObject.SetActive(run.cash > 0 && run.debt > 0 && !ShouldShowEnding(run, w5));
                    _restart.gameObject.SetActive(!ShouldShowEnding(run, w5));
                    break;
                case WeekOutcome.WeekFailed:
                    _result.text = WeekSchedule.InWeek4(run)
                        ? "4주차 목표 미달 (에이전시 설립, 그리고 부채 1만 이하 또는 현금 18만)."
                        : WeekSchedule.InWeek3(run)
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

            if (conflict)
                _next.gameObject.SetActive(false);

            ApplyEndingOverlay(run, w5);
            ApplyResultSplashes(run, w5);
        }

        void PlaceTripleButtons()
        {
            // Positions come from SafePairLayout on ActionRow (side-by-side or stacked).
        }

        void OnLetter()
        {
            var gm = GameManager.Instance;
            if (!FandomRules.SendLetter(gm.Run, gm.Balance, gm.Fandom))
                return;
            PlayLetterSfx();
            var f = gm.Fandom;
            int loy = f != null ? f.letterLoyalty : 4;
            int men = f != null ? f.letterMental : 8;
            if (_letterHeart != null)
            {
                _letterHeart.text = $"♥  충성 +{loy}  멘탈 +{men}";
                var c = Palette.Pink;
                c.a = 1f;
                _letterHeart.color = c;
            }
            _letterHeartFlash = 1.2f;
            if (_letterHeart != null)
                _letterHeart.transform.SetAsLastSibling();
            CloseLetter(true);
            Render();
            AdvanceBeats();
        }

        void OnLetterLater()
        {
            CloseLetter(true);
            AdvanceBeats();
        }

        void MaybeShowLetter()
        {
            if (_letterOpen || _letterDismissed)
                return;
            var gm = GameManager.Instance;
            if (gm == null || gm.Run == null)
                return;
            if (!FandomRules.ShouldOfferLetter(gm.Run))
                return;
            if (ShouldShowEnding(gm.Run, gm.Week5))
                return;
            var look = FanLetterLook.For(gm.Run, gm.Fandom);
            if (string.IsNullOrEmpty(look.From))
                return;
            if (_letterFrom != null)
                _letterFrom.text = look.From;
            if (_letterTag != null)
                _letterTag.text = look.Tag;
            if (_letterBody != null)
            {
                _letterBody.text = look.Body;
                _letterBody.color = look.Cold ? new Color(0.28f, 0.22f, 0.26f, 1f) : Palette.Ink;
            }
            if (_letterRoot != null)
            {
                _letterRoot.SetActive(true);
                _letterRoot.transform.SetAsLastSibling();
            }
            _letterOpen = true;
        }

        void CloseLetter(bool dismissed)
        {
            _letterOpen = false;
            if (dismissed)
                _letterDismissed = true;
            if (_letterRoot != null)
                _letterRoot.SetActive(false);
        }

        void OnToggleAuto()
        {
            var gm = GameManager.Instance;
            FandomRules.SetAutoReply(gm.Run, !gm.Run.autoReplyOn);
            Render();
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
            var sootheCap = _conflictSoothe != null ? _conflictSoothe.GetComponentInChildren<Text>() : null;
            if (sootheCap != null)
                sootheCap.text = $"특별방송으로 달래기\n멘탈 −{mental}\n충성 +{sootheLoy}";
            var styleCap = _conflictStyle != null ? _conflictStyle.GetComponentInChildren<Text>() : null;
            if (styleCap != null)
                styleCap.text = $"내 스타일대로\nT2 −{t2}\n충성 −{styleLoy}\n다음 위협 +{EconomyRules.FormatWon(extra)}";
            if (_conflictOverlayResult != null)
                _conflictOverlayResult.text = "";
            if (_conflictSoothe != null)
                _conflictSoothe.gameObject.SetActive(true);
            if (_conflictStyle != null)
                _conflictStyle.gameObject.SetActive(true);
        }

        void ShowConflictCard()
        {
            FillConflictCards();
            if (_conflictOverlay != null)
            {
                _conflictOverlay.SetActive(true);
                _conflictOverlay.transform.SetAsLastSibling();
            }
            _conflictOpen = true;
        }

        void CloseConflictCard()
        {
            _conflictOpen = false;
            if (_conflictOverlay != null)
                _conflictOverlay.SetActive(false);
        }

        void OnSootheConflict()
        {
            var gm = GameManager.Instance;
            if (gm == null || !FandomRules.SootheConflict(gm.Run, gm.Fandom))
                return;
            var f = gm.Fandom;
            int mental = f != null ? f.conflictSootheMental : 10;
            int loy = f != null ? f.conflictSootheLoyalty : 8;
            _result.text = $"달랬다 멘탈 −{mental} · 충성 +{loy}";
            CloseConflictCard();
            gm.SaveRun();
            Render();
            AdvanceBeats();
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
            _result.text = $"내 스타일대로 T2 −{t2} · 충성 −{loy} · 다음 위협 +{EconomyRules.FormatWon(extra)}";
            CloseConflictCard();
            gm.SaveRun();
            Render();
            AdvanceBeats();
        }

        void ShowAutoCard()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.Run == null)
                return;
            var f = gm.Fandom;
            int cost = f != null ? f.autoDailyCost : 8000;
            if (_autoBody != null)
                _autoBody.text = $"하루 {EconomyRules.FormatWon(cost)}";
            if (_autoRoot != null)
            {
                _autoRoot.SetActive(true);
                _autoRoot.transform.SetAsLastSibling();
            }
            _autoOpen = true;
        }

        void CloseAutoCard(bool on)
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.Run == null)
                return;
            FandomRules.SetAutoReply(gm.Run, on);
            gm.Run.autoReplyPrompted = true;
            _autoOpen = false;
            if (_autoRoot != null)
                _autoRoot.SetActive(false);
            gm.SaveRun();
            Render();
            AdvanceBeats();
        }

        void OnAutoOn() => CloseAutoCard(true);

        void OnAutoOff() => CloseAutoCard(false);

        void OnRepay()
        {
            var run = GameManager.Instance.Run;
            EconomyRules.RepayDebt(run, run.cash);
            Render();
        }

        void AdvanceBeats()
        {
            if (_letterOpen || _memberOpen || _clipOpen || _goodsOpen || _agencyOpen || _agencySplashOpen || _juniorOpen || _concertOpen || _concertResultOpen || _conflictOpen || _autoOpen)
                return;
            var gm = GameManager.Instance;
            if (gm == null || gm.Run == null)
                return;
            if (!_concertResultDismissed && gm.Run.concertResultApplied && (gm.Run.lastConcertFailed || gm.Run.lastConcertPayout > 0))
            {
                ShowConcertResult();
                return;
            }
            if (ShouldShowEnding(gm.Run, gm.Week5))
                return;
            if (!_letterDismissed && FandomRules.ShouldOfferLetter(gm.Run))
            {
                MaybeShowLetter();
                if (_letterOpen)
                    return;
            }
            if (gm.Run.membershipJustUnlocked)
            {
                ShowMemberSplash();
                return;
            }
            if (Week2Rules.CanOfferClip(gm.Run, gm.Week2))
            {
                ShowClipCard();
                return;
            }
            if (gm.Run.goodsJustUnlocked)
            {
                ShowGoodsSplash();
                return;
            }
            if (FandomRules.MustResolveConflict(gm.Run))
            {
                ShowConflictCard();
                return;
            }
            if (gm.Run.agencyJustFounded)
            {
                ShowAgencySplash();
                return;
            }
            if (!_agencyDismissed && Week4Rules.CanFoundAgency(gm.Run, gm.Week4))
            {
                ShowAgencyCard();
                return;
            }
            if (!_juniorDismissed && Week4Rules.CanScoutJunior(gm.Run, gm.Week4))
            {
                ShowJuniorCard();
                return;
            }
            if (!gm.Run.autoReplyPrompted && FandomRules.CanToggleAuto(gm.Run) && !ShouldShowEnding(gm.Run, gm.Week5))
            {
                ShowAutoCard();
                return;
            }
            if (!_concertDismissed && Week5Rules.CanBookConcert(gm.Run, gm.Week5))
                ShowConcertCard();
        }

        void ShowMemberSplash()
        {
            var w2 = GameManager.Instance.Week2;
            int start = w2 != null ? w2.startingMembers : 8;
            int pay = w2 != null ? w2.membershipPassivePerMember : 150;
            if (_memberBody != null)
                _memberBody.text = $"시작 {start}명\n정산 때 멤버×{EconomyRules.FormatWon(pay)}";
            if (_memberRoot != null)
            {
                _memberRoot.SetActive(true);
                _memberRoot.transform.SetAsLastSibling();
            }
            _memberOpen = true;
            PlayMemberSfx();
        }

        void OnMemberAck()
        {
            var run = GameManager.Instance != null ? GameManager.Instance.Run : null;
            if (run != null)
                run.membershipJustUnlocked = false;
            _memberOpen = false;
            if (_memberRoot != null)
                _memberRoot.SetActive(false);
            Render();
            AdvanceBeats();
        }

        void ShowClipCard()
        {
            if (_clipRoot != null)
            {
                _clipRoot.SetActive(true);
                _clipRoot.transform.SetAsLastSibling();
            }
            _clipOpen = true;
            PlayClipSfx();
        }

        void CloseClipCard()
        {
            _clipOpen = false;
            if (_clipRoot != null)
                _clipRoot.SetActive(false);
        }

        void OnClipYes()
        {
            var gm = GameManager.Instance;
            bool ok = Week2Rules.AttemptClip(gm.Run, gm.Week2);
            CloseClipCard();
            if (ok)
            {
                var w2 = gm.Week2;
                int cash = w2 != null ? w2.clipCash : 30000;
                int viewers = w2 != null ? w2.clipViewerBonus : 10;
                if (_clipSlam != null)
                {
                    _clipSlam.text = $"{EconomyRules.FormatWon(cash)}  ·  시청자 +{viewers}";
                    var c = Palette.Gold;
                    c.a = 1f;
                    _clipSlam.color = c;
                    _clipSlam.transform.SetAsLastSibling();
                }
                _clipSlamFlash = 1.25f;
            }
            Render();
        }

        void OnClipNo()
        {
            Week2Rules.DeclineClip(GameManager.Instance.Run);
            CloseClipCard();
            Render();
        }

        void ShowGoodsSplash()
        {
            var w3 = GameManager.Instance.Week3;
            int stock = w3 != null ? w3.goodsUnlockStock : 20;
            int cost = w3 != null ? w3.goodsProduceCost : 2500;
            int price = w3 != null ? w3.goodsPrice : 7000;
            if (_goodsBody != null)
                _goodsBody.text = $"재고 {stock}\n원가 {EconomyRules.FormatWon(cost)}\n판매 {EconomyRules.FormatWon(price)}";
            if (_goodsRoot != null)
            {
                _goodsRoot.SetActive(true);
                _goodsRoot.transform.SetAsLastSibling();
            }
            _goodsOpen = true;
            if (_produce != null)
                _produce.gameObject.SetActive(false);
            PlayGoodsSfx();
        }

        void OnGoodsAck()
        {
            var run = GameManager.Instance != null ? GameManager.Instance.Run : null;
            if (run != null)
                run.goodsJustUnlocked = false;
            _goodsOpen = false;
            if (_goodsRoot != null)
                _goodsRoot.SetActive(false);
            Render();
            AdvanceBeats();
        }

        void OnProduce()
        {
            var gm = GameManager.Instance;
            Week3Rules.ProduceGoods(gm.Run, gm.Week3);
            Render();
        }

        void ShowAgencyCard()
        {
            var w4 = GameManager.Instance.Week4;
            int cost = w4 != null ? w4.agencyFoundCost : 40000;
            int daily = w4 != null ? w4.agencyDailyCost : 15000;
            int bills = w4 != null ? w4.TotalDailyBills + w4.agencyDailyCost : 53000;
            if (_agencyBody != null)
                _agencyBody.text = $"에이전시 설립 {EconomyRules.FormatWon(cost)}\n이후 일 +{EconomyRules.FormatWon(daily)}\n고정비 {EconomyRules.FormatWon(bills)}";
            if (_agencyRoot != null)
            {
                _agencyRoot.SetActive(true);
                _agencyRoot.transform.SetAsLastSibling();
            }
            _agencyOpen = true;
            HideWeek4QuietButtons();
            PlayAgencySfx();
        }

        void CloseAgencyCard()
        {
            _agencyOpen = false;
            if (_agencyRoot != null)
                _agencyRoot.SetActive(false);
        }

        void OnAgencyYes()
        {
            OnFoundAgency();
        }

        void OnAgencyLater()
        {
            _agencyDismissed = true;
            CloseAgencyCard();
            Render();
            AdvanceBeats();
        }

        void ShowAgencySplash()
        {
            var w4 = GameManager.Instance.Week4;
            int daily = w4 != null ? w4.agencyDailyCost : 15000;
            int bills = w4 != null ? w4.TotalDailyBills + w4.agencyDailyCost : 53000;
            if (_agencySplashBody != null)
                _agencySplashBody.text = $"이후 일 +{EconomyRules.FormatWon(daily)}\n고정비 {EconomyRules.FormatWon(bills)}";
            if (_agencySplashRoot != null)
            {
                _agencySplashRoot.SetActive(true);
                _agencySplashRoot.transform.SetAsLastSibling();
            }
            _agencySplashOpen = true;
            HideWeek4QuietButtons();
        }

        void OnAgencySplashAck()
        {
            var run = GameManager.Instance != null ? GameManager.Instance.Run : null;
            if (run != null)
                run.agencyJustFounded = false;
            _agencySplashOpen = false;
            if (_agencySplashRoot != null)
                _agencySplashRoot.SetActive(false);
            Render();
            AdvanceBeats();
        }

        void ShowJuniorCard()
        {
            var w4 = GameManager.Instance.Week4;
            int cost = w4 != null ? w4.juniorScoutCost : 25000;
            if (_juniorBody != null)
                _juniorBody.text = $"{EconomyRules.FormatWon(cost)}";
            if (_juniorRoot != null)
            {
                _juniorRoot.SetActive(true);
                _juniorRoot.transform.SetAsLastSibling();
            }
            _juniorOpen = true;
            HideWeek4QuietButtons();
            PlayAgencySfx();
        }

        void CloseJuniorCard()
        {
            _juniorOpen = false;
            if (_juniorRoot != null)
                _juniorRoot.SetActive(false);
        }

        void OnJuniorYes()
        {
            OnScout();
        }

        void OnJuniorLater()
        {
            _juniorDismissed = true;
            CloseJuniorCard();
            Render();
            AdvanceBeats();
        }

        void HideWeek4QuietButtons()
        {
            if (_foundAgency != null)
                _foundAgency.gameObject.SetActive(false);
            if (_scout != null)
                _scout.gameObject.SetActive(false);
            if (_signSponsor != null)
                _signSponsor.gameObject.SetActive(false);
            if (_produce != null)
                _produce.gameObject.SetActive(false);
        }

        void OnFoundAgency()
        {
            var gm = GameManager.Instance;
            bool ok = Week4Rules.FoundAgency(gm.Run, gm.Week4);
            CloseAgencyCard();
            if (ok)
                ShowAgencySplash();
            Render();
        }

        void OnScout()
        {
            var gm = GameManager.Instance;
            Week4Rules.ScoutJunior(gm.Run, gm.Week4);
            CloseJuniorCard();
            _juniorDismissed = true;
            Render();
            AdvanceBeats();
        }

        void OnSignSponsor()
        {
            var gm = GameManager.Instance;
            Week4Rules.SignSponsor(gm.Run, gm.Week4);
            Render();
        }

        void FillRankPanel(GameRunState run, Week5Balance w5)
        {
            int you = run.lastRankingScore;
            int n0 = run.lastNpcScore != null && run.lastNpcScore.Length > 0 ? run.lastNpcScore[0] : 0;
            int n1 = run.lastNpcScore != null && run.lastNpcScore.Length > 1 ? run.lastNpcScore[1] : 0;
            int n2 = run.lastNpcScore != null && run.lastNpcScore.Length > 2 ? run.lastNpcScore[2] : 0;
            string first = run.lastDailyRank == 1
                ? $"\n1위 +{EconomyRules.FormatWon(w5 != null ? w5.rankingDailyFirstCash : 10000)}"
                : "";
            _rankPanel.text =
                "챌린지 랭킹\n" +
                $"나         {you}\n" +
                $"루나벨     {n0}\n" +
                $"하츠비     {n1}\n" +
                $"네온토끼   {n2}" +
                first;
        }

        void ShowConcertCard()
        {
            var w5 = GameManager.Instance.Week5;
            int cost = w5 != null ? w5.concertCost : 80000;
            if (_concertBody != null)
                _concertBody.text = $"콘서트 개최 {EconomyRules.FormatWon(cost)}";
            if (_concertRoot != null)
            {
                _concertRoot.SetActive(true);
                _concertRoot.transform.SetAsLastSibling();
            }
            _concertOpen = true;
            PlayConcertBookSfx();
            if (_bookConcert != null)
                _bookConcert.gameObject.SetActive(false);
            if (_produce != null)
                _produce.gameObject.SetActive(false);
        }

        void CloseConcertCard()
        {
            _concertOpen = false;
            if (_concertRoot != null)
                _concertRoot.SetActive(false);
        }

        void OnConcertYes()
        {
            OnBookConcert();
        }

        void OnConcertLater()
        {
            _concertDismissed = true;
            CloseConcertCard();
            Render();
            AdvanceBeats();
        }

        void ShowConcertResult()
        {
            var gm = GameManager.Instance;
            var run = gm.Run;
            var w5 = gm.Week5;
            int pay = w5 != null ? w5.concertBasePayout : 200000;
            if (_concertResultRoot != null)
            {
                _concertResultRoot.SetActive(true);
                _concertResultRoot.transform.SetAsLastSibling();
            }
            _concertResultOpen = true;
            if (run.lastConcertFailed)
            {
                _concertResultTitle.text = "개최비만 날림";
                _concertResultTitle.color = Palette.MoneyRed;
                _concertResultSub.text = $"멘탈 −{(w5 != null ? w5.concertFailMental : 25)}";
                _concertResultSub.color = Palette.MoneyRed;
                if (_concertResultPanel != null)
                {
                    ArtSprites.Apply(_concertResultPanel, ArtSprites.ConcertStage, new Color(1f, 0.72f, 0.74f, 0.98f), new Color(1f, 0.78f, 0.80f, 1f));
                    _concertResultPanel.preserveAspect = false;
                }
            }
            else
            {
                _concertResultTitle.text = EconomyRules.FormatWon(run.lastConcertPayout > 0 ? run.lastConcertPayout : pay);
                _concertResultTitle.color = Palette.Gold;
                _concertResultSub.text = EconomyRules.FormatWon(pay);
                _concertResultSub.color = Palette.Pastel;
                if (_concertResultPanel != null)
                {
                    ArtSprites.Apply(_concertResultPanel, ArtSprites.ConcertStage, new Color(1f, 0.9f, 0.5f, 0.98f), Color.white);
                    _concertResultPanel.preserveAspect = false;
                }
            }
            if (_bookConcert != null)
                _bookConcert.gameObject.SetActive(false);
            if (_produce != null)
                _produce.gameObject.SetActive(false);
        }

        void OnConcertResultAck()
        {
            _concertResultDismissed = true;
            _concertResultOpen = false;
            if (_concertResultRoot != null)
                _concertResultRoot.SetActive(false);
            Render();
            AdvanceBeats();
        }

        void OnBookConcert()
        {
            var gm = GameManager.Instance;
            Week5Rules.BookConcert(gm.Run, gm.Week5);
            CloseConcertCard();
            _concertDismissed = true;
            Render();
            AdvanceBeats();
        }

        void OnConcertLive()
        {
            LeaveSettle(() => GameManager.Instance.GoLive());
        }

        void OnRetire()
        {
            var gm = GameManager.Instance;
            gm.Run.retirePicked = true;
            gm.Run.lastEnding = Week5Rules.ResolveEnding(gm.Run, gm.Week5, true);
            gm.Run.lastOutcome = WeekOutcome.Ending;
            Render();
        }

        static bool IsBankruptResult(GameRunState run) =>
            run != null &&
            (run.lastOutcome == WeekOutcome.Bankrupt || run.lastEnding == EndingKind.Bankrupt);

        static bool IsBurnoutResult(GameRunState run) =>
            run != null && run.lastEnding == EndingKind.Burnout;

        static bool IsWeekClear(GameRunState run) =>
            run != null &&
            (run.lastOutcome == WeekOutcome.Win ||
             run.lastOutcome == WeekOutcome.Week2Win ||
             run.lastOutcome == WeekOutcome.Week3Win ||
             run.lastOutcome == WeekOutcome.Week4Win);

        static bool ShouldShowEnding(GameRunState run, Week5Balance w5)
        {
            if (run == null || !WeekSchedule.InWeek5(run))
                return false;
            if (IsBankruptResult(run) || IsBurnoutResult(run))
                return false;
            if (run.lastOutcome != WeekOutcome.Ending)
                return false;
            return !Week5Rules.CanBookConcert(run, w5) && !Week5Rules.ConcertStreamReady(run);
        }

        void ApplyResultSplashes(GameRunState run, Week5Balance w5)
        {
            bool clear = IsWeekClear(run);
            bool bankrupt = IsBankruptResult(run);
            bool burnout = IsBurnoutResult(run);
            if (_clearRoot != null)
                _clearRoot.SetActive(clear && !bankrupt && !burnout);
            if (_stampRoot != null)
                _stampRoot.SetActive(bankrupt || burnout);
            if (clear && _clearRoot != null && _clearRoot.activeSelf)
            {
                _clearTitle.text = run.lastOutcome switch
                {
                    WeekOutcome.Week4Win => "4주차 클리어",
                    WeekOutcome.Week3Win => "3주차 클리어",
                    WeekOutcome.Week2Win => "2주차 클리어",
                    _ => "1주차 생존"
                };
                _clearCash.text = "현금  " + EconomyRules.FormatWon(run.cash);
                _clearDebt.text = "부채  " + EconomyRules.FormatWon(run.debt);
                if (_clearHeadline != null)
                    _clearHeadline.text = DayHeadline.Build(run);
                _clearPortrait?.PoseEnding(EndingKind.SoloLegend);
            }
            if ((bankrupt || burnout) && _stampRoot != null && _stampRoot.activeSelf)
            {
                bool burn = burnout && !bankrupt;
                _stampWash.color = burn
                    ? new Color(0.58f, 0.52f, 0.58f, 1f)
                    : Color.white;
                _stampMark.text = burn ? "번아웃" : "파산";
                _stampMark.color = burn ? Palette.PastelDim : Palette.MoneyRed;
                _stampDebt.text = burn
                    ? $"멘탈 0   ·   {run.zeroMentalDays}일"
                    : "부채  " + EconomyRules.FormatWon(run.debt);
                int cap = EconomyRules.BankruptDebt(run, GameManager.Instance.Balance, GameManager.Instance.Week2, GameManager.Instance.Week3, GameManager.Instance.Week4, w5);
                _stampEpitaph.text = burn
                    ? Week5Rules.EndingBody(EndingKind.Burnout)
                    : $"부채가 {EconomyRules.FormatWon(cap)}을 넘었습니다. 채널은 여기서 멈춥니다.";
                if (_stampHeadline != null)
                    _stampHeadline.text = DayHeadline.Build(run);
                _stampPortrait?.PoseEnding(burn ? EndingKind.Burnout : EndingKind.Bankrupt);
            }
            if (!_resultStingPlayed)
            {
                if (clear && !bankrupt && !burnout)
                {
                    PlaySettleSfx(_clearCue, 0.56f);
                    QuietSettleBgm();
                    _resultStingPlayed = true;
                }
                else if (bankrupt)
                {
                    PlaySettleSfx(_bankruptCue, 0.62f);
                    QuietSettleBgm();
                    _resultStingPlayed = true;
                }
            }
            if (clear || bankrupt || burnout)
            {
                _next.gameObject.SetActive(clear);
                _repay.gameObject.SetActive(false);
                _restart.gameObject.SetActive(!clear);
                _letter.gameObject.SetActive(false);
                _auto.gameObject.SetActive(false);
                _soothe.gameObject.SetActive(false);
                _style.gameObject.SetActive(false);
                _clipYes.gameObject.SetActive(false);
                _clipNo.gameObject.SetActive(false);
                _produce.gameObject.SetActive(false);
                _foundAgency.gameObject.SetActive(false);
                _scout.gameObject.SetActive(false);
                _signSponsor.gameObject.SetActive(false);
                _bookConcert.gameObject.SetActive(false);
                _concertLive.gameObject.SetActive(false);
            }
        }

        void ApplyHeadline(GameRunState run)
        {
            DayHeadline.Remember(run);
            string line = DayHeadline.Build(run);
            if (_headline != null)
                _headline.text = line;
            bool on = !string.IsNullOrEmpty(line);
            if (_headlineClip != null)
                _headlineClip.gameObject.SetActive(on);
            if (_headlineTag != null)
                _headlineTag.gameObject.SetActive(on);
            if (_clearHeadline != null)
                _clearHeadline.text = line;
            if (_stampHeadline != null)
                _stampHeadline.text = line;
            if (_endingHeadline != null)
                _endingHeadline.text = line;
        }

        void PaintShowLine(GameRunState run)
        {
            if (_showLine == null)
                return;
            bool has = ContentRules.HasPick(run);
            _showLine.gameObject.SetActive(has);
            if (_showLineIcon != null)
                _showLineIcon.gameObject.SetActive(has);
            if (!has)
                return;
            _showLine.text = ShowLineName(run.contentPicked);
            _showLine.color = ShowLineAccent(run.contentPicked);
            if (_showLineIcon == null)
                return;
            string icon = ArtSprites.ForContent(run.contentPicked);
            if (icon != null)
            {
                ArtSprites.Apply(_showLineIcon, icon, Color.white, Color.white);
                _showLineIcon.preserveAspect = true;
                _showLineIcon.enabled = true;
            }
            else
                _showLineIcon.enabled = false;
        }

        static string ShowLineName(StreamContentType type) => type switch
        {
            StreamContentType.Talk => "오늘 토크",
            StreamContentType.Game => "오늘 게임",
            StreamContentType.Song => "오늘 노래",
            StreamContentType.Reaction => "오늘 리액션",
            _ => ""
        };

        static Color ShowLineAccent(StreamContentType type) => type switch
        {
            StreamContentType.Talk => Palette.Pink,
            StreamContentType.Game => Palette.Troll,
            StreamContentType.Song => Palette.Gold,
            StreamContentType.Reaction => Palette.PastelDim,
            _ => Palette.Muted
        };

        void ApplyEndingOverlay(GameRunState run, Week5Balance w5)
        {
            bool show = ShouldShowEnding(run, w5);
            _endingRoot.SetActive(show);
            if (!show)
            {
                _retire.gameObject.SetActive(false);
                return;
            }

            var kind = run.lastEnding == EndingKind.None
                ? Week5Rules.ResolveEnding(run, w5, run.retirePicked)
                : run.lastEnding;
            _endingTitle.text = Week5Rules.EndingTitle(kind);
            _endingBody.text = Week5Rules.EndingBody(kind);
            if (_endingHeadline != null)
                _endingHeadline.text = DayHeadline.Build(run);
            _endingPortrait?.PoseEnding(kind);
            bool offerRetire = Week5Rules.CanOfferRetire(run, w5) && !run.retirePicked;
            _retire.gameObject.SetActive(offerRetire);
            _next.gameObject.SetActive(false);
            _repay.gameObject.SetActive(false);
            _restart.gameObject.SetActive(false);
            _bookConcert.gameObject.SetActive(false);
            _concertLive.gameObject.SetActive(false);
            _foundAgency.gameObject.SetActive(false);
            _scout.gameObject.SetActive(false);
            _signSponsor.gameObject.SetActive(false);
            _produce.gameObject.SetActive(false);
            _clipYes.gameObject.SetActive(false);
            _clipNo.gameObject.SetActive(false);
            _letter.gameObject.SetActive(false);
            _auto.gameObject.SetActive(false);
            _soothe.gameObject.SetActive(false);
            _style.gameObject.SetActive(false);
        }

        void StartSettleBgm()
        {
            var clip = Resources.Load<AudioClip>("Audio/bgm_settlement");
            if (clip == null)
                return;
            _settleBgm = gameObject.AddComponent<AudioSource>();
            _settleBgm.clip = clip;
            _settleBgm.loop = true;
            _settleBgm.playOnAwake = false;
            _settleBgm.volume = 0.16f;
            _settleBgm.Play();
            _settleSfx = gameObject.AddComponent<AudioSource>();
            _settleSfx.playOnAwake = false;
            _clearCue = Resources.Load<AudioClip>("Audio/sfx_clear");
            _bankruptCue = Resources.Load<AudioClip>("Audio/sfx_bankrupt");
            _nextDayCue = Resources.Load<AudioClip>("Audio/sfx_nextday");
            _letterCue = Resources.Load<AudioClip>("Audio/sfx_letter");
            _memberCue = Resources.Load<AudioClip>("Audio/sfx_membership");
            _clipCue = Resources.Load<AudioClip>("Audio/sfx_clip");
            _goodsCue = Resources.Load<AudioClip>("Audio/sfx_goods");
            _agencyCue = Resources.Load<AudioClip>("Audio/sfx_agency");
            _rankingCue = Resources.Load<AudioClip>("Audio/sfx_ranking");
            _concertBookCue = Resources.Load<AudioClip>("Audio/sfx_concert_book");
        }

        void PlaySettleSfx(AudioClip clip, float volume)
        {
            if (_settleSfx != null && clip != null)
                _settleSfx.PlayOneShot(clip, volume);
        }

        void PlayNextDaySfx()
        {
            if (_settleSfx != null && _nextDayCue != null)
                _settleSfx.PlayOneShot(_nextDayCue, 0.46f);
        }

        void PlayLetterSfx()
        {
            if (_settleSfx != null && _letterCue != null)
                _settleSfx.PlayOneShot(_letterCue, 0.44f);
        }

        void QuietSettleBgm()
        {
            if (_settleBgm == null || !_settleBgm.isPlaying)
                return;
            StartCoroutine(FadeSettleBgmThen(null));
        }

        void PlayMemberSfx()
        {
            if (_settleSfx != null && _memberCue != null)
                _settleSfx.PlayOneShot(_memberCue, 0.48f);
        }

        void LeaveSettle(System.Action next)
        {
            if (_leavingSettle)
                return;
            _leavingSettle = true;
            StartCoroutine(FadeSettleBgmThen(next));
        }

        void PlayClipSfx()
        {
            if (_settleSfx != null && _clipCue != null)
                _settleSfx.PlayOneShot(_clipCue, 0.46f);
        }

        IEnumerator FadeSettleBgmThen(System.Action next)
        {
            if (_settleBgm != null && _settleBgm.isPlaying)
            {
                float start = _settleBgm.volume;
                float t = 0f;
                const float fade = 0.2f;
                while (t < fade)
                {
                    t += Time.deltaTime;
                    _settleBgm.volume = Mathf.Lerp(start, 0f, t / fade);
                    yield return null;
                }
                _settleBgm.Stop();
            }
            next?.Invoke();
        }

        void PlayRankingSfx()
        {
            if (_settleSfx != null && _rankingCue != null)
                _settleSfx.PlayOneShot(_rankingCue, 0.48f);
        }

        void PlayConcertBookSfx()
        {
            if (_settleSfx != null && _concertBookCue != null)
                _settleSfx.PlayOneShot(_concertBookCue, 0.48f);
        }

        void PlayAgencySfx()
        {
            if (_settleSfx != null && _agencyCue != null)
                _settleSfx.PlayOneShot(_agencyCue, 0.48f);
        }

        void PlayGoodsSfx()
        {
            if (_settleSfx != null && _goodsCue != null)
                _settleSfx.PlayOneShot(_goodsCue, 0.48f);
        }
    }
}
