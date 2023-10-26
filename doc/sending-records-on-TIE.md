
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
<InterfaceData GUID="3f3f1f9c-e0cd-4998-8d23-082c350d10ac" PackageGuid="00000000-0000-0000-0000-000000000000" TimeStamp="2023-09-27T10:42:01.9616546+02:00" Action="APPEND" Site="AHA" Project="0000" ObjectClass="RecordSet" ObjectType="RecordSet" MessageVersion="001.1">
  <MetaData />
  <Objects>
    <Object Class="RecordSet" Site="AHA" Project="0000" Method="APPEND" GUID="673e1357-f525-442b-b64e-a3433bc5cea0">
	  <Attributes>
	    <Attribute Name="Contract">1111000011</Attribute>
		<Attribute Name="Contractor">AS</Attribute>
	  </Attributes>
	  <SubObjects />
	  <Files>
	    <File Class="Record" GUID="ec844a28-6c1a-446d-bdb1-0b9dc225d6f8" ParentObjectGUID="673e1357-f525-442b-b64e-a3433bc5cea0" FileName="https://record.example.com/record/8ee6a905-0dc0-46f1-969c-dc7aa174cf32" FileType="json-ld">
		  <Attributes>
		    <Attribute Name="MimeType">application/ld+json</Attribute>
		  </Attributes>
		</File>
		  <File Class="Record" GUID="3e6911ee-3373-4d37-a490-aec3b76c880d" ParentObjectGUID="673e1357-f525-442b-b64e-a3433bc5cea0" FileName="https://record.example.com/record/3b7a0a9b-8caa-4652-b82b-f863fbaefc82" FileType="json-ld">
		    <Attributes>
			  <Attribute Name="MimeType">application/ld+json</Attribute>
	        </Attributes>
		</File>
		    <File Class="Record" GUID="e592a03a-62f3-46a5-acf0-21b2a525ccc2" ParentObjectGUID="673e1357-f525-442b-b64e-a3433bc5cea0" FileName="https://record.example.com/record/215e31e8-4f61-4e2b-8c3b-a12214b6854d" FileType="json-ld">
			  <Attributes>
			    <Attribute Name="MimeType">application/ld+json</Attribute>
              </Attributes>
	   </File>
         <File Class="Record" GUID="5651b0b2-ebae-4750-946d-8196ea8b612d" ParentObjectGUID="673e1357-f525-442b-b64e-a3433bc5cea0" FileName="https://record.example.com/record/0bb71a06-bd6f-4c83-8ff0-45983b376542" FileType="json-ld">
           <Attributes>
             <Attribute Name="MimeType">application/ld+json</Attribute>
           </Attributes>
       </File>
	     <File Class="Record" GUID="0025092b-afe9-4d7f-ae11-e33edaf4a62a" ParentObjectGUID="673e1357-f525-442b-b64e-a3433bc5cea0" FileName="https://record.example.com/record/d5f6be8f-1ba9-47fd-b15e-efd2f5fe203f" FileType="json-ld">
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