using Crolow.AzureServices.Interfaces;
using Crolow.AzureServices.Models.Configuration;
using Crolow.OpenAi.Umbraco.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StackExchange.Profiling.Internal;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Crolow.OpenAi.Umbraco.Services;

public class BaseApiService
{
    protected readonly IScopeProvider scopeProvider;
    protected readonly IContentService contentService;
    protected readonly OpenAiConfiguration openAIConfig;
    protected readonly ICrolowOpenAiService openAIService;
    protected readonly IContentTypeService contentTypeService;


    public BaseApiService(IScopeProvider scopeProvider, IContentTypeService contentTypeService, IContentService contentService, ICrolowOpenAiService openAIService, OpenAiConfiguration config)
    {
        this.scopeProvider = scopeProvider;
        this.contentTypeService = contentTypeService;
        this.contentService = contentService;
        openAIConfig = config;
        this.openAIService = openAIService;
    }

    protected virtual DataTypeMapping CreateMapping(string dataType)
    {
        return null;
    }

    #region Extract methods
    protected void ExtractBlock(BaseActionParameters parameters, JToken data, Dictionary<string, string> extractedValues)
    {
        var contentId = new Guid(data["contentTypeKey"]?.Value<string>() ?? Guid.Empty.ToString());
        var udi = data.Value<string>("udi");

        if (contentId != Guid.Empty)
        {
            var contentType = contentTypeService.Get(contentId);
            DataTypeMapping mapping = CreateMapping(contentType.Alias);

            foreach (var pty in contentType.PropertyTypes)
            {
                switch (pty.PropertyEditorAlias)
                {
                    case "Umbraco.TinyMCE":
                        {
                            var ptyValue = data.Value<string>(pty.Alias);
                            if (!string.IsNullOrEmpty(ptyValue))
                            {
                                var value = JsonConvert.DeserializeObject<JObject>(ptyValue);
                                extractedValues.Add($"{udi}-{pty.Alias}", value.Value<string>("markup"));
                            }
                        }
                        break;
                    case "Umbraco.TextBox":
                        {
                            var ptyValue = data.Value<string>(pty.Alias);
                            if (!string.IsNullOrEmpty(ptyValue))
                            {
                                extractedValues.Add($"{udi}-{pty.Alias}", ptyValue);
                            }
                        }
                        break;
                    case "Umbraco.Tags":
                        {
                            var ptyValue = data[pty.Alias];
                            if (!string.IsNullOrEmpty(ptyValue.Value<string>()))
                            {
                                var values = string.Join(',', ptyValue.Value<string>());
                                extractedValues.Add($"{udi}-{pty.Alias}", values);
                            }
                        }
                        break;
                    case "Umbraco.BlockGrid":
                        {
                            ExtractBlock(parameters, data.Value<JToken>(pty.Alias), extractedValues);
                        }
                        break;
                    case "Umbraco.BlockList":
                        {
                            var value = data.Value<JToken>(pty.Alias);
                            if (value != null)
                            {
                                var contentData = value.Values("contentData");
                                foreach (var content in contentData)
                                {
                                    ExtractBlock(parameters, content, extractedValues);
                                }
                            }
                        }
                        break;
                }
            }
        }
    }

    protected Dictionary<string, string> Extract(IContent? content, BaseActionParameters parameters)
    {
        var extractedValues = new Dictionary<string, string>();
        DataTypeMapping mapping = CreateMapping(content.ContentType.Alias);

        extractedValues.Add("@Name", content.Name);

        foreach (var field in mapping.Fields)
        {
            if (content.HasProperty(field))
            {
                var pty = content.Properties.FirstOrDefault(p => p.Alias == field);

                switch (pty.PropertyType.PropertyEditorAlias)
                {
                    case "Umbraco.TinyMCE":
                        {
                            var value = content.GetValue<string>(field, parameters.SourceLanguage).FromJson<dynamic>();
                            if (value != null && value.markup != null)
                            {
                                extractedValues.Add(field, value.markup.ToString());
                            }
                        }
                        break;
                    case "Umbraco.TextBox":
                        {
                            var value = content.GetValue<string>(field, parameters.SourceLanguage ?? "fr");
                            if (!string.IsNullOrEmpty(value))
                            {
                                extractedValues.Add(field, value);
                            }
                        }
                        break;
                    case "Umbraco.Tags":
                        {
                            var value = content.GetValue<string>(field, parameters.SourceLanguage).FromJson<dynamic>();
                            if (value != null)
                            {
                                extractedValues.Add(field, string.Join(',', value));
                            }
                        }
                        break;
                    case "Umbraco.BlockGrid":
                    case "Umbraco.BlockList":
                        {
                            var value = content.GetValue<string>(field, parameters.SourceLanguage).FromJson<JObject>();
                            if (value != null && value["contentData"] != null)
                            {
                                var contentData = value["contentData"].Children();
                                foreach (var data in contentData)
                                {
                                    ExtractBlock(parameters, data, extractedValues);
                                }
                            }
                        }
                        break;

                }
            }
        }
        return extractedValues;
    }

    #endregion

    protected bool UpdateBlock(BaseActionParameters parameters, JToken data, Dictionary<string, string> extractedValues)
    {
        var modified = false;
        var contentId = new Guid(data["contentTypeKey"]?.Value<string>() ?? Guid.Empty.ToString());
        var udi = data.Value<string>("udi");

        if (contentId != Guid.Empty)
        {
            var contentType = contentTypeService.Get(contentId);
            DataTypeMapping mapping = CreateMapping(contentType.Alias);

            foreach (var pty in contentType.PropertyTypes)
            {
                var key = $"{udi.ToString()}-{pty.Alias}";
                var updatedValue = string.Empty;
                if (extractedValues.ContainsKey(key))
                {
                    updatedValue = extractedValues[key];
                }


                switch (pty.PropertyEditorAlias)
                {
                    case "Umbraco.BlockGrid":
                        {
                            modified = UpdateBlock(parameters, data[pty.Alias], extractedValues);
                        }
                        break;

                    case "Umbraco.BlockList":
                        {
                            var value = data.Value<JToken>(pty.Alias);
                            if (value != null)
                            {
                                var contentData = value.Values("contentData");
                                foreach (var content in contentData)
                                {
                                    UpdateBlock(parameters, content, extractedValues);
                                }
                            }
                        }
                        break;
                }

                if (!string.IsNullOrEmpty(updatedValue))
                {
                    switch (pty.PropertyEditorAlias)
                    {
                        case "Umbraco.TinyMCE":
                            {
                                var ptyValue = data.Value<string>(pty.Alias);
                                var value = JsonConvert.DeserializeObject<JObject>(ptyValue);

                                if (!updatedValue.Equals(value.Value<string>("markup")))
                                {
                                    value["markup"] = updatedValue;
                                    data[pty.Alias] = value;
                                    modified = true;
                                }
                            }
                            break;
                        case "Umbraco.TextBox":
                            {
                                var ptyValue = data[pty.Alias];
                                data[pty.Alias] = updatedValue;
                                modified = true;
                            }
                            break;
                        case "Umbraco.Tags":
                            {
                                var ptyValue = data[pty.Alias];
                                var values = string.Join(',', ptyValue.Value<string>());
                                data[pty.Alias] = new JArray(updatedValue.Split(','));
                                modified = true;
                            }
                            break;
                    }

                }
            }
        }
        return modified;
    }

    protected bool Update(IContent? content, BaseActionParameters parameters, Dictionary<string, string> extractedValues)
    {
        bool modified = false;
        DataTypeMapping mapping = CreateMapping(content.ContentType.Alias);

        if (extractedValues.ContainsKey("@Name"))
        {
            content.SetCultureName(extractedValues["@Name"], parameters.TargetLanguage);
        }

        foreach (var field in mapping.Fields)
        {
            if (content.HasProperty(field))
            {
                var pty = content.Properties.FirstOrDefault(p => p.Alias == field);

                var updatedValue = string.Empty;
                if (extractedValues.ContainsKey(pty.Alias))
                {
                    updatedValue = extractedValues[pty.Alias];
                }

                switch (pty.PropertyType.PropertyEditorAlias)
                {
                    case "Umbraco.BlockGrid":
                    case "Umbraco.BlockList":
                        {
                            var value = content.GetValue<string>(field, parameters.TargetLanguage).FromJson<JObject>();
                            if (value != null && value["contentData"] != null)
                            {
                                var contentData = value["contentData"].Children();
                                foreach (var data in contentData)
                                {
                                    UpdateBlock(parameters, data, extractedValues);
                                }
                            }
                            content.SetValue(field, value, parameters.TargetLanguage);

                        }
                        break;
                }

                if (!string.IsNullOrEmpty(updatedValue))
                {

                    switch (pty.PropertyType.PropertyEditorAlias)
                    {
                        case "Umbraco.TinyMCE":
                            {
                                var value = content.GetValue<string>(field, parameters.TargetLanguage).FromJson<dynamic>();
                                if (value != null)
                                {
                                    value.markup = updatedValue;
                                    content.SetValue(field, value, parameters.TargetLanguage);
                                    modified = true;
                                }
                            }
                            break;
                        case "Umbraco.TextBox":
                            {
                                var value = content.GetValue<string>(field, parameters.TargetLanguage ?? "fr");
                                value = updatedValue;
                                content.SetValue(field, value, parameters.TargetLanguage);
                                modified = true;
                            }
                            break;
                        case "Umbraco.Tags":
                            {
                                var value = content.GetValue<string[]>(field, parameters.TargetLanguage);
                                var jsonValue = JsonConvert.SerializeObject(updatedValue.Split(',').ToArray());
                                content.SetValue(field, jsonValue, parameters.TargetLanguage);
                                modified = true;
                            }
                            break;
                    }
                }
            }
        }
        return modified;
    }

    public void CreateContentVariant(IContent content, string fromCulture, string toCulture)
    {
        var contentItems = new List<IContent>();

        content.SetCultureName(content.Name, toCulture);
        foreach (var property in content.Properties)
        {
            var value = property
                .Values
                .FirstOrDefault(x => x.Culture != null && x.Culture.Equals(fromCulture, StringComparison.InvariantCultureIgnoreCase))?.PublishedValue;
            if (value != null)
            {
                try
                {
                    content.SetValue(property.Alias, value, toCulture);
                }
                catch { continue; }
            }
        }
    }
}