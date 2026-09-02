using UnityEngine;
using UnityEngine.UI;

namespace BankruptVtuber
{
    /// <summary>
    /// Fits a RectTransform to Screen.safeArea so pads and cards sit above the Android nav bar.
    /// </summary>
    public class StreamSafeArea : MonoBehaviour
    {
        RectTransform _rt;

        public static RectTransform Attach(Transform canvas)
        {
            var safe = UiKit.Panel(canvas, "Safe", new Color(0, 0, 0, 0));
            UiKit.Stretch(safe);
            var img = safe.GetComponent<Image>();
            if (img != null)
                img.raycastTarget = false;
            safe.gameObject.AddComponent<StreamSafeArea>();
            return safe;
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
                return;
            float w = Screen.width;
            float h = Screen.height;
            if (w < 1f || h < 1f)
                return;
            Rect safe = Screen.safeArea;
            _rt.anchorMin = new Vector2(safe.xMin / w, safe.yMin / h);
            _rt.anchorMax = new Vector2(safe.xMax / w, safe.yMax / h);
            _rt.offsetMin = Vector2.zero;
            _rt.offsetMax = Vector2.zero;
        }
    }
}
