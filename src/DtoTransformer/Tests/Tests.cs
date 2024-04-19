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
using Lucene.Net.Search.Similarities;


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
            Assert.Equal(reviewDto.ReviewGuid, reviewDtoAfterTransformation.ReviewGuid);
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
            var actualFilePath = "output.xlsx";

            // Act
            var graph = RdfGenerator.GenerateRdf(reviewDto);
            var reviewDtoAfterTransformation = DtoGenerator.GenerateDto(graph);
            ExcelGenerator.CreateExcelAt(reviewDtoAfterTransformation, actualFilePath);

            var reviewDtoFromExcel = ExcelParser.ParseExcelToReviewDTO(actualFilePath);


            // Assert
            Assert.Equal(reviewDto.ReviewGuid, reviewDtoAfterTransformation.ReviewGuid);
            Assert.Equal(reviewDto.IssuedBy, reviewDtoAfterTransformation.IssuedBy);
            Assert.Equal(reviewDto.ReviewStatus, reviewDtoAfterTransformation.ReviewStatus);
            Assert.Equal(reviewDto.Label, reviewDtoAfterTransformation.Label);

            //sort HasComments CommentDto based on each of the CommentId

            var sortedCommentsDto = reviewDto.HasComments.OrderBy(comment => comment.CommentId).ToList();
            var sortedCommentsDtoAfterTransformation = reviewDtoAfterTransformation.HasComments.OrderBy(comment => comment.CommentId).ToList();

            for (int i = 0; i < sortedCommentsDto.Count; i++)
            {
                var commentDto = sortedCommentsDto[i];
                var commentDtoAfterTransformation = sortedCommentsDtoAfterTransformation[i];

                Assert.Equal(commentDto.CommentId, commentDtoAfterTransformation.CommentId);
                Assert.Equal(commentDto.IssuedBy, commentDtoAfterTransformation.IssuedBy);
                Assert.Equal(commentDto.CommentText, commentDtoAfterTransformation.CommentText);

                //Sort CommentDto.AboutObject by Uri for both commentDto and commentDtoAfterTransformation
                commentDto.AboutObject.Sort((x, y) => Uri.Compare(x.property, y.property, UriComponents.AbsoluteUri, UriFormat.UriEscaped, StringComparison.Ordinal));
                commentDtoAfterTransformation.AboutObject.Sort((x, y) => Uri.Compare(x.property, y.property, UriComponents.AbsoluteUri, UriFormat.UriEscaped, StringComparison.Ordinal));
                for (int j = 0; i < commentDto.AboutObject.Count; i++)
                {
                    Assert.Equal(commentDto.AboutObject[j], commentDtoAfterTransformation.AboutObject[j]);
                }


            }




        }

        public ReviewDTO CreateReviewDto()
        {
            var reviewDto = new ReviewDTO
            {
                ReviewIri = "https://example.com/doc/reply-A123-BC-D-EF-00001.F01",
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
