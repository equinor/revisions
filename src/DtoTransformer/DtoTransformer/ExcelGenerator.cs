using ClosedXML.Excel;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using VDS.RDF;

namespace Review;

public static class ExcelGenerator
{
    // Naive implementation of revision name getter
    public static string GetRevisionName(Uri revisionUri) =>
        revisionUri.LocalPath.ToString();

    
    public static void CreateExcelAt(ReviewDTO review, string path)
    {
        review.GenerateExcel(getSuffix).SaveAs(path);
    }
    
    public static XLWorkbook GenerateExcel(this ReviewDTO review, Func<Uri, string> getUriLabel)
    {
        var workbook = new XLWorkbook();
        var worksheet = workbook.AddWorksheet("Review Comments");

        review.AddReviewHeader(worksheet, getUriLabel);
        var prefixes = new Dictionary<Uri, string>()
        {
            { new Uri("https://example.com/data/"), "comment" }
        };


        int nextFreeRow = worksheet.AddOttrPrefix(9, prefixes);
        review.GenerateCommentRows(worksheet, nextFreeRow, getUriLabel, prefixes);
                worksheet.Columns().AdjustToContents(); // This will adjust all columns
        worksheet.Rows().AdjustToContents();
        return workbook;
    }

    public static int GenerateCommentRows(this ReviewDTO review, IXLWorksheet worksheet, int startRow,
        Func<Uri, string> propertyLabelGetter, IDictionary<Uri, string> prefixes)
    {
        var filterUriss = GetAllObjectFilters(review.HasComments)
            .ToArray();
        var filterNames = filterUriss.Select(propertyLabelGetter).ToArray();
        var commentRow = review.AddOttrTemplateStart(worksheet, startRow, filterNames);
        foreach (var comment in review.HasComments)
        {
            comment.CommentToExcelRow(worksheet, commentRow, filterUriss, prefixes);
            commentRow++;
        }
        return review.AddOttrTemplateEnd(worksheet, commentRow);
    }
    
    public static void AddReviewHeader(this ReviewDTO review, IXLWorksheet worksheet, Func<Uri, string> getRevisionName)
    {
        var reviewHeaders = new Dictionary<string, string>
        {
            ["A1"] = review.Label,
            ["A2"] = "MASTER EQUIPMENT LIST",
            ["A3"] = "Status",
            ["B3"] = review.ReviewStatus,
            ["A4"] = "Revision",
            ["B4"] = getRevisionName(review.AboutRevision),
            ["A5"] = "Comments responsible",
            ["B5"] = review.IssuedBy
        };

        foreach (var header in reviewHeaders)
        {
            var cell = worksheet.Cell(header.Key);
            cell.Value = header.Value;
            if (header.Key.StartsWith("A"))
            {
                cell.Style.Font.SetBold();
            }
        }

    }
    // Simplify since only one prefx, or just 
    public static int AddOttrPrefix(this IXLWorksheet worksheet, int startRow, IDictionary<Uri, string> prefixes)
    {
        var currentRow = startRow;
        worksheet.Cell($"A{currentRow}").Value = "#OTTR";
        worksheet.Cell($"B{currentRow}").Value = "prefix";
        currentRow++;
        foreach (var (prefixUri, prefixNamekeypair) in prefixes)
        {
            currentRow = worksheet.AddSheetRow(currentRow, new string[] { prefixNamekeypair, prefixUri.ToString() });
        }
        worksheet.Rows(startRow, currentRow).Hide();
        return worksheet.AddSheetRow(currentRow, new string[]{"#OTTR", "end"});
    }

    public static string NextColumn(string columnName)
    {
        char lastLetter = columnName[^1];
        string prefix = columnName[..^1];
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
    
    
    // TODO: Move this to Test program, since only used for testing
    public static string getSuffix(Uri uri)
    {
        if (uri.Fragment.Equals(""))
            return uri.ToString().Split("/").Last();
        var value = uri.Fragment.Substring(1);
        return uri.Fragment.Substring(1);
    }
  

    // Fetches all IRIs used as object filters in aboutObject
    public static IEnumerable<Uri> GetAllObjectFilters(IEnumerable<CommentDto> comments) =>
        comments.SelectMany(comment =>
                comment.AboutObject
                    .Select(aboutFilter => aboutFilter.property.ToString()))
            .Distinct()
            .Select(filter => new Uri(filter));
    
    public static void CommentToExcelRow(this CommentDto commentDto, IXLWorksheet worksheet, int row,
        Uri[] filternames, IDictionary<Uri, string> prefixes) =>
        worksheet.AddSheetRow(row, new string[]
            {
                // TODO Use new hardcoded prefix for comment
                //commentDto.getCommentQName(prefixes),
                commentDto.CommentId.ToString(),
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
        var currentRow = startRow;
        currentRow = worksheet.AddSheetRow(currentRow,
            new[] { "#OTTR", "template", "https://rdf.equinor.com/ontology/template#CommentReply" });
        var ottrArgumentIndex =
            new string[] { "1" }
                .Concat(Enumerable.Repeat("0", 3 + filterNames.Count()))
                .Concat(new string[] { "2", "3" });
        currentRow = worksheet.AddSheetRow(currentRow, ottrArgumentIndex);
        var ottrArgumentType =
            new string[] { "iri" }
                .Concat(Enumerable.Repeat("", 3 + filterNames.Count()))
                .Concat(new string[] { "text", "text" });
        currentRow = worksheet.AddSheetRow(currentRow, ottrArgumentType);
        var ottrArgumentName =
            new string[] { "Comment Id", "Equinor comment", "Equinor author" }
                .Concat(filterNames)
                .Concat(new string[] { "Property", "Contractor reply", "Contractor author" });
        worksheet.Row(currentRow).Style.Font.SetBold();
        worksheet.Rows(startRow, currentRow-1).Hide();

        return worksheet.AddSheetRow(currentRow, ottrArgumentName);
        
    }
    
    public static int AddOttrTemplateEnd(this ReviewDTO review, IXLWorksheet worksheet, int startRow) { 
        var row = worksheet.AddSheetRow(startRow, new[] { "#OTTR", "end" });
        worksheet.Row(row-1).Hide();
        return row;
    }

}