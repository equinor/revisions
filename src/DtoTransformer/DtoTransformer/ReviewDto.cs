﻿namespace Review;
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
}

