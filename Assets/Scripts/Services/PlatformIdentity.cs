using System.Threading.Tasks;

namespace CapyChaos.Services
{
    public enum PlatformIdentityKind
    {
        None,
        Google,
        Apple
    }

    public readonly struct PlatformIdentityToken
    {
        public PlatformIdentityToken(PlatformIdentityKind kind, string idToken)
        {
            Kind = kind;
            IdToken = idToken;
        }

        public PlatformIdentityKind Kind { get; }
        public string IdToken { get; }
        public bool IsValid => Kind != PlatformIdentityKind.None && !string.IsNullOrWhiteSpace(IdToken);
    }

    public interface IPlatformIdentityProvider
    {
        Task<PlatformIdentityToken> TryAuthenticateAsync();
    }

    public sealed class PendingPlatformIdentityProvider : IPlatformIdentityProvider
    {
        public Task<PlatformIdentityToken> TryAuthenticateAsync()
        {
            return Task.FromResult(new PlatformIdentityToken(PlatformIdentityKind.None, string.Empty));
        }
    }

    public static class PlatformIdentityRegistry
    {
        public static IPlatformIdentityProvider Provider { get; set; } = new PendingPlatformIdentityProvider();
    }
}
