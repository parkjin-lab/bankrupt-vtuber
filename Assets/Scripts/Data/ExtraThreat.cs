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
        public int chancePercent;
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

        public static ExtraThreatDef[] DefaultWeek2Table()
        {
            return new[]
            {
                new ExtraThreatDef
                {
                    id = "gear_break",
                    displayName = "장비 고장",
                    minWon = 5000,
                    maxWon = 12000,
                    chancePercent = 20,
                    artPath = ArtSprites.BillGear,
                    tintHex = "FF6A6A"
                },
                new ExtraThreatDef
                {
                    id = "petty_bill",
                    displayName = "소액 추가 청구",
                    minWon = 3000,
                    maxWon = 8000,
                    chancePercent = 25,
                    artPath = ArtSprites.BillFood,
                    tintHex = "FFB020"
                },
                new ExtraThreatDef
                {
                    id = "platform_fee",
                    displayName = "플랫폼 수수료",
                    minWon = 3000,
                    maxWon = 3000,
                    chancePercent = 15,
                    artPath = ArtSprites.Superchat,
                    tintHex = "FFB020"
                }
            };
        }

        public static ExtraThreatDef[] TableOrDefault(Week1Balance b)
        {
            if (b != null && b.extraThreats != null && b.extraThreats.Length > 0)
                return b.extraThreats;
            return DefaultTable();
        }

        public static ExtraThreatDef[] TableOrDefault(ExtraThreatDef[] table, ExtraThreatDef[] fallback)
        {
            if (table != null && table.Length > 0)
                return table;
            return fallback ?? DefaultTable();
        }

        public static int MixSeed(int runSeed, int day) =>
            unchecked(runSeed * 397 ^ day * 1009);

        /// <summary>
        /// One extra threat per morning. Order is shuffled by runSeed so a week
        /// is not the same five ₩22,000 days, and Restart is not identical.
        /// Amount uses day + runSeed so it is debug-reproducible.
        /// </summary>
        public static ExtraThreatRoll Roll(Week1Balance b, int runSeed, int day) =>
            Roll(TableOrDefault(b), runSeed, day);

        public static ExtraThreatRoll Roll(ExtraThreatDef[] table, int runSeed, int day)
        {
            if (table == null || table.Length == 0)
                table = DefaultTable();
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

        public static ExtraThreatDef[] DefaultWeek3Table()
        {
            return new[]
            {
                new ExtraThreatDef
                {
                    id = "gear_break",
                    displayName = "장비 고장",
                    minWon = 6000,
                    maxWon = 15000,
                    chancePercent = 25,
                    artPath = ArtSprites.BillGear,
                    tintHex = "FF6A6A"
                },
                new ExtraThreatDef
                {
                    id = "petty_bill",
                    displayName = "소액 추가",
                    minWon = 4000,
                    maxWon = 10000,
                    chancePercent = 25,
                    artPath = ArtSprites.BillFood,
                    tintHex = "FFB020"
                },
                new ExtraThreatDef
                {
                    id = "platform_fee",
                    displayName = "플랫폼 수수료",
                    minWon = 4000,
                    maxWon = 4000,
                    chancePercent = 20,
                    artPath = ArtSprites.Superchat,
                    tintHex = "FFB020"
                }
            };
        }

        public static ExtraThreatRoll[] RollWeek2(Week2Balance w2, int runSeed, int day)
        {
            var table = ExtraThreatRules.TableOrDefault(
                w2 != null ? w2.extraThreats : null,
                DefaultWeek2Table());
            int cap = w2 != null && w2.extraThreatMaxPerDay > 0 ? w2.extraThreatMaxPerDay : 2;
            return RollChanceExtras(table, cap, runSeed, day);
        }

        public static ExtraThreatRoll[] RollWeek3(Week3Balance w3, int runSeed, int day)
        {
            var table = ExtraThreatRules.TableOrDefault(
                w3 != null ? w3.extraThreats : null,
                DefaultWeek3Table());
            int cap = w3 != null && w3.extraThreatMaxPerDay > 0 ? w3.extraThreatMaxPerDay : 2;
            return RollChanceExtras(table, cap, runSeed, day);
        }

        public static ExtraThreatDef[] DefaultWeek4Table()
        {
            return new[]
            {
                new ExtraThreatDef
                {
                    id = "gear_break",
                    displayName = "장비 고장",
                    minWon = 8000,
                    maxWon = 18000,
                    chancePercent = 25,
                    artPath = ArtSprites.BillGear,
                    tintHex = "FF6A6A"
                },
                new ExtraThreatDef
                {
                    id = "petty_bill",
                    displayName = "소액",
                    minWon = 5000,
                    maxWon = 12000,
                    chancePercent = 25,
                    artPath = ArtSprites.BillFood,
                    tintHex = "FFB020"
                },
                new ExtraThreatDef
                {
                    id = "platform_fee",
                    displayName = "수수료",
                    minWon = 5000,
                    maxWon = 5000,
                    chancePercent = 20,
                    artPath = ArtSprites.Superchat,
                    tintHex = "FFB020"
                }
            };
        }

        public static ExtraThreatRoll[] RollWeek4(Week4Balance w4, int runSeed, int day)
        {
            var table = ExtraThreatRules.TableOrDefault(
                w4 != null ? w4.extraThreats : null,
                DefaultWeek4Table());
            int cap = w4 != null && w4.extraThreatMaxPerDay > 0 ? w4.extraThreatMaxPerDay : 2;
            return RollChanceExtras(table, cap, runSeed, day);
        }

        public static ExtraThreatDef[] DefaultWeek5Table()
        {
            return new[]
            {
                new ExtraThreatDef
                {
                    id = "gear_break",
                    displayName = "고장",
                    minWon = 10000,
                    maxWon = 20000,
                    chancePercent = 30,
                    artPath = ArtSprites.BillGear,
                    tintHex = "FF6A6A"
                },
                new ExtraThreatDef
                {
                    id = "petty_bill",
                    displayName = "소액",
                    minWon = 6000,
                    maxWon = 15000,
                    chancePercent = 25,
                    artPath = ArtSprites.BillFood,
                    tintHex = "FFB020"
                },
                new ExtraThreatDef
                {
                    id = "platform_fee",
                    displayName = "수수료",
                    minWon = 6000,
                    maxWon = 6000,
                    chancePercent = 20,
                    artPath = ArtSprites.Superchat,
                    tintHex = "FFB020"
                }
            };
        }

        public static ExtraThreatRoll[] RollWeek5(Week5Balance w5, int runSeed, int day)
        {
            var table = ExtraThreatRules.TableOrDefault(
                w5 != null ? w5.extraThreats : null,
                DefaultWeek5Table());
            int cap = w5 != null && w5.extraThreatMaxPerDay > 0 ? w5.extraThreatMaxPerDay : 2;
            return RollChanceExtras(table, cap, runSeed, day);
        }

        public static ExtraThreatRoll[] RollChanceExtras(ExtraThreatDef[] table, int cap, int runSeed, int day)
        {
            if (table == null || table.Length == 0)
                table = DefaultWeek2Table();
            if (cap <= 0)
                cap = 2;
            var rng = new System.Random(MixSeed(runSeed, day));
            var hits = new System.Collections.Generic.List<ExtraThreatDef>(table.Length);
            for (int i = 0; i < table.Length; i++)
            {
                int chance = table[i].chancePercent;
                if (chance > 0 && rng.Next(100) < chance)
                    hits.Add(table[i]);
            }

            if (hits.Count > cap)
            {
                for (int i = hits.Count - 1; i > 0; i--)
                {
                    int j = rng.Next(i + 1);
                    var tmp = hits[i];
                    hits[i] = hits[j];
                    hits[j] = tmp;
                }
                hits.RemoveRange(cap, hits.Count - cap);
            }

            var rolls = new ExtraThreatRoll[hits.Count];
            for (int i = 0; i < hits.Count; i++)
            {
                var def = hits[i];
                int lo = System.Math.Min(def.minWon, def.maxWon);
                int hi = System.Math.Max(def.minWon, def.maxWon);
                int amount = lo == hi ? lo : rng.Next(lo, hi + 1);
                amount = amount / 100 * 100;
                if (amount < lo)
                    amount = lo;
                if (amount > hi)
                    amount = hi;
                string art = string.IsNullOrEmpty(def.artPath) ? ArtSprites.Troll : def.artPath;
                rolls[i] = new ExtraThreatRoll(def.id, def.displayName, amount, art, def.Tint);
            }

            return rolls;
        }

        public static void EnsureRolled(GameRunState state, Week1Balance b, Week2Balance w2 = null, Week3Balance w3 = null, Week4Balance w4 = null, Week5Balance w5 = null)
        {
            if (state == null || b == null)
                return;
            if (state.extraThreatRolled)
                return;
            if (WeekSchedule.InWeek5(state))
            {
                state.ApplyExtraRolls(RollWeek5(w5, state.runSeed, state.day));
                return;
            }
            if (WeekSchedule.InWeek4(state))
            {
                state.ApplyExtraRolls(RollWeek4(w4, state.runSeed, state.day));
                return;
            }
            if (WeekSchedule.InWeek3(state))
            {
                state.ApplyExtraRolls(RollWeek3(w3, state.runSeed, state.day));
                return;
            }
            if (WeekSchedule.InWeek2(state))
            {
                state.ApplyExtraRolls(RollWeek2(w2, state.runSeed, state.day));
                return;
            }

            var roll = Roll(TableOrDefault(b), state.runSeed, state.day);
            state.ApplyExtraThreat(roll);
        }
    }
}
