using System;
using System.Collections.Generic;

namespace BankruptVtuber
{
    public class ChatNote
    {
        public ChatKind Kind;
        public string Text;
        public string User;
        public int Id;
        public float SpawnTime;
        public float HitTime;
        public int SuperchatWon;
        public bool Consumed;
        public bool IsSuperchat;
        public bool NamedFan;
        public bool FanWounded;
    }

    public class StreamSession
    {
        public readonly Week1Balance Balance;
        public readonly ChatCatalog Catalog;
        public readonly ContentBalance Content;
        public readonly ContentTuning Tuning;
        public readonly Random Rng;

        public float TimeLeft;
        public float Elapsed;
        public float Viewers;
        public int Mental;
        public int PerfectCombo;
        public int Combo;
        public int PeakCombo;
        public int MissStreak;
        public int PeakMissStreak;
        public bool HadSuccessfulSuperchat;
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
        public readonly StreamEventState Event = new StreamEventState();
        public readonly GoodsPromoState Promo = new GoodsPromoState();
        public readonly SponsorLineState Line = new SponsorLineState();
        public readonly ConcertPerformanceState Concert = new ConcertPerformanceState();
        public bool RivalActive;
        public float RivalViewers;
        public float PeakViewers;
        public float IncomeShieldLeft;
        public float IncomeFreezeLeft;
        public float ShieldViewers;

        public readonly List<ChatNote> Notes = new List<ChatNote>(64);

        float _nextChatAt;
        float _nextSuperchatAt;
        float _eventAt;
        int _superchatsSpawned;
        int _superchatTarget;
        int _userSerial;
        int _runSeed;
        bool _pendingHypeEvent;
        bool _pendingMissEvent;
        Week3Balance _week3;
        Week4Balance _week4;
        Week5Balance _week5;
        bool _promoEnabled;
        bool _lineEnabled;
        bool _concertEnabled;
        string _fanMinjun;
        string _fanHaeun;
        bool _minjunWounded;
        bool _haeunWounded;
        int _haeunHurtAt;
        bool _coachEnabled;
        bool _coachDone;
        int _coachSuccesses;
        int _coachPresented;
        float _coachElapsed;
        ChatNote _coachHeld;

        public const int CoachSuccessTarget = 3;
        public const float CoachSeconds = 8f;

        public StreamSession(
            Week1Balance balance,
            ChatCatalog catalog,
            int mental,
            float extraViewers = 0f,
            int? seed = null,
            ContentBalance content = null,
            StreamContentType contentType = StreamContentType.None)
        {
            Balance = balance;
            Catalog = catalog;
            Content = content;
            Tuning = ContentRules.Tuning(content, contentType);
            Rng = seed.HasValue ? new Random(seed.Value) : new Random();
            TimeLeft = balance.streamSeconds;
            Viewers = balance.startingViewers + extraViewers;
            PeakViewers = Viewers;
            Mental = mental;
            _nextChatAt = 0.4f;
            int extraSc = Tuning.ExtraSuperchat;
            if (extraSc < 0)
                extraSc = 0;
            _superchatTarget = Rng.Next(balance.superchatMinCount, balance.superchatMaxCount + 1) + extraSc;
            _nextSuperchatAt = NextSuperchatDelay();
            float earliest = balance.eventEarliestSeconds;
            float latest = balance.eventLatestSeconds;
            if (latest < earliest)
                latest = earliest;
            _eventAt = earliest + (float)Rng.NextDouble() * (latest - earliest);
            Event.Reset();
        }

        public bool EventActive => Event.Active;

        public bool PromoActive => Promo.Active;

        public bool LineActive => Line.Active;

        public bool ConcertActive => Concert.Active;

        public bool HypeActive => HypeLeft > 0f;

        public void EnableRival(Week3Balance w3)
        {
            if (w3 == null)
                return;
            _week3 = w3;
            RivalActive = true;
            RivalViewers = w3.rivalStartViewers;
        }

        public void EnablePromo(Week3Balance w3)
        {
            if (w3 == null)
                return;
            _week3 = w3;
            _promoEnabled = true;
            Promo.Reset();
            Promo.Window = w3.promoWindowSeconds > 0.2f ? w3.promoWindowSeconds : 1.2f;
        }

        public void EnableSponsorLine(Week4Balance w4)
        {
            if (w4 == null)
                return;
            _week4 = w4;
            _lineEnabled = true;
            Line.Reset();
            Line.Window = w4.lineWindowSeconds > 0.2f ? w4.lineWindowSeconds : 1.2f;
        }

        public void BindChatSeed(int runSeed)
        {
            _runSeed = runSeed;
        }

        public void BindNamedFans(string minjun, bool minjunWounded, string haeun, bool haeunWounded, int haeunHurtStreak = 0)
        {
            _fanMinjun = minjun;
            _fanHaeun = haeun;
            _minjunWounded = minjunWounded;
            _haeunWounded = haeunWounded;
            _haeunHurtAt = haeunHurtStreak;
        }

        public void EnableConcert(Week5Balance w5)
        {
            if (w5 == null)
                return;
            _week5 = w5;
            _concertEnabled = true;
            Concert.Reset();
            Concert.Window = w5.concertWindowSeconds > 0.2f ? w5.concertWindowSeconds : 1.2f;
        }

        public bool CoachActive => _coachEnabled && !_coachDone;

        public ChatNote CoachHeld => _coachHeld;

        public int CoachSuccesses => _coachSuccesses;

        public int CoachPresented => _coachPresented;

        public static bool ShouldOfferFirstStreamCoach(GameRunState run)
        {
            if (run == null)
                return false;
            if (run.day != 1)
                return false;
            if (run.streamDoneThisDay)
                return false;
            if (run.successfulStreams != 0)
                return false;
            if (run.lastStreamIncome != 0 || run.lastTickIncome != 0)
                return false;
            return true;
        }

        public void EnableFirstStreamCoach()
        {
            _coachEnabled = true;
            _coachDone = false;
            _coachSuccesses = 0;
            _coachPresented = 0;
            _coachElapsed = 0f;
            _coachHeld = null;
        }

        public float IncomeMultiplier =>
            StreamRules.IncomeMultiplier(PerfectCombo, HypeActive, Balance) * Tuning.IncomeMul;

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

            if (Event.Active)
            {
                FreezeNotes(dt);
                Event.TimeLeft -= dt;
                if (Event.TimeLeft <= 0f)
                    ResolveEvent(false);
            }

            if (Promo.Active)
            {
                FreezeNotes(dt);
                Promo.TimeLeft -= dt;
                if (Promo.TimeLeft <= 0f)
                    ResolvePromo(false);
            }

            if (Line.Active)
            {
                FreezeNotes(dt);
                Line.TimeLeft -= dt;
                if (Line.TimeLeft <= 0f)
                    ResolveLine(false);
            }

            if (Concert.Active)
            {
                FreezeNotes(dt);
                Concert.TimeLeft -= dt;
                if (Concert.TimeLeft <= 0f)
                    ResolveConcert(false);
            }

            if (RivalActive && _week3 != null)
            {
                RivalViewers += _week3.rivalViewersPerSec * dt;
                if (RivalViewers < 0f)
                    RivalViewers = 0f;
            }

            if (HypeActive)
            {
                HypeLeft -= dt;
                Viewers += Balance.hypeViewersPerSec * dt;
            }

            if (IncomeShieldLeft > 0f)
                IncomeShieldLeft -= dt;
            if (IncomeFreezeLeft > 0f)
                IncomeFreezeLeft -= dt;
            else
            {
                float viewersForIncome = Viewers;
                if (IncomeShieldLeft > 0f && ShieldViewers > viewersForIncome)
                    viewersForIncome = ShieldViewers;
                float mul = IncomeMultiplier;
                IncomeRemainder += MathF.Floor(viewersForIncome) * Balance.incomePerViewerPerSec * mul * dt;
                int gained = (int)Math.Floor(IncomeRemainder);
                if (gained > 0)
                {
                    TickIncome += gained;
                    IncomeRemainder -= gained;
                }
            }

            if (!Event.Active && !Promo.Active && !Line.Active && !Concert.Active)
            {
                MaybeSpawnRegular();
                MaybeSpawnSuperchat();
                ExpireMisses();
                MaybeStartEvent();
                MaybeStartPromo();
                MaybeStartLine();
                MaybeStartConcert();
            }

            TickCoach(dt);

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
                if (Event.Active)
                    ResolveEvent(false);
                if (Promo.Active)
                    ResolvePromo(false);
                if (Line.Active)
                    ResolveLine(false);
                if (Concert.Active)
                    ResolveConcert(false);
                ExpireAllRemaining();
                Finished = true;
            }

            if (Viewers > PeakViewers)
                PeakViewers = Viewers;
        }

        public bool TryHit(ChatKind kind, float now, bool hold)
        {
            if (Finished || Event.Active || Promo.Active || Line.Active || Concert.Active)
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
                return MissHeldCoach();

            // Hold only consumes when the note is inside the Good window.
            if (hold && bestAbs > Balance.goodWindow)
                return MissHeldCoach();

            if (bestAbs > Balance.goodWindow * 1.15f)
                return MissHeldCoach();

            best.Consumed = true;
            var judgement = StreamRules.Judge(bestAbs, Balance, Tuning.PerfectWindowMul);
            Resolve(best, judgement);
            return true;
        }

        void MaybeSpawnRegular()
        {
            if (Elapsed < _nextChatAt || TimeLeft < Balance.approachSeconds * 0.4f)
                return;

            float t = 1f - TimeLeft / Balance.streamSeconds;
            float interval = Lerp(Balance.chatSpawnStart, Balance.chatSpawnEnd, t) * Tuning.ChatSpawnMul;
            _nextChatAt = Elapsed + interval;

            SpawnNote(ContentRules.RollRegularKind(Tuning, Rng), superchat: false, 0);
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
            _userSerial += 1;
            int id = _userSerial;
            string user = ChatNicks.Pick(_runSeed, id);
            bool named = false;
            bool wounded = false;
            if (superchat && !string.IsNullOrEmpty(_fanMinjun) && _userSerial % 2 == 0)
            {
                user = _fanMinjun;
                named = true;
                wounded = _minjunWounded;
            }
            else if (!superchat && kind != ChatKind.Laugh && !string.IsNullOrEmpty(_fanHaeun) && _userSerial % 3 == 0)
            {
                user = _fanHaeun;
                named = true;
                wounded = _haeunWounded;
            }

            Notes.Add(new ChatNote
            {
                Kind = kind,
                IsSuperchat = superchat,
                SuperchatWon = won,
                Text = Catalog.Pick(kind, Rng),
                User = user,
                Id = id,
                NamedFan = named,
                FanWounded = wounded,
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

            float viewers = result.ViewerDelta;
            float extraLoss = result.ExtraViewerLoss;
            if (judgement == Judgement.Perfect)
                viewers *= Tuning.PerfectViewerMul;
            else if (judgement == Judgement.Miss)
            {
                viewers *= Tuning.MissViewerMul;
                extraLoss *= Tuning.MissViewerMul;
            }
            Viewers = StreamRules.ClampViewers(Viewers + viewers - extraLoss, Balance);
            ApplyRivalSteal(judgement);
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
                _pendingHypeEvent = true;
            }

            if (result.ExtraViewerLoss > 0)
                _pendingMissEvent = true;

            switch (judgement)
            {
                case Judgement.Perfect: Perfects++; break;
                case Judgement.Great: Greats++; break;
                case Judgement.Good: Goods++; break;
                default: Misses++; break;
            }

            if (judgement == Judgement.Miss)
            {
                PeakMissStreak = Math.Max(PeakMissStreak, MissStreak);
                if (!string.IsNullOrEmpty(_fanHaeun) && _haeunHurtAt > 0 && PeakMissStreak >= _haeunHurtAt)
                {
                    _haeunWounded = true;
                    for (int i = 0; i < Notes.Count; i++)
                    {
                        if (Notes[i].NamedFan && Notes[i].User == _fanHaeun)
                            Notes[i].FanWounded = true;
                    }
                }
            }
            else if (note.IsSuperchat)
            {
                SuperchatIncome += note.SuperchatWon;
                HadSuccessfulSuperchat = true;
            }

            LastJudgement = judgement;
            LastResolved = note;
            NoteCoachResolved(note, judgement);
        }

        void ApplyRivalSteal(Judgement judgement)
        {
            if (!RivalActive || _week3 == null)
                return;
            if (judgement == Judgement.Perfect)
            {
                Viewers = StreamRules.ClampViewers(Viewers + _week3.rivalPerfectSteal, Balance);
                RivalViewers -= _week3.rivalPerfectSteal;
                if (RivalViewers < 0f)
                    RivalViewers = 0f;
            }
            else if (judgement == Judgement.Miss)
            {
                Viewers = StreamRules.ClampViewers(Viewers - _week3.rivalMissSteal, Balance);
                RivalViewers += _week3.rivalMissSteal;
            }
        }

        public bool TryPromo(bool success)
        {
            if (!Promo.Active || Finished)
                return false;
            ResolvePromo(success);
            return true;
        }

        public bool TryEventKey(int key)
        {
            if (!Event.Active || Finished)
                return false;
            if (key < 1 || key > 4)
                return false;
            ResolveEvent(key == Event.TargetKey);
            return true;
        }

        void MaybeStartEvent()
        {
            if (Event.Fired || Event.Active || Finished)
                return;

            StreamEventTrigger trigger;
            if (_pendingMissEvent)
                trigger = StreamEventTrigger.FirstMissStreak;
            else if (_pendingHypeEvent)
                trigger = StreamEventTrigger.FirstHype;
            else if (Elapsed >= _eventAt)
                trigger = StreamEventTrigger.Scheduled;
            else
                return;

            _pendingHypeEvent = false;
            _pendingMissEvent = false;
            StartEvent(trigger);
        }

        void StartEvent(StreamEventTrigger trigger)
        {
            Event.Fired = true;
            Event.Active = true;
            Event.Resolved = false;
            Event.Trigger = trigger;
            Event.Window = Balance.eventWindowSeconds > 0.2f ? Balance.eventWindowSeconds : 1.15f;
            Event.TimeLeft = Event.Window;
            Event.TargetKey = Rng.Next(1, 5);
            if (trigger == StreamEventTrigger.FirstHype)
                Event.Kind = StreamEventKind.GearLag;
            else if (trigger == StreamEventTrigger.FirstMissStreak)
                Event.Kind = StreamEventKind.AntiWave;
            else
                Event.Kind = Rng.Next(0, 2) == 0 ? StreamEventKind.AntiWave : StreamEventKind.GearLag;
        }

        void ResolveEvent(bool success)
        {
            if (!Event.Active)
                return;

            Event.Active = false;
            Event.Resolved = true;
            Event.Success = success;
            Event.TimeLeft = 0f;

            if (Event.Kind == StreamEventKind.AntiWave)
            {
                if (success)
                    Viewers = StreamRules.ClampViewers(Viewers + Balance.eventAntiSuccessViewers, Balance);
                else
                {
                    Viewers = StreamRules.ClampViewers(Viewers - Balance.eventAntiFailViewers, Balance);
                    Mental -= Balance.eventAntiFailMental;
                    if (Mental < 0)
                        Mental = 0;
                }
            }
            else if (Event.Kind == StreamEventKind.GearLag)
            {
                if (success)
                {
                    IncomeShieldLeft = Balance.eventLagShieldSeconds;
                    ShieldViewers = Viewers;
                }
                else
                    IncomeFreezeLeft = Balance.eventLagFailFreezeSeconds;
            }
        }

        public bool TryLine(bool success)
        {
            if (!Line.Active || Finished)
                return false;
            ResolveLine(success);
            return true;
        }

        public bool TryConcert(bool success)
        {
            if (!Concert.Active || Finished)
                return false;
            ResolveConcert(success);
            return true;
        }

        void MaybeStartPromo()
        {
            if (!_promoEnabled || Promo.Fired || Promo.Active || Event.Active || Line.Active || Concert.Active || Finished)
                return;
            float fallback = _week3 != null ? _week3.promoFallbackSeconds : 55f;
            if (!HypeActive && Elapsed < fallback)
                return;
            StartPromo();
        }

        void StartPromo()
        {
            Promo.Fired = true;
            Promo.Active = true;
            Promo.Resolved = false;
            Promo.Success = false;
            Promo.TimeLeft = Promo.Window;
        }

        void ResolvePromo(bool success)
        {
            if (!Promo.Active)
                return;
            Promo.Active = false;
            Promo.Resolved = true;
            Promo.Success = success;
            Promo.TimeLeft = 0f;
        }

        void MaybeStartLine()
        {
            if (!_lineEnabled || Line.Fired || Line.Active || Event.Active || Promo.Active || Concert.Active || Finished)
                return;
            float fallback = _week4 != null ? _week4.lineFallbackSeconds : 55f;
            if (!HypeActive && Elapsed < fallback)
                return;
            StartLine();
        }

        void StartLine()
        {
            Line.Fired = true;
            Line.Active = true;
            Line.Resolved = false;
            Line.Success = false;
            Line.TimeLeft = Line.Window;
        }

        void ResolveLine(bool success)
        {
            if (!Line.Active)
                return;
            Line.Active = false;
            Line.Resolved = true;
            Line.Success = success;
            Line.TimeLeft = 0f;
        }

        void MaybeStartConcert()
        {
            if (!_concertEnabled || Concert.Fired || Concert.Active || Event.Active || Promo.Active || Line.Active || Finished)
                return;
            float fallback = _week5 != null ? _week5.concertFallbackSeconds : 55f;
            if (!HypeActive && Elapsed < fallback)
                return;
            StartConcert();
        }

        void StartConcert()
        {
            Concert.Fired = true;
            Concert.Active = true;
            Concert.Resolved = false;
            Concert.Success = false;
            Concert.TimeLeft = Concert.Window;
        }

        void ResolveConcert(bool success)
        {
            if (!Concert.Active)
                return;
            Concert.Active = false;
            Concert.Resolved = true;
            Concert.Success = success;
            Concert.TimeLeft = 0f;
        }

        void TickCoach(float dt)
        {
            if (!_coachEnabled || _coachDone || Finished)
                return;

            _coachElapsed += dt;
            if (_coachElapsed >= CoachSeconds || _coachSuccesses >= CoachSuccessTarget)
            {
                EndCoach();
                return;
            }

            if (Event.Active || Promo.Active || Line.Active || Concert.Active)
                return;

            if (_coachHeld != null && _coachHeld.Consumed)
                _coachHeld = null;

            if (_coachHeld == null)
                TryGrabCoachNote();

            if (_coachHeld != null && !_coachHeld.Consumed)
            {
                FreezeNotes(dt);
                _coachHeld.HitTime = Elapsed;
            }
        }

        void TryGrabCoachNote()
        {
            ChatNote best = null;
            float bestHit = float.MaxValue;
            for (int i = 0; i < Notes.Count; i++)
            {
                var n = Notes[i];
                if (n.Consumed)
                    continue;
                if (Elapsed + 0.16f < n.HitTime)
                    continue;
                if (n.HitTime < bestHit)
                {
                    bestHit = n.HitTime;
                    best = n;
                }
            }

            if (best == null)
                return;

            _coachHeld = best;
            _coachPresented += 1;
            best.HitTime = Elapsed;
        }

        public bool MissHeldCoach()
        {
            if (_coachHeld == null || _coachHeld.Consumed || _coachDone)
                return false;
            var n = _coachHeld;
            n.Consumed = true;
            _coachHeld = null;
            Resolve(n, Judgement.Miss);
            return true;
        }

        void NoteCoachResolved(ChatNote note, Judgement judgement)
        {
            if (!_coachEnabled || _coachDone)
                return;
            if (_coachHeld == note)
                _coachHeld = null;
            if (judgement == Judgement.Miss)
                return;
            _coachSuccesses += 1;
            if (_coachSuccesses >= CoachSuccessTarget)
                EndCoach();
        }

        void EndCoach()
        {
            _coachDone = true;
            _coachHeld = null;
        }

        void FreezeNotes(float dt)
        {
            for (int i = 0; i < Notes.Count; i++)
            {
                var n = Notes[i];
                if (n.Consumed)
                    continue;
                n.SpawnTime += dt;
                n.HitTime += dt;
            }
        }

        public Judgement? LastJudgement;
        public ChatNote LastResolved;

        float NextSuperchatDelay()
        {
            double t = Rng.NextDouble();
            float raw = (float)(Balance.superchatMinInterval + t * (Balance.superchatMaxInterval - Balance.superchatMinInterval));
            float mul = Tuning.SuperchatIntervalMul > 0f ? Tuning.SuperchatIntervalMul : 1f;
            return raw * mul;
        }

        static float Lerp(float a, float b, float t) => a + (b - a) * Clamp01(t);
        static float Clamp01(float t) => t < 0 ? 0 : t > 1 ? 1 : t;
    }
}
