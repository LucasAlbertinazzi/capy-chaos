using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Leaderboards;

namespace CapyChaos.Services
{
    public readonly struct PlayerRanking
    {
        public PlayerRanking(int rank, int playerCount)
        {
            Rank = rank;
            PlayerCount = playerCount;
        }

        public int Rank { get; }
        public int PlayerCount { get; }
    }

    public sealed class UnityLeaderboardGateway
    {
        public const string LeaderboardId = "capy-chaos-global";

        public bool IsAuthenticated => AuthenticationService.Instance.IsSignedIn;

        public async Task<bool> TryAuthenticateAsync(IPlatformIdentityProvider identityProvider)
        {
            await UnityServices.InitializeAsync();

            if (AuthenticationService.Instance.IsSignedIn)
            {
                return true;
            }

            PlatformIdentityToken identity;
            try
            {
                identity = await identityProvider.TryAuthenticateAsync();
            }
            catch (Exception)
            {
                identity = new PlatformIdentityToken(PlatformIdentityKind.None, string.Empty);
            }

            if (identity.IsValid)
            {
                try
                {
                    switch (identity.Kind)
                    {
                        case PlatformIdentityKind.Google:
                            await AuthenticationService.Instance.SignInWithGoogleAsync(identity.IdToken);
                            break;
                        case PlatformIdentityKind.Apple:
                            await AuthenticationService.Instance.SignInWithAppleAsync(identity.IdToken);
                            break;
                    }
                }
                catch (Exception)
                {
                    if (!AuthenticationService.Instance.IsSignedIn)
                    {
                        await AuthenticationService.Instance.SignInAnonymouslyAsync();
                    }
                }
                return AuthenticationService.Instance.IsSignedIn;
            }

            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            return AuthenticationService.Instance.IsSignedIn;
        }

        public async Task<PlayerRanking> SubmitBestAsync(int score)
        {
            if (!IsAuthenticated)
            {
                throw new InvalidOperationException("O jogador precisa estar autenticado para enviar pontuação.");
            }

            var entry = await LeaderboardsService.Instance.AddPlayerScoreAsync(LeaderboardId, Math.Max(0, score));
            return new PlayerRanking(entry.Rank, 0);
        }
    }
}
