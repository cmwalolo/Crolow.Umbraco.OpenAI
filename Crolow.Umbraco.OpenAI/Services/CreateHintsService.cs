using Crolow.OpenAi.Umbraco.Interfaces;
using Newtonsoft.Json.Linq;
using StackExchange.Profiling.Internal;
using System.Text;
using Umbraco.Cms.Core.Services;

namespace Crolow.OpenAi.Umbraco.Services;

public class CreateHintsApiService : ICreateHintsApiService
{
    protected readonly IContentService contentService;
    protected readonly IContentTypeService contentTypeService;
    public CreateHintsApiService(IContentService contentService, IContentTypeService contentTypeService)
    {
        this.contentService = contentService;
        this.contentTypeService = contentTypeService;
    }

    public void Execute()
    {
        var contentType = contentTypeService.Get("mediaPage");
        long totalRecords = 0;
        var contents = contentService.GetPagedOfType(contentType.Id, 0, int.MaxValue,
            out totalRecords, null);

        foreach (var item in contents)
        {
            StringBuilder sb = new StringBuilder();

            var value = item.GetValue<string>("content", "fr").FromJson<JObject>();
            if (value != null && value["contentData"] != null)
            {
                var contentData = value["contentData"].Children();
                foreach (var data in contentData)
                {
                    ExtractBlock((JObject)data, sb);
                }
            }

            var result = new JObject();

            if (sb.Length > 0)
            {
                result["markup"] = sb.ToString();
                item.SetValue("description", result, "fr");
            }


            contentService.Save(item);
        }
    }

    protected void ExtractBlock(JObject data, StringBuilder sb)
    {
        var contentId = new Guid(data["contentTypeKey"]?.Value<string>() ?? Guid.Empty.ToString());
        var udi = data.Value<string>("udi");

        if (contentId != Guid.Empty)
        {
            var contentType = contentTypeService.Get(contentId);

            foreach (var pty in contentType.PropertyTypes)
            {
                switch (pty.PropertyEditorAlias)
                {
                    case "Umbraco.TinyMCE":
                        {
                            var ptyValue = data.Value<string>(pty.Alias);
                            if (!string.IsNullOrEmpty(ptyValue))
                            {
                                var value = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(ptyValue);
                                sb.AppendLine(value.Value<string>("markup"));
                            }
                        }
                        break;
                    case "Umbraco.TextBox":
                        {
                            var ptyValue = data.Value<string>(pty.Alias);
                            if (!string.IsNullOrEmpty(ptyValue))
                            {
                                sb.AppendLine(ptyValue);
                            }
                        }
                        break;
                    case "Umbraco.BlockList":
                        {
                            ExtractBlock((JObject)data[pty.Alias], sb);
                        }
                        break;
                }
            }
        }
    }


}