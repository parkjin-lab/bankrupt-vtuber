namespace BankruptVtuber
{
    public static class SceneFlow
    {
        public const string WeekStart = "WeekStart";
        public const string LiveStream = "LiveStream";
        public const string Settlement = "Settlement";

        public static readonly string[] BuildOrder =
        {
            WeekStart,
            LiveStream,
            Settlement
        };
    }
}
