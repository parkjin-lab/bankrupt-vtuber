namespace BankruptVtuber
{
    /// <summary>
    /// Mid-stream 콘서트 퍼포먼스 타이밍. A/S 성공, D/F 또는 시간초과는 배율 없음.
    /// Week 5's one new stream variable.
    /// </summary>
    public class ConcertPerformanceState
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
