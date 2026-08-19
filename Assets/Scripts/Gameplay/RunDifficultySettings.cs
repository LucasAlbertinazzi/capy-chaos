using System;
using UnityEngine;

namespace CapyChaos.Gameplay
{
    [Serializable]
    public struct RunDifficultyTuning
    {
        [SerializeField] private string displayName;
        [SerializeField] private float initialSpeed;
        [SerializeField] private float accelerationPerMeter;
        [SerializeField] private float maximumSpeed;
        [SerializeField] private float zoomiesMultiplier;
        [SerializeField] private int obstacleTileOffset;

        public RunDifficultyTuning(
            string displayName,
            float initialSpeed,
            float accelerationPerMeter,
            float maximumSpeed,
            float zoomiesMultiplier,
            int obstacleTileOffset)
        {
            this.displayName = displayName;
            this.initialSpeed = initialSpeed;
            this.accelerationPerMeter = accelerationPerMeter;
            this.maximumSpeed = maximumSpeed;
            this.zoomiesMultiplier = zoomiesMultiplier;
            this.obstacleTileOffset = obstacleTileOffset;
        }

        public string DisplayName => displayName;
        public float InitialSpeed => initialSpeed;
        public float AccelerationPerMeter => accelerationPerMeter;
        public float MaximumSpeed => maximumSpeed;
        public float ZoomiesMultiplier => zoomiesMultiplier;
        public int ObstacleTileOffset => obstacleTileOffset;
    }

    [CreateAssetMenu(fileName = "RunDifficultySettings", menuName = "CAPY CHAOS/Configuração de dificuldade")]
    public sealed class RunDifficultySettings : ScriptableObject
    {
        public const string ResourcePath = "Config/RunDifficultySettings";

        [SerializeField] private RunDifficultyTuning standard = GetDefault();

        public RunDifficultyTuning Standard => standard;

        public void ResetToDefaults()
        {
            standard = GetDefault();
        }

        public static RunDifficultyTuning GetDefault()
        {
            return new RunDifficultyTuning("PADRÃO", 7.5f, 0.0025f, 18f, 1.65f, 0);
        }
    }

    public static class SpeedProgression
    {
        public static float Calculate(float distance, RunDifficultyTuning tuning, bool zoomiesActive)
        {
            float safeDistance = Mathf.Max(0f, distance);
            float normalSpeed = Mathf.Min(
                tuning.MaximumSpeed,
                tuning.InitialSpeed + safeDistance * tuning.AccelerationPerMeter);
            return zoomiesActive ? normalSpeed * tuning.ZoomiesMultiplier : normalSpeed;
        }
    }
}
