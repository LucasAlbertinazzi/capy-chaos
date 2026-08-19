using CapyChaos.Core;
using CapyChaos.Environment;
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

            const string scenePath = "Assets/Scenes/Prototype.unity";
            Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath));
            Assert.IsTrue(EditorBuildSettings.scenes.Length > 0);
            Assert.AreEqual(scenePath, EditorBuildSettings.scenes[0].path);
            Assert.IsTrue(EditorBuildSettings.scenes[0].enabled);

            SessionState.SetBool("CapyChaos.ValidationPassed", true);
            Debug.Log("CAPY_CHAOS_TESTS_PASSED: 16 validações concluídas.");
        }
    }
}
