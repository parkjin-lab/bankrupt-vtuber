using UnityEngine;

namespace BankruptVtuber
{
    public enum ExtraThreatFx
    {
        None,
        Gear,
        Net,
        Rival,
        Scandal,
        Fee,
        Generic
    }

    /// <summary>
    /// Presentation fingerprint for an already-billed extra threat.
    /// Does not change ₩, spawn, income, or input.
    /// </summary>
    public readonly struct ExtraThreatLook
    {
        public readonly ExtraThreatFx Fx;
        public readonly string Id;
        public readonly string Badge;
        public readonly Color Tint;
        public readonly string Art;

        public ExtraThreatLook(ExtraThreatFx fx, string id, string badge, Color tint, string art)
        {
            Fx = fx;
            Id = id ?? "";
            Badge = badge ?? "";
            Tint = tint;
            Art = art ?? "";
        }

        public static ExtraThreatLook For(ExtraThreatRoll roll) =>
            For(roll.Id, roll.DisplayName, roll.Tint, roll.ArtPath);

        public static ExtraThreatLook For(string id, string displayName, Color tint, string art)
        {
            string key = id ?? "";
            string name = displayName ?? "";
            string path = string.IsNullOrEmpty(art) ? ArtSprites.Troll : art;
            Color color = tint.a <= 0.01f ? Palette.MoneyRed : tint;

            if (key == "gear_break")
                return new ExtraThreatLook(ExtraThreatFx.Gear, key, "장비 불안정", color, path);
            if (key == "net_drop")
                return new ExtraThreatLook(ExtraThreatFx.Net, key, "재연결 중…", color, path);
            if (key == "rival")
                return new ExtraThreatLook(ExtraThreatFx.Rival, key, string.IsNullOrEmpty(name) ? "라이벌 견제" : name, color, path);
            if (key == "scandal")
                return new ExtraThreatLook(ExtraThreatFx.Scandal, key, string.IsNullOrEmpty(name) ? "스캔들 루머" : name, color, path);
            if (key == "platform_fee" || key == "petty_bill")
                return new ExtraThreatLook(ExtraThreatFx.Fee, key, "수수료", color, path);
            if (string.IsNullOrEmpty(key) && string.IsNullOrEmpty(name))
                return new ExtraThreatLook(ExtraThreatFx.None, "", "", color, path);
            return new ExtraThreatLook(ExtraThreatFx.Generic, key, name, color, path);
        }
    }
}
