using VDS.RDF;
using VDS.RDF.Query;
using Graph = VDS.RDF.Graph;


namespace Review;
public class DtoGenerator
{
    public static ReviewDTO GenerateDto(Graph graph)
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
            SELECT ?reviewId ?aboutRevision ?issuedBy ?generatedAtTime ?reviewStatus ?label
            WHERE {
                ?reviewId a review:Review ;
                            review:aboutRevision ?aboutRevision ;
                            review:issuedBy ?issuedBy ;
                            prov:generatedAtTime ?generatedAtTime ;
                            rdf:type ?reviewStatus ;
                            rdfs:label ?label .
                FILTER (?reviewStatus != review:Review)
            }";

        var reviewResults = (SparqlResultSet)graph.ExecuteQuery(reviewQuery);


        // Initialise ReviewDto
        var reviewDto = new ReviewDTO();
        if (reviewResults.Count > 0)
        {
            SparqlResult result = (SparqlResult)reviewResults[0];
            reviewDto.ReviewId = result["reviewId"].ToString();
            reviewDto.AboutRevision = new Uri(result["aboutRevision"].ToString());
            reviewDto.IssuedBy = ((LiteralNode)result["issuedBy"]).Value;
            reviewDto.GeneratedAtTime = DateOnly.Parse(((LiteralNode)result["generatedAtTime"]).Value);
            reviewDto.ReviewStatus = result["reviewStatus"].ToString();
            reviewDto.Label = ((LiteralNode)result["label"]).Value;
            reviewDto.HasComments = new List<CommentDto>();
        }

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
            commentDto.CommentId = result["commentId"].ToString();
            commentDto.CommentText = ((LiteralNode)result["commentText"]).Value;
            commentDto.IssuedBy = ((LiteralNode)result["issuedBy"]).Value;
            commentDto.GeneratedAtTime = DateOnly.Parse(((LiteralNode)result["generatedAtTime"]).Value);

            commentDto.AboutData = new List<Uri>();
            foreach (var data in aboutDataResults)
            {
                if (data["commentId"].ToString() == commentDto.CommentId)
                {
                    commentDto.AboutData.Add(new Uri(data["data"].ToString()));
                }
            }

            commentDto.AboutObject = new List<(Uri property, string value)>();
            foreach (var data in aboutObjectResults)
            {
                if (data["commentId"].ToString() == commentDto.CommentId)
                {
                    commentDto.AboutObject.Add((new Uri(data["property"].ToString()), ((LiteralNode)data["value"]).Value));
                }
            }

            reviewDto.HasComments.Add(commentDto);
        }

        return reviewDto;

    }


}

