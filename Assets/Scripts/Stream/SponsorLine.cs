namespace BankruptVtuber
{
    /// <summary>
    /// Mid-stream 스폰서 멘트 타이밍. ←/↑ 성공, →/↓ 또는 시간초과는 실패.
    /// Week 4's one new stream variable.
    /// </summary>
    public class SponsorLineState
    {
        public bool Active;
        public bool Fired;
        public bool Resolved;
        public bool Success;
        public float TimeLeft;
        public float Window = 1.2f;

        public void Reset()
        {
            Active = false;
            Fired = false;
            Resolved = false;
            Success = false;
            TimeLeft = 0f;
            Window = 1.2f;
        }
    }
}
