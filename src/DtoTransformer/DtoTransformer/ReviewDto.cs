namespace Review;
public class ReviewDTO
{
    //Id of review
    public string ReviewId { get; set; }
    //Revision IRI. 
    public Uri AboutRevision { get; set; }
    //Author of comment. In MEL this is known as comment responsible
    public string IssuedBy { get; set; }
    public DateOnly GeneratedAtTime { get; set; }
    //Number indicating status of review. Plaintext for now
    public string ReviewStatus { get; set; }
    //Optional description of whole review
    public string Label { get; set; }
    //The comments in the review
    public List<CommentDto> HasComments { get; set; }

    public string GetReviewStatusDescription()
    {
        switch (ReviewStatus)
        {
            case "https://rdf.equinor.com/ontology/review/Code1":
                return "Code 1: Accepted";
            case "https://rdf.equinor.com/ontology/review/Code2":
                return "Code 2: Minor Changes Needed";
            case "https://rdf.equinor.com/ontology/review/Code3":
                return "Code 3: Major Changes Needed";
            case "https://rdf.equinor.com/ontology/review/Code4":
                return "Code 4: Redesign Required";
            default:
                return "Status Unknown";
        }
    }
}

