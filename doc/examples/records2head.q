INSERT {
    GRAPH rec:HeadContent {?s ?p ?o}
} WHERE {
    GRAPH ?rec {?rec a rec:Record. ?s ?p ?o}
}