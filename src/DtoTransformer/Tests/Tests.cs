using VDS.RDF;
using VDS.RDF.Writing;
using VDS.RDF.Parsing;
using VDS.RDF.Shacl;
using Xunit.Abstractions;
using FluentAssertions;
using static System.Net.WebRequestMethods;


namespace Review.Tests
{
    public class Tests(ITestOutputHelper testOutputHelper)
    {

        private readonly ITestOutputHelper _testOutputHelper = testOutputHelper;

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
        public void TransformerShouldHandleExampleComments2Trig()
        {
            ITripleStore exampleRdf = new TripleStore();
            exampleRdf.LoadFromFile("TestData/comments2.trig");
            IGraph graph = exampleRdf[new UriNode(new Uri("https://example.com/data/RecordID123_5"))] ??
                           throw new Exception("File comments2.trig should have a record with id exdata:RecordID123_5");
            var reviewDto = DtoGenerator.GenerateDto(graph);
            reviewDto.Should().NotBeNull();
            reviewDto.HasComments.Count.Should().Be(3, "exdoc:reply-A123-BC-D-EF-00001_F01 has three comments");
            reviewDto.GeneratedAtTime.Should().Be(new DateOnly(2024, 1, 11));
            reviewDto.Status.Should().Be(ReviewStatus.Code1);
        }

        [Fact]
        public void TransformerShouldHandleExampleCommentsTrig()
        {
            ITripleStore exampleRdf = new TripleStore();
            exampleRdf.LoadFromFile("TestData/comments.trig");
            IGraph graph = exampleRdf[new UriNode(new Uri("https://example.com/data/RecordID123_5"))] ??
                           throw new Exception("File comments2.trig should have a record with id exdata:RecordID123_5");
            var reviewDto = DtoGenerator.GenerateDto(graph);
            reviewDto.Should().NotBeNull();
            reviewDto.HasComments.Count.Should().Be(3, "exdoc:reply-A123-BC-D-EF-00001_F01 has three comments");
            reviewDto.GeneratedAtTime.Should().Be(new DateOnly(2023, 6, 15));
            reviewDto.Status.Should().Be(ReviewStatus.Code1);
            var act = Record.Exception(() => reviewDto.ReviewGuid);
            act.Should().NotBeNull()
                .And.BeOfType<InvalidOperationException>();
            act?.Message.Should().Be("ReviewId not set");
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
            Assert.Equal(reviewDto.Status, reviewDtoAfterTransformation.Status);
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
                commentDto.AboutObject.Sort((x, y) => Uri.Compare(x.Property, y.Property, UriComponents.AbsoluteUri, UriFormat.UriEscaped, StringComparison.Ordinal));
                commentDtoAfterTransformation.AboutObject.Sort((x, y) => Uri.Compare(x.Property, y.Property, UriComponents.AbsoluteUri, UriFormat.UriEscaped, StringComparison.Ordinal));
                for (int j = 0; i < commentDto.AboutObject.Count; i++)
                {
                    commentDto.AboutObject[j].Should().BeEquivalentTo(commentDtoAfterTransformation.AboutObject[j]);
                }
            }

        }

        public static ReviewDTO CreateReviewDto()
        {
            var reviewDto = new ReviewDTO
            {
                ReviewIri = "https://example.com/doc/reply-A123-BC-D-EF-00001.F01",
                AboutRevision = new Uri("https://example.com/data/A123-BC-D-EF-00001.F01"),
                IssuedBy = "Turi Skogen",
                GeneratedAtTime = DateOnly.FromDateTime(DateTime.Now),
                Status = ReviewStatus.Code1,
                Label = "Reply to revision F01",
                HasComments = new List<CommentDto>()
            };


            var commentDto = new CommentDto
            {
                CommentUri = new Uri($"https://rdf.equinor.com/data/review/comment/{Guid.NewGuid()}"),
                CommentText = "A comment",
                IssuedBy = "Johannes",
                GeneratedAtTime = DateOnly.FromDateTime(DateTime.Now),
                AboutData =
                [
                    new Uri("https://example.com/doc/A123-BC-D-EF-00001.F01row1"),
                    new Uri("https://example.com/doc/A123-BC-D-EF-00001.F01row3"),
                    new Uri("https://example.com/doc/A123-BC-D-EF-00001.F01row10")
                ],
                AboutObject =
                [
                    new PropertyValuePair { Property = new Uri("https://rdf.equinor.com/ontology/mel/v1#tagNumber"), Value = "The tag number" },
                    new PropertyValuePair { Property = new Uri("https://rdf.equinor.com/ontology/mel/v1#weightHandlingCode"), Value = "The handling code" },
                    new PropertyValuePair { Property = new Uri("https://rdf.equinor.com/ontology/mel/v1#importantField"), Value = "The important field" }
                ]
            };

            var anotherCommentDto = new CommentDto
            {
                CommentUri = new Uri($"https://rdf.equinor.com/data/review/comment/{Guid.NewGuid()}"),
                CommentText = "Another comment",
                IssuedBy = "John Doe",
                GeneratedAtTime = DateOnly.FromDateTime(DateTime.Now),
                AboutData = new List<Uri>()
                {
                    new("https://example.com/doc/AnotherDocument.Row1")
                },
                AboutObject =
                [
                    new PropertyValuePair { Property = new Uri("https://rdf.equinor.com/ontology/mel/v1#tagNumber"), Value = "The tag number" },
                    new PropertyValuePair { Property = new Uri("https://rdf.equinor.com/ontology/mel/v1#weightHandlingCode"), Value = "The handling code" }
                ]
            };

            reviewDto.HasComments.Add(commentDto);
            reviewDto.HasComments.Add(anotherCommentDto);

            return reviewDto;
        }

    }
}