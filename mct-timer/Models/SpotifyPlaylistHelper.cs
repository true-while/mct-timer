using System.Text.RegularExpressions;

namespace mct_timer.Models
{
    public static class SpotifyPlaylistHelper
    {
        private static readonly Regex PlaylistIdPattern = new("^[A-Za-z0-9]{22}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private const string SpotifyOpenHost = "open.spotify.com";

        public static bool TryNormalizeEmbedUrl(string? value, out string embedUrl)
        {
            embedUrl = string.Empty;

            if (!TryGetPlaylistId(value, out var playlistId))
            {
                return false;
            }

            embedUrl = BuildEmbedUrl(playlistId);
            return true;
        }

        public static bool TryGetPlaylistId(string? value, out string playlistId)
        {
            playlistId = string.Empty;
            var trimmedValue = value?.Trim();

            if (string.IsNullOrWhiteSpace(trimmedValue))
            {
                return false;
            }

            if (TryGetPlaylistIdFromSpotifyUri(trimmedValue, out playlistId))
            {
                return true;
            }

            if (TryGetPlaylistIdFromSpotifyUrl(trimmedValue, out playlistId))
            {
                return true;
            }

            if (IsValidPlaylistId(trimmedValue))
            {
                playlistId = trimmedValue;
                return true;
            }

            return false;
        }

        public static string BuildEmbedUrl(string playlistId)
        {
            return $"https://open.spotify.com/embed/playlist/{playlistId}";
        }

        private static bool TryGetPlaylistIdFromSpotifyUri(string value, out string playlistId)
        {
            playlistId = string.Empty;
            var parts = value.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (parts.Length != 3 ||
                !string.Equals(parts[0], "spotify", StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(parts[1], "playlist", StringComparison.OrdinalIgnoreCase) ||
                !IsValidPlaylistId(parts[2]))
            {
                return false;
            }

            playlistId = parts[2];
            return true;
        }

        private static bool TryGetPlaylistIdFromSpotifyUrl(string value, out string playlistId)
        {
            playlistId = string.Empty;

            if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
                !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(uri.Host, SpotifyOpenHost, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var pathSegments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            // Spotify has two public URL shapes for playlists. Query strings are intentionally ignored
            // so a shared playlist URL becomes a stable, clean embed URL for the timer session.
            if (pathSegments.Length >= 2 &&
                string.Equals(pathSegments[0], "playlist", StringComparison.OrdinalIgnoreCase) &&
                IsValidPlaylistId(pathSegments[1]))
            {
                playlistId = pathSegments[1];
                return true;
            }

            if (pathSegments.Length >= 3 &&
                string.Equals(pathSegments[0], "embed", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(pathSegments[1], "playlist", StringComparison.OrdinalIgnoreCase) &&
                IsValidPlaylistId(pathSegments[2]))
            {
                playlistId = pathSegments[2];
                return true;
            }

            return false;
        }

        private static bool IsValidPlaylistId(string value)
        {
            return PlaylistIdPattern.IsMatch(value);
        }
    }
}
