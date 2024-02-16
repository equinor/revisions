public class CommentDto
{
    //Id of comment
    public string CommentId { get; set; }
    //Actual commenttext
    public string CommentText { get; set; }
    //Author of comment
    public string IssuedBy { get; set; }
    public DateOnly GeneratedAtTime { get; set; }
    //What the Filter is commenting on. For mel this will be row numbers.
    public List<Uri> AboutData { get; set; }
    //Filter that makes sense within a context. For Mel this could be Tagnumber and WeightHandlingCode
    public List<(Uri property, string value)> AboutObject { get; set; }
}

