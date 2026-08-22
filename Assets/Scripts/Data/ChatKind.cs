namespace BankruptVtuber
{
    /// <summary>A/S/D/F mapped chat types. Gold/superchat is Thanks.</summary>
    public enum ChatKind
    {
        Positive = 0, // A 긍정 / blue normal
        Empathy = 1,  // S 공감 / green question
        Laugh = 2,    // D 웃음 / red troll
        Thanks = 3    // F 감사 + Space / gold superchat
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
        Bankrupt,
        WeekFailed
    }
}
