/**
 * Client-side prompt validation for AI image generation
 * Mirrors the server-side UploadValidator.ValidatePrompt logic
 */
class PromptValidator {
    constructor() {
        // Banned keywords that violate content policies
        this.bannedKeywords = [
            // Violence and harm
            "violence", "violent", "gore", "gory", "kill", "death", "murder", "blood", "torture",
            "weapon", "bomb", "gun", "knife", "illegal",
            
            // Adult content
            "nude", "naked", "pornographic", "porn", "sex", "xxx", "adult",
            
            // Hate and discrimination
            "hate", "racist", "racism", "sexist", "sexism", "discrimination",
            
            // Self-harm
            "suicide", "self-harm", "cutting", "overdose",
            
            // Illegal activities
            "drug", "cocaine", "heroin", "meth", "counterfeit", "theft", "steal",
            
            // Harassment
            "harass", "bully", "bullying", "defame", "slander",
            
            // Misinformation
            "fake news", "conspiracy", "hoax"
        ];

        // Patterns to detect suspicious requests
        this.suspiciousPatterns = [
            /\b(real|actual|photo|photograph)\b.*\b(person|people|child|children)\b/i,
            /\b(deepfake|fake)\b.*\b(person|politician|celebrity)\b/i,
            /bypass|circumvent|ignore.*policy|rule|filter/i
        ];
    }

    /**
     * Validates a prompt for AI image generation
     * @param {string} prompt - The prompt to validate
     * @returns {object} { isValid: boolean, reason: string }
     */
    validate(prompt) {
        // Null or empty check
        if (!prompt || prompt.trim() === "") {
            return { isValid: false, reason: "Prompt cannot be empty." };
        }

        // Length check (prevent excessive prompts)
        if (prompt.length > 2000) {
            return { isValid: false, reason: "Prompt is too long (max 2000 characters)." };
        }

        // Check for banned keywords
        const lowerPrompt = prompt.toLowerCase();
        for (const keyword of this.bannedKeywords) {
            if (lowerPrompt.includes(keyword.toLowerCase())) {
                return { 
                    isValid: false, 
                    reason: `Prompt contains prohibited content: '${keyword}'.` 
                };
            }
        }

        // Check for suspicious patterns
        for (const pattern of this.suspiciousPatterns) {
            if (pattern.test(prompt)) {
                return { 
                    isValid: false, 
                    reason: "Prompt matches suspicious pattern that may violate policies." 
                };
            }
        }

        return { isValid: true, reason: "" };
    }
}

// Create a singleton instance
const promptValidator = new PromptValidator();
