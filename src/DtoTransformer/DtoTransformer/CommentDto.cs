﻿namespace Review;
public class CommentDto
{
    //Id of comment
    public Guid CommentId { get; set; }

    public Uri CommentUri
    {
        get { return new Uri(new Uri(Namespaces.Data.Comment), CommentId.ToString()); }
        set
        {
            if (!value.ToString().StartsWith(Namespaces.Data.Comment) || !Guid.TryParse(value.Segments.Last(), out Guid commentId))
                throw new Exception(
                    $"Invalid Uri {value.ToString()} used for comment. Comment URIs must start with {Namespaces.Data.Comment} followed by a Guid.");
            CommentId = commentId;
        }
    }

    //Actual commenttext
    public string CommentText { get; set; }
    //Author of comment
    public string IssuedBy { get; set; }
    public DateOnly GeneratedAtTime { get; set; }
    //What the Filter is commenting on. For mel this will be row IRIs.
    public List<Uri> AboutData { get; set; }
    //Filter that makes sense within a context. For Mel this could be Tagnumber and WeightHandlingCode
    public List<PropertyValuePair> AboutObject { get; set; }
}

public class PropertyValuePair()
{
    public required Uri Property { get; set; }
    public required string Value { get; set; }

}

