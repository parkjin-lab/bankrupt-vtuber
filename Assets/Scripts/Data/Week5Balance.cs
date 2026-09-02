using UnityEngine;

namespace BankruptVtuber
{
    /// <summary>
    /// Locked Week 5 numbers (days 21–25, ending week). Weeks 1–4 stay on their own assets.
    /// </summary>
    [CreateAssetMenu(fileName = "Week5Balance", menuName = "파산 버튜버/Week 5 Balance")]
    public class Week5Balance : ScriptableObject
    {
        [Header("Calendar")]
        public int firstDay = 21;
        public int lastDay = 25;

        [Header("Win / lose (KRW)")]
        public int bankruptDebt = 350000;

        [Header("Daily bills (KRW) — ₩45,000 solo / ₩60,000 agency")]
        public int billRent = 15000;
        public int billElectricNet = 8000;
        public int billAvatarLicense = 8000;
        public int billFood = 8000;
        public int billGear = 6000;
        public int agencyDailyCost = 15000;

        [Header("Daily extra threats (0–2)")]
        public ExtraThreatDef[] extraThreats;
        public int extraThreatMaxPerDay = 2;

        [Header("Ranking")]
        public int rankingDay = 22;
        public float rankingPeakViewers = 100f;
        public int rankingPeakFactor = 3;
        public int rankingMembersFactor = 8;
        public int rankingGoodsFactor = 4;
        public int rankingPerfectsFactor = 2;
        public int rankingDailyFirstCash = 10000;
        public int npcBase0 = 420;
        public int npcBase1 = 360;
        public int npcBase2 = 300;
        public int npcVarianceLo = -70;
        public int npcVarianceHi = 90;

        [Header("Concert")]
        public int concertUnlockCash = 150000;
        public float concertUnlockPeak = 90f;
        public int concertUnlockDay = 22;
        public int concertCost = 80000;
        public int concertBasePayout = 200000;
        public int concertRankBonus = 500;
        public float concertSuccessMultiplier = 1.3f;
        public int concertFailMisses = 12;
        public int concertFailMental = 25;
        public int concertFailViewers = 10;
        public int concertLowMental = 24;

        [Header("콘서트 퍼포먼스 타이밍 (one new stream variable)")]
        public float concertWindowSeconds = 1.2f;
        public float concertFallbackSeconds = 55f;

        [Header("Endings")]
        public int endingSoloMental = 40;
        public int endingEmpireCash = 250000;
        public int burnoutZeroMentalDays = 2;

        public int TotalDailyBills =>
            billRent + billElectricNet + billAvatarLicense + billFood + billGear;

        public static readonly string[] NpcNames = { "루나벨", "하츠비", "네온토끼" };

        public static Week5Balance Load()
        {
            var asset = Resources.Load<Week5Balance>("Balance/Week5Balance");
            if (asset != null)
                return asset;

            asset = CreateInstance<Week5Balance>();
            asset.ApplyLockedWeek5Defaults();
            return asset;
        }

        public void ApplyLockedWeek5Defaults()
        {
            firstDay = 21;
            lastDay = 25;
            bankruptDebt = 350000;
            billRent = 15000;
            billElectricNet = 8000;
            billAvatarLicense = 8000;
            billFood = 8000;
            billGear = 6000;
            agencyDailyCost = 15000;
            extraThreatMaxPerDay = 2;
            rankingDay = 22;
            rankingPeakViewers = 100f;
            rankingPeakFactor = 3;
            rankingMembersFactor = 8;
            rankingGoodsFactor = 4;
            rankingPerfectsFactor = 2;
            rankingDailyFirstCash = 10000;
            npcBase0 = 420;
            npcBase1 = 360;
            npcBase2 = 300;
            npcVarianceLo = -70;
            npcVarianceHi = 90;
            concertUnlockCash = 150000;
            concertUnlockPeak = 90f;
            concertUnlockDay = 22;
            concertCost = 80000;
            concertBasePayout = 200000;
            concertRankBonus = 500;
            concertSuccessMultiplier = 1.3f;
            concertFailMisses = 12;
            concertFailMental = 25;
            concertFailViewers = 10;
            concertLowMental = 24;
            concertWindowSeconds = 1.2f;
            concertFallbackSeconds = 55f;
            endingSoloMental = 40;
            endingEmpireCash = 250000;
            burnoutZeroMentalDays = 2;
            extraThreats = ExtraThreatRules.DefaultWeek5Table();
        }
    }
}
