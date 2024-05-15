﻿using IriTools;
namespace Review;

public class ReviewDTO
{
    //Id of review
    private Guid? _reviewGuid;
    internal IriReference? _reviewIri;
    public Guid ReviewGuid
    {
        get => _reviewGuid ?? throw new InvalidOperationException("ReviewId not set");
        set
        {
            _reviewGuid = value;
            _reviewIri ??= new IriReference($"{Namespaces.Data.Review}{value.ToString()}");
        }
    }

    public IriReference ReviewIri
    {
        get => _reviewIri ?? throw new InvalidOperationException("ReviewIri not set");
        set
        {
            _reviewIri = value;
            _reviewGuid ??= Guid.NewGuid();
        }
    }

    //Revision IRI. 
    public Uri AboutRevision { get; set; }
    //Author of comment. In MEL this is known as comment responsible
    public string IssuedBy { get; set; }
    public DateOnly GeneratedAtTime { get; set; }
    //Number indicating status of review. Plaintext for now
    public ReviewStatus Status { get; set; }
    //public string ReviewStatus { get; set; }
    //Optional description of whole review
    public string Label { get; set; }
    //The comments in the review
    public List<CommentDto> HasComments { get; set; }

    public string GetReviewStatusDescription()
    {
        switch (Status)
        {
            case ReviewStatus.Code1:
                return "Code 1: Accepted";
            case ReviewStatus.Code2:
                return "Code 2: Minor Changes Needed";
            case ReviewStatus.Code3:
                return "Code 3: Major Changes Needed";
            case ReviewStatus.Code4:
                return "Code 4: Redesign Required";
            default:
                return "Status Unknown";
        }
    }
}

