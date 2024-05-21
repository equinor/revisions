using IriTools;
using VDS.RDF;
using VDS.RDF.Parsing;
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
        // Load the Turtle file
        Graph g = new Graph();
        TurtleParser ttlparser = new TurtleParser();
        ttlparser.Load(g, "review.ttl"); // replace with your file path

        // Get the comment for the current status
        string statusUri = $"https://rdf.equinor.com/ontology/review/{Status}";
        INode statusNode = g.CreateUriNode(UriFactory.Create(statusUri));
        INode commentPredicate = g.CreateUriNode("rdfs:comment");
        Triple commentTriple = g.GetTriplesWithSubjectPredicate(statusNode, commentPredicate).FirstOrDefault();

        if (commentTriple != null)
        {
            return ((LiteralNode)commentTriple.Object).Value;
        }
        else
        {
            return "Status Unknown";
        }
    }
}

