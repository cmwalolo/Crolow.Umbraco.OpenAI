namespace Crolow.AzureServices.Models.Requests
{
    public class OpenAiBaseRequest
    {
        public List<string> Prompts { get; set; }
        public List<string> SystemMessages { get; set; }

        public string SourceLanguage { get; set; }
        public string TargetLanguage { get; set; }
        public string? UserName { get; set; } = "unknown";
        public string? ResponseFormat { get; set; } = "json";
        public List<string>? StopSequences { get; set; }
        public int? ChoiceCount { get; set; }
        public float? FrequencyPenalty { get; set; }
        public float? PresencePenalty { get; set; }
        public int? MaxTokens { get; set; }
        public long? Seed { get; set; }
        public float? Temperature { get; set; }
        public bool IsHtml { get; set; }



    }
}