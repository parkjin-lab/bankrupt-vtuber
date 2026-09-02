using UnityEngine;
using UnityEngine.UI;

namespace BankruptVtuber
{
    /// <summary>
    /// Shared Title / WeekStart / Settlement chrome so the day loop
    /// matches LiveStream (webcam, loud KRW, sliced cards).
    /// </summary>
    public class StudioPortrait
    {
        public readonly RectTransform Root;
        public readonly RectTransform Body;
        public readonly Image Bust;
        public readonly Image Flash;
        public readonly Image[] Sparkles;
        readonly Color _idle = Color.white;
        float _bob;
        float _happy;
        float _hurt;
        float _spark;
        float _slump;
        Color _tint;

        public StudioPortrait(Transform parent, Vector2 anchor, Vector2 size, bool liveTag)
        {
            Root = UiKit.Panel(parent, "Portrait", new Color(0, 0, 0, 0));
            UiKit.Layout(Root, anchor, anchor, new Vector2(0.5f, 0.5f), Vector2.zero, size);

            var bezel = UiKit.Image(Root, "Bezel", new Color(0.08f, 0.05f, 0.1f, 0.96f));
            UiKit.Stretch(bezel.rectTransform);
            var frame = UiKit.Image(Root, "Frame", new Color(0.92f, 0.28f, 0.48f, 0.95f));
            UiKit.Stretch(frame.rectTransform, 8, 8, 36, 10);
            var window = UiKit.Image(Root, "Window", new Color(0.12f, 0.07f, 0.16f, 0.98f));
            UiKit.Stretch(window.rectTransform, 16, 16, 44, 18);

            Body = UiKit.Panel(Root, "BodyRoot", new Color(0, 0, 0, 0));
            UiKit.Stretch(Body, 28, 28, 56, 26);
            Bust = UiKit.Image(Body, "Bust", Color.white);
            UiKit.Stretch(Bust.rectTransform, 4, 4, 4, 4);
            ArtSprites.Apply(Bust, ArtSprites.Avatar, _idle, Color.white);
            _tint = _idle;

            Flash = UiKit.Image(Root, "Flash", new Color(1f, 0.2f, 0.28f, 0f));
            UiKit.Stretch(Flash.rectTransform, 16, 16, 44, 18);

            Sparkles = new Image[4];
            for (int i = 0; i < Sparkles.Length; i++)
            {
                var sp = UiKit.Image(Body, "Spark" + i, Color.white);
                UiKit.Layout(sp.rectTransform, new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(26, 26));
                ArtSprites.Apply(sp, ArtSprites.Sparkle, Palette.Gold, Color.white);
                sp.color = new Color(1, 1, 1, 0);
                Sparkles[i] = sp;
            }

            if (liveTag)
            {
                var live = UiKit.Panel(Root, "LiveTag", new Color(0.86f, 0.12f, 0.22f, 0.96f));
                UiKit.Layout(live, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(16, -8), new Vector2(132, 28));
                var liveL = UiKit.Label(live, "L", "LIVE", 15, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold);
                UiKit.Stretch(liveL.rectTransform);
            }

            var name = UiKit.Label(Root, "Name", "파산냥", 20, Palette.Pastel, TextAnchor.UpperCenter, FontStyle.Bold);
            UiKit.Layout(name.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -6), new Vector2(180, 26));
        }

        public void PoseEnding(EndingKind kind)
        {
            _happy = _hurt = _spark = _slump = 0f;
            switch (kind)
            {
                case EndingKind.SoloLegend:
                    _happy = 1f;
                    _spark = 1f;
                    _tint = new Color(1f, 0.94f, 0.72f, 1f);
                    break;
                case EndingKind.AgencyEmpire:
                    _happy = 0.85f;
                    _spark = 1f;
                    _tint = new Color(1f, 0.90f, 0.55f, 1f);
                    break;
                case EndingKind.RetireProducer:
                    _happy = 0.4f;
                    _tint = new Color(0.92f, 0.82f, 0.88f, 1f);
                    break;
                case EndingKind.Bankrupt:
                    _hurt = 1f;
                    _tint = new Color(1f, 0.55f, 0.58f, 1f);
                    break;
                case EndingKind.Burnout:
                    _hurt = 0.7f;
                    _slump = 1f;
                    _tint = new Color(0.62f, 0.55f, 0.58f, 1f);
                    break;
                default:
                    _tint = _idle;
                    break;
            }
        }

        public void Tick(float dt)
        {
            _bob += dt;
            float bobY = Mathf.Sin(_bob * 2.1f) * 6f - _slump * 18f;
            Body.anchoredPosition = new Vector2(Mathf.Sin(Time.time * 40f) * _hurt * 8f, bobY);
            float squash = _happy * 0.08f - _slump * 0.08f;
            Body.localScale = new Vector3(1f + squash, 1f - squash * 0.45f - _slump * 0.06f, 1f);
            Bust.color = _tint;
            Flash.color = new Color(1f, 0.18f, 0.28f, _hurt * 0.26f);
            for (int i = 0; i < Sparkles.Length; i++)
            {
                float u = Mathf.Repeat(_bob * 0.35f + i * 0.2f, 1f);
                float a = _spark * (0.35f + 0.65f * Mathf.Abs(Mathf.Sin(_bob * 3f + i)));
                float ang = i * 1.57f;
                Sparkles[i].rectTransform.anchoredPosition = new Vector2(Mathf.Cos(ang) * (36f + u * 28f), 24f + u * 50f);
                Sparkles[i].color = new Color(1f, 0.92f, 0.45f, a);
            }
        }
    }

    public static class StudioChrome
    {
        public static void Wash(Transform root)
        {
            var wash = UiKit.Image(root, "Wash", Palette.Studio);
            UiKit.Stretch(wash.rectTransform);
            var glow = UiKit.Image(root, "Glow", new Color(0.91f, 0.22f, 0.38f, 0.18f));
            UiKit.Layout(glow.rectTransform, new Vector2(0.78f, 0.42f), new Vector2(0.78f, 0.42f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(640, 640));
        }

        public static void StyleCard(Image image, Color color, bool raycast = false)
        {
            ArtSprites.ApplySliced(image, ArtSprites.PanelDark, color, new Vector4(40f, 36f, 40f, 36f));
            image.raycastTarget = raycast;
        }

        public static Text RecapTile(Transform parent, string name, string label, Color accent, float x0, float x1, float y, float h, bool green)
        {
            var box = UiKit.Panel(parent, name, Color.white);
            box.anchorMin = new Vector2(x0, y);
            box.anchorMax = new Vector2(x1, y + h);
            box.pivot = new Vector2(0.5f, 0.5f);
            box.offsetMin = new Vector2(8f, 6f);
            box.offsetMax = new Vector2(-8f, -6f);
            var img = box.GetComponent<Image>();
            ArtSprites.ApplySliced(
                img,
                green ? ArtSprites.CashBanner : ArtSprites.ThreatBanner,
                accent,
                new Vector4(28f, 24f, 28f, 24f));
            UiKit.Label(box, "L", label, 16, Color.white, TextAnchor.UpperLeft, FontStyle.Bold);
            var l = box.Find("L") as RectTransform;
            UiKit.Layout(l, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1), new Vector2(16, -10), new Vector2(-24, 22));
            var v = UiKit.Label(box, "V", "—", 30, Color.white, TextAnchor.MiddleLeft, FontStyle.Bold);
            UiKit.Layout(v.rectTransform, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 0), new Vector2(16, 8), new Vector2(-20, -28));
            return v;
        }

        public static RectTransform FanChip(Transform parent, string name, string title, string sub, float x)
        {
            var box = UiKit.Panel(parent, name, Color.white);
            UiKit.Layout(box, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(x, 0), new Vector2(240, 52));
            ArtSprites.ApplySliced(box.GetComponent<Image>(), ArtSprites.BubblePill, Palette.Gold);
            var t = UiKit.Label(box, "T", title, 18, Palette.Ink, TextAnchor.MiddleLeft, FontStyle.Bold);
            UiKit.Layout(t.rectTransform, new Vector2(0, 0.42f), new Vector2(1, 1), new Vector2(0, 1), new Vector2(16, -2), new Vector2(-20, 0));
            var s = UiKit.Label(box, "S", sub, 13, Palette.Ink, TextAnchor.MiddleLeft);
            UiKit.Layout(s.rectTransform, new Vector2(0, 0), new Vector2(1, 0.48f), new Vector2(0, 0), new Vector2(16, 4), new Vector2(-20, 0));
            return box;
        }
    }
}
