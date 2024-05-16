public static class ReviewStatusHelper
{
    private static readonly Dictionary<ReviewStatus, string> StatusToIriMap = new()
    {
        { ReviewStatus.Code1, "https://rdf.equinor.com/ontology/review/Code1" },
        { ReviewStatus.Code2, "https://rdf.equinor.com/ontology/review/Code2" },
        { ReviewStatus.Code3, "https://rdf.equinor.com/ontology/review/Code3" },
        { ReviewStatus.Code4, "https://rdf.equinor.com/ontology/review/Code4" },
        { ReviewStatus.Code5, "https://rdf.equinor.com/ontology/review/Code5" },
    };

    private static readonly Dictionary<string, ReviewStatus> IriToStatusMap = StatusToIriMap.ToDictionary(pair => pair.Value, pair => pair.Key);

    public static string GetIri(ReviewStatus status)
    {
        if (StatusToIriMap.TryGetValue(status, out var iri))
        {
            return iri;
        }

        throw new ArgumentException("Invalid value");
    }

    public static ReviewStatus GetStatus(string iri)
    {
        if (IriToStatusMap.TryGetValue(iri, out var status))
        {
            return status;
        }

        throw new ArgumentException($"Invalid review status: {iri}");
    }
}