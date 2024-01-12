SELECT ?status_name ?comment_text ?author WHERE {
  GRAPH rec:HeadContent {
    ?r a rev:Revision.
    FILTER NOT EXISTS {
        ?new rev:replaces ?r.
    }
    ?reply a rev:Reply;
        rev:aboutRevision ?r;
        rev:hasStatus ?s.
    ?s a ?status;
        rev:aboutObject ?object;
        rev:issuedBy ?author.
    ?status rdfs:subClassOf rev:RevisionState;
        rdfs:label ?status_name .
    OPTIONAL {
        ?s rev:hasComment ?comment.
            ?comment rdfs:label ?comment_text.
    }
  }
}