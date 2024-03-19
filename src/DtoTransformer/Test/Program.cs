using VDS.RDF.Writing;
using Review;
using System.Runtime.CompilerServices;
using static System.Runtime.InteropServices.JavaScript.JSType;


var reviewDto = new ReviewDTO
{
    ReviewId = "https://example.com/doc/reply-A123-BC-D-EF-00001.F01",
    AboutRevision = new Uri("https://example.com/data/A123-BC-D-EF-00001.F01"),
    IssuedBy = "Turi Skogen",
    GeneratedAtTime = DateOnly.FromDateTime(DateTime.Now),
    ReviewStatus = "https://rdf.equinor.com/ontology/review/Code1",
    Label = "Reply to revision F01",
    HasComments = new List<CommentDto>()
};


var commentDto = new CommentDto
{
    CommentUri = new Uri($"https://rdf.equinor.com/data/review/comment/{Guid.NewGuid()}"),
    CommentText = "A comment",
    IssuedBy = "Johannes",
    GeneratedAtTime = DateOnly.FromDateTime(DateTime.Now),
    AboutData = new List<Uri>()
    {
        new Uri("https://example.com/doc/A123-BC-D-EF-00001.F01row1"),
        new Uri("https://example.com/doc/A123-BC-D-EF-00001.F01row3"),
        new Uri("https://example.com/doc/A123-BC-D-EF-00001.F01row10")
    },
    AboutObject = new List<(Uri property, string value)>()
    {
        (new Uri("https://rdf.equinor.com/ontology/mel/v1#tagNumber"), "the tag number"),
        (new Uri("https://rdf.equinor.com/ontology/mel/v1#weightHandlingCode"), "The handling code"),
        (new Uri("https://rdf.equinor.com/ontology/mel/v1#importantField"), "The important field")
    }
};

var anotherCommentDto = new CommentDto
{
    CommentUri = new Uri($"https://rdf.equinor.com/data/review/comment/{Guid.NewGuid()}"),
    CommentText = "Another comment",
    IssuedBy = "John Doe",
    GeneratedAtTime = DateOnly.FromDateTime(DateTime.Now),
    AboutData = new List<Uri>()
    {
        new Uri("https://example.com/doc/AnotherDocument.Row1")
    },
    AboutObject = new List<(Uri property, string value)>()
    {
        (new Uri("https://rdf.equinor.com/ontology/mel/v1#tagNumber"), "the tag number"),
        (new Uri("https://rdf.equinor.com/ontology/mel/v1#weightHandlingCode"), "The handling code")
    }
};


reviewDto.HasComments.Add(commentDto);
reviewDto.HasComments.Add(anotherCommentDto);


Console.Write("First DTO");
printInfo(reviewDto);

var graph = RdfGenerator.GenerateRdf(reviewDto);

Console.Write("First RDF");

var rdfCode = VDS.RDF.Writing.StringWriter.Write(graph, new CompressingTurtleWriter());

Console.WriteLine(rdfCode);

//This line does not work, stating "Error CS0103	The name 'ExcelGenerator' does not exist in the current context	Test"

//Generating excel should work correctly.

ExcelGenerator.CreateExcelAt(reviewDto, "output.xlsx");
Console.WriteLine("Generated excel at output.xlsx");

//SECOND DTO
reviewDto = DtoGenerator.GenerateDto(graph);

Console.Write("Second DTO");
printInfo(reviewDto);

graph = RdfGenerator.GenerateRdf(reviewDto);
Console.Write("Second RDF");
rdfCode = VDS.RDF.Writing.StringWriter.Write(graph, new CompressingTurtleWriter());
Console.WriteLine(rdfCode);

//SECOND DTO
reviewDto = DtoGenerator.GenerateDto(graph);
Console.Write("Third DTO");
printInfo(reviewDto);



static void printInfo(ReviewDTO review)
{
    Console.WriteLine("Review ID: " + review.ReviewId);
    Console.WriteLine("Review Label: " + review.Label);
    Console.WriteLine("Review Version: " + review.AboutRevision);
    Console.WriteLine("Issued By: " + review.IssuedBy);
    Console.WriteLine("Generated At Time: " + review.GeneratedAtTime);
    Console.WriteLine("Review Status: " + review.ReviewStatus);

    Console.WriteLine("Has comments");

    foreach (CommentDto comment in review.HasComments)
    {
        Console.WriteLine("------------------------------");

        Console.WriteLine("Comment ID: " + comment.CommentId);
        Console.WriteLine("Comment Text: " + comment.CommentText);

        Console.WriteLine("Issued By: " + comment.IssuedBy);
        Console.WriteLine("Generated At Time: " + comment.GeneratedAtTime);

        Console.WriteLine("About Data: " + string.Join(", ", comment.AboutData));
        Console.WriteLine("About Object: " + string.Join(", ", comment.AboutObject.Select(x => x.property + " = " + x.value)));
    }
    Console.WriteLine("------------------------------");
}
