using Xunit;
using VDS.RDF;
using VDS.RDF.Writing;
using Review;
using VDS.RDF.Parsing;
using VDS.RDF.Shacl;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Spreadsheet;
using static Review.Namespaces;
using VDS.RDF.Shacl.Validation;
using Xunit.Abstractions;
using FluentAssertions;
using ClosedXML.Excel;


namespace Review.Tests
{
    public class Tests
    {

        private readonly ITestOutputHelper _testOutputHelper;

        public Tests(ITestOutputHelper testOutputHelper)
        {
            this._testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void ReviewDtoTransformationShouldMaintainEquality()
        {
            //Arrange
            var reviewDto = CreateReviewDto();

            //Act
            var graph = RdfGenerator.GenerateRdf(reviewDto);
            var reviewDtoAfterTransformation = DtoGenerator.GenerateDto(graph);

            // Assert
            Assert.Equal(reviewDto.ReviewId, reviewDtoAfterTransformation.ReviewId);
            Assert.Equal(reviewDto.IssuedBy, reviewDtoAfterTransformation.IssuedBy);
            Assert.Equal(reviewDto.Label, reviewDtoAfterTransformation.Label);
          
            for (int i = 0; i < reviewDto.HasComments.Count; i++)
            {
                var expectedComment = reviewDto.HasComments[i];
                var actualComment = reviewDtoAfterTransformation.HasComments[i];

                Guid parsedGuid;
                bool isValidGuid = Guid.TryParse(actualComment.CommentId.ToString(), out parsedGuid);
                Assert.True(isValidGuid, "The CommentId is not a valid GUID.");

                Assert.False(string.IsNullOrEmpty(actualComment.CommentText), "The CommentText should not be null or empty.");

            }
        }
        [Fact]
        public void UseShaclToChekValidityOfRdf()
        {
            //Arrange
            var reviewDto = CreateReviewDto();
            var graph = RdfGenerator.GenerateRdf(reviewDto);
            var rdfCode = VDS.RDF.Writing.StringWriter.Write(graph, new CompressingTurtleWriter());
            CheckReview(rdfCode, "review.shacl");
        }
        internal void CheckReview(string rdf, string shacl_name)
        {
             //Act
             var graph = new Graph();
             StringParser.Parse(graph, rdf);
             
             var currentDirectory = Directory.GetCurrentDirectory();
             var shaclFileLocation = $"{currentDirectory}/TestData/{shacl_name}";
             var shapes = new Graph();
             shapes.LoadFromFile(shaclFileLocation);
             var shapeGraph = new ShapesGraph(shapes);

             //Assert
             var report = shapeGraph.Validate(graph);
             if (!report.Conforms)
             {
                foreach (var res in report.Results)
                    _testOutputHelper.WriteLine($"{res.FocusNode.ToString()}: {res.Message} detail: {res}");   
             }

             report.Conforms.Should().BeTrue();
        }


        [Fact]
        public void ValidateThatExcelIsCorrect()
        {
            // Arrange
            var reviewDto = CreateReviewDto();
            var expectedFilePath = "C:/Users/johannes.telle//source/repos/revisions/src/DtoTransformer/Tests/TestData/CorrectOutput.xlsx";
            var actualFilePath = "output.xlsx";

            // Act
            var graph = RdfGenerator.GenerateRdf(reviewDto);
            var reviewDtoAfterTransformation = DtoGenerator.GenerateDto(graph);
            ExcelGenerator.CreateExcelAt(reviewDtoAfterTransformation, actualFilePath);

            // Assert
            using (var expectedWorkbook = new XLWorkbook(expectedFilePath))
            using (var actualWorkbook = new XLWorkbook(actualFilePath))
            {
                foreach (var expectedSheet in expectedWorkbook.Worksheets)
                {
                    var actualSheet = actualWorkbook.Worksheet(expectedSheet.Name);
                    foreach (var expectedCell in expectedSheet.CellsUsed())
                    {
                        if (expectedCell.GetText().Contains("comment:"))
                        {
                            //pass
                        }
                        else
                        {
                            var actualCell = actualSheet.Cell(expectedCell.Address);
                            Assert.Equal(expectedCell.Value, actualCell.Value);
                        }
                    }
                }
            }
        }

        public ReviewDTO CreateReviewDto() {
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

            return reviewDto;
        }

    }
}
