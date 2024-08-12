namespace DtoTransformer;

public enum TR
{
    None,
    MelReportingTemplate
}

public static class TRExtensions
{
    public const string MelReportingTemplateUri = "https://rdf.equinor.com/ontology/technical-requirement/v1#MelReportingTemplate";

    public static Uri ToUri(this TR type) =>
        type switch
        {
            TR.MelReportingTemplate => new Uri(MelReportingTemplateUri),
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };

    public static TR StringUriToTR(string stringUri) =>
        stringUri switch
        {
            MelReportingTemplateUri => TR.MelReportingTemplate,
            _ => throw new ArgumentOutOfRangeException(stringUri)
        };
}