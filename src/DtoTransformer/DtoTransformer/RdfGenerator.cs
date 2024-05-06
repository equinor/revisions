
using VDS.RDF;


namespace Review;
public class RdfGenerator
{
    public static Graph GenerateRdf(ReviewDTO reviewDto)
    {
        var graph = new Graph();
        //Add nAmespaces
        graph.NamespaceMap.AddNamespace("rdf", new Uri(Namespaces.Rdf.BaseUrl));
        graph.NamespaceMap.AddNamespace("rdfs", new Uri(Namespaces.Rdfs.BaseUrl));
        graph.NamespaceMap.AddNamespace("xsd", new Uri(Namespaces.Xsd.BaseUrl));
        graph.NamespaceMap.AddNamespace("rev", new Uri(Namespaces.Revision.BaseUrl));
        graph.NamespaceMap.AddNamespace("review", new Uri(Namespaces.Review.BaseUrl));
        graph.NamespaceMap.AddNamespace("prov", new Uri(Namespaces.Prov.BaseUrl));
        graph.NamespaceMap.AddNamespace("comment", new Uri(Namespaces.Data.Comment));

        // Create and assert Review triples
        var reviewId = graph.CreateUriNode(reviewDto.ReviewIri);
        var aboutRevision = graph.CreateUriNode(reviewDto.AboutRevision);
        var issuedBy = graph.CreateLiteralNode(reviewDto.IssuedBy);
        var generatedAtTime = graph.CreateLiteralNode(formatDate(reviewDto.GeneratedAtTime), UriFactory.Create("http://www.w3.org/2001/XMLSchema#date"));
        var reviewStatus = graph.CreateUriNode(new Uri(reviewDto.ReviewStatus));
        var label = graph.CreateLiteralNode(reviewDto.Label);
        var guidvalue = graph.CreateLiteralNode(reviewDto.ReviewGuid.ToString());


        graph.Assert(new Triple(reviewId, graph.CreateUriNode("rdf:type"), graph.CreateUriNode("review:Review")));
        graph.Assert(new Triple(reviewId, graph.CreateUriNode("rdf:type"), reviewStatus));
        graph.Assert(new Triple(reviewId, graph.CreateUriNode("rdfs:label"), label));
        graph.Assert(new Triple(reviewId, graph.CreateUriNode("prov:generatedAtTime"), generatedAtTime));
        graph.Assert(new Triple(reviewId, graph.CreateUriNode("review:aboutRevision"), aboutRevision));
        graph.Assert(new Triple(reviewId, graph.CreateUriNode("review:issuedBy"), issuedBy));
        graph.Assert(new Triple(reviewId, graph.CreateUriNode(new Uri(Namespaces.Review.HasGuid)), guidvalue));


        //Create and assert comment triples
        foreach (var commentDto in reviewDto.HasComments)
        {
            var commentId = graph.CreateUriNode(commentDto.CommentUri);
            var commentText = graph.CreateLiteralNode(commentDto.CommentText);
            var commentIssuedBy = graph.CreateLiteralNode(commentDto.IssuedBy);
            var commentGeneratedAtTime = graph.CreateLiteralNode(formatDate(commentDto.GeneratedAtTime), UriFactory.Create(Namespaces.Xsd.Date));
            var reviewAboutData = graph.CreateUriNode(new Uri(Namespaces.Review.AboutData));
            var aboutObject = graph.CreateBlankNode();
            graph.Assert(new Triple(commentId, graph.CreateUriNode(new Uri(Namespaces.Review.AboutObject)), aboutObject));
            graph.Assert(new Triple(aboutObject, graph.CreateUriNode(new Uri(Namespaces.Rdf.Type)), graph.CreateUriNode(new Uri(Namespaces.Review.FilterObject))));
            foreach (var pair in commentDto.AboutObject)
            {
                var propertyNode = graph.CreateUriNode(pair.property);
                var valueNode = graph.CreateLiteralNode(pair.value);

                graph.Assert(new Triple(aboutObject, propertyNode, valueNode));
            }

            graph.Assert(new Triple(commentId, graph.CreateUriNode(new Uri(Namespaces.Rdf.Type)), graph.CreateUriNode(new Uri(Namespaces.Review.Comment))));
            graph.Assert(new Triple(commentId, graph.CreateUriNode(new Uri(Namespaces.Rdfs.Label)), commentText));
            graph.Assert(new Triple(commentId, graph.CreateUriNode(new Uri(Namespaces.Prov.GeneratedAtTime)), commentGeneratedAtTime));
            graph.Assert(new Triple(commentId, graph.CreateUriNode(new Uri(Namespaces.Review.IssuedBy)), commentIssuedBy));

            var nodes = new List<INode>();
            foreach (var uri in commentDto.AboutData)
            {
                var dataNode = graph.CreateUriNode(uri);
                nodes.Add(dataNode);
            }
            var listRoot = graph.AssertList(nodes);

            graph.Assert(new Triple(commentId, reviewAboutData, listRoot));

            graph.Assert(new Triple(reviewId, graph.CreateUriNode(new Uri(Namespaces.Review.HasComment)), commentId));
        }

        AddBlankNodeToReview(graph, reviewId);

        return graph;
    }

    private static string formatDate(DateOnly date)
    // The formatDate method is used to format the DateOnly object to a string representation in the format "yyyy-MM-dd".
    // This formatting is necessary because the generatedAtTime property in the RDF graph requires a specific format defined by the XML Schema Definition (XSD) standard.
    // By formatting the date in this way, we ensure that it conforms to the XSD date format and can be properly represented in the RDF graph.
    {
        return date.ToString("yyyy-MM-dd");
    }
    public static void AddBlankNodeToReview(Graph graph, IRefNode rdfSubject)
    {
        IRefNode activity = graph.CreateBlankNode();
        AddProvenance(graph, activity);

        var wasGeneratedByPredicate = new Triple(
            rdfSubject,
            new UriNode(new Uri(Namespaces.Prov.WasGeneratedBy)),
            activity);

        graph.Assert(wasGeneratedByPredicate);
    }

    private static void AddProvenance(Graph graph, IRefNode activity)
    {
        var versionUri = new UriNode(new Uri(CreateReviewVersionUri()));
        graph.Assert(new Triple(
            activity,
            new UriNode(new Uri(Namespaces.Prov.WasAssociatedWith)),
            versionUri));

        graph.Assert(new Triple(
            activity,
            new UriNode(new Uri(Namespaces.Rdfs.Comment)),
            new LiteralNode("Version of ReviewDto used to create the Rdf")));

        graph.Assert(new Triple(
            activity,
            new UriNode(new Uri(Namespaces.Rdfs.Label)),
            new LiteralNode(GetReviewVersion())));
    }
    private static string CreateReviewVersionUri() =>
        $"https://www.nuget.org/packages/Review/{GetReviewVersion()}";
    private static string GetReviewVersion()
    {
        return typeof(RdfGenerator).Assembly.GetName().Version?.ToString() ?? throw new Exception("Could not get version of Review");
    }
}

