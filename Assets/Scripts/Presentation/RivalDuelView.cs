using UnityEngine;
using UnityEngine.UI;

namespace BankruptVtuber
{
    /// <summary>
    /// Week 3 rival-match chrome. Presentation only — steal and payout stay on
    /// StreamSession / Week3Rules. Hidden when RivalActive is false.
    /// </summary>
    public sealed class RivalDuelView
    {
        readonly RectTransform _root;
        readonly RectTransform _playerFill;
        readonly RectTransform _rivalFill;
        readonly Text _playerCount;
        readonly Text _rivalCount;
        readonly Text _camCount;
        readonly Image _onAir;
        readonly Text _stealFlash;
        readonly Image _stealBg;
        readonly GameObject _resultRoot;
        readonly Image _resultPanel;
        readonly Text _resultTitle;
        readonly Text _resultSub;
        StreamSession _session;
        float _stealUntil;
        float _resultUntil;

        public RivalDuelView(RectTransform parent)
        {
            _root = UiKit.Panel(parent, "RivalDuelRoot", new Color(0, 0, 0, 0));
            UiKit.Stretch(_root);
            var rootImg = _root.GetComponent<Image>();
            if (rootImg != null)
                rootImg.raycastTarget = false;
            _root.gameObject.SetActive(false);

            var cam = UiKit.Panel(_root, "RivalCam", new Color(0, 0, 0, 0));
            UiKit.Layout(cam, new Vector2(0.52f, 0.24f), new Vector2(0.52f, 0.24f), new Vector2(0.5f, 0f), new Vector2(0f, 16f), new Vector2(280f, 380f));

            var bezel = UiKit.Image(cam, "Bezel", Color.white);
            UiKit.Stretch(bezel.rectTransform, 0, 0, 0, 0);
            ArtSprites.ApplySliced(bezel, ArtSprites.WebcamBezel, new Color(1f, 0.90f, 0.92f, 1f), new Vector4(56f, 64f, 56f, 64f));
            bezel.raycastTarget = false;

            var frame = UiKit.Image(cam, "RivalFrame", new Color(0.78f, 0.22f, 0.34f, 0.55f));
            UiKit.Stretch(frame.rectTransform, 8, 8, 36, 10);

            var window = UiKit.Image(cam, "RivalWindow", new Color(0.12f, 0.06f, 0.1f, 0.98f));
            UiKit.Stretch(window.rectTransform, 16, 16, 44, 18);

            var bust = UiKit.Image(cam, "RivalBust", Color.white);
            UiKit.Stretch(bust.rectTransform, 28, 28, 56, 26);
            ArtSprites.Apply(bust, ArtSprites.RivalAvatar, new Color(0.72f, 0.42f, 0.5f, 1f), Color.white);
            bust.preserveAspect = true;

            _onAir = UiKit.Image(cam, "RivalOnAir", Color.white);
            UiKit.Layout(_onAir.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(10f, 10f), new Vector2(120f, 30f));
            ArtSprites.Apply(_onAir, ArtSprites.OnAirLed, Color.white, Color.white);
            _onAir.preserveAspect = false;
            _onAir.raycastTarget = false;
            var onAirCopy = UiKit.Label(_onAir.transform, "T", "ON AIR", 14, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Stretch(onAirCopy.rectTransform, 14f, 14f, 4f, 4f);

            var tag = UiKit.Panel(cam, "RivalTag", new Color(0.86f, 0.12f, 0.22f, 0.96f));
            UiKit.Layout(tag, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(14, -44), new Vector2(108, 30));
            var tagL = UiKit.Label(tag, "L", "라이벌", 16, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Stretch(tagL.rectTransform);

            _camCount = UiKit.Label(cam, "RivalCamCount", "25", 18, Palette.Pastel, TextAnchor.MiddleRight, FontStyle.Bold);
            UiKit.Layout(_camCount.rectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(-12, -10), new Vector2(140, 28));

            var bars = UiKit.Panel(_root, "RivalBars", new Color(0.06f, 0.04f, 0.08f, 0.88f));
            UiKit.Layout(bars, new Vector2(0.08f, 1f), new Vector2(0.58f, 1f), new Vector2(0f, 1f), new Vector2(8f, -268f), new Vector2(-8f, 68f));

            var youL = UiKit.Label(bars, "YouBarLabel", "나", 14, Palette.Blue, TextAnchor.MiddleLeft, FontStyle.Bold);
            UiKit.Layout(youL.rectTransform, new Vector2(0f, 0.55f), new Vector2(0.14f, 1f), new Vector2(0f, 0.5f), new Vector2(8f, 0f), new Vector2(-4f, 0f));
            _playerCount = UiKit.Label(bars, "YouBarCount", "0", 14, Palette.Pastel, TextAnchor.MiddleRight, FontStyle.Bold);
            UiKit.Layout(_playerCount.rectTransform, new Vector2(0.82f, 0.55f), new Vector2(1f, 1f), new Vector2(1f, 0.5f), new Vector2(-8f, 0f), new Vector2(-4f, 0f));
            _playerFill = MakeFill(bars, "YouFill", new Vector2(0.14f, 0.62f), new Vector2(0.82f, 0.90f), Palette.Blue);

            var themL = UiKit.Label(bars, "RivalBarLabel", "라이벌", 14, Palette.Troll, TextAnchor.MiddleLeft, FontStyle.Bold);
            UiKit.Layout(themL.rectTransform, new Vector2(0f, 0f), new Vector2(0.14f, 0.45f), new Vector2(0f, 0.5f), new Vector2(8f, 0f), new Vector2(-4f, 0f));
            _rivalCount = UiKit.Label(bars, "RivalBarCount", "0", 14, Palette.Pastel, TextAnchor.MiddleRight, FontStyle.Bold);
            UiKit.Layout(_rivalCount.rectTransform, new Vector2(0.82f, 0f), new Vector2(1f, 0.45f), new Vector2(1f, 0.5f), new Vector2(-8f, 0f), new Vector2(-4f, 0f));
            _rivalFill = MakeFill(bars, "RivalFill", new Vector2(0.14f, 0.10f), new Vector2(0.82f, 0.38f), Palette.Troll);

            var flash = UiKit.Panel(_root, "StealFlash", new Color(1f, 0.85f, 0.2f, 0f));
            UiKit.Layout(flash, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-80f, -344f), new Vector2(280f, 36f));
            _stealBg = flash.GetComponent<Image>();
            _stealFlash = UiKit.Label(flash, "StealFlashText", "스틸 +0.6", 20, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Stretch(_stealFlash.rectTransform);
            SetAlpha(_stealFlash, 0f);

            _resultRoot = UiKit.Panel(_root, "RivalResultSlam", new Color(0.16f, 0.12f, 0.04f, 0.96f)).gameObject;
            var resultRt = _resultRoot.GetComponent<RectTransform>();
            UiKit.Layout(resultRt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 36f), new Vector2(420f, 110f));
            _resultPanel = _resultRoot.GetComponent<Image>();
            _resultTitle = UiKit.Label(_resultRoot.transform, "RivalResultTitle", "라이벌 승", 32, Palette.Gold, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(_resultTitle.rectTransform, new Vector2(0f, 0.42f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            _resultSub = UiKit.Label(_resultRoot.transform, "RivalResultSub", "+₩20,000", 20, Palette.Pastel, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(_resultSub.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0.48f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            _resultRoot.SetActive(false);
        }

        public void Bind(StreamSession session)
        {
            _session = session;
            bool on = session != null && session.RivalActive;
            if (_root != null)
                _root.gameObject.SetActive(on);
            if (_onAir != null)
                _onAir.gameObject.SetActive(on);
            if (_resultRoot != null)
                _resultRoot.SetActive(false);
            _stealUntil = 0f;
            _resultUntil = 0f;
            SetAlpha(_stealFlash, 0f);
            if (_stealBg != null)
            {
                var c = _stealBg.color;
                c.a = 0f;
                _stealBg.color = c;
            }
            if (on)
                RefreshBars();
        }

        public void Tick(float dt)
        {
            if (_root == null || !_root.gameObject.activeSelf || _session == null || !_session.RivalActive)
                return;
            RefreshBars();

            if (_stealUntil > 0f)
            {
                _stealUntil -= dt;
                float a = Mathf.Clamp01(_stealUntil / 0.55f);
                SetAlpha(_stealFlash, a);
                if (_stealBg != null)
                {
                    var c = _stealBg.color;
                    c.a = 0.88f * a;
                    _stealBg.color = c;
                }
            }
            else
            {
                SetAlpha(_stealFlash, 0f);
                if (_stealBg != null)
                {
                    var c = _stealBg.color;
                    c.a = 0f;
                    _stealBg.color = c;
                }
            }

            if (_resultUntil > 0f)
            {
                _resultUntil -= dt;
                if (_resultUntil <= 0f && _resultRoot != null)
                    _resultRoot.SetActive(false);
            }
        }

        public void FlashSteal(bool playerStole, float perfectSteal, float missSteal)
        {
            if (_stealFlash == null || _session == null || !_session.RivalActive)
                return;
            if (playerStole)
            {
                _stealFlash.text = $"스틸 +{perfectSteal:0.#}";
                _stealFlash.color = Palette.Gold;
                if (_stealBg != null)
                    _stealBg.color = new Color(1f, 0.82f, 0.15f, 0.88f);
            }
            else
            {
                _stealFlash.text = $"라이벌 스틸 +{missSteal:0.#}";
                _stealFlash.color = new Color(1f, 0.78f, 0.82f);
                if (_stealBg != null)
                    _stealBg.color = new Color(0.72f, 0.16f, 0.22f, 0.9f);
            }
            _stealUntil = 0.7f;
            SetAlpha(_stealFlash, 1f);
        }

        public void ShowResult(bool won, int winCash, int loseMental)
        {
            if (_resultRoot == null || _session == null || !_session.RivalActive)
                return;
            if (_onAir != null)
                _onAir.gameObject.SetActive(false);
            _resultRoot.SetActive(true);
            if (won)
            {
                _resultTitle.text = "라이벌 승";
                _resultTitle.color = Palette.Gold;
                _resultSub.text = $"+{EconomyRules.FormatWon(winCash)}";
                _resultSub.color = Palette.Gold;
                _resultPanel.color = new Color(0.16f, 0.12f, 0.04f, 0.96f);
            }
            else
            {
                _resultTitle.text = "라이벌 패";
                _resultTitle.color = Palette.MoneyRed;
                _resultSub.text = $"멘탈 −{loseMental}";
                _resultSub.color = new Color(1f, 0.7f, 0.72f);
                _resultPanel.color = new Color(0.16f, 0.05f, 0.06f, 0.96f);
            }
            _resultUntil = 1.4f;
        }

        void RefreshBars()
        {
            float you = _session.Viewers;
            float them = _session.RivalViewers;
            float max = Mathf.Max(1f, Mathf.Max(you, them));
            SetFill(_playerFill, you / max);
            SetFill(_rivalFill, them / max);
            if (_playerCount != null)
                _playerCount.text = you.ToString("0");
            if (_rivalCount != null)
                _rivalCount.text = them.ToString("0");
            if (_camCount != null)
                _camCount.text = them.ToString("0");
        }

        static RectTransform MakeFill(Transform parent, string name, Vector2 min, Vector2 max, Color color)
        {
            var track = UiKit.Image(parent, name + "Track", new Color(1f, 1f, 1f, 0.1f));
            var trackRt = track.rectTransform;
            trackRt.anchorMin = min;
            trackRt.anchorMax = max;
            trackRt.offsetMin = Vector2.zero;
            trackRt.offsetMax = Vector2.zero;

            var fill = UiKit.Image(track.transform, name, color);
            var fillRt = fill.rectTransform;
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.offsetMin = Vector2.zero;
            fillRt.offsetMax = Vector2.zero;
            return fillRt;
        }

        static void SetFill(RectTransform fill, float t)
        {
            if (fill == null)
                return;
            fill.anchorMin = Vector2.zero;
            fill.anchorMax = new Vector2(Mathf.Clamp01(t), 1f);
            fill.offsetMin = Vector2.zero;
            fill.offsetMax = Vector2.zero;
        }

        static void SetAlpha(Text label, float a)
        {
            if (label == null)
                return;
            var c = label.color;
            c.a = a;
            label.color = c;
        }
    }
}
