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
        Text _income;
        Text _mental;
        Text _timer;
        Text _combo;
        Text _judge;
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
        Image _tensionFill;
        Image _hypeFlash;
        AudioSource _audio;
        AudioClip _ok;
        AudioClip _bad;
        AudioClip _sc;

        readonly Dictionary<ChatNote, RectTransform> _views = new Dictionary<ChatNote, RectTransform>();
        float _judgeFlash;
        bool _ending;
        bool _eventWasActive;

        const float LaneTop = 260f;
        const float LaneHit = -210f;

        void Awake()
        {
            UiKit.EnsureCamera(Palette.Studio);
            UiKit.EnsureEventSystem();
            Build();
            _audio = gameObject.AddComponent<AudioSource>();
            _audio.playOnAwake = false;
            _ok = Beep(880, 0.07f);
            _bad = Beep(180, 0.11f);
            _sc = Beep(1320, 0.14f);
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
            _session = new StreamSession(gm.Balance, gm.Catalog, gm.Run.mental, gm.Run.viewerBonus);
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
        }

        void Update()
        {
            if (_session == null || _ending)
                return;

            float dt = Time.deltaTime;
            _session.Tick(dt);

            if (_session.EventActive)
            {
                if (StreamBindings.EventKeyPressed(out int idx))
                    _session.TryEventKey(idx);
            }
            else if (_session.PromoActive)
            {
                if (StreamBindings.PromoConfirmDown())
                    _session.TryPromo(true);
                else if (StreamBindings.PromoSkipDown())
                    _session.TryPromo(false);
            }
            else if (_session.LineActive)
            {
                if (StreamBindings.PromoConfirmDown())
                    _session.TryLine(true);
                else if (StreamBindings.PromoSkipDown())
                    _session.TryLine(false);
            }
            else if (_session.ConcertActive)
            {
                if (StreamBindings.PromoConfirmDown())
                    _session.TryConcert(true);
                else if (StreamBindings.PromoSkipDown())
                    _session.TryConcert(false);
            }
            else if (StreamBindings.TryConsumeKind(out var kind, out var hold))
                _session.TryHit(kind, _session.Elapsed, hold);

            MaybeSettleSponsorLine();

            if (_eventWasActive && !_session.EventActive && _session.Event.Resolved)
            {
                bool okHit = _session.Event.Success;
                _judge.text = okHit
                    ? StreamEventState.SuccessCopy(_session.Event.Kind)
                    : StreamEventState.FailCopy(_session.Event.Kind);
                _judge.color = okHit ? Palette.CashGreen : Palette.MoneyRed;
                _judgeFlash = 1f;
                _audio.PlayOneShot(okHit ? _ok : _bad, 0.5f);
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
                    _audio.PlayOneShot(_bad, 0.45f);
                else if (note.IsSuperchat)
                    _audio.PlayOneShot(_sc, 0.5f);
                else
                    _audio.PlayOneShot(_ok, 0.35f);
            }

            SyncNotes();
            RefreshEventOverlay();
            RefreshPromoOverlay();
            RefreshLineOverlay();
            RefreshConcertOverlay();
            RefreshHud();
            _avatar.Tick(dt);

            _judgeFlash = Mathf.MoveTowards(_judgeFlash, 0f, dt * 2.2f);
            var jc = _judge.color;
            jc.a = _judgeFlash;
            _judge.color = jc;
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
            var root = canvas.transform;

            UiKit.Image(root, "Wash", Palette.Studio);
            UiKit.Stretch(root.Find("Wash") as RectTransform);

            _hypeFlash = UiKit.Image(root, "HypeFlash", new Color(1f, 0.82f, 0.25f, 0f));
            UiKit.Stretch(_hypeFlash.rectTransform);

            var top = UiKit.Panel(root, "Top", new Color(0.08f, 0.04f, 0.1f, 0.78f));
            UiKit.Layout(top, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), Vector2.zero, new Vector2(0, 86));

            _viewers = Chip(top, "Viewers", "시청자", new Vector2(20, -16));
            _rival = Chip(top, "Rival", "라이벌", new Vector2(300, -16));
            _income = Chip(top, "Income", "실시간 수익", new Vector2(580, -16));
            _mental = Chip(top, "Mental", "멘탈", new Vector2(860, -16));
            _timer = Chip(top, "Timer", "남은 시간", new Vector2(1140, -16));
            _rival.transform.parent.gameObject.SetActive(false);

            _avatar = new AvatarView(root as RectTransform);

            var chatPanel = UiKit.Panel(root, "Chat", new Color(0.07f, 0.05f, 0.1f, 0.88f));
            UiKit.Layout(chatPanel, new Vector2(1, 0), new Vector2(1, 1), new Vector2(1, 0.5f), new Vector2(-18, 0), new Vector2(420, -120));
            UiKit.Label(chatPanel, "ChatTitle", "채팅", 22, Palette.Pastel, TextAnchor.UpperLeft, FontStyle.Bold);
            var ct = chatPanel.Find("ChatTitle") as RectTransform;
            UiKit.Layout(ct, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -10), new Vector2(-24, 30));

            _lane = UiKit.Panel(chatPanel, "Lane", new Color(1, 1, 1, 0.03f));
            UiKit.Stretch(_lane, 12, 12, 44, 70);

            _hit = UiKit.Panel(_lane, "Hit", new Color(1f, 1f, 1f, 0.22f));
            UiKit.Layout(_hit, new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, LaneHit), new Vector2(0, 10));

            var hitLabel = UiKit.Label(_lane, "HitL", "타이밍", 16, Palette.Pastel, TextAnchor.MiddleRight);
            UiKit.Layout(hitLabel.rectTransform, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-4, LaneHit + 18), new Vector2(80, 20));

            var bottom = UiKit.Panel(root, "Bottom", new Color(0.08f, 0.04f, 0.1f, 0.82f));
            UiKit.Layout(bottom, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), Vector2.zero, new Vector2(0, 110));

            _combo = UiKit.Label(bottom, "Combo", "COMBO 0", 34, Palette.Pastel, TextAnchor.MiddleLeft, FontStyle.Bold);
            UiKit.Layout(_combo.rectTransform, new Vector2(0, 0.45f), new Vector2(0.4f, 1), new Vector2(0, 0.5f), new Vector2(28, 0), Vector2.zero);

            var tensionBg = UiKit.Image(bottom, "TensionBg", new Color(1, 1, 1, 0.12f));
            UiKit.Layout(tensionBg.rectTransform, new Vector2(0, 0), new Vector2(0.38f, 0.38f), new Vector2(0, 0.5f), new Vector2(28, 14), new Vector2(0, 16));
            _tensionFill = UiKit.Image(tensionBg.transform, "Fill", Palette.Troll);
            UiKit.Stretch(_tensionFill.rectTransform);
            var tlab = UiKit.Label(bottom, "TensionL", "텐션 (미스 스트릭)", 14, Palette.Muted, TextAnchor.LowerLeft);
            UiKit.Layout(tlab.rectTransform, new Vector2(0, 0), new Vector2(0.38f, 0.22f), new Vector2(0, 0), new Vector2(28, 4), Vector2.zero);

            var keys = UiKit.Label(bottom, "Keys", "A 긍정   S 공감   D 웃음   F 감사   Space 슈퍼챗   이벤트 1–4   홍보/멘트/콘서트 A/S·D/F", 18, Palette.PastelDim, TextAnchor.MiddleRight);
            UiKit.Layout(keys.rectTransform, new Vector2(0.38f, 0), new Vector2(1, 1), new Vector2(1, 0.5f), new Vector2(-24, 8), Vector2.zero);

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
            UiKit.Layout(_eventTimer.rectTransform, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), new Vector2(0, 16), new Vector2(0, 24));
            for (int i = 0; i < 4; i++)
            {
                var keyImg = UiKit.Image(_eventRoot, "EKey" + (i + 1), new Color(1, 1, 1, 0.12f));
                UiKit.Layout(keyImg.rectTransform, new Vector2(0.5f, 0.38f), new Vector2(0.5f, 0.38f), new Vector2(0.5f, 0.5f), new Vector2((i - 1.5f) * 110f, 0), new Vector2(88, 88));
                var lab = UiKit.Label(keyImg.transform, "L", (i + 1).ToString(), 36, Palette.Pastel, TextAnchor.MiddleCenter, FontStyle.Bold);
                UiKit.Stretch(lab.rectTransform);
                _eventKeys[i] = keyImg;
                _eventKeyLabels[i] = lab;
            }
            _eventRoot.gameObject.SetActive(false);
            _eventDim.gameObject.SetActive(false);

            _promoRoot = UiKit.Panel(root, "PromoCard", new Color(0.12f, 0.08f, 0.18f, 0.96f));
            UiKit.Layout(_promoRoot, new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.5f), new Vector2(-80, 10), new Vector2(560, 220));
            _promoTitle = UiKit.Label(_promoRoot, "PTitle", "굿즈 홍보 타이밍", 34, Palette.Gold, TextAnchor.UpperCenter, FontStyle.Bold);
            UiKit.Layout(_promoTitle.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -16), new Vector2(-24, 44));
            _promoBody = UiKit.Label(_promoRoot, "PBody", "A / S  지금 아크릴 스탠드 홍보\nD / F  넘어가기", 20, Palette.Pastel, TextAnchor.MiddleCenter);
            UiKit.Layout(_promoBody.rectTransform, new Vector2(0, 0.28f), new Vector2(1, 0.72f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(-28, 0));
            _promoTimer = UiKit.Label(_promoRoot, "PTimer", "", 18, Palette.MoneyRed, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(_promoTimer.rectTransform, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), new Vector2(0, 16), new Vector2(0, 24));
            _promoRoot.gameObject.SetActive(false);

            _lineRoot = UiKit.Panel(root, "LineCard", new Color(0.14f, 0.09f, 0.16f, 0.96f));
            UiKit.Layout(_lineRoot, new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.5f), new Vector2(-80, 10), new Vector2(560, 220));
            _lineTitle = UiKit.Label(_lineRoot, "LTitle", "스폰서 멘트 타이밍", 34, Palette.Gold, TextAnchor.UpperCenter, FontStyle.Bold);
            UiKit.Layout(_lineTitle.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -16), new Vector2(-24, 44));
            _lineBody = UiKit.Label(_lineRoot, "LBody", "A / S  스폰서 멘트 넣기\nD / F  놓치면 계약 종료", 20, Palette.Pastel, TextAnchor.MiddleCenter);
            UiKit.Layout(_lineBody.rectTransform, new Vector2(0, 0.28f), new Vector2(1, 0.72f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(-28, 0));
            _lineTimer = UiKit.Label(_lineRoot, "LTimer", "", 18, Palette.MoneyRed, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(_lineTimer.rectTransform, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), new Vector2(0, 16), new Vector2(0, 24));
            _lineRoot.gameObject.SetActive(false);

            _concertRoot = UiKit.Panel(root, "ConcertCard", new Color(0.16f, 0.07f, 0.18f, 0.96f));
            UiKit.Layout(_concertRoot, new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.5f), new Vector2(-80, 10), new Vector2(560, 220));
            _concertTitle = UiKit.Label(_concertRoot, "CTitle", "콘서트 퍼포먼스 타이밍", 34, Palette.Gold, TextAnchor.UpperCenter, FontStyle.Bold);
            UiKit.Layout(_concertTitle.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -16), new Vector2(-24, 44));
            _concertBody = UiKit.Label(_concertRoot, "CBody", "A / S  성공 — 정산 배율 1.3x\nD / F  놓치면 배율 없음", 20, Palette.Pastel, TextAnchor.MiddleCenter);
            UiKit.Layout(_concertBody.rectTransform, new Vector2(0, 0.28f), new Vector2(1, 0.72f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(-28, 0));
            _concertTimer = UiKit.Label(_concertRoot, "CTimer", "", 18, Palette.MoneyRed, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(_concertTimer.rectTransform, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), new Vector2(0, 16), new Vector2(0, 24));
            _concertRoot.gameObject.SetActive(false);
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

        Text Chip(Transform parent, string name, string label, Vector2 pos)
        {
            var box = UiKit.Panel(parent, name, new Color(1, 1, 1, 0.04f));
            UiKit.Layout(box, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), pos, new Vector2(280, 58));
            UiKit.Label(box, "L", label, 14, Palette.Muted, TextAnchor.UpperLeft);
            var l = box.Find("L") as RectTransform;
            UiKit.Layout(l, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1), new Vector2(12, -4), new Vector2(-16, 18));
            var v = UiKit.Label(box, "V", "-", 26, Palette.Pastel, TextAnchor.LowerLeft, FontStyle.Bold);
            UiKit.Layout(v.rectTransform, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 0), new Vector2(12, 4), new Vector2(-16, -20));
            return v;
        }

        void RefreshHud()
        {
            _viewers.text = $"{_session.Viewers:0.0}";
            _viewers.color = Palette.Pastel;
            bool vs = _session.RivalActive;
            _rival.transform.parent.gameObject.SetActive(vs);
            if (vs)
            {
                _rival.text = $"{_session.RivalViewers:0.0}";
                _rival.color = Palette.Troll;
            }
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
                _combo.text = $"HYPE {_session.HypeLeft:0.0}s  ·  x{_session.IncomeMultiplier:0.0}";
            else
                _combo.text = $"COMBO {_session.Combo}   PERFECT {_session.PerfectCombo}";
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
                var img = rt.GetComponent<UnityEngine.UI.Image>();
                if (img != null)
                {
                    var c = img.color;
                    c.a = _session.EventActive || _session.PromoActive || _session.LineActive || _session.ConcertActive ? 0.22f : 0.18f;
                    img.color = c;
                }
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
            var color = Palette.ForKind(note.Kind);
            var card = UiKit.Panel(_lane, "Note", new Color(color.r, color.g, color.b, 0.18f));
            float h = note.IsSuperchat ? 78f : 58f;
            UiKit.Layout(card, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(380, h));
            var stripe = UiKit.Image(card, "Stripe", color);
            UiKit.Layout(stripe.rectTransform, new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0.5f), Vector2.zero, new Vector2(8, 0));

            var badge = UiKit.Image(card, "Badge", color);
            UiKit.Layout(badge.rectTransform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(44, 0), new Vector2(44, 44));
            if (note.IsSuperchat)
                ArtSprites.Apply(badge, ArtSprites.Superchat, Palette.Gold);
            else if (note.Kind == ChatKind.Laugh)
                ArtSprites.Apply(badge, ArtSprites.Troll, Palette.Troll);
            else
                badge.color = color;

            string key = note.IsSuperchat ? "SPACE" : Palette.KeyFor(note.Kind);
            var keyT = UiKit.Label(card, "Key", key, 14, color, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(keyT.rectTransform, new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0.5f), new Vector2(86, 0), new Vector2(52, 0));
            string body = note.IsSuperchat
                ? $"{note.User}  {EconomyRules.FormatWon(note.SuperchatWon)}\n{note.Text}"
                : $"{note.User}  {note.Text}";
            var msg = UiKit.Label(card, "Msg", body, note.IsSuperchat ? 16 : 17, Palette.Pastel, TextAnchor.MiddleLeft);
            UiKit.Layout(msg.rectTransform, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 0.5f), new Vector2(142, 0), new Vector2(-150, 0));
            msg.horizontalOverflow = HorizontalWrapMode.Wrap;
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
        }

        static AudioClip Beep(float freq, float dur)
        {
            int samples = Mathf.CeilToInt(44100 * dur);
            var clip = AudioClip.Create("beep", samples, 1, 44100, false);
            var data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float env = 1f - i / (float)samples;
                data[i] = Mathf.Sin(2f * Mathf.PI * freq * i / 44100f) * 0.22f * env;
            }
            clip.SetData(data, 0);
            return clip;
        }
    }
}
