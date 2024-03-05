using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office2010.Excel;
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
        // TODO Refactor to use a single-element dictionary hardcoded 
        // SOmething like new Dictionary<Uri, string>(){"https://...." => "comment"}
        var prefixes = new Dictionary<Uri, string>() { };
        int nextFreeRow = worksheet.AddOttrPrefix(9, prefixes);
        review.GenerateCommentRows(worksheet, nextFreeRow, getUriLabel, prefixes);
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
        worksheet.Cell("A1").Value = review.Label;
        worksheet.Cell("A2").Value = "MASTER EQUIPMENT LIST";
        worksheet.Cell("A3").Value = "Status";
        worksheet.Cell("B3").Value = review.ReviewStatus;
        worksheet.Cell("A4").Value = "Revision";
        worksheet.Cell("B4").Value = getRevisionName(review.AboutRevision);
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
    // TODO: Not used after hardcoded comment prefix
    public static string getPrefix(Uri uri)
    {
        if (uri.Fragment.Equals(""))
            return string.Join("", uri.ToString().Split("/").SkipLast(1));
        return uri.GetLeftPart(UriPartial.Path);
    }
    
    // TODO: Move this to Test program, since only used for testing
    public static string getSuffix(Uri uri)
    {
        if (uri.Fragment.Equals(""))
            return uri.ToString().Split("/").Last();
        return uri.Fragment.Substring(1);
    }
    // TODO: Remove since only used by unnecessary prefix getter
    public static string getCommentQName(this CommentDto comment, IDictionary<Uri, string> prefixes)
    {
        var commentUri = new Uri(comment.CommentId);
        var commentUriLeftPart = new Uri(getPrefix(commentUri));
        var prefix = prefixes[commentUriLeftPart];
        return $"{prefix}:{getSuffix(commentUri)}";
    }

    // TODO: This is unnecessary since comment id is a guid
    public static IDictionary<Uri, string> GetCommentIdPrefixes(this ReviewDTO review)
    {

        var namespaces = review.GetCommentIdNamespaces().ToArray();
        return namespaces
            .Zip(Enumerable.Range(1, namespaces.Length))
            .ToDictionary(
                x => x.First, 
                x => $"comment{x.Second}");
    }

    public static IEnumerable<Uri> GetCommentIdNamespaces(this ReviewDTO review) =>
        review.HasComments
            .Select(comment => getPrefix(new Uri(comment.CommentId)))
            .Distinct()
            .Select(uriString => new Uri(uriString));
        
    
    
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
        worksheet.Rows(startRow, currentRow).Hide();
        return worksheet.AddSheetRow(currentRow, ottrArgumentName);
        
    }
    
    public static int AddOttrTemplateEnd(this ReviewDTO review, IXLWorksheet worksheet, int startRow) =>
        worksheet.AddSheetRow(startRow,
                new[] { "#OTTR", "end" });

    
    
}