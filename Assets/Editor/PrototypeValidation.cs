using CapyChaos.Core;
using CapyChaos.Environment;
using CapyChaos.Gameplay;
using CapyChaos.Input;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace CapyChaos.Editor
{
    public static class PrototypeValidation
    {
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

            SessionState.SetBool("CapyChaos.ValidationPassed", true);
            Debug.Log("CAPY_CHAOS_TESTS_PASSED: 31 validações concluídas.");
        }
    }
}
