using Crolow.AzureServices.Interfaces;
using Crolow.AzureServices.Models.Configuration;
using Crolow.AzureServices.Services;
using Crolow.OpenAi.Umbraco.Interfaces;
using Crolow.OpenAi.Umbraco.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenAI;
using System.ClientModel;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Crolow.OpenAi;

namespace Crolow.OpenAi.Umbraco.Composers
{
    public class CrolowComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.ManifestFilters().Append<OpenAiManifest>();

            var sectionOpenAI = builder.Config.GetSection("Crolow").GetSection("OpenAiConfiguration");
            var configOpenAI = sectionOpenAI.Get<OpenAiConfiguration>();
            if (configOpenAI != null)
            {
                builder.Services.AddSingleton(configOpenAI);

                var options = new OpenAIClientOptions()
                {
                };

                builder.Services.AddSingleton(_ =>
                new OpenAIClient(new ApiKeyCredential(configOpenAI.ApiKey), options));
            }
            else
            {
                builder.Services.AddSingleton<OpenAIClient>(_ => null);
            }

            builder.Services.AddSingleton<ICrolowOpenAiService, CrolowOpenAiService>();
            builder.Services.AddScoped<ICorrectApiService, CorrectApiService>();
            builder.Services.AddScoped<ITranslateApiService, TranslateApiService>();
            builder.Services.AddScoped<ISummarizeApiService, SummarizeApiService>();
            builder.Services.AddScoped<ICreateHintsApiService, CreateHintsApiService>();
            builder.Services.AddScoped<IDescribeImageApiService, DescribeImageApiService>();
        }
    }
}
