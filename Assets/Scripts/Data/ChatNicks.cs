namespace BankruptVtuber
{
    /// <summary>
    /// Fake chat nicks for falling notes. Presentation only. 민준/하은 stay named fans.
    /// </summary>
    public static class ChatNicks
    {
        public static readonly string[] Pool =
        {
            "밤샌사람",
            "월세토끼",
            "정산요정",
            "ㄹㅇ팬",
            "빚쟁이형",
            "야식요정",
            "고정닉",
            "민초파",
            "후원요정",
            "달콤이",
            "초롱이",
            "월세공포",
            "네코링",
            "별하",
            "질문봇",
            "트롤킹",
            "라떼는",
            "밤샘러",
            "이모트창",
            "청구요정"
        };

        public static string Pick(int runSeed, int noteId)
        {
            if (Pool == null || Pool.Length == 0)
                return "고정닉";
            int mix = unchecked(runSeed * 397 ^ noteId * 1009);
            if (mix < 0)
                mix = -mix;
            return Pool[mix % Pool.Length];
        }
    }
}
