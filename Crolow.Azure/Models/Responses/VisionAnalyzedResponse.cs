namespace Crolow.AzureServices.Models.Responses
{
    public class VisionAnalyzedResponse
    {
        public string Language { get; set; }

        public List<AnalyzedSection> Tags;
        public string LongCaption { get; set; }
        public List<AnalyzedSection> ShortCaptions { get; set; }

        public class AnalyzedSection
        {
            public string Value { get; set; }
            public double Confidence { get; set; }
        }

    }
}
