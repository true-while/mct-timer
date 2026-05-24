namespace mct_timer.Models
{
    public static class InstructorProfileHelper
    {
        public static bool TryNormalizeLinkedInUrl(string? value, out string profileUrl)
        {
            profileUrl = string.Empty;

            if (string.IsNullOrWhiteSpace(value))
            {
                return true;
            }

            var trimmed = value.Trim();
            if (!trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                trimmed = "https://" + trimmed;
            }

            if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
            {
                return false;
            }

            var host = uri.Host.ToLowerInvariant();
            if (host != "linkedin.com" && !host.EndsWith(".linkedin.com", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            profileUrl = uri.ToString();
            return true;
        }

        public static bool TryNormalizeQrCodeUrl(string? value, out string qrCodeUrl)
        {
            qrCodeUrl = string.Empty;

            if (string.IsNullOrWhiteSpace(value))
            {
                return true;
            }

            if (!SessionMediaHelper.TryNormalizeMediaUrl(value, out var mediaUrl, out var kind) ||
                kind != ShowcaseMediaKind.Image)
            {
                return false;
            }

            qrCodeUrl = mediaUrl;
            return true;
        }
    }
}
