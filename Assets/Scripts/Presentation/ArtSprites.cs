using System.Collections.Generic;
using UnityEngine;

namespace BankruptVtuber
{
    /// <summary>
    /// Runtime-loads PNGs from Resources/Art and builds sprites.
    /// Unity generates the texture .meta on first import.
    /// </summary>
    public static class ArtSprites
    {
        public const string Avatar = "Art/pasan_nyang";
        public const string BillRent = "Art/bill_rent";
        public const string BillElectric = "Art/bill_electric";
        public const string BillLicense = "Art/bill_license";
        public const string BillFood = "Art/bill_food";
        public const string BillGear = "Art/bill_gear";
        public const string BillNotice = "Art/bill_notice";
        public const string StreamOverlay = "Art/stream_overlay";
        public const string TitleStudio = "Art/title_studio";
        public const string SettlementDesk = "Art/settlement_desk";
        public const string MorningRoom = "Art/morning_room";
        public const string Superchat = "Art/badge_superchat";
        public const string Troll = "Art/badge_troll";
        public const string BubblePill = "Art/bubble_pill";
        public const string SuperchatBanner = "Art/bubble_superchat";
        public const string TrollBubble = "Art/bubble_troll";
        public const string Sparkle = "Art/sparkle";
        public const string PanelDark = "Art/panel_dark";
        public const string ThreatBanner = "Art/banner_red";
        public const string CashBanner = "Art/banner_green";

        static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

        public static Sprite Get(string resourcePath)
        {
            if (string.IsNullOrEmpty(resourcePath))
                return null;
            if (Cache.TryGetValue(resourcePath, out var cached) && cached != null)
                return cached;

            var tex = Resources.Load<Texture2D>(resourcePath);
            if (tex == null)
            {
                Debug.LogWarning("[파산 버튜버] missing art " + resourcePath);
                return null;
            }

            tex.filterMode = FilterMode.Bilinear;
            var sprite = Sprite.Create(
                tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f),
                100f);
            sprite.name = tex.name;
            Cache[resourcePath] = sprite;
            return sprite;
        }

        public static void Apply(UnityEngine.UI.Image image, string resourcePath, Color? fallbackTint = null, Color? multiply = null)
        {
            if (image == null)
                return;
            var sprite = Get(resourcePath);
            if (sprite == null)
            {
                if (fallbackTint.HasValue)
                    image.color = fallbackTint.Value;
                return;
            }

            image.sprite = sprite;
            image.preserveAspect = true;
            image.color = multiply ?? Color.white;
            image.type = UnityEngine.UI.Image.Type.Simple;
        }

        public static void ApplySliced(UnityEngine.UI.Image image, string resourcePath, Color color, Vector4? border = null)
        {
            if (image == null)
                return;
            var sprite = GetSliced(resourcePath, border ?? new Vector4(48f, 36f, 48f, 36f));
            if (sprite == null)
            {
                Apply(image, resourcePath, color, color);
                return;
            }

            image.sprite = sprite;
            image.type = UnityEngine.UI.Image.Type.Sliced;
            image.color = color;
            image.raycastTarget = false;
        }

        static Sprite GetSliced(string resourcePath, Vector4 border)
        {
            string key = resourcePath + "|slice";
            if (Cache.TryGetValue(key, out var cached) && cached != null)
                return cached;

            var tex = Resources.Load<Texture2D>(resourcePath);
            if (tex == null)
                return Get(resourcePath);

            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            var sprite = Sprite.Create(
                tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect,
                border);
            sprite.name = tex.name + "_slice";
            Cache[key] = sprite;
            return sprite;
        }
    }
}
