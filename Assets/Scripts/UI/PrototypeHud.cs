using CapyChaos.Core;
using CapyChaos.Environment;
using CapyChaos.Services;
using UnityEngine;

namespace CapyChaos.UI
{
    public sealed class PrototypeHud : MonoBehaviour
    {
        private GameSession session;
        private ProceduralTrack track;
        private Transform runner;
        private LeaderboardCoordinator leaderboard;
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle scoreStyle;
        private GUIStyle gameOverStyle;
        private GUIStyle resultStyle;

        public void Initialize(
            GameSession gameSession,
            ProceduralTrack proceduralTrack,
            Transform runnerTransform,
            LeaderboardCoordinator leaderboardCoordinator)
        {
            session = gameSession;
            track = proceduralTrack;
            runner = runnerTransform;
            leaderboard = leaderboardCoordinator;
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

            float speedKmh = session.CurrentSpeed * 3.6f;
            int bestScore = leaderboard != null ? leaderboard.BestScore : 0;
            GUI.Label(new Rect(30f, 127f, width - 60f, 42f), $"PONTOS  {session.Score:000000}   •   RECORDE  {bestScore:000000}   •   {speedKmh:0} KM/H", scoreStyle);
            GUI.Label(new Rect(30f, 160f, width - 60f, 34f), $"DISTÂNCIA  {Mathf.FloorToInt(session.Distance):0000} M   •   CAPY COINS  {session.Coins:000}", bodyStyle);
            if (leaderboard != null)
            {
                GUI.Label(new Rect(30f, 190f, width - 60f, 34f), leaderboard.GetStatusText(), bodyStyle);
            }
            if (session.IsZoomiesActive)
            {
                GUI.Label(new Rect(width - 250f, 190f, 220f, 34f), "ZOOMIES!  4 PATAS", scoreStyle);
            }

            string controls = Application.isMobilePlatform
                ? "DESLIZE ←/→: trocar de faixa   •   DESLIZE ↑: pular   •   DESLIZE ↓: deslizar"
                : "A/D ou ←/→: faixa   •   W/↑: pular   •   S/↓: deslizar   •   Shift: testar Zoomies";
            GUI.Label(new Rect(24f, height - 48f, width - 48f, 32f), controls, bodyStyle);

            if (session.State == RunState.GameOver)
            {
                float panelX = width * 0.12f;
                float panelY = height * 0.29f;
                float panelWidth = width * 0.76f;
                const float panelHeight = 320f;
                DrawPanel(new Rect(panelX, panelY, panelWidth, panelHeight));
                GUI.Label(new Rect(panelX + 20f, panelY + 20f, panelWidth - 40f, 52f), "CAPY CAIU NO CAOS!", gameOverStyle);
                GUI.Label(new Rect(panelX + 24f, panelY + 82f, panelWidth - 48f, 34f), $"PONTUAÇÃO  {session.Score:000000}", resultStyle);
                GUI.Label(new Rect(panelX + 24f, panelY + 120f, panelWidth - 48f, 34f), $"DISTÂNCIA  {Mathf.FloorToInt(session.Distance)} M   •   CAPY COINS  {session.Coins}", resultStyle);

                if (leaderboard != null)
                {
                    GUI.Label(new Rect(panelX + 24f, panelY + 166f, panelWidth - 48f, 30f), $"RECORDE  {leaderboard.BestScore:000000}", bodyStyle);
                    GUI.Label(new Rect(panelX + 24f, panelY + 198f, panelWidth - 48f, 30f), $"MELHOR DISTÂNCIA  {leaderboard.BestDistanceMeters} M   •   MAIS MOEDAS  {leaderboard.BestCoins}", bodyStyle);
                }

                string restartInstruction = Application.isMobilePlatform
                    ? "Toque para reiniciar"
                    : "Pressione R ou Espaço para reiniciar";
                GUI.Label(new Rect(panelX + 20f, panelY + 258f, panelWidth - 40f, 38f), restartInstruction, resultStyle);
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
            resultStyle = new GUIStyle(GUI.skin.label) { fontSize = 20, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, normal = { textColor = Color.white } };
        }
    }
}
