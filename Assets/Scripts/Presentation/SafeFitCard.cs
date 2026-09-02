using UnityEngine;

namespace BankruptVtuber
{
    /// <summary>
    /// Keeps a centered overlay card inside its safe-area parent on portrait 9:16.
    /// </summary>
    public class SafeFitCard : MonoBehaviour
    {
        public float MaxWidth = 720f;
        public float MaxHeight = 400f;
        public float Pad = 24f;

        RectTransform _rt;

        public static SafeFitCard Bind(RectTransform card, float maxWidth, float maxHeight, float pad = 24f)
        {
            if (card == null)
                return null;
            var fit = card.GetComponent<SafeFitCard>();
            if (fit == null)
                fit = card.gameObject.AddComponent<SafeFitCard>();
            fit.MaxWidth = maxWidth;
            fit.MaxHeight = maxHeight;
            fit.Pad = pad;
            fit.Apply();
            return fit;
        }

        void Awake()
        {
            _rt = transform as RectTransform;
            Apply();
        }

        void LateUpdate() => Apply();

        void Apply()
        {
            if (_rt == null)
                _rt = transform as RectTransform;
            var parent = _rt != null ? _rt.parent as RectTransform : null;
            if (_rt == null || parent == null)
                return;
            float w = Mathf.Max(120f, parent.rect.width - Pad * 2f);
            float h = Mathf.Max(180f, parent.rect.height - Pad * 2f);
            _rt.anchorMin = new Vector2(0.5f, 0.5f);
            _rt.anchorMax = new Vector2(0.5f, 0.5f);
            _rt.pivot = new Vector2(0.5f, 0.5f);
            _rt.anchoredPosition = Vector2.zero;
            _rt.sizeDelta = new Vector2(Mathf.Min(MaxWidth, w), Mathf.Min(MaxHeight, h));
        }
    }
}
