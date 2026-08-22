using UnityEngine;

namespace BankruptVtuber
{
    /// <summary>
    /// Locked Week 1 numbers. Retune here or on the Resources ScriptableObject.
    /// All money is integer KRW.
    /// </summary>
    [CreateAssetMenu(fileName = "Week1Balance", menuName = "파산 버튜버/Week 1 Balance")]
    public class Week1Balance : ScriptableObject
    {
        [Header("Start")]
        public int startingCash = 45000;
        public int startingDebt = 50000;
        public int startingMental = 100;
        public int maxMental = 100;
        public float startingViewers = 12f;

        [Header("Week / win-lose")]
        public int daysInWeek = 5;
        public int winDebtMax = 30000;
        public int winCashMin = 70000;
        public int bankruptDebt = 180000;

        [Header("Daily bills (KRW)")]
        public int billRent = 8000;
        public int billElectricNet = 4000;
        public int billAvatarLicense = 3000;
        public int billFood = 5000;
        public int billGear = 2000;

        [Header("Daily extra threat (one roll / morning)")]
        public ExtraThreatDef[] extraThreats;

        [Header("Stream clock")]
        public float streamSeconds = 90f;
        public int incomePerViewerPerSec = 3;
        public float minViewers = 1f;

        [Header("Judgement viewer delta")]
        public float perfectViewerDelta = 0.5f;
        public float greatViewerDelta = 0.2f;
        public float goodViewerDelta = 0f;
        public float missViewerDelta = -1.2f;

        [Header("Timing windows (seconds from hit time)")]
        public float perfectWindow = 0.07f;
        public float greatWindow = 0.13f;
        public float goodWindow = 0.22f;
        public float approachSeconds = 1.35f;
        public float chatSpawnStart = 1.55f;
        public float chatSpawnEnd = 1.05f;

        [Header("Superchat")]
        public float superchatMinInterval = 9f;
        public float superchatMaxInterval = 11f;
        public int superchatMinCount = 8;
        public int superchatMaxCount = 10;
        public int superchatMinWon = 1000;
        public int superchatMaxWon = 6000;
        public int hypeSuperchatMinWon = 2000;
        public int hypeSuperchatMaxWon = 12000;

        [Header("Combo / hype")]
        public int comboIncomeThreshold = 5;
        public float comboIncomeMultiplier = 1.5f;
        public int hypePerfectCombo = 9;
        public float hypeSeconds = 12f;
        public float hypeIncomeMultiplier = 2.5f;
        public float hypeSuperchatMultiplier = 2f;
        public float hypeViewersPerSec = 1f;

        [Header("Mental")]
        public int missStreakMental = 3;
        public int missStreakMentalPenalty = 12;
        public int missStreakViewerPenalty = 4;
        public int totalMissMentalTrigger = 10;
        public int totalMissMentalPenalty = 20;
        public int forceEndIncomeNumerator = 1;
        public int forceEndIncomeDenominator = 2;
        public int mentalRestoreEachMorning = 15;

        public int TotalDailyBills =>
            billRent + billElectricNet + billAvatarLicense + billFood + billGear;

        public static Week1Balance Load()
        {
            var asset = Resources.Load<Week1Balance>("Balance/Week1Balance");
            if (asset != null)
                return asset;

            asset = CreateInstance<Week1Balance>();
            asset.ApplyLockedWeek1Defaults();
            return asset;
        }

        public void ApplyLockedWeek1Defaults()
        {
            startingCash = 45000;
            startingDebt = 50000;
            startingMental = 100;
            maxMental = 100;
            startingViewers = 12f;
            daysInWeek = 5;
            winDebtMax = 30000;
            winCashMin = 70000;
            bankruptDebt = 180000;
            billRent = 8000;
            billElectricNet = 4000;
            billAvatarLicense = 3000;
            billFood = 5000;
            billGear = 2000;
            streamSeconds = 90f;
            incomePerViewerPerSec = 3;
            minViewers = 1f;
            perfectViewerDelta = 0.5f;
            greatViewerDelta = 0.2f;
            goodViewerDelta = 0f;
            missViewerDelta = -1.2f;
            perfectWindow = 0.07f;
            greatWindow = 0.13f;
            goodWindow = 0.22f;
            approachSeconds = 1.35f;
            chatSpawnStart = 1.55f;
            chatSpawnEnd = 1.05f;
            superchatMinInterval = 9f;
            superchatMaxInterval = 11f;
            superchatMinCount = 8;
            superchatMaxCount = 10;
            superchatMinWon = 1000;
            superchatMaxWon = 6000;
            hypeSuperchatMinWon = 2000;
            hypeSuperchatMaxWon = 12000;
            comboIncomeThreshold = 5;
            comboIncomeMultiplier = 1.5f;
            hypePerfectCombo = 9;
            hypeSeconds = 12f;
            hypeIncomeMultiplier = 2.5f;
            hypeSuperchatMultiplier = 2f;
            hypeViewersPerSec = 1f;
            missStreakMental = 3;
            missStreakMentalPenalty = 12;
            missStreakViewerPenalty = 4;
            totalMissMentalTrigger = 10;
            totalMissMentalPenalty = 20;
            forceEndIncomeNumerator = 1;
            forceEndIncomeDenominator = 2;
            mentalRestoreEachMorning = 15;
            extraThreats = ExtraThreatRules.DefaultTable();
        }
    }
}
