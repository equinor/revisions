
using VDS.RDF;
using VDS.RDF.Writing;

internal class RdfGenerator
{
    public static string GenerateRdf(ReviewDTO reviewDto) {
        // Create a Graph object
        var graph = new Graph();

        graph.NamespaceMap.AddNamespace("rdf", new Uri("http://www.w3.org/1999/02/22-rdf-syntax-ns#"));
        graph.NamespaceMap.AddNamespace("rdfs", new Uri("http://www.w3.org/2000/01/rdf-schema#"));
        graph.NamespaceMap.AddNamespace("xsd", new Uri("http://www.w3.org/2001/XMLSchema#"));
        graph.NamespaceMap.AddNamespace("exdata", new Uri("https://example.com/data/"));
        graph.NamespaceMap.AddNamespace("exdoc", new Uri("https://example.com/doc/"));
        graph.NamespaceMap.AddNamespace("rec", new Uri("https://rdf.equinor.com/ontology/record/"));
        graph.NamespaceMap.AddNamespace("rev", new Uri("https://rdf.equinor.com/ontology/revision/"));
        graph.NamespaceMap.AddNamespace("review", new Uri("https://rdf.equinor.com/ontology/review/"));
        graph.NamespaceMap.AddNamespace("prov", new Uri("http://www.w3.org/ns/prov#"));
        graph.NamespaceMap.AddNamespace("rdl", new Uri("http://example.com/rdl/"));
        graph.NamespaceMap.AddNamespace("mel", new Uri("https://rdf.equinor.com/ontology/mel/v1#"));

        var reviewId = graph.CreateUriNode(new Uri(reviewDto.ReviewId));
        var aboutVersion = graph.CreateUriNode(new Uri(reviewDto.AboutVersion));
        var issuedBy = graph.CreateLiteralNode(reviewDto.IssuedBy);
        var generatedAtTime = graph.CreateLiteralNode(reviewDto.GeneratedAtTime.ToString(), UriFactory.Create("http://www.w3.org/2001/XMLSchema#date"));
        var reviewStatus = graph.CreateUriNode(new Uri(reviewDto.ReviewStatus));
        var label = graph.CreateLiteralNode(reviewDto.Label);

        // Assert Reviews triples
        graph.Assert(new Triple(reviewId, graph.CreateUriNode("rdf:type"), graph.CreateUriNode("review:Review")));
        graph.Assert(new Triple(reviewId, graph.CreateUriNode("rdf:typeFIX"), reviewStatus));
        graph.Assert(new Triple(reviewId, graph.CreateUriNode("rdfs:label"), label));
        graph.Assert(new Triple(reviewId, graph.CreateUriNode("prov:generatedAtTime"), generatedAtTime));
        graph.Assert(new Triple(reviewId, graph.CreateUriNode("review:aboutVersion"), aboutVersion));
        graph.Assert(new Triple(reviewId, graph.CreateUriNode("review:issuedBy"), issuedBy));

        foreach (var commentDto in reviewDto.HasComments)
        {
            var commentId = graph.CreateUriNode(new Uri(commentDto.CommentId));
            var commentText = graph.CreateLiteralNode(commentDto.CommentText);
            var commentIssuedBy = graph.CreateLiteralNode(commentDto.IssuedBy);
            var commentGeneratedAtTime = graph.CreateLiteralNode(commentDto.GeneratedAtTime.ToString(), UriFactory.Create("http://www.w3.org/2001/XMLSchema#date"));
            var reviewAboutData = graph.CreateUriNode("review:aboutData");
            var aboutObject = graph.CreateBlankNode();
            graph.Assert(new Triple(commentId, graph.CreateUriNode("review:aboutObject"), aboutObject));
            graph.Assert(new Triple(aboutObject, graph.CreateUriNode("rdf:type"), graph.CreateUriNode("review:FilterObject")));
            foreach (var pair in commentDto.AboutObject)
            {
                var propertyNode = graph.CreateUriNode(pair.property);
                var valueNode = graph.CreateLiteralNode(pair.value);

                graph.Assert(new Triple(aboutObject, propertyNode, valueNode));
            }

            // Assert Comments triples
            graph.Assert(new Triple(commentId, graph.CreateUriNode("rdf:type"), graph.CreateUriNode("review:Comment")));
            graph.Assert(new Triple(commentId, graph.CreateUriNode("rdfs:label"), commentText));
            graph.Assert(new Triple(commentId, graph.CreateUriNode("prov:generatedAtTime"), commentGeneratedAtTime));
            graph.Assert(new Triple(commentId, graph.CreateUriNode("review:issuedBy"), commentIssuedBy));


            List<INode> nodes = new List<INode>();
            foreach (var uri in commentDto.AboutData)
            {
                var dataNode = graph.CreateUriNode(uri);
                nodes.Add(dataNode);
            }
            INode listRoot = graph.AssertList(nodes);

            graph.Assert(new Triple(commentId, reviewAboutData, listRoot));

            graph.Assert(new Triple(reviewId, graph.CreateUriNode("review:hasComment"), commentId));
        }

        var turtle = VDS.RDF.Writing.StringWriter.Write(graph, new CompressingTurtleWriter());


        return turtle;

    }
}

