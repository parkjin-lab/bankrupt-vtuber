using UnityEngine;

namespace BankruptVtuber
{
    [CreateAssetMenu(fileName = "ChatCatalog", menuName = "파산 버튜버/Chat Catalog")]
    public class ChatCatalog : ScriptableObject
    {
        [TextArea] public string[] positive;
        [TextArea] public string[] empathy;
        [TextArea] public string[] laugh;
        [TextArea] public string[] thanks;

        public static ChatCatalog Load()
        {
            var asset = Resources.Load<ChatCatalog>("Balance/ChatCatalog");
            if (asset != null && HasLines(asset))
                return asset;

            asset = CreateInstance<ChatCatalog>();
            asset.ApplyDefaults();
            return asset;
        }

        static bool HasLines(ChatCatalog c) =>
            c.positive != null && c.positive.Length > 0 &&
            c.empathy != null && c.empathy.Length > 0 &&
            c.laugh != null && c.laugh.Length > 0 &&
            c.thanks != null && c.thanks.Length > 0;

        public void ApplyDefaults()
        {
            positive = new[]
            {
                "오늘 컨디션 좋아 보여요!",
                "안녕하세요~ 들어왔어요",
                "고정닉 출석합니다",
                "배경 너무 귀여워요",
                "목소리 힐돼요",
                "이모트 폭탄 가즈아",
                "썸네일 보고 클릭함",
                "오늘도 화이팅이에요"
            };
            empathy = new[]
            {
                "저녁 뭐 드셨어요?",
                "요즘 제일 힘든 거 뭐예요?",
                "다음 컨텐츠 뭐예요?",
                "노래 한 곡만 해주실 수 있어요?",
                "수면 시간은 괜찮아요?",
                "부채 괜찮아요…? 걱정됨",
                "아바타 라이선스 비싸요?",
                "오늘 목표 시청자 몇 명이에요?"
            };
            laugh = new[]
            {
                "구독 취소함 ㅋ",
                "재미없는데요",
                "다른 방 가는 중",
                "목소리 작다",
                "돈 벌 생각은 있음?",
                "채팅 읽기는 하냐",
                "광고만 나와라",
                "저 방이 더 나음"
            };
            thanks = new[]
            {
                "밥 챙겨 먹어요!!",
                "이번 달 월세 보태세요",
                "응원합니다 화이팅",
                "장비 업글 하세요",
                "멘탈 지키세요",
                "오늘 정산 꼭 남기세요",
                "슈퍼챗으로 전기세 냄",
                "파산만은 안 돼"
            };
        }

        public string Pick(ChatKind kind, System.Random rng)
        {
            var arr = kind switch
            {
                ChatKind.Positive => positive,
                ChatKind.Empathy => empathy,
                ChatKind.Laugh => laugh,
                _ => thanks
            };
            if (arr == null || arr.Length == 0)
                return "…";
            return arr[rng.Next(arr.Length)];
        }
    }
}
