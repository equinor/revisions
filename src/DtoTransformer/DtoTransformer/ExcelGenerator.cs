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
        worksheet.Protect();
        return workbook;
    }

    public static int GenerateCommentRows(this ReviewDTO review, IXLWorksheet worksheet, int startRow,
        Func<Uri, string> propertyLabelGetter, IDictionary<Uri, string> prefixes)
    {
        var filterUris = GetAllObjectFilters(review.HasComments)
            .ToArray();
        var filterNames = filterUris.Select(propertyLabelGetter).ToArray();
        var commentRow = review.AddOttrTemplateStart(worksheet, startRow, filterNames);
        foreach (var comment in review.HasComments)
        {
            comment.CommentToExcelRow(worksheet, commentRow, filterUris, prefixes);
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
    
    public static string getSuffix(Uri uri)
    {
        if (uri.Fragment.Equals(""))
            return uri.ToString().Split("/").Last();
        return uri.Fragment.Substring(1);
    }

    public static string getCommentQName(this CommentDto comment, IDictionary<Uri, string> prefixes)
    {
        //Returns a string on the form comment:[GUID]
        var guid = comment.CommentId.ToString();
        var prefix = prefixes.Single();
        return $"{prefix.Value}:{guid}";
    }


    // Fetches all IRIs used as object filters in aboutObject
    public static IEnumerable<Uri> GetAllObjectFilters(IEnumerable<CommentDto> comments) =>
        comments.SelectMany(comment =>
                comment.AboutObject
                    .Select(aboutFilter => aboutFilter.property.ToString()))
            .Distinct()
            .Select(filter => new Uri(filter));

    public static void CommentToExcelRow(this CommentDto commentDto, IXLWorksheet worksheet, int row,
    Uri[] filternames, IDictionary<Uri, string> prefixes)
    {
        // Define column indexes based on your sheet layout
        const int commentTextColumnIndex = 2;

        // Calculate the indexes for contractor reply and author by offsetting from the number of filter names
        int columnOffset = filternames.Length + 4; // 4 for the initial columns (QName, CommentText, IssuedBy) and Property
        int contractorReplyColumnIndex = columnOffset + 1;
        int contractorAuthorColumnIndex = contractorReplyColumnIndex + 1;

        // Create the row data
        var rowData = new string[]
        {
        commentDto.getCommentQName(prefixes),
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
        "", // Contractor reply is filled by contractor
        "" // Contractor author is filled by contractor
        })
        .ToArray();

        // Add the row data to the worksheet

        worksheet.AddSheetRow(row, rowData);

        //worksheet.Cell(row, commentTextColumnIndex).Style.Alignment.WrapText = true;

        // Unlock the cells for contractor reply and author
        worksheet.Cell(row, contractorReplyColumnIndex).Style.Protection.SetLocked(false);
        worksheet.Cell(row, contractorAuthorColumnIndex).Style.Protection.SetLocked(false);

       foreach (var column in worksheet.ColumnsUsed())
        {
            //if (column.ColumnNumber() != commentTextColumnIndex)
            {
                column.AdjustToContents();
            }
        }
    }

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