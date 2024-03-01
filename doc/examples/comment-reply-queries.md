# Example SPARQL queries 
These are queries over the data in [comment-reply.trig]()

## Query 1
This query demonstrates how it is possible to get all the comment replies from a review done on a specific revision. 

```sparql
prefix rec: <https://rdf.equinor.com/ontology/record/>
prefix rev: <https://rdf.equinor.com/ontology/revision/>
prefix review: <https://rdf.equinor.com/ontology/review/>
prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#>

SELECT ?RevisionName ?ReviewInfo ?CommentLabel ?ReplyText
WHERE {
  GRAPH ?revisionRecord {
    ?revisionId a rev:Revision ;
               rdfs:label ?RevisionName .
  }
  
  GRAPH ?reviewRecord {
    ?reviewRecord rec:describes ?reviewId ;
                  rdfs:comment  ?ReviewInfo .
    ?reviewId a review:Review ;
             review:aboutRevision ?revisionId ;
             review:hasComment ?commentId .  
    ?commentId rdfs:label ?CommentLabel .
  }
  
  GRAPH ?replyRecord {
    ?replyRecord rec:describes ?replyId .
    ?replyId a review:ReviewReply ;
             review:aboutReview ?reviewId ;
             review:replyHasComment ?reply .
    ?reply review:replyText ?ReplyText ;
           review:isReplyToComment ?commentId .
  }
}
```
### Output

| RevisionName                       | ReviewInfo                       | LabelId   | ReplyText |
|------------------------------------| ---------------------------------|-----------| ----------|
| Revision F01 of A123-BC-D-EF-00001 | This is a review of revision F01 | Test1     | OK        |
| Revision F01 of A123-BC-D-EF-00001 | This is a review of revision F01 | Test2     | Fixed     |
| Revision F01 of A123-BC-D-EF-00001 | This is a review of revision F01 | Test3     | Disagree  |


