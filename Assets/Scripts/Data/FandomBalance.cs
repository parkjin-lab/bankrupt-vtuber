using UnityEngine;

namespace BankruptVtuber
{
    /// <summary>
    /// Locked fandom numbers folded into Weeks 1–5. Does not retune Week 1–5 economy.
    /// </summary>
    [CreateAssetMenu(fileName = "FandomBalance", menuName = "파산 버튜버/Fandom Balance")]
    public class FandomBalance : ScriptableObject
    {
        [Header("Start")]
        public int startT0 = 12;
        public int startLoyalty = 40;
        public int maxLoyalty = 100;

        [Header("After stream — Perfects >= 8")]
        public int perfectHigh = 8;
        public int perfectHighT0toT1 = 2;
        public int perfectHighT1toT2 = 1;
        public int perfectHighLoyalty = 5;

        [Header("After stream — Perfects 4–7")]
        public int perfectMidLo = 4;
        public int perfectMidHi = 7;
        public int perfectMidT0toT1 = 1;
        public int perfectMidLoyalty = 1;

        [Header("After stream — Misses >= 10")]
        public int missCount = 10;
        public int missLoyalty = 8;
        public int missT2Loss = 1;

        [Header("Named T4")]
        public string minjunName = "민준";
        public string haeunName = "하은";
        public int minjunIgnoreSettlements = 3;
        public int minjunLeaveLoyalty = 12;
        public int haeunHurtStreak = 3;
        public int haeunLeaveLoyalty = 15;
        public int haeunAppearDay = 2;

        [Header("팬레터")]
        public int letterLoyalty = 4;
        public int letterMental = 8;

        [Header("팬 지원금 (loyalty >= 60)")]
        public int supportLoyaltyMin = 60;
        public int supportBase = 3000;
        public int supportPerT3 = 200;
        public int supportPerT4 = 4000;
        public int supportMin = 3000;
        public int supportMax = 20000;

        [Header("콘텐츠 편중 갈등 (once, day 11)")]
        public int conflictDay = 11;
        public int conflictSootheMental = 10;
        public int conflictSootheLoyalty = 8;
        public int conflictStyleT2 = 2;
        public int conflictStyleLoyalty = 10;
        public int conflictExtraSurcharge = 2000;

        [Header("기본 자동응답 (Week 4+ agency)")]
        public int autoDailyCost = 8000;
        public int autoLoyaltyDrain = 1;

        public static FandomBalance Load()
        {
            var asset = Resources.Load<FandomBalance>("Balance/FandomBalance");
            if (asset != null)
                return asset;

            asset = CreateInstance<FandomBalance>();
            asset.ApplyLockedFandomDefaults();
            return asset;
        }

        public void ApplyLockedFandomDefaults()
        {
            startT0 = 12;
            startLoyalty = 40;
            maxLoyalty = 100;
            perfectHigh = 8;
            perfectHighT0toT1 = 2;
            perfectHighT1toT2 = 1;
            perfectHighLoyalty = 5;
            perfectMidLo = 4;
            perfectMidHi = 7;
            perfectMidT0toT1 = 1;
            perfectMidLoyalty = 1;
            missCount = 10;
            missLoyalty = 8;
            missT2Loss = 1;
            minjunName = "민준";
            haeunName = "하은";
            minjunIgnoreSettlements = 3;
            minjunLeaveLoyalty = 12;
            haeunHurtStreak = 3;
            haeunLeaveLoyalty = 15;
            haeunAppearDay = 2;
            letterLoyalty = 4;
            letterMental = 8;
            supportLoyaltyMin = 60;
            supportBase = 3000;
            supportPerT3 = 200;
            supportPerT4 = 4000;
            supportMin = 3000;
            supportMax = 20000;
            conflictDay = 11;
            conflictSootheMental = 10;
            conflictSootheLoyalty = 8;
            conflictStyleT2 = 2;
            conflictStyleLoyalty = 10;
            conflictExtraSurcharge = 2000;
            autoDailyCost = 8000;
            autoLoyaltyDrain = 1;
        }
    }
}
