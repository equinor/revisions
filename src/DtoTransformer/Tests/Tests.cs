using Xunit;
using VDS.RDF;
using VDS.RDF.Writing;


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
                CommentId = "https://example.com/data/first-uuid",
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
        public void RdfGeneratorCorrectlyCreatesRdfFromReviewDto()
        {
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
                CommentId = "https://example.com/data/first-uuid",
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
                CommentId = "https://example.com/data/another-uuid",
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

            var graph = RdfGenerator.GenerateRdf(reviewDto);

            // Assert
            var expectedGraph = new Graph();
            expectedGraph.LoadFromString(correctRdfString);

            foreach (var expectedTriple in expectedGraph.Triples)
            {
                Assert.Contains(expectedTriple, graph.Triples);
            }

            
        }
        private string correctRdfString = @"
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
            @prefix mel: <https://rdf.equinor.com/ontology/mel/v1#>.

            exdata:another-uuid a review:Comment;
                                rdfs:label ""Another comment"";
                                prov:generatedAtTime ""3/14/2024""^^xsd:date;
                                review:aboutData (<https://example.com/doc/AnotherDocument.Row1>);
                                review:aboutObject [a review:FilterObject ; 
                                                    mel:tagNumber ""the tag number"" ; 
                                                    mel:weightHandlingCode ""The handling code""];
                                review:issuedBy ""John Doe"".
            exdata:first-uuid a review:Comment;
                              rdfs:label ""A comment"";
                              prov:generatedAtTime ""3/14/2024""^^xsd:date;
                              review:aboutData (<https://example.com/doc/A123-BC-D-EF-00001.F01row1>                                    <https://example.com/doc/A123-BC-D-EF-00001.F01row3>                                    <https://example.com/doc/A123-BC-D-EF-00001.F01row10>);
                              review:aboutObject [a review:FilterObject ; 
                                                  mel:tagNumber ""the tag number"" ; 
                                                  mel:weightHandlingCode ""The handling code"" ; 
                                                  mel:importantField ""The important field""];
                              review:issuedBy ""Johannes"".
            <https://example.com/doc/reply-A123-BC-D-EF-00001.F01> a review:Review,
                                                                     review:Code1;
                                                                   rdfs:label ""Reply to revision F01"";
                                                                   prov:generatedAtTime ""3/14/2024""^^xsd:date;
                                                                   review:aboutRevision <https://example.com/data/A123-BC-D-EF-00001.F01>;
                                                                   review:hasComment exdata:first-uuid,
                                                                                     exdata:another-uuid;
                                                                   review:issuedBy ""Turi Skogen"".

            ";

    }
}
