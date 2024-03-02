using ClosedXML.Excel;
using VDS.RDF;

namespace Review;

public static class ExcelGenerator
{
    // Naive implementation of revision name getter
    public static string GetRevisionName(Uri revisionUri) =>
        revisionUri.LocalPath.ToString();

    
    // Naive implementation of getting property labels
    public static string GetPropertyName(Uri propertyUri) =>
        propertyUri.Fragment.Substring(1);

    
    public static void CreateExcelAt(ReviewDTO review, string path)
    {
        review.GenerateExcel(GetRevisionName, GetPropertyName).SaveAs(path);
    }
    
    public static XLWorkbook GenerateExcel(this ReviewDTO review, Func<Uri, string> getRevisionName, Func<Uri, string> getPropertyName)
    {
        var workbook = new XLWorkbook();
        var worksheet = workbook.AddWorksheet("Review Comments");
        review.AddReviewHeader(worksheet, getRevisionName);
        int nextFreeRow = worksheet.AddOttrPrefix(9);
        review.GenerateCommentRows(worksheet, nextFreeRow, getPropertyName);
        return workbook;
    }

    public static int GenerateCommentRows(this ReviewDTO review, IXLWorksheet worksheet, int startRow,
        Func<Uri, string> propertyLabelGetter)
    {
        var filterUriss = GetAllObjectFilters(review.HasComments)
            .ToArray();
        var filterNames = filterUriss.Select(propertyLabelGetter).ToArray();
        var commentRow = review.AddOttrTemplateStart(worksheet, startRow, filterNames);
        foreach (var comment in review.HasComments)
        {
            comment.CommentToExcelRow(worksheet, commentRow, filterUriss);
            commentRow++;
        }
        return review.AddOttrTemplateEnd(worksheet, commentRow);
    }
    
    public static void AddReviewHeader(this ReviewDTO review, IXLWorksheet worksheet, Func<Uri, string> getRevisionName)
    {
        worksheet.Cell("A1").Value = review.Label;
        worksheet.Cell("A2").Value = "MASTER EQUIPMENT LIST";
        worksheet.Cell("A3").Value = "Status";
        worksheet.Cell("B3").Value = review.ReviewStatus;
        worksheet.Cell("A4").Value = "Revision";
        worksheet.Cell("B4").Value = getRevisionName(review.AboutRevision);
    }
    
    public static int AddOttrPrefix(this IXLWorksheet worksheet, int startRow)
    {
        worksheet.Cell($"A{startRow}").Value = "#OTTR";
        worksheet.Cell($"B{startRow}").Value = "prefix";
        worksheet.Cell($"A{startRow+1}").Value = "comment";
        worksheet.Cell($"B{startRow+1}").Value = "https://example.com/data/";
        worksheet.Cell($"A{startRow+2}").Value = "#OTTR";
        return startRow + 3;
    }

    public static string NextColumn(string columnName)
    {
        char lastLetter = columnName[columnName.Length - 1];
        string prefix = columnName.Substring(0, columnName.Length - 1);
        if (lastLetter == 'Z')
        {
            string newprefix = prefix.Equals("") ? "A" : NextColumn(prefix);
            return $"{newprefix}A";
        }
        else
        {
            char nextLetter = (Char)(Convert.ToUInt16(lastLetter) + 1);
            return $"{prefix}{nextLetter}";
        }
    }
    
    public static int AddSheetRow(this IXLWorksheet worksheet, int row, IEnumerable<string> cellValues)
    {
        var colName = "A";
        foreach (var value in cellValues)
        {
            worksheet.Cell($"{colName}{row}").Value = value;
            colName = NextColumn(colName);
        }

        return row + 1;
    }
    //TODO: This hardcoding of the properties labelled with the fragment is not a good long-term solution
    // But it works with TR1244 and TR1245 ontologies as they are now
    public static IEnumerable<Uri> GetAllObjectFilters(IEnumerable<CommentDto> comments) =>
        comments.SelectMany(comment =>
                comment.AboutObject
                    .Select(aboutFilter => aboutFilter.property.ToString()))
            .Distinct()
            .Select(filter => new Uri(filter));
    
    public static void CommentToExcelRow(this CommentDto commentDto, IXLWorksheet worksheet, int row,
        Uri[] filternames) =>
        worksheet.AddSheetRow(row, new string[]
            {
                commentDto.CommentId,
                commentDto.CommentText,
                commentDto.IssuedBy,
            }
            .Concat(filternames.Select(filterName =>
                commentDto.AboutObject
                    .Where(aboutFilter => aboutFilter.property.ToString().Equals(filterName.ToString()))
                .Select(aboutFilter => aboutFilter.value)
                .DefaultIfEmpty("")
                .Single()
                )
            )
            .Concat(new string[]
            {
                "", // Property is not part of the DTO yet
                "", //Contractor reply is filled by contractor
                "" //Contractor author is filled by contractor
            })
            );

    public static int AddOttrTemplateStart(this ReviewDTO review, IXLWorksheet worksheet, int startRow,
        string[] filterNames)
    {
        worksheet.AddSheetRow(startRow,
            new[] { "#OTTR", "template", "https://rdf.equinor.com/ontology/template#CommentReply" });
        var ottrArgumentIndex =
            new string[] { "1" }
                .Concat(Enumerable.Repeat("0", 3 + filterNames.Count()))
                .Concat(new string[] { "2", "3" });
        startRow = worksheet.AddSheetRow(startRow, ottrArgumentIndex);
        var ottrArgumentType =
            new string[] { "iri" }
                .Concat(Enumerable.Repeat("", 3 + filterNames.Count()))
                .Concat(new string[] { "text", "text" });
        startRow = worksheet.AddSheetRow(startRow, ottrArgumentType);
        var ottrArgumentName =
            new string[] { "Comment Id", "Equinor comment", "Equinor author" }
                .Concat(filterNames)
                .Concat(new string[] { "Property", "Contractor reply", "Contractor author" });
        return worksheet.AddSheetRow(startRow, ottrArgumentName);
        
    }
    
    public static int AddOttrTemplateEnd(this ReviewDTO review, IXLWorksheet worksheet, int startRow) =>
        worksheet.AddSheetRow(startRow,
                new[] { "#OTTR", "end" });

    
    
}