using Crolow.AzureServices.Models.Configuration;
using Crolow.OpenAi.Umbraco.Interfaces;
using Crolow.OpenAi.Umbraco.Models;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Web.BackOffice.Controllers;

namespace Crolow.OpenAi.Umbraco.Controllers;

public partial class CrolowAiApiController : UmbracoAuthorizedJsonController
{
    public readonly ICorrectApiService correctApiService;
    public readonly ITranslateApiService translateApiService;
    public readonly ISummarizeApiService summarizeApiService;
    public readonly ICreateHintsApiService createHintsApiService;
    public readonly IDescribeImageApiService describeImageApiService;
    protected readonly OpenAiConfiguration openAIConfig;

    public CrolowAiApiController(OpenAiConfiguration openAIConfig
        , ICorrectApiService correctApiService,
                        ITranslateApiService translateApiService,
                        ISummarizeApiService summarizeApiService,
                        ICreateHintsApiService createHintsApiService,
                        IDescribeImageApiService describeImageApiService)
    {
        this.openAIConfig = openAIConfig;
        this.correctApiService = correctApiService;
        this.translateApiService = translateApiService;
        this.summarizeApiService = summarizeApiService;
        this.createHintsApiService = createHintsApiService;
        this.describeImageApiService = describeImageApiService;
    }

    [HttpPost]
    public ServicesConfiguration GetConfig()
    {
        return openAIConfig.Services;
    }

    [HttpPost]
    public void Correct(CorrectActionParameters parameters)
    {
        parameters.TargetLanguage = parameters.SourceLanguage;
        correctApiService.Execute(parameters);
    }

    [HttpPost]
    public void Translate(TranslateActionParameters parameters)
    {
        translateApiService.Execute(parameters);
    }

    [HttpPost]
    public void Summarize(SummarizeActionParameters parameters)
    {
        summarizeApiService.Execute(parameters);
    }

    [HttpPost]
    public void CreateHints()
    {
        createHintsApiService.Execute();
    }

    [HttpPost]
    public void DescribeImage(DescribeImageActionParameters parameters)
    {
        describeImageApiService.Execute(parameters);
    }

}