namespace DtoTransformer;

public enum TR
{
    None,
    MelReportingTemplate
}

public static class TRExtensions
{
    public const string MelReportingTemplateUri = "https://rdf.equinor.com/ontology/technical-requirement/v1#MelReportingTemplate";

    public static string ToUri(this TR type) =>
        type switch
        {
            TR.MelReportingTemplate => MelReportingTemplateUri,
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
}