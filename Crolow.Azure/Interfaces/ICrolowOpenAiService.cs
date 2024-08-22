using Crolow.AzureServices.Models.Requests;
using OpenAI.Chat;
using OpenAI.Images;

namespace Crolow.AzureServices.Interfaces
{
    public interface ICrolowOpenAiService
    {
        ChatCompletion CorrectText(CorrectionRequest request);
        ChatCompletion CreateHashTags(CreateHashTagsRequest request);
        ChatCompletion DescribeImage(ImageDescriptionRequest request);
        GeneratedImage GenerateImage(ImageGenerationRequest request);
        ChatCompletion SummarizeText(SummarizeRequest request);
        ChatCompletion TranslateText(TranslationRequest request);
    }
}