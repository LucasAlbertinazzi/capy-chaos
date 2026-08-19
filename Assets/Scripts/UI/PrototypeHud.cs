using CapyChaos.Core;
using CapyChaos.Environment;
using UnityEngine;

namespace CapyChaos.UI
{
    public sealed class PrototypeHud : MonoBehaviour
    {
        private GameSession session;
        private ProceduralTrack track;
        private Transform runner;
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle scoreStyle;
        private GUIStyle gameOverStyle;

        public void Initialize(GameSession gameSession, ProceduralTrack proceduralTrack, Transform runnerTransform)
        {
            session = gameSession;
            track = proceduralTrack;
            runner = runnerTransform;
        }

        private void OnGUI()
        {
            if (session == null || track == null || runner == null)
            {
                return;
            }

            EnsureStyles();
            float scale = Mathf.Max(0.8f, Screen.width / 1080f);
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
            float width = Screen.width / scale;
            float height = Screen.height / scale;

            ThemeDefinition definition = ThemeSequence.Describe(track.GetThemeAt(runner.position.z));
            DrawPanel(new Rect(22f, 18f, width - 44f, 100f));
            GUI.Label(new Rect(42f, 28f, width - 84f, 30f), definition.Title, titleStyle);
            GUI.Label(new Rect(42f, 59f, width - 84f, 48f), definition.Subtitle, bodyStyle);

            GUI.Label(new Rect(30f, 127f, 350f, 42f), $"PONTOS  {session.Score:000000}   •   MOEDAS  {session.Coins}", scoreStyle);
            if (session.IsZoomiesActive)
            {
                GUI.Label(new Rect(width - 250f, 127f, 220f, 42f), "ZOOMIES!  4 PATAS", scoreStyle);
            }

            GUI.Label(new Rect(24f, height - 48f, width - 48f, 32f), "A/D ou ←/→: faixa   •   W/↑: pular   •   S/↓: deslizar   •   Shift: testar Zoomies", bodyStyle);

            if (session.State == RunState.GameOver)
            {
                DrawPanel(new Rect(width * 0.18f, height * 0.34f, width * 0.64f, 170f));
                GUI.Label(new Rect(width * 0.2f, height * 0.37f, width * 0.6f, 58f), "CAPY CAIU NO CAOS!", gameOverStyle);
                GUI.Label(new Rect(width * 0.2f, height * 0.45f, width * 0.6f, 40f), $"Pontuação: {session.Score}   •   Pressione R ou Espaço para reiniciar", scoreStyle);
            }
        }

        private static void DrawPanel(Rect rect)
        {
            Color previous = GUI.color;
            GUI.color = new Color(0.035f, 0.045f, 0.055f, 0.88f);
            GUI.Box(rect, GUIContent.none);
            GUI.color = previous;
        }

        private void EnsureStyles()
        {
            if (titleStyle != null)
            {
                return;
            }

            titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 24, fontStyle = FontStyle.Bold, normal = { textColor = new Color(1f, 0.77f, 0.18f) } };
            bodyStyle = new GUIStyle(GUI.skin.label) { fontSize = 16, wordWrap = true, normal = { textColor = Color.white } };
            scoreStyle = new GUIStyle(GUI.skin.label) { fontSize = 19, fontStyle = FontStyle.Bold, normal = { textColor = Color.white } };
            gameOverStyle = new GUIStyle(GUI.skin.label) { fontSize = 34, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, normal = { textColor = new Color(1f, 0.42f, 0.18f) } };
        }
    }
}
