using UnityEngine;
using UnityEngine.UI;

namespace BankruptVtuber
{
    public class AvatarView
    {
        readonly RectTransform _root;
        readonly RectTransform _body;
        readonly Image _face;
        readonly Image _blushL;
        readonly Image _blushR;
        readonly Image _mouth;
        readonly Image _eyeL;
        readonly Image _eyeR;
        float _bounce;
        float _shake;
        float _blush;

        public AvatarView(RectTransform parent)
        {
            _root = UiKit.Panel(parent, "Avatar", new Color(0, 0, 0, 0));
            UiKit.Layout(_root, new Vector2(0.22f, 0.18f), new Vector2(0.22f, 0.18f), new Vector2(0.5f, 0f), new Vector2(0, 40), new Vector2(360, 520));

            var window = UiKit.Image(_root, "Window", Palette.Hex("1C1228"));
            UiKit.Stretch(window.rectTransform, 10, 10, 30, 10);
            window.color = new Color(0.11f, 0.07f, 0.16f, 0.92f);

            var frame = UiKit.Image(_root, "Frame", Palette.Pink);
            UiKit.Stretch(frame.rectTransform, 4, 4, 24, 4);
            frame.color = new Color(1f, 0.56f, 0.78f, 0.28f);

            _body = UiKit.Panel(_root, "BodyRoot", new Color(0, 0, 0, 0));
            UiKit.Stretch(_body, 40, 40, 70, 30);

            var torso = UiKit.Image(_body, "Torso", Palette.Hex("B8E0FF"));
            UiKit.Layout(torso.rectTransform, new Vector2(0.5f, 0.12f), new Vector2(0.5f, 0.12f), new Vector2(0.5f, 0f), Vector2.zero, new Vector2(150, 160));

            var bow = UiKit.Image(_body, "Bow", Palette.PinkDeep);
            UiKit.Layout(bow.rectTransform, new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(36, 22));

            var earL = UiKit.Image(_body, "EarL", Palette.Hex("FFD4E6"));
            UiKit.Layout(earL.rectTransform, new Vector2(0.32f, 0.88f), new Vector2(0.32f, 0.88f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(44, 56));
            earL.rectTransform.localRotation = Quaternion.Euler(0, 0, 18);
            var earR = UiKit.Image(_body, "EarR", Palette.Hex("FFD4E6"));
            UiKit.Layout(earR.rectTransform, new Vector2(0.68f, 0.88f), new Vector2(0.68f, 0.88f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(44, 56));
            earR.rectTransform.localRotation = Quaternion.Euler(0, 0, -18);

            _face = UiKit.Image(_body, "Face", Palette.Hex("FFE4F0"));
            UiKit.Layout(_face.rectTransform, new Vector2(0.5f, 0.62f), new Vector2(0.5f, 0.62f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(188, 188));

            _eyeL = UiKit.Image(_face.rectTransform, "EyeL", Palette.Ink);
            UiKit.Layout(_eyeL.rectTransform, new Vector2(0.32f, 0.55f), new Vector2(0.32f, 0.55f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(22, 34));
            _eyeR = UiKit.Image(_face.rectTransform, "EyeR", Palette.Ink);
            UiKit.Layout(_eyeR.rectTransform, new Vector2(0.68f, 0.55f), new Vector2(0.68f, 0.55f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(22, 34));
            var shineL = UiKit.Image(_eyeL.rectTransform, "Shine", Color.white);
            UiKit.Layout(shineL.rectTransform, new Vector2(0.65f, 0.72f), new Vector2(0.65f, 0.72f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(8, 8));
            var shineR = UiKit.Image(_eyeR.rectTransform, "Shine", Color.white);
            UiKit.Layout(shineR.rectTransform, new Vector2(0.65f, 0.72f), new Vector2(0.65f, 0.72f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(8, 8));

            _blushL = UiKit.Image(_face.rectTransform, "BlushL", new Color(1f, 0.45f, 0.6f, 0f));
            UiKit.Layout(_blushL.rectTransform, new Vector2(0.22f, 0.36f), new Vector2(0.22f, 0.36f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(28, 12));
            _blushR = UiKit.Image(_face.rectTransform, "BlushR", new Color(1f, 0.45f, 0.6f, 0f));
            UiKit.Layout(_blushR.rectTransform, new Vector2(0.78f, 0.36f), new Vector2(0.78f, 0.36f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(28, 12));

            _mouth = UiKit.Image(_face.rectTransform, "Mouth", Palette.PinkDeep);
            UiKit.Layout(_mouth.rectTransform, new Vector2(0.5f, 0.24f), new Vector2(0.5f, 0.24f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(28, 10));

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
                _blush = 1f;
        }

        public void Tick(float dt)
        {
            _bounce = Mathf.MoveTowards(_bounce, 0f, dt * 2.4f);
            _shake = Mathf.MoveTowards(_shake, 0f, dt * 3.2f);
            _blush = Mathf.MoveTowards(_blush, 0f, dt * 0.55f);
            float y = Mathf.Sin(Time.time * 2.1f) * 6f + _bounce * 18f;
            float x = Mathf.Sin(Time.time * 47f) * _shake * 10f;
            _body.anchoredPosition = new Vector2(x, y);
            var blush = new Color(1f, 0.45f, 0.6f, _blush * 0.55f);
            _blushL.color = blush;
            _blushR.color = blush;
            _mouth.rectTransform.sizeDelta = new Vector2(28 + _bounce * 10f, 10 + _bounce * 6f);
        }
    }
}
