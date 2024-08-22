using Crolow.AzureServices.Interfaces;
using Crolow.AzureServices.Models.Configuration;
using Crolow.AzureServices.Models.Requests;
using Crolow.OpenAi.Umbraco.Interfaces;
using Crolow.OpenAi.Umbraco.Models;
using StackExchange.Profiling.Internal;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Crolow.OpenAi.Umbraco.Services;

public class TranslateApiService : BaseApiService, ITranslateApiService
{
    public TranslateApiService(IScopeProvider scopeProvider, IContentTypeService contentTypeService, IContentService contentService, ICrolowOpenAiService openAIService, OpenAiConfiguration config)
     : base(scopeProvider, contentTypeService, contentService, openAIService, config)
    {
    }

    public void Execute(TranslateActionParameters parameters)
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

        ProcessTranslateAll(content, parameters);
    }

    private void ProcessTranslateAll(IContent? content, TranslateActionParameters parameters)
    {
        if (!parameters.SkipIfExists || parameters.SkipIfExists && !content.AvailableCultures.Contains(parameters.TargetLanguage))
        {
            if (content.ContentType.VariesByCulture())
            {
                var extractedValues = Extract(content, parameters);

                TranslationRequest request = new TranslationRequest()
                {
                    ResponseFormat = "json",
                    Prompts = new[] { extractedValues.ToJson() }.ToList(),
                    SourceLanguage = parameters.SourceLanguage,
                    TargetLanguage = parameters.TargetLanguage,
                };

                if (!string.IsNullOrEmpty(parameters.Role))
                {
                    request.SystemMessages = openAIConfig.Services?.Translations?.Roles?.FirstOrDefault(p => p.Name == parameters.Role)?.Prompts?.ToList();
                }

                var result = openAIService.TranslateText(request);
                extractedValues = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(result.Content.First().Text);


                if (!content.AvailableCultures.Contains(parameters.TargetLanguage))
                {
                    CreateContentVariant(content, request.SourceLanguage, request.TargetLanguage);
                }

                Update(content, parameters, extractedValues);

                contentService.Save(content);
            }
        }
        // Execute Children
        if (parameters.Recursive)
        {
            long count = 0;
            var children = contentService.GetPagedChildren(content.Id, 0, int.MaxValue, out count);
            foreach (var child in children)
            {
                ProcessTranslateAll(child, parameters);
            }
        }
    }
    protected override DataTypeMapping CreateMapping(string dataType)
    {
        var config = openAIConfig.Services.Translations.DataTypeMappings.Where(p => p.Alias == "default").FirstOrDefault();
        var config2 = openAIConfig.Services.Translations.DataTypeMappings.Where(p => p.Alias == dataType).FirstOrDefault();

        DataTypeMapping mapping = new DataTypeMapping();
        mapping.Fields = new List<string>();
        if (config != null)
        {
            mapping.Fields.AddRange(config.Fields);
        }
        if (config2 != null)
        {
            mapping.Fields.AddRange(config2.Fields);
        }
        return mapping;
    }
}