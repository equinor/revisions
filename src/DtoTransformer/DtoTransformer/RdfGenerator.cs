
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
        graph.NamespaceMap.AddNamespace("comment", new Uri(Namespaces.CommentData.BaseUrl));

        // Create and assert Review triples
        var reviewId = graph.CreateUriNode(new Uri(reviewDto.ReviewId));
        var aboutRevision = graph.CreateUriNode(reviewDto.AboutRevision);
        var issuedBy = graph.CreateLiteralNode(reviewDto.IssuedBy);
        var generatedAtTime = graph.CreateLiteralNode(reviewDto.GeneratedAtTime.ToString(), UriFactory.Create("http://www.w3.org/2001/XMLSchema#date"));
        var reviewStatus = graph.CreateUriNode(new Uri(reviewDto.ReviewStatus));
        var label = graph.CreateLiteralNode(reviewDto.Label);

        
        graph.Assert(new Triple(reviewId, graph.CreateUriNode("rdf:type"), graph.CreateUriNode("review:Review")));
        graph.Assert(new Triple(reviewId, graph.CreateUriNode("rdf:type"), reviewStatus));
        graph.Assert(new Triple(reviewId, graph.CreateUriNode("rdfs:label"), label));
        graph.Assert(new Triple(reviewId, graph.CreateUriNode("prov:generatedAtTime"), generatedAtTime));
        graph.Assert(new Triple(reviewId, graph.CreateUriNode("review:aboutRevision"), aboutRevision));
        graph.Assert(new Triple(reviewId, graph.CreateUriNode("review:issuedBy"), issuedBy));

        //Create and assert comment triples
        foreach (var commentDto in reviewDto.HasComments)
        {
            var commentId = graph.CreateUriNode(commentDto.CommentUri);
            var commentText = graph.CreateLiteralNode(commentDto.CommentText);
            var commentIssuedBy = graph.CreateLiteralNode(commentDto.IssuedBy);
            var commentGeneratedAtTime = graph.CreateLiteralNode(commentDto.GeneratedAtTime.ToString(), UriFactory.Create(Namespaces.Xsd.Date));
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


        return graph;

    }
}

