using System;
using UnityEngine;

namespace BankruptVtuber
{
    [Serializable]
    public class ExtraThreatDef
    {
        public string id;
        public string displayName;
        public int minWon = 4000;
        public int maxWon = 12000;
        public string artPath;
        public string tintHex = "FF6A6A";

        public Color Tint
        {
            get
            {
                var hex = tintHex;
                if (string.IsNullOrEmpty(hex))
                    hex = "FF6A6A";
                if (hex[0] != '#')
                    hex = "#" + hex;
                return ColorUtility.TryParseHtmlString(hex, out var c) ? c : new Color(1f, 0.42f, 0.42f);
            }
        }
    }

    public readonly struct ExtraThreatRoll
    {
        public readonly string Id;
        public readonly string DisplayName;
        public readonly int Amount;
        public readonly string ArtPath;
        public readonly Color Tint;

        public ExtraThreatRoll(string id, string displayName, int amount, string artPath, Color tint)
        {
            Id = id;
            DisplayName = displayName;
            Amount = amount;
            ArtPath = artPath;
            Tint = tint;
        }
    }

    public static class ExtraThreatRules
    {
        public static ExtraThreatDef[] DefaultTable()
        {
            return new[]
            {
                new ExtraThreatDef
                {
                    id = "gear_break",
                    displayName = "장비 고장",
                    minWon = 7000,
                    maxWon = 11000,
                    artPath = ArtSprites.BillGear,
                    tintHex = "FF6A6A"
                },
                new ExtraThreatDef
                {
                    id = "rival",
                    displayName = "라이벌 견제",
                    minWon = 5000,
                    maxWon = 9000,
                    artPath = ArtSprites.Troll,
                    tintHex = "C47BFF"
                },
                new ExtraThreatDef
                {
                    id = "platform_fee",
                    displayName = "플랫폼 수수료",
                    minWon = 4000,
                    maxWon = 7000,
                    artPath = ArtSprites.Superchat,
                    tintHex = "FFB020"
                },
                new ExtraThreatDef
                {
                    id = "scandal",
                    displayName = "스캔들 루머",
                    minWon = 8000,
                    maxWon = 12000,
                    artPath = ArtSprites.Troll,
                    tintHex = "FF3355"
                },
                new ExtraThreatDef
                {
                    id = "net_drop",
                    displayName = "인터넷 끊김",
                    minWon = 4000,
                    maxWon = 6000,
                    artPath = ArtSprites.BillElectric,
                    tintHex = "4EC8FF"
                }
            };
        }

        public static ExtraThreatDef[] TableOrDefault(Week1Balance b)
        {
            if (b != null && b.extraThreats != null && b.extraThreats.Length > 0)
                return b.extraThreats;
            return DefaultTable();
        }

        public static int MixSeed(int runSeed, int day) =>
            unchecked(runSeed * 397 ^ day * 1009);

        /// <summary>
        /// One extra threat per morning. Order is shuffled by runSeed so a week
        /// is not the same five ₩22,000 days, and Restart is not identical.
        /// Amount uses day + runSeed so it is debug-reproducible.
        /// </summary>
        public static ExtraThreatRoll Roll(Week1Balance b, int runSeed, int day)
        {
            var table = TableOrDefault(b);
            int[] order = new int[table.Length];
            for (int i = 0; i < order.Length; i++)
                order[i] = i;

            var shuffle = new System.Random(runSeed);
            for (int i = order.Length - 1; i > 0; i--)
            {
                int j = shuffle.Next(i + 1);
                int tmp = order[i];
                order[i] = order[j];
                order[j] = tmp;
            }

            int slot = Math.Abs(day - 1) % table.Length;
            var def = table[order[slot]];
            int lo = Math.Min(def.minWon, def.maxWon);
            int hi = Math.Max(def.minWon, def.maxWon);
            var amountRng = new System.Random(MixSeed(runSeed, day));
            int amount = amountRng.Next(lo, hi + 1);
            amount = amount / 100 * 100;
            if (amount < lo)
                amount = lo;
            if (amount > hi)
                amount = hi;

            string art = string.IsNullOrEmpty(def.artPath) ? ArtSprites.Troll : def.artPath;
            return new ExtraThreatRoll(def.id, def.displayName, amount, art, def.Tint);
        }

        public static void EnsureRolled(GameRunState state, Week1Balance b)
        {
            if (state == null || b == null)
                return;
            if (state.extraThreatRolled && state.extraThreatAmount > 0)
                return;
            var roll = Roll(b, state.runSeed, state.day);
            state.ApplyExtraThreat(roll);
        }
    }
}
