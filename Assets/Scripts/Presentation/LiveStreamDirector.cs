using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BankruptVtuber
{
    public class LiveStreamDirector : MonoBehaviour
    {
        StreamSession _session;
        AvatarView _avatar;
        RivalDuelView _rivalDuel;
        RectTransform _lane;
        RectTransform _hit;
        Image _strike;
        Text _viewers;
        RectTransform _viewerChip;
        Image _viewerChipImg;
        float _viewerChipPop;
        bool _viewerChipUp;
        Text _rival;
        Text _cash;
        Text _debt;
        Text _income;
        Text _mental;
        Text _timer;
        RectTransform _timerChip;
        Image _timerChipImg;
        int _lastClockSec = -1;
        Text _billToday;
        Text _billChip;
        Image _billChipImg;
        Image _billFill;
        Text _incomeNow;
        Text _remain;
        Text _hypeMul;
        Text _bankruptLeft;
        Text _sting;
        Image _raceFill;
        Image _bankruptFill;
        RectTransform _bankruptRow;
        Text _combo;
        Image _comboPlate;
        Text _judge;
        Image _judgeStamp;
        Image _liveDot;
        RectTransform _onAirRoot;
        Image _onAirWash;
        Image _onAirLed;
        Image _onAirPip;
        Text _onAirLive;
        Text _onAirCopy;
        float _onAirLeft;
        RectTransform _endCutRoot;
        Image _endCutWash;
        Image _endCutCard;
        Image _endCutPip;
        Text _endCutCopy;
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
        Text _promoSlam;
        float _promoSlamFlash;
        bool _promoWasActive;
        Text _lineSlam;
        float _lineSlamFlash;
        bool _lineWasActive;
        RectTransform _lineRoot;
        Text _lineTitle;
        Text _lineBody;
        Text _lineTimer;
        bool _lineSettled;
        RectTransform _concertRoot;
        Text _concertTitle;
        Text _concertBody;
        Text _concertTimer;
        Text _concertSlam;
        float _concertSlamFlash;
        bool _concertWasActive;
        Text _coverSlam;
        float _coverSlamFlash;
        bool _billsCovered;
        readonly Image[] _eventKeys = new Image[4];
        readonly Text[] _eventKeyLabels = new Text[4];
        readonly StreamPadButton[] _lanePads = new StreamPadButton[5];
        readonly StreamPadButton[] _eventPads = new StreamPadButton[4];
        StreamPadButton _promoYes;
        StreamPadButton _promoNo;
        StreamPadButton _lineYes;
        StreamPadButton _lineNo;
        StreamPadButton _concertYes;
        StreamPadButton _concertNo;
        Text _echo;
        float _echoFlash;
        Image _tensionFill;
        Image _hypeFlash;
        Image _hypeFrame;
        AudioSource _audio;
        AudioSource _bed;
        AudioClip _ok;
        AudioClip _bad;
        AudioClip _perfect;
        AudioClip _good;
        AudioClip _miss;
        AudioClip _comboBreakSfx;
        AudioClip _sc;
        AudioClip _comboCue;
        AudioClip _hypeCue;
        AudioClip _antiCue;
        AudioClip _lagCue;
        AudioClip _clockTick;
        AudioClip _onAirCue;
        AudioClip _endCutCue;
        AudioClip _billCoverCue;
        AudioClip _padClick;
        AudioClip _mentalCue;
        AudioClip _rivalWinCue;
        AudioClip _rivalLoseCue;
        AudioClip _goodsCue;
        AudioClip _sponsorCue;
        Image _wash;
        Image _washVeil;
        Image _chatPanel;
        Image _hypeChatGlow;
        Text _hypeBanner;
        Text _hypeCount;
        Text _comboSting;
        float _comboStingFlash;
        Text _comboBreak;
        Image _comboBreakStamp;
        float _comboBreakLeft;
        float _comboPop;
        bool _comboPopBig;
        bool _hypeWasOn;
        Image _mentalGrain;
        Text _mentalWarn;
        RectTransform _mentalWarnBox;
        RectTransform _eventWarnBox;
        Text _eventWarn;
        RectTransform _forceEndRoot;
        float _mentalPunch;
        int _hudMental = 100;
        bool _mentalWasTired;
        bool _mentalWasDanger;
        Text _showTitle;
        Text _showChip;
        Image _showChipImg;
        Image _showChipIcon;
        ContentShowLook _look = ContentShowLook.For(StreamContentType.None);
        bool _concertShow;
        float _bedVolume;
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
        Text _coachHint;
        Text _coachPrompt;
        Image _coachPadIcon;
        RectTransform _coachLegend;
        Image _eventSting;
        Text _eventStingLabel;
        readonly Image[] _eventStingBars = new Image[7];
        float _eventStingLeft;
        StreamEventKind _eventStingKind;
        bool _eventScarAnti;
        bool _eventScarGear;
        Image[] _eventCrack;
        Image _eventStatic;
        Image _laneFreeze;

        readonly Dictionary<ChatNote, RectTransform> _views = new Dictionary<ChatNote, RectTransform>();
        readonly HashSet<ChatNote> _heldNotes = new HashSet<ChatNote>();
        readonly List<WonFly> _wonFlies = new List<WonFly>(8);
        readonly List<ScCrack> _scCracks = new List<ScCrack>(4);
        Image _scPipBg;
        Text _scPip;
        RectTransform _fxRoot;
        float _incomePunch;
        float _judgeFlash;
        float _judgePop;
        float _judgePopMax;
        bool _judgeBig;
        float _shownViewers;
        float _shownIncome;
        float _stingFlash;
        float _viewerFlash;
        Text _viewerPop;
        float _viewerPopFlash;
        Text _incomePop;
        float _incomePopFlash;
        int _incomeMarked;
        float _incomeMarkedAt;
        float _hypeViewAcc;
        bool _viewerJudged;
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
            _perfect = Resources.Load<AudioClip>("Audio/sfx_perfect");
            _good = Resources.Load<AudioClip>("Audio/sfx_good");
            _miss = Resources.Load<AudioClip>("Audio/sfx_miss");
            _comboBreakSfx = Resources.Load<AudioClip>("Audio/sfx_combo_break");
            if (_perfect == null)
                _perfect = ToneClip("sfx_perfect", new[] { 880f, 1320f }, 0.07f, 0.22f);
            if (_good == null)
                _good = ToneClip("sfx_good", new[] { 520f, 780f }, 0.08f, 0.14f);
            if (_miss == null)
                _miss = BuzzerClip("sfx_miss", 0.12f, 0.20f);
            if (_comboBreakSfx == null)
                _comboBreakSfx = _miss;
            _ok = _perfect;
            _bad = _miss;
            _sc = Resources.Load<AudioClip>("Audio/sfx_superchat");
            if (_sc == null)
                _sc = ToneClip("sfx_super", new[] { 523f, 659f, 784f, 1046f }, 0.06f, 0.20f);
            _comboCue = ToneClip("sfx_combo", new[] { 698f, 880f, 1174f }, 0.07f, 0.24f);
            _hypeCue = Resources.Load<AudioClip>("Audio/sfx_hype");
            if (_hypeCue == null)
                _hypeCue = ToneClip("sfx_hype", new[] { 523f, 659f, 784f, 1046f }, 0.07f, 0.24f);
            _antiCue = Resources.Load<AudioClip>("Audio/sfx_anti");
            if (_antiCue == null)
                _antiCue = BuzzerClip("sfx_anti", 0.16f, 0.24f);
            _lagCue = Resources.Load<AudioClip>("Audio/sfx_lag");
            if (_lagCue == null)
                _lagCue = BuzzerClip("sfx_lag", 0.14f, 0.22f);
            _clockTick = Resources.Load<AudioClip>("Audio/sfx_clock_tick");
            if (_clockTick == null)
                _clockTick = ToneClip("sfx_clock", new[] { 1320f }, 0.045f, 0.18f);
            _onAirCue = Resources.Load<AudioClip>("Audio/sfx_onair");
            if (_onAirCue == null)
                _onAirCue = ToneClip("sfx_onair", new[] { 392f, 523f, 784f }, 0.06f, 0.22f);
            _endCutCue = Resources.Load<AudioClip>("Audio/sfx_end_cut");
            if (_endCutCue == null)
                _endCutCue = ToneClip("sfx_end_cut", new[] { 330f, 196f }, 0.08f, 0.20f);
            _billCoverCue = Resources.Load<AudioClip>("Audio/sfx_bill_cover");
            if (_billCoverCue == null)
                _billCoverCue = ToneClip("sfx_bill_cover", new[] { 880f, 1174f, 1568f }, 0.07f, 0.22f);
            _padClick = Resources.Load<AudioClip>("Audio/sfx_pad");
            if (_padClick == null)
                _padClick = ToneClip("sfx_pad", new[] { 1800f, 900f }, 0.03f, 0.16f);
            _mentalCue = Resources.Load<AudioClip>("Audio/sfx_mental");
            if (_mentalCue == null)
                _mentalCue = ToneClip("sfx_mental", new[] { 220f, 277f, 165f }, 0.10f, 0.18f);
            _rivalWinCue = Resources.Load<AudioClip>("Audio/sfx_rival_win");
            if (_rivalWinCue == null)
                _rivalWinCue = ToneClip("sfx_rival_win", new[] { 523f, 659f, 784f, 1046f }, 0.07f, 0.22f);
            _rivalLoseCue = Resources.Load<AudioClip>("Audio/sfx_rival_lose");
            if (_rivalLoseCue == null)
                _rivalLoseCue = ToneClip("sfx_rival_lose", new[] { 330f, 247f, 196f }, 0.08f, 0.16f);
            _goodsCue = Resources.Load<AudioClip>("Audio/sfx_goods");
            if (_goodsCue == null)
                _goodsCue = ToneClip("sfx_goods", new[] { 1318f, 1760f }, 0.07f, 0.20f);
            _sponsorCue = Resources.Load<AudioClip>("Audio/sfx_sponsor");
            if (_sponsorCue == null)
                _sponsorCue = ToneClip("sfx_sponsor", new[] { 1109f, 1397f, 1760f }, 0.07f, 0.20f);
            StreamBindings.OnLanePadPress += PlayPadClick;
        }

        void OnDestroy()
        {
            StreamBindings.OnLanePadPress -= PlayPadClick;
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
                _concertShow = true;
            }
            _shownViewers = _session.Viewers;
            _incomeMarked = _session.LiveIncome;
            _incomeMarkedAt = _session.Elapsed;
            _lastViewers = _session.Viewers;
            _lastMental = _session.Mental;
            _hudMental = _session.Mental;
            _lastCombo = _session.Combo;
            _tonightBills = EconomyRules.TonightBills(gm.Run);
            _bankruptAt = EconomyRules.BankruptDebt(gm.Run, gm.Balance, gm.Week2, gm.Week3, gm.Week4, gm.Week5);
            var fandom = gm.Fandom;
            string minjun = gm.Run.minjunPresent && fandom != null ? fandom.minjunName : null;
            string haeun = gm.Run.haeunPresent && fandom != null ? fandom.haeunName : null;
            _session.BindChatSeed(gm.Run.runSeed);
            _session.BindNamedFans(
                minjun,
                gm.Run.minjunPresent && gm.Run.minjunIgnoreSettlements > 0,
                haeun,
                gm.Run.haeunPresent && gm.Run.haeunHurtThisDay,
                fandom != null ? fandom.haeunHurtStreak : 0);
            ApplyContentShow(ContentShowLook.For(gm.Run.contentPicked));
            ApplyThreatShow(gm.Run);
            if (StreamSession.ShouldOfferFirstStreamCoach(gm.Run))
                _session.EnableFirstStreamCoach();
            _onAirLeft = 0.6f;
            TickOnAir();
            PlaySfx(_onAirCue, 0.46f);
            _avatar.SetViewers(_shownViewers);
            if (_rivalDuel != null)
                _rivalDuel.Bind(_session);
        }

        void Update()
        {
            if (_session == null || _ending)
                return;

            if (!_session.EventActive
                && !_session.PromoActive
                && !_session.LineActive
                && !_session.ConcertActive
                && StreamBindings.LaneKeyboardPressDown())
                PlayPadClick();

            float dt = Time.deltaTime;
            int comboWas = _session.Combo;
            _session.Tick(dt);
            if (_session.Combo >= 5 && _lastCombo < 5)
            {
                PlaySfx(_comboCue, 0.52f);
                if (!_session.HypeActive)
                    _comboStingFlash = 1f;
            }
            if (_session.Combo > _lastCombo)
            {
                _comboPop = 0.1f;
                _comboPopBig = _session.Combo >= 5;
            }
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
                    Echo("입력됨 홍보", _promoYes);
                    _session.TryPromo(true);
                }
                else if (StreamBindings.PromoSkipDown())
                {
                    Echo("입력됨 넘김", _promoNo);
                    _session.TryPromo(false);
                }
                StreamBindings.DiscardLaneQueue();
            }
            else if (_session.LineActive)
            {
                if (StreamBindings.PromoConfirmDown())
                {
                    Echo("입력됨 멘트", _lineYes);
                    _session.TryLine(true);
                }
                else if (StreamBindings.PromoSkipDown())
                {
                    Echo("입력됨 넘김", _lineNo);
                    _session.TryLine(false);
                }
                StreamBindings.DiscardLaneQueue();
            }
            else if (_session.ConcertActive)
            {
                if (StreamBindings.PromoConfirmDown())
                {
                    Echo("입력됨 퍼포먼스", _concertYes);
                    _session.TryConcert(true);
                }
                else if (StreamBindings.PromoSkipDown())
                {
                    Echo("입력됨 넘김", _concertNo);
                    _session.TryConcert(false);
                }
                StreamBindings.DiscardLaneQueue();
            }
            else if (StreamBindings.TryConsumeKind(out var kind, out var hold))
            {
                Echo($"입력됨 {Palette.LabelFor(kind)}", KindPressPad(kind));
                _session.TryHit(kind, _session.Elapsed, hold);
            }

            MaybeSettleSponsorLine();

            if (!_eventWasActive && _session.EventActive)
                BeginEventAccident(_session.Event.Kind);
            if (_eventWasActive && !_session.EventActive && _session.Event.Resolved)
            {
                bool okHit = _session.Event.Success;
                _judge.text = okHit
                    ? StreamEventState.RecoverCopy(_session.Event.Kind)
                    : StreamEventState.FailCopy(_session.Event.Kind);
                _judge.color = okHit ? Palette.CashGreen : Palette.MoneyRed;
                if (_judgeStamp != null)
                    _judgeStamp.gameObject.SetActive(false);
                _judgeFlash = 1f;
                _judgeBig = true;
                _judgePopMax = 0.28f;
                _judgePop = _judgePopMax;
                PlaySfx(okHit ? _ok : _bad, 0.5f);
                if (!okHit)
                    ApplyEventScar(_session.Event.Kind);
                ResetEventPads();
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
                if (_session.RivalActive && _rivalDuel != null && (j == Judgement.Perfect || j == Judgement.Miss))
                {
                    var w3 = GameManager.Instance.Week3;
                    _rivalDuel.FlashSteal(j == Judgement.Perfect, w3.rivalPerfectSteal, w3.rivalMissSteal);
                }
                if (j == Judgement.Miss)
                {
                    float dv = _session.Viewers - _lastViewers;
                    int dm = _session.Mental - _lastMental;
                    ShowMissSting(dv, dm);
                    if (comboWas >= 2)
                    {
                        ShowComboBreak();
                        PlaySfx(_comboBreakSfx, 0.48f);
                    }
                    else
                        PlaySfx(_miss, 0.48f);
                    _viewerJudged = true;
                    if (note.IsSuperchat)
                        BeginSuperchatCrack(note);
                }
                else if (note.IsSuperchat)
                {
                    PlaySfx(_sc, 0.55f);
                    BeginSuperchatFly(note);
                }
                else if (j == Judgement.Perfect)
                    PlaySfx(_perfect, 0.42f);
                else
                    PlaySfx(_good, 0.22f);
                if (j != Judgement.Miss)
                {
                    float dv = _session.Viewers - _lastViewers;
                    if (Mathf.Abs(dv) >= 0.049f)
                        ShowViewerDelta(dv);
                    if (!note.IsSuperchat)
                        ShowIncomeDelta(_session.LiveIncome - _incomeMarked);
                    _viewerJudged = true;
                }
                _incomeMarked = _session.LiveIncome;
                _incomeMarkedAt = _session.Elapsed;
            }

            SyncNotes();
            TickStrike();
            TickEventWarn();
            TickSuperchatPip();
            RefreshEventOverlay();
            if (!_promoWasActive && _session.PromoActive)
                PlaySfx(_goodsCue, 0.48f);
            if (_promoWasActive && !_session.PromoActive && _session.Promo.Resolved && _session.Promo.Success)
                FlashPromoSuccess();
            _promoWasActive = _session.PromoActive;
            if (!_lineWasActive && _session.LineActive)
                PlaySfx(_sponsorCue, 0.48f);
            if (_lineWasActive && !_session.LineActive && _session.Line.Resolved)
                FlashLineResult();
            _lineWasActive = _session.LineActive;
            if (_concertWasActive && !_session.ConcertActive && _session.Concert.Resolved && _session.Concert.Success)
                FlashConcertSuccess();
            _concertWasActive = _session.ConcertActive;
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
            if (_rivalDuel != null)
                _rivalDuel.Tick(dt);
            RefreshCoach();
            if (!_viewerJudged)
            {
                float idleDv = _session.Viewers - _lastViewers;
                if (Mathf.Abs(idleDv) >= 0.25f)
                    ShowViewerDelta(idleDv);
                else if (_session.HypeActive && idleDv > 0f)
                {
                    _hypeViewAcc += idleDv;
                    if (_hypeViewAcc >= 1f)
                    {
                        ShowViewerDelta(1f);
                        _hypeViewAcc -= 1f;
                    }
                }
            }
            _viewerJudged = false;
            _lastViewers = _session.Viewers;
            _lastMental = _session.Mental;
            _avatar.Tick(dt);

            _judgeFlash = Mathf.MoveTowards(_judgeFlash, 0f, dt * 2.2f);
            var jc = _judge.color;
            jc.a = _judgeFlash;
            _judge.color = jc;
            if (_judgeStamp != null && _judgeStamp.gameObject.activeSelf)
            {
                var sc = _judgeStamp.color;
                sc.a = _judgeFlash;
                _judgeStamp.color = sc;
            }
            if (_judgePop > 0f)
            {
                _judgePop = Mathf.MoveTowards(_judgePop, 0f, dt);
                float u = _judgePopMax <= 0.001f ? 0f : Mathf.Clamp01(_judgePop / _judgePopMax);
                float s = _judgeBig ? 1f + 0.58f * u : 1f + 0.18f * u;
                var scale = Vector3.one * s;
                if (_judge != null)
                    _judge.rectTransform.localScale = scale;
                if (_judgeStamp != null && _judgeStamp.gameObject.activeSelf)
                    _judgeStamp.rectTransform.localScale = scale;
            }
            else
            {
                if (_judge != null)
                    _judge.rectTransform.localScale = Vector3.one;
                if (_judgeStamp != null)
                    _judgeStamp.rectTransform.localScale = Vector3.one;
            }
            _onAirLeft = Mathf.MoveTowards(_onAirLeft, 0f, dt);
            TickOnAir();
            if (_liveDot != null)
            {
                var pip = _onAirLeft > 0f ? Palette.MoneyRed : Color.white;
                _liveDot.color = new Color(pip.r, pip.g, pip.b, 0.4f + 0.6f * Mathf.Abs(Mathf.Sin(Time.time * 6f)));
            }
            _stingFlash = Mathf.MoveTowards(_stingFlash, 0f, dt * 1.4f);
            _viewerFlash = Mathf.MoveTowards(_viewerFlash, 0f, dt * 1.8f);
            TickViewerChipPop();
            _viewerPopFlash = Mathf.MoveTowards(_viewerPopFlash, 0f, dt * 1.6f);
            _incomePopFlash = Mathf.MoveTowards(_incomePopFlash, 0f, dt * 1.6f);
            if (_viewerPop != null)
            {
                var pc = _viewerPop.color;
                pc.a = _viewerPopFlash;
                _viewerPop.color = pc;
                _viewerPop.rectTransform.anchoredPosition = new Vector2(8f, 6f + 22f * (1f - _viewerPopFlash));
                _viewerPop.rectTransform.localScale = Vector3.one * (1f + 0.16f * _viewerPopFlash);
            }
            if (_incomePop != null)
            {
                var ic = _incomePop.color;
                ic.a = _incomePopFlash;
                _incomePop.color = ic;
                _incomePop.rectTransform.anchoredPosition = new Vector2(8f, 6f + 22f * (1f - _incomePopFlash));
                _incomePop.rectTransform.localScale = Vector3.one * (1f + 0.16f * _incomePopFlash);
            }
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
            _promoSlamFlash = Mathf.MoveTowards(_promoSlamFlash, 0f, dt * 0.7f);
            if (_promoSlam != null)
            {
                var pc = _promoSlam.color;
                pc.a = _promoSlamFlash;
                _promoSlam.color = pc;
                _promoSlam.rectTransform.localScale = Vector3.one * (1f + 0.32f * _promoSlamFlash);
            }
            _lineSlamFlash = Mathf.MoveTowards(_lineSlamFlash, 0f, dt * 0.7f);
            if (_lineSlam != null)
            {
                var lc = _lineSlam.color;
                lc.a = _lineSlamFlash;
                _lineSlam.color = lc;
                _lineSlam.rectTransform.localScale = Vector3.one * (1f + 0.32f * _lineSlamFlash);
            }
            _concertSlamFlash = Mathf.MoveTowards(_concertSlamFlash, 0f, dt * 0.7f);
            if (_concertSlam != null)
            {
                var cc = _concertSlam.color;
                cc.a = _concertSlamFlash;
                _concertSlam.color = cc;
                _concertSlam.rectTransform.localScale = Vector3.one * (1f + 0.32f * _concertSlamFlash);
            }
            _coverSlamFlash = Mathf.MoveTowards(_coverSlamFlash, 0f, dt * 2.5f);
            if (_coverSlam != null)
            {
                var kc = _coverSlam.color;
                kc.a = _coverSlamFlash;
                _coverSlam.color = kc;
                _coverSlam.rectTransform.localScale = Vector3.one * (1f + 0.42f * _coverSlamFlash);
            }
            var sc = _stub.color;
            sc.a = Mathf.MoveTowards(sc.a, 0f, dt * 0.7f);
            _stub.color = sc;

            _comboStingFlash = Mathf.MoveTowards(_comboStingFlash, 0f, dt * 1.7f);
            _comboBreakLeft = Mathf.MoveTowards(_comboBreakLeft, 0f, dt);
            TickComboBreak();
            TickComboPop();
            _incomePunch = Mathf.MoveTowards(_incomePunch, 0f, dt * 2.2f);
            TickSuperchatFx(dt);
            if (_session.Mental < _hudMental)
                _mentalPunch = 1f;
            _hudMental = _session.Mental;
            _mentalPunch = Mathf.MoveTowards(_mentalPunch, 0f, dt * 1.8f);
            RefreshHypeShow();
            RefreshMentalShow();
            _bedDuck = Mathf.MoveTowards(_bedDuck, 0f, dt * 1.8f);
            if (_bed != null)
                _bed.volume = Mathf.Lerp(_bedVolume, _bedVolume * 0.28f, _bedDuck);
            TickThreatFx();
            TickEventAccident(dt);

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
            if (_session.RivalActive && _rivalDuel != null)
            {
                _rivalDuel.ShowResult(gm.Run.lastRivalWon, gm.Week3.rivalWinCash, gm.Week3.rivalLoseMental);
                PlaySfx(gm.Run.lastRivalWon ? _rivalWinCue : _rivalLoseCue, 0.56f);
                yield return new WaitForSeconds(0.85f);
            }
            _judge.text = _session.ForceEnded ? "멘탈 붕괴 — 강제 종료" : "방송 종료";
            _judge.color = Color.white;
            if (_judgeStamp != null)
                _judgeStamp.gameObject.SetActive(false);
            _judgeFlash = 1f;
            if (_session.ForceEnded && _forceEndRoot != null)
            {
                StartCoroutine(FadeStreamBed());
                _forceEndRoot.gameObject.SetActive(true);
                _forceEndRoot.SetAsLastSibling();
                yield return new WaitForSeconds(1.25f);
            }
            else
            {
                ShowEndCut();
                yield return new WaitForSeconds(0.5f);
            }
            Debug.Log("[파산 버튜버] stream payout " + paid);
            gm.GoSettlement();
        }

        void ShowEndCut()
        {
            if (_liveDot != null)
                _liveDot.color = new Color(0.2f, 0.02f, 0.04f, 0.2f);
            StartCoroutine(FadeStreamBed());
            PlaySfx(_endCutCue, 0.50f);
            if (_endCutRoot == null)
                return;
            _endCutRoot.gameObject.SetActive(true);
            _endCutRoot.SetAsLastSibling();
            if (_endCutWash != null)
                _endCutWash.color = new Color(0f, 0f, 0f, 0.96f);
            if (_endCutCard != null)
            {
                ArtSprites.Apply(_endCutCard, ArtSprites.EndCut, new Color(0.20f, 0.04f, 0.08f, 0.98f), Color.white);
                _endCutCard.preserveAspect = false;
                _endCutCard.rectTransform.localScale = Vector3.one * 1.06f;
            }
            if (_endCutPip != null)
                _endCutPip.color = new Color(0.28f, 0.03f, 0.06f, 1f);
            if (_endCutCopy != null)
            {
                _endCutCopy.text = "방송 종료";
                _endCutCopy.color = Palette.MoneyRed;
            }
        }

        System.Collections.IEnumerator FadeStreamBed()
        {
            if (_bed == null || !_bed.isPlaying)
                yield break;
            float start = _bed.volume;
            float t = 0f;
            const float fade = 0.2f;
            while (t < fade)
            {
                t += Time.deltaTime;
                if (_bed != null)
                    _bed.volume = Mathf.Lerp(start, 0f, t / fade);
                yield return null;
            }
            if (_bed != null)
                _bed.Stop();
        }

        void Build()
        {
            var canvas = UiKit.CreateCanvas("LiveCanvas", transform);
            canvas.gameObject.AddComponent<StreamPointerRelay>();
            var canvasRoot = canvas.transform;

            _wash = UiKit.Image(canvasRoot, "Wash", Palette.Studio);
            UiKit.Stretch(_wash.rectTransform);
            var overlay = UiKit.Image(canvasRoot, "StreamOverlay", Color.white);
            UiKit.Stretch(overlay.rectTransform);
            ArtSprites.Apply(overlay, ArtSprites.StreamOverlay, Palette.Studio, Color.white);
            overlay.preserveAspect = false;
            overlay.raycastTarget = false;
            _washVeil = UiKit.Image(canvasRoot, "WashVeil", new Color(0, 0, 0, 0));
            UiKit.Stretch(_washVeil.rectTransform);
            _mentalGrain = UiKit.Image(canvasRoot, "MentalGrain", new Color(1f, 1f, 1f, 0f));
            UiKit.Stretch(_mentalGrain.rectTransform);
            _mentalGrain.sprite = GrainSprite();
            _mentalGrain.type = Image.Type.Tiled;
            _mentalGrain.raycastTarget = false;

            var safe = StreamSafeArea.Attach(canvasRoot);
            var root = safe;
            _fxRoot = root as RectTransform;

            _hypeFlash = UiKit.Image(root, "HypeFlash", new Color(1f, 0.82f, 0.25f, 0f));
            UiKit.Stretch(_hypeFlash.rectTransform);
            _hypeFlash.raycastTarget = false;
            _hypeFrame = UiKit.Image(root, "HypeFrame", Color.white);
            UiKit.Stretch(_hypeFrame.rectTransform);
            ArtSprites.Apply(_hypeFrame, ArtSprites.HypeFrame, Palette.Gold, Color.white);
            _hypeFrame.preserveAspect = false;
            _hypeFrame.raycastTarget = false;
            _hypeFrame.gameObject.SetActive(false);
            _hypeBanner = UiKit.Label(root, "HypeBanner", "", 62, Palette.Gold, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(_hypeBanner.rectTransform, new Vector2(0.08f, 0.58f), new Vector2(0.52f, 0.58f), new Vector2(0.5f, 0.5f), new Vector2(0, 36), new Vector2(0, 72));
            _hypeCount = UiKit.Label(root, "HypeCount", "", 34, Palette.Gold, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(_hypeCount.rectTransform, new Vector2(0.08f, 0.58f), new Vector2(0.52f, 0.58f), new Vector2(0.5f, 0.5f), new Vector2(0, -18), new Vector2(0, 40));
            _comboSting = UiKit.Label(root, "ComboSting", "", 28, Palette.Gold, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(_comboSting.rectTransform, new Vector2(0.12f, 0.54f), new Vector2(0.48f, 0.54f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(0, 36));
            _comboBreakStamp = UiKit.Image(root, "ComboBreakStamp", Color.white);
            UiKit.Layout(_comboBreakStamp.rectTransform, new Vector2(0.34f, 0.16f), new Vector2(0.34f, 0.16f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(420f, 84f));
            ArtSprites.Apply(_comboBreakStamp, ArtSprites.ComboBreak, Palette.MoneyRed, Color.white);
            _comboBreakStamp.preserveAspect = false;
            _comboBreakStamp.raycastTarget = false;
            _comboBreakStamp.gameObject.SetActive(false);
            _comboBreak = UiKit.Label(root, "ComboBreak", "", 40, Palette.MoneyRed, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(_comboBreak.rectTransform, new Vector2(0.10f, 0.16f), new Vector2(0.58f, 0.16f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(0, 44));
            var warn = UiKit.Panel(root, "MentalWarnBox", new Color(1f, 0.95f, 0.72f, 0.98f));
            _mentalWarnBox = warn;
            UiKit.Layout(warn, new Vector2(0.74f, 1), new Vector2(1f, 1), new Vector2(1, 1), new Vector2(-16, -214), new Vector2(236, 48));
            var warnImg = warn.GetComponent<Image>();
            if (warnImg != null)
            {
                ArtSprites.Apply(warnImg, ArtSprites.MentalNote, new Color(1f, 0.95f, 0.72f, 0.98f), Color.white);
                warnImg.preserveAspect = false;
                warnImg.raycastTarget = false;
            }
            _mentalWarn = UiKit.Label(warn, "MentalWarn", "멘탈 위험", 18, Palette.MoneyRed, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Stretch(_mentalWarn.rectTransform, 10f, 10f, 6f, 6f);
            warn.gameObject.SetActive(false);
            _eventWarnBox = UiKit.Panel(root, "EventWarnBox", new Color(0.58f, 0.08f, 0.16f, 0.94f));
            UiKit.Layout(_eventWarnBox, new Vector2(0.18f, 0.62f), new Vector2(0.52f, 0.62f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(0, 72));
            var eventWarnImg = _eventWarnBox.GetComponent<Image>();
            if (eventWarnImg != null)
            {
                ArtSprites.Apply(eventWarnImg, ArtSprites.EventWarn, new Color(0.58f, 0.08f, 0.16f, 0.94f), Color.white);
                eventWarnImg.preserveAspect = false;
                eventWarnImg.raycastTarget = false;
            }
            _eventWarn = UiKit.Label(_eventWarnBox, "EventWarn", "안티 온다", 28, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Stretch(_eventWarn.rectTransform, 96f, 48f, 18f, 12f);
            _eventWarnBox.gameObject.SetActive(false);

            var top = UiKit.Panel(root, "Top", new Color(0.08f, 0.04f, 0.1f, 0.36f));
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
            _viewerChip = _viewers.transform.parent as RectTransform;
            _viewerChipImg = _viewerChip != null ? _viewerChip.GetComponent<Image>() : null;
            if (_viewerChipImg != null)
            {
                ArtSprites.Apply(_viewerChipImg, ArtSprites.ViewerBadge, new Color(0.16f, 0.22f, 0.38f, 0.96f), Color.white);
                _viewerChipImg.preserveAspect = false;
                _viewerChipImg.raycastTarget = false;
            }
            if (_viewerChip != null)
            {
                var viewerCap = _viewerChip.Find("L");
                if (viewerCap != null)
                {
                    var viewerCapT = viewerCap.GetComponent<Text>();
                    if (viewerCapT != null)
                        viewerCapT.color = Palette.PastelDim;
                }
            }
            _viewerPop = UiKit.Label(_viewers.transform.parent, "ViewerPop", "", 20, Palette.CashGreen, TextAnchor.MiddleLeft, FontStyle.Bold);
            UiKit.Layout(_viewerPop.rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0f, 0.5f), new Vector2(8f, 6f), new Vector2(150f, 28f));
            _rival = Chip(top, "Rival", "라이벌", 0.40f, 0.64f, -6f);
            _timer = Chip(top, "Timer", "남은 시간", 0.64f, 1f, -6f);
            _timerChip = _timer.transform.parent as RectTransform;
            _timerChipImg = _timerChip != null ? _timerChip.GetComponent<Image>() : null;
            if (_timerChipImg != null)
            {
                ArtSprites.Apply(_timerChipImg, ArtSprites.ClockPlate, new Color(0.22f, 0.12f, 0.20f, 0.96f), Color.white);
                _timerChipImg.preserveAspect = false;
                _timerChipImg.raycastTarget = false;
            }
            if (_timerChip != null)
            {
                var timerCap = _timerChip.Find("L");
                if (timerCap != null)
                {
                    var timerCapT = timerCap.GetComponent<Text>();
                    if (timerCapT != null)
                        timerCapT.color = Palette.PastelDim;
                }
            }
            _cash = Chip(top, "Cash", "현금", 0f, 0.25f, -64f);
            _debt = Chip(top, "Debt", "부채", 0.25f, 0.50f, -64f);
            _income = Chip(top, "Income", "실시간 수익", 0.50f, 0.75f, -64f);
            _mental = Chip(top, "Mental", "멘탈", 0.75f, 1f, -64f);
            _billToday = Chip(top, "TonightBills", "오늘 청구", 0f, 0.25f, -124f);
            _incomeNow = Chip(top, "TonightIncome", "지금 수입", 0.25f, 0.50f, -124f);
            var incomeTile = _incomeNow.transform.parent as RectTransform;
            if (incomeTile != null)
            {
                var incomeImg = incomeTile.GetComponent<Image>();
                if (incomeImg != null)
                {
                    ArtSprites.Apply(incomeImg, ArtSprites.CashSlip, new Color(0.98f, 0.94f, 0.86f, 0.98f), Color.white);
                    incomeImg.preserveAspect = false;
                    incomeImg.raycastTarget = false;
                }
                var incomeCap = incomeTile.Find("L") as RectTransform;
                if (incomeCap != null)
                {
                    var incomeCapT = incomeCap.GetComponent<Text>();
                    if (incomeCapT != null)
                        incomeCapT.color = Palette.Ink;
                }
            }
            _incomePop = UiKit.Label(_incomeNow.transform.parent, "IncomePop", "", 20, Palette.CashGreen, TextAnchor.MiddleLeft, FontStyle.Bold);
            UiKit.Layout(_incomePop.rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0f, 0.5f), new Vector2(8f, 6f), new Vector2(160f, 28f));
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
            UiKit.Layout(_showTitle.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(164, -212), new Vector2(280, 44));
            var showChip = UiKit.Panel(root, "ShowChip", Palette.Pink);
            UiKit.Layout(showChip, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(24, -214), new Vector2(148, 36));
            _showChipImg = showChip.GetComponent<Image>();
            if (_showChipImg != null)
                _showChipImg.raycastTarget = false;
            _showChipIcon = UiKit.Image(showChip, "Icon", Color.white);
            UiKit.Layout(_showChipIcon.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(18f, 0f), new Vector2(26f, 26f));
            _showChipIcon.raycastTarget = false;
            _showChip = UiKit.Label(showChip, "T", "", 20, Palette.Ink, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(_showChip.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), new Vector2(28f, 0f), new Vector2(-8f, 0f));
            var billChip = UiKit.Panel(root, "BillChip", new Color(0.55f, 0.08f, 0.16f, 0.94f));
            UiKit.Layout(billChip, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(460, -214), new Vector2(240, 40));
            _billChipImg = billChip.GetComponent<Image>();
            if (_billChipImg != null)
            {
                ArtSprites.ApplySliced(_billChipImg, ArtSprites.BillNotice, Color.white, new Vector4(28f, 16f, 28f, 16f));
                _billChipImg.raycastTarget = false;
            }
            _billChip = UiKit.Label(billChip, "T", "청구 ₩0", 22, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Stretch(_billChip.rectTransform);
            var billTrack = UiKit.Image(root, "BillFillTrack", new Color(1f, 1f, 1f, 0.14f));
            UiKit.Layout(billTrack.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(710, -224), new Vector2(180, 10));
            billTrack.raycastTarget = false;
            _billFill = UiKit.Image(billTrack.rectTransform, "BillFill", Palette.MoneyRed);
            UiKit.Stretch(_billFill.rectTransform);
            _billFill.rectTransform.anchorMax = new Vector2(0f, 1f);
            _billFill.raycastTarget = false;

            _avatar = new AvatarView(root as RectTransform);
            _rivalDuel = new RivalDuelView(root as RectTransform);

            var chatPanel = UiKit.Panel(root, "Chat", new Color(0.07f, 0.05f, 0.1f, 0.40f));
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
            var hitRail = UiKit.Image(_lane, "HitRail", Color.white);
            UiKit.Stretch(hitRail.rectTransform);
            ArtSprites.Apply(hitRail, ArtSprites.HitRail, Color.white, new Color(1f, 1f, 1f, 0.94f));
            hitRail.preserveAspect = false;
            hitRail.raycastTarget = false;
            hitRail.transform.SetAsFirstSibling();
            _hypeChatGlow = UiKit.Image(_lane, "HypeChatGlow", new Color(1f, 0.86f, 0.28f, 0f));
            UiKit.Stretch(_hypeChatGlow.rectTransform);
            _hypeChatGlow.raycastTarget = false;
            _hypeChatGlow.transform.SetSiblingIndex(1);

            _hit = UiKit.Panel(_lane, "Hit", new Color(1f, 1f, 1f, 0.22f));
            UiKit.Layout(_hit, new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, LaneHit), new Vector2(0, 10));
            _strike = UiKit.Image(_lane, "Strike", new Color(1f, 0.95f, 0.72f, 0.96f));
            UiKit.Layout(_strike.rectTransform, new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, LaneHit), new Vector2(0, 4));
            _strike.raycastTarget = false;

            var hitLabel = UiKit.Label(_lane, "HitL", "타이밍", 16, Palette.Pastel, TextAnchor.MiddleRight);
            UiKit.Layout(hitLabel.rectTransform, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-4, LaneHit + 18), new Vector2(80, 20));

            var bottom = UiKit.Panel(root, "Bottom", new Color(0.08f, 0.04f, 0.1f, 0.36f));
            UiKit.Layout(bottom, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), Vector2.zero, new Vector2(0, 200));

            _comboPlate = UiKit.Image(bottom, "ComboPlate", Color.white);
            UiKit.Layout(_comboPlate.rectTransform, new Vector2(0, 0.70f), new Vector2(0.55f, 1), new Vector2(0, 1), new Vector2(12, -4), new Vector2(0, 40));
            ArtSprites.Apply(_comboPlate, ArtSprites.ComboPlate, new Color(0.22f, 0.12f, 0.28f, 0.96f), Color.white);
            _comboPlate.preserveAspect = false;
            _comboPlate.raycastTarget = false;
            _combo = UiKit.Label(_comboPlate.transform, "Combo", "COMBO 0", 22, Palette.Pastel, TextAnchor.MiddleLeft, FontStyle.Bold);
            UiKit.Stretch(_combo.rectTransform, 18f, 18f, 6f, 6f);

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
            BuildSuperchatPip(_lanePads[4]);

            _sting = UiKit.Label(root, "MissSting", "", 40, Palette.MoneyRed, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(_sting.rectTransform, new Vector2(0.22f, 0.48f), new Vector2(0.22f, 0.48f), new Vector2(0.5f, 0.5f), new Vector2(0, 0), new Vector2(420, 80));
            _sting.color = new Color(1f, 0.18f, 0.32f, 0f);

            _coachHint = UiKit.Label(root, "CoachHint", "색에 맞는 키 또는 아래 버튼을 눌러.", 22, Palette.Pastel, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(_coachHint.rectTransform, new Vector2(0.5f, 0.30f), new Vector2(0.5f, 0.30f), new Vector2(0.5f, 0.5f), new Vector2(-80, 0), new Vector2(640, 36));
            _coachHint.gameObject.SetActive(false);
            _coachLegend = BuildCoachLegend(root);
            _coachLegend.gameObject.SetActive(false);
            _coachPadIcon = UiKit.Image(root, "CoachPadIcon", Color.white);
            UiKit.Layout(_coachPadIcon.rectTransform, new Vector2(0.5f, 0.40f), new Vector2(0.5f, 0.40f), new Vector2(0.5f, 0.5f), new Vector2(-280, 0), new Vector2(84, 84));
            _coachPadIcon.raycastTarget = false;
            _coachPadIcon.gameObject.SetActive(false);
            _coachPrompt = UiKit.Label(root, "CoachPrompt", "", 56, Palette.Gold, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(_coachPrompt.rectTransform, new Vector2(0.5f, 0.40f), new Vector2(0.5f, 0.40f), new Vector2(0.5f, 0.5f), new Vector2(-20, 0), new Vector2(560, 72));
            _coachPrompt.gameObject.SetActive(false);

            _judgeStamp = UiKit.Image(root, "JudgeStamp", Color.white);
            UiKit.Layout(_judgeStamp.rectTransform, new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.5f), new Vector2(-80, 0), new Vector2(520, 96));
            _judgeStamp.preserveAspect = false;
            _judgeStamp.raycastTarget = false;
            _judgeStamp.gameObject.SetActive(false);
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
            UiKit.Layout(_eventRoot, new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(560, 280));
            SafeFitCard.Bind(_eventRoot, 560f, 280f);
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

            _laneFreeze = UiKit.Image(_lane, "LaneFreeze", new Color(0.08f, 0.04f, 0.1f, 0f));
            UiKit.Stretch(_laneFreeze.rectTransform);
            _laneFreeze.raycastTarget = false;

            _eventSting = UiKit.Image(root, "EventSting", new Color(1f, 0.08f, 0.18f, 0f));
            UiKit.Stretch(_eventSting.rectTransform);
            _eventSting.raycastTarget = false;
            for (int i = 0; i < _eventStingBars.Length; i++)
            {
                var bar = UiKit.Image(_eventSting.rectTransform, "StingBar" + i, new Color(1f, 1f, 1f, 0f));
                UiKit.Layout(bar.rectTransform, new Vector2(0, i / 7f), new Vector2(1, i / 7f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(0, 18));
                bar.raycastTarget = false;
                _eventStingBars[i] = bar;
            }
            _eventStingLabel = UiKit.Label(_eventSting.rectTransform, "StingName", "", 72, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(_eventStingLabel.rectTransform, new Vector2(0, 0.4f), new Vector2(1, 0.6f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            _eventSting.gameObject.SetActive(false);

            _promoRoot = UiKit.Panel(root, "PromoCard", new Color(0.18f, 0.08f, 0.16f, 0.97f));
            UiKit.Layout(_promoRoot, new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(720, 380));
            SafeFitCard.Bind(_promoRoot, 720f, 380f);
            _promoTitle = UiKit.Label(_promoRoot, "PTitle", "굿즈 홍보 타이밍", 40, Palette.Gold, TextAnchor.UpperCenter, FontStyle.Bold);
            UiKit.Layout(_promoTitle.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -18), new Vector2(-24, 52));
            var promoStand = UiKit.Image(_promoRoot, "PromoStand", Color.white);
            UiKit.Layout(promoStand.rectTransform, new Vector2(0.5f, 0.68f), new Vector2(0.5f, 0.68f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(120, 120));
            ArtSprites.Apply(promoStand, ArtSprites.GoodsStand, Color.white, Color.white);
            promoStand.preserveAspect = true;
            promoStand.raycastTarget = false;
            _promoBody = UiKit.Label(_promoRoot, "PBody", "지금 아크릴 홍보?\n성공 시 오늘 판매 1.5배", 28, Palette.Pastel, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(_promoBody.rectTransform, new Vector2(0, 0.30f), new Vector2(1, 0.58f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(-36, 0));
            _promoTimer = UiKit.Label(_promoRoot, "PTimer", "", 20, Palette.MoneyRed, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(_promoTimer.rectTransform, new Vector2(0, 0.22f), new Vector2(1, 0.30f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            AddOverlayChoice(_promoRoot, "홍보하기", "넘어가기", out _promoYes, out _promoNo);
            _promoRoot.gameObject.SetActive(false);
            _promoSlam = UiKit.Label(root, "PromoSlam", "홍보 성공 1.5x", 56, Palette.Gold, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(_promoSlam.rectTransform, new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.5f), new Vector2(-80, 0), new Vector2(720, 80));
            var promoSlamC = _promoSlam.color;
            promoSlamC.a = 0f;
            _promoSlam.color = promoSlamC;

            _lineRoot = UiKit.Panel(root, "LineCard", Color.white);
            UiKit.Layout(_lineRoot, new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(720, 380));
            var lineImg = _lineRoot.GetComponent<Image>();
            ArtSprites.Apply(lineImg, ArtSprites.SponsorCard, new Color(0.18f, 0.08f, 0.16f, 0.97f), Color.white);
            lineImg.preserveAspect = false;
            SafeFitCard.Bind(_lineRoot, 720f, 380f);
            _lineTitle = UiKit.Label(_lineRoot, "LTitle", "스폰서 멘트 타이밍", 40, Palette.Gold, TextAnchor.UpperCenter, FontStyle.Bold);
            UiKit.Layout(_lineTitle.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -18), new Vector2(-24, 52));
            _lineBody = UiKit.Label(_lineRoot, "LBody", "스폰서 멘트\n계약 유지 +₩3,000\n실패 시 계약 파기", 26, Palette.Pastel, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(_lineBody.rectTransform, new Vector2(0, 0.30f), new Vector2(1, 0.78f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(-36, 0));
            _lineTimer = UiKit.Label(_lineRoot, "LTimer", "", 20, Palette.MoneyRed, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(_lineTimer.rectTransform, new Vector2(0, 0.22f), new Vector2(1, 0.30f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            AddOverlayChoice(_lineRoot, "멘트 넣기", "놓치기", out _lineYes, out _lineNo);
            _lineRoot.gameObject.SetActive(false);
            _lineSlam = UiKit.Label(root, "LineSlam", "계약 유지 +₩3,000", 48, Palette.Gold, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(_lineSlam.rectTransform, new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.5f), new Vector2(-80, 0), new Vector2(820, 80));
            var lineSlamC = _lineSlam.color;
            lineSlamC.a = 0f;
            _lineSlam.color = lineSlamC;

            _concertRoot = UiKit.Panel(root, "ConcertCard", Color.white);
            UiKit.Layout(_concertRoot, new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(720, 380));
            var concertLiveImg = _concertRoot.GetComponent<Image>();
            ArtSprites.Apply(concertLiveImg, ArtSprites.ConcertStage, new Color(0.18f, 0.07f, 0.16f, 0.97f), Color.white);
            concertLiveImg.preserveAspect = false;
            SafeFitCard.Bind(_concertRoot, 720f, 380f);
            _concertTitle = UiKit.Label(_concertRoot, "CTitle", "콘서트 퍼포먼스 타이밍", 40, Palette.Gold, TextAnchor.UpperCenter, FontStyle.Bold);
            UiKit.Layout(_concertTitle.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -18), new Vector2(-24, 52));
            _concertBody = UiKit.Label(_concertRoot, "CBody", "퍼포먼스 지금?\n성공 시 정산 ×1.3", 28, Palette.Pastel, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(_concertBody.rectTransform, new Vector2(0, 0.30f), new Vector2(1, 0.78f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(-36, 0));
            _concertTimer = UiKit.Label(_concertRoot, "CTimer", "", 20, Palette.MoneyRed, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(_concertTimer.rectTransform, new Vector2(0, 0.22f), new Vector2(1, 0.30f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            AddOverlayChoice(_concertRoot, "성공", "넘기기", out _concertYes, out _concertNo);
            _concertRoot.gameObject.SetActive(false);
            _coverSlam = UiKit.Label(root, "CoverSlam", "청구 커버", 72, Palette.Gold, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(_coverSlam.rectTransform, new Vector2(0.5f, 0.56f), new Vector2(0.5f, 0.56f), new Vector2(0.5f, 0.5f), new Vector2(-80, 0), new Vector2(720, 90));
            var coverC = _coverSlam.color;
            coverC.a = 0f;
            _coverSlam.color = coverC;
            _concertSlam = UiKit.Label(root, "ConcertSlam", "정산 ×1.3", 56, Palette.Gold, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(_concertSlam.rectTransform, new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.5f), new Vector2(-80, 0), new Vector2(720, 80));
            var concertSlamC = _concertSlam.color;
            concertSlamC.a = 0f;
            _concertSlam.color = concertSlamC;

            _onAirRoot = UiKit.Panel(canvasRoot, "OnAir", new Color(0.08f, 0.02f, 0.05f, 0.72f));
            UiKit.Stretch(_onAirRoot);
            _onAirWash = _onAirRoot.GetComponent<Image>();
            if (_onAirWash != null)
                _onAirWash.raycastTarget = false;
            _onAirLed = UiKit.Image(_onAirRoot, "OnAirLed", Color.white);
            UiKit.Layout(_onAirLed.rectTransform, new Vector2(0.5f, 0.56f), new Vector2(0.5f, 0.56f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(560f, 120f));
            ArtSprites.Apply(_onAirLed, ArtSprites.OnAirLed, new Color(0.35f, 0.04f, 0.08f, 0.98f), Color.white);
            _onAirLed.preserveAspect = false;
            _onAirLed.raycastTarget = false;
            _onAirPip = UiKit.Image(_onAirLed.transform, "Pip", Palette.MoneyRed);
            UiKit.Layout(_onAirPip.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(36f, 0f), new Vector2(22f, 22f));
            _onAirPip.raycastTarget = false;
            _onAirLive = UiKit.Label(_onAirLed.transform, "Live", "ON AIR", 64, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Stretch(_onAirLive.rectTransform, 56f, 28f, 18f, 18f);
            _onAirCopy = UiKit.Label(_onAirRoot, "Copy", "방송 시작", 36, Palette.MoneyRed, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(_onAirCopy.rectTransform, new Vector2(0.5f, 0.46f), new Vector2(0.5f, 0.46f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(420f, 44f));

            _endCutRoot = UiKit.Panel(canvasRoot, "EndCut", new Color(0f, 0f, 0f, 0.96f));
            UiKit.Stretch(_endCutRoot);
            _endCutWash = _endCutRoot.GetComponent<Image>();
            if (_endCutWash != null)
                _endCutWash.raycastTarget = false;
            _endCutCard = UiKit.Image(_endCutRoot, "EndCutCard", Color.white);
            UiKit.Layout(_endCutCard.rectTransform, new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(560f, 140f));
            ArtSprites.Apply(_endCutCard, ArtSprites.EndCut, new Color(0.20f, 0.04f, 0.08f, 0.98f), Color.white);
            _endCutCard.preserveAspect = false;
            _endCutCard.raycastTarget = false;
            _endCutPip = UiKit.Image(_endCutCard.transform, "Pip", new Color(0.28f, 0.03f, 0.06f, 1f));
            UiKit.Layout(_endCutPip.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(40f, 0f), new Vector2(20f, 20f));
            _endCutPip.raycastTarget = false;
            var endLive = UiKit.Label(_endCutCard.transform, "Live", "LIVE", 28, new Color(0.45f, 0.12f, 0.16f, 1f), TextAnchor.MiddleLeft, FontStyle.Bold);
            UiKit.Layout(endLive.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(58f, 0f), new Vector2(90f, 36f));
            _endCutCopy = UiKit.Label(_endCutCard.transform, "Copy", "방송 종료", 52, Palette.MoneyRed, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Stretch(_endCutCopy.rectTransform, 48f, 36f, 18f, 18f);
            _endCutRoot.gameObject.SetActive(false);

            _forceEndRoot = UiKit.Panel(canvasRoot, "ForceEnd", new Color(0.10f, 0.02f, 0.05f, 0.88f));
            UiKit.Stretch(_forceEndRoot);
            _forceEndRoot.gameObject.SetActive(false);
            var fe = UiKit.Label(_forceEndRoot, "T", "강제 종료", 72, Palette.MoneyRed, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(fe.rectTransform, new Vector2(0.5f, 0.54f), new Vector2(0.5f, 0.54f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(720, 90));
            var fs = UiKit.Label(_forceEndRoot, "S", "멘탈 붕괴", 28, Palette.Pastel, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(fs.rectTransform, new Vector2(0.5f, 0.46f), new Vector2(0.5f, 0.46f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(520, 40));
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
            string keycap = KeycapFor(mode, kind);
            if (keycap != null)
                ArtSprites.ApplySliced(img, keycap, new Color(0.86f, 0.86f, 0.86f, 1f), new Vector4(36f, 36f, 36f, 36f));
            var cap = UiKit.Label(img.transform, "L", label, count >= 5 ? 22 : 28, Palette.Ink, TextAnchor.MiddleCenter, FontStyle.Bold);
            if (keycap != null)
                UiKit.Layout(cap.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0.48f), new Vector2(0.5f, 0f), Vector2.zero, Vector2.zero);
            else
                UiKit.Stretch(cap.rectTransform);
            return StreamPadButton.Attach(img.gameObject, mode, kind, eventIndex);
        }

        static string KeycapFor(StreamPadButton.Mode mode, ChatKind kind)
        {
            if (mode == StreamPadButton.Mode.Superchat)
                return ArtSprites.PadSuperchat;
            if (mode != StreamPadButton.Mode.Kind)
                return null;
            switch (kind)
            {
                case ChatKind.Empathy:
                    return ArtSprites.PadDown;
                case ChatKind.Laugh:
                    return ArtSprites.PadRight;
                case ChatKind.Thanks:
                    return ArtSprites.PadUp;
                default:
                    return ArtSprites.PadLeft;
            }
        }

        RectTransform BuildCoachLegend(Transform root)
        {
            var row = UiKit.Panel(root, "CoachLegend", new Color(0f, 0f, 0f, 0f));
            UiKit.Layout(row, new Vector2(0.5f, 0.345f), new Vector2(0.5f, 0.345f), new Vector2(0.5f, 0.5f), new Vector2(-80, 0), new Vector2(700, 64));
            var rowImg = row.GetComponent<Image>();
            if (rowImg != null)
                rowImg.raycastTarget = false;
            var tips = new (string art, string bind)[]
            {
                (ArtSprites.PadLeft, "←"),
                (ArtSprites.PadDown, "↓"),
                (ArtSprites.PadRight, "→"),
                (ArtSprites.PadUp, "↑"),
                (ArtSprites.PadSuperchat, "Space"),
            };
            for (int i = 0; i < tips.Length; i++)
            {
                float a = i / (float)tips.Length;
                float b = (i + 1) / (float)tips.Length;
                var cell = UiKit.Panel(row, "Tip" + i, new Color(0f, 0f, 0f, 0f));
                cell.anchorMin = new Vector2(a, 0f);
                cell.anchorMax = new Vector2(b, 1f);
                cell.offsetMin = new Vector2(4f, 0f);
                cell.offsetMax = new Vector2(-4f, 0f);
                var cellImg = cell.GetComponent<Image>();
                if (cellImg != null)
                    cellImg.raycastTarget = false;
                var icon = UiKit.Image(cell, "Icon", Color.white);
                UiKit.Layout(icon.rectTransform, new Vector2(0.5f, 0.62f), new Vector2(0.5f, 0.62f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(44, 44));
                ArtSprites.Apply(icon, tips[i].art, Color.white, Color.white);
                icon.preserveAspect = true;
                icon.raycastTarget = false;
                var bind = UiKit.Label(cell, "Bind", tips[i].bind, 18, Palette.Pastel, TextAnchor.MiddleCenter, FontStyle.Bold);
                UiKit.Layout(bind.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0.38f), new Vector2(0.5f, 0f), Vector2.zero, Vector2.zero);
            }
            return row;
        }

        void BuildSuperchatPip(StreamPadButton pad)
        {
            if (pad == null)
                return;
            var bg = UiKit.Image(pad.transform, "ScPip", Palette.Gold);
            UiKit.Layout(bg.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 0f), new Vector2(0f, 6f), new Vector2(148f, 40f));
            ArtSprites.Apply(bg, ArtSprites.SuperchatPip, Palette.Gold, Color.white);
            bg.preserveAspect = false;
            bg.raycastTarget = false;
            _scPipBg = bg;
            _scPip = UiKit.Label(bg.transform, "T", "슈퍼챗", 18, Palette.Ink, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Stretch(_scPip.rectTransform, 52f, 10f, 6f, 6f);
            bg.gameObject.SetActive(false);
        }

        void TickSuperchatPip()
        {
            bool warn = false;
            if (_session != null)
            {
                for (int i = 0; i < _session.Notes.Count; i++)
                {
                    var n = _session.Notes[i];
                    if (!n.IsSuperchat || n.Consumed)
                        continue;
                    float eta = n.HitTime - _session.Elapsed;
                    if (eta <= 0.4f && eta >= -0.15f)
                    {
                        warn = true;
                        break;
                    }
                }
            }
            if (_scPipBg == null)
                return;
            _scPipBg.gameObject.SetActive(warn);
            if (!warn)
                return;
            ArtSprites.Apply(_scPipBg, ArtSprites.SuperchatPip, Palette.Gold, Color.white);
            _scPipBg.preserveAspect = false;
            float pulse = 1f + 0.14f * Mathf.Abs(Mathf.Sin(Time.time * 14f));
            _scPipBg.rectTransform.localScale = Vector3.one * pulse;
            var c = Palette.Gold;
            c.a = 0.75f + 0.25f * Mathf.Abs(Mathf.Sin(Time.time * 14f));
            _scPipBg.color = c;
            if (_scPip != null)
                _scPip.color = Palette.Ink;
        }

        void AddOverlayChoice(Transform parent, string confirm, string skip, out StreamPadButton yes, out StreamPadButton no)
        {
            var row = UiKit.Panel(parent, "ChoiceRow", new Color(0, 0, 0, 0));
            UiKit.Layout(row, new Vector2(0, 0), new Vector2(1, 0.24f), new Vector2(0.5f, 0), Vector2.zero, Vector2.zero);
            var rowImg = row.GetComponent<Image>();
            if (rowImg != null)
                rowImg.raycastTarget = false;
            yes = AddColumnPad(row, 0, 2, confirm, Palette.PinkDeep, StreamPadButton.Mode.PromoConfirm);
            no = AddColumnPad(row, 1, 2, skip, Palette.Troll, StreamPadButton.Mode.PromoSkip);
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

        StreamPadButton KindPressPad(ChatKind kind)
        {
            if (UnityEngine.Input.GetKeyUp(KeyCode.Space)
                || UnityEngine.Input.GetKeyUp(KeyCode.Return)
                || UnityEngine.Input.GetKeyUp(KeyCode.KeypadEnter))
                return _lanePads[4];
            return LanePad(kind);
        }

        StreamPadButton CoachPad(ChatNote note)
        {
            if (note == null)
                return null;
            if (note.IsSuperchat)
                return _lanePads[4];
            return LanePad(note.Kind);
        }

        static string CoachPrompt(ChatNote note, int presented)
        {
            if (note == null)
                return "";
            if (note.IsSuperchat)
                return presented >= 3 ? "눌러서 차지 후 떼기" : "슈퍼챗 Space";
            return note.Kind switch
            {
                ChatKind.Positive => "← 긍정",
                ChatKind.Empathy => "↓ 공감",
                ChatKind.Laugh => "→ 웃음",
                _ => "↑ 감사"
            };
        }

        void TickOnAir()
        {
            float u = _onAirLeft / 0.6f;
            bool show = u > 0.001f;
            if (_onAirRoot != null)
            {
                _onAirRoot.gameObject.SetActive(show);
                if (show)
                    _onAirRoot.SetAsLastSibling();
            }
            if (!show)
                return;
            if (_onAirWash != null)
            {
                var w = _onAirWash.color;
                w.a = 0.72f * u;
                _onAirWash.color = w;
            }
            if (_onAirLed != null)
            {
                var led = Color.white;
                led.a = u;
                _onAirLed.color = led;
                _onAirLed.rectTransform.localScale = Vector3.one * (1f + 0.18f * u);
            }
            if (_onAirPip != null)
            {
                var p = Palette.MoneyRed;
                p.a = u;
                _onAirPip.color = p;
                _onAirPip.rectTransform.localScale = Vector3.one * (1f + 0.35f * u);
            }
            if (_onAirLive != null)
            {
                var c = Color.white;
                c.a = u;
                _onAirLive.color = c;
            }
            if (_onAirCopy != null)
            {
                var c = Palette.MoneyRed;
                c.a = u;
                _onAirCopy.color = c;
            }
        }

        void RefreshCoach()
        {
            bool on = _session != null && _session.CoachActive && _onAirLeft <= 0f;
            if (_coachHint != null)
                _coachHint.gameObject.SetActive(on);
            if (_coachLegend != null)
                _coachLegend.gameObject.SetActive(on);
            var held = on ? _session.CoachHeld : null;
            if (_coachPrompt != null)
            {
                _coachPrompt.gameObject.SetActive(held != null);
                if (held != null)
                {
                    _coachPrompt.text = CoachPrompt(held, _session.CoachPresented);
                    _coachPrompt.color = held.IsSuperchat ? Palette.Gold : Palette.ForKind(held.Kind);
                }
            }
            if (_coachPadIcon != null)
            {
                _coachPadIcon.gameObject.SetActive(held != null);
                if (held != null)
                {
                    string art = held.IsSuperchat
                        ? ArtSprites.PadSuperchat
                        : KeycapFor(StreamPadButton.Mode.Kind, held.Kind);
                    ArtSprites.Apply(_coachPadIcon, art, Color.white, Color.white);
                    _coachPadIcon.preserveAspect = true;
                }
            }

            StreamPadButton hot = held != null ? CoachPad(held) : null;
            for (int i = 0; i < _lanePads.Length; i++)
            {
                if (_lanePads[i] == null)
                    continue;
                _lanePads[i].SetPulse(_lanePads[i] == hot);
            }
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

        void RefreshClockChip()
        {
            int shown = Mathf.CeilToInt(_session.TimeLeft);
            bool lastTen = _session.TimeLeft > 0f && _session.TimeLeft <= 10f;
            if (_session.TimeLeft <= 0f)
            {
                _timer.text = "종료";
                _timer.color = Palette.MoneyRed;
                if (_timerChip != null)
                    _timerChip.localScale = Vector3.one;
                if (_timerChipImg != null)
                    _timerChipImg.color = Color.Lerp(Color.white, Palette.MoneyRed, 0.72f);
            }
            else if (lastTen)
            {
                _timer.text = shown.ToString();
                float pulse = 0.5f + 0.5f * Mathf.Abs(Mathf.Sin(Time.time * 9f));
                _timer.color = Color.Lerp(Palette.MoneyRed, Color.white, pulse * 0.35f);
                if (_timerChip != null)
                    _timerChip.localScale = Vector3.one * (1f + 0.10f * pulse);
                if (_timerChipImg != null)
                    _timerChipImg.color = Color.Lerp(Color.white, Palette.MoneyRed, 0.35f + pulse * 0.55f);
                if (shown != _lastClockSec && shown >= 1)
                    PlaySfx(_clockTick, 0.38f);
            }
            else
            {
                _timer.text = shown + "s";
                _timer.color = Palette.Pastel;
                if (_timerChip != null)
                    _timerChip.localScale = Vector3.one;
                if (_timerChipImg != null)
                    _timerChipImg.color = Color.white;
            }
            _lastClockSec = shown;
        }

        void RefreshHud()
        {
            _viewers.text = Mathf.RoundToInt(_shownViewers).ToString();
            PaintViewerChip();
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
            float punch = 1f + 0.28f * _incomePunch;
            _incomeNow.rectTransform.localScale = Vector3.one * punch;
            if (_remain != null)
                _remain.rectTransform.localScale = Vector3.one * punch;
            int remain = _tonightBills - ticking;
            bool covered = remain <= 0 || _billsCovered;
            if (!_billsCovered && shown >= _tonightBills && _tonightBills > 0)
            {
                _billsCovered = true;
                covered = true;
                SlamBillCover();
            }
            _remain.text = covered ? "청구 커버" : EconomyRules.FormatWon(remain);
            _remain.color = covered ? Palette.CashGreen : Palette.MoneyRed;
            if (_billChip != null)
            {
                _billChip.text = "청구 " + EconomyRules.FormatWon(_tonightBills);
                _billChip.color = covered || _billsCovered ? Palette.Ink : Color.white;
            }
            if (_billChipImg != null)
                _billChipImg.color = covered || _billsCovered
                    ? Palette.Gold
                    : Color.white;
            if (_billFill != null)
            {
                float fill = _tonightBills <= 0 ? 1f : Mathf.Clamp01(ticking / (float)_tonightBills);
                _billFill.rectTransform.anchorMax = new Vector2(fill, 1f);
                _billFill.color = covered || _billsCovered ? Palette.Gold : Palette.MoneyRed;
            }
            if (_session.HypeActive)
                _hypeMul.text = $"하이프 {_session.Balance.hypeIncomeMultiplier:0.#}x";
            else if (_session.PerfectCombo >= _session.Balance.comboIncomeThreshold)
                _hypeMul.text = $"콤보 {_session.Balance.comboIncomeMultiplier:0.#}x";
            else
                _hypeMul.text = "";
            _hypeMul.color = Palette.Gold;
            _hypeMul.fontSize = _session.HypeActive ? 22 : 14;
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
                _raceFill.color = covered || _billsCovered ? Palette.CashGreen : Palette.MoneyRed;
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
            var mentalCol = _session.Mental <= 24 ? Palette.MoneyRed : Palette.Pink;
            _mental.color = Color.Lerp(mentalCol, Color.white, _mentalPunch * 0.35f);
            if (_mentalPunch > 0.02f)
                _mental.color = Color.Lerp(_mental.color, Palette.MoneyRed, _mentalPunch);
            _mental.rectTransform.localScale = Vector3.one * (1f + 0.32f * _mentalPunch);
            RefreshClockChip();
            if (_comboBreakLeft > 0f)
            {
                _combo.text = "콤보 끊김";
                _combo.color = Palette.MoneyRed;
                _combo.fontSize = 34;
            }
            else if (_session.IncomeFreezeLeft > 0f)
            {
                _combo.fontSize = 22;
                _combo.text = $"송출 끊김 {_session.IncomeFreezeLeft:0.0}s";
                _combo.color = Palette.Pastel;
            }
            else if (_session.IncomeShieldLeft > 0f)
            {
                _combo.fontSize = 22;
                _combo.text = $"수익 보호막 {_session.IncomeShieldLeft:0.0}s";
                _combo.color = Palette.Pastel;
            }
            else if (_session.HypeActive)
            {
                _combo.fontSize = 22;
                _combo.text = $"{_session.Tuning.Name}  ·  하이프 {_session.HypeLeft:0.0}s  ·  {_session.Balance.hypeIncomeMultiplier:0.#}x";
                _combo.color = Palette.Gold;
            }
            else
            {
                _combo.fontSize = 22;
                _combo.text = $"{_session.Tuning.Name}  ·  COMBO {_session.Combo}   PERFECT {_session.PerfectCombo}";
                _combo.color = Palette.Pastel;
            }
            float tension = Mathf.Clamp01(_session.MissStreak / (float)_session.Balance.missStreakMental);
            _tensionFill.rectTransform.anchorMax = new Vector2(tension, 1f);
        }

        void TickEventWarn()
        {
            if (_eventWarnBox == null)
                return;
            bool on = _session != null && _session.TryPeekEventWarn(out var kind);
            _eventWarnBox.gameObject.SetActive(on);
            if (!on)
                return;
            if (_eventWarn != null)
                _eventWarn.text = StreamEventState.WarnCopy(kind);
            var img = _eventWarnBox.GetComponent<Image>();
            bool anti = kind == StreamEventKind.AntiWave;
            float u = 0.5f + 0.5f * Mathf.Abs(Mathf.Sin(Time.time * 10f));
            if (img != null)
            {
                ArtSprites.Apply(img, ArtSprites.EventWarn, new Color(0.58f, 0.08f, 0.16f, 0.94f), Color.white);
                img.preserveAspect = false;
                img.color = anti
                    ? new Color(0.72f, 0.08f, 0.16f, 0.88f + 0.10f * u)
                    : new Color(0.22f, 0.30f, 0.38f, 0.88f + 0.10f * u);
            }
            _eventWarnBox.localScale = Vector3.one * (1f + 0.06f * u);
        }

        void TickStrike()
        {
            if (_strike == null)
                return;
            bool perfect = false;
            if (_session != null)
            {
                var notes = _session.Notes;
                for (int i = 0; i < notes.Count; i++)
                {
                    var n = notes[i];
                    if (n.Consumed)
                        continue;
                    float abs = Mathf.Abs(_session.Elapsed - n.HitTime);
                    if (StreamRules.Judge(abs, _session.Balance, _session.Tuning.PerfectWindowMul) == Judgement.Perfect)
                    {
                        perfect = true;
                        break;
                    }
                }
            }
            if (perfect)
            {
                float u = 0.5f + 0.5f * Mathf.Abs(Mathf.Sin(Time.time * 12f));
                _strike.color = Color.Lerp(Color.white, Palette.Gold, 0.35f + 0.55f * u);
                _strike.rectTransform.localScale = new Vector3(1f, 1f + 0.28f * u, 1f);
            }
            else
            {
                _strike.color = new Color(1f, 0.95f, 0.72f, 0.96f);
                _strike.rectTransform.localScale = Vector3.one;
            }
        }

        void SyncNotes()
        {
            foreach (var note in _session.Notes)
            {
                if (note.Consumed)
                {
                    if (_heldNotes.Contains(note))
                        continue;
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
                float jitter = _session.EventActive ? 0f : _look.LaneJitter;
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
                float abs = Mathf.Abs(_session.Elapsed - note.HitTime);
                bool hot = abs <= 0.15f;
                var glow = rt.Find("Hot");
                if (glow != null)
                {
                    var g = glow.GetComponent<Image>();
                    if (g != null)
                        g.color = new Color(1f, 1f, 1f, hot ? 0.40f : 0f);
                }
                TintTravelNote(rt, note);
                if (note.IsSuperchat)
                {
                    float slam = Mathf.Clamp01((_session.Elapsed - note.SpawnTime) / 0.18f);
                    float s = Mathf.Lerp(1.38f, 1f, slam * slam);
                    rt.localScale = Vector3.one * (s * (hot ? 1.06f : 1f));
                }
                else
                {
                    rt.localScale = Vector3.one * (hot ? 1.08f : 1f);
                }
                if (note.FanWounded)
                    DimNamedBubble(rt);
            }

            var fade = _lane.GetComponent<CanvasGroup>();
            if (fade != null)
            {
                bool overlay = _session.EventActive || _session.PromoActive || _session.LineActive || _session.ConcertActive;
                fade.alpha = _session.EventActive ? 0.22f : overlay ? 0.38f : 1f;
            }
            if (_laneFreeze != null)
            {
                bool frozen = _session.EventActive;
                var fc = _eventStingKind == StreamEventKind.GearLag
                    ? new Color(0.35f, 0.55f, 0.7f, frozen ? 0.42f : 0f)
                    : new Color(0.7f, 0.08f, 0.16f, frozen ? 0.4f : 0f);
                _laneFreeze.color = fc;
            }
        }

        void RefreshPromoOverlay()
        {
            bool on = _session.PromoActive;
            _promoRoot.gameObject.SetActive(on);
            if (on)
            {
                _eventDim.gameObject.SetActive(true);
                var w3 = GameManager.Instance != null ? GameManager.Instance.Week3 : null;
                float mul = w3 != null ? w3.goodsPromoMultiplier : 1.5f;
                _promoBody.text = $"지금 아크릴 홍보?\n성공 시 오늘 판매 {mul:0.#}배";
                _promoTimer.text = $"{_session.Promo.TimeLeft:0.00}s";
            }
        }

        void FlashPromoSuccess()
        {
            var w3 = GameManager.Instance != null ? GameManager.Instance.Week3 : null;
            float mul = w3 != null ? w3.goodsPromoMultiplier : 1.5f;
            if (_promoSlam == null)
                return;
            _promoSlam.text = $"홍보 성공 {mul:0.#}x";
            var c = Palette.Gold;
            c.a = 1f;
            _promoSlam.color = c;
            _promoSlam.transform.SetAsLastSibling();
            _promoSlamFlash = 1.2f;
        }

        void RefreshLineOverlay()
        {
            bool on = _session.LineActive;
            _lineRoot.gameObject.SetActive(on);
            if (on)
            {
                _eventDim.gameObject.SetActive(true);
                var w4 = GameManager.Instance != null ? GameManager.Instance.Week4 : null;
                int keep = w4 != null ? w4.sponsorLineBonus : 3000;
                _lineBody.text = $"스폰서 멘트\n계약 유지 +{EconomyRules.FormatWon(keep)}\n실패 시 계약 파기";
                _lineTimer.text = $"{_session.Line.TimeLeft:0.00}s";
            }
        }

        void FlashLineResult()
        {
            if (_lineSlam == null)
                return;
            var w4 = GameManager.Instance != null ? GameManager.Instance.Week4 : null;
            int keep = w4 != null ? w4.sponsorLineBonus : 3000;
            int lose = w4 != null ? w4.sponsorFailCash : 15000;
            int mental = w4 != null ? w4.sponsorFailMental : 12;
            bool ok = _session != null && _session.Line.Success;
            if (ok)
            {
                _lineSlam.text = $"계약 유지 +{EconomyRules.FormatWon(keep)}";
                _lineSlam.color = Palette.Gold;
            }
            else
            {
                _lineSlam.text = $"계약 파기 현금 −{EconomyRules.FormatWon(lose)} 멘탈 −{mental}";
                _lineSlam.color = Palette.MoneyRed;
            }
            var c = _lineSlam.color;
            c.a = 1f;
            _lineSlam.color = c;
            _lineSlam.transform.SetAsLastSibling();
            _lineSlamFlash = 1.2f;
        }

        void RefreshConcertOverlay()
        {
            bool on = _session.ConcertActive;
            _concertRoot.gameObject.SetActive(on);
            if (on)
            {
                _eventDim.gameObject.SetActive(true);
                var w5 = GameManager.Instance != null ? GameManager.Instance.Week5 : null;
                float mul = w5 != null ? w5.concertSuccessMultiplier : 1.3f;
                _concertBody.text = $"퍼포먼스 지금?\n성공 시 정산 ×{mul:0.#}";
                _concertTimer.text = $"{_session.Concert.TimeLeft:0.00}s";
            }
        }

        void FlashConcertSuccess()
        {
            if (_concertSlam == null)
                return;
            var w5 = GameManager.Instance != null ? GameManager.Instance.Week5 : null;
            float mul = w5 != null ? w5.concertSuccessMultiplier : 1.3f;
            _concertSlam.text = $"정산 ×{mul:0.#}";
            var c = Palette.Gold;
            c.a = 1f;
            _concertSlam.color = c;
            _concertSlam.transform.SetAsLastSibling();
            _concertSlamFlash = 1.2f;
        }

        void RefreshEventOverlay()
        {
            bool on = _session.EventActive;
            bool sting = on && _eventStingLeft > 0f;
            _eventRoot.gameObject.SetActive(on && !sting);
            _eventDim.gameObject.SetActive(on || _session.PromoActive || _session.LineActive || _session.ConcertActive);
            if (_eventDim != null && on)
                _eventDim.color = new Color(0.06f, 0.03f, 0.08f, sting ? 0.82f : 0.62f);
            _charge.gameObject.SetActive(!on && !_session.PromoActive && !_session.LineActive && !_session.ConcertActive && StreamBindings.SuperchatCharging);
            if (!on)
            {
                ResetEventPads();
                return;
            }

            _eventTitle.text = StreamEventState.DisplayName(_session.Event.Kind);
            _eventBody.text = StreamEventState.Prompt(_session.Event.Kind);
            _eventTimer.text = $"{_session.Event.TimeLeft:0.00}s";
            int target = _session.Event.TargetKey;
            for (int i = 0; i < 4; i++)
            {
                bool hot = i + 1 == target;
                float pulse = 0.88f + 0.12f * Mathf.Abs(Mathf.Sin(Time.time * 12f));
                var idle = hot
                    ? new Color(1f, 0.94f, 0.28f, pulse)
                    : new Color(0.28f, 0.24f, 0.3f, 0.2f);
                if (_eventPads[i] != null)
                    _eventPads[i].SetIdleColor(idle);
                else
                    _eventKeys[i].color = idle;
                _eventKeyLabels[i].color = hot ? Palette.Ink : Palette.Muted;
                _eventKeys[i].rectTransform.localScale = hot
                    ? Vector3.one * (1.18f + 0.1f * Mathf.Abs(Mathf.Sin(Time.time * 10f)))
                    : Vector3.one;
                _eventPads[i]?.SetPulse(hot);
            }
        }

        static Color NotePadColor(ChatNote note)
        {
            return note.IsSuperchat ? Palette.Gold : Palette.ForKind(note.Kind);
        }

        static void TintTravelNote(RectTransform rt, ChatNote note)
        {
            if (rt == null || note.FanWounded)
                return;
            var pad = NotePadColor(note);
            var chip = rt.Find("NoteChip");
            if (chip != null)
            {
                var c = chip.GetComponent<Image>();
                if (c != null)
                {
                    pad.a = c.color.a > 0.01f ? c.color.a : 1f;
                    c.color = pad;
                }
            }
            var edge = rt.Find("KindEdge");
            if (edge != null)
            {
                var e = edge.GetComponent<Image>();
                if (e != null)
                {
                    pad.a = e.color.a > 0.01f ? e.color.a : 1f;
                    e.color = pad;
                }
                return;
            }
            if (chip != null)
                return;
            var body = rt.GetComponent<Image>();
            if (body == null)
                return;
            pad.a = body.color.a;
            body.color = pad;
        }

        static float NoteChipAngle(ChatKind kind)
        {
            return kind switch
            {
                ChatKind.Empathy => 90f,
                ChatKind.Laugh => 180f,
                ChatKind.Thanks => 270f,
                _ => 0f
            };
        }

        RectTransform MakeBubble(ChatNote note)
        {
            bool super = note.IsSuperchat;
            bool troll = !super && note.Kind == ChatKind.Laugh;
            bool named = note.NamedFan;
            var color = NotePadColor(note);
            var card = UiKit.Panel(_lane, "Note", Color.white);
            float scale = _look.BubbleScale > 0.1f ? _look.BubbleScale : 1f;
            if (_look.LoudTroll && troll)
                scale *= 1.12f;
            float h = (named || super ? 96f : troll ? 82f : 78f) * scale;
            float w = (super || named ? 400f : 372f) * scale;
            UiKit.Layout(card, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(w, h));
            var img = card.GetComponent<Image>();
            float a = _look.DimWash ? 0.80f : 0.94f;
            bool chatBubble = !super && !troll;
            if (named && super)
                ArtSprites.ApplySliced(img, ArtSprites.SuperchatBanner, note.FanWounded ? new Color(0.72f, 0.62f, 0.28f, 0.72f) : new Color(1f, 0.86f, 0.28f, 1f), new Vector4(36f, 28f, 36f, 28f));
            else if (super)
                ArtSprites.ApplySliced(img, ArtSprites.SuperchatBanner, new Color(1f, 0.86f, 0.28f, 1f), new Vector4(36f, 28f, 36f, 28f));
            else if (troll)
            {
                ArtSprites.Apply(img, ArtSprites.TrollBubble, color, color);
                img.preserveAspect = false;
            }
            else
                ArtSprites.ApplySliced(img, ArtSprites.ChatBubble, Color.white, new Vector4(48f, 36f, 48f, 36f));
            if (chatBubble)
            {
                var edge = UiKit.Image(card, "KindEdge", color);
                UiKit.Layout(edge.rectTransform, new Vector2(0f, 0.18f), new Vector2(0f, 0.82f), new Vector2(0f, 0.5f), new Vector2(14f, 0f), new Vector2(12f, 0f));
                edge.raycastTarget = false;
                edge.transform.SetAsFirstSibling();
                if (named && note.FanWounded)
                    edge.color = new Color(color.r * 0.72f, color.g * 0.72f, color.b * 0.72f, 0.55f);
                img.color = new Color(1f, 1f, 1f, named && note.FanWounded ? 0.72f : a);
            }
            {
                var chipCol = named && note.FanWounded
                    ? new Color(color.r * 0.72f, color.g * 0.72f, color.b * 0.72f, 0.72f)
                    : new Color(color.r, color.g, color.b, a);
                var chip = UiKit.Image(card, "NoteChip", chipCol);
                float chipSize = (super ? 76f : troll ? 78f : 72f) * scale;
                UiKit.Layout(chip.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(42f, 0f), new Vector2(chipSize, chipSize));
                if (super)
                    ArtSprites.Apply(chip, ArtSprites.SuperchatChip, chipCol, chipCol);
                else
                {
                    ArtSprites.Apply(chip, ArtSprites.NoteChip, chipCol, chipCol);
                    chip.rectTransform.localEulerAngles = new Vector3(0f, 0f, NoteChipAngle(note.Kind));
                }
                chip.preserveAspect = true;
                chip.raycastTarget = false;
            }

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
            if (super)
            {
                var keyT = UiKit.Label(card, "Key", key, 16, Palette.Ink, TextAnchor.MiddleCenter, FontStyle.Bold);
                UiKit.Layout(keyT.rectTransform, new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0.5f), new Vector2(18, 0), new Vector2(56, 0));
            }
            string nickLine = note.User ?? "";
            if (!named && super && note.SuperchatWon > 0)
                nickLine = $"{note.User}  ·  {EconomyRules.FormatWon(note.SuperchatWon)}";
            else if (named)
                nickLine = $"{note.User}  ·  {fanTag}";
            var nickCol = named
                ? (super ? Palette.Ink : new Color(0.42f, 0.12f, 0.28f, 1f))
                : super
                    ? Palette.Ink
                    : troll
                        ? new Color(1f, 0.92f, 0.94f, 0.95f)
                        : Palette.Gold;
            var nickT = UiKit.Label(card, "Nick", nickLine, named || super ? 15 : 14, nickCol, TextAnchor.MiddleLeft, FontStyle.Bold);
            UiKit.Layout(nickT.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1), new Vector2(76, -6), new Vector2(-88, 22));
            if (super && note.SuperchatWon > 0)
            {
                var wonT = UiKit.Label(card, "Won", EconomyRules.FormatWon(note.SuperchatWon), 20, Palette.Ink, TextAnchor.MiddleRight, FontStyle.Bold);
                UiKit.Layout(wonT.rectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(-14, -6), new Vector2(120, 24));
            }
            string body = named && super
                ? $"{kind}  {EconomyRules.FormatWon(note.SuperchatWon)}  {note.Text}"
                : note.Text;
            var msgCol = troll ? Color.white : Palette.Ink;
            if (named && note.FanWounded)
                msgCol = new Color(msgCol.r, msgCol.g, msgCol.b, 0.55f);
            var msg = UiKit.Label(card, "Msg", body, super ? 16 : 17, msgCol, TextAnchor.MiddleLeft, FontStyle.Bold);
            UiKit.Layout(msg.rectTransform, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 0), new Vector2(76, 6), new Vector2(-88, -28));
            msg.horizontalOverflow = HorizontalWrapMode.Wrap;
            if (super)
                card.localScale = Vector3.one * 1.38f;
            var glow = UiKit.Image(card, "Hot", new Color(1f, 1f, 1f, 0f));
            UiKit.Stretch(glow.rectTransform);
            glow.raycastTarget = false;
            glow.transform.SetSiblingIndex(0);
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
            DimNamedLabel(rt.Find("Msg"));
            DimNamedLabel(rt.Find("Nick"));
        }

        static void DimNamedLabel(Transform node)
        {
            if (node == null)
                return;
            var t = node.GetComponent<Text>();
            if (t != null && t.color.a > 0.6f)
            {
                var c = t.color;
                t.color = new Color(c.r, c.g, c.b, 0.55f);
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
                Judgement.Good => Color.white,
                _ => Palette.MoneyRed
            };
            _judge.fontSize = j switch
            {
                Judgement.Perfect => 76,
                Judgement.Great => 48,
                Judgement.Good => 34,
                _ => 64
            };
            if (_judgeStamp != null)
            {
                if (j == Judgement.Perfect)
                {
                    ArtSprites.Apply(_judgeStamp, ArtSprites.JudgePerfect, Palette.Gold, Color.white);
                    _judgeStamp.preserveAspect = false;
                    _judgeStamp.gameObject.SetActive(true);
                    UiKit.Layout(_judgeStamp.rectTransform, new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.5f), new Vector2(-80, 0), new Vector2(560, 110));
                }
                else if (j == Judgement.Good)
                {
                    ArtSprites.Apply(_judgeStamp, ArtSprites.JudgeGood, Color.white, Color.white);
                    _judgeStamp.preserveAspect = false;
                    _judgeStamp.gameObject.SetActive(true);
                    UiKit.Layout(_judgeStamp.rectTransform, new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.5f), new Vector2(-80, 0), new Vector2(360, 72));
                }
                else if (j == Judgement.Miss)
                {
                    ArtSprites.Apply(_judgeStamp, ArtSprites.JudgeMiss, Palette.MoneyRed, Color.white);
                    _judgeStamp.preserveAspect = false;
                    _judgeStamp.gameObject.SetActive(true);
                    UiKit.Layout(_judgeStamp.rectTransform, new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.5f), new Vector2(-80, 0), new Vector2(480, 96));
                }
                else
                {
                    _judgeStamp.gameObject.SetActive(false);
                }
            }
            _judgeFlash = 1f;
            _judgeBig = j == Judgement.Perfect || j == Judgement.Miss;
            if (j == Judgement.Perfect)
            {
                _judgePopMax = 0.2f;
                var s = Vector3.one * 1.72f;
                _judge.rectTransform.localScale = s;
                if (_judgeStamp != null)
                    _judgeStamp.rectTransform.localScale = s;
            }
            else if (j == Judgement.Miss)
            {
                _judgePopMax = 0.25f;
                var s = Vector3.one * 1.58f;
                _judge.rectTransform.localScale = s;
                if (_judgeStamp != null)
                    _judgeStamp.rectTransform.localScale = s;
            }
            else if (j == Judgement.Good)
            {
                _judgePopMax = 0.12f;
                var s = Vector3.one * 1.08f;
                _judge.rectTransform.localScale = s;
                if (_judgeStamp != null)
                    _judgeStamp.rectTransform.localScale = s;
            }
            else
            {
                _judgePopMax = 0.12f;
                _judge.rectTransform.localScale = Vector3.one * 1.22f;
            }
            _judgePop = _judgePopMax;
        }

        void BeginSuperchatFly(ChatNote note)
        {
            if (note == null || _fxRoot == null)
                return;
            _views.TryGetValue(note, out var fromRt);
            bool firstMinjun = false;
            var gm = GameManager.Instance;
            if (gm != null && gm.Run != null && !gm.Run.minjunEver)
                firstMinjun = true;
            if (fromRt != null)
            {
                _heldNotes.Add(note);
                var won = fromRt.Find("Won");
                if (won != null)
                    won.localScale = Vector3.one * 1.45f;
                if (firstMinjun)
                    StampMinjunFirst(fromRt);
            }

            string wonLine = EconomyRules.FormatWon(note.SuperchatWon);
            var fly = UiKit.Label(_fxRoot, "WonFly", firstMinjun ? wonLine + "\n민준 첫 도네" : wonLine, 34, Palette.CashGreen, TextAnchor.MiddleCenter, FontStyle.Bold);
            fly.horizontalOverflow = HorizontalWrapMode.Overflow;
            fly.verticalOverflow = VerticalWrapMode.Overflow;
            UiKit.Layout(fly.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(280, 64));
            var start = fromRt != null ? LocalIn(_fxRoot, fromRt) : new Vector2(180f, -40f);
            var dest = _incomeNow != null ? LocalIn(_fxRoot, _incomeNow.rectTransform) : new Vector2(-80f, 280f);
            fly.rectTransform.anchoredPosition = start;
            fly.rectTransform.localScale = Vector3.one * 1.35f;
            _wonFlies.Add(new WonFly { Rt = fly.rectTransform, From = start, To = dest, T = 0f });
        }

        void BeginSuperchatCrack(ChatNote note)
        {
            if (note == null || !_views.TryGetValue(note, out var rt) || rt == null)
                return;
            _heldNotes.Add(note);
            _scCracks.Add(new ScCrack { Note = note, Rt = rt, T = 0f, Start = rt.anchoredPosition });
        }

        static void StampMinjunFirst(RectTransform banner)
        {
            if (banner == null)
                return;
            var nick = banner.Find("Nick");
            if (nick != null)
            {
                var t = nick.GetComponent<Text>();
                if (t != null)
                    t.text = "민준 첫 도네";
            }
        }

        void TickSuperchatFx(float dt)
        {
            for (int i = _wonFlies.Count - 1; i >= 0; i--)
            {
                var fly = _wonFlies[i];
                fly.T += dt / 0.48f;
                float u = Mathf.Clamp01(fly.T);
                float ease = 1f - (1f - u) * (1f - u);
                if (fly.Rt != null)
                {
                    var p = Vector2.Lerp(fly.From, fly.To, ease);
                    p.y += Mathf.Sin(u * Mathf.PI) * 36f;
                    fly.Rt.anchoredPosition = p;
                    fly.Rt.localScale = Vector3.one * Mathf.Lerp(1.35f, 0.72f, ease);
                    var text = fly.Rt.GetComponent<Text>();
                    if (text != null)
                    {
                        var c = text.color;
                        c.a = u < 0.82f ? 1f : 1f - (u - 0.82f) / 0.18f;
                        text.color = c;
                    }
                }
                if (u >= 1f)
                {
                    _incomePunch = 1f;
                    if (fly.Rt != null)
                        Destroy(fly.Rt.gameObject);
                    _wonFlies.RemoveAt(i);
                }
                else
                    _wonFlies[i] = fly;
            }

            for (int i = _scCracks.Count - 1; i >= 0; i--)
            {
                var crack = _scCracks[i];
                crack.T += dt / 0.42f;
                float u = Mathf.Clamp01(crack.T);
                if (crack.Rt != null)
                {
                    crack.Rt.anchoredPosition = crack.Start + new Vector2(u * 28f, -u * u * 160f);
                    crack.Rt.localEulerAngles = new Vector3(0f, 0f, -18f * u);
                    crack.Rt.localScale = Vector3.one * (1f - 0.18f * u);
                    var img = crack.Rt.GetComponent<Image>();
                    if (img != null)
                    {
                        var c = img.color;
                        c.a = 1f - u;
                        img.color = c;
                    }
                }
                if (u >= 1f)
                {
                    if (crack.Note != null)
                    {
                        _heldNotes.Remove(crack.Note);
                        _views.Remove(crack.Note);
                    }
                    if (crack.Rt != null)
                        Destroy(crack.Rt.gameObject);
                    _scCracks.RemoveAt(i);
                }
                else
                    _scCracks[i] = crack;
            }

            if (_wonFlies.Count == 0)
            {
                var done = new List<ChatNote>();
                foreach (var n in _heldNotes)
                {
                    bool cracking = false;
                    for (int c = 0; c < _scCracks.Count; c++)
                    {
                        if (_scCracks[c].Note == n)
                            cracking = true;
                    }
                    if (!cracking)
                        done.Add(n);
                }
                for (int i = 0; i < done.Count; i++)
                {
                    var n = done[i];
                    _heldNotes.Remove(n);
                    if (_views.TryGetValue(n, out var rt) && rt != null && n.Consumed)
                    {
                        Destroy(rt.gameObject);
                        _views.Remove(n);
                    }
                }
            }
        }

        static Vector2 LocalIn(RectTransform host, RectTransform src)
        {
            if (host == null || src == null)
                return Vector2.zero;
            var world = src.TransformPoint(src.rect.center);
            return (Vector2)host.InverseTransformPoint(world);
        }

        struct WonFly
        {
            public RectTransform Rt;
            public Vector2 From;
            public Vector2 To;
            public float T;
        }

        struct ScCrack
        {
            public ChatNote Note;
            public RectTransform Rt;
            public float T;
            public Vector2 Start;
        }

        void SlamBillCover()
        {
            _coverSlamFlash = 1f;
            if (_coverSlam != null)
            {
                _coverSlam.text = "청구 커버";
                var c = Palette.Gold;
                c.a = 1f;
                _coverSlam.color = c;
            }
            PlaySfx(_billCoverCue, 0.56f);
            _avatar?.HappyPop();
        }

        void ShowIncomeDelta(int won)
        {
            if (_incomePop == null)
                return;
            if (won <= 0 && _session != null)
            {
                float span = Mathf.Max(0.08f, _session.Elapsed - _incomeMarkedAt);
                won = StreamRules.TickIncome(
                    _session.Viewers,
                    span,
                    _session.IncomeMultiplier,
                    _session.Balance);
            }
            if (won <= 0)
                return;
            _incomePop.text = "+" + EconomyRules.FormatWon(won);
            var c = Palette.CashGreen;
            c.a = 1f;
            _incomePop.color = c;
            _incomePopFlash = 1f;
        }

        void ShowViewerDelta(float viewerDelta)
        {
            if (_viewerPop == null || Mathf.Abs(viewerDelta) < 0.049f)
                return;
            bool up = viewerDelta > 0f;
            _viewerPop.text = up
                ? $"시청 +{viewerDelta:0.0}"
                : $"시청 −{Mathf.Abs(viewerDelta):0.0}";
            var c = up ? Palette.CashGreen : Palette.MoneyRed;
            c.a = 1f;
            _viewerPop.color = c;
            _viewerPopFlash = 1f;
            _viewerChipPop = 0.1f;
            _viewerChipUp = up;
            if (up)
                _viewerFlash = 0f;
            else
                _viewerFlash = 1f;
        }

        void TickViewerChipPop()
        {
            _viewerChipPop = Mathf.MoveTowards(_viewerChipPop, 0f, Time.deltaTime);
            PaintViewerChip();
        }

        void PaintViewerChip()
        {
            float u = _viewerChipPop / 0.1f;
            if (_viewerChip != null)
                _viewerChip.localScale = Vector3.one * (1f + 0.12f * u);
            if (_viewers != null)
            {
                var tint = _viewerChipUp ? Palette.CashGreen : Palette.MoneyRed;
                float amt = Mathf.Max(u, _viewerChipUp ? 0f : _viewerFlash);
                _viewers.color = Color.Lerp(Palette.Pastel, tint, amt);
            }
            if (_viewerChipImg != null)
            {
                var bg = Color.white;
                var wash = _viewerChipUp ? Palette.CashGreen : Palette.MoneyRed;
                _viewerChipImg.color = Color.Lerp(bg, wash, u * 0.55f);
            }
        }

        void ShowMissSting(float viewerDelta, int mentalDelta)
        {
            ShowViewerDelta(viewerDelta);
            if (mentalDelta != 0)
            {
                _sting.text = $"멘탈 {mentalDelta}";
                var c = Palette.MoneyRed;
                c.a = 1f;
                _sting.color = c;
                _stingFlash = 1.15f;
            }
            _viewerFlash = 1f;
        }

        void ShowComboBreak()
        {
            _comboBreakLeft = 0.25f;
            TickComboBreak();
        }

        void TickComboPop()
        {
            _comboPop = Mathf.MoveTowards(_comboPop, 0f, Time.deltaTime);
            var popRt = _comboPlate != null ? _comboPlate.rectTransform : _combo != null ? _combo.rectTransform : null;
            if (popRt == null)
                return;
            float u = _comboPop / 0.1f;
            float amp = _comboPopBig ? 0.22f : 0.15f;
            popRt.localScale = Vector3.one * (1f + amp * u);
        }

        void TickComboBreak()
        {
            if (_comboBreak == null)
                return;
            if (_comboBreakLeft > 0.001f)
            {
                _comboBreak.text = "콤보 끊김";
                float u = _comboBreakLeft / 0.25f;
                var c = Palette.MoneyRed;
                c.a = Mathf.Clamp01(u);
                _comboBreak.color = c;
                var scale = Vector3.one * (1f + 0.28f * u);
                _comboBreak.rectTransform.localScale = scale;
                if (_comboBreakStamp != null)
                {
                    _comboBreakStamp.gameObject.SetActive(true);
                    ArtSprites.Apply(_comboBreakStamp, ArtSprites.ComboBreak, Palette.MoneyRed, Color.white);
                    _comboBreakStamp.preserveAspect = false;
                    var sc = _comboBreakStamp.color;
                    sc.a = c.a;
                    _comboBreakStamp.color = sc;
                    _comboBreakStamp.rectTransform.localScale = scale;
                }
            }
            else
            {
                _comboBreak.text = "";
                _comboBreak.rectTransform.localScale = Vector3.one;
                if (_comboBreakStamp != null)
                {
                    _comboBreakStamp.gameObject.SetActive(false);
                    _comboBreakStamp.rectTransform.localScale = Vector3.one;
                }
            }
        }

        void RefreshHypeShow()
        {
            bool hype = _session != null && _session.HypeActive;
            if (hype && !_hypeWasOn)
                PlaySfx(_hypeCue, 0.58f);
            float pulse = Mathf.Abs(Mathf.Sin(Time.time * 8f));
            if (hype)
            {
                var goldWash = new Color(0.54f, 0.38f, 0.06f, 1f);
                if (_wash != null)
                    _wash.color = Color.Lerp(_look.Wash, goldWash, 0.86f);
                if (_washVeil != null)
                    _washVeil.color = new Color(1f, 0.84f, 0.22f, 0.40f + 0.10f * pulse);
                if (_hypeFlash != null)
                    _hypeFlash.color = new Color(1f, 0.86f, 0.22f, 0.46f + 0.12f * pulse);
                if (_hypeFrame != null)
                {
                    _hypeFrame.gameObject.SetActive(true);
                    ArtSprites.Apply(_hypeFrame, ArtSprites.HypeFrame, Palette.Gold, Color.white);
                    _hypeFrame.preserveAspect = false;
                    var fc = _hypeFrame.color;
                    fc.a = 0.88f + 0.12f * pulse;
                    _hypeFrame.color = fc;
                }
                if (_hypeChatGlow != null)
                    _hypeChatGlow.color = new Color(1f, 0.88f, 0.32f, 0.16f + 0.06f * pulse);
                if (_hypeBanner != null)
                    _hypeBanner.text = $"하이프 {_session.Balance.hypeIncomeMultiplier:0.#}x";
                if (_hypeCount != null)
                    _hypeCount.text = $"{_session.HypeLeft:0.0}s";
                if (_comboSting != null)
                    _comboSting.text = "";
                _avatar?.SetHype(true);
                UiKit.EnsureCamera(_wash != null ? _wash.color : goldWash);
            }
            else
            {
                if (_hypeWasOn)
                {
                    if (_wash != null)
                        _wash.color = _look.Wash;
                    if (_washVeil != null)
                        _washVeil.color = _look.WashVeil;
                    UiKit.EnsureCamera(_look.Wash);
                    _avatar?.SetHype(false);
                }
                if (_hypeFlash != null)
                    _hypeFlash.color = new Color(1f, 0.82f, 0.25f, _comboStingFlash * 0.20f);
                if (_hypeFrame != null)
                    _hypeFrame.gameObject.SetActive(false);
                if (_hypeChatGlow != null)
                    _hypeChatGlow.color = new Color(1f, 0.86f, 0.28f, 0f);
                if (_hypeBanner != null)
                    _hypeBanner.text = "";
                if (_hypeCount != null)
                    _hypeCount.text = "";
                if (_comboSting != null)
                {
                    _comboSting.text = _comboStingFlash > 0.02f
                        ? $"콤보 {_session.Balance.comboIncomeMultiplier:0.#}x"
                        : "";
                    var c = Palette.Gold;
                    c.a = _comboStingFlash;
                    _comboSting.color = c;
                    _comboSting.rectTransform.localScale = Vector3.one * (1f + 0.18f * _comboStingFlash);
                }
            }
            _hypeWasOn = hype;
        }

        void RefreshMentalShow()
        {
            if (_session == null)
                return;
            int m = _session.Mental;
            bool tired = m <= 40;
            bool danger = m <= 20;
            _avatar?.SetTired(tired, danger);
            if (_mentalWarnBox != null)
                _mentalWarnBox.gameObject.SetActive(danger);
            if (danger && !_mentalWasDanger)
                PlaySfx(_mentalCue, 0.50f);
            if (_mentalGrain != null)
            {
                float a = danger ? 0.28f : tired ? 0.10f : 0f;
                _mentalGrain.color = new Color(1f, 1f, 1f, a);
            }

            bool hype = _session.HypeActive;
            if (!hype && tired)
            {
                var baseWash = _look.Wash;
                float gray = baseWash.grayscale;
                var washed = Color.Lerp(baseWash, new Color(gray, gray, gray, 1f), danger ? 0.55f : 0.38f);
                washed = Color.Lerp(washed, new Color(0.06f, 0.05f, 0.07f, 1f), danger ? 0.32f : 0.16f);
                if (_wash != null)
                    _wash.color = washed;
                if (_washVeil != null)
                {
                    var veil = _look.WashVeil;
                    _washVeil.color = Color.Lerp(veil, new Color(0.04f, 0.03f, 0.05f, danger ? 0.34f : 0.18f), 1f);
                }
                UiKit.EnsureCamera(_wash != null ? _wash.color : washed);
            }
            else if (!hype && _mentalWasTired && !tired)
            {
                if (_wash != null)
                    _wash.color = _look.Wash;
                if (_washVeil != null)
                    _washVeil.color = _look.WashVeil;
                UiKit.EnsureCamera(_look.Wash);
            }
            _mentalWasTired = tired;
            _mentalWasDanger = danger;
        }

        static Sprite GrainSprite()
        {
            var tex = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.filterMode = FilterMode.Point;
            var rng = new System.Random(19);
            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    int n = rng.Next(0, 80);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, n / 255f));
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 64f);
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
            PaintShowChip(look.Type);
            UiKit.EnsureCamera(look.Wash);
            _avatar?.ApplyShow(look);
            if (_bed != null)
            {
                var clip = Resources.Load<AudioClip>(_concertShow ? "Audio/bgm_concert" : "Audio/bgm_stream");
                _bed.clip = clip != null ? clip : BedClip(look.Type);
                _bed.loop = true;
                _bedVolume = _concertShow ? 0.24f : look.BedVolume;
                _bed.volume = _bedVolume;
                _bed.Play();
            }
        }

        void PaintShowChip(StreamContentType type)
        {
            string name = ShowChipName(type);
            if (_showChip != null)
                _showChip.text = name;
            if (_showChipImg != null)
                _showChipImg.color = ShowChipAccent(type);
            if (_showChipIcon != null)
            {
                string icon = ArtSprites.ForContent(type);
                if (icon != null)
                {
                    ArtSprites.Apply(_showChipIcon, icon, Color.white, Color.white);
                    _showChipIcon.preserveAspect = true;
                    _showChipIcon.enabled = true;
                }
                else
                    _showChipIcon.enabled = false;
            }
            if (_showChip != null && _showChip.transform.parent != null)
                _showChip.transform.parent.gameObject.SetActive(name.Length > 0);
        }

        static string ShowChipName(StreamContentType type) => type switch
        {
            StreamContentType.Talk => "토크",
            StreamContentType.Game => "게임",
            StreamContentType.Song => "노래",
            StreamContentType.Reaction => "리액션",
            _ => ""
        };

        static Color ShowChipAccent(StreamContentType type) => type switch
        {
            StreamContentType.Talk => Palette.Pink,
            StreamContentType.Game => Palette.Troll,
            StreamContentType.Song => Palette.Gold,
            StreamContentType.Reaction => Palette.PastelDim,
            _ => Palette.Muted
        };

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

        void BeginEventAccident(StreamEventKind kind)
        {
            _eventStingKind = kind;
            _eventStingLeft = 0.2f;
            _avatar?.Panic();
            if (_eventSting != null)
            {
                _eventSting.gameObject.SetActive(true);
                _eventSting.transform.SetAsLastSibling();
                bool anti = kind == StreamEventKind.AntiWave;
                ArtSprites.Apply(
                    _eventSting,
                    anti ? ArtSprites.AntiSting : ArtSprites.LagSting,
                    anti ? new Color(0.92f, 0.05f, 0.16f, 0.92f) : new Color(0.22f, 0.28f, 0.34f, 0.92f),
                    Color.white);
                _eventSting.preserveAspect = false;
                if (_eventStingLabel != null)
                    _eventStingLabel.text = StreamEventState.DisplayName(kind);
            }
            if (kind == StreamEventKind.AntiWave)
                PlaySfx(_antiCue, 0.62f);
            else if (kind == StreamEventKind.GearLag)
                PlaySfx(_lagCue, 0.62f);
            else
                PlaySfx(_bad, 0.62f);
        }

        void TickEventAccident(float dt)
        {
            if (_eventStingLeft > 0f)
                _eventStingLeft = Mathf.MoveTowards(_eventStingLeft, 0f, dt);

            bool stingOn = _eventStingLeft > 0f;
            if (_eventSting != null)
            {
                _eventSting.gameObject.SetActive(stingOn || _eventSting.color.a > 0.02f);
                float a = stingOn ? 0.92f : Mathf.MoveTowards(_eventSting.color.a, 0f, dt * 6f);
                bool anti = _eventStingKind == StreamEventKind.AntiWave;
                if (stingOn)
                {
                    ArtSprites.Apply(
                        _eventSting,
                        anti ? ArtSprites.AntiSting : ArtSprites.LagSting,
                        anti ? new Color(0.92f, 0.05f, 0.16f, a) : new Color(0.22f, 0.28f, 0.34f, a),
                        Color.white);
                    _eventSting.preserveAspect = false;
                }
                var sc = _eventSting.color;
                sc.a = a;
                _eventSting.color = sc;
                if (_eventStingLabel != null)
                {
                    var lc = _eventStingLabel.color;
                    lc.a = a;
                    _eventStingLabel.color = lc;
                    _eventStingLabel.rectTransform.localScale = Vector3.one * (1f + 0.12f * a);
                }
                for (int i = 0; i < _eventStingBars.Length; i++)
                {
                    if (_eventStingBars[i] == null)
                        continue;
                    float slice = Mathf.Repeat(Time.unscaledTime * (anti ? 9f : 18f) + i * 0.17f, 1f);
                    _eventStingBars[i].rectTransform.anchorMin = new Vector2(0f, slice);
                    _eventStingBars[i].rectTransform.anchorMax = new Vector2(1f, slice);
                    _eventStingBars[i].color = anti
                        ? new Color(1f, 0.85f, 0.9f, a * 0.35f)
                        : new Color(0.85f, 0.95f, 1f, a * (0.15f + 0.55f * Mathf.Abs(Mathf.Sin(Time.unscaledTime * 40f + i))));
                }
                if (!stingOn && a <= 0.02f)
                    _eventSting.gameObject.SetActive(false);
            }

            TickEventScar();
        }

        void ApplyEventScar(StreamEventKind kind)
        {
            if (kind == StreamEventKind.AntiWave)
            {
                _eventScarAnti = true;
                BuildEventCrack();
            }
            else if (kind == StreamEventKind.GearLag)
            {
                _eventScarGear = true;
                BuildEventStatic();
            }
        }

        void BuildEventCrack()
        {
            if (_eventCrack != null || _avatar == null || _avatar.Root == null)
                return;
            _eventCrack = new Image[3];
            for (int i = 0; i < _eventCrack.Length; i++)
            {
                var crack = UiKit.Image(_avatar.Root, "EventCrack" + i, new Color(1f, 0.85f, 0.9f, 0.55f));
                float y = 0.28f + i * 0.18f;
                UiKit.Layout(crack.rectTransform, new Vector2(0.08f + i * 0.04f, y), new Vector2(0.92f - i * 0.06f, y), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(0, 5));
                crack.rectTransform.localEulerAngles = new Vector3(0f, 0f, i == 1 ? -12f : 8f + i * 6f);
                crack.raycastTarget = false;
                _eventCrack[i] = crack;
            }
        }

        void BuildEventStatic()
        {
            if (_eventStatic != null || _avatar == null || _avatar.Root == null)
                return;
            _eventStatic = UiKit.Image(_avatar.Root, "EventStatic", new Color(0.75f, 0.85f, 1f, 0.16f));
            UiKit.Stretch(_eventStatic.rectTransform, 16, 16, 44, 18);
            _eventStatic.raycastTarget = false;
        }

        void TickEventScar()
        {
            if (_eventScarAnti && _eventCrack != null)
            {
                float t = Time.unscaledTime;
                for (int i = 0; i < _eventCrack.Length; i++)
                {
                    if (_eventCrack[i] == null)
                        continue;
                    float flicker = 0.42f + 0.16f * Mathf.Abs(Mathf.Sin(t * 2.4f + i));
                    _eventCrack[i].color = new Color(1f, 0.82f, 0.88f, flicker);
                }
            }

            if (_eventScarGear && _eventStatic != null)
            {
                float t = Time.unscaledTime;
                bool slice = Mathf.Repeat(t, 1.6f) < 0.18f;
                float y = Mathf.Repeat(t * 11f, 1f);
                _eventStatic.rectTransform.offsetMin = new Vector2(0f, slice ? y * 10f : 0f);
                _eventStatic.rectTransform.offsetMax = new Vector2(0f, slice ? -((1f - y) * 8f) : 0f);
                _eventStatic.color = new Color(0.7f, 0.82f, 1f, slice ? 0.22f : 0.1f);
            }
        }

        void ResetEventPads()
        {
            for (int i = 0; i < _eventPads.Length; i++)
            {
                _eventPads[i]?.SetPulse(false);
                if (_eventKeys[i] != null)
                    _eventKeys[i].rectTransform.localScale = Vector3.one;
            }
        }

        void PlayPadClick() => PlaySfx(_padClick, 0.34f);

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
