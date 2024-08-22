namespace Crolow.AzureServices.Models.Requests
{
    public class ImageDescriptionRequest : OpenAiBaseRequest
    {
        public string ImageUrl { get; set; }
        public string Hint { get; set; }
    }
}