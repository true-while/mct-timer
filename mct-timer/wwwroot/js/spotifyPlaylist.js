(function (global) {
    const spotifyPlaylistIdPattern = /^[A-Za-z0-9]{22}$/;
    const spotifyOpenHost = "open.spotify.com";

    function buildEmbedUrl(playlistId) {
        return `https://open.spotify.com/embed/playlist/${playlistId}`;
    }

    function normalizeSpotifyPlaylist(value) {
        const trimmedValue = (value || "").trim();

        if (!trimmedValue) {
            return { valid: true, playlistId: "", embedUrl: "" };
        }

        let playlistId = getPlaylistIdFromSpotifyUri(trimmedValue) ||
            getPlaylistIdFromSpotifyUrl(trimmedValue) ||
            (spotifyPlaylistIdPattern.test(trimmedValue) ? trimmedValue : "");

        if (!playlistId) {
            return { valid: false, playlistId: "", embedUrl: "" };
        }

        return {
            valid: true,
            playlistId: playlistId,
            embedUrl: buildEmbedUrl(playlistId)
        };
    }

    function getPlaylistIdFromSpotifyUri(value) {
        const parts = value.split(":").map(part => part.trim());

        if (parts.length === 3 &&
            parts[0].toLowerCase() === "spotify" &&
            parts[1].toLowerCase() === "playlist" &&
            spotifyPlaylistIdPattern.test(parts[2])) {
            return parts[2];
        }

        return "";
    }

    function getPlaylistIdFromSpotifyUrl(value) {
        let url;

        try {
            url = new URL(value);
        } catch {
            return "";
        }

        if (url.protocol !== "https:" || url.hostname.toLowerCase() !== spotifyOpenHost) {
            return "";
        }

        const pathSegments = url.pathname.split("/").filter(Boolean);

        // Spotify share URLs often include ?si=...; the embed should use only the playlist ID.
        if (pathSegments.length >= 2 &&
            pathSegments[0].toLowerCase() === "playlist" &&
            spotifyPlaylistIdPattern.test(pathSegments[1])) {
            return pathSegments[1];
        }

        if (pathSegments.length >= 3 &&
            pathSegments[0].toLowerCase() === "embed" &&
            pathSegments[1].toLowerCase() === "playlist" &&
            spotifyPlaylistIdPattern.test(pathSegments[2])) {
            return pathSegments[2];
        }

        return "";
    }

    class SpotifyPlaylistPlayer {
        constructor(root) {
            this.root = root;
            this.src = root.dataset.spotifySrc;
            this.iframe = root.querySelector("#spotify-playlist-iframe");
            this.frameWrap = root.querySelector("#spotify-player-frame-wrap");
            this.startButton = root.querySelector("#spotify-playlist-start");
            this.toggleButton = root.querySelector("#spotify-player-toggle");
            this.stopButton = root.querySelector("#spotify-player-stop");
            this.clearButton = root.querySelector("#spotify-player-clear");
            this.status = root.querySelector("#spotify-player-status");

            this.startButton.addEventListener("click", () => this.start());
            this.toggleButton.addEventListener("click", () => this.toggle());
            this.stopButton.addEventListener("click", () => this.stop());
            this.clearButton.addEventListener("click", () => this.clear());
        }

        start() {
            if (this.iframe.getAttribute("src") !== this.src) {
                this.iframe.setAttribute("src", this.src);
            }

            this.root.style.display = "";
            this.frameWrap.style.display = "";
            this.toggleButton.textContent = "Hide player";
            this.status.textContent = "Use the Spotify play button in the embedded player to start music.";
        }

        toggle() {
            const isHidden = this.frameWrap.style.display === "none";
            this.frameWrap.style.display = isHidden ? "" : "none";
            this.toggleButton.textContent = isHidden ? "Hide player" : "Show player";
        }

        stop() {
            if (!this.iframe) {
                return;
            }

            // The public Spotify iframe does not expose a lightweight pause API. Resetting the iframe
            // is the reliable no-OAuth way to stop playback when the timer ends or the user stops it.
            this.iframe.setAttribute("src", "about:blank");
            if (this.status) {
                this.status.textContent = "Music stopped. Select Play playlist to reload the Spotify player.";
            }
        }

        clear() {
            this.stop();
            this.root.remove();
        }
    }

    global.SpotifyPlaylistHelper = {
        normalize: normalizeSpotifyPlaylist,
        buildEmbedUrl: buildEmbedUrl
    };
    global.SpotifyPlaylistPlayer = SpotifyPlaylistPlayer;
})(window);
