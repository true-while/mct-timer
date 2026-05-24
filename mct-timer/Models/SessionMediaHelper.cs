namespace mct_timer.Models
{
    public enum ShowcaseMediaKind
    {
        None,
        Image,
        Video,
        Embed
    }

    public static class SessionMediaHelper
    {
        private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".gif",
            ".webp"
        };

        private static readonly HashSet<string> VideoExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".mp4",
            ".webm",
            ".ogg"
        };

        public static bool TryNormalizeMediaUrl(string? value, out string mediaUrl, out ShowcaseMediaKind kind)
        {
            mediaUrl = string.Empty;
            kind = ShowcaseMediaKind.None;

            if (string.IsNullOrWhiteSpace(value))
            {
                return true;
            }

            if (!Uri.TryCreate(value.Trim(), UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
            {
                return false;
            }

            var extension = Path.GetExtension(uri.AbsolutePath);
            if (ImageExtensions.Contains(extension))
            {
                mediaUrl = uri.ToString();
                kind = ShowcaseMediaKind.Image;
                return true;
            }

            if (VideoExtensions.Contains(extension))
            {
                mediaUrl = uri.ToString();
                kind = ShowcaseMediaKind.Video;
                return true;
            }

            if (TryNormalizeYouTubeUrl(uri, out mediaUrl) || TryNormalizeVimeoUrl(uri, out mediaUrl))
            {
                kind = ShowcaseMediaKind.Embed;
                return true;
            }

            mediaUrl = string.Empty;
            kind = ShowcaseMediaKind.None;
            return false;
        }

        private static bool TryNormalizeYouTubeUrl(Uri uri, out string embedUrl)
        {
            embedUrl = string.Empty;
            var host = uri.Host.ToLowerInvariant();
            string videoId = string.Empty;

            if (host == "youtu.be")
            {
                videoId = uri.AbsolutePath.Trim('/');
            }
            else if (host == "www.youtube.com" || host == "youtube.com")
            {
                if (uri.AbsolutePath.StartsWith("/embed/", StringComparison.OrdinalIgnoreCase))
                {
                    videoId = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? string.Empty;
                }
                else
                {
                    var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
                    videoId = query.TryGetValue("v", out var value) ? value.ToString() : string.Empty;
                }
            }

            if (!IsSafeVideoId(videoId))
            {
                return false;
            }

            embedUrl = $"https://www.youtube.com/embed/{videoId}?autoplay=1&mute=1&playsinline=1&loop=1&playlist={videoId}&rel=0";
            return true;
        }

        private static bool TryNormalizeVimeoUrl(Uri uri, out string embedUrl)
        {
            embedUrl = string.Empty;
            var host = uri.Host.ToLowerInvariant();
            if (host != "vimeo.com" && host != "www.vimeo.com" && host != "player.vimeo.com")
            {
                return false;
            }

            var videoId = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? string.Empty;
            if (!videoId.All(char.IsDigit))
            {
                return false;
            }

            embedUrl = $"https://player.vimeo.com/video/{videoId}?autoplay=1&muted=1&loop=1";
            return true;
        }

        private static bool IsSafeVideoId(string videoId)
        {
            return !string.IsNullOrWhiteSpace(videoId) &&
                videoId.Length <= 32 &&
                videoId.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '_');
        }
    }
}
