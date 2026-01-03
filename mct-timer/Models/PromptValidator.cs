using System.Text.RegularExpressions;
using Microsoft.ApplicationInsights;

namespace mct_timer.Models
{
    public interface IPromptValidator
    {
        /// <summary>
        /// Validates if a prompt is acceptable for image generation
        /// </summary>
        /// <returns>ValidationResult with IsValid flag and reason if invalid</returns>
        ValidationResult ValidatePrompt(string prompt);
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string Reason { get; set; }

        public ValidationResult(bool isValid, string reason = "")
        {
            IsValid = isValid;
            Reason = reason;
        }
    }

    public class PromptValidator : IPromptValidator
    {
        private readonly TelemetryClient _logger;

        // Banned keywords that violate content policies
        private static readonly HashSet<string> BannedKeywords = new(StringComparer.OrdinalIgnoreCase)
        {
            // Violence and harm
            "violence", "violent", "gore", "gory", "kill", "death", "murder", "blood", "torture",
            "weapon", "bomb", "gun", "knife", "weapon", "illegal",
            
            // Adult content
            "nude", "naked", "pornographic", "porn", "sex", "xxx", "adult",
            
            // Hate and discrimination
            "hate", "racist", "racism", "sexist", "sexism", "discrimination",
            
            // Self-harm
            "suicide", "self-harm", "cutting", "overdose",
            
            // Illegal activities
            "drug", "cocaine", "heroin", "meth", "illegal", "counterfeit", "theft", "steal",
            
            // Harassment
            "harass", "bully", "bullying", "defame", "slander",
            
            // Misinformation
            "fake news", "conspiracy", "hoax",
        };

        // Patterns to detect suspicious requests
        private static readonly Regex[] SuspiciousPatterns = new[]
        {
            new Regex(@"\b(real|actual|photo|photograph)\b.*\b(person|people|child|children)\b", RegexOptions.IgnoreCase),
            new Regex(@"\b(deepfake|fake)\b.*\b(person|politician|celebrity)\b", RegexOptions.IgnoreCase),
            new Regex(@"bypass|circumvent|ignore.*policy|rule|filter", RegexOptions.IgnoreCase),
        };

        public PromptValidator(TelemetryClient logger)
        {
            _logger = logger;
        }

        public ValidationResult ValidatePrompt(string prompt)
        {
            // Null or empty check
            if (string.IsNullOrWhiteSpace(prompt))
            {
                return new ValidationResult(false, "Prompt cannot be empty");
            }

            // Length check (prevent excessive prompts)
            if (prompt.Length > 2000)
            {
                return new ValidationResult(false, "Prompt is too long (max 2000 characters)");
            }

            // Check for banned keywords
            var bannedWordFound = BannedKeywords.FirstOrDefault(keyword =>
                prompt.Contains(keyword, StringComparison.OrdinalIgnoreCase));

            if (bannedWordFound != null)
            {
                var reason = $"Prompt contains prohibited content: '{bannedWordFound}'";
                _logger.TrackTrace($"Prompt rejected: {reason}", severityLevel: Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Information);
                return new ValidationResult(false, reason);
            }

            // Check for suspicious patterns
            foreach (var pattern in SuspiciousPatterns)
            {
                if (pattern.IsMatch(prompt))
                {
                    var reason = "Prompt matches suspicious pattern that may violate policies";
                    _logger.TrackTrace($"Prompt flagged: {reason}", severityLevel: Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Warning);
                    return new ValidationResult(false, reason);
                }
            }

            // Check for excessive special characters (potential injection attempt)
            int specialCharCount = prompt.Count(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c));
            if (specialCharCount > prompt.Length * 0.3)
            {
                return new ValidationResult(false, "Prompt contains excessive special characters");
            }

            // Check for repeated characters (spam pattern)
            if (HasExcessiveRepetition(prompt))
            {
                return new ValidationResult(false, "Prompt contains excessive character repetition");
            }

            _logger.TrackTrace($"Prompt validated successfully", severityLevel: Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
            return new ValidationResult(true, "");
        }

        private static bool HasExcessiveRepetition(string text)
        {
            // Check if any character is repeated more than 5 times in a row
            for (int i = 0; i < text.Length - 5; i++)
            {
                if (text[i] == text[i + 1] && text[i + 1] == text[i + 2] &&
                    text[i + 2] == text[i + 3] && text[i + 3] == text[i + 4])
                {
                    return true;
                }
            }
            return false;
        }
    }
}
