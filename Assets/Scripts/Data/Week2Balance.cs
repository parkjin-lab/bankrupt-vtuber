using UnityEngine;

namespace BankruptVtuber
{
    /// <summary>
    /// Locked Week 2 numbers (days 6–10). Days 1–5 keep Week1Balance.
    /// </summary>
    [CreateAssetMenu(fileName = "Week2Balance", menuName = "파산 버튜버/Week 2 Balance")]
    public class Week2Balance : ScriptableObject
    {
        [Header("Calendar")]
        public int firstDay = 6;
        public int lastDay = 10;

        [Header("Week 2 entry (after Week 1 clear)")]
        public int entryCash = 15000;
        public int entryDebtRelief = 10000;
        public int entryMental = 100;

        [Header("Win / lose (KRW)")]
        public int bankruptDebt = 220000;
        public int winDebtMax = 20000;
        public int winCashMin = 110000;

        [Header("Daily bills (KRW) — ₩28,000")]
        public int billRent = 10000;
        public int billElectricNet = 5000;
        public int billAvatarLicense = 5000;
        public int billFood = 6000;
        public int billGear = 2000;

        [Header("Daily extra threats (0–2 independent rolls)")]
        public ExtraThreatDef[] extraThreats;
        public int extraThreatMaxPerDay = 2;

        [Header("Membership")]
        public int startingMembers = 8;
        public float unlockPeakViewers = 40f;
        public int unlockSuccessfulStreams = 4;
        public int membersFromHype = 1;
        public int membersFromHypeDayMax = 2;
        public int membersMissPenaltyAt = 10;
        public int membersMissLoss = 1;
        public int membershipPassivePerMember = 150;

        [Header("Viral clip (the one new stream variable)")]
        public int clipPerfectsRequired = 25;
        public int clipChance = 30;
        public int clipCash = 30000;
        public int clipViewerBonus = 10;

        public int TotalDailyBills =>
            billRent + billElectricNet + billAvatarLicense + billFood + billGear;

        public static Week2Balance Load()
        {
            var asset = Resources.Load<Week2Balance>("Balance/Week2Balance");
            if (asset != null)
                return asset;

            asset = CreateInstance<Week2Balance>();
            asset.ApplyLockedWeek2Defaults();
            return asset;
        }

        public void ApplyLockedWeek2Defaults()
        {
            firstDay = 6;
            lastDay = 10;
            entryCash = 15000;
            entryDebtRelief = 10000;
            entryMental = 100;
            bankruptDebt = 220000;
            winDebtMax = 20000;
            winCashMin = 110000;
            billRent = 10000;
            billElectricNet = 5000;
            billAvatarLicense = 5000;
            billFood = 6000;
            billGear = 2000;
            extraThreatMaxPerDay = 2;
            startingMembers = 8;
            unlockPeakViewers = 40f;
            unlockSuccessfulStreams = 4;
            membersFromHype = 1;
            membersFromHypeDayMax = 2;
            membersMissPenaltyAt = 10;
            membersMissLoss = 1;
            membershipPassivePerMember = 150;
            clipPerfectsRequired = 25;
            clipChance = 30;
            clipCash = 30000;
            clipViewerBonus = 10;
            extraThreats = ExtraThreatRules.DefaultWeek2Table();
        }
    }
}
