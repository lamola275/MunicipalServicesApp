using System.Drawing;

namespace MunicipalServicesApp
{

    internal static class UiTheme
    {
        public static readonly Color Primary = Color.FromArgb(0, 77, 64);       
        public static readonly Color PrimaryLight = Color.FromArgb(178, 223, 219); 
        public static readonly Color Accent = Color.FromArgb(0, 121, 107);      
        public static readonly Color AccentHover = Color.FromArgb(0, 96, 85);
        public static readonly Color Background = Color.FromArgb(245, 247, 250);
        public static readonly Color Surface = Color.White;
        public static readonly Color TextDark = Color.FromArgb(38, 50, 56);
        public static readonly Color TextMuted = Color.FromArgb(96, 110, 116);

        public static Font TitleFont
        {
            get { return new Font("Segoe UI", 16f, FontStyle.Bold); }
        }

        public static Font SubtitleFont
        {
            get { return new Font("Segoe UI", 10f, FontStyle.Regular); }
        }

        public static Font ButtonFont
        {
            get { return new Font("Segoe UI", 10f, FontStyle.Bold); }
        }

        public static Font LabelFont
        {
            get { return new Font("Segoe UI", 9.5f, FontStyle.Regular); }
        }

        public static Font SmallFont
        {
            get { return new Font("Segoe UI", 8.5f, FontStyle.Regular); }
        }
    }
}
