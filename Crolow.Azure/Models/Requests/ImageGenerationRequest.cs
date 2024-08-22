namespace Crolow.AzureServices.Models.Requests
{
    public class ImageGenerationRequest : OpenAiBaseRequest
    {
        public string Size { get; set; }
        public string Quality { get; set; }
        public string Style { get; set; }
    }
}