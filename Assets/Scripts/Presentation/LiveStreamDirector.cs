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
        AudioClip _ok;
        AudioClip _bad;
        AudioClip _sc;
        AudioClip _comboCue;

        readonly Dictionary<ChatNote, RectTransform> _views = new Dictionary<ChatNote, RectTransform>();
        float _judgeFlash;
        float _judgePop;
        float _judgePopMax;
        bool _judgeBig;
        float _shownViewers;
        int _lastCombo;
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
            _ok = ToneClip("sfx_perfect", new[] { 880f, 1320f }, 0.07f, 0.22f);
            _bad = BuzzerClip("sfx_miss", 0.12f, 0.20f);
            _sc = ToneClip("sfx_super", new[] { 523f, 659f, 784f, 1046f }, 0.06f, 0.20f);
            _comboCue = ToneClip("sfx_combo", new[] { 698f, 880f, 1174f }, 0.07f, 0.24f);
        }

        void OnDestroy()
        {
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
            _lastCombo = _session.Combo;
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
                    PlaySfx(_bad, 0.48f);
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
            _shownViewers = Mathf.MoveTowards(_shownViewers, _session.Viewers, dt * 80f);
            _avatar.SetViewers(_shownViewers);
            RefreshHud();
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

            UiKit.Image(canvasRoot, "Wash", Palette.Studio);
            UiKit.Stretch(canvasRoot.Find("Wash") as RectTransform);

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

            var top = UiKit.Panel(root, "Top", new Color(0.08f, 0.04f, 0.1f, 0.86f));
            UiKit.Layout(top, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), Vector2.zero, new Vector2(0, 124));

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
            _rival.transform.parent.gameObject.SetActive(false);

            _avatar = new AvatarView(root as RectTransform);

            var chatPanel = UiKit.Panel(root, "Chat", new Color(0.07f, 0.05f, 0.1f, 0.88f));
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
            _viewers.color = Palette.Pastel;
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
            _income.text = EconomyRules.FormatWon(shown);
            _income.color = Palette.CashGreen;
            _mental.text = $"{_session.Mental}/{_session.Balance.maxMental}";
            _mental.color = _session.Mental <= 24 ? Palette.MoneyRed : Palette.Pink;
            _timer.text = $"{Mathf.CeilToInt(_session.TimeLeft)}s";
            if (_session.IncomeFreezeLeft > 0f)
                _combo.text = $"송출 끊김 {_session.IncomeFreezeLeft:0.0}s";
            else if (_session.IncomeShieldLeft > 0f)
                _combo.text = $"수익 보호막 {_session.IncomeShieldLeft:0.0}s";
            else if (_session.HypeActive)
                _combo.text = $"{_session.Tuning.Name}  ·  HYPE {_session.HypeLeft:0.0}s  ·  x{_session.IncomeMultiplier:0.00}";
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
                rt.anchoredPosition = new Vector2(0, y);
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
            var color = super ? Palette.Gold : Palette.ForKind(note.Kind);
            var card = UiKit.Panel(_lane, "Note", Color.white);
            float h = super ? 88f : troll ? 72f : 64f;
            float w = super ? 400f : 372f;
            UiKit.Layout(card, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(w, h));
            var img = card.GetComponent<Image>();
            if (super)
                ArtSprites.ApplySliced(img, ArtSprites.SuperchatBanner, new Color(1f, 0.86f, 0.28f, 1f), new Vector4(36f, 28f, 36f, 28f));
            else if (troll)
            {
                ArtSprites.Apply(img, ArtSprites.TrollBubble, Palette.Troll, Palette.Troll);
                img.preserveAspect = false;
            }
            else
                ArtSprites.ApplySliced(img, ArtSprites.BubblePill, new Color(color.r, color.g, color.b, 0.94f));

            string key = super ? "SPACE" : note.Kind switch
            {
                ChatKind.Positive => "←",
                ChatKind.Empathy => "↓",
                ChatKind.Laugh => "→",
                _ => "↑"
            };
            string kind = super ? "슈퍼챗" : troll ? "트롤" : Palette.LabelFor(note.Kind);
            var keyCol = super || troll ? Palette.Ink : Color.white;
            var keyT = UiKit.Label(card, "Key", key, super ? 16 : 18, keyCol, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(keyT.rectTransform, new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0.5f), new Vector2(18, 0), new Vector2(56, 0));
            string body = super
                ? $"{note.User}  ·  {kind}  {EconomyRules.FormatWon(note.SuperchatWon)}\n{note.Text}"
                : $"{note.User}  ·  {kind}\n{note.Text}";
            var msgCol = troll ? Color.white : Palette.Ink;
            var msg = UiKit.Label(card, "Msg", body, super ? 16 : 17, msgCol, TextAnchor.MiddleLeft, FontStyle.Bold);
            UiKit.Layout(msg.rectTransform, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 0.5f), new Vector2(76, 0), new Vector2(-88, 0));
            msg.horizontalOverflow = HorizontalWrapMode.Wrap;
            if (super)
                card.localScale = Vector3.one * 1.38f;
            return card;
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

        void PlaySfx(AudioClip clip, float volume)
        {
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
