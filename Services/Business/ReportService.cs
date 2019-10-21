using Common;
using Common.Utils;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace Services.Business
{
    public class ReportService : IReportService
    {
        private readonly ILogger _logger;
        public ReportService(ILoggerFactory logger)
        {
            _logger = logger.CreateLogger<ReportService>();
        }

        public bool CreateWordFile(Data valve, string fileName, IStringLocalizer localizer, string directory = "")
        {
            fileName = Path.Combine(Environment.CurrentDirectory,
                AppConstants.DownloadFolderName, directory, fileName);
            try
            {
                var pTotal = new List<OpenXmlElement>
                {
                    //head tag number   
                    WordParagraph.GenerateFileTitle(valve.TagNumber),
                    // head sub-title   "Valve Maintenance/Repair History"
                    WordParagraph.GenerateFileSubTitle(localizer["FileSubTitle"], "35"),
                    WordParagraph.GenerateParagraphHeading("  "),
                    //Title ->Valve information:
                    WordParagraph.GenerateParagraphHeading(localizer["FileValveInfoTitle"]),
                    //begin: valve information tagnumber
                    WordParagraph.GenerateParagraph(localizer["FileValveInfoTag"].ToString().PadRight(30, ' '),
                        valve.TagNumber),
                    //begin: valve information - serial number
                    WordParagraph.GenerateParagraph(localizer["FileValveInfoSerialNumber"].ToString().PadRight(20, ' '),
                        valve.SerialNumber),
                    //begin: valve information - Owner Name
                    WordParagraph.GenerateParagraph(localizer["FileValveInfoOwnerName"].ToString().PadRight(19, ' '),
                        valve.SerialNumber),
                    //begin: valve information - Plant Location
                    WordParagraph.GenerateParagraph(
                        localizer["FileValveInfoPlantLocation"].ToString().PadRight(22, ' '), valve.PlantLocation),
                    WordParagraph.GenerateNormalText(string.Empty, string.Empty),
                    //Create your heading2 into docx
                    WordParagraph.GenerateParagraphHeading(localizer["FileSummaryRepairTitle"])
                };

                //var pSummaryRepairs = new Paragraph(); //datetesed maintenance for
                var pSummaryRepairss = new List<OpenXmlElement>(); //total of above repair
                var pDetailedRepairs = new List<OpenXmlElement>(); //effective date title

                foreach (var v in valve.Repairs)
                {

                    pSummaryRepairss.Add(WordParagraph.GenerateParagraph(v.EffectiveDate.PadRight(16, ' '),
                        v.MaintenanceFor));
                    pDetailedRepairs.Add(WordParagraph.GenerateParagraphHeading(v.EffectiveDate));
                    pDetailedRepairs.Add(WordParagraph.GenerateNormalText(
                        (localizer["FileDetailRepairMaintenanceFor"].ToString().PadRight(25, ' ')).PadLeft(10, ' '),
                        v.MaintenanceFor));

                    pDetailedRepairs.Add(WordParagraph.GenerateNormalText(localizer["FileDetailRepairPartsUsed"],
                        string.Empty));
                    if (v.Parts == null)
                    {
                        pDetailedRepairs.Add(WordParagraph.GenerateNormalText(string.Empty, string.Empty));
                        continue;
                    }

                    foreach (var ps in v.Parts)
                    {
                        pDetailedRepairs.Add(
                            WordParagraph.GeneratePartInfo(
                                ps.PartQuantity.PadLeft(10, ' ') + "," + ps.PartNumber.PadLeft(10, ' ') + "," +
                                ps.PartName.PadLeft(10, ' ') + "," + ps.WorkPerformed.PadLeft(10, ' ')));
                    }

                    pDetailedRepairs.Add(WordParagraph.GenerateNormalText(string.Empty, string.Empty));
                }

                pSummaryRepairss.Add(WordParagraph.GenerateNormalText(string.Empty, string.Empty));
                pSummaryRepairss.ForEach(x => pTotal.Add(x));
                pDetailedRepairs.ForEach(x => pTotal.Add(x));

                var document =
                    WordprocessingDocument.Create(fileName, WordprocessingDocumentType.Document);
                var mainPart = document.AddMainDocumentPart();
                mainPart.Document = new Document();
                WordParagraph.AddStyleToDoc(mainPart, "heading1", "heading 1",
                    WordParagraph.GenerateStyleRunProperties("2F5496", "35"));
                WordParagraph.AddStyleToDoc(mainPart, "heading2", "heading 2",
                    WordParagraph.GenerateStyleRunProperties("2F5496", "35"));
                var docBody = new Body();
                docBody.Append(pTotal.ToArray());
                mainPart.Document.Append(docBody);
                document.Save();
                document.Close();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Get Valve Data Error: {ex}");
                return false;
            }
        }

        public string CreateWordFiles(List<Data> valves, IStringLocalizer localizer)
        {
            string fileName;

            if (valves.Count <= 0)
                return null;

            if (valves.Count == 1)
            {
                // generate word file
                var valve = valves[0];
                fileName = string.Format(AppConstants.NameTemplateDoc, valve.TagNumber, valve.SerialNumber,
                    DateTime.UtcNow.ToString(Constants.FileNameDateFormat));

                if (CreateWordFile(valve, fileName, localizer))
                    return fileName;
            }
            else
            {

                var tmpDirectory = Guid.NewGuid().ToStringEx();
                if (!Utils.CreateDirectory(tmpDirectory))
                    return null;

                // generate word file in temp directory
                foreach (var valve in valves)
                {
                    var fname = string.Format(AppConstants.NameTemplateDoc, valve.TagNumber, valve.SerialNumber,
                        DateTime.UtcNow.ToString(Constants.FileNameDateFormat));
                    if (!CreateWordFile(valve, fname, localizer, tmpDirectory))
                        return null;
                }

                fileName = string.Format(AppConstants.NameTemplateZip, valves[0].TagNumber, valves[0].SerialNumber,
                    DateTime.UtcNow.ToString(Constants.FileNameDateFormat));

                return Utils.CreateZipFile(tmpDirectory, ref fileName) ? fileName : null;
            }

            return fileName;

        }
    }
}
