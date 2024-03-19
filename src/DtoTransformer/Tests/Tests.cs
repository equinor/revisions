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
                Assert.Equal(expectedComment.CommentId, actualComment.CommentId);
                Assert.Equal(expectedComment.CommentText, actualComment.CommentText);
            }
        }
        [Fact]
        public void UseShaclToChekValidityOfRdf()
        {
            var reviewDto = CreateReviewDto();
            var graph = RdfGenerator.GenerateRdf(reviewDto);
            var rdfCode = VDS.RDF.Writing.StringWriter.Write(graph, new CompressingTurtleWriter());
            CheckReview(rdfCode, "C:/Users/johannes.telle/source/repos/revisions/schema/review.shacl");
        }
        internal void CheckReview(string rdf, string shacl_name)
        {
             var graph = new Graph();
             StringParser.Parse(graph, rdf);
             var shaclFileLocation = shacl_name;
             var shapes = new Graph();
             shapes.LoadFromFile(shaclFileLocation);
             var shapeGraph = new ShapesGraph(shapes);
             var report = shapeGraph.Validate(graph);
             if (!report.Conforms)
             {
                foreach (var res in report.Results)
                    _testOutputHelper.WriteLine($"{res.FocusNode.ToString()}: {res.Message} detail: {res}");   
             }

             report.Conforms.Should().BeTrue();
        }

        public ReviewDTO CreateReviewDto() {
            var reviewDto = new ReviewDTO
            {
                ReviewId = "https://example.com/doc/reply-A123-BC-D-EF-00001.F01",
                AboutRevision = new Uri("https://example.com/data/A123-BC-D-EF-00001.F01"),
                IssuedBy = "Turi Skogen",
                GeneratedAtTime = DateOnly.FromDateTime(DateTime.UtcNow),
                ReviewStatus = "https://rdf.equinor.com/ontology/review/Code1",
                Label = "Reply to revision F01",
                HasComments = new List<CommentDto>()
            };

            var commentDto = new CommentDto
            {
                CommentUri = new Uri($"https://rdf.equinor.com/data/review/comment/{Guid.NewGuid()}"),
                CommentText = "A comment",
                IssuedBy = "Johannes",
                GeneratedAtTime = DateOnly.FromDateTime(DateTime.UtcNow),
                AboutData = new List<Uri>
                {
                    new Uri("https://example.com/doc/A123-BC-D-EF-00001.F01row1"),
                    new Uri("https://example.com/doc/A123-BC-D-EF-00001.F01row3"),
                    new Uri("https://example.com/doc/A123-BC-D-EF-00001.F01row10")
                },
                AboutObject = new List<(Uri property, string value)>
                {
                    (new Uri("https://rdf.equinor.com/ontology/mel/v1#tagNumber"), "the tag number"),
                    (new Uri("https://rdf.equinor.com/ontology/mel/v1#weightHandlingCode"), "The handling code"),
                    (new Uri("https://rdf.equinor.com/ontology/mel/v1#importantField"), "The important field")
                }
            };


            reviewDto.HasComments.Add(commentDto);
            return reviewDto;
        }

    }
}
