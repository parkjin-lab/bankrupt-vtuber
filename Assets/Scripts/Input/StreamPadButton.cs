using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BankruptVtuber
{
    /// <summary>
    /// LiveStream tap target. EventSystem pointer (StandaloneInputModule on) is primary;
    /// StreamPointerRelay is a fallback if the module is off.
    /// </summary>
    public class StreamPadButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public enum Mode
        {
            Kind,
            Superchat,
            Event,
            PromoConfirm,
            PromoSkip
        }

        public Mode mode = Mode.Kind;
        public ChatKind kind;
        public int eventIndex;

        Image _img;
        Color _base = Color.white;
        float _flash;

        void Awake()
        {
            _img = GetComponent<Image>();
            if (_img != null)
                _base = _img.color;
        }

        void Update()
        {
            if (_img == null || _flash <= 0f)
                return;
            _flash -= Time.unscaledDeltaTime;
            _img.color = Color.Lerp(_base, Color.white, Mathf.Clamp01(_flash / 0.16f));
        }

        public void OnPointerDown(PointerEventData eventData) => Press();

        public void OnPointerUp(PointerEventData eventData) => Release();

        public void Flash()
        {
            if (_img != null)
                _base = _img.color;
            _flash = 0.16f;
        }

        public void Press()
        {
            Flash();
            switch (mode)
            {
                case Mode.Kind:
                    StreamBindings.QueueKind(kind);
                    break;
                case Mode.Superchat:
                    StreamBindings.BeginSuperchatCharge();
                    break;
                case Mode.Event:
                    StreamBindings.QueueEvent(eventIndex);
                    break;
                case Mode.PromoConfirm:
                    StreamBindings.QueuePromo(true);
                    break;
                case Mode.PromoSkip:
                    StreamBindings.QueuePromo(false);
                    break;
            }
        }

        public void Release()
        {
            if (mode == Mode.Superchat)
                StreamBindings.EndSuperchatCharge();
        }

        public static StreamPadButton Attach(
            GameObject go,
            Mode mode,
            ChatKind kind = ChatKind.Positive,
            int eventIndex = 0)
        {
            var pad = go.GetComponent<StreamPadButton>();
            if (pad == null)
                pad = go.AddComponent<StreamPadButton>();
            pad.mode = mode;
            pad.kind = kind;
            pad.eventIndex = eventIndex;
            var img = go.GetComponent<Image>();
            if (img != null)
            {
                img.raycastTarget = true;
                pad._img = img;
                pad._base = img.color;
            }

            var btn = go.GetComponent<Button>();
            if (btn == null)
                btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.transition = Selectable.Transition.None;
            var nav = btn.navigation;
            nav.mode = Navigation.Mode.None;
            btn.navigation = nav;
            btn.onClick.RemoveAllListeners();
            return pad;
        }
    }
}
