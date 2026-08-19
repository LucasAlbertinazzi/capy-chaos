using System;
using System.Threading.Tasks;
using CapyChaos.Core;
using UnityEngine;

namespace CapyChaos.Services
{
    public enum LeaderboardConnectionState
    {
        Offline,
        Connecting,
        Synchronizing,
        Online,
        OnlineServiceUnavailable,
        OnlineWithPendingScore
    }

    public static class ConnectivityPolicy
    {
        public static bool IsInternetAvailable(NetworkReachability reachability)
        {
            return reachability != NetworkReachability.NotReachable;
        }
    }

    public static class LeaderboardStatusFormatter
    {
        public static string Format(LeaderboardConnectionState state, int rank, bool hasPendingScore)
        {
            switch (state)
            {
                case LeaderboardConnectionState.Online:
                    return rank >= 0 ? $"ONLINE • RANK GLOBAL #{rank + 1}" : "ONLINE";
                case LeaderboardConnectionState.OnlineWithPendingScore:
                    return "ONLINE • SINCRONIZAÇÃO PENDENTE";
                case LeaderboardConnectionState.OnlineServiceUnavailable:
                    return "ONLINE • RANKING INDISPONÍVEL";
                case LeaderboardConnectionState.Synchronizing:
                    return "SINCRONIZANDO";
                case LeaderboardConnectionState.Connecting:
                    return "CONECTANDO";
                default:
                    return hasPendingScore ? "OFFLINE • PONTUAÇÃO SALVA" : "OFFLINE";
            }
        }
    }

    public sealed class LeaderboardCoordinator : MonoBehaviour
    {
        private const float ConnectivityPollIntervalSeconds = 2f;
        private const float RetryDelaySeconds = 10f;

        private readonly UnityLeaderboardGateway gateway = new UnityLeaderboardGateway();
        private IPlayerProgressStore progressStore;
        private PlayerProgressData progress;
        private GameSession session;
        private bool scoreRecordedForRun;
        private bool synchronizationInProgress;
        private bool internetWasAvailable;
        private float nextConnectivityPollTime;
        private float nextRetryTime;

        public LeaderboardConnectionState ConnectionState { get; private set; } = LeaderboardConnectionState.Offline;
        public int BestScore => progress.BestScore;
        public int BestDistanceMeters => progress.BestDistanceMeters;
        public int BestCoins => progress.BestCoins;
        public int Rank => progress.CachedRank;
        public bool HasPendingScore => progress.PendingScore > 0;

        public void Initialize(GameSession gameSession)
        {
            if (session != null)
            {
                session.StateChanged -= HandleSessionStateChanged;
            }

            session = gameSession;
            progressStore = new PlayerPrefsProgressStore();
            progress = progressStore.Load();
            session.StateChanged += HandleSessionStateChanged;
            internetWasAvailable = HasInternetConnection();
            ConnectionState = internetWasAvailable
                ? LeaderboardConnectionState.Connecting
                : LeaderboardConnectionState.Offline;
            _ = ConnectAndSynchronizeAsync();
        }

        private void Update()
        {
            if (session == null || Time.unscaledTime < nextConnectivityPollTime)
            {
                return;
            }

            nextConnectivityPollTime = Time.unscaledTime + ConnectivityPollIntervalSeconds;
            bool internetAvailable = HasInternetConnection();
            if (!internetAvailable)
            {
                internetWasAvailable = false;
                ConnectionState = LeaderboardConnectionState.Offline;
                return;
            }

            bool connectionRecovered = !internetWasAvailable;
            internetWasAvailable = true;
            bool needsRetry = ConnectionState == LeaderboardConnectionState.Offline
                || ConnectionState == LeaderboardConnectionState.OnlineServiceUnavailable
                || ConnectionState == LeaderboardConnectionState.OnlineWithPendingScore
                || HasPendingScore;

            if ((connectionRecovered || needsRetry) && Time.unscaledTime >= nextRetryTime)
            {
                _ = ConnectAndSynchronizeAsync();
            }
        }

        private void OnDestroy()
        {
            if (session != null)
            {
                session.StateChanged -= HandleSessionStateChanged;
            }
        }

        private void HandleSessionStateChanged()
        {
            if (session.State == RunState.Running)
            {
                scoreRecordedForRun = false;
                return;
            }

            if (session.State != RunState.GameOver || scoreRecordedForRun)
            {
                return;
            }

            scoreRecordedForRun = true;
            progress = PlayerProgressRules.RecordRun(progress, session.Score, session.Distance, session.Coins);
            progressStore.Save(progress);
            if (HasInternetConnection())
            {
                _ = ConnectAndSynchronizeAsync();
            }
            else
            {
                ConnectionState = LeaderboardConnectionState.Offline;
            }
        }

        public async Task ConnectAndSynchronizeAsync()
        {
            if (synchronizationInProgress)
            {
                return;
            }

            if (!HasInternetConnection())
            {
                internetWasAvailable = false;
                ConnectionState = LeaderboardConnectionState.Offline;
                return;
            }

            synchronizationInProgress = true;
            ConnectionState = LeaderboardConnectionState.Connecting;
            try
            {
                bool authenticated = await gateway.TryAuthenticateAsync(PlatformIdentityRegistry.Provider);
                if (!authenticated)
                {
                    MarkOnlineRetryRequired();
                    return;
                }

                if (progress.PendingScore > 0)
                {
                    ConnectionState = LeaderboardConnectionState.Synchronizing;
                    await SynchronizePendingScoreAsync();
                }

                ConnectionState = HasPendingScore
                    ? LeaderboardConnectionState.OnlineWithPendingScore
                    : LeaderboardConnectionState.Online;
            }
            catch (Exception)
            {
                MarkOnlineRetryRequired();
            }
            finally
            {
                synchronizationInProgress = false;
            }
        }

        private async Task SynchronizePendingScoreAsync()
        {
            if (!gateway.IsAuthenticated || progress.PendingScore <= 0)
            {
                return;
            }

            try
            {
                PlayerRanking ranking = await gateway.SubmitBestAsync(progress.PendingScore);
                progress = PlayerProgressRules.MarkSynchronized(progress, ranking.Rank, ranking.PlayerCount);
                progressStore.Save(progress);
                ConnectionState = LeaderboardConnectionState.Online;
            }
            catch (Exception)
            {
                MarkOnlineRetryRequired();
            }
        }

        public string GetStatusText()
        {
            return LeaderboardStatusFormatter.Format(ConnectionState, Rank, HasPendingScore);
        }

        private static bool HasInternetConnection()
        {
            return ConnectivityPolicy.IsInternetAvailable(Application.internetReachability);
        }

        private void MarkOnlineRetryRequired()
        {
            ConnectionState = HasInternetConnection()
                ? (HasPendingScore
                    ? LeaderboardConnectionState.OnlineWithPendingScore
                    : LeaderboardConnectionState.OnlineServiceUnavailable)
                : LeaderboardConnectionState.Offline;
            nextRetryTime = Time.unscaledTime + RetryDelaySeconds;
        }
    }
}
