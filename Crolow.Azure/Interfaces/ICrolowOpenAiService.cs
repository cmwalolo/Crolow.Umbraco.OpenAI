using Crolow.OpenAI.Models.Requests;
using OpenAI.Chat;
using OpenAI.Images;

namespace Crolow.OpenAI.Interfaces
{
    public interface ICrolowOpenAiService
    {
        ChatCompletion CorrectText(CorrectionRequest request);
        ChatCompletion CreateHashTags(CreateHashTagsRequest request);
        ChatCompletion DescribeImage(ImageDescriptionRequest request);
        GeneratedImage GenerateImage(ImageGenerationRequest request);
        ChatCompletion SummarizeText(SummarizeRequest request);
        ChatCompletion TranslateText(TranslationRequest request);
        ChatCompletion GenericRequest(OpenAiBaseRequest request);
    }
}