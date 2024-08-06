using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using VDS.RDF.Query.Algebra;
using static System.Net.WebRequestMethods;

namespace Review.Tests
{
    public static class ExcelParser
    {
        public static ReviewDTO ParseExcelToReviewDTO(string excelFilePath)
        {
            using var workbook = new XLWorkbook(excelFilePath);
            var worksheet = workbook.Worksheet("Review Comments");

            // Create a new ReviewDTO object
            var reviewDto = new ReviewDTO
            {
                // Assuming ReviewId would be a part of the Excel file, if not, generate or fetch from another source.
                ReviewIri = "https://example.com/doc/reply-" + GetCellValue(worksheet, "B4"),
                AboutRevision = new Uri("https://example.com/data/" + GetCellValue(worksheet, "B4")),
                IssuedBy = GetCellValue(worksheet, "B5"),
                Status = GetReviewStatusFromDescription(GetCellValue(worksheet, "B3")),
                Label = GetCellValue(worksheet, "A1"),
                HasComments = new List<CommentDto>()
            };

            // Loop through rows to get CommentDto objects
            var currentRow = 16; // Assuming data starts from the 9th row
            var filterStartColumnIndex = 4;
            while (true)
            {
                var idCell = worksheet.Cell($"A{currentRow}");
                var text = idCell.GetText();
                if (idCell == null || string.IsNullOrEmpty(idCell.Value.ToString()) || idCell.Value.ToString() == "#OTTR")
                    break;

                var commentDto = new CommentDto
                {
                    // Parse the comment ID from the QName, assuming the ID is after the last colon
                    CommentId = Guid.Parse(idCell.Value.ToString().Split(':').Last()),
                    CommentText = GetCellValue(worksheet, $"B{currentRow}"),
                    IssuedBy = GetCellValue(worksheet, $"C{currentRow}"),
                    AboutObject = []
                };
                // Retrieve filter values from the row
                for (var columnIndex = filterStartColumnIndex; columnIndex < worksheet.ColumnsUsed().Count(); columnIndex++)
                {
                    var propertyCell = worksheet.Cell(15, columnIndex); // Assuming property URIs are in the 8th row
                    var valueCell = worksheet.Cell(currentRow, columnIndex);
                    if (propertyCell != null && valueCell.Value.ToString() != "")
                    {
                        var filter = new AboutObjectFilter
                        {
                            Property = new Uri("https://rdf.equinor.com/ontology/mel/v1#" + propertyCell.Value.ToString()),
                            Value = valueCell.Value.ToString()
                        };
                        commentDto.AboutObject.Add(new PropertyValuePair { Property = filter.Property, Value = filter.Value });
                    }
                }

                reviewDto.HasComments.Add(commentDto);
                currentRow++;
            }

            return reviewDto;
        }

        private static string GetCellValue(IXLWorksheet worksheet, string cellAddress)
        {
            var value = worksheet.Cell(cellAddress).Value.ToString().Trim();
            return value;
        }

        private static ReviewStatus GetReviewStatusFromDescription(string description)
        {
            // Parse the review status code from the description text
            switch (description)
            {
                case "Code 1: Accepted":
                    return ReviewStatus.Code1;
                case "Code 2: Minor Changes Needed":
                    return ReviewStatus.Code2;
                case "Code 3: Major Changes Needed":
                    return ReviewStatus.Code3;
                case "Code 4: Redesign Required":
                    return ReviewStatus.Code4;
                default:
                    return ReviewStatus.Code5;
            }
        }

    }
    public class AboutObjectFilter
    {
        public Uri Property { get; set; }
        public string Value { get; set; }
    }

}