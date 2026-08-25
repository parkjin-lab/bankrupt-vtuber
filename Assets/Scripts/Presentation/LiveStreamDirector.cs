using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BankruptVtuber
{
    public class LiveStreamDirector : MonoBehaviour
    {
        StreamSession _session;
        AvatarView _avatar;
        RectTransform _lane;
        RectTransform _hit;
        Text _viewers;
        Text _rival;
        Text _cash;
        Text _debt;
        Text _income;
        Text _mental;
        Text _timer;
        Text _billToday;
        Text _incomeNow;
        Text _remain;
        Text _hypeMul;
        Text _bankruptLeft;
        Text _sting;
        Image _raceFill;
        Image _bankruptFill;
        RectTransform _bankruptRow;
        Text _combo;
        Text _judge;
        Image _liveDot;
        Text _stub;
        Text _charge;
        RectTransform _eventRoot;
        Image _eventDim;
        Text _eventTitle;
        Text _eventBody;
        Text _eventTimer;
        RectTransform _promoRoot;
        Text _promoTitle;
        Text _promoBody;
        Text _promoTimer;
        RectTransform _lineRoot;
        Text _lineTitle;
        Text _lineBody;
        Text _lineTimer;
        bool _lineSettled;
        RectTransform _concertRoot;
        Text _concertTitle;
        Text _concertBody;
        Text _concertTimer;
        readonly Image[] _eventKeys = new Image[4];
        readonly Text[] _eventKeyLabels = new Text[4];
        readonly StreamPadButton[] _lanePads = new StreamPadButton[5];
        readonly StreamPadButton[] _eventPads = new StreamPadButton[4];
        Text _echo;
        float _echoFlash;
        Image _tensionFill;
        Image _hypeFlash;
        AudioSource _audio;
        AudioSource _bed;
        AudioClip _ok;
        AudioClip _bad;
        AudioClip _sc;
        AudioClip _comboCue;
        Image _wash;
        Image _washVeil;
        Image _chatPanel;
        Text _showTitle;
        ContentShowLook _look = ContentShowLook.For(StreamContentType.None);
        float _bedDuck;
        bool _threatGear;
        bool _threatNet;
        bool _threatRival;
        bool _threatScandal;
        bool _threatFee;
        int _feeShown;
        Image[] _gearTears;
        Image _netFx;
        Text _netLabel;
        Text[] _rivalSpam;
        Image _scandalVeil;
        Text _feeChip;
        RectTransform _chatRoot;

        readonly Dictionary<ChatNote, RectTransform> _views = new Dictionary<ChatNote, RectTransform>();
        float _judgeFlash;
        float _judgePop;
        float _judgePopMax;
        bool _judgeBig;
        float _shownViewers;
        float _shownIncome;
        float _stingFlash;
        float _viewerFlash;
        int _lastCombo;
        int _tonightBills;
        int _bankruptAt;
        int _lastMental;
        float _lastViewers;
        bool _ending;
        bool _eventWasActive;

        const float LaneTop = 260f;
        const float LaneHit = -210f;

        void Awake()
        {
            UiKit.EnsureCamera(Palette.Studio);
            UiKit.EnsureEventSystem();
            UiKit.LockUiInputForStream();
            Build();
            _audio = gameObject.AddComponent<AudioSource>();
            _audio.playOnAwake = false;
            _bed = gameObject.AddComponent<AudioSource>();
            _bed.playOnAwake = false;
            _bed.loop = true;
            _ok = ToneClip("sfx_perfect", new[] { 880f, 1320f }, 0.07f, 0.22f);
            _bad = BuzzerClip("sfx_miss", 0.12f, 0.20f);
            _sc = ToneClip("sfx_super", new[] { 523f, 659f, 784f, 1046f }, 0.06f, 0.20f);
            _comboCue = ToneClip("sfx_combo", new[] { 698f, 880f, 1174f }, 0.07f, 0.24f);
        }

        void OnDestroy()
        {
            if (_bed != null)
                _bed.Stop();
            UiKit.UnlockUiInputForStream();
        }

        void Start()
        {
            var gm = GameManager.Instance;
            if (!gm.Run.billsAppliedThisDay)
            {
                EconomyRules.ApplyDailyBills(gm.Run, gm.Balance, gm.Week2, gm.Week3, gm.Week4, gm.Week5, gm.Fandom);
                gm.SaveRun();
            }
            Week3Rules.TryUnlockGoods(gm.Run, gm.Week3);
            ContentRules.ApplyStartMental(gm.Run, gm.Content, gm.Balance);
            _session = new StreamSession(
                gm.Balance,
                gm.Catalog,
                gm.Run.mental,
                gm.Run.viewerBonus,
                null,
                gm.Content,
                gm.Run.contentPicked);
            if (Week3Rules.ShouldStartRival(gm.Run, gm.Week3))
            {
                Week3Rules.MarkRivalStarted(gm.Run);
                _session.EnableRival(gm.Week3);
            }
            if (WeekSchedule.InWeek3(gm.Run) && gm.Run.goodsUnlocked)
                _session.EnablePromo(gm.Week3);
            if (gm.Run.sponsorActive && !WeekSchedule.InWeek5(gm.Run))
                _session.EnableSponsorLine(gm.Week4);
            if (Week5Rules.ConcertStreamReady(gm.Run))
            {
                Week5Rules.MarkConcertStarted(gm.Run);
                _session.EnableConcert(gm.Week5);
            }
            _shownViewers = _session.Viewers;
            _lastViewers = _session.Viewers;
            _lastMental = _session.Mental;
            _lastCombo = _session.Combo;
            _tonightBills = EconomyRules.TonightBills(gm.Run);
            _bankruptAt = EconomyRules.BankruptDebt(gm.Run, gm.Balance, gm.Week2, gm.Week3, gm.Week4, gm.Week5);
            var fandom = gm.Fandom;
            string minjun = gm.Run.minjunPresent && fandom != null ? fandom.minjunName : null;
            string haeun = gm.Run.haeunPresent && fandom != null ? fandom.haeunName : null;
            _session.BindNamedFans(
                minjun,
                gm.Run.minjunPresent && gm.Run.minjunIgnoreSettlements > 0,
                haeun,
                gm.Run.haeunPresent && gm.Run.haeunHurtThisDay,
                fandom != null ? fandom.haeunHurtStreak : 0);
            ApplyContentShow(ContentShowLook.For(gm.Run.contentPicked));
            ApplyThreatShow(gm.Run);
            _avatar.SetViewers(_shownViewers);
        }

        void Update()
        {
            if (_session == null || _ending)
                return;

            float dt = Time.deltaTime;
            _session.Tick(dt);
            if (_session.Combo >= 5 && _lastCombo < 5)
                PlaySfx(_comboCue, 0.52f);
            _lastCombo = _session.Combo;
            if (UnityEngine.EventSystems.EventSystem.current != null)
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);

            if (_session.EventActive)
            {
                if (StreamBindings.EventKeyPressed(out int idx))
                {
                    Echo($"입력됨 {idx}", EventPad(idx));
                    _session.TryEventKey(idx);
                }
                StreamBindings.DiscardLaneQueue();
            }
            else if (_session.PromoActive)
            {
                if (StreamBindings.PromoConfirmDown())
                {
                    Echo("입력됨 홍보");
                    _session.TryPromo(true);
                }
                else if (StreamBindings.PromoSkipDown())
                {
                    Echo("입력됨 넘김");
                    _session.TryPromo(false);
                }
                StreamBindings.DiscardLaneQueue();
            }
            else if (_session.LineActive)
            {
                if (StreamBindings.PromoConfirmDown())
                {
                    Echo("입력됨 멘트");
                    _session.TryLine(true);
                }
                else if (StreamBindings.PromoSkipDown())
                {
                    Echo("입력됨 넘김");
                    _session.TryLine(false);
                }
                StreamBindings.DiscardLaneQueue();
            }
            else if (_session.ConcertActive)
            {
                if (StreamBindings.PromoConfirmDown())
                {
                    Echo("입력됨 퍼포먼스");
                    _session.TryConcert(true);
                }
                else if (StreamBindings.PromoSkipDown())
                {
                    Echo("입력됨 넘김");
                    _session.TryConcert(false);
                }
                StreamBindings.DiscardLaneQueue();
            }
            else if (StreamBindings.TryConsumeKind(out var kind, out var hold))
            {
                Echo($"입력됨 {Palette.LabelFor(kind)}", LanePad(kind));
                _session.TryHit(kind, _session.Elapsed, hold);
            }

            MaybeSettleSponsorLine();

            if (_eventWasActive && !_session.EventActive && _session.Event.Resolved)
            {
                bool okHit = _session.Event.Success;
                _judge.text = okHit
                    ? StreamEventState.SuccessCopy(_session.Event.Kind)
                    : StreamEventState.FailCopy(_session.Event.Kind);
                _judge.color = okHit ? Palette.CashGreen : Palette.MoneyRed;
                _judgeFlash = 1f;
                PlaySfx(okHit ? _ok : _bad, 0.5f);
            }
            _eventWasActive = _session.EventActive;

            if (_session.LastJudgement.HasValue && _session.LastResolved != null)
            {
                var j = _session.LastJudgement.Value;
                var note = _session.LastResolved;
                _session.LastJudgement = null;
                _session.LastResolved = null;
                ShowJudge(j, note);
                _avatar.React(j, note.IsSuperchat);
                if (j == Judgement.Miss)
                {
                    float dv = _session.Viewers - _lastViewers;
                    int dm = _session.Mental - _lastMental;
                    ShowMissSting(dv, dm);
                    PlaySfx(_bad, 0.48f);
                }
                else if (note.IsSuperchat)
                    PlaySfx(_sc, 0.55f);
                else if (j == Judgement.Perfect)
                    PlaySfx(_ok, 0.42f);
                else
                    PlaySfx(_ok, 0.22f);
            }

            SyncNotes();
            RefreshEventOverlay();
            RefreshPromoOverlay();
            RefreshLineOverlay();
            RefreshConcertOverlay();
            float viewerSpeed = _stingFlash > 0.4f ? 220f : 80f;
            _shownViewers = Mathf.MoveTowards(_shownViewers, _session.Viewers, dt * viewerSpeed);
            float incomeSpeed = 90f * StreamRules.IncomeMultiplier(
                _session.PerfectCombo,
                _session.HypeActive,
                _session.Balance);
            int live = _session.ForceEnded ? _session.PayoutIncome : _session.LiveIncome;
            _shownIncome = Mathf.MoveTowards(_shownIncome, live, dt * incomeSpeed);
            _avatar.SetViewers(_shownViewers);
            RefreshHud();
            _lastViewers = _session.Viewers;
            _lastMental = _session.Mental;
            _avatar.Tick(dt);

            _judgeFlash = Mathf.MoveTowards(_judgeFlash, 0f, dt * 2.2f);
            var jc = _judge.color;
            jc.a = _judgeFlash;
            _judge.color = jc;
            if (_judgePop > 0f)
            {
                _judgePop = Mathf.MoveTowards(_judgePop, 0f, dt);
                float u = _judgePopMax <= 0.001f ? 0f : Mathf.Clamp01(_judgePop / _judgePopMax);
                float s = _judgeBig ? 1f + 0.58f * u : 1f + 0.18f * u;
                _judge.rectTransform.localScale = Vector3.one * s;
            }
            else
            {
                _judge.rectTransform.localScale = Vector3.one;
            }
            if (_liveDot != null)
                _liveDot.color = new Color(1f, 1f, 1f, 0.4f + 0.6f * Mathf.Abs(Mathf.Sin(Time.time * 6f)));
            _stingFlash = Mathf.MoveTowards(_stingFlash, 0f, dt * 1.4f);
            _viewerFlash = Mathf.MoveTowards(_viewerFlash, 0f, dt * 1.8f);
            if (_sting != null)
            {
                var st = _sting.color;
                st.a = _stingFlash;
                _sting.color = st;
                _sting.rectTransform.localScale = Vector3.one * (1f + 0.28f * _stingFlash);
            }
            _echoFlash = Mathf.MoveTowards(_echoFlash, 0f, dt * 1.6f);
            if (_echo != null)
            {
                var ec = _echo.color;
                ec.a = _echoFlash;
                _echo.color = ec;
            }
            var sc = _stub.color;
            sc.a = Mathf.MoveTowards(sc.a, 0f, dt * 0.7f);
            _stub.color = sc;

            var hype = _hypeFlash.color;
            hype.a = _session.HypeActive ? 0.16f + Mathf.Sin(Time.time * 8f) * 0.05f : 0f;
            _hypeFlash.color = hype;
            _bedDuck = Mathf.MoveTowards(_bedDuck, 0f, dt * 1.8f);
            if (_bed != null)
                _bed.volume = Mathf.Lerp(_look.BedVolume, _look.BedVolume * 0.28f, _bedDuck);
            TickThreatFx();

            if (_session.Finished)
                StartCoroutine(EndRoutine());
        }

        System.Collections.IEnumerator EndRoutine()
        {
            _ending = true;
            var gm = GameManager.Instance;
            var paid = EconomyRules.ApplyStreamPayout(
                gm.Run,
                _session.TickIncome,
                _session.SuperchatIncome,
                _session.ForceEnded,
                gm.Balance);
            gm.Run.mental = _session.Mental;
            gm.Run.lastPerfects = _session.Perfects;
            gm.Run.lastGreats = _session.Greats;
            gm.Run.lastGoods = _session.Goods;
            gm.Run.lastMisses = _session.Misses;
            gm.Run.lastPeakCombo = _session.PeakCombo;
            gm.Run.lastHadHype = _session.HadHype;
            gm.Run.lastStreamEventHappened = _session.Event.Fired;
            gm.Run.lastStreamEventName = StreamEventState.DisplayName(_session.Event.Kind);
            gm.Run.lastStreamEventSuccess = _session.Event.Success;
            gm.Run.lastStreamPeakViewers = _session.PeakViewers;
            gm.Run.lastGoodsPromoSuccess = _session.Promo.Success;
            gm.Run.lastConcertPerformanceSuccess = _session.Concert.Success;
            MaybeSettleSponsorLine();
            Week5Rules.NoteZeroMentalDay(gm.Run);
            Week2Rules.AfterStream(
                gm.Run,
                _session.PeakViewers,
                _session.ForceEnded,
                _session.HadHype,
                _session.Misses,
                gm.Week2);
            gm.Run.lastMissStreak = _session.PeakMissStreak;
            gm.Run.lastHadSuccessfulSuperchat = _session.HadSuccessfulSuperchat;
            FandomRules.AfterStream(gm.Run, gm.Balance, gm.Fandom);
            ContentRules.AfterStream(gm.Run, gm.Content, gm.Fandom);
            Week3Rules.ApplyRivalResult(
                gm.Run,
                gm.Balance,
                gm.Week3,
                _session.Viewers,
                _session.RivalViewers,
                _session.RivalActive);
            _judge.text = _session.ForceEnded ? "멘탈 붕괴 — 강제 종료" : "방송 종료";
            _judge.color = Color.white;
            _judgeFlash = 1f;
            yield return new WaitForSeconds(1.1f);
            Debug.Log("[파산 버튜버] stream payout " + paid);
            gm.GoSettlement();
        }

        void Build()
        {
            var canvas = UiKit.CreateCanvas("LiveCanvas", transform);
            canvas.gameObject.AddComponent<StreamPointerRelay>();
            var canvasRoot = canvas.transform;

            _wash = UiKit.Image(canvasRoot, "Wash", Palette.Studio);
            UiKit.Stretch(_wash.rectTransform);
            _washVeil = UiKit.Image(canvasRoot, "WashVeil", new Color(0, 0, 0, 0));
            UiKit.Stretch(_washVeil.rectTransform);

            var safe = UiKit.Panel(canvasRoot, "Safe", new Color(0, 0, 0, 0));
            UiKit.Stretch(safe);
            var safeImg = safe.GetComponent<Image>();
            if (safeImg != null)
                safeImg.raycastTarget = false;
            safe.gameObject.AddComponent<StreamSafeArea>();
            var root = safe;

            _hypeFlash = UiKit.Image(root, "HypeFlash", new Color(1f, 0.82f, 0.25f, 0f));
            UiKit.Stretch(_hypeFlash.rectTransform);
            _hypeFlash.raycastTarget = false;

            var top = UiKit.Panel(root, "Top", new Color(0.08f, 0.04f, 0.1f, 0.90f));
            UiKit.Layout(top, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), Vector2.zero, new Vector2(0, 200));

            var liveBox = UiKit.Panel(top, "LiveHud", new Color(0.86f, 0.12f, 0.22f, 0.96f));
            liveBox.anchorMin = new Vector2(0f, 1f);
            liveBox.anchorMax = new Vector2(0.16f, 1f);
            liveBox.pivot = new Vector2(0.5f, 1f);
            liveBox.anchoredPosition = new Vector2(0f, -8f);
            liveBox.sizeDelta = new Vector2(-8f, 48f);
            _liveDot = UiKit.Image(liveBox, "Dot", Color.white);
            UiKit.Layout(_liveDot.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(16f, 0f), new Vector2(10f, 10f));
            var liveL = UiKit.Label(liveBox, "L", "LIVE", 18, Color.white, TextAnchor.MiddleLeft, FontStyle.Bold);
            UiKit.Layout(liveL.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0f, 0.5f), new Vector2(28f, 0f), new Vector2(-16f, 0f));

            _viewers = Chip(top, "Viewers", "시청자", 0.16f, 0.40f, -6f);
            _rival = Chip(top, "Rival", "라이벌", 0.40f, 0.64f, -6f);
            _timer = Chip(top, "Timer", "남은 시간", 0.64f, 1f, -6f);
            _cash = Chip(top, "Cash", "현금", 0f, 0.25f, -64f);
            _debt = Chip(top, "Debt", "부채", 0.25f, 0.50f, -64f);
            _income = Chip(top, "Income", "실시간 수익", 0.50f, 0.75f, -64f);
            _mental = Chip(top, "Mental", "멘탈", 0.75f, 1f, -64f);
            _billToday = Chip(top, "TonightBills", "오늘 청구", 0f, 0.25f, -124f);
            _incomeNow = Chip(top, "TonightIncome", "지금 수입", 0.25f, 0.50f, -124f);
            _remain = Chip(top, "Remain", "남은 금액", 0.50f, 0.78f, -124f);
            _bankruptLeft = Chip(top, "ToBankrupt", "파산까지", 0.78f, 1f, -124f);
            _bankruptRow = _bankruptLeft.transform.parent as RectTransform;
            _bankruptFill = UiKit.Image(_bankruptRow, "BankruptFill", Palette.MoneyRed);
            UiKit.Layout(_bankruptFill.rectTransform, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), new Vector2(0, 3), new Vector2(-10, 5));
            _bankruptFill.raycastTarget = false;
            _bankruptFill.gameObject.SetActive(false);
            _hypeMul = UiKit.Label(top, "HypeMul", "", 14, Palette.Gold, TextAnchor.MiddleLeft, FontStyle.Bold);
            UiKit.Layout(_hypeMul.rectTransform, new Vector2(0.25f, 1), new Vector2(0.50f, 1), new Vector2(0, 1), new Vector2(12, -176), new Vector2(-16, 18));
            var raceBg = UiKit.Image(top, "RaceBg", new Color(1, 1, 1, 0.12f));
            UiKit.Layout(raceBg.rectTransform, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), new Vector2(0, 6), new Vector2(-16, 8));
            _raceFill = UiKit.Image(raceBg.transform, "RaceFill", Palette.MoneyRed);
            UiKit.Stretch(_raceFill.rectTransform);
            _rival.transform.parent.gameObject.SetActive(false);

            _showTitle = UiKit.Label(root, "ShowTitle", "", 34, Palette.Gold, TextAnchor.MiddleLeft, FontStyle.Bold);
            UiKit.Layout(_showTitle.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(24, -212), new Vector2(420, 44));

            _avatar = new AvatarView(root as RectTransform);

            var chatPanel = UiKit.Panel(root, "Chat", new Color(0.07f, 0.05f, 0.1f, 0.88f));
            _chatPanel = chatPanel.GetComponent<Image>();
            _chatRoot = chatPanel;
            UiKit.Layout(chatPanel, new Vector2(1, 0), new Vector2(1, 1), new Vector2(1, 0.5f), new Vector2(-18, 0), new Vector2(420, -220));
            UiKit.Label(chatPanel, "ChatTitle", "실시간 채팅", 22, Palette.Pastel, TextAnchor.UpperLeft, FontStyle.Bold);
            var ct = chatPanel.Find("ChatTitle") as RectTransform;
            UiKit.Layout(ct, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -10), new Vector2(-24, 30));

            _lane = UiKit.Panel(chatPanel, "Lane", new Color(1, 1, 1, 0.03f));
            UiKit.Stretch(_lane, 12, 12, 44, 70);
            var laneFade = _lane.gameObject.AddComponent<CanvasGroup>();
            laneFade.blocksRaycasts = false;
            laneFade.interactable = false;

            _hit = UiKit.Panel(_lane, "Hit", new Color(1f, 1f, 1f, 0.22f));
            UiKit.Layout(_hit, new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, LaneHit), new Vector2(0, 10));

            var hitLabel = UiKit.Label(_lane, "HitL", "타이밍", 16, Palette.Pastel, TextAnchor.MiddleRight);
            UiKit.Layout(hitLabel.rectTransform, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-4, LaneHit + 18), new Vector2(80, 20));

            var bottom = UiKit.Panel(root, "Bottom", new Color(0.08f, 0.04f, 0.1f, 0.82f));
            UiKit.Layout(bottom, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), Vector2.zero, new Vector2(0, 200));

            _combo = UiKit.Label(bottom, "Combo", "COMBO 0", 22, Palette.Pastel, TextAnchor.MiddleLeft, FontStyle.Bold);
            UiKit.Layout(_combo.rectTransform, new Vector2(0, 0.70f), new Vector2(0.55f, 1), new Vector2(0, 1), new Vector2(12, -4), new Vector2(0, 36));

            var tensionBg = UiKit.Image(bottom, "TensionBg", new Color(1, 1, 1, 0.12f));
            UiKit.Layout(tensionBg.rectTransform, new Vector2(0, 0.62f), new Vector2(0.40f, 0.70f), new Vector2(0, 0.5f), new Vector2(12, 0), new Vector2(0, 0));
            _tensionFill = UiKit.Image(tensionBg.transform, "Fill", Palette.Troll);
            UiKit.Stretch(_tensionFill.rectTransform);
            var tlab = UiKit.Label(bottom, "TensionL", "텐션 (미스 스트릭)", 12, Palette.Muted, TextAnchor.LowerLeft);
            UiKit.Layout(tlab.rectTransform, new Vector2(0, 0.54f), new Vector2(0.40f, 0.62f), new Vector2(0, 0), new Vector2(12, 0), Vector2.zero);

            var keys = UiKit.Label(bottom, "Keys", "←↓→↑  ·  A/S/D/F  ·  WASD  ·  Space 슈퍼챗  ·  1–4 이벤트", 14, Palette.PastelDim, TextAnchor.MiddleLeft);
            UiKit.Layout(keys.rectTransform, new Vector2(0.42f, 0.70f), new Vector2(1, 1), new Vector2(1, 1), new Vector2(-12, -4), new Vector2(0, 36));

            _echo = UiKit.Label(bottom, "Echo", "", 18, Palette.Gold, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(_echo.rectTransform, new Vector2(0, 0.54f), new Vector2(1, 0.70f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            var padRow = UiKit.Panel(bottom, "PadRow", new Color(0, 0, 0, 0));
            UiKit.Layout(padRow, new Vector2(0, 0), new Vector2(1, 0.54f), new Vector2(0.5f, 0), Vector2.zero, Vector2.zero);
            var padRowImg = padRow.GetComponent<Image>();
            if (padRowImg != null)
                padRowImg.raycastTarget = false;
            _lanePads[0] = AddColumnPad(padRow, 0, 5, "긍정", Palette.ForKind(ChatKind.Positive), StreamPadButton.Mode.Kind, ChatKind.Positive);
            _lanePads[1] = AddColumnPad(padRow, 1, 5, "공감", Palette.ForKind(ChatKind.Empathy), StreamPadButton.Mode.Kind, ChatKind.Empathy);
            _lanePads[2] = AddColumnPad(padRow, 2, 5, "웃음", Palette.ForKind(ChatKind.Laugh), StreamPadButton.Mode.Kind, ChatKind.Laugh);
            _lanePads[3] = AddColumnPad(padRow, 3, 5, "감사", Palette.ForKind(ChatKind.Thanks), StreamPadButton.Mode.Kind, ChatKind.Thanks);
            _lanePads[4] = AddColumnPad(padRow, 4, 5, "슈퍼챗", Palette.Gold, StreamPadButton.Mode.Superchat);

            _sting = UiKit.Label(root, "MissSting", "", 40, Palette.MoneyRed, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(_sting.rectTransform, new Vector2(0.22f, 0.48f), new Vector2(0.22f, 0.48f), new Vector2(0.5f, 0.5f), new Vector2(0, 0), new Vector2(420, 80));
            _sting.color = new Color(1f, 0.18f, 0.32f, 0f);

            _judge = UiKit.Label(root, "Judge", "", 64, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(_judge.rectTransform, new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.5f), new Vector2(-80, 0), new Vector2(520, 80));

            _stub = UiKit.Label(root, "Stub", "", 22, Palette.Gold, TextAnchor.MiddleCenter);
            UiKit.Layout(_stub.rectTransform, new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.5f), new Vector2(-80, 0), new Vector2(520, 36));
            _charge = UiKit.Label(root, "Charge", "슈퍼챗 차지… 떼면 한 번만 판정", 22, Palette.Gold, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(_charge.rectTransform, new Vector2(0.5f, 0.18f), new Vector2(0.5f, 0.18f), new Vector2(0.5f, 0.5f), new Vector2(-80, 0), new Vector2(560, 36));
            _charge.gameObject.SetActive(false);

            _eventDim = UiKit.Image(root, "EventDim", new Color(0.06f, 0.03f, 0.08f, 0.55f));
            UiKit.Stretch(_eventDim.rectTransform);
            _eventDim.raycastTarget = false;

            _eventRoot = UiKit.Panel(root, "EventCard", new Color(0.16f, 0.07f, 0.12f, 0.96f));
            UiKit.Layout(_eventRoot, new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.5f), new Vector2(-80, 10), new Vector2(560, 280));
            _eventTitle = UiKit.Label(_eventRoot, "ETitle", "", 34, Palette.Gold, TextAnchor.UpperCenter, FontStyle.Bold);
            UiKit.Layout(_eventTitle.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -16), new Vector2(-24, 44));
            _eventBody = UiKit.Label(_eventRoot, "EBody", "", 20, Palette.Pastel, TextAnchor.UpperCenter);
            UiKit.Layout(_eventBody.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -64), new Vector2(-28, 52));
            _eventTimer = UiKit.Label(_eventRoot, "ETimer", "", 18, Palette.MoneyRed, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(_eventTimer.rectTransform, new Vector2(0, 0.28f), new Vector2(1, 0.36f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            var eventRow = UiKit.Panel(_eventRoot, "EKeys", new Color(0, 0, 0, 0));
            UiKit.Layout(eventRow, new Vector2(0, 0), new Vector2(1, 0.28f), new Vector2(0.5f, 0), Vector2.zero, Vector2.zero);
            var eventRowImg = eventRow.GetComponent<Image>();
            if (eventRowImg != null)
                eventRowImg.raycastTarget = false;
            for (int i = 0; i < 4; i++)
            {
                var pad = AddColumnPad(eventRow, i, 4, (i + 1).ToString(), new Color(1, 1, 1, 0.18f), StreamPadButton.Mode.Event, eventIndex: i + 1);
                _eventPads[i] = pad;
                _eventKeys[i] = pad.GetComponent<Image>();
                _eventKeyLabels[i] = pad.transform.Find("L").GetComponent<Text>();
            }
            _eventRoot.gameObject.SetActive(false);
            _eventDim.gameObject.SetActive(false);

            _promoRoot = UiKit.Panel(root, "PromoCard", new Color(0.12f, 0.08f, 0.18f, 0.96f));
            UiKit.Layout(_promoRoot, new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.5f), new Vector2(-80, 10), new Vector2(560, 280));
            _promoTitle = UiKit.Label(_promoRoot, "PTitle", "굿즈 홍보 타이밍", 34, Palette.Gold, TextAnchor.UpperCenter, FontStyle.Bold);
            UiKit.Layout(_promoTitle.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -16), new Vector2(-24, 44));
            _promoBody = UiKit.Label(_promoRoot, "PBody", "← / ↑  지금 아크릴 스탠드 홍보\n→ / ↓  넘어가기", 20, Palette.Pastel, TextAnchor.MiddleCenter);
            UiKit.Layout(_promoBody.rectTransform, new Vector2(0, 0.28f), new Vector2(1, 0.72f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(-28, 0));
            _promoTimer = UiKit.Label(_promoRoot, "PTimer", "", 18, Palette.MoneyRed, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(_promoTimer.rectTransform, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), new Vector2(0, 78), new Vector2(0, 24));
            AddOverlayChoice(_promoRoot, "홍보하기", "넘어가기");
            _promoRoot.gameObject.SetActive(false);

            _lineRoot = UiKit.Panel(root, "LineCard", new Color(0.14f, 0.09f, 0.16f, 0.96f));
            UiKit.Layout(_lineRoot, new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.5f), new Vector2(-80, 10), new Vector2(560, 280));
            _lineTitle = UiKit.Label(_lineRoot, "LTitle", "스폰서 멘트 타이밍", 34, Palette.Gold, TextAnchor.UpperCenter, FontStyle.Bold);
            UiKit.Layout(_lineTitle.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -16), new Vector2(-24, 44));
            _lineBody = UiKit.Label(_lineRoot, "LBody", "← / ↑  스폰서 멘트 넣기\n→ / ↓  놓치면 계약 종료", 20, Palette.Pastel, TextAnchor.MiddleCenter);
            UiKit.Layout(_lineBody.rectTransform, new Vector2(0, 0.28f), new Vector2(1, 0.72f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(-28, 0));
            _lineTimer = UiKit.Label(_lineRoot, "LTimer", "", 18, Palette.MoneyRed, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(_lineTimer.rectTransform, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), new Vector2(0, 78), new Vector2(0, 24));
            AddOverlayChoice(_lineRoot, "멘트 넣기", "놓치기");
            _lineRoot.gameObject.SetActive(false);

            _concertRoot = UiKit.Panel(root, "ConcertCard", new Color(0.16f, 0.07f, 0.18f, 0.96f));
            UiKit.Layout(_concertRoot, new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.5f), new Vector2(-80, 10), new Vector2(560, 280));
            _concertTitle = UiKit.Label(_concertRoot, "CTitle", "콘서트 퍼포먼스 타이밍", 34, Palette.Gold, TextAnchor.UpperCenter, FontStyle.Bold);
            UiKit.Layout(_concertTitle.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -16), new Vector2(-24, 44));
            _concertBody = UiKit.Label(_concertRoot, "CBody", "← / ↑  성공 — 정산 배율 1.3x\n→ / ↓  놓치면 배율 없음", 20, Palette.Pastel, TextAnchor.MiddleCenter);
            UiKit.Layout(_concertBody.rectTransform, new Vector2(0, 0.28f), new Vector2(1, 0.72f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(-28, 0));
            _concertTimer = UiKit.Label(_concertRoot, "CTimer", "", 18, Palette.MoneyRed, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(_concertTimer.rectTransform, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), new Vector2(0, 78), new Vector2(0, 24));
            AddOverlayChoice(_concertRoot, "성공", "넘기기");
            _concertRoot.gameObject.SetActive(false);
        }

        StreamPadButton AddColumnPad(
            Transform parent,
            int index,
            int count,
            string label,
            Color color,
            StreamPadButton.Mode mode,
            ChatKind kind = ChatKind.Positive,
            int eventIndex = 0)
        {
            var img = UiKit.Image(parent, "Pad" + label, new Color(color.r, color.g, color.b, 0.92f));
            float a = index / (float)count;
            float b = (index + 1) / (float)count;
            img.rectTransform.anchorMin = new Vector2(a, 0f);
            img.rectTransform.anchorMax = new Vector2(b, 1f);
            img.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            img.rectTransform.offsetMin = new Vector2(4f, 4f);
            img.rectTransform.offsetMax = new Vector2(-4f, -4f);
            var cap = UiKit.Label(img.transform, "L", label, count >= 5 ? 22 : 28, Palette.Ink, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Stretch(cap.rectTransform);
            return StreamPadButton.Attach(img.gameObject, mode, kind, eventIndex);
        }

        void AddOverlayChoice(Transform parent, string confirm, string skip)
        {
            var row = UiKit.Panel(parent, "ChoiceRow", new Color(0, 0, 0, 0));
            UiKit.Layout(row, new Vector2(0, 0), new Vector2(1, 0.24f), new Vector2(0.5f, 0), Vector2.zero, Vector2.zero);
            var rowImg = row.GetComponent<Image>();
            if (rowImg != null)
                rowImg.raycastTarget = false;
            AddColumnPad(row, 0, 2, confirm, Palette.PinkDeep, StreamPadButton.Mode.PromoConfirm);
            AddColumnPad(row, 1, 2, skip, Palette.Troll, StreamPadButton.Mode.PromoSkip);
        }

        void Echo(string text, StreamPadButton pad = null)
        {
            if (_echo != null)
            {
                _echo.text = text;
                var c = _echo.color;
                c.a = 1f;
                _echo.color = c;
            }
            _echoFlash = 1f;
            pad?.Flash();
        }

        StreamPadButton LanePad(ChatKind kind)
        {
            int i = (int)kind;
            if (i >= 0 && i < 4)
                return _lanePads[i];
            return _lanePads[4];
        }

        StreamPadButton EventPad(int index)
        {
            int i = index - 1;
            if (i >= 0 && i < _eventPads.Length)
                return _eventPads[i];
            return null;
        }

        void MaybeSettleSponsorLine()
        {
            if (_session == null || _lineSettled || !_session.Line.Resolved)
                return;
            _lineSettled = true;
            var gm = GameManager.Instance;
            Week4Rules.ApplySponsorLine(gm.Run, gm.Week4, _session.Line.Success);
            _session.Mental = gm.Run.mental;
        }

        Text Chip(Transform parent, string name, string label, float x0, float x1, float y)
        {
            var box = UiKit.Panel(parent, name, new Color(1, 1, 1, 0.06f));
            box.anchorMin = new Vector2(x0, 1f);
            box.anchorMax = new Vector2(x1, 1f);
            box.pivot = new Vector2(0.5f, 1f);
            box.anchoredPosition = new Vector2(0f, y);
            box.sizeDelta = new Vector2(-8f, 52f);
            UiKit.Label(box, "L", label, 13, Palette.Muted, TextAnchor.UpperLeft);
            var l = box.Find("L") as RectTransform;
            UiKit.Layout(l, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1), new Vector2(10, -3), new Vector2(-16, 16));
            var v = UiKit.Label(box, "V", "-", 20, Palette.Pastel, TextAnchor.LowerLeft, FontStyle.Bold);
            UiKit.Layout(v.rectTransform, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 0), new Vector2(10, 4), new Vector2(-16, -18));
            return v;
        }

        void RefreshHud()
        {
            _viewers.text = Mathf.RoundToInt(_shownViewers).ToString();
            _viewers.color = Color.Lerp(Palette.Pastel, Palette.MoneyRed, _viewerFlash);
            bool vs = _session.RivalActive;
            _rival.transform.parent.gameObject.SetActive(vs);
            if (vs)
            {
                _rival.text = $"{_session.RivalViewers:0}";
                _rival.color = Palette.Troll;
            }
            var run = GameManager.Instance.Run;
            _cash.text = EconomyRules.FormatWon(run.cash);
            _cash.color = Palette.CashGreen;
            _debt.text = EconomyRules.FormatWon(run.debt);
            _debt.color = Palette.MoneyRed;
            int shown = _session.ForceEnded ? _session.PayoutIncome : _session.LiveIncome;
            int ticking = Mathf.RoundToInt(_shownIncome);
            _income.text = EconomyRules.FormatWon(shown);
            _income.color = Palette.CashGreen;
            _billToday.text = EconomyRules.FormatWon(_tonightBills);
            _billToday.color = Palette.MoneyRed;
            _incomeNow.text = EconomyRules.FormatWon(ticking);
            _incomeNow.color = Palette.CashGreen;
            int remain = _tonightBills - ticking;
            bool covered = remain <= 0;
            _remain.text = covered ? "청구 커버" : EconomyRules.FormatWon(remain);
            _remain.color = covered ? Palette.CashGreen : Palette.MoneyRed;
            if (_session.HypeActive)
                _hypeMul.text = $"하이프 {_session.Balance.hypeIncomeMultiplier:0.#}x";
            else if (_session.PerfectCombo >= _session.Balance.comboIncomeThreshold)
                _hypeMul.text = $"콤보 {_session.Balance.comboIncomeMultiplier:0.#}x";
            else
                _hypeMul.text = "";
            _hypeMul.color = Palette.Gold;
            int room = _bankruptAt - run.debt;
            bool atRisk = run.cash + shown < _tonightBills;
            if (_bankruptRow != null)
                _bankruptRow.gameObject.SetActive(atRisk);
            if (atRisk)
            {
                _bankruptLeft.text = EconomyRules.FormatWon(Mathf.Max(0, room));
                _bankruptLeft.color = Palette.MoneyRed;
            }
            if (_raceFill != null)
            {
                float u = _tonightBills <= 0 ? 1f : Mathf.Clamp01(ticking / (float)_tonightBills);
                _raceFill.rectTransform.anchorMax = new Vector2(u, 1f);
                _raceFill.color = covered ? Palette.CashGreen : Palette.MoneyRed;
            }
            if (_bankruptFill != null)
            {
                _bankruptFill.gameObject.SetActive(atRisk);
                if (atRisk)
                {
                    float risk = _bankruptAt <= 0 ? 1f : Mathf.Clamp01(run.debt / (float)_bankruptAt);
                    _bankruptFill.rectTransform.anchorMax = new Vector2(risk, 1f);
                }
            }
            _mental.text = $"{_session.Mental}/{_session.Balance.maxMental}";
            _mental.color = _session.Mental <= 24 ? Palette.MoneyRed : Palette.Pink;
            _timer.text = $"{Mathf.CeilToInt(_session.TimeLeft)}s";
            if (_session.IncomeFreezeLeft > 0f)
                _combo.text = $"송출 끊김 {_session.IncomeFreezeLeft:0.0}s";
            else if (_session.IncomeShieldLeft > 0f)
                _combo.text = $"수익 보호막 {_session.IncomeShieldLeft:0.0}s";
            else if (_session.HypeActive)
                _combo.text = $"{_session.Tuning.Name}  ·  하이프 {_session.HypeLeft:0.0}s  ·  {_session.Balance.hypeIncomeMultiplier:0.#}x";
            else
                _combo.text = $"{_session.Tuning.Name}  ·  COMBO {_session.Combo}   PERFECT {_session.PerfectCombo}";
            _combo.color = _session.HypeActive ? Palette.Gold : Palette.Pastel;
            float tension = Mathf.Clamp01(_session.MissStreak / (float)_session.Balance.missStreakMental);
            _tensionFill.rectTransform.anchorMax = new Vector2(tension, 1f);
        }

        void SyncNotes()
        {
            foreach (var note in _session.Notes)
            {
                if (note.Consumed)
                {
                    if (_views.TryGetValue(note, out var dead))
                    {
                        Destroy(dead.gameObject);
                        _views.Remove(note);
                    }
                    continue;
                }

                if (!_views.TryGetValue(note, out var rt))
                {
                    rt = MakeBubble(note);
                    _views[note] = rt;
                }

                float span = note.HitTime - note.SpawnTime;
                float u = span <= 0.001f ? 1f : (_session.Elapsed - note.SpawnTime) / span;
                float y = Mathf.Lerp(LaneTop, LaneHit, Mathf.Clamp01(u));
                float jitter = _look.LaneJitter;
                float x = 0f;
                float tilt = 0f;
                if (jitter > 0.01f)
                {
                    float h = Mathf.Repeat(note.SpawnTime * 17.3f, 1f);
                    x = (h - 0.5f) * jitter * 36f + Mathf.Sin(Time.time * 6.5f + note.SpawnTime) * jitter * 10f;
                    tilt = (h - 0.5f) * jitter * 8f;
                }
                rt.anchoredPosition = new Vector2(x, y);
                rt.localEulerAngles = new Vector3(0f, 0f, tilt);
                if (note.IsSuperchat)
                {
                    float slam = Mathf.Clamp01((_session.Elapsed - note.SpawnTime) / 0.18f);
                    float s = Mathf.Lerp(1.38f, 1f, slam * slam);
                    rt.localScale = Vector3.one * s;
                }
                else
                {
                    rt.localScale = Vector3.one;
                }
                if (note.FanWounded)
                    DimNamedBubble(rt);
            }

            var fade = _lane.GetComponent<CanvasGroup>();
            if (fade != null)
            {
                bool overlay = _session.EventActive || _session.PromoActive || _session.LineActive || _session.ConcertActive;
                fade.alpha = overlay ? 0.38f : 1f;
            }
        }

        void RefreshPromoOverlay()
        {
            bool on = _session.PromoActive;
            _promoRoot.gameObject.SetActive(on);
            if (on)
            {
                _eventDim.gameObject.SetActive(true);
                _promoTimer.text = $"{_session.Promo.TimeLeft:0.00}s";
            }
        }

        void RefreshLineOverlay()
        {
            bool on = _session.LineActive;
            _lineRoot.gameObject.SetActive(on);
            if (on)
            {
                _eventDim.gameObject.SetActive(true);
                _lineTimer.text = $"{_session.Line.TimeLeft:0.00}s";
            }
        }

        void RefreshConcertOverlay()
        {
            bool on = _session.ConcertActive;
            _concertRoot.gameObject.SetActive(on);
            if (on)
            {
                _eventDim.gameObject.SetActive(true);
                _concertTimer.text = $"{_session.Concert.TimeLeft:0.00}s";
            }
        }

        void RefreshEventOverlay()
        {
            bool on = _session.EventActive;
            _eventRoot.gameObject.SetActive(on);
            _eventDim.gameObject.SetActive(on || _session.PromoActive || _session.LineActive || _session.ConcertActive);
            _charge.gameObject.SetActive(!on && !_session.PromoActive && !_session.LineActive && !_session.ConcertActive && StreamBindings.SuperchatCharging);
            if (!on)
                return;

            _eventTitle.text = StreamEventState.DisplayName(_session.Event.Kind);
            _eventBody.text = StreamEventState.Prompt(_session.Event.Kind);
            _eventTimer.text = $"{_session.Event.TimeLeft:0.00}s";
            int target = _session.Event.TargetKey;
            for (int i = 0; i < 4; i++)
            {
                bool hot = i + 1 == target;
                float pulse = hot ? 0.55f + 0.45f * Mathf.Abs(Mathf.Sin(Time.time * 10f)) : 0.12f;
                _eventKeys[i].color = hot ? new Color(1f, 0.82f, 0.25f, pulse) : new Color(1f, 1f, 1f, 0.12f);
                _eventKeyLabels[i].color = hot ? Palette.Ink : Palette.Pastel;
            }
        }

        RectTransform MakeBubble(ChatNote note)
        {
            bool super = note.IsSuperchat;
            bool troll = !super && note.Kind == ChatKind.Laugh;
            bool named = note.NamedFan;
            var color = super ? Palette.Gold : Palette.ForKind(note.Kind);
            if (named && !super)
                color = Palette.Pink;
            if (_look.WarmChat && !super && !named && (note.Kind == ChatKind.Positive || note.Kind == ChatKind.Empathy))
                color = Color.Lerp(color, Color.white, 0.08f);
            if (_look.LoudTroll && troll)
                color = Color.Lerp(Palette.Troll, Palette.MoneyRed, 0.35f);
            if (_look.GoldSparkle && !troll && !named)
                color = Color.Lerp(color, Palette.Gold, 0.28f);
            var card = UiKit.Panel(_lane, "Note", Color.white);
            float scale = _look.BubbleScale > 0.1f ? _look.BubbleScale : 1f;
            if (_look.LoudTroll && troll)
                scale *= 1.12f;
            float h = (named || super ? 92f : troll ? 72f : 64f) * scale;
            float w = (super || named ? 400f : 372f) * scale;
            UiKit.Layout(card, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(w, h));
            var img = card.GetComponent<Image>();
            float a = _look.DimWash ? 0.80f : 0.94f;
            if (named && super)
                ArtSprites.ApplySliced(img, ArtSprites.SuperchatBanner, note.FanWounded ? new Color(0.72f, 0.62f, 0.28f, 0.72f) : new Color(1f, 0.86f, 0.28f, 1f), new Vector4(36f, 28f, 36f, 28f));
            else if (named)
                ArtSprites.ApplySliced(img, ArtSprites.BubblePill, note.FanWounded ? new Color(0.72f, 0.42f, 0.55f, 0.55f) : Palette.Pink);
            else if (super)
                ArtSprites.ApplySliced(img, ArtSprites.SuperchatBanner, new Color(1f, 0.86f, 0.28f, 1f), new Vector4(36f, 28f, 36f, 28f));
            else if (troll)
            {
                ArtSprites.Apply(img, ArtSprites.TrollBubble, color, color);
                img.preserveAspect = false;
            }
            else
                ArtSprites.ApplySliced(img, ArtSprites.BubblePill, new Color(color.r, color.g, color.b, a));

            string key = super ? "SPACE" : note.Kind switch
            {
                ChatKind.Positive => "←",
                ChatKind.Empathy => "↓",
                ChatKind.Laugh => "→",
                _ => "↑"
            };
            string kind = super ? "슈퍼챗" : troll ? "트롤" : Palette.LabelFor(note.Kind);
            string fanTag = "";
            if (named)
            {
                var gm = GameManager.Instance;
                var f = gm != null ? gm.Fandom : null;
                if (f != null && note.User == f.minjunName)
                    fanTag = "슈퍼팬 · 첫 도네";
                else if (f != null && note.User == f.haeunName)
                    fanTag = "슈퍼팬 · 매일 오는 야간";
                else
                    fanTag = "슈퍼팬";
            }
            var keyCol = super || troll || named ? Palette.Ink : Color.white;
            var keyT = UiKit.Label(card, "Key", key, super ? 16 : 18, keyCol, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(keyT.rectTransform, new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0.5f), new Vector2(18, 0), new Vector2(56, 0));
            string body = named
                ? (super
                    ? $"{note.User}  ·  {fanTag}\n{kind}  {EconomyRules.FormatWon(note.SuperchatWon)}  {note.Text}"
                    : $"{note.User}  ·  {fanTag}\n{note.Text}")
                : super
                    ? $"{note.User}  ·  {kind}  {EconomyRules.FormatWon(note.SuperchatWon)}\n{note.Text}"
                    : $"{note.User}  ·  {kind}\n{note.Text}";
            var msgCol = troll ? Color.white : Palette.Ink;
            if (named && note.FanWounded)
                msgCol = new Color(msgCol.r, msgCol.g, msgCol.b, 0.55f);
            var msg = UiKit.Label(card, "Msg", body, super ? 16 : 17, msgCol, TextAnchor.MiddleLeft, FontStyle.Bold);
            UiKit.Layout(msg.rectTransform, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 0.5f), new Vector2(76, 0), new Vector2(-88, 0));
            msg.horizontalOverflow = HorizontalWrapMode.Wrap;
            if (super)
                card.localScale = Vector3.one * 1.38f;
            return card;
        }

        static void DimNamedBubble(RectTransform rt)
        {
            if (rt == null)
                return;
            var img = rt.GetComponent<Image>();
            if (img != null && img.color.a > 0.74f)
            {
                var c = img.color;
                img.color = new Color(c.r * 0.78f, c.g * 0.78f, c.b * 0.78f, 0.62f);
            }
            var msg = rt.Find("Msg");
            if (msg != null)
            {
                var t = msg.GetComponent<Text>();
                if (t != null && t.color.a > 0.6f)
                {
                    var c = t.color;
                    t.color = new Color(c.r, c.g, c.b, 0.55f);
                }
            }
        }

        void ShowJudge(Judgement j, ChatNote note)
        {
            _judge.text = j switch
            {
                Judgement.Perfect => note.IsSuperchat ? "PERFECT 슈퍼챗" : "PERFECT",
                Judgement.Great => "GREAT",
                Judgement.Good => "GOOD",
                _ => "MISS"
            };
            _judge.color = j switch
            {
                Judgement.Perfect => Palette.Gold,
                Judgement.Great => Palette.Green,
                Judgement.Good => Palette.Blue,
                _ => Palette.MoneyRed
            };
            _judgeFlash = 1f;
            _judgeBig = j == Judgement.Perfect || j == Judgement.Miss;
            _judgePopMax = _judgeBig ? 0.25f : 0.12f;
            _judgePop = _judgePopMax;
            _judge.rectTransform.localScale = Vector3.one * (_judgeBig ? 1.58f : 1.18f);
        }

        void ShowMissSting(float viewerDelta, int mentalDelta)
        {
            float drop = Mathf.Abs(Mathf.Min(0f, viewerDelta));
            string line = $"시청자 −{drop:0.0} / 멘탈";
            if (mentalDelta != 0)
                line = $"시청자 −{drop:0.0} / 멘탈 {mentalDelta}";
            _sting.text = line;
            var c = Palette.MoneyRed;
            c.a = 1f;
            _sting.color = c;
            _stingFlash = 1.15f;
            _viewerFlash = 1f;
        }

        void ApplyContentShow(ContentShowLook look)
        {
            _look = look;
            if (_wash != null)
                _wash.color = look.Wash;
            if (_washVeil != null)
                _washVeil.color = look.WashVeil;
            if (_chatPanel != null)
                _chatPanel.color = look.Lane;
            if (_showTitle != null)
            {
                _showTitle.text = look.OverlayTitle;
                _showTitle.color = look.Type == StreamContentType.Game ? Palette.Troll
                    : look.Type == StreamContentType.Reaction ? Palette.PastelDim
                    : look.Card;
            }
            UiKit.EnsureCamera(look.Wash);
            _avatar?.ApplyShow(look);
            if (look.Type == StreamContentType.Talk)
                _ok = ToneClip("sfx_perfect", new[] { 660f, 880f }, 0.08f, 0.16f);
            else if (look.Type == StreamContentType.Song)
                _ok = ToneClip("sfx_perfect", new[] { 1046f, 1480f, 1760f }, 0.06f, 0.22f);
            if (look.Type == StreamContentType.Game)
                _bad = BuzzerClip("sfx_miss", 0.18f, 0.28f);
            else if (look.Type == StreamContentType.Talk)
                _bad = BuzzerClip("sfx_miss", 0.09f, 0.14f);
            if (_bed != null)
            {
                _bed.clip = BedClip(look.Type);
                _bed.volume = look.BedVolume;
                _bed.Play();
            }
        }

        void ApplyThreatShow(GameRunState run)
        {
            if (run == null || run.extraRolls == null || run.extraRolls.Count == 0)
                return;

            var root = _showTitle != null ? _showTitle.transform.parent : transform;
            var badges = UiKit.Panel(root, "ThreatBadges", new Color(0, 0, 0, 0));
            UiKit.Layout(badges, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(24, -256), new Vector2(440, 40));
            int shown = 0;
            for (int i = 0; i < run.extraRolls.Count; i++)
            {
                var look = ExtraThreatLook.For(run.extraRolls[i]);
                if (look.Fx == ExtraThreatFx.None)
                    continue;
                if (look.Fx == ExtraThreatFx.Gear)
                    _threatGear = true;
                else if (look.Fx == ExtraThreatFx.Net)
                    _threatNet = true;
                else if (look.Fx == ExtraThreatFx.Rival)
                    _threatRival = true;
                else if (look.Fx == ExtraThreatFx.Scandal)
                    _threatScandal = true;
                else if (look.Fx == ExtraThreatFx.Fee)
                {
                    _threatFee = true;
                    _feeShown += run.extraRolls[i].Amount;
                }
                AddThreatBadge(badges, look, shown);
                shown += 1;
            }

            if (_threatGear)
                BuildGearGlitch();
            if (_threatNet)
                BuildNetFx();
            if (_threatRival)
                BuildRivalSpam();
            if (_threatScandal)
                BuildScandalWash();
            if (_threatFee)
                BuildFeeChip();
        }

        void AddThreatBadge(RectTransform parent, ExtraThreatLook look, int index)
        {
            var box = UiKit.Panel(parent, "ThreatBadge" + index, look.Tint);
            box.anchorMin = new Vector2(0f, 0f);
            box.anchorMax = new Vector2(0f, 1f);
            box.pivot = new Vector2(0f, 0.5f);
            box.anchoredPosition = new Vector2(index * 150f, 0f);
            box.sizeDelta = new Vector2(144f, 0f);
            var icon = UiKit.Image(box, "I", Color.white);
            UiKit.Layout(icon.rectTransform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(18, 0), new Vector2(22, 22));
            ArtSprites.Apply(icon, look.Art, look.Tint, Color.white);
            var lab = UiKit.Label(box, "L", look.Badge, 14, Palette.Ink, TextAnchor.MiddleLeft, FontStyle.Bold);
            UiKit.Layout(lab.rectTransform, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 0.5f), new Vector2(36, 0), new Vector2(-8, 0));
        }

        void BuildGearGlitch()
        {
            if (_avatar == null || _avatar.Root == null)
                return;
            _gearTears = new Image[4];
            for (int i = 0; i < _gearTears.Length; i++)
            {
                var tear = UiKit.Image(_avatar.Root, "GearTear" + i, new Color(1f, 1f, 1f, 0f));
                UiKit.Layout(tear.rectTransform, new Vector2(0, 0.2f + i * 0.18f), new Vector2(1, 0.2f + i * 0.18f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(-20, 7));
                tear.raycastTarget = false;
                _gearTears[i] = tear;
            }
        }

        void BuildNetFx()
        {
            if (_lane == null)
                return;
            _netFx = UiKit.Image(_lane, "NetFx", new Color(0.3f, 0.7f, 1f, 0f));
            UiKit.Stretch(_netFx.rectTransform);
            _netFx.raycastTarget = false;
            _netLabel = UiKit.Label(_lane, "NetLabel", "재연결 중…", 22, Palette.Hex("4EC8FF"), TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(_netLabel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 40), new Vector2(280, 32));
            var c = _netLabel.color;
            c.a = 0f;
            _netLabel.color = c;
        }

        void BuildRivalSpam()
        {
            if (_chatRoot == null)
                return;
            string[] nicks = { "견제계정", "라이벌팬", "안티닉", "견제봇" };
            _rivalSpam = new Text[nicks.Length];
            for (int i = 0; i < nicks.Length; i++)
            {
                var t = UiKit.Label(_chatRoot, "RivalSpam" + i, nicks[i] + "  ·  라이벌 견제", 13, new Color(0.77f, 0.48f, 1f, 0.55f), TextAnchor.MiddleLeft, FontStyle.Bold);
                UiKit.Layout(t.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1), new Vector2(12, -36 - i * 18), new Vector2(-24, 16));
                _rivalSpam[i] = t;
            }
        }

        void BuildScandalWash()
        {
            if (_washVeil == null)
                return;
            var red = new Color(1f, 0.12f, 0.28f, 0.22f);
            _scandalVeil = UiKit.Image(_washVeil.transform.parent, "ScandalWash", red);
            UiKit.Stretch(_scandalVeil.rectTransform);
            _scandalVeil.raycastTarget = false;
            _scandalVeil.transform.SetSiblingIndex(_washVeil.transform.GetSiblingIndex() + 1);
            if (_chatPanel != null)
                _chatPanel.color = Color.Lerp(_chatPanel.color, Palette.Troll, 0.28f);
        }

        void BuildFeeChip()
        {
            if (_incomeNow == null)
                return;
            var host = _incomeNow.transform.parent;
            _feeChip = UiKit.Label(host, "FeeChip", "수수료 " + EconomyRules.FormatWon(_feeShown), 12, Palette.Hex("FFB020"), TextAnchor.UpperRight, FontStyle.Bold);
            UiKit.Layout(_feeChip.rectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(-8, -4), new Vector2(120, 16));
        }

        void TickThreatFx()
        {
            float t = Time.time;
            if (_threatGear && _gearTears != null)
            {
                bool flash = Mathf.Repeat(t, 2.2f) < 0.18f;
                for (int i = 0; i < _gearTears.Length; i++)
                {
                    if (_gearTears[i] == null)
                        continue;
                    float y = 0.18f + i * 0.17f + Mathf.Sin(t * 9f + i) * 0.02f;
                    _gearTears[i].rectTransform.anchorMin = new Vector2(flash ? 0.04f : 0f, y);
                    _gearTears[i].rectTransform.anchorMax = new Vector2(flash ? 0.96f : 1f, y);
                    _gearTears[i].color = flash
                        ? new Color(0.7f + (i % 2) * 0.3f, 0.2f, 0.25f, 0.55f)
                        : new Color(1f, 1f, 1f, 0.04f);
                }
            }

            if (_threatNet && _netFx != null)
            {
                bool drop = Mathf.Repeat(t, 3.1f) < 0.7f;
                _netFx.color = drop ? new Color(0.25f, 0.65f, 1f, 0.22f) : new Color(0.25f, 0.65f, 1f, 0f);
                if (_netLabel != null)
                {
                    var c = _netLabel.color;
                    c.a = drop ? 1f : 0f;
                    _netLabel.color = c;
                }
                if (drop && _lane != null)
                {
                    float slice = Mathf.Repeat(t * 14f, 1f);
                    _netFx.rectTransform.offsetMin = new Vector2(0f, slice * 8f);
                    _netFx.rectTransform.offsetMax = new Vector2(0f, -((1f - slice) * 6f));
                }
            }

            if (_threatRival && _rivalSpam != null)
            {
                for (int i = 0; i < _rivalSpam.Length; i++)
                {
                    if (_rivalSpam[i] == null)
                        continue;
                    float u = Mathf.Repeat(t * 0.18f + i * 0.22f, 1f);
                    _rivalSpam[i].rectTransform.anchoredPosition = new Vector2(12f, -36f - u * 72f);
                    var c = _rivalSpam[i].color;
                    c.a = 0.25f + 0.35f * (1f - u);
                    _rivalSpam[i].color = c;
                }
            }

            if (_threatScandal && _scandalVeil != null)
            {
                float pulse = 0.16f + 0.08f * Mathf.Abs(Mathf.Sin(t * 1.6f));
                _scandalVeil.color = new Color(1f, 0.12f, 0.28f, pulse);
            }
        }

        void PlaySfx(AudioClip clip, float volume)
        {
            _bedDuck = 1f;
            if (_audio != null && clip != null)
                _audio.PlayOneShot(clip, volume);
        }

        static AudioClip ToneClip(string name, float[] freqs, float noteDur, float amp)
        {
            int noteSamples = Mathf.Max(1, Mathf.CeilToInt(44100 * noteDur));
            int samples = noteSamples * freqs.Length;
            var clip = AudioClip.Create(name, samples, 1, 44100, false);
            var data = new float[samples];
            int w = 0;
            for (int n = 0; n < freqs.Length; n++)
            {
                float freq = freqs[n];
                for (int i = 0; i < noteSamples && w < samples; i++, w++)
                {
                    float env = 1f - i / (float)noteSamples;
                    data[w] = Mathf.Sin(2f * Mathf.PI * freq * i / 44100f) * amp * env;
                }
            }
            clip.SetData(data, 0);
            return clip;
        }

        static AudioClip BedClip(StreamContentType type)
        {
            const int rate = 22050;
            float dur = 1.6f;
            int samples = Mathf.CeilToInt(rate * dur);
            var data = new float[samples];
            float[] freqs;
            float amp;
            bool square;
            switch (type)
            {
                case StreamContentType.Talk:
                    freqs = new[] { 220f, 277f, 330f, 277f };
                    amp = 0.045f;
                    square = false;
                    break;
                case StreamContentType.Game:
                    freqs = new[] { 165f, 196f, 247f, 196f };
                    amp = 0.040f;
                    square = true;
                    break;
                case StreamContentType.Song:
                    freqs = new[] { 523f, 659f, 784f, 880f };
                    amp = 0.042f;
                    square = false;
                    break;
                default:
                    freqs = new[] { 110f, 165f, 147f, 110f };
                    amp = 0.032f;
                    square = false;
                    break;
            }

            int step = Mathf.Max(1, samples / freqs.Length);
            for (int i = 0; i < samples; i++)
            {
                float freq = freqs[Mathf.Clamp(i / step, 0, freqs.Length - 1)];
                float t = i / (float)rate;
                float phase = 2f * Mathf.PI * freq * t;
                float wave = square ? Mathf.Sign(Mathf.Sin(phase)) : Mathf.Sin(phase);
                float env = 0.35f + 0.65f * Mathf.Sin(Mathf.PI * i / (float)samples);
                data[i] = wave * amp * env;
            }

            var clip = AudioClip.Create("bgm_" + type, samples, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        static AudioClip BuzzerClip(string name, float dur, float amp)
        {
            int samples = Mathf.CeilToInt(44100 * dur);
            var clip = AudioClip.Create(name, samples, 1, 44100, false);
            var data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = i / 44100f;
                float env = 1f - i / (float)samples;
                float square = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * 140f * t));
                float rasp = Mathf.Sin(2f * Mathf.PI * 90f * t);
                data[i] = (square * 0.55f + rasp * 0.45f) * amp * env;
            }
            clip.SetData(data, 0);
            return clip;
        }
    }
}
