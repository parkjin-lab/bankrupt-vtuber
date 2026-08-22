namespace BankruptVtuber
{
    /// <summary>
    /// Mid-stream 멤버십 유도. A/S 권유, D/F 스킵. Not a 1–4 event.
    /// </summary>
    public class MembershipPitchState
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
