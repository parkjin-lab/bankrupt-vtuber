#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BankruptVtuber.Editor
{
    [InitializeOnLoad]
    public static class PlayFromWeekStart
    {
        const string WeekStartPath = "Assets/Scenes/WeekStart.unity";
        const string LivePath = "Assets/Scenes/LiveStream.unity";
        const string SettlePath = "Assets/Scenes/Settlement.unity";

        static PlayFromWeekStart()
        {
            EditorApplication.delayCall += Apply;
        }

        static void Apply()
        {
            var week = AssetDatabase.LoadAssetAtPath<SceneAsset>(WeekStartPath);
            if (week != null)
                EditorSceneManager.playModeStartScene = week;

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(WeekStartPath, true),
                new EditorBuildSettingsScene(LivePath, true),
                new EditorBuildSettingsScene(SettlePath, true)
            };
        }

        [MenuItem("파산 버튜버/Verify Week 1 Hookup")]
        public static void Verify()
        {
            var missing = false;
            foreach (var path in new[] { WeekStartPath, LivePath, SettlePath })
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

            if (!missing)
                Debug.Log("[파산 버튜버] Week 1 scenes + balance hooked. Start scene = WeekStart.");
        }
    }
}
#endif
