using UnityEngine;

namespace BankruptVtuber
{
    /// <summary>
    /// Locked Week 4 numbers (days 16–20). Weeks 1–3 stay on their own assets.
    /// </summary>
    [CreateAssetMenu(fileName = "Week4Balance", menuName = "파산 버튜버/Week 4 Balance")]
    public class Week4Balance : ScriptableObject
    {
        [Header("Calendar")]
        public int firstDay = 16;
        public int lastDay = 20;

        [Header("Win / lose (KRW)")]
        public int bankruptDebt = 300000;
        public int winDebtMax = 10000;
        public int winCashMin = 180000;

        [Header("Daily bills (KRW) — ₩38,000 before agency")]
        public int billRent = 14000;
        public int billElectricNet = 7000;
        public int billAvatarLicense = 7000;
        public int billFood = 7000;
        public int billGear = 3000;

        [Header("Daily extra threats (0–2)")]
        public ExtraThreatDef[] extraThreats;
        public int extraThreatMaxPerDay = 2;

        [Header("Agency")]
        public int agencyUnlockCash = 100000;
        public int agencyUnlockDebtMax = 40000;
        public int agencyFoundCost = 40000;
        public int agencyDailyCost = 15000;

        [Header("Junior (slot 1)")]
        public int juniorScoutCost = 25000;
        public int juniorDailySuccess = 4000;
        public int juniorTrainFailMental = 8;
        public int juniorTrainFailMisses = 10;

        [Header("Sponsor (one deal)")]
        public float sponsorPeakViewers = 70f;
        public int sponsorDaily = 10000;
        public int sponsorDays = 5;
        public int sponsorLineBonus = 3000;
        public int sponsorFailCash = 15000;
        public int sponsorFailMental = 12;

        [Header("스폰서 멘트 타이밍 (one new stream variable)")]
        public float lineWindowSeconds = 1.2f;
        public float lineFallbackSeconds = 55f;

        public int TotalDailyBills =>
            billRent + billElectricNet + billAvatarLicense + billFood + billGear;

        public static Week4Balance Load()
        {
            var asset = Resources.Load<Week4Balance>("Balance/Week4Balance");
            if (asset != null)
                return asset;

            asset = CreateInstance<Week4Balance>();
            asset.ApplyLockedWeek4Defaults();
            return asset;
        }

        public void ApplyLockedWeek4Defaults()
        {
            firstDay = 16;
            lastDay = 20;
            bankruptDebt = 300000;
            winDebtMax = 10000;
            winCashMin = 180000;
            billRent = 14000;
            billElectricNet = 7000;
            billAvatarLicense = 7000;
            billFood = 7000;
            billGear = 3000;
            extraThreatMaxPerDay = 2;
            agencyUnlockCash = 100000;
            agencyUnlockDebtMax = 40000;
            agencyFoundCost = 40000;
            agencyDailyCost = 15000;
            juniorScoutCost = 25000;
            juniorDailySuccess = 4000;
            juniorTrainFailMental = 8;
            juniorTrainFailMisses = 10;
            sponsorPeakViewers = 70f;
            sponsorDaily = 10000;
            sponsorDays = 5;
            sponsorLineBonus = 3000;
            sponsorFailCash = 15000;
            sponsorFailMental = 12;
            lineWindowSeconds = 1.2f;
            lineFallbackSeconds = 55f;
            extraThreats = ExtraThreatRules.DefaultWeek4Table();
        }
    }
}
