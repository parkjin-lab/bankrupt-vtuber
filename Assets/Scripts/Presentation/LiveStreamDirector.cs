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
        Text _income;
        Text _mental;
        Text _timer;
        Text _combo;
        Text _judge;
        Text _stub;
        Image _tensionFill;
        Image _hypeFlash;
        AudioSource _audio;
        AudioClip _ok;
        AudioClip _bad;
        AudioClip _sc;

        readonly Dictionary<ChatNote, RectTransform> _views = new Dictionary<ChatNote, RectTransform>();
        float _judgeFlash;
        bool _ending;

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
                EconomyRules.ApplyDailyBills(gm.Run, gm.Balance);
            _session = new StreamSession(gm.Balance, gm.Catalog, gm.Run.mental);
        }

        void Update()
        {
            if (_session == null || _ending)
                return;

            float dt = Time.deltaTime;
            _session.Tick(dt);

            if (StreamBindings.TryConsumeKind(out var kind, out var hold))
                _session.TryHit(kind, _session.Elapsed, hold);

            if (StreamBindings.EventStubPressed(out int idx))
            {
                _stub.text = $"이벤트 QTE {idx} · 2주차 예정";
                _stub.color = new Color(1, 1, 1, 1);
            }

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

            _viewers = Chip(top, "Viewers", "시청자", new Vector2(40, -16));
            _income = Chip(top, "Income", "실시간 수익", new Vector2(360, -16));
            _mental = Chip(top, "Mental", "멘탈", new Vector2(760, -16));
            _timer = Chip(top, "Timer", "남은 시간", new Vector2(1100, -16));

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

            var keys = UiKit.Label(bottom, "Keys", "A 긍정(파랑)   S 공감(초록)   D 웃음(빨강)   F 감사(골드)   Space 슈퍼챗 홀드", 18, Palette.PastelDim, TextAnchor.MiddleRight);
            UiKit.Layout(keys.rectTransform, new Vector2(0.38f, 0), new Vector2(1, 1), new Vector2(1, 0.5f), new Vector2(-24, 8), Vector2.zero);

            _judge = UiKit.Label(root, "Judge", "", 64, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(_judge.rectTransform, new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.5f), new Vector2(-80, 0), new Vector2(520, 80));

            _stub = UiKit.Label(root, "Stub", "", 22, Palette.Gold, TextAnchor.MiddleCenter);
            UiKit.Layout(_stub.rectTransform, new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.5f), new Vector2(-80, 0), new Vector2(520, 36));
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
            int shown = _session.ForceEnded ? _session.PayoutIncome : _session.LiveIncome;
            _income.text = EconomyRules.FormatWon(shown);
            _income.color = Palette.CashGreen;
            _mental.text = $"{_session.Mental}/{_session.Balance.maxMental}";
            _mental.color = _session.Mental <= 24 ? Palette.MoneyRed : Palette.Pink;
            _timer.text = $"{Mathf.CeilToInt(_session.TimeLeft)}s";
            if (_session.HypeActive)
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
            string key = note.IsSuperchat ? "SPACE" : Palette.KeyFor(note.Kind);
            var keyT = UiKit.Label(card, "Key", key, 16, color, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(keyT.rectTransform, new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0.5f), new Vector2(48, 0), new Vector2(70, 0));
            string body = note.IsSuperchat
                ? $"{note.User}  {EconomyRules.FormatWon(note.SuperchatWon)}\n{note.Text}"
                : $"{note.User}  {note.Text}";
            var msg = UiKit.Label(card, "Msg", body, note.IsSuperchat ? 16 : 17, Palette.Pastel, TextAnchor.MiddleLeft);
            UiKit.Layout(msg.rectTransform, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 0.5f), new Vector2(120, 0), new Vector2(-130, 0));
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
