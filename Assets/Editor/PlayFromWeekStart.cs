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

            if (!missing)
                Debug.Log("[파산 버튜버] Week 1 scenes + balance hooked. Start scene = Title.");
        }
    }
}
#endif
