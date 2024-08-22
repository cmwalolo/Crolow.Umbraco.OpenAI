using Crolow.AzureServices.Interfaces;
using Crolow.AzureServices.Models.Requests;
using Microsoft.AspNetCore.Mvc;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Images;
using System.Net;

namespace Crolow.AzureServices.Services
{
    public class CrolowOpenAiService : ICrolowOpenAiService
    {
        private readonly string DefaultModel = /*"gpt-3.5-turbo-0125";*/ "gpt-4o";
        private readonly OpenAIClient _openAiClient;

        public CrolowOpenAiService(OpenAIClient openAiClient)
        {
            _openAiClient = openAiClient;
        }

        public GeneratedImage GenerateImage(ImageGenerationRequest request)
        {
            var client = _openAiClient.GetImageClient("dal-e-3");
            var options = new ImageGenerationOptions()
            {
                ResponseFormat = GeneratedImageFormat.Uri,
                Size = GeneratedImageSize.W1024xH1792,
                Style = GeneratedImageStyle.Vivid,
                Quality = GeneratedImageQuality.High,
                User = request.UserName ?? "",
            };

            var prompt = string.Join(" ", request.Prompts);

            var result = client.GenerateImageAsync(prompt, options);
            // Image Generations responses provide URLs you can use to retrieve requested images
            return result.Result.Value;
        }


        [HttpPost("describe-image")]
        public ChatCompletion DescribeImage(ImageDescriptionRequest request)
        {
            var client = _openAiClient.GetChatClient(DefaultModel);
            var messages = new List<ChatMessage>();

            var options = BuildBaseOptions((OpenAiBaseRequest)request);
            messages.AddRange(GetSystemMessages("You are a helpful creative writer.", request.SystemMessages, request.ResponseFormat));
            messages.Add(new SystemChatMessage($"Describe the given image"));

            messages.Add(new SystemChatMessage($"Response Language should be {request.SourceLanguage}."));
            messages.Add(new SystemChatMessage($"The ouput should be in JSON with a description, an array of tags and an array of hashtags property"));


            using var webClient = new WebClient();
            byte[] imageBytes = webClient.DownloadData(request.ImageUrl);

            messages.Add(new UserChatMessage(new ChatMessageContentPart[]
            { ChatMessageContentPart.CreateImageMessageContentPart(new BinaryData(imageBytes), "image/jpg") }));

            if (!string.IsNullOrEmpty(request.Hint))
            {
                messages.Add(new UserChatMessage("Use the next prompt as a hint to help you transform the description"));
                messages.Add(new UserChatMessage(request.Hint));
            }

            var results = client.CompleteChatAsync(messages, options);
            return results.Result.Value;
        }

        [HttpPost("translate-text")]
        public ChatCompletion TranslateText(TranslationRequest request)
        {
            var client = _openAiClient.GetChatClient(DefaultModel);
            var messages = new List<ChatMessage>();

            var options = BuildBaseOptions((OpenAiBaseRequest)request);
            messages.AddRange(GetSystemMessages("You are a helpful creative translator.", request.SystemMessages, request.ResponseFormat));
            messages.Add(new SystemChatMessage("$Translate the give JSON property values from {request.SourceLanguage} to {request.TargetLanguage}"));

            if (request.Prompts != null)
            {
                messages.Add(new OpenAI.Chat.UserChatMessage(string.Join("\n", request.Prompts)));
            }

            var results = client.CompleteChatAsync(messages, options);
            return results.Result.Value;
        }

        [HttpPost("correct-text")]
        public ChatCompletion CorrectText(CorrectionRequest request)
        {
            var client = _openAiClient.GetChatClient(DefaultModel);
            var messages = new List<ChatMessage>();

            var options = BuildBaseOptions((OpenAiBaseRequest)request);
            messages.AddRange(GetSystemMessages("You are a helpful creative corrector.", request.SystemMessages, request.ResponseFormat));
            messages.Add(new SystemChatMessage("Correct the given JSON and preserve the structure. Used language is {request.SourceLanguage}"));

            if (request.Prompts != null)
            {
                messages.Add(new OpenAI.Chat.UserChatMessage(string.Join("\n", request.Prompts)));
            }

            var results = client.CompleteChatAsync(messages, options);
            return results.Result.Value;
        }

        [HttpPost("summarize-text")]
        public ChatCompletion SummarizeText(SummarizeRequest request)
        {
            var client = _openAiClient.GetChatClient(DefaultModel);
            var messages = new List<ChatMessage>();

            var options = BuildBaseOptions((OpenAiBaseRequest)request);
            messages.AddRange(GetSystemMessages("You are a helpful, creative and teasing metadata writer.", request.SystemMessages, request.ResponseFormat));


            if (request.Prompts != null)
            {
                messages.Add($"Extract the text from the given JSON and write me a title, a teaser and a summary of the following text. Text language is {request.SourceLanguage}");
                messages.Add($"The output is a a JSON object with a title, a teaser and a summary field");
                messages.Add(new OpenAI.Chat.UserChatMessage(string.Join("\n", request.Prompts)));
            }


            var results = client.CompleteChatAsync(messages, options);
            return results.Result.Value;
        }

        [HttpPost("create-hashtags")]
        public ChatCompletion CreateHashTags(CreateHashTagsRequest request)
        {
            var client = _openAiClient.GetChatClient(DefaultModel);
            var messages = new List<ChatMessage>();

            if (request.Prompts != null)
            {
                messages.Add($"Create a maximum {request.AmountOfTags} of popular hashtags found on Instagram. The output Hashtags language is {request.TargetLanguage}. Used language is {request.SourceLanguage}.");
                messages.Add(new OpenAI.Chat.UserChatMessage(string.Join("\n", request.Prompts)));
            }

            var options = BuildBaseOptions((OpenAiBaseRequest)request);
            messages.AddRange(GetSystemMessages("You are a helpful metadata writer..", request.SystemMessages, request.ResponseFormat));

            var results = client.CompleteChatAsync(messages, options);
            return results.Result.Value;
        }

        private IList<SystemChatMessage> GetSystemMessages(string defaultMessage, List<string> messages, string format = "", bool keepHtml = true)
        {
            var result = new List<SystemChatMessage>();
            if (messages != null && messages.Any())
            {
                foreach (var message in messages)
                {
                    result.Add(new SystemChatMessage(message));
                }
            }
            else
            {
                result.Add(new SystemChatMessage(defaultMessage));
            }

            if (!string.IsNullOrEmpty(format))
            {
                result.Add(new SystemChatMessage("Result format will be returned as JSON. Field used as response is correctedText"));
            }

            if (keepHtml)
            {
                result.Add(new SystemChatMessage("Result format will keep the HTML tags"));
            }

            return result;
        }

        private List<UserChatMessage> GetUserMessages(List<string> prompts, string imageUrl)
        {
            var content = new List<UserChatMessage>();

            if (prompts != null)
            {
                foreach (var prompt in prompts)
                {
                    if (prompt != null)
                    {
                        content.Add(new UserChatMessage(prompt));
                    }
                }
            }
            return content;
        }

        private ChatCompletionOptions BuildBaseOptions(OpenAiBaseRequest request)
        {
            var options = new OpenAI.Chat.ChatCompletionOptions()
            {
                User = request.UserName,
                //                options.Count = request.ChoiceCount;
                FrequencyPenalty = request.FrequencyPenalty,
                MaxTokens = request.MaxTokens,
                PresencePenalty = request.PresencePenalty,
                ResponseFormat = request.ResponseFormat.Equals("json") ? ChatResponseFormat.JsonObject : ChatResponseFormat.Text,
                Seed = request.Seed,
                Temperature = 0.6f,
                TopP = 0.6f
                //zTemperature = request.Temperature,
            };

            return options;
        }

    }
}