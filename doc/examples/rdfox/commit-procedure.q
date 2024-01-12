INSERT {
    GRAPH rec:HeadContent {?s ?p ?o}
} WHERE {
    GRAPH ?rec {?rec a rec:Record. ?s ?p ?o}
}
INSERT { 
    ?s a <https://rdfox.com/vocabulary#ConstraintViolation> . 
    ?s ?p ?o 
} WHERE { 
    TT SHACL { rec:HeadContent schema:shacl ?s ?p ?o} . 
    FILTER(?p IN (sh:sourceShape, sh:resultMessage, sh:value)) 
}