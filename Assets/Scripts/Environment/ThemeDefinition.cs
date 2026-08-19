using UnityEngine;

namespace CapyChaos.Environment
{
    public enum BrazilTheme
    {
        SaoPaulo,
        RioDeJaneiro,
        Brasilia,
        Amazonia
    }

    public readonly struct ThemeDefinition
    {
        public ThemeDefinition(string title, string subtitle, Color ground, Color accent, Color secondary)
        {
            Title = title;
            Subtitle = subtitle;
            Ground = ground;
            Accent = accent;
            Secondary = secondary;
        }

        public string Title { get; }
        public string Subtitle { get; }
        public Color Ground { get; }
        public Color Accent { get; }
        public Color Secondary { get; }
    }

    public static class ThemeSequence
    {
        public const int TilesPerTheme = 3;
        private const int ThemeCount = 4;

        public static BrazilTheme GetTheme(int tileIndex)
        {
            int safeIndex = Mathf.Max(0, tileIndex);
            return (BrazilTheme)((safeIndex / TilesPerTheme) % ThemeCount);
        }

        public static int GetVariant(int tileIndex)
        {
            int safeIndex = Mathf.Max(0, tileIndex);
            return (safeIndex / (TilesPerTheme * ThemeCount)) % 3;
        }

        public static ThemeDefinition Describe(BrazilTheme theme)
        {
            switch (theme)
            {
                case BrazilTheme.SaoPaulo:
                    return new ThemeDefinition(
                        "SÃO PAULO — RIO TIETÊ",
                        "A capivara corre dentro da água escura: espuma, lixo e odor denunciam a poluição urbana.",
                        new Color(0.16f, 0.22f, 0.16f),
                        new Color(0.47f, 0.55f, 0.34f),
                        new Color(0.28f, 0.30f, 0.29f));
                case BrazilTheme.RioDeJaneiro:
                    return new ThemeDefinition(
                        "RIO — COPACABANA",
                        "Beleza e desigualdade dividem a paisagem; o cotidiano da favela aparece com dignidade.",
                        new Color(0.78f, 0.68f, 0.43f),
                        new Color(0.05f, 0.56f, 0.76f),
                        new Color(0.88f, 0.35f, 0.27f));
                case BrazilTheme.Brasilia:
                    return new ThemeDefinition(
                        "BRASÍLIA — PLANALTO CENTRAL",
                        "Luxo, malas e dinheiro satirizam a corrupção sistêmica sem partidos ou pessoas reais.",
                        new Color(0.72f, 0.69f, 0.57f),
                        new Color(0.91f, 0.73f, 0.22f),
                        new Color(0.27f, 0.42f, 0.62f));
                default:
                    return new ThemeDefinition(
                        "AMAZÔNIA",
                        "A floresta viva dá lugar a trechos de corte, fumaça e regeneração — uma mudança visível.",
                        new Color(0.16f, 0.36f, 0.16f),
                        new Color(0.20f, 0.61f, 0.25f),
                        new Color(0.48f, 0.27f, 0.12f));
            }
        }
    }
}
