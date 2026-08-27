using MudBlazor;

namespace OrderStatus.Web;

/// <summary>
/// This app's own look: teal primary on a cool grey ground, flat bordered
/// surfaces rather than shadows, and a tighter type scale. Deliberately unlike
/// the other apps in the portfolio.
/// </summary>
public static class BoardTheme
{
    public static readonly MudTheme Theme = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#0f766e",
            Secondary = "#0ea5e9",
            Tertiary = "#14b8a6",
            Info = "#0284c7",
            Success = "#15803d",
            Warning = "#b45309",
            Error = "#b91c1c",
            Background = "#f1f5f6",
            Surface = "#ffffff",
            AppbarBackground = "#ffffff",
            AppbarText = "#0f766e",
            TextPrimary = "#12232b",
            TextSecondary = "#5b7079",
            Divider = "#dbe4e6",
            TableLines = "#e6eef0",
            ActionDefault = "#5b7079"
        },
        Typography = new Typography
        {
            Default = new DefaultTypography { FontFamily = ["Inter", "Segoe UI", "sans-serif"], FontSize = "0.875rem" },
            H5 = new H5Typography { FontSize = "1.35rem", FontWeight = "700", LetterSpacing = "-.01em" },
            H6 = new H6Typography { FontSize = "1rem", FontWeight = "600" },
            Subtitle2 = new Subtitle2Typography { FontSize = "0.8rem", FontWeight = "600" },
            Button = new ButtonTypography { TextTransform = "none", FontWeight = "600" }
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "6px"
        }
    };
}
