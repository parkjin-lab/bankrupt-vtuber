namespace BankruptVtuber
{
    public enum StreamEventKind
    {
        None = 0,
        AntiWave = 1,
        GearLag = 2
    }

    public enum StreamEventTrigger
    {
        Scheduled = 0,
        FirstHype = 1,
        FirstMissStreak = 2
    }

    public class StreamEventState
    {
        public StreamEventKind Kind;
        public StreamEventTrigger Trigger;
        public bool Active;
        public bool Fired;
        public bool Resolved;
        public bool Success;
        public int TargetKey;
        public float TimeLeft;
        public float Window;

        public void Reset()
        {
            Kind = StreamEventKind.None;
            Trigger = StreamEventTrigger.Scheduled;
            Active = false;
            Fired = false;
            Resolved = false;
            Success = false;
            TargetKey = 1;
            TimeLeft = 0f;
            Window = 0f;
        }

        public static string DisplayName(StreamEventKind kind) => kind switch
        {
            StreamEventKind.AntiWave => "안티 웨이브",
            StreamEventKind.GearLag => "장비 렉",
            _ => ""
        };

        public static string Prompt(StreamEventKind kind) => kind switch
        {
            StreamEventKind.AntiWave => "채팅창이 테러 당한다! 빛나는 키로 막아!",
            StreamEventKind.GearLag => "캡처보드가 멈춘다! 빛나는 키로 재시작!",
            _ => ""
        };

        public static string SuccessCopy(StreamEventKind kind) => kind switch
        {
            StreamEventKind.AntiWave => "방어 성공 — 시청자 회복",
            StreamEventKind.GearLag => "재연결 — 수익 보호막",
            _ => ""
        };

        public static string FailCopy(StreamEventKind kind) => kind switch
        {
            StreamEventKind.AntiWave => "방어 실패 — 시청자·멘탈 타격",
            StreamEventKind.GearLag => "송출 끊김 — 3초 무수익",
            _ => ""
        };
    }
}
