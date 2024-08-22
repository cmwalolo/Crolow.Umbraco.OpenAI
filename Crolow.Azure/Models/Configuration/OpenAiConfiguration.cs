namespace Crolow.AzureServices.Models.Configuration
{
    public class OpenAiConfiguration
    {
        public string AzureKey { get; set; }
        public string AzureEndPoint { get; set; }
        public string ApiKey { get; set; } = string.Empty;
        public string ApiUser { get; set; } = string.Empty;
        public ServicesConfiguration Services { get; set; }

    }

    public class ServicesConfiguration
    {

        public TranslationConfiguration Translations { get; set; }
        public CorrectionsConfiguration Corrections { get; set; }
        public SummarizeConfiguration Summaries { get; set; }
        public DescribeImageConfiguration DescribeImages { get; set; }
    }
    public class CorrectionsConfiguration
    {
        public Role[] Roles { get; set; }
        public DataTypeMapping[] DataTypeMappings { get; set; }
    }

    public class TranslationConfiguration
    {
        public Role[] Roles { get; set; }
        public DataTypeMapping[] DataTypeMappings { get; set; }
    }

    public class DescribeImageConfiguration
    {
        public Role[] Roles { get; set; }
        public DescribeImageSetting[] Settings { get; set; }
    }
    public class DescribeImageSetting
    {
        public Role[] Roles { get; set; }
        public string Template { get; set; }
        public string ImageField { get; set; }
        public string HintField { get; set; }
        public string OutputField { get; set; }
        public string OutputTagField { get; set; }
        public string OutputHashtagField { get; set; }
    }


    public class SummarizeConfiguration
    {
        public Role[] Roles { get; set; }
        public string[] DescriptionFields { get; set; }
        public string[] TitleFields { get; set; }
        public string[] TeaserFields { get; set; }
        public DataTypeMapping[] DataTypeMappings { get; set; }
    }

    public class Role
    {
        public string Name { get; set; }
        public string[] Prompts { get; set; }
    }

    public class DataTypeMapping
    {
        public string Alias { get; set; }
        public List<string> Fields { get; set; }
    }
}
