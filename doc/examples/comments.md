# Example 
Based on this
```json
[
    {
        "UniqueId": "00X0001",
        "Text": "Test1",
        "DocumentName": "A123-BC-D-EF-00001",
        "Revision": "F01",
        "User": "Line Danser",
        "Guid": "cb086a54-b4af-11ee-9f99-37a86e9a48aa",
        "AboutData": [
            "https://example.com/doc/A123-BC-D-EF-00001.F01row1",
            "https://example.com/doc/A123-BC-D-EF-00001.F01row3",
            "https://example.com/doc/A123-BC-D-EF-00001.F01row10"
        ],
        "AboutObject": [
            {
                "property": "https://rdf.equinor.com/ontology/mel/v1#tagNumber",
                "value": "the tag number"
            },
            {
                "property": "https://rdf.equinor.com/ontology/mel/v1#weightHandlingCode",
                "value": "The handling code"
            },
            {
                "property": "https://rdf.equinor.com/ontology/mel/v1#importantField",
                "value": "The important field"
            }
        ]
    },
    {
        "UniqueId": "00X0001",
        "Text": "Test2",
        "DocumentName": "A123-BC-D-EF-00001",
        "Revision": "F01",
        "User": "Turi Skogen",
        "Guid": "d24cca6c-b4af-11ee-ad44-df4f729a38a3",
        "AboutData": [
            "https://example.com/doc/A123-BC-D-EF-00001.F01row1"
        ],
        "AboutObject": [
            {
                "property": "https://rdf.equinor.com/ontology/mel/v1#tagNumber",
                "value": "the tag number"
            },
            {
                "property": "https://rdf.equinor.com/ontology/mel/v1#weightHandlingCode",
                "value": "The handling code"
            }
        ]
    },
    {
        "UniqueId": "00X0002",
        "Text": "Test3",
        "DocumentName": "A123-BC-D-EF-00001",
        "Revision": "F01",
        "User": "Kari Nordmann",
        "Guid": "ff086a54-b4af-11ee-9f99-37a86e9a48aa",
        "AboutData": [
            "https://example.com/doc/A123-BC-D-EF-00001.F02row5",
            "https://example.com/doc/A123-BC-D-EF-00001.F02row7"
        ],
        "AboutObject": [
            {
                "property": "https://rdf.equinor.com/ontology/mel/v1#tagNumber",
                "value": "the tag number"
            },
            {
                "property": "https://rdf.equinor.com/ontology/mel/v1#weightHandlingCode",
                "value": "The handling code"
            }
        ]
    }
]
```
We create this
```turtle
@prefix rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>.
@prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#>.
@prefix xsd: <http://www.w3.org/2001/XMLSchema#>.
@prefix exdata: <https://example.com/data/>.
@prefix exdoc: <https://example.com/doc/>.
@prefix rec: <https://rdf.equinor.com/ontology/record/>.
@prefix rev: <https://rdf.equinor.com/ontology/revision/>.
@prefix review: <https://rdf.equinor.com/ontology/review/>.
@prefix prov: <http://www.w3.org/ns/prov#>.
@prefix rdl: <http://example.com/rdl/>.
@prefix mel:            <https://rdf.equinor.com/ontology/mel/v1#> .




exdata:RecordID123.1 {
    exdata:RecordID123.1 a rec:Record;
        rdfs:comment "This is the revision";
                        rec:isInScope 
                            <https://rdf.equinor.com/ontology/technical-requirement/v1#MelReportingTemplate>, 
                            <https://commonlib.equinor.com/rdf/code/Project/1JjvqAHFRbED1YPuaY> , 
                            <https://commonlib.equinor.com/rdf/code/LCIContractor/4Do7Yb64WXh> ,
                            <https://commonlib.equinor.com/rdf/code/Contract/1JjvqAHFRbED1YPuaX> , 
                            <https://commonlib.equinor.com/rdf/code/Facility/FM5LjGVt> ;
                        rec:describes exdoc:A123-BC-D-EF-00001.F01.
    exdoc:A123-BC-D-EF-00001.F01 a rev:Revision;
        rev:describes exdoc:A123-BC-D-EF-00001.
    exdoc:A123-BC-D-EF-00001.F01row1 mel:tagNumber "00X0001".
}

exdata:RecordID123.5 {
    exdata:RecordID123.5 a rec:Record;
                        rdfs:comment "Based on example data from Sondre"^^xsd:string;
                        prov:generatedAtTime "2024-01-11"^^xsd:date;
                        rec:isInScope review:Review, 
                            <https://rdf.equinor.com/ontology/technical-requirement/v1#MelReportingTemplate>, 
                            <https://commonlib.equinor.com/rdf/code/Project/1JjvqAHFRbED1YPuaY> , 
                            <https://commonlib.equinor.com/rdf/code/LCIContractor/4Do7Yb64WXh> ,
                            <https://commonlib.equinor.com/rdf/code/Contract/1JjvqAHFRbED1YPuaX> , 
                            <https://commonlib.equinor.com/rdf/code/Facility/FM5LjGVt> ;
                        rec:describes exdoc:A123-BC-D-EF-00001.F01.

    exdoc:reply-A123-BC-D-EF-00001.F01 a review:Review;
                                review:aboutVersion exdoc:A123-BC-D-EF-00001.F01;
                                review:issuedBy "Turi Skogen";
                                prov:generatedAtTime "2024-01-11"^^xsd:date;
                                rdf:type review:Code1;
                                rdfs:label "Reply to revision F01";
                                review:hasComment exdata:cb086a54-b4af-11ee-9f99-37a86e9a48aa, 
                                    exdata:d24cca6c-b4af-11ee-ad44-df4f729a38a3, 
                                    exdata:ff086a54-b4af-11ee-9f99-37a86e9a48aa.
                            
    exdata:cb086a54-b4af-11ee-9f99-37a86e9a48aa a review:Comment;
                                                rdfs:label "Test1";
                                                prov:generatedAtTime "2022-01-02"^^xsd:date;
                                                review:aboutData (<https://example.com/doc/A123-BC-D-EF-00001.F01row1>
                                                                <https://example.com/doc/A123-BC-D-EF-00001.F01row3>
                                                                <https://example.com/doc/A123-BC-D-EF-00001.F01row10>);
                                                review:aboutObject [a review:FilterObject ;
                                                                    mel:tagNumber "the tag number";
                                                                    mel:weightHandlingCode "The handling code";
                                                                    mel:importantField "The important field"];
                                                review:issuedBy "Line Danser".

    exdata:d24cca6c-b4af-11ee-ad44-df4f729a38a3 a review:Comment;
                                                rdfs:label "Test2";
                                                prov:generatedAtTime "2022-01-02"^^xsd:date;
                                                review:aboutData (<https://example.com/doc/A123-BC-D-EF-00001.F01row1>);
                                                review:aboutObject [a review:FilterObject ;
                                                                    mel:tagNumber "the tag number";
                                                                    mel:weightHandlingCode "The handling code"];
                                                review:issuedBy "Turi Skogen".

    exdata:ff086a54-b4af-11ee-9f99-37a86e9a48aa a review:Comment;
                                                rdfs:label "Test3";
                                                prov:generatedAtTime "2022-01-02"^^xsd:date;
                                                review:aboutData (<https://example.com/doc/A123-BC-D-EF-00001.F01row5> <https://example.com/doc/A123-BC-D-EF-00001.F01row7>);
                                                review:aboutObject [a review:FilterObject ;
                                                                    mel:tagNumber "the tag number";
                                                                    mel:weightHandlingCode "The handling code"];
                                                review:issuedBy "Kari Nordmann".
}


```
### Some of the IDs here are copied from the input revision
* All scopes except review:Review. (Facility, Project, COntract, Contractor, and MelReportingTemplate). Including all scopes from the revision is also ok, although perhaps slightly misleading. 
* The object of the relation review:aboutVersion
* The objects of the relation review:aboutObject

### Some of the IDs are generated
* The record IRI (antyhing goes, but must be unique)
* The comment IRI (based on comment ID?)
* The ID of the Review. It must be unique for the revision, but otherwise anything goes - the link to the revision is in the aboutRevision property


![Graphical representation of the graph above](comments2.png)