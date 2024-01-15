SELECT * WHERE {
    GRAPH rec:HeadContent {
    ?reply a rev:Reply;
        rev:issuedBy ?comment_responsible;
        rev:aboutRevision ?revision;
        rev:hasComment ?comment.
        ?comment a rev:Comment;
        rdfs:label ?comment_text;
        rev:issuedBy ?author;
        prov:generatedAtTime ?date;
        rev:aboutObject ?row .
        ?row mel:tagNumber ?tagNumber .
       
        ?revision rev:describes ?document.
    }
}