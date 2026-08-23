namespace BankruptVtuber
{
    /// <summary>Arrow-mapped chat types. Gold/superchat is Thanks.</summary>
    public enum ChatKind
    {
        Positive = 0, // ← 긍정 / blue normal
        Empathy = 1,  // ↓ 공감 / green question
        Laugh = 2,    // → 웃음 / red troll
        Thanks = 3    // ↑ 감사 + Space / gold superchat
    }

    public enum Judgement
    {
        Perfect,
        Great,
        Good,
        Miss
    }

    public enum WeekOutcome
    {
        Continue,
        Win,
        Week2Win,
        Week3Win,
        Week4Win,
        Ending,
        Bankrupt,
        WeekFailed
    }

    public enum EndingKind
    {
        None,
        Bankrupt,
        Burnout,
        SoloLegend,
        AgencyEmpire,
        RetireProducer,
        Nameless
    }

    /// <summary>WeekStart content pick. Retunes the existing arrow-key stream.</summary>
    public enum StreamContentType
    {
        None = 0,
        Talk = 1,
        Game = 2,
        Song = 3,
        Reaction = 4
    }
}
