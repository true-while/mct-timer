using Ixnas.AltchaNet;

namespace mct_timer.Models
{
    public class InMemoryStore : IAltchaChallengeStore
    {
        private class StoredChallenge
        {
            public string Challenge { get; set; }
            public DateTimeOffset ExpiryUtc { get; set; }
        }

        private readonly List<StoredChallenge> _stored = new List<StoredChallenge>();

        public Task Store(string challenge, DateTimeOffset expiryUtc)
        {
            var challengeToStore = new StoredChallenge
            {
                Challenge = challenge,
                ExpiryUtc = expiryUtc
            };
            _stored.Add(challengeToStore);
            return Task.CompletedTask;
        }

        public Task<bool> Exists(string challenge)
        {
            _stored.RemoveAll(storedChallenge => storedChallenge.ExpiryUtc <= DateTimeOffset.UtcNow);
            var exists = _stored.Exists(storedChallenge => storedChallenge.Challenge == challenge);
            return Task.FromResult(exists);
        }
    }
}
