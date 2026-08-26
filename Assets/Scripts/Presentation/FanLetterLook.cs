namespace BankruptVtuber
{
    /// <summary>
    /// In-character 팬레터 copy for already-present 민준 / 하은.
    /// Does not change loyalty, mental, or ignore counts.
    /// </summary>
    public readonly struct FanLetterLook
    {
        public readonly string From;
        public readonly string Tag;
        public readonly string Body;
        public readonly bool Cold;

        public FanLetterLook(string from, string tag, string body, bool cold)
        {
            From = from ?? "";
            Tag = tag ?? "";
            Body = body ?? "";
            Cold = cold;
        }

        public static FanLetterLook For(GameRunState run, FandomBalance f)
        {
            string minjun = f != null ? f.minjunName : "민준";
            string haeun = f != null ? f.haeunName : "하은";
            bool hasMinjun = run != null && run.minjunPresent;
            bool hasHaeun = run != null && run.haeunPresent;
            bool minjunCold = hasMinjun && run.minjunIgnoreSettlements > 0;
            bool haeunCold = hasHaeun && run.haeunHurtThisDay;

            if (hasMinjun && hasHaeun)
            {
                bool cold = minjunCold || haeunCold;
                return new FanLetterLook(
                    $"{haeun} · {minjun}",
                    "매일 오는 야간  ·  첫 도네",
                    cold
                        ? $"{haeun}: 오늘은 짧게만.\n{minjun}: 답이 없어서 이것만 남겨요."
                        : $"{haeun}: 오늘 야간도 잘 들었어요. 밥 먹고 자요.\n{minjun}: 슈퍼챗 타이밍 맞아서 다행이에요. 내일도 켤 거죠?\n둘 다 내일 채팅에 있을게요.",
                    cold);
            }

            if (hasHaeun)
            {
                return new FanLetterLook(
                    haeun,
                    "매일 오는 야간",
                    haeunCold
                        ? "오늘 채팅이 좀 아팠어요.\n그래도 편지 남길게요.\n답 없어도… 알겠어요."
                        : "오늘도 야간 들어왔어요.\n웃음 키 맞출 때 좀 웃겼어요.\n밥은 먹었어요? 정산 보고 자면 안 돼요.\n내일 밤에 또 올게.",
                    haeunCold);
            }

            if (hasMinjun)
            {
                return new FanLetterLook(
                    minjun,
                    "첫 도네",
                    minjunCold
                        ? "오늘은 답이 없더라고요.\n그래도 보냈어요. 짧게요."
                        : "오늘 슈퍼챗 보낸 거, 타이밍이 맞아서 다행이에요.\n방송 끝날 때까지 채팅창 맨 위에 있었어요.\n내일도 켤 거죠? 월세는 제가 응원으로 메울게요.\n조금만 더 버텨요.",
                    minjunCold);
            }

            return new FanLetterLook("", "", "", false);
        }
    }
}
