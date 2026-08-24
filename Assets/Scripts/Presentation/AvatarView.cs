using UnityEngine;
using UnityEngine.UI;

namespace BankruptVtuber
{
    public class AvatarView
    {
        readonly RectTransform _root;
        readonly RectTransform _body;
        readonly Image _bust;
        readonly Image _blush;
        readonly Image _flash;
        readonly Image[] _sparkles;
        readonly Text _liveViewers;
        readonly Image _liveDot;
        readonly Color _idleTint = new Color(1f, 0.86f, 0.92f, 1f);
        float _bob;
        float _pop;
        float _shake;
        float _happy;
        float _hurt;
        float _spark;
        float _shownViewers;

        public AvatarView(RectTransform parent)
        {
            _root = UiKit.Panel(parent, "Avatar", new Color(0, 0, 0, 0));
            UiKit.Layout(_root, new Vector2(0.20f, 0.22f), new Vector2(0.20f, 0.22f), new Vector2(0.5f, 0f), new Vector2(8, 8), new Vector2(420, 560));

            var bezel = UiKit.Image(_root, "Bezel", Palette.Ink);
            UiKit.Stretch(bezel.rectTransform, 0, 0, 0, 0);
            bezel.color = new Color(0.08f, 0.05f, 0.1f, 0.96f);

            var frame = UiKit.Image(_root, "Frame", Palette.PinkDeep);
            UiKit.Stretch(frame.rectTransform, 8, 8, 36, 10);
            frame.color = new Color(0.92f, 0.28f, 0.48f, 0.95f);

            var window = UiKit.Image(_root, "Window", Palette.Hex("1C1228"));
            UiKit.Stretch(window.rectTransform, 16, 16, 44, 18);
            window.color = new Color(0.12f, 0.07f, 0.16f, 0.98f);

            _body = UiKit.Panel(_root, "BodyRoot", new Color(0, 0, 0, 0));
            UiKit.Stretch(_body, 28, 28, 56, 26);

            _bust = UiKit.Image(_body, "Bust", Color.white);
            UiKit.Stretch(_bust.rectTransform, 4, 4, 4, 4);
            ArtSprites.Apply(_bust, ArtSprites.Avatar, _idleTint, _idleTint);

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

        public void React(Judgement j, bool superchat)
        {
            if (j == Judgement.Perfect || j == Judgement.Great)
            {
                _pop = 1f;
                _happy = j == Judgement.Perfect ? 1f : 0.7f;
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

        public void SetViewers(float viewers)
        {
            _shownViewers = viewers;
        }

        public void Tick(float dt)
        {
            _bob += dt;
            _pop = Mathf.MoveTowards(_pop, 0f, dt * 3.6f);
            _shake = Mathf.MoveTowards(_shake, 0f, dt * 3.2f);
            _happy = Mathf.MoveTowards(_happy, 0f, dt * 1.8f);
            _hurt = Mathf.MoveTowards(_hurt, 0f, dt * 2.4f);
            _spark = Mathf.MoveTowards(_spark, 0f, dt * 1.5f);

            float bobY = Mathf.Sin(_bob * 2.1f) * 7f;
            float popY = _pop * 22f;
            float x = Mathf.Sin(Time.time * 52f) * _shake * 12f;
            _body.anchoredPosition = new Vector2(x, bobY + popY);
            float squash = _pop * 0.22f;
            _body.localScale = new Vector3(1f + squash, 1f - squash * 0.55f, 1f);

            var tint = _idleTint;
            if (_hurt > 0.01f)
                tint = Color.Lerp(tint, new Color(1f, 0.55f, 0.58f, 1f), _hurt);
            if (_happy > 0.01f)
                tint = Color.Lerp(tint, new Color(1f, 0.94f, 0.72f, 1f), _happy);
            _bust.color = tint;
            _blush.color = new Color(1f, 0.45f, 0.62f, _happy * 0.28f);
            _flash.color = new Color(1f, 0.18f, 0.28f, _hurt * 0.28f);
            _liveDot.color = new Color(1f, 1f, 1f, 0.45f + 0.55f * Mathf.Abs(Mathf.Sin(Time.time * 6f)));
            _liveViewers.text = $"시청자 {Mathf.RoundToInt(_shownViewers)}";

            for (int i = 0; i < _sparkles.Length; i++)
            {
                float u = Mathf.Repeat(_spark + i * 0.17f, 1f);
                float a = _spark * (1f - u);
                float ang = i * 1.256f;
                _sparkles[i].rectTransform.anchoredPosition = new Vector2(Mathf.Cos(ang) * (40f + u * 50f), 30f + u * 70f);
                _sparkles[i].color = new Color(1f, 0.92f, 0.45f, a);
                _sparkles[i].rectTransform.localScale = Vector3.one * (0.7f + u * 0.6f);
            }
        }
    }
}
