using FluentAssertions;
using Review;
using VDS.RDF;
using Xunit;

namespace DtoTransformerNugetTest;

public class TestDtoTransformer
{
    [Fact]
    public void TransformerShouldHandleExampleCommentsTrig()
    {
        ITripleStore exampleRdf = new TripleStore();
        exampleRdf.LoadFromFile("comments.trig");
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
}