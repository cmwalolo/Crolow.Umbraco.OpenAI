namespace Crolow.AzureServices.Models.Requests
{
    public class CreateHashTagsRequest : OpenAiBaseRequest
    {
        public int AmountOfTags { get; set; }
        public string? OutputFormat { get; set; }
    }
}