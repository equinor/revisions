namespace Review;
public struct Namespaces
{
    public struct Prov
    {
        public const string BaseUrl = "http://www.w3.org/ns/prov#";

        public const string WasDerivedFrom = $"{BaseUrl}wasDerivedFrom";
        public const string AtLocation = $"{BaseUrl}atLocation";
        public const string GeneratedAtTime = $"{BaseUrl}generatedAtTime";
        public const string HadMember = $"{BaseUrl}hadMember";
        public const string WasGeneratedBy = $"{BaseUrl}wasGeneratedBy";
        public const string WasAssociatedWith = $"{BaseUrl}wasAssociatedWith";
    }

    public struct Rdf
    {
        public const string BaseUrl = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";

        public const string Nil = $"{BaseUrl}nil";
        public const string Type = $"{BaseUrl}type";
    }

    public struct Rdfs
    {
        public const string BaseUrl = "http://www.w3.org/2000/01/rdf-schema#";

        public const string Comment = $"{BaseUrl}comment";
        public const string Label = $"{BaseUrl}label";
    }

    public struct Xsd
    {
        public const string BaseUrl = "http://www.w3.org/2001/XMLSchema#";

        public const string DateTime = $"{BaseUrl}dateTime";
        public const string Date = $"{BaseUrl}date";
    }

    public struct Revision
    {
        public const string BaseUrl = "https://rdf.equinor.com/ontology/revision/";

        public const string RevisionClass = $"{BaseUrl}Revision";
        public const string DocumentRevision = $"{BaseUrl}DocumentRevision";
        public const string containsRecord = $"{BaseUrl}containsRecord";
        public const string describes = $"{BaseUrl}describes";
        public const string isNewRevisionOf = $"{BaseUrl}isNewRevisionOf";
        public const string revisionNumber = $"{BaseUrl}revisionNumber";
        public const string hasAttachment = $"{BaseUrl}hasAttachment";
        public const string revisionSequenceNumber = $"{BaseUrl}hasSequenceNumber";
    }

    public struct Review
    {
        public const string BaseUrl = "https://rdf.equinor.com/ontology/review/";

        public const string ReviewClass = $"{BaseUrl}Review";
        public const string Comment = $"{BaseUrl}Comment";
        public const string AboutData = $"{BaseUrl}aboutData";
        public const string AboutObject = $"{BaseUrl}aboutObject";
        public const string IssuedBy = $"{BaseUrl}issuedBy";
        public const string FilterObject = $"{BaseUrl}FilterObject";
        public const string HasComment = $"{BaseUrl}hasComment";
        public const string HasGuid = $"{BaseUrl}hasGuid";
        public const string ReviewOfClass = $"{BaseUrl}reviewOfClass";
    }
    public struct Data
    {
        public const string Comment = "https://rdf.equinor.com/data/review/comment/";

        public const string Review = "https://rdf.equinor.com/data/review/";

    }
}