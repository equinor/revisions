# Revisions


Data model and examples for representing revisions and tracking information in rdf.

This is an adaptation of the records format from https://github.com/equinor/records to the TR1244 format
at https://commonlib.equinor.com/Schema#/specification/TR1244 also using some types defined in tie.ttl in the
ti-spine-ontologies repo.

There are three main types of objects, the actual engineering **Content**, the **Document**, and the **Revision**.

## Content
This is the actual data that users want to transmit and read. In many use cases this is data created from engineering applications or even by hand.
The data can be used by the format described here if
* It is in Rdf (Note that many tabular formats can easily be translated to rdf)
* It is plain triples, no named graphs. (It is possible to use named graphs, but then they must match up with the named graphs used as records)
* There are Iris that identify the objects the data is about, and these Iris do not change over time. (This is usually the case when using Rdf, but it is not a technical requirement in Rdf, therefore stating here.)

Non-rdf documents should be represented as attachments to the Rdf.


## Revision
Revisions have information about when and how data changed. For example, if you add a new part to a machine or change the weight of an existing part, you need to make a new revision.
The revision represents a specific version of data, frozen at some moment in time. It is identified uniquely by the combination of site, document number and revision number.
It is meant to represent the existing revision concept and should support review processes.
The revision is related to all records that contain the data of that revision with the relation "rev:containsRecord". The triples of this relation _must_ be in the record describing the revision.
If the revision is delivered as one big record with document, revision and data, this relation is unnecessary.
We recommend putting the revision in a separate record for ease of manual reading during investigation.

## Document
The work processes we support also need a navigation path into the data called "document".
The document does not represent a version or concrete doument, it rather represents the collection of information, evolving over time.
Any information that does not change over time, could be put as properties on the document, in stead of on each revision.

## Examples
[MasterEquipmentList]('doc/mel-revision-example.md')
