using UnityEngine;
using UnityEngine.SceneManagement;

namespace BankruptVtuber
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public Week1Balance Balance { get; private set; }
        public Week2Balance Week2 { get; private set; }
        public Week3Balance Week3 { get; private set; }
        public Week4Balance Week4 { get; private set; }
        public Week5Balance Week5 { get; private set; }
        public FandomBalance Fandom { get; private set; }
        public ContentBalance Content { get; private set; }
        public ChatCatalog Catalog { get; private set; }
        public GameRunState Run { get; private set; }

        /// <summary>Play-session flag. Survives Restart so the prologue is once per Play.</summary>
        public bool PrologueSeenThisSession { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Boot()
        {
            if (Instance != null)
                return;

            var go = new GameObject("GameManager");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<GameManager>();
            Instance.Initialize();
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            if (Run == null)
                Initialize();
        }

        void Initialize()
        {
            Balance = Week1Balance.Load();
            Week2 = Week2Balance.Load();
            Week3 = Week3Balance.Load();
            Week4 = Week4Balance.Load();
            Week5 = Week5Balance.Load();
            Fandom = FandomBalance.Load();
            Content = ContentBalance.Load();
            Catalog = ChatCatalog.Load();
            if (Catalog.positive == null || Catalog.positive.Length == 0)
                Catalog.ApplyDefaults();
            Run = new GameRunState();
            Run.ResetNewRun(Balance);
            FandomRules.Reset(Run, Balance, Fandom);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            PlaytestDebug.Attach(this);
#endif
            Debug.Log("[파산 버튜버] Week 1 boot seed=" + Run.runSeed + " cash=" + Run.cash + " debt=" + Run.debt + " mental=" + Run.mental);
        }

        public bool ShouldPlayPrologue()
        {
            return Run != null && Run.day == 1 && !Run.billsAppliedThisDay && !PrologueSeenThisSession;
        }

        public void MarkPrologueSeen()
        {
            PrologueSeenThisSession = true;
        }

        public void GoTitle()
        {
            Load(SceneFlow.Title);
        }

        public void GoWeekStart()
        {
            Load(SceneFlow.WeekStart);
        }

        public void RestartRun()
        {
            StartNewRun();
            Debug.Log("[파산 버튜버] new run seed=" + Run.runSeed);
            Load(SceneFlow.Title);
        }

        public void StartNewRun()
        {
            RunSave.Delete();
            Run.ResetNewRun(Balance);
            FandomRules.Reset(Run, Balance, Fandom);
        }

        public bool ContinueRun()
        {
            if (!RunSave.TryLoad(Run))
                return false;
            MarkPrologueSeen();
            ResumeFromSave();
            return true;
        }

        public void ResumeFromSave()
        {
            if (Run != null && Run.streamDoneThisDay)
                Load(SceneFlow.Settlement);
            else
                Load(SceneFlow.WeekStart);
        }

        public void SaveRun()
        {
            RunSave.Write(Run);
        }

        public void PrepareSettlement()
        {
            if (Run == null)
                return;
            Week2Rules.ApplyMembershipPassive(Run, Week2);
            Week3Rules.TryUnlockGoods(Run, Week3);
            Week3Rules.ApplyGoodsSales(Run, Week3);
            Week4Rules.ApplyJuniorDaily(Run, Week4);
            Week4Rules.ApplySponsorDaily(Run, Week4);
            Week5Rules.ApplyRanking(Run, Week5);
            Week5Rules.ApplyConcertResult(Run, Balance, Week5);
            Week5Rules.NoteZeroMentalDay(Run);
        }

        public void GoLive()
        {
            if (FandomRules.MustResolveConflict(Run))
                return;
            if (ContentRules.MustPick(Run))
                return;
            ExtraThreatRules.EnsureRolled(Run, Balance, Week2, Week3, Week4, Week5);
            Week3Rules.TryUnlockGoods(Run, Week3);
            if (!Run.billsAppliedThisDay)
            {
                EconomyRules.ApplyDailyBills(Run, Balance, Week2, Week3, Week4, Week5, Fandom);
                SaveRun();
            }
            Load(SceneFlow.LiveStream);
        }

        public void GoSettlement()
        {
            PrepareSettlement();
            SaveRun();
            Load(SceneFlow.Settlement);
        }

        public void NextMorning()
        {
            if (FandomRules.MustResolveConflict(Run))
                return;
            FandomRules.ResolveEndOfDay(Run, Fandom);
            Run.BeginNextDay(Balance, Week2, Week3, Week4, Week5, Fandom);
            SaveRun();
            Load(SceneFlow.WeekStart);
        }

        public void Load(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
