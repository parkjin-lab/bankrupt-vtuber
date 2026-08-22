using System;
using System.Collections.Generic;

namespace BankruptVtuber
{
    public class ChatNote
    {
        public ChatKind Kind;
        public string Text;
        public string User;
        public float SpawnTime;
        public float HitTime;
        public int SuperchatWon;
        public bool Consumed;
        public bool IsSuperchat;
    }

    public class StreamSession
    {
        public readonly Week1Balance Balance;
        public readonly ChatCatalog Catalog;
        public readonly Random Rng;

        public float TimeLeft;
        public float Elapsed;
        public float Viewers;
        public int Mental;
        public int PerfectCombo;
        public int Combo;
        public int PeakCombo;
        public int MissStreak;
        public int TotalMiss;
        public bool TotalMissPenaltyUsed;
        public float HypeLeft;
        public float IncomeRemainder;
        public int TickIncome;
        public int SuperchatIncome;
        public int Perfects, Greats, Goods, Misses;
        public bool ForceEnded;
        public bool Finished;
        public bool HadHype;

        public readonly List<ChatNote> Notes = new List<ChatNote>(64);

        float _nextChatAt;
        float _nextSuperchatAt;
        int _superchatsSpawned;
        int _superchatTarget;
        int _userSerial;

        static readonly string[] FakeUsers =
        {
            "별하", "네코링", "고정닉A", "야식요정", "민초파", "라떼는", "밤샘러", "후원요정",
            "트롤킹", "질문봇", "달콤이", "초롱이", "빚쟁이아님", "월세공포", "이모트창"
        };

        public StreamSession(Week1Balance balance, ChatCatalog catalog, int mental, int? seed = null)
        {
            Balance = balance;
            Catalog = catalog;
            Rng = seed.HasValue ? new Random(seed.Value) : new Random();
            TimeLeft = balance.streamSeconds;
            Viewers = balance.startingViewers;
            Mental = mental;
            _nextChatAt = 0.4f;
            _superchatTarget = Rng.Next(balance.superchatMinCount, balance.superchatMaxCount + 1);
            _nextSuperchatAt = NextSuperchatDelay();
        }

        public bool HypeActive => HypeLeft > 0f;

        public float IncomeMultiplier => StreamRules.IncomeMultiplier(PerfectCombo, HypeActive, Balance);

        public int LiveIncome => TickIncome + SuperchatIncome;

        public int PayoutIncome
        {
            get
            {
                int raw = LiveIncome;
                if (!ForceEnded)
                    return raw;
                return raw * Balance.forceEndIncomeNumerator / Math.Max(1, Balance.forceEndIncomeDenominator);
            }
        }

        public void Tick(float dt)
        {
            if (Finished)
                return;

            Elapsed += dt;
            TimeLeft -= dt;

            if (HypeActive)
            {
                HypeLeft -= dt;
                Viewers += Balance.hypeViewersPerSec * dt;
            }

            float mul = IncomeMultiplier;
            IncomeRemainder += Math.Floor(Viewers) * Balance.incomePerViewerPerSec * mul * dt;
            int gained = (int)Math.Floor(IncomeRemainder);
            if (gained > 0)
            {
                TickIncome += gained;
                IncomeRemainder -= gained;
            }

            MaybeSpawnRegular();
            MaybeSpawnSuperchat();

            ExpireMisses();

            if (Mental <= 0)
            {
                Mental = 0;
                ForceEnded = true;
                Finished = true;
                return;
            }

            if (TimeLeft <= 0f)
            {
                TimeLeft = 0f;
                ExpireAllRemaining();
                Finished = true;
            }
        }

        public bool TryHit(ChatKind kind, float now, bool hold)
        {
            if (Finished)
                return false;

            ChatNote best = null;
            float bestAbs = float.MaxValue;
            for (int i = 0; i < Notes.Count; i++)
            {
                var n = Notes[i];
                if (n.Consumed || n.Kind != kind)
                    continue;
                float abs = Math.Abs(now - n.HitTime);
                if (abs < bestAbs)
                {
                    bestAbs = abs;
                    best = n;
                }
            }

            if (best == null)
                return false;

            // Hold only consumes when the note is inside the Good window.
            if (hold && bestAbs > Balance.goodWindow)
                return false;

            if (bestAbs > Balance.goodWindow * 1.15f)
                return false;

            best.Consumed = true;
            var judgement = StreamRules.Judge(bestAbs, Balance);
            Resolve(best, judgement);
            return true;
        }

        void MaybeSpawnRegular()
        {
            if (Elapsed < _nextChatAt || TimeLeft < Balance.approachSeconds * 0.4f)
                return;

            float t = 1f - TimeLeft / Balance.streamSeconds;
            float interval = Lerp(Balance.chatSpawnStart, Balance.chatSpawnEnd, t);
            _nextChatAt = Elapsed + interval;

            var roll = Rng.NextDouble();
            ChatKind kind = roll < 0.50 ? ChatKind.Positive
                : roll < 0.78 ? ChatKind.Empathy
                : ChatKind.Laugh;

            SpawnNote(kind, superchat: false, 0);
        }

        void MaybeSpawnSuperchat()
        {
            if (_superchatsSpawned >= _superchatTarget)
                return;
            if (Elapsed < _nextSuperchatAt)
                return;
            if (TimeLeft < Balance.approachSeconds * 0.5f)
                return;

            int won = StreamRules.SuperchatAmount(HypeActive, Rng, Balance);
            SpawnNote(ChatKind.Thanks, superchat: true, won);
            _superchatsSpawned += 1;
            _nextSuperchatAt = Elapsed + NextSuperchatDelay();
        }

        void SpawnNote(ChatKind kind, bool superchat, int won)
        {
            Notes.Add(new ChatNote
            {
                Kind = kind,
                IsSuperchat = superchat,
                SuperchatWon = won,
                Text = Catalog.Pick(kind, Rng),
                User = FakeUsers[_userSerial++ % FakeUsers.Length],
                SpawnTime = Elapsed,
                HitTime = Elapsed + Balance.approachSeconds
            });
        }

        void ExpireMisses()
        {
            for (int i = 0; i < Notes.Count; i++)
            {
                var n = Notes[i];
                if (n.Consumed)
                    continue;
                if (Elapsed <= n.HitTime + Balance.goodWindow)
                    continue;
                n.Consumed = true;
                Resolve(n, Judgement.Miss);
            }
        }

        void ExpireAllRemaining()
        {
            for (int i = 0; i < Notes.Count; i++)
            {
                var n = Notes[i];
                if (n.Consumed)
                    continue;
                n.Consumed = true;
                Resolve(n, Judgement.Miss);
            }
        }

        void Resolve(ChatNote note, Judgement judgement)
        {
            var result = StreamRules.ApplyJudgement(
                judgement,
                ref PerfectCombo,
                ref MissStreak,
                ref TotalMiss,
                ref TotalMissPenaltyUsed,
                Balance);

            Viewers = StreamRules.ClampViewers(Viewers + result.ViewerDelta - result.ExtraViewerLoss, Balance);
            Mental += result.MentalDelta;
            if (Mental < 0)
                Mental = 0;
            if (Mental > Balance.maxMental)
                Mental = Balance.maxMental;

            if (result.ResetCombo)
                Combo = 0;
            else
            {
                Combo += 1;
                if (Combo > PeakCombo)
                    PeakCombo = Combo;
            }

            if (result.StartedHype)
            {
                HypeLeft = Balance.hypeSeconds;
                HadHype = true;
            }

            switch (judgement)
            {
                case Judgement.Perfect: Perfects++; break;
                case Judgement.Great: Greats++; break;
                case Judgement.Good: Goods++; break;
                default: Misses++; break;
            }

            if (judgement != Judgement.Miss && note.IsSuperchat)
                SuperchatIncome += note.SuperchatWon;

            LastJudgement = judgement;
            LastResolved = note;
        }

        public Judgement? LastJudgement;
        public ChatNote LastResolved;

        float NextSuperchatDelay()
        {
            double t = Rng.NextDouble();
            return (float)(Balance.superchatMinInterval + t * (Balance.superchatMaxInterval - Balance.superchatMinInterval));
        }

        static float Lerp(float a, float b, float t) => a + (b - a) * Clamp01(t);
        static float Clamp01(float t) => t < 0 ? 0 : t > 1 ? 1 : t;
    }
}
