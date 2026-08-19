using System;
using UnityEngine;

namespace CapyChaos.Services
{
    [Serializable]
    public struct PlayerProgressData
    {
        public int BestScore;
        public int BestDistanceMeters;
        public int BestCoins;
        public int PendingScore;
        public int CachedRank;
        public int CachedPlayerCount;

        public static PlayerProgressData Empty => new PlayerProgressData { CachedRank = -1 };
    }

    public static class PlayerProgressRules
    {
        public static PlayerProgressData RecordScore(PlayerProgressData progress, int score)
        {
            return RecordRun(progress, score, 0f, 0);
        }

        public static PlayerProgressData RecordRun(PlayerProgressData progress, int score, float distance, int coins)
        {
            int safeScore = Mathf.Max(0, score);
            int safeDistance = Mathf.Max(0, Mathf.FloorToInt(distance));
            int safeCoins = Mathf.Max(0, coins);
            progress.BestScore = Mathf.Max(progress.BestScore, safeScore);
            progress.BestDistanceMeters = Mathf.Max(progress.BestDistanceMeters, safeDistance);
            progress.BestCoins = Mathf.Max(progress.BestCoins, safeCoins);
            progress.PendingScore = Mathf.Max(progress.PendingScore, safeScore);
            return progress;
        }

        public static PlayerProgressData MarkSynchronized(PlayerProgressData progress, int rank, int playerCount)
        {
            progress.PendingScore = 0;
            progress.CachedRank = Mathf.Max(-1, rank);
            progress.CachedPlayerCount = Mathf.Max(0, playerCount);
            return progress;
        }
    }

    public interface IPlayerProgressStore
    {
        PlayerProgressData Load();
        void Save(PlayerProgressData progress);
    }

    public sealed class PlayerPrefsProgressStore : IPlayerProgressStore
    {
        private const string StorageKey = "capy-chaos.player-progress.v1";

        public PlayerProgressData Load()
        {
            if (!PlayerPrefs.HasKey(StorageKey))
            {
                return PlayerProgressData.Empty;
            }

            PlayerProgressData progress = JsonUtility.FromJson<PlayerProgressData>(PlayerPrefs.GetString(StorageKey));
            if (progress.CachedRank < -1)
            {
                progress.CachedRank = -1;
            }
            return progress;
        }

        public void Save(PlayerProgressData progress)
        {
            PlayerPrefs.SetString(StorageKey, JsonUtility.ToJson(progress));
            PlayerPrefs.Save();
        }
    }
}
