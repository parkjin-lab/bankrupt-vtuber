using UnityEngine;
using UnityEngine.SceneManagement;

namespace BankruptVtuber
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public Week1Balance Balance { get; private set; }
        public ChatCatalog Catalog { get; private set; }
        public GameRunState Run { get; private set; }

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
            Catalog = ChatCatalog.Load();
            if (Catalog.positive == null || Catalog.positive.Length == 0)
                Catalog.ApplyDefaults();
            Run = new GameRunState();
            Run.ResetNewRun(Balance);
            Debug.Log("[파산 버튜버] Week 1 boot — cash=" + Run.cash + " debt=" + Run.debt + " mental=" + Run.mental);
        }

        public void RestartRun()
        {
            Run.ResetNewRun(Balance);
            Load(SceneFlow.WeekStart);
        }

        public void GoLive()
        {
            if (!Run.billsAppliedThisDay)
                EconomyRules.ApplyDailyBills(Run, Balance);
            Load(SceneFlow.LiveStream);
        }

        public void GoSettlement()
        {
            Load(SceneFlow.Settlement);
        }

        public void NextMorning()
        {
            Run.BeginNextDay(Balance);
            Load(SceneFlow.WeekStart);
        }

        public void Load(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
