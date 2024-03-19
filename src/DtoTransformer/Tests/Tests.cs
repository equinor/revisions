using Xunit;
using VDS.RDF;
using VDS.RDF.Writing;
using Review;


namespace Review.Tests
{
    public class Tests
    {
        [Fact]
        public void ReviewDtoTransformationShouldMaintainEquality()
        {
            // Arrange
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
            //TODO            
        }

    }
}
