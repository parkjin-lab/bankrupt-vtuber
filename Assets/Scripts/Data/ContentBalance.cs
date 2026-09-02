using UnityEngine;

namespace BankruptVtuber
{
    /// <summary>
    /// Locked stream content-type retunes. Does not change Week 1–5 bills or win numbers.
    /// </summary>
    [CreateAssetMenu(fileName = "ContentBalance", menuName = "파산 버튜버/Content Balance")]
    public class ContentBalance : ScriptableObject
    {
        [Header("토크")]
        public string talkName = "토크";
        public float talkIncomeMultiplier = 1.0f;
        public int talkMentalCost = 6;
        public int talkPositiveWeight = 55;
        public int talkEmpathyWeight = 35;
        public int talkLaughWeight = 10;
        public int talkT0toT1 = 1;
        public int talkT1toT2 = 1;

        [Header("게임")]
        public string gameName = "게임";
        public float gameIncomeMultiplier = 1.15f;
        public int gameMentalCost = 10;
        public int gamePositiveWeight = 28;
        public int gameEmpathyWeight = 22;
        public int gameLaughWeight = 50;
        public float gamePerfectViewerMul = 1.4f;
        public float gameMissViewerMul = 1.35f;

        [Header("노래")]
        public string songName = "노래";
        public float songIncomeMultiplier = 1.1f;
        public int songMentalCost = 8;
        public int songPositiveWeight = 40;
        public int songEmpathyWeight = 25;
        public int songLaughWeight = 35;
        public float songPerfectWindowMul = 0.85f;
        public float songSuperchatIntervalMul = 0.75f;
        public int songExtraSuperchat = 2;

        [Header("리액션")]
        public string reactionName = "리액션";
        public float reactionIncomeMultiplier = 0.9f;
        public int reactionMentalCost = 4;
        public int reactionPositiveWeight = 45;
        public int reactionEmpathyWeight = 35;
        public int reactionLaughWeight = 20;
        public float reactionChatSpawnMul = 1.35f;
        public int reactionLoyalty = 2;
        public int reactionMissMax = 8;

        public static ContentBalance Load()
        {
            var asset = Resources.Load<ContentBalance>("Balance/ContentBalance");
            if (asset != null)
                return asset;

            asset = CreateInstance<ContentBalance>();
            asset.ApplyLockedContentDefaults();
            return asset;
        }

        public void ApplyLockedContentDefaults()
        {
            talkName = "토크";
            talkIncomeMultiplier = 1.0f;
            talkMentalCost = 6;
            talkPositiveWeight = 55;
            talkEmpathyWeight = 35;
            talkLaughWeight = 10;
            talkT0toT1 = 1;
            talkT1toT2 = 1;
            gameName = "게임";
            gameIncomeMultiplier = 1.15f;
            gameMentalCost = 10;
            gamePositiveWeight = 28;
            gameEmpathyWeight = 22;
            gameLaughWeight = 50;
            gamePerfectViewerMul = 1.4f;
            gameMissViewerMul = 1.35f;
            songName = "노래";
            songIncomeMultiplier = 1.1f;
            songMentalCost = 8;
            songPositiveWeight = 40;
            songEmpathyWeight = 25;
            songLaughWeight = 35;
            songPerfectWindowMul = 0.85f;
            songSuperchatIntervalMul = 0.75f;
            songExtraSuperchat = 2;
            reactionName = "리액션";
            reactionIncomeMultiplier = 0.9f;
            reactionMentalCost = 4;
            reactionPositiveWeight = 45;
            reactionEmpathyWeight = 35;
            reactionLaughWeight = 20;
            reactionChatSpawnMul = 1.35f;
            reactionLoyalty = 2;
            reactionMissMax = 8;
        }
    }
}
