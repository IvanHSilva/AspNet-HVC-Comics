using ComicModel = HVC_Comics.Models.Comic;

namespace HVC_Comics.Views.Comic
{
    public class ComicView
    {
        public ComicModel Model { get; }
        public string Name { get; }
        public string Publisher { get; }
        public string Licensor { get; }
        public string? CoverUrl { get; }
        public string EditorClass { get; }
        public string StatusClass { get; }
        public List<string> PublisherLogos { get; }
        public List<string> LicensorLogos { get; }

        public IEnumerable<string> AllLogos =>
            PublisherLogos.Concat(LicensorLogos).Distinct();

        public ComicView(ComicModel model)
        {
            Model = model ?? throw new ArgumentNullException(nameof(model));

            Name = (model.Name ?? "").Trim();
            Publisher = (model.Publisher ?? "").Trim();
            Licensor = (model.Licensor ?? "").Trim();
            CoverUrl = BuildCoverUrl(Name, model.Guid);
            EditorClass = GetEditorClass(Name, Licensor);
            StatusClass = GetStatusClass(model.ComicSituation);
            PublisherLogos = GetLogos(Publisher);
            LicensorLogos = GetLogos(Licensor);
        }

        public static List<string> GetLogos(string? value)
        {
            var logos = new List<string>();

            if (string.IsNullOrWhiteSpace(value))
                return logos;

            var parts = value.Split(['/', ','], StringSplitOptions.RemoveEmptyEntries |
                StringSplitOptions.TrimEntries
            );

            foreach (var part in parts)
            {
                var logo = part switch
                {
                    var n when n.Contains("Marvel", StringComparison.OrdinalIgnoreCase) => "~/img/logo-marvel.png",
                    var n when n.Contains("DC", StringComparison.OrdinalIgnoreCase) => "~/img/logo-dc.png",
                    var n when n.Contains("Panini", StringComparison.OrdinalIgnoreCase) => "~/img/logo-panini.png",
                    var n when n.Contains("Image", StringComparison.OrdinalIgnoreCase) => "~/img/logo-image.png",
                    var n when n.Contains("Dark Horse", StringComparison.OrdinalIgnoreCase) => "~/img/logo-dark-horse.png",
                    var n when n.Contains("IDW", StringComparison.OrdinalIgnoreCase) => "~/img/logo-idw.png",
                    var n when n.Contains("Boom", StringComparison.OrdinalIgnoreCase) => "~/img/logo-boom-studios.png",
                    var n when n.Contains("Dynamite", StringComparison.OrdinalIgnoreCase) => "~/img/logo-dynamite.png",
                    var n when n.Contains("Avatar", StringComparison.OrdinalIgnoreCase) => "~/img/logo-avatar.png",
                    var n when n.Contains("Aspen", StringComparison.OrdinalIgnoreCase) => "~/img/logo-aspen.png",
                    var n when n.Contains("Top Cow", StringComparison.OrdinalIgnoreCase) => "~/img/logo-top-cow.png",
                    var n when n.Contains("Virgin", StringComparison.OrdinalIgnoreCase) => "~/img/logo-virgin.png",
                    var n when n.Contains("Dreamwave", StringComparison.OrdinalIgnoreCase) => "~/img/logo-dreamwave.png",
                    var n when n.Contains("Activision", StringComparison.OrdinalIgnoreCase) => "~/img/logo-activision.png",
                    var n when n.Contains("Europa", StringComparison.OrdinalIgnoreCase) => "~/img/logo-europa.png",
                    _ => null
                };

                if (logo is not null && !logos.Contains(logo))
                    logos.Add(logo);
            }

            return logos;
        }

        private static string? BuildCoverUrl(string name, string? guid)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(guid))
                return null;

            char first = name[0];
            string folder = char.IsDigit(first) ? "%23" : char.ToUpperInvariant(first).ToString();
            string fileName = $"{guid.Trim()}.jpg";

            return $"/capas/{folder}/{Uri.EscapeDataString(fileName)}";
        }

        private static string GetEditorClass(string name, string licensor)
        {
            if (name.Contains("Super-Heróis", StringComparison.OrdinalIgnoreCase) ||
                name.Contains("Mundo Nerd", StringComparison.OrdinalIgnoreCase) ||
                name.Contains("Wizard", StringComparison.OrdinalIgnoreCase) ||
                name.Contains("Wizmania", StringComparison.OrdinalIgnoreCase))
            {
                return "edit-info";
            }

            if (licensor.Contains("Marvel", StringComparison.OrdinalIgnoreCase))
                return "edit-marvel";

            if (licensor.Contains("DC", StringComparison.OrdinalIgnoreCase))
                return "edit-dc";

            return "edit-outra";
        }

        private static string GetStatusClass(string? situation)
        {
            if (situation?.Contains("encerr", StringComparison.OrdinalIgnoreCase) == true)
                return "info-status-ended";

            if (situation?.Contains("public", StringComparison.OrdinalIgnoreCase) == true)
                return "info-status-published";

            return "info-status-special";
        }
    }
}
