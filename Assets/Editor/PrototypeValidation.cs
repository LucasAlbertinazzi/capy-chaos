using System.IO;
using CapyChaos.Core;
using CapyChaos.Environment;
using CapyChaos.Gameplay;
using CapyChaos.Input;
using CapyChaos.Rendering;
using CapyChaos.Services;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace CapyChaos.Editor
{
    public static class PrototypeValidation
    {
        private const string DifficultyAssetPath = "Assets/Resources/Config/RunDifficultySettings.asset";
        private const string MobileValidationSessionKey = "CapyChaos.RunResultsValidationV4";

        [InitializeOnLoadMethod]
        private static void ValidateMobileGameplayAfterReload()
        {
            EditorApplication.delayCall += () =>
            {
                if (SessionState.GetBool(MobileValidationSessionKey, false))
                {
                    return;
                }

                EnsureDifficultySettingsAsset();
                RunAll();
                SessionState.SetBool(MobileValidationSessionKey, true);
            };
        }

        [MenuItem("CAPY CHAOS/Criar configuração de dificuldade")]
        public static void CreateDifficultySettingsAsset()
        {
            RunDifficultySettings settings = EnsureDifficultySettingsAsset();
            Selection.activeObject = settings;
            EditorGUIUtility.PingObject(settings);
        }

        [MenuItem("CAPY CHAOS/Validar Protótipo")]
        public static void RunAll()
        {
            SessionState.SetBool("CapyChaos.ValidationPassed", false);
            Assert.AreEqual(-1, RunnerRules.ClampLane(-4));
            Assert.AreEqual(0, RunnerRules.ClampLane(0));
            Assert.AreEqual(1, RunnerRules.ClampLane(4));
            Assert.AreEqual(142, RunnerRules.CalculateScore(102.9f, 4));

            Assert.AreEqual(BrazilTheme.SaoPaulo, ThemeSequence.GetTheme(0));
            Assert.AreEqual(BrazilTheme.SaoPaulo, ThemeSequence.GetTheme(2));
            Assert.AreEqual(BrazilTheme.RioDeJaneiro, ThemeSequence.GetTheme(3));
            Assert.AreEqual(BrazilTheme.Brasilia, ThemeSequence.GetTheme(6));
            Assert.AreEqual(BrazilTheme.Amazonia, ThemeSequence.GetTheme(9));
            Assert.AreEqual(BrazilTheme.SaoPaulo, ThemeSequence.GetTheme(12));
            Assert.AreEqual(0, ThemeSequence.GetVariant(0));
            Assert.AreEqual(1, ThemeSequence.GetVariant(12));

            RunDifficultySettings difficultySettings = AssetDatabase.LoadAssetAtPath<RunDifficultySettings>(DifficultyAssetPath);
            Assert.IsNotNull(difficultySettings);
            RunDifficultyTuning standardTuning = difficultySettings.Standard;
            Assert.AreEqual("PADRÃO", standardTuning.DisplayName);
            Assert.AreApproximatelyEqual(7.5f, standardTuning.InitialSpeed, 0.001f);
            Assert.AreApproximatelyEqual(0.0025f, standardTuning.AccelerationPerMeter, 0.0001f);
            Assert.AreApproximatelyEqual(18f, standardTuning.MaximumSpeed, 0.001f);
            Assert.AreApproximatelyEqual(7.5f, SpeedProgression.Calculate(0f, standardTuning, false), 0.001f);
            Assert.AreApproximatelyEqual(7.75f, SpeedProgression.Calculate(100f, standardTuning, false), 0.001f);
            Assert.AreApproximatelyEqual(10f, SpeedProgression.Calculate(1000f, standardTuning, false), 0.001f);
            Assert.AreApproximatelyEqual(18f, SpeedProgression.Calculate(10000f, standardTuning, false), 0.001f);
            Assert.AreApproximatelyEqual(29.7f, SpeedProgression.Calculate(10000f, standardTuning, true), 0.001f);

            DifficultyProfile easy = DifficultyProgression.Evaluate(0);
            Assert.AreEqual(1, easy.ObstacleCount);
            Assert.AreEqual(4, easy.CoinCount);
            Assert.IsFalse(easy.OverheadObstaclesEnabled);

            DifficultyProfile medium = DifficultyProgression.Evaluate(6);
            Assert.AreEqual(2, medium.ObstacleCount);
            Assert.AreEqual(4, medium.CoinCount);
            Assert.IsTrue(medium.OverheadObstaclesEnabled);

            DifficultyProfile hard = DifficultyProgression.Evaluate(18);
            Assert.AreEqual(3, hard.ObstacleCount);
            Assert.AreEqual(5, hard.CoinCount);
            Assert.IsTrue(hard.OverheadObstaclesEnabled);

            PlayerProgressData progress = PlayerProgressData.Empty;
            progress = PlayerProgressRules.RecordRun(progress, -10, -5f, -2);
            Assert.AreEqual(0, progress.BestScore);
            Assert.AreEqual(0, progress.BestDistanceMeters);
            Assert.AreEqual(0, progress.BestCoins);
            Assert.AreEqual(0, progress.PendingScore);
            progress = PlayerProgressRules.RecordRun(progress, 120, 56.9f, 4);
            Assert.AreEqual(120, progress.BestScore);
            Assert.AreEqual(56, progress.BestDistanceMeters);
            Assert.AreEqual(4, progress.BestCoins);
            Assert.AreEqual(120, progress.PendingScore);
            progress = PlayerProgressRules.RecordRun(progress, 80, 90.2f, 2);
            Assert.AreEqual(120, progress.BestScore);
            Assert.AreEqual(90, progress.BestDistanceMeters);
            Assert.AreEqual(4, progress.BestCoins);
            Assert.AreEqual(120, progress.PendingScore);
            progress = PlayerProgressRules.RecordRun(progress, 340, 70f, 8);
            Assert.AreEqual(340, progress.BestScore);
            Assert.AreEqual(90, progress.BestDistanceMeters);
            Assert.AreEqual(8, progress.BestCoins);
            Assert.AreEqual(340, progress.PendingScore);
            progress = PlayerProgressRules.MarkSynchronized(progress, 4, 100);
            Assert.AreEqual(0, progress.PendingScore);
            Assert.AreEqual(4, progress.CachedRank);
            Assert.AreEqual(100, progress.CachedPlayerCount);

            Assert.AreEqual(SwipeDirection.None, RunnerInput.InterpretSwipe(new Vector2(10f, 10f), 50f));
            Assert.AreEqual(SwipeDirection.Left, RunnerInput.InterpretSwipe(new Vector2(-80f, 10f), 50f));
            Assert.AreEqual(SwipeDirection.Right, RunnerInput.InterpretSwipe(new Vector2(80f, 10f), 50f));
            Assert.AreEqual(SwipeDirection.Up, RunnerInput.InterpretSwipe(new Vector2(10f, 80f), 50f));
            Assert.AreEqual(SwipeDirection.Down, RunnerInput.InterpretSwipe(new Vector2(10f, -80f), 50f));

            GameObject poolHost = new GameObject("Pool Validation");
            try
            {
                TrackObjectPool pool = poolHost.AddComponent<TrackObjectPool>();
                GameObject firstRental = pool.Rent(TrackObjectKind.Coin, PrimitiveType.Cylinder, poolHost.transform);
                pool.Return(TrackObjectKind.Coin, firstRental);
                GameObject secondRental = pool.Rent(TrackObjectKind.Coin, PrimitiveType.Cylinder, poolHost.transform);
                Assert.IsTrue(firstRental == secondRental);
            }
            finally
            {
                Object.DestroyImmediate(poolHost);
            }

            const string scenePath = "Assets/Scenes/Prototype.unity";
            Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath));
            Assert.IsTrue(EditorBuildSettings.scenes.Length > 0);
            Assert.AreEqual(scenePath, EditorBuildSettings.scenes[0].path);
            Assert.IsTrue(EditorBuildSettings.scenes[0].enabled);

            Shader runtimeShader = RuntimeMaterialFactory.LoadShader();
            Assert.IsNotNull(runtimeShader);
            Assert.AreEqual(RuntimeMaterialFactory.ShaderName, runtimeShader.name);
            Material runtimeMaterial = RuntimeMaterialFactory.Create("Validation Material", Color.magenta, true);
            try
            {
                Assert.AreEqual(Color.magenta, runtimeMaterial.color);
                Assert.IsTrue(runtimeMaterial.enableInstancing);
            }
            finally
            {
                Object.DestroyImmediate(runtimeMaterial);
            }

            const string linkerPath = "Assets/link.xml";
            Assert.IsTrue(File.Exists(linkerPath));
            string linkerConfiguration = File.ReadAllText(linkerPath);
            Assert.IsTrue(linkerConfiguration.Contains("UnityEngine.BoxCollider"));
            Assert.IsTrue(linkerConfiguration.Contains("UnityEngine.SphereCollider"));
            Assert.IsTrue(linkerConfiguration.Contains("UnityEngine.CapsuleCollider"));
            Assert.IsTrue(linkerConfiguration.Contains("UnityEngine.MeshCollider"));

            const string leaderboardPath = "Assets/Services/capy-chaos-global.lb";
            Assert.IsTrue(File.Exists(leaderboardPath));
            string leaderboardConfiguration = File.ReadAllText(leaderboardPath);
            Assert.IsTrue(leaderboardConfiguration.Contains("\"SortOrder\": \"desc\""));
            Assert.IsTrue(leaderboardConfiguration.Contains("\"UpdateType\": \"keepBest\""));
            Assert.AreEqual("capy-chaos-global", UnityLeaderboardGateway.LeaderboardId);

            Assert.IsFalse(ConnectivityPolicy.IsInternetAvailable(NetworkReachability.NotReachable));
            Assert.IsTrue(ConnectivityPolicy.IsInternetAvailable(NetworkReachability.ReachableViaCarrierDataNetwork));
            Assert.IsTrue(ConnectivityPolicy.IsInternetAvailable(NetworkReachability.ReachableViaLocalAreaNetwork));
            Assert.AreEqual("OFFLINE", LeaderboardStatusFormatter.Format(LeaderboardConnectionState.Offline, -1, false));
            Assert.AreEqual("OFFLINE • PONTUAÇÃO SALVA", LeaderboardStatusFormatter.Format(LeaderboardConnectionState.Offline, -1, true));
            Assert.AreEqual("CONECTANDO", LeaderboardStatusFormatter.Format(LeaderboardConnectionState.Connecting, -1, false));
            Assert.AreEqual("SINCRONIZANDO", LeaderboardStatusFormatter.Format(LeaderboardConnectionState.Synchronizing, -1, true));
            Assert.AreEqual("ONLINE", LeaderboardStatusFormatter.Format(LeaderboardConnectionState.Online, -1, false));
            Assert.AreEqual("ONLINE • RANK GLOBAL #5", LeaderboardStatusFormatter.Format(LeaderboardConnectionState.Online, 4, false));
            Assert.AreEqual("ONLINE • RANKING INDISPONÍVEL", LeaderboardStatusFormatter.Format(LeaderboardConnectionState.OnlineServiceUnavailable, -1, false));
            Assert.AreEqual("ONLINE • SINCRONIZAÇÃO PENDENTE", LeaderboardStatusFormatter.Format(LeaderboardConnectionState.OnlineWithPendingScore, -1, true));

            SessionState.SetBool("CapyChaos.ValidationPassed", true);
            Debug.Log("CAPY_CHAOS_TESTS_PASSED: 84 validações concluídas.");
        }

        private static RunDifficultySettings EnsureDifficultySettingsAsset()
        {
            RunDifficultySettings existing = AssetDatabase.LoadAssetAtPath<RunDifficultySettings>(DifficultyAssetPath);
            if (existing != null)
            {
                if (File.ReadAllText(DifficultyAssetPath).Contains("selectedDifficulty:"))
                {
                    existing.ResetToDefaults();
                    EditorUtility.SetDirty(existing);
                    AssetDatabase.SaveAssets();
                }
                return existing;
            }

            Directory.CreateDirectory("Assets/Resources/Config");
            RunDifficultySettings settings = ScriptableObject.CreateInstance<RunDifficultySettings>();
            settings.ResetToDefaults();
            AssetDatabase.CreateAsset(settings, DifficultyAssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return settings;
        }
    }
}
