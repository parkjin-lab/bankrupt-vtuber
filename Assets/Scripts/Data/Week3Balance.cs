using UnityEngine;

namespace BankruptVtuber
{
    /// <summary>
    /// Locked Week 3 numbers (days 11–15). Weeks 1–2 stay on their own assets.
    /// </summary>
    [CreateAssetMenu(fileName = "Week3Balance", menuName = "파산 버튜버/Week 3 Balance")]
    public class Week3Balance : ScriptableObject
    {
        [Header("Calendar")]
        public int firstDay = 11;
        public int lastDay = 15;

        [Header("Win / lose (KRW)")]
        public int bankruptDebt = 260000;
        public int winDebtMax = 15000;
        public int winCashMin = 140000;

        [Header("Daily bills (KRW) — ₩34,000")]
        public int billRent = 12000;
        public int billElectricNet = 6000;
        public int billAvatarLicense = 6000;
        public int billFood = 7000;
        public int billGear = 3000;

        [Header("Daily extra threats (0–2)")]
        public ExtraThreatDef[] extraThreats;
        public int extraThreatMaxPerDay = 2;

        [Header("Rival")]
        public int rivalDay = 12;
        public float rivalPeakViewers = 55f;
        public float rivalStartViewers = 25f;
        public float rivalViewersPerSec = 0.9f;
        public float rivalPerfectSteal = 0.6f;
        public float rivalMissSteal = 0.8f;
        public int rivalWinCash = 20000;
        public int rivalWinViewerBonus = 6;
        public int rivalLoseViewerPenalty = 5;
        public int rivalLoseMental = 12;

        [Header("Goods — 아크릴 스탠드")]
        public int goodsUnlockCash = 60000;
        public int goodsUnlockStock = 20;
        public int goodsProduceCost = 2500;
        public int goodsPrice = 7000;
        public float goodsSoldMembersFactor = 0.4f;
        public float goodsSoldPeakFactor = 0.08f;
        public int goodsSoldMin = 1;
        public float goodsPromoMultiplier = 1.5f;

        [Header("굿즈 홍보 타이밍 (one new stream variable)")]
        public float promoWindowSeconds = 1.2f;
        public float promoFallbackSeconds = 55f;

        public int TotalDailyBills =>
            billRent + billElectricNet + billAvatarLicense + billFood + billGear;

        public static Week3Balance Load()
        {
            var asset = Resources.Load<Week3Balance>("Balance/Week3Balance");
            if (asset != null)
                return asset;

            asset = CreateInstance<Week3Balance>();
            asset.ApplyLockedWeek3Defaults();
            return asset;
        }

        public void ApplyLockedWeek3Defaults()
        {
            firstDay = 11;
            lastDay = 15;
            bankruptDebt = 260000;
            winDebtMax = 15000;
            winCashMin = 140000;
            billRent = 12000;
            billElectricNet = 6000;
            billAvatarLicense = 6000;
            billFood = 7000;
            billGear = 3000;
            extraThreatMaxPerDay = 2;
            rivalDay = 12;
            rivalPeakViewers = 55f;
            rivalStartViewers = 25f;
            rivalViewersPerSec = 0.9f;
            rivalPerfectSteal = 0.6f;
            rivalMissSteal = 0.8f;
            rivalWinCash = 20000;
            rivalWinViewerBonus = 6;
            rivalLoseViewerPenalty = 5;
            rivalLoseMental = 12;
            goodsUnlockCash = 60000;
            goodsUnlockStock = 20;
            goodsProduceCost = 2500;
            goodsPrice = 7000;
            goodsSoldMembersFactor = 0.4f;
            goodsSoldPeakFactor = 0.08f;
            goodsSoldMin = 1;
            goodsPromoMultiplier = 1.5f;
            promoWindowSeconds = 1.2f;
            promoFallbackSeconds = 55f;
            extraThreats = ExtraThreatRules.DefaultWeek3Table();
        }
    }
}
