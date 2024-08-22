using Crolow.AzureServices.Interfaces;
using Crolow.AzureServices.Models.Configuration;
using Crolow.AzureServices.Models.Requests;
using Crolow.OpenAi.Umbraco.Interfaces;
using Crolow.OpenAi.Umbraco.Models;
using Newtonsoft.Json.Linq;
using StackExchange.Profiling.Internal;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Crolow.OpenAi.Umbraco.Services;

public class SummarizeApiService : BaseApiService, ISummarizeApiService
{
    public SummarizeApiService(IScopeProvider scopeProvider, IContentTypeService contentTypeService, IContentService contentService, ICrolowOpenAiService openAIService, OpenAiConfiguration config)
     : base(scopeProvider, contentTypeService, contentService, openAIService, config)
    {
    }

    public void Execute(SummarizeActionParameters parameters)
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

        ProcessSummarizeAll(content, parameters);
    }

    private void ProcessSummarizeAll(IContent? content, SummarizeActionParameters parameters)
    {
        var extractedValues = Extract(content, parameters);

        SummarizeRequest request = new SummarizeRequest()
        {
            ResponseFormat = "json",
            Prompts = new[] { extractedValues.ToJson() }.ToList(),
            SourceLanguage = parameters.SourceLanguage,
            TargetLanguage = parameters.TargetLanguage,
        };
        if (!string.IsNullOrEmpty(parameters.Role))
        {
            request.SystemMessages = openAIConfig.Services?.Summaries?.Roles?.FirstOrDefault(p => p.Name == parameters.Role)?.Prompts?.ToList();
        }

        var result = openAIService.SummarizeText(request);
        extractedValues = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(result.Content.First().Text);

        //Update(content, parameters, extractedValues);
        foreach (var entry in extractedValues)
        {
            switch (entry.Key)
            {
                case "title":
                    UpdateContent(content, entry.Value, openAIConfig.Services.Summaries.TitleFields, parameters);
                    break;
                case "teaser":
                    UpdateContent(content, entry.Value, openAIConfig.Services.Summaries.TeaserFields, parameters);
                    break;
                case "summary":
                    var value = new JObject();
                    value["markup"] = entry.Value;
                    UpdateContent(content, value.ToJson(), openAIConfig.Services.Summaries.DescriptionFields, parameters);
                    break;
            }
        }

        contentService.Save(content);

        if (parameters.Recursive)
        {
            // Execute Children
            long count = 0;
            var children = contentService.GetPagedChildren(content.Id, 0, int.MaxValue, out count);
            foreach (var child in children)
            {
                ProcessSummarizeAll(child, parameters);
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
    protected override DataTypeMapping CreateMapping(string dataType)
    {
        var config = openAIConfig.Services.Summaries.DataTypeMappings.Where(p => p.Alias == "default").FirstOrDefault();
        var config2 = openAIConfig.Services.Summaries.DataTypeMappings.Where(p => p.Alias == dataType).FirstOrDefault();

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