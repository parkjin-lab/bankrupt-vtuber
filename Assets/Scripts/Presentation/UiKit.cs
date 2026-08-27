using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BankruptVtuber
{
    public static class Palette
    {
        public static readonly Color Studio = Hex("2A1B33");
        public static readonly Color StudioHi = Hex("3B2750");
        public static readonly Color Pastel = Hex("FFF4FA");
        public static readonly Color PastelDim = Hex("F3D6E8");
        public static readonly Color Pink = Hex("FF8EC7");
        public static readonly Color PinkDeep = Hex("E85A9C");
        public static readonly Color Ink = Hex("2B1833");
        public static readonly Color Muted = Hex("8A6A86");
        public static readonly Color MoneyRed = Hex("FF2D55");
        public static readonly Color CashGreen = Hex("2ECC8A");
        public static readonly Color Gold = Hex("FFD54A");
        public static readonly Color Blue = Hex("4DA3FF");
        public static readonly Color Green = Hex("3DDC97");
        public static readonly Color Troll = Hex("FF4D6D");
        public static readonly Color Panel = Hex("1A1224");
        public static readonly Color Hit = Hex("FFFFFF");

        public static Color ForKind(ChatKind kind) => kind switch
        {
            ChatKind.Positive => Blue,
            ChatKind.Empathy => Green,
            ChatKind.Laugh => Troll,
            _ => Gold
        };

        public static string KeyFor(ChatKind kind) => kind switch
        {
            ChatKind.Positive => "←",
            ChatKind.Empathy => "↓",
            ChatKind.Laugh => "→",
            _ => "↑ / Space"
        };

        public static string LabelFor(ChatKind kind) => kind switch
        {
            ChatKind.Positive => "긍정",
            ChatKind.Empathy => "공감",
            ChatKind.Laugh => "웃음",
            _ => "감사"
        };

        public static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out var c);
            return c;
        }
    }

    public static class UiKit
    {
        static Sprite _white;
        static Font _font;

        public static Sprite WhiteSprite
        {
            get
            {
                if (_white != null)
                    return _white;
                var t = Texture2D.whiteTexture;
                _white = Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f), 4f);
                _white.name = "UiWhite";
                return _white;
            }
        }

        public static Font Font
        {
            get
            {
                if (_font != null)
                    return _font;
                _font = Resources.Load<Font>("Fonts/NotoSansKR-Regular");
                if (_font == null)
                    _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (_font == null)
                    _font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                return _font;
            }
        }

        public static Canvas CreateCanvas(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.transform.SetParent(parent, false);
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600, 900);
            scaler.matchWidthOrHeight = 0.5f;
            var rt = go.GetComponent<RectTransform>();
            Stretch(rt);
            return canvas;
        }

        public static RectTransform Panel(Transform parent, string name, Color color)
        {
            var img = Image(parent, name, color);
            return img.rectTransform;
        }

        public static Image Image(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.sprite = WhiteSprite;
            img.color = color;
            img.raycastTarget = false;
            return img;
        }

        public static Text Label(Transform parent, string name, string text, int size, Color color, TextAnchor align = TextAnchor.MiddleLeft, FontStyle style = FontStyle.Normal)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<Text>();
            t.font = Font;
            t.text = text;
            t.fontSize = size;
            t.color = color;
            t.alignment = align;
            t.fontStyle = style;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            return t;
        }

        public static Button Button(Transform parent, string name, string caption, UnityAction onClick, Color bg, Color fg)
        {
            var img = Image(parent, name, bg);
            img.raycastTarget = true;
            var btn = img.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            var colors = btn.colors;
            colors.highlightedColor = Color.Lerp(bg, Color.white, 0.18f);
            colors.pressedColor = Color.Lerp(bg, Color.black, 0.15f);
            btn.colors = colors;
            btn.onClick.AddListener(onClick);
            Label(img.transform, "Caption", caption, 28, fg, TextAnchor.MiddleCenter, FontStyle.Bold);
            var cap = img.transform.Find("Caption") as RectTransform;
            Stretch(cap);
            return btn;
        }

        public static void Layout(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchored, Vector2 size)
        {
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchored;
            rt.sizeDelta = size;
        }

        public static void Stretch(RectTransform rt, float l = 0, float r = 0, float t = 0, float b = 0)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(l, b);
            rt.offsetMax = new Vector2(-r, -t);
        }

        public static void Wrap(Text text)
        {
            if (text == null)
                return;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
        }

        public static ScrollRect MakeScrollBody(Text body)
        {
            if (body == null)
                return null;
            Wrap(body);
            var view = body.transform.parent as RectTransform;
            if (view == null)
                return null;
            var mask = view.GetComponent<Mask>();
            if (mask == null)
            {
                mask = view.gameObject.AddComponent<Mask>();
                mask.showMaskGraphic = false;
            }
            var scroll = view.GetComponent<ScrollRect>();
            if (scroll == null)
                scroll = view.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 24f;
            scroll.viewport = view;
            var bodyRt = body.rectTransform;
            bodyRt.anchorMin = new Vector2(0f, 1f);
            bodyRt.anchorMax = new Vector2(1f, 1f);
            bodyRt.pivot = new Vector2(0.5f, 1f);
            bodyRt.offsetMin = new Vector2(0f, -bodyRt.rect.height);
            bodyRt.offsetMax = Vector2.zero;
            var fitter = body.GetComponent<ContentSizeFitter>();
            if (fitter == null)
                fitter = body.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.content = bodyRt;
            return scroll;
        }

        public static void EnsureEventSystem()
        {
            var es = UnityEngine.Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
            if (es == null)
            {
                var go = new GameObject("EventSystem");
                es = go.AddComponent<UnityEngine.EventSystems.EventSystem>();
                go.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
            UnityEngine.Object.DontDestroyOnLoad(es.gameObject);
        }

        static UnityEngine.EventSystems.EventSystem _lockedEs;
        static UnityEngine.EventSystems.StandaloneInputModule _lockedModule;
        static string _lockedHorizontal = "Horizontal";
        static string _lockedVertical = "Vertical";
        static string _lockedSubmit = "Submit";
        static string _lockedCancel = "Cancel";
        static bool _lockedNav = true;
        static bool _streamInputLocked;

        /// <summary>
        /// Keep StandaloneInputModule on so pad clicks work. Point its axes at unused
        /// names and drop navigation so arrows / Space / A-D are not Submit/Horizontal.
        /// </summary>
        public static void LockUiInputForStream()
        {
            var es = UnityEngine.EventSystems.EventSystem.current;
            if (es == null)
                es = UnityEngine.Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
            if (es == null)
                return;
            var module = es.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            if (!_streamInputLocked)
            {
                _lockedEs = es;
                _lockedModule = module;
                _lockedNav = es.sendNavigationEvents;
                if (module != null)
                {
                    _lockedHorizontal = module.horizontalAxis;
                    _lockedVertical = module.verticalAxis;
                    _lockedSubmit = module.submitButton;
                    _lockedCancel = module.cancelButton;
                }
                _streamInputLocked = true;
            }
            es.sendNavigationEvents = false;
            es.SetSelectedGameObject(null);
            if (module == null)
                return;
            module.horizontalAxis = "Disabled";
            module.verticalAxis = "Disabled";
            module.submitButton = "Disabled";
            module.cancelButton = "Disabled";
            module.enabled = true;
        }

        public static void UnlockUiInputForStream()
        {
            if (!_streamInputLocked)
                return;
            var es = _lockedEs;
            if (es == null)
                es = UnityEngine.EventSystems.EventSystem.current;
            if (es == null)
                es = UnityEngine.Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
            var module = _lockedModule;
            if (module == null && es != null)
                module = es.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            if (es != null)
            {
                es.sendNavigationEvents = _lockedNav;
                es.SetSelectedGameObject(null);
            }
            if (module != null)
            {
                module.horizontalAxis = _lockedHorizontal;
                module.verticalAxis = _lockedVertical;
                module.submitButton = _lockedSubmit;
                module.cancelButton = _lockedCancel;
                module.enabled = true;
            }
            _lockedEs = null;
            _lockedModule = null;
            _streamInputLocked = false;
        }

        public static void EnsureCamera(Color bg)
        {
            var cam = Camera.main;
            if (cam == null)
            {
                var go = new GameObject("Main Camera");
                cam = go.AddComponent<Camera>();
                go.AddComponent<AudioListener>();
                go.tag = "MainCamera";
            }

            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = bg;
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.transform.position = new Vector3(0, 0, -10);
        }
    }
}
