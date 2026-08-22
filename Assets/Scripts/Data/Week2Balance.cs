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

        [Header("Win / lose (KRW)")]
        public int bankruptDebt = 180000;
        public int winDebtMax = 20000;
        public int winCashMin = 120000;
        public int winMembershipMin = 15;

        [Header("Daily bills (KRW) — ₩28,000")]
        public int billRent = 10000;
        public int billElectricNet = 5000;
        public int billAvatarLicense = 4000;
        public int billFood = 6000;
        public int billGear = 3000;

        [Header("Daily extra threat (₩6,000–₩16,000)")]
        public ExtraThreatDef[] extraThreats;

        [Header("Membership")]
        public int startingMembers = 8;
        public int membersHighPerfects = 8;
        public int membersHighGain = 2;
        public int membersLowPerfects = 4;
        public int membersLowGain = 1;
        public int membershipPassivePerMember = 400;
        public float membershipPitchSeconds = 1.2f;
        public float membershipPitchAtSeconds = 60f;
        public int pitchMemberBonus = 3;

        [Header("Viral clip")]
        public int clipChanceWithHype = 35;
        public int clipChanceNoHype = 15;
        public int clipCash = 25000;
        public int clipViewerBonus = 8;

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
            bankruptDebt = 180000;
            winDebtMax = 20000;
            winCashMin = 120000;
            winMembershipMin = 15;
            billRent = 10000;
            billElectricNet = 5000;
            billAvatarLicense = 4000;
            billFood = 6000;
            billGear = 3000;
            startingMembers = 8;
            membersHighPerfects = 8;
            membersHighGain = 2;
            membersLowPerfects = 4;
            membersLowGain = 1;
            membershipPassivePerMember = 400;
            membershipPitchSeconds = 1.2f;
            membershipPitchAtSeconds = 60f;
            pitchMemberBonus = 3;
            clipChanceWithHype = 35;
            clipChanceNoHype = 15;
            clipCash = 25000;
            clipViewerBonus = 8;
            extraThreats = ExtraThreatRules.DefaultWeek2Table();
        }
    }
}
