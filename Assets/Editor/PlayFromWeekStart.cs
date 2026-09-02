#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BankruptVtuber.Editor
{
    [InitializeOnLoad]
    public static class PlayFromWeekStart
    {
        const string TitlePath = "Assets/Scenes/Title.unity";
        const string WeekStartPath = "Assets/Scenes/WeekStart.unity";
        const string LivePath = "Assets/Scenes/LiveStream.unity";
        const string SettlePath = "Assets/Scenes/Settlement.unity";

        static PlayFromWeekStart()
        {
            EditorApplication.delayCall += Apply;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
                EditorApplication.delayCall += FocusGameView;
        }

        static void FocusGameView()
        {
            var t = typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");
            if (t == null)
                return;
            var w = EditorWindow.GetWindow(t, false);
            if (w != null)
                w.Focus();
        }

        static void Apply()
        {
            var title = AssetDatabase.LoadAssetAtPath<SceneAsset>(TitlePath);
            if (title != null)
                EditorSceneManager.playModeStartScene = title;

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(TitlePath, true),
                new EditorBuildSettingsScene(WeekStartPath, true),
                new EditorBuildSettingsScene(LivePath, true),
                new EditorBuildSettingsScene(SettlePath, true)
            };
        }

        [MenuItem("파산 버튜버/Verify Week 1 Hookup")]
        public static void Verify()
        {
            var missing = false;
            foreach (var path in new[] { TitlePath, WeekStartPath, LivePath, SettlePath })
            {
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null)
                {
                    Debug.LogError("[파산 버튜버] missing scene " + path);
                    missing = true;
                }
            }

            if (Resources.Load<BankruptVtuber.Week1Balance>("Balance/Week1Balance") == null)
            {
                Debug.LogError("[파산 버튜버] missing Resources/Balance/Week1Balance");
                missing = true;
            }

            if (Resources.Load<BankruptVtuber.Week2Balance>("Balance/Week2Balance") == null)
            {
                Debug.LogError("[파산 버튜버] missing Resources/Balance/Week2Balance");
                missing = true;
            }

            if (Resources.Load<BankruptVtuber.Week3Balance>("Balance/Week3Balance") == null)
            {
                Debug.LogError("[파산 버튜버] missing Resources/Balance/Week3Balance");
                missing = true;
            }

            if (Resources.Load<BankruptVtuber.Week4Balance>("Balance/Week4Balance") == null)
            {
                Debug.LogError("[파산 버튜버] missing Resources/Balance/Week4Balance");
                missing = true;
            }

            if (Resources.Load<BankruptVtuber.Week5Balance>("Balance/Week5Balance") == null)
            {
                Debug.LogError("[파산 버튜버] missing Resources/Balance/Week5Balance");
                missing = true;
            }

            if (Resources.Load<BankruptVtuber.FandomBalance>("Balance/FandomBalance") == null)
            {
                Debug.LogError("[파산 버튜버] missing Resources/Balance/FandomBalance");
                missing = true;
            }

            if (Resources.Load<BankruptVtuber.ContentBalance>("Balance/ContentBalance") == null)
            {
                Debug.LogError("[파산 버튜버] missing Resources/Balance/ContentBalance");
                missing = true;
            }

            if (!BankruptVtuber.RunSave.DummyRoundTrip())
            {
                Debug.LogError("[파산 버튜버] save dummy roundtrip failed");
                missing = true;
            }

            if (!missing)
                Debug.Log("[파산 버튜버] Week 1 scenes + balance hooked. Start scene = Title.");
        }

        [MenuItem("파산 버튜버/DEBUG 오늘 스킵 (F10)")]
        public static void DebugSkipDay()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[파산 버튜버] DEBUG skip needs Play Mode.");
                return;
            }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            BankruptVtuber.PlaytestDebug.SkipRestOfDay();
#endif
        }

        [MenuItem("파산 버튜버/DEBUG 다음 주 점프 (F9)")]
        public static void DebugSkipWeek()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[파산 버튜버] DEBUG skip needs Play Mode.");
                return;
            }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            BankruptVtuber.PlaytestDebug.SkipToNextWeek();
#endif
        }
    }
}
#endif
