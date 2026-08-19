using System;
using CapyChaos.Gameplay;
using CapyChaos.Input;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CapyChaos.Core
{
    public enum RunState
    {
        Running,
        GameOver
    }

    public sealed class GameSession : MonoBehaviour
    {
        [SerializeField] private RunDifficultySettings difficultySettings;

        private float zoomiesRemaining;
        private RunDifficultyTuning activeTuning;

        public event Action StateChanged;

        public RunState State { get; private set; } = RunState.Running;
        public float Distance { get; private set; }
        public int Coins { get; private set; }
        public int Score => RunnerRules.CalculateScore(Distance, Coins);
        public bool IsZoomiesActive => zoomiesRemaining > 0f;
        public RunDifficultyTuning DifficultyTuning => activeTuning;
        public string DifficultyName => activeTuning.DisplayName;
        public float CurrentSpeed => SpeedProgression.Calculate(Distance, activeTuning, IsZoomiesActive);

        private void Awake()
        {
            if (difficultySettings == null)
            {
                difficultySettings = Resources.Load<RunDifficultySettings>(RunDifficultySettings.ResourcePath);
            }

            activeTuning = difficultySettings != null
                ? difficultySettings.Standard
                : RunDifficultySettings.GetDefault();
        }

        private void Update()
        {
            if (State != RunState.Running)
            {
                if (RunnerInput.IsRestartRequested())
                {
                    Restart();
                }

                return;
            }

            Distance += CurrentSpeed * Time.deltaTime;
            zoomiesRemaining = Mathf.Max(0f, zoomiesRemaining - Time.deltaTime);
        }

        public void CollectCoin()
        {
            if (State == RunState.Running)
            {
                Coins++;
            }
        }

        public void ActivateZoomies(float duration)
        {
            if (State == RunState.Running)
            {
                zoomiesRemaining = Mathf.Max(zoomiesRemaining, duration);
            }
        }

        public void EndRun()
        {
            if (State == RunState.GameOver)
            {
                return;
            }

            State = RunState.GameOver;
            StateChanged?.Invoke();
        }

        public void Restart()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.IsValid() && !string.IsNullOrEmpty(activeScene.path))
            {
                SceneManager.LoadScene(activeScene.buildIndex);
                return;
            }

            Distance = 0f;
            Coins = 0;
            zoomiesRemaining = 0f;
            State = RunState.Running;
            StateChanged?.Invoke();
        }
    }

    public static class RunnerRules
    {
        public const int MinimumLane = -1;
        public const int MaximumLane = 1;

        public static int ClampLane(int lane)
        {
            return Mathf.Clamp(lane, MinimumLane, MaximumLane);
        }

        public static int CalculateScore(float distance, int coins)
        {
            return Mathf.Max(0, Mathf.FloorToInt(distance)) + Mathf.Max(0, coins) * 10;
        }
    }
}
