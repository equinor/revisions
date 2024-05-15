using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Query;
using Graph = VDS.RDF.Graph;


namespace Review;
public class DtoGenerator
{
    public static ReviewDTO GenerateDto(IGraph graph)
    {
        var reviewQuery = @"
                PREFIX review: <https://rdf.equinor.com/ontology/review/>
                PREFIX prov: <http://www.w3.org/ns/prov#>
                PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
                prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#>
                prefix xsd: <http://www.w3.org/2001/XMLSchema#>
                prefix exdata: <https://example.com/data/>
                prefix exdoc: <https://example.com/doc/>
                prefix rec: <https://rdf.equinor.com/ontology/record/>
                prefix rev: <https://rdf.equinor.com/ontology/revision/>
                prefix rdl: <http://example.com/rdl/>
                prefix mel: <https://rdf.equinor.com/ontology/mel/v1#>
                SELECT DISTINCT ?reviewId ?aboutRevision ?issuedBy ?generatedAtTime ?reviewStatus ?label ?guid
                WHERE {
                    ?reviewId a review:Review ;
                                review:aboutRevision ?aboutRevision ;
                                review:issuedBy ?issuedBy ;
                                prov:generatedAtTime ?generatedAtTime ;
                                rdf:type ?reviewStatus ;
                                rdfs:label ?label .
                    OPTIONAL{?reviewId review:hasGuid ?guid} .
                    FILTER (?reviewStatus != review:Review)
                }";

        var reviewResults = (SparqlResultSet)graph.ExecuteQuery(reviewQuery);


        // Initialise ReviewDto
        var reviewDto = new ReviewDTO();
        if (reviewResults.Count != 1)
        {
            throw new InvalidOperationException($"The IGraph contained {reviewResults.Count} reviews. There should be exactly one.");
        }
        SparqlResult reviewResult = (SparqlResult)reviewResults[0];
        reviewDto._reviewIri = reviewResult["reviewId"].ToString();
        if (reviewResult.HasValue("guid"))
            reviewDto.ReviewGuid = Guid.Parse(((LiteralNode)reviewResult["guid"]).Value);
        reviewDto.AboutRevision = new Uri(reviewResult["aboutRevision"].ToString());
        reviewDto.IssuedBy = ((LiteralNode)reviewResult["issuedBy"]).Value;
        reviewDto.GeneratedAtTime = DateOnly.Parse(((LiteralNode)reviewResult["generatedAtTime"]).Value);
        reviewDto.Status = ParseReviewStatus(((LiteralNode) reviewResult["reviewStatus"]).Value);
        reviewDto.Label = ((LiteralNode)reviewResult["label"]).Value;
        reviewDto.HasComments = new List<CommentDto>();


        var commentQuery = @"
                PREFIX review: <https://rdf.equinor.com/ontology/review/>
                PREFIX prov: <http://www.w3.org/ns/prov#>
                PREFIX mel: <https://rdf.equinor.com/ontology/mel/v1#>
                PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
                prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#>
                SELECT ?commentId ?commentText ?issuedBy ?generatedAtTime
                WHERE {
                    ?commentId a review:Comment ;
                                rdfs:label ?commentText ;
                                review:issuedBy ?issuedBy ;
                                prov:generatedAtTime ?generatedAtTime .
                }
                GROUP BY ?commentId ?commentText ?issuedBy ?generatedAtTime
                ORDER BY ?commentId";

        var commentResults = (SparqlResultSet)graph.ExecuteQuery(commentQuery);

        string aboutDataQuery = @"
                PREFIX review: <https://rdf.equinor.com/ontology/review/>
                PREFIX prov: <http://www.w3.org/ns/prov#>
                PREFIX mel: <https://rdf.equinor.com/ontology/mel/v1#>
                PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
                prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#>
                SELECT ?commentId ?data
                WHERE {
                    ?commentId a review:Comment ;
                                review:aboutData/(rdf:rest*/rdf:first) ?data .             
                }
                GROUP BY ?commentId ?data
                ORDER BY ?commentId";

        var aboutDataResults = (SparqlResultSet)graph.ExecuteQuery(aboutDataQuery);

        var aboutObjectQuery = @"
                PREFIX review: <https://rdf.equinor.com/ontology/review/>
                PREFIX prov: <http://www.w3.org/ns/prov#>
                PREFIX mel: <https://rdf.equinor.com/ontology/mel/v1#>
                PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
                prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#>
                SELECT ?commentId ?property ?value
                WHERE {
                    ?commentId a review:Comment ;
                                review:aboutObject ?object .
                ?object ?property ?value .  
                FILTER (?value != review:FilterObject)
                }
                GROUP BY ?commentId ?property ?value
                ORDER BY ?commentId";

        var aboutObjectResults = (SparqlResultSet)graph.ExecuteQuery(aboutObjectQuery);


        //Initalise CommentDTOs
        foreach (SparqlResult result in commentResults)
        {
            var commentDto = new CommentDto();
            Uri commentUri = ((UriNode)result["commentId"]).Uri;
            commentDto.CommentUri = commentUri;
            commentDto.CommentText = ((LiteralNode)result["commentText"]).Value;
            commentDto.IssuedBy = ((LiteralNode)result["issuedBy"]).Value;
            commentDto.GeneratedAtTime = DateOnly.Parse(((LiteralNode)result["generatedAtTime"]).Value);

            commentDto.AboutData = new List<Uri>();
            foreach (var data in aboutDataResults)
            {
                if (data["commentId"].ToString() == commentUri.ToString())
                {
                    commentDto.AboutData.Add(new Uri(data["data"].ToString()));
                }
            }

            commentDto.AboutObject = new List<(Uri property, string value)>();
            foreach (var data in aboutObjectResults)
            {
                if (data["commentId"].ToString() == commentUri.ToString())
                {
                    commentDto.AboutObject.Add((new Uri(data["property"].ToString()), ((LiteralNode)data["value"]).Value));
                }
            }

            reviewDto.HasComments.Add(commentDto);
        }

        return reviewDto;
    }

    private static ReviewStatus ParseReviewStatus(string status)
    {
        return status switch
        {
            "Code1" => ReviewStatus.Code1,
            "Code2" => ReviewStatus.Code2,
            "Code3" => ReviewStatus.Code3,
            "Code4" => ReviewStatus.Code4,
            "Code5" => ReviewStatus.Code5,
            _ => throw new ArgumentException($"Invalid review status: {status}")
        };
    }
}

