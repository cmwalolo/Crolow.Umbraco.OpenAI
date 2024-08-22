using Crolow.OpenAi.Umbraco.Models;

namespace Crolow.OpenAi.Umbraco.Interfaces
{
    public interface ICorrectApiService
    {
        void Execute(CorrectActionParameters parameters);
    }

    public interface ITranslateApiService
    {
        void Execute(TranslateActionParameters parameters);
    }

    public interface ISummarizeApiService
    {
        void Execute(SummarizeActionParameters parameters);
    }

    public interface IDescribeImageApiService
    {
        void Execute(DescribeImageActionParameters parameters);
    }

    public interface ICreateHintsApiService
    {
        void Execute();
    }
}