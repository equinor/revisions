SELECT distinct ?object ?comment ?author ?p ?tag WHERE {
  GRAPH rec:HeadContent {
    ?reply a rev:Reply;
        rev:aboutRevision exdoc:C232-AI-R-LA-00001.F01;
        rev:hasComment ?c.
    ?c rev:aboutObject ?object ;
        rdfs:label ?comment ;
        rev:issuedBy ?author.
    ?object ?p ?tag.
  }
}