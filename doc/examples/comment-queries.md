# Example SPARQL queries
These are queries over the data in [comments.trig]() and [revisions.trig]()

The first two queries assume that all relevant data is put into the default un-named named graph, for example some kind of "head" graph of all not replaced records. 
They also assume that [../schema/comment.ttl]() is loaded into this un-named graph

## Query 1
Get status of the newest revision.
```sparql
prefix rec: <https://rdf.equinor.com/ontology/record/>
prefix rev: <https://rdf.equinor.com/ontology/revision/>

SELECT ?r ?status_name ?author WHERE {
    ?r a rev:Revision.
    FILTER NOT EXISTS {
        ?new rev:replaces ?r.
    }
    ?review a review:Review;
        review:aboutVersion ?r.
    ?review a ?status;
        review:issuedBy ?author.
    ?status rdfs:subClassOf review:ReviewState;
        rdfs:label ?status_name .
}
```
### Example answer
<table>
<tr>
exdoc:A123-BC-D-EF-00001.F01 "Reviewed" "Turi Skogen" .
</tr>
</table>
## Query 2
 Get all comments on a specific revision

```sparql
prefix rec: <https://rdf.equinor.com/ontology/record/>
prefix rev: <https://rdf.equinor.com/ontology/revision/>
prefix review: <https://rdf.equinor.com/ontology/review/>

SELECT ?reply ?comment ?object ?author  WHERE {
  
    ?reply a review:Review;
        review:aboutVersion exdoc:A123-BC-D-EF-00001.F01;
        review:hasComment ?c.
    ?c review:aboutObject ?object ;
        rdfs:label ?comment ;
        review:issuedBy ?author.
    OPTIONAL {
        ?c review:aboutProperty ?property.
    }
} 
```

### Example answers 
<table>
<tr>
exdoc:reply-A123-BC-D-EF-00001.F01 "<p>Test1</p>" exdoc:A123-BC-D-EF-00001.F01row1 "Turi Skogen" .
</tr><tr>
exdoc:reply-A123-BC-D-EF-00001.F01 "<p>Test2</p>" exdoc:A123-BC-D-EF-00001.F01row1 "Turi Skogen" .
</tr>
</table>


## Query 3
This query gets the status of the newest revisions for all documents
Just to show it is possible, this query does not assume materialization into a "head" graph, but instead assumes the records are in their own named graphs, as in the example files.
It also assumes that the comment ontology is in the un-named graph

```sparql
prefix rec: <https://rdf.equinor.com/ontology/record/>
prefix rev: <https://rdf.equinor.com/ontology/revision/>

SELECT distinct ?document ?newest_revision_name ?status_name ?reply_name ?reply_date ?comment_responsible WHERE {
    GRAPH ?record1 {
        ?reply a review:Review, ?status;
            review:aboutVersion ?revision;
            rdfs:label ?reply_name;
            prov:generatedAtTime ?reply_date;
            review:issuedBy ?comment_responsible.
    }
    GRAPH ?record2 {
        ?revision rdfs:label ?newest_revision_name;
            rev:describes ?document.
    }
    FILTER NOT EXISTS {
        GRAPH ?record3 {
            ?newRevision rev:replaces ?revision.
        }
    }
    ?status rdfs:subClassOf review:ReviewState;
        rdfs:label ?status_name.
}
```
### Example answers
<table>
<tr>
<td>document</td>	<td>newest_revision_name</td> 	<td>status_name</td>	<td>reply_name</td><td>reply_date</td>	<td>comment_responsible</td>
</tr>
<tr>
<td>exdoc:A123-BC-D-EF-00001</td><td> "Revision F01 of A123-BC-D-EF-00001" </td><td>"Reviewed" </td><td>"Review of revision F01" </td><td>"2024-01-11"^^xsd:date </td><td>"Turi Skogen"</td>
</tr>
</table>


## Query 4
This query gets all comments and the tag they are on(from any revisions and reviews). Here we assume all relevant data is materialized to the named graph rec:HeadContent. 

```sparql
prefix rec: <https://rdf.equinor.com/ontology/record/>
prefix rev: <https://rdf.equinor.com/ontology/revision/>
prefix review: <https://rdf.equinor.com/ontology/review/>

SELECT distinct ?document ?revision ?comment_text ?author ?comment_responsible ?date ?tag WHERE {
    GRAPH rec:HeadContent {
        ?reply a review:Review;
            review:issuedBy ?comment_responsible;
            review:aboutVersion ?revision;
            review:hasComment ?comment.
        ?comment a review:Comment;
            rdfs:label ?comment_text;
            review:issuedBy ?author;
            prov:generatedAtTime ?date;
            review:aboutObject ?row.
        ?row mel:tagNumber ?tag.
        ?revision rev:describes ?document.
    }
}
```
### Example answers
<table>
<tr>
exdoc:A123-BC-D-EF-00001 exdoc:A123-BC-D-EF-00001.F01 "<p>Test1</p>" "Turi Skogen" "Turi Skogen" "2024-01-10"^^xsd:date "00X0001" .
</tr><tr>
exdoc:A123-BC-D-EF-00001 exdoc:A123-BC-D-EF-00001.F01 "<p>Test2</p>" "Turi Skogen" "Turi Skogen" "2024-01-10"^^xsd:date "00X0001" .
</tr><tr>
exdoc:A123-BC-D-EF-00001 exdoc:A123-BC-D-EF-00001.F01 "<p>Test3</p>" "Turi Skogen" "Turi Skogen" "2024-01-10"^^xsd:date "00X0001" .
</tr>
</table>