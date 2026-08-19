using UnityEngine;

namespace CapyChaos.Gameplay
{
    public readonly struct DifficultyProfile
    {
        public DifficultyProfile(int obstacleCount, int coinCount, bool overheadObstaclesEnabled)
        {
            ObstacleCount = obstacleCount;
            CoinCount = coinCount;
            OverheadObstaclesEnabled = overheadObstaclesEnabled;
        }

        public int ObstacleCount { get; }
        public int CoinCount { get; }
        public bool OverheadObstaclesEnabled { get; }
    }

    public static class DifficultyProgression
    {
        public static DifficultyProfile Evaluate(int tileIndex)
        {
            return Evaluate(tileIndex, RunDifficultySettings.GetDefault());
        }

        public static DifficultyProfile Evaluate(int tileIndex, RunDifficultyTuning tuning)
        {
            int safeTile = Mathf.Max(0, tileIndex + tuning.ObstacleTileOffset);
            int obstacleCount = safeTile < 6 ? 1 : safeTile < 18 ? 2 : 3;
            int coinCount = Mathf.Clamp(4 + safeTile / 12, 4, 7);
            return new DifficultyProfile(obstacleCount, coinCount, safeTile >= 4);
        }
    }
}
