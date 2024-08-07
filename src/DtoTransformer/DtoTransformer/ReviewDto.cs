﻿using System.Reflection;
using DtoTransformer;
using IriTools;
using VDS.RDF;
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
    //The Technical requirement of the document that the review is about
    public TR TechnicalRequirement { get; set; }
    //Number indicating status of review. Plaintext for now
    public ReviewStatus Status { get; set; }
    //public string ReviewStatus { get; set; }
    //Optional description of whole review
    public string Label { get; set; }
    //The comments in the review
    public List<CommentDto> HasComments { get; set; }

    private static Graph GetReviewGraph()
    {
        var reviewGraph = new Graph();
        var outputFolderPath = Assembly.GetExecutingAssembly()
                                   .GetManifestResourceStream("DtoTransformer.review.ttl") ??
                               throw new Exception("Could not get assembly path of review.ttl.");
        var shapeString = new StreamReader(outputFolderPath).ReadToEnd();
        reviewGraph.LoadFromString(shapeString);
        return reviewGraph;
    }

    public string GetReviewStatusDescription()
    {
        //Get reviewstatus comment text from ontology
        Graph g = GetReviewGraph();

        string statusUri = $"https://rdf.equinor.com/ontology/review/{Status}";
        INode statusNode = g.CreateUriNode(UriFactory.Create(statusUri));
        INode commentPredicate = g.CreateUriNode(UriFactory.Create("http://www.w3.org/2000/01/rdf-schema#comment"));
        Triple commentTriple = g.GetTriplesWithSubjectPredicate(statusNode, commentPredicate).FirstOrDefault();

        // Return the value of the comment triple's object if it exists, otherwise return "Status Unknown"
        return commentTriple != null ? ((LiteralNode)commentTriple.Object).Value : "Status Unknown";
    }
}

