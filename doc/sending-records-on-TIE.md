
# Sending records on TIE

## The TIE format

### Object Class Record Set
The attributes Contract and Contractor is required. 
The values of Contract, Contractor, Site and Project must exist in the column "Name" of the corresponding library in Common Library. Please refer to the table below for guidance on which
libraries to use in order to find the values.

TIE meta data | Common Library |
--- | --- | 
Site | https://commonlib.equinor.com/Library/Facility | 
Project | https://commonlib.equinor.com/Library/Project | 
Contract | https://commonlib.equinor.com/Library/Contract | 
Contractor | https://commonlib.equinor.com/Library/LCIContractor | 

### File Class Record
The MimeType attribute is required and should always have the value "application/ld+json".

If any of the attributes listed above is missing, or if the values is not according to the description, the message will be rejected by Bravo.

### Example TIE message
```xml
<?xml version="1.0" encoding="utf-8"?>
<InterfaceData GUID="00000001-0000-0000-0000-000000000001" PackageGuid="00000000-0000-0000-0000-000000000000" TimeStamp="2023-10-25T12:42:01.12345+02:00" Action="APPEND" Site="Site" Project="Project" ObjectClass="RecordSet" ObjectType="RecordSet" MessageVersion="001.1">
  <MetaData />
  <Objects>
    <Object Class="RecordSet" Site="Site" Project="Project" Method="APPEND" GUID="00000003-0000-0000-0000-000000000003">
	  <Attributes>
	    <Attribute Name="Contract">Contract</Attribute>
		<Attribute Name="Contractor">Contractor</Attribute>
	  </Attributes>
	  <SubObjects />
	  <Files>
	    <File Class="Record" GUID="00000002-0000-0000-0000-000000000002" ParentObjectGUID="00000003-0000-0000-0000-000000000003" FileName="record1.jsonld" FileType="json-ld">
		  <Attributes>
		    <Attribute Name="MimeType">application/ld+json</Attribute>
		  </Attributes>
		</File>
		  <File Class="Record" GUID="00000004-0000-0000-0000-000000000004" ParentObjectGUID="00000003-0000-0000-0000-000000000003" FileName="record2.jsonld" FileType="json-ld">
		    <Attributes>
			  <Attribute Name="MimeType">application/ld+json</Attribute>
	        </Attributes>
		</File>
		    <File Class="Record" GUID="00000005-0000-0000-0000-000000000005" ParentObjectGUID="00000003-0000-0000-0000-000000000003" FileName="recrd3.jsonld" FileType="json-ld">
			  <Attributes>
			    <Attribute Name="MimeType">application/ld+json</Attribute>
              </Attributes>
	   </File>
     </Files>
     <Relationships />
   </Object>
  </Objects>
  <Relationships />
</InterfaceData>
```