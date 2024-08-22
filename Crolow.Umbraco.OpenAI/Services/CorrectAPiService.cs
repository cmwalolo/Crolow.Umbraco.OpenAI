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

public class CorrectApiService : BaseApiService, ICorrectApiService
{
    public CorrectApiService(IScopeProvider scopeProvider, IContentTypeService contentTypeService, IContentService contentService, ICrolowOpenAiService openAIService, OpenAiConfiguration config)
     : base(scopeProvider, contentTypeService, contentService, openAIService, config)
    {
    }

    public void Execute(CorrectActionParameters parameters)
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

        ProcessCorrectAll(content, parameters);
    }

    private void ProcessCorrectAll(IContent? content, CorrectActionParameters parameters)
    {
        var extractedValues = Extract(content, parameters);


        CorrectionRequest request = new CorrectionRequest()
        {
            ResponseFormat = "json",
            Prompts = new[] { extractedValues.ToJson() }.ToList(),
            SourceLanguage = parameters.SourceLanguage,
            TargetLanguage = parameters.SourceLanguage,
        };

        if (!string.IsNullOrEmpty(parameters.Role))
        {
            request.SystemMessages = openAIConfig.Services?.Corrections?.Roles?.FirstOrDefault(p => p.Name == parameters.Role)?.Prompts?.ToList();
        }
        var result = openAIService.CorrectText(request);
        extractedValues = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(result.Content.First().Text);
        Update(content, parameters, extractedValues);

        contentService.Save(content);

        if (parameters.Recursive)
        {
            // Execute Children
            long count = 0;
            var children = contentService.GetPagedChildren(content.Id, 0, int.MaxValue, out count);
            foreach (var child in children)
            {
                ProcessCorrectAll(child, parameters);
            }
        }
    }


    protected override DataTypeMapping CreateMapping(string dataType)
    {
        var config = openAIConfig.Services.Corrections.DataTypeMappings.Where(p => p.Alias == "default").FirstOrDefault();
        var config2 = openAIConfig.Services.Corrections.DataTypeMappings.Where(p => p.Alias == dataType).FirstOrDefault();

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