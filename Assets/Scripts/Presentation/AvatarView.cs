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
        float _bounce;
        float _shake;
        float _blushAmt;

        public AvatarView(RectTransform parent)
        {
            _root = UiKit.Panel(parent, "Avatar", new Color(0, 0, 0, 0));
            UiKit.Layout(_root, new Vector2(0.22f, 0.16f), new Vector2(0.22f, 0.16f), new Vector2(0.5f, 0f), new Vector2(0, 36), new Vector2(400, 540));

            var window = UiKit.Image(_root, "Window", Palette.Hex("1C1228"));
            UiKit.Stretch(window.rectTransform, 10, 10, 30, 10);
            window.color = new Color(0.11f, 0.07f, 0.16f, 0.92f);

            var frame = UiKit.Image(_root, "Frame", Palette.Pink);
            UiKit.Stretch(frame.rectTransform, 4, 4, 24, 4);
            frame.color = new Color(1f, 0.56f, 0.78f, 0.28f);

            _body = UiKit.Panel(_root, "BodyRoot", new Color(0, 0, 0, 0));
            UiKit.Stretch(_body, 24, 24, 52, 18);

            _bust = UiKit.Image(_body, "Bust", Color.white);
            UiKit.Stretch(_bust.rectTransform, 8, 8, 8, 8);
            ArtSprites.Apply(_bust, ArtSprites.Avatar, new Color(1f, 0.83f, 0.9f, 1f));

            _blush = UiKit.Image(_body, "Blush", new Color(1f, 0.45f, 0.6f, 0f));
            UiKit.Layout(_blush.rectTransform, new Vector2(0.5f, 0.38f), new Vector2(0.5f, 0.38f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(180, 36));

            UiKit.Label(_root, "Name", "파산냥 · LIVE", 22, Palette.Pastel, TextAnchor.UpperCenter, FontStyle.Bold);
            var name = _root.Find("Name") as RectTransform;
            UiKit.Layout(name, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -6), new Vector2(280, 28));
        }

        public void React(Judgement j, bool superchat)
        {
            if (j == Judgement.Perfect)
                _bounce = 1f;
            else if (j == Judgement.Miss)
                _shake = 1f;
            if (superchat && j != Judgement.Miss)
                _blushAmt = 1f;
        }

        public void Tick(float dt)
        {
            _bounce = Mathf.MoveTowards(_bounce, 0f, dt * 2.4f);
            _shake = Mathf.MoveTowards(_shake, 0f, dt * 3.2f);
            _blushAmt = Mathf.MoveTowards(_blushAmt, 0f, dt * 0.55f);
            float y = Mathf.Sin(Time.time * 2.1f) * 6f + _bounce * 18f;
            float x = Mathf.Sin(Time.time * 47f) * _shake * 10f;
            _body.anchoredPosition = new Vector2(x, y);
            _blush.color = new Color(1f, 0.45f, 0.6f, _blushAmt * 0.35f);
        }
    }
}
