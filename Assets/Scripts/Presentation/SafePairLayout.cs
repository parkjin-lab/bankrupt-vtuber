using UnityEngine;

namespace BankruptVtuber
{
    /// <summary>
    /// Side-by-side on a wide canvas; stacks on portrait so both actions stay above the nav bar.
    /// </summary>
    public class SafePairLayout : MonoBehaviour
    {
        public RectTransform[] Items;
        public float Gap = 16f;
        public float Pad = 20f;
        public float MinEach = 300f;
        public float RowHeight = 72f;
        public float Bottom = 28f;
        public bool Fill;
        public bool DockBottom = true;

        public static SafePairLayout Bind(RectTransform host, RectTransform a, RectTransform b, bool fill = false, bool dockBottom = true)
        {
            if (host == null)
                return null;
            var pair = host.GetComponent<SafePairLayout>();
            if (pair == null)
                pair = host.gameObject.AddComponent<SafePairLayout>();
            pair.Items = new[] { a, b };
            pair.Fill = fill;
            pair.DockBottom = dockBottom;
            pair.Apply();
            return pair;
        }

        public static SafePairLayout BindMany(RectTransform host, bool fill, bool dockBottom, params RectTransform[] items)
        {
            if (host == null)
                return null;
            var pair = host.GetComponent<SafePairLayout>();
            if (pair == null)
                pair = host.gameObject.AddComponent<SafePairLayout>();
            pair.Items = items;
            pair.Fill = fill;
            pair.DockBottom = dockBottom;
            pair.Apply();
            return pair;
        }

        void LateUpdate() => Apply();

        void Apply()
        {
            if (Items == null || Items.Length == 0)
                return;
            var host = transform as RectTransform;
            if (host == null)
                return;

            int n = 0;
            for (int i = 0; i < Items.Length; i++)
            {
                if (Items[i] != null && Items[i].gameObject.activeSelf)
                    n++;
            }
            if (n == 0)
                return;

            float w = host.rect.width;
            float h = host.rect.height;
            bool stack = w < n * MinEach + (n - 1) * Gap + Pad * 2f;

            if (DockBottom && !Fill && stack)
            {
                float need = n * RowHeight + (n - 1) * Gap + Bottom + 8f;
                if (host.anchorMin.y == host.anchorMax.y && host.anchorMin.y == 0f)
                    host.sizeDelta = new Vector2(host.sizeDelta.x, need);
            }

            w = host.rect.width;
            h = host.rect.height;
            int slot = 0;
            for (int i = 0; i < Items.Length; i++)
            {
                var item = Items[i];
                if (item == null || !item.gameObject.activeSelf)
                    continue;
                if (stack)
                    PlaceStacked(item, slot, n, w, h);
                else
                    PlaceRow(item, slot, n, w, h);
                slot++;
            }
        }

        void PlaceRow(RectTransform item, int index, int count, float w, float h)
        {
            float inner = Mathf.Max(80f, w - Pad * 2f - Gap * (count - 1));
            float itemW = inner / count;
            float x = -w * 0.5f + Pad + itemW * 0.5f + index * (itemW + Gap);
            if (Fill)
            {
                item.anchorMin = new Vector2(0.5f, 0.5f);
                item.anchorMax = new Vector2(0.5f, 0.5f);
                item.pivot = new Vector2(0.5f, 0.5f);
                item.anchoredPosition = new Vector2(x, 0f);
                item.sizeDelta = new Vector2(itemW, Mathf.Max(160f, h - 16f));
                return;
            }

            item.anchorMin = new Vector2(0.5f, 0f);
            item.anchorMax = new Vector2(0.5f, 0f);
            item.pivot = new Vector2(0.5f, 0f);
            item.anchoredPosition = new Vector2(x, Bottom);
            item.sizeDelta = new Vector2(itemW, RowHeight);
        }

        void PlaceStacked(RectTransform item, int index, int count, float w, float h)
        {
            float itemW = Mathf.Max(120f, w - Pad * 2f);
            if (Fill)
            {
                float itemH = Mathf.Max(120f, (h - Pad * 2f - Gap * (count - 1)) / count);
                float top = h * 0.5f - Pad - itemH * 0.5f;
                item.anchorMin = new Vector2(0.5f, 0.5f);
                item.anchorMax = new Vector2(0.5f, 0.5f);
                item.pivot = new Vector2(0.5f, 0.5f);
                item.anchoredPosition = new Vector2(0f, top - index * (itemH + Gap));
                item.sizeDelta = new Vector2(itemW, itemH);
                return;
            }

            // Bottom item is the last so confirm/skip stay above the nav bar.
            int fromBottom = count - 1 - index;
            item.anchorMin = new Vector2(0.5f, 0f);
            item.anchorMax = new Vector2(0.5f, 0f);
            item.pivot = new Vector2(0.5f, 0f);
            item.anchoredPosition = new Vector2(0f, Bottom + fromBottom * (RowHeight + Gap));
            item.sizeDelta = new Vector2(itemW, RowHeight);
        }
    }
}
