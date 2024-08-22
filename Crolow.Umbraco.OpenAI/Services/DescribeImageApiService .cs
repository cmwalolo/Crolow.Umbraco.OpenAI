using Crolow.AzureServices.Interfaces;
using Crolow.AzureServices.Models.Configuration;
using Crolow.AzureServices.Models.Requests;
using Crolow.OpenAi.Umbraco.Interfaces;
using Crolow.OpenAi.Umbraco.Models;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json.Linq;
using StackExchange.Profiling.Internal;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Crolow.OpenAi.Umbraco.Services;

public class DescribeImageApiService : BaseApiService, IDescribeImageApiService
{
    protected readonly IUmbracoContextFactory contextFactory;
    public DescribeImageApiService(IUmbracoContextFactory contextFactory, IScopeProvider scopeProvider, IContentTypeService contentTypeService, IContentService contentService, ICrolowOpenAiService openAIService, OpenAiConfiguration config)
     : base(scopeProvider, contentTypeService, contentService, openAIService, config)
    {
        this.contextFactory = contextFactory;
    }

    public void Execute(DescribeImageActionParameters parameters)
    {
        IContent content = null;
        try
        {
            var nodeId = new GuidUdi(new Uri(parameters.NodeId)).Guid;
            content = contentService.GetById(nodeId);
        }
        catch
        {
            content = contentService.GetById(int.Parse(parameters.NodeId));
        }

        ProcessAll(content, parameters);
    }

    private void ProcessAll(IContent? content, DescribeImageActionParameters parameters)
    {
        using var factory = contextFactory.EnsureUmbracoContext();

        //        var extractedValues = Extract(content, parameters);

        if (openAIConfig.Services.DescribeImages.Settings.Any(p => p.Template == content.ContentType.Alias))
        {
            var config = openAIConfig.Services.DescribeImages.Settings.First(p => p.Template == content.ContentType.Alias);

            ImageDescriptionRequest request = new ImageDescriptionRequest()
            {
                ResponseFormat = "json",
                SourceLanguage = parameters.SourceLanguage,
                TargetLanguage = parameters.TargetLanguage,
            };

            if (!string.IsNullOrEmpty(parameters.Role))
            {
                request.SystemMessages = openAIConfig.Services?.DescribeImages?.Roles?.FirstOrDefault(p => p.Name == parameters.Role)?.Prompts?.ToList();
            }

            if (!string.IsNullOrEmpty(config.HintField))
            {
                var hint = content.GetValue<string>(config.HintField, request.SourceLanguage).FromJson<dynamic>();
                if (hint != null && hint.markup != null)
                {
                    request.Hint = hint.markup;
                }
            }

            var media = content.GetValue<string>(config.ImageField).FromJson<dynamic>();
            if (media != null)
            {
                var mediaItem = factory.UmbracoContext.Media.GetById(new Guid(media[0].mediaKey.ToString()));
                var url = mediaItem.Url(request.SourceLanguage, UrlMode.Absolute);
                Console.Write(url);
                request.ImageUrl = url; // url.Replace("https://localhost:44351", "https://crolow.eu").ToString();
            }

            var result = openAIService.DescribeImage(request);
            var extractedValues = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(result.Content.First().Text);

            if (extractedValues["description"] != null)
            {
                var desc = new JObject();
                desc["markup"] = extractedValues["description"].ToString();
                content.SetValue(config.OutputField, desc.ToJson(), request.SourceLanguage);
            }

            if (extractedValues["tags"] != null)
            {
                var tags = extractedValues["tags"].Select(p => p.ToString()).ToArray();
                content.SetValue(config.OutputTagField, tags.ToJson(), request.SourceLanguage);
            }

            if (extractedValues["hashtags"] != null)
            {
                var hashtags = string.Join(" ", extractedValues["hashtags"].Select(p => p.ToString()));
                content.SetValue(config.OutputHashtagField, hashtags, request.SourceLanguage);
            }

            contentService.Save(content);
        }

        if (parameters.Recursive)
        {
            // Execute Children
            long count = 0;
            var children = contentService.GetPagedChildren(content.Id, 0, int.MaxValue, out count);
            foreach (var child in children)
            {
                ProcessAll(child, parameters);
            }
        }
    }
    private void UpdateContent(IContent content, string value, string[] fields, SummarizeActionParameters parameters)
    {
        foreach (var field in fields)
        {
            if (content.HasProperty(field))
            {
                content.SetValue(field, value, parameters.SourceLanguage);
            }
        }
    }
}