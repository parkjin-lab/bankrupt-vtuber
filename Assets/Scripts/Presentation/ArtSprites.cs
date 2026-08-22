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
        public const string Superchat = "Art/badge_superchat";
        public const string Troll = "Art/badge_troll";

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

        public static void Apply(UnityEngine.UI.Image image, string resourcePath, Color? fallbackTint = null)
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
            image.color = Color.white;
            image.type = UnityEngine.UI.Image.Type.Simple;
        }
    }
}
