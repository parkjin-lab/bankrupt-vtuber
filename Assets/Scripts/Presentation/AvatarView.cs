using UnityEngine;
using UnityEngine.UI;

namespace BankruptVtuber
{
    public class AvatarView
    {
        readonly RectTransform _root;
        readonly RectTransform _body;
        readonly Image _bezel;
        readonly Image _frame;
        readonly Image _window;
        readonly Image _bust;
        readonly Image _blush;
        readonly Image _flash;
        readonly Image[] _sparkles;
        readonly Text _liveViewers;
        readonly Image _liveDot;
        readonly Color _idleTint = Color.white;
        public RectTransform Root => _root;
        Image _karaokeFill;
        Text _camTag;
        bool _songGlow;
        bool _closeCam;
        float _bob;
        float _pop;
        float _shake;
        float _happy;
        float _hurt;
        float _spark;
        float _panic;
        float _shownViewers;
        bool _hypeOn;
        float _tired;
        float _punch;
        float _nod;
        float _baseScale = 1f;

        public AvatarView(RectTransform parent)
        {
            _root = UiKit.Panel(parent, "Avatar", new Color(0, 0, 0, 0));
            UiKit.Layout(_root, new Vector2(0.20f, 0.22f), new Vector2(0.20f, 0.22f), new Vector2(0.5f, 0f), new Vector2(8, 8), new Vector2(420, 560));

            _bezel = UiKit.Image(_root, "Bezel", Palette.Ink);
            UiKit.Stretch(_bezel.rectTransform, 0, 0, 0, 0);
            _bezel.color = new Color(0.08f, 0.05f, 0.1f, 0.96f);

            _frame = UiKit.Image(_root, "Frame", Palette.PinkDeep);
            UiKit.Stretch(_frame.rectTransform, 8, 8, 36, 10);
            _frame.color = new Color(0.92f, 0.28f, 0.48f, 0.95f);

            _window = UiKit.Image(_root, "Window", Palette.Hex("1C1228"));
            UiKit.Stretch(_window.rectTransform, 16, 16, 44, 18);
            _window.color = new Color(0.12f, 0.07f, 0.16f, 0.98f);

            _body = UiKit.Panel(_root, "BodyRoot", new Color(0, 0, 0, 0));
            UiKit.Stretch(_body, 28, 28, 56, 26);

            _bust = UiKit.Image(_body, "Bust", Color.white);
            UiKit.Stretch(_bust.rectTransform, 4, 4, 4, 4);
            ArtSprites.Apply(_bust, ArtSprites.Avatar, _idleTint, Color.white);

            _blush = UiKit.Image(_body, "Blush", new Color(1f, 0.45f, 0.6f, 0f));
            UiKit.Layout(_blush.rectTransform, new Vector2(0.5f, 0.36f), new Vector2(0.5f, 0.36f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(180, 36));

            _flash = UiKit.Image(_root, "HurtFlash", new Color(1f, 0.2f, 0.28f, 0f));
            UiKit.Stretch(_flash.rectTransform, 16, 16, 44, 18);

            _sparkles = new Image[5];
            for (int i = 0; i < _sparkles.Length; i++)
            {
                var sp = UiKit.Image(_body, "Spark" + i, Color.white);
                UiKit.Layout(sp.rectTransform, new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(28, 28));
                ArtSprites.Apply(sp, ArtSprites.Sparkle, Palette.Gold, Color.white);
                sp.color = new Color(1f, 1f, 1f, 0f);
                _sparkles[i] = sp;
            }

            var live = UiKit.Panel(_root, "LiveTag", new Color(0.86f, 0.12f, 0.22f, 0.96f));
            UiKit.Layout(live, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(18, -8), new Vector2(168, 32));
            _liveDot = UiKit.Image(live, "Dot", Color.white);
            UiKit.Layout(_liveDot.rectTransform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(16, 0), new Vector2(10, 10));
            var liveL = UiKit.Label(live, "L", "LIVE", 16, Color.white, TextAnchor.MiddleLeft, FontStyle.Bold);
            UiKit.Layout(liveL.rectTransform, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 0.5f), new Vector2(28, 0), new Vector2(-36, 0));

            _liveViewers = UiKit.Label(_root, "CamViewers", "시청자 12", 16, Palette.Pastel, TextAnchor.MiddleRight, FontStyle.Bold);
            UiKit.Layout(_liveViewers.rectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(-16, -10), new Vector2(180, 28));

            var name = UiKit.Label(_root, "Name", "파산냥", 22, Palette.Pastel, TextAnchor.UpperCenter, FontStyle.Bold);
            UiKit.Layout(name.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -6), new Vector2(200, 28));
        }

        public void ApplyShow(ContentShowLook look)
        {
            _frame.color = look.CamFrame;
            _window.color = look.CamWindow;
            _bezel.color = Color.Lerp(Palette.Ink, look.Wash, 0.55f);
            _songGlow = look.GoldSparkle;
            _closeCam = look.Type == StreamContentType.Talk;

            if (look.Type == StreamContentType.Talk)
            {
                _root.localScale = Vector3.one * 1.16f;
                _root.anchoredPosition = new Vector2(28f, -12f);
                UiKit.Stretch(_body, 8, 8, 36, 10);
                TagCam("클로즈업", look.CamFrame);
            }
            else if (look.Type == StreamContentType.Game)
            {
                _root.localScale = Vector3.one;
                BuildGameBezel();
                TagCam("게임 화면", Palette.CashGreen);
            }
            else if (look.Type == StreamContentType.Song)
            {
                _root.localScale = Vector3.one;
                BuildKaraokeBar();
                TagCam("노래방", Palette.Gold);
            }
            else if (look.Type == StreamContentType.Reaction)
            {
                _root.localScale = Vector3.one * 0.92f;
                _root.anchoredPosition = new Vector2(-10f, 8f);
                BuildReactionFrame();
                TagCam("리액션 캠", Palette.PastelDim);
            }
            _baseScale = _root.localScale.x;
        }

        void TagCam(string copy, Color color)
        {
            var bg = _root.Find("CamTagBg") as RectTransform;
            if (bg == null)
            {
                var img = UiKit.Image(_root, "CamTagBg", color);
                UiKit.Layout(img.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 10), new Vector2(168, 26));
                bg = img.rectTransform;
            }
            else
                bg.GetComponent<Image>().color = color;

            if (_camTag == null)
            {
                _camTag = UiKit.Label(_root, "CamTag", copy, 16, Palette.Ink, TextAnchor.MiddleCenter, FontStyle.Bold);
                UiKit.Layout(_camTag.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 10), new Vector2(160, 24));
            }
            _camTag.text = copy;
            _camTag.color = lookInkFor(color);
            _camTag.rectTransform.SetAsLastSibling();
        }

        static Color lookInkFor(Color bg) =>
            bg.grayscale > 0.55f ? Palette.Ink : Palette.Pastel;

        void BuildGameBezel()
        {
            var bar = UiKit.Panel(_root, "GameTitle", new Color(0.08f, 0.1f, 0.08f, 0.98f));
            UiKit.Layout(bar, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, 0), new Vector2(0, 34));
            UiKit.Label(bar, "G", "게임 화면", 16, Palette.CashGreen, TextAnchor.MiddleLeft, FontStyle.Bold);
            var gl = bar.Find("G") as RectTransform;
            UiKit.Layout(gl, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 0.5f), new Vector2(16, 0), new Vector2(-80, 0));
            for (int i = 0; i < 3; i++)
            {
                var c = i == 0 ? Palette.Troll : i == 1 ? Palette.Gold : Palette.CashGreen;
                var dot = UiKit.Image(bar, "W" + i, c);
                UiKit.Layout(dot.rectTransform, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-14 - i * 18, 0), new Vector2(10, 10));
            }
            UiKit.Stretch(_frame.rectTransform, 6, 6, 34, 8);
            UiKit.Stretch(_window.rectTransform, 12, 12, 40, 14);
        }

        void BuildKaraokeBar()
        {
            var bar = UiKit.Panel(_root, "Karaoke", new Color(0.12f, 0.08f, 0.02f, 0.94f));
            UiKit.Layout(bar, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), new Vector2(0, 36), new Vector2(-20, 22));
            var bg = UiKit.Image(bar, "KBg", new Color(1f, 0.82f, 0.25f, 0.22f));
            UiKit.Stretch(bg.rectTransform, 8, 8, 4, 4);
            _karaokeFill = UiKit.Image(bar, "KFill", Palette.Gold);
            UiKit.Layout(_karaokeFill.rectTransform, new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0.5f), new Vector2(8, 0), new Vector2(0, -8));
            var lyric = UiKit.Label(bar, "Lyric", "♪ 따라 부르기", 14, Palette.Ink, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Stretch(lyric.rectTransform);
        }

        void BuildReactionFrame()
        {
            var outer = UiKit.Image(_root, "ReactOuter", new Color(0.22f, 0.24f, 0.22f, 0.9f));
            UiKit.Stretch(outer.rectTransform, -10, -10, -8, -10);
            outer.transform.SetAsFirstSibling();
            var pip = UiKit.Panel(_root, "ReactPip", new Color(0.08f, 0.09f, 0.08f, 0.95f));
            UiKit.Layout(pip, new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0), new Vector2(-18, 40), new Vector2(120, 78));
            UiKit.Label(pip, "P", "보고 있는 영상", 12, Palette.PastelDim, TextAnchor.MiddleCenter, FontStyle.Bold);
            var pl = pip.Find("P") as RectTransform;
            UiKit.Stretch(pl, 4, 4, 4, 4);
        }

        public void React(Judgement j, bool superchat)
        {
            if (j == Judgement.Perfect)
            {
                _punch = 0.12f;
                _pop = 1f;
                _happy = 1f;
            }
            else if (j == Judgement.Great)
            {
                _pop = 1f;
                _happy = 0.7f;
            }
            else if (j == Judgement.Good)
            {
                _nod = 0.16f;
                _happy = 0.4f;
            }
            else if (j == Judgement.Miss)
            {
                _shake = 1f;
                _hurt = 1f;
            }

            if (superchat && j != Judgement.Miss)
            {
                _spark = 1f;
                _happy = 1f;
            }
        }

        public void Panic()
        {
            _panic = 1f;
            _shake = 1f;
            _hurt = 1f;
            _happy = 0f;
            _pop = 0f;
        }

        public void SetViewers(float viewers)
        {
            _shownViewers = viewers;
        }

        public void HappyPop()
        {
            _pop = 1f;
            _happy = 1f;
        }

        public void SetHype(bool on)
        {
            _hypeOn = on;
            if (on)
            {
                _happy = 1f;
                _spark = 1f;
            }
        }

        public void SetTired(bool tired, bool danger)
        {
            _tired = !tired ? 0f : danger ? 1f : 0.55f;
        }

        public void Tick(float dt)
        {
            _bob += dt;
            _pop = Mathf.MoveTowards(_pop, 0f, dt * 3.6f);
            _punch = Mathf.MoveTowards(_punch, 0f, dt);
            _nod = Mathf.MoveTowards(_nod, 0f, dt);
            _shake = Mathf.MoveTowards(_shake, 0f, dt * 3.2f);
            _happy = Mathf.MoveTowards(_happy, 0f, dt * 1.8f);
            _hurt = Mathf.MoveTowards(_hurt, 0f, dt * 2.4f);
            _spark = Mathf.MoveTowards(_spark, _hypeOn ? 0.92f : 0f, dt * 1.5f);
            _panic = Mathf.MoveTowards(_panic, 0f, dt * 1.15f);
            if (_hypeOn)
                _happy = Mathf.Max(_happy, 0.88f);

            float bobAmp = (_closeCam ? 4f : 7f) * (1f - 0.7f * _tired);
            float bobY = Mathf.Sin(_bob * (_closeCam ? 1.6f : 2.1f) * (1f - 0.45f * _tired)) * bobAmp;
            float popY = _pop * 22f;
            float x = Mathf.Sin(Time.time * 52f) * _shake * 12f;
            if (_panic > 0.01f)
                x += Mathf.Sin(Time.time * 78f) * _panic * 16f;
            if (_tired > 0.01f)
                x += Mathf.Sin(Time.time * 1.3f) * _tired * 4f;
            float nodU = _nod / 0.16f;
            float nodY = -12f * nodU;
            _body.anchoredPosition = new Vector2(x, bobY + popY + nodY - _panic * 18f - _tired * 22f);
            _body.localEulerAngles = new Vector3(0f, 0f, -7f * nodU);
            float punchU = _punch / 0.12f;
            _root.localScale = Vector3.one * (_baseScale * (1f + 0.08f * punchU));
            float squash = _pop * 0.22f;
            if (_panic > 0.01f)
                _body.localScale = new Vector3(1.08f + 0.06f * _panic, 0.82f - 0.08f * _panic, 1f);
            else
                _body.localScale = new Vector3(1f + squash + 0.06f * _tired, 1f - squash * 0.55f - 0.10f * _tired, 1f);

            var tint = _idleTint;
            if (_hurt > 0.01f)
                tint = Color.Lerp(tint, new Color(1f, 0.55f, 0.58f, 1f), _hurt);
            if (_happy > 0.01f)
                tint = Color.Lerp(tint, new Color(1f, 0.94f, 0.72f, 1f), _happy * (1f - 0.35f * _tired));
            if (_tired > 0.01f)
                tint = Color.Lerp(tint, new Color(0.58f, 0.54f, 0.60f, 1f), _tired);
            if (_panic > 0.01f)
                tint = Color.Lerp(tint, new Color(1f, 0.38f, 0.42f, 1f), _panic);
            _bust.color = tint;
            _blush.color = new Color(1f, 0.45f, 0.62f, _happy * 0.28f);
            if (punchU > 0.001f)
                _flash.color = new Color(1f, 0.96f, 0.88f, 0.55f * punchU);
            else
                _flash.color = new Color(1f, 0.12f, 0.22f, Mathf.Max(_hurt * 0.28f, _panic * 0.5f));
            _liveDot.color = new Color(1f, 1f, 1f, 0.45f + 0.55f * Mathf.Abs(Mathf.Sin(Time.time * 6f)));
            _liveViewers.text = $"시청자 {Mathf.RoundToInt(_shownViewers)}";

            for (int i = 0; i < _sparkles.Length; i++)
            {
                float u = Mathf.Repeat(_spark + i * 0.17f, 1f);
                float a = _spark * (1f - u);
                if (_songGlow)
                    a = Mathf.Max(a, 0.18f + 0.10f * Mathf.Abs(Mathf.Sin(Time.time * 3f + i)));
                float ang = i * 1.256f;
                _sparkles[i].rectTransform.anchoredPosition = new Vector2(Mathf.Cos(ang) * (40f + u * 50f), 30f + u * 70f);
                _sparkles[i].color = new Color(1f, 0.92f, 0.45f, a);
                _sparkles[i].rectTransform.localScale = Vector3.one * (0.7f + u * 0.6f);
            }

            if (_karaokeFill != null)
            {
                float u = 0.18f + 0.72f * (0.5f + 0.5f * Mathf.Sin(Time.time * 1.4f));
                _karaokeFill.rectTransform.anchorMax = new Vector2(u, 1f);
            }
        }
    }
}
