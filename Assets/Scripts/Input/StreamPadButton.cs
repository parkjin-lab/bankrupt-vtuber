using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BankruptVtuber
{
    /// <summary>
    /// LiveStream tap target. Works via IPointerDownHandler when an input module
    /// is present, and via StreamPointerRelay when StandaloneInputModule is off.
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

        public void OnPointerDown(PointerEventData eventData) => Press();

        public void OnPointerUp(PointerEventData eventData) => Release();

        public void Press()
        {
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
                img.raycastTarget = true;
            return pad;
        }
    }
}
