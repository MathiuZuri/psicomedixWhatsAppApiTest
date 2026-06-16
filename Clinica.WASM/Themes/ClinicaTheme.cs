using MudBlazor;
using MudBlazor.Utilities;

namespace Clinica.WASM.Themes;

public class ClinicaTheme : MudTheme
{
    public ClinicaTheme()
    {
        PaletteLight = new PaletteLight
        {
            // Azul Institucional profundo
            Primary = new MudColor("#2B4CDE"),

            // REALCE ROSE GOLD VIBRANTE: Más saturado para que no se pierda en fondos claros
            Secondary = new MudColor("#D48A9C"),

            // NUEVO: Oro/Amarillo cálido inspirado en tu 3ra imagen de referencia (Forecast Arena)
            Tertiary = new MudColor("#E5B324"),
            
            Info = new MudColor("#0EA5E9"),
            Success = new MudColor("#10B981"),
            Warning = new MudColor("#F59E0B"),
            Error = new MudColor("#EF4444"),

            // Mayor contraste: Un fondo ligeramente más gris/azulado para que el Surface (blanco) resalte
            Background = new MudColor("#F4F7FB"),
            BackgroundGray = new MudColor("#EAEFF5"),
            Surface = new MudColor("#FFFFFF"),

            AppbarBackground = new MudColor("#FFFFFF"),
            AppbarText = new MudColor("#0F172A"),
            DrawerBackground = new MudColor("#FFFFFF"),
            DrawerText = new MudColor("#334155"),
            DrawerIcon = new MudColor("#64748B"),

            TextPrimary = new MudColor("#0F172A"),
            TextSecondary = new MudColor("#64748B"),

            // Líneas un poco más visibles para separar secciones estructuradas
            Divider = new MudColor("#CBD5E1"),
            LinesDefault = new MudColor("#CBD5E1"),
            TableLines = new MudColor("#E2E8F0"),

            TableStriped = new MudColor("#F8FAFC"),
            TableHover = new MudColor("#F0F4FF"),

            ActionDefault = new MudColor("#64748B"),
            ActionDisabled = new MudColor("#94A3B8"),
            ActionDisabledBackground = new MudColor("#F1F5F9")
        };

        PaletteDark = new PaletteDark
        {
            Primary = new MudColor("#849DFF"),

            // Rose Gold adaptado para brillar en fondos oscuros
            Secondary = new MudColor("#EBB1C3"),

            // Oro vibrante para modo noche
            Tertiary = new MudColor("#FCD34D"),
            
            Info = new MudColor("#38BDF8"),
            Success = new MudColor("#34D399"),
            Warning = new MudColor("#FBBF24"),
            Error = new MudColor("#F87171"),

            // Fondos oscuros con más profundidad (tipo Navy/Slate profundo)
            Background = new MudColor("#090E17"),
            BackgroundGray = new MudColor("#111827"),
            Surface = new MudColor("#161D2F"),

            AppbarBackground = new MudColor("#111827"),
            AppbarText = new MudColor("#F8FAFC"),
            DrawerBackground = new MudColor("#111827"),
            DrawerText = new MudColor("#E5E7EB"),
            DrawerIcon = new MudColor("#94A3B8"),

            TextPrimary = new MudColor("#F8FAFC"),
            TextSecondary = new MudColor("#94A3B8"),

            Divider = new MudColor("#2A3554"),
            LinesDefault = new MudColor("#2A3554"),
            TableLines = new MudColor("#2A3554"),

            TableStriped = new MudColor("#1A2235"),
            TableHover = new MudColor("#253154"),

            ActionDefault = new MudColor("#94A3B8"),
            ActionDisabled = new MudColor("#64748B"),
            ActionDisabledBackground = new MudColor("#111827")
        };

        Typography = new Typography
        {
            Default = new DefaultTypography
            {
                FontFamily = new[] { "Inter", "system-ui", "sans-serif" },
                FontSize = "0.875rem",
                FontWeight = "400",
                LineHeight = "1.5"
            },
            H1 = new H1Typography
            {
                FontFamily = new[] { "Inter", "system-ui", "sans-serif" },
                FontWeight = "800",
                FontSize = "2.25rem",
                LineHeight = "1.25",
                LetterSpacing = "-0.025em"
            },
            H2 = new H2Typography
            {
                FontFamily = new[] { "Inter", "system-ui", "sans-serif" },
                FontWeight = "700",
                FontSize = "1.75rem",
                LineHeight = "1.3",
                LetterSpacing = "-0.02em"
            },
            H3 = new H3Typography
            {
                FontFamily = new[] { "Inter", "system-ui", "sans-serif" },
                FontWeight = "600",
                FontSize = "1.25rem",
                LineHeight = "1.35"
            },
            Button = new ButtonTypography
            {
                FontFamily = new[] { "Inter", "system-ui", "sans-serif" },
                FontWeight = "600",
                FontSize = "0.875rem",
                TextTransform = "none"
            }
        };

        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "16px",
            DrawerWidthLeft = "260px",
            DrawerWidthRight = "260px",
            AppbarHeight = "64px"
        };
        
        Shadows = new Shadow
        {
            Elevation = new[]
            {
                "none", // 0
                "0 2px 4px -1px rgba(15, 23, 42, 0.04), 0 1px 2px -1px rgba(15, 23, 42, 0.03)", // 1 (Elementos pequeños)
                "0 4px 12px -2px rgba(15, 23, 42, 0.05), 0 2px 6px -1px rgba(15, 23, 42, 0.04)", // 2 (Tarjetas base)
                "0 8px 24px -4px rgba(15, 23, 42, 0.08), 0 4px 10px -2px rgba(15, 23, 42, 0.05)", // 3 (Hover en tarjetas)
                "0 12px 32px -4px rgba(15, 23, 42, 0.10), 0 6px 14px -2px rgba(15, 23, 42, 0.06)", // 4 (Dropdowns/Menús)
                "0 16px 40px -6px rgba(15, 23, 42, 0.12), 0 8px 18px -4px rgba(15, 23, 42, 0.08)", // 5
                "0 20px 48px -8px rgba(15, 23, 42, 0.14), 0 10px 22px -4px rgba(15, 23, 42, 0.10)", // 6 (Modales/Dialogs)
                "0 24px 56px -10px rgba(15, 23, 42, 0.16), 0 12px 26px -6px rgba(15, 23, 42, 0.12)", // 7
                "0 28px 64px -12px rgba(15, 23, 42, 0.18)", // 8
                "0 32px 72px -14px rgba(15, 23, 42, 0.20)", // 9
                "0 36px 80px -16px rgba(15, 23, 42, 0.22)", // 10
                "0 40px 88px -18px rgba(15, 23, 42, 0.24)", // 11
                "0 44px 96px -20px rgba(15, 23, 42, 0.26)", // 12
                "0 48px 104px -22px rgba(15, 23, 42, 0.28)", // 13
                "0 52px 112px -24px rgba(15, 23, 42, 0.30)", // 14
                "0 56px 120px -26px rgba(15, 23, 42, 0.32)", // 15
                "0 60px 128px -28px rgba(15, 23, 42, 0.34)", // 16
                "0 64px 136px -30px rgba(15, 23, 42, 0.36)", // 17
                "0 68px 144px -32px rgba(15, 23, 42, 0.38)", // 18
                "0 72px 152px -34px rgba(15, 23, 42, 0.40)", // 19
                "0 76px 160px -36px rgba(15, 23, 42, 0.42)", // 20
                "0 80px 168px -38px rgba(15, 23, 42, 0.44)", // 21
                "0 84px 176px -40px rgba(15, 23, 42, 0.46)", // 22
                "0 88px 184px -42px rgba(15, 23, 42, 0.48)", // 23
                "0 92px 192px -44px rgba(15, 23, 42, 0.50)", // 24
                "0 96px 200px -46px rgba(15, 23, 42, 0.52)"  // 25
            }
        };
    }
}