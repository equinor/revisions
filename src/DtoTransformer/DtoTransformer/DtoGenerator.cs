using VDS.RDF;
using VDS.RDF.Query;
using VDS.RDF.Query.Paths;
using Graph = VDS.RDF.Graph;

public class DtoGenerator
{
    public static ReviewDTO GenerateDto(Graph graph)
    {
        // Run first sparql query retrieving Review data
        string query = @"
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
    SELECT ?reviewId ?aboutVersion ?issuedBy ?generatedAtTime ?reviewStatus ?label
    WHERE {
        ?reviewId a review:Review ;
                  review:aboutVersion ?aboutVersion ;
                  review:issuedBy ?issuedBy ;
                  prov:generatedAtTime ?generatedAtTime ;
                  rdf:type ?reviewStatus ;
                  rdfs:label ?label .
        FILTER (?reviewStatus != review:Review)
    }";

        ReviewDTO reviewDto = new ReviewDTO();

        SparqlResultSet results = (SparqlResultSet)graph.ExecuteQuery(query);
        if (results.Count > 0)
        {
            SparqlResult result = (SparqlResult)results[0];
            reviewDto.ReviewId = result["reviewId"].ToString();
            reviewDto.AboutVersion = result["aboutVersion"].ToString();
            reviewDto.IssuedBy = result["issuedBy"].ToString().Split("^^")[0];
            reviewDto.GeneratedAtTime = DateOnly.Parse(result["generatedAtTime"].ToString().Split("^^")[0]);
            reviewDto.ReviewStatus = result["reviewStatus"].ToString().Split("^^")[0];
            reviewDto.Label = result["label"].ToString().Split("^^")[0];
            reviewDto.HasComments = new List<CommentDto>();
        }

        // Run second sparql query retriving comments data
        query = @"
    PREFIX review: <https://rdf.equinor.com/ontology/review/>
    PREFIX prov: <http://www.w3.org/ns/prov#>
    PREFIX mel: <https://rdf.equinor.com/ontology/mel/v1#>
    PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
    prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#>
    SELECT ?commentId ?commentText ?issuedBy ?generatedAtTime (GROUP_CONCAT(DISTINCT ?data; separator=',') AS ?aboutData) (GROUP_CONCAT(DISTINCT ?property; separator=',') AS ?filterProperty) (GROUP_CONCAT(DISTINCT ?value; separator=',') AS ?filterValue)
    WHERE {
        ?commentId a review:Comment ;
                   rdfs:label ?commentText ;
                   review:issuedBy ?issuedBy ;
                   prov:generatedAtTime ?generatedAtTime ;
                   review:aboutData/(rdf:rest*/rdf:first) ?data ;
                   review:aboutObject ?object .
               
        # Use a ?variable to get the property and value of ?object
        ?object ?property ?value .
    }
    GROUP BY ?commentId ?commentText ?issuedBy ?generatedAtTime";


        results = (SparqlResultSet)graph.ExecuteQuery(query);
        foreach (SparqlResult result in results)
        {
            CommentDto commentDto = new CommentDto();
            commentDto.CommentId = result["commentId"].ToString();
            commentDto.CommentText = result["commentText"].ToString().Split("^^")[0];
            commentDto.IssuedBy = result["issuedBy"].ToString().Split("^^")[0];
            commentDto.GeneratedAtTime = DateOnly.Parse(result["generatedAtTime"].ToString().Split("^^")[0]);
            commentDto.AboutData = new List<Uri>();
            foreach (string data in result["aboutData"].ToString().Split(","))
            {
                if (!(data.StartsWith("http://") | data.StartsWith("^^")))
                {
                    commentDto.AboutData.Add(new Uri(data.Split("^^")[0]));
                }
            }
            commentDto.AboutObject = new List<(Uri property, string value)>();
            string[] properties = result["filterProperty"].ToString().Split(",");
            string[] values = result["filterValue"].ToString().Split(",");
            for (int i = 0; i < properties.Length; i++)
            {
                if (!(properties[i].StartsWith("http://")|properties[i].StartsWith("^^")))
                {
                    commentDto.AboutObject.Add((new Uri(properties[i].Split("^^")[0]), values[i].Split("^^")[0]));
                }
            }

            reviewDto.HasComments.Add(commentDto);
        }

        return reviewDto;

    }


}