namespace Crolow.OpenAi.Umbraco.Models;

public class BaseActionParameters
{
    public string NodeId { get; set; }
    public string SourceLanguage { get; set; }
    public string TargetLanguage { get; set; }
    public bool Recursive { get; set; }
    public string Role { get; set; }
}

public class CorrectActionParameters : BaseActionParameters
{
}

public class TranslateActionParameters : BaseActionParameters
{
    public bool SkipIfExists { get; set; }
}

public class SummarizeActionParameters : BaseActionParameters
{
}
public class DescribeImageActionParameters : BaseActionParameters
{
}
