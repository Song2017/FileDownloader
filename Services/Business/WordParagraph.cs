using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Linq;

namespace Services.Business
{
    public static class WordParagraph
    {
        static Text SetText(string value, SpaceProcessingModeValues modelValue = SpaceProcessingModeValues.Preserve)
        {
            return new Text(value) { Space = modelValue };
        }
        static RunProperties SetRunProperties(Bold _bold, Color _color, FontSize _fontSize)
        {
            RunProperties rpTagNumbera = new RunProperties();
            rpTagNumbera.Bold = _bold;
            rpTagNumbera.Color = _color;
            rpTagNumbera.FontSize = _fontSize;

            return rpTagNumbera;
        }
        static Run SetRun(Text _text, RunProperties _runProperties)
        {
            Run _run = new Run();
            if (null != _runProperties)
                _run.Append(_runProperties);
            _run.Append(_text);
            return _run;
        }

        static ParagraphProperties SetParagraphProperties(string style, string spacNum, JustificationValues justValue)
        {
            ParagraphProperties ppHEmpty = new ParagraphProperties();
            ppHEmpty.ParagraphStyleId = new ParagraphStyleId() { Val = style };
            ppHEmpty.SpacingBetweenLines = new SpacingBetweenLines() { After = spacNum };
            ppHEmpty.Justification = new Justification { Val = justValue };
            return ppHEmpty;
        }

        public static Paragraph GenerateParagraph(string tag, string value)
        {
            Paragraph pTagNumber = new Paragraph();
            pTagNumber.Append(
                SetRun(
                    SetText("" + tag),
                    SetRunProperties(
                        new Bold(),
                        new Color { Val = "000000" },
                        new FontSize { Val = "20" }
                        )
                    )
                  );
            pTagNumber.Append(SetRun(SetText(value), null));
            return pTagNumber;
        }

        public static Paragraph GenerateParagraphHeading(string value)
        {
            Paragraph pHEmpty = new Paragraph();
            pHEmpty.Append(SetParagraphProperties("heading1", "0", JustificationValues.Left));
            pHEmpty.Append(SetRun(SetText(value), null));
            return pHEmpty;

        }

        public static Paragraph GenerateFileTitle(string value, string fontColor = "", string fontSize = "")
        {
            Paragraph p = new Paragraph();
            p.Append(SetParagraphProperties(string.Empty, string.Empty, JustificationValues.Center));
            p.Append(
                SetRun(
                    SetText(value),
                    SetRunProperties(
                        new Bold(),
                        new Color
                        {
                            Val = string.IsNullOrEmpty(fontColor) ? "FF0000" : fontColor
                        },
                        new FontSize
                        {
                            Val = string.IsNullOrEmpty(fontSize) ? "35" : fontSize
                        })
                     )
                   );
            return p;
        }
        public static Paragraph GenerateFileSubTitle(string value, string fontSize)
        {
            Paragraph pTagNumberSub = new Paragraph();
            pTagNumberSub.Append(SetParagraphProperties(string.Empty, string.Empty, JustificationValues.Center));
            pTagNumberSub.Append(
                SetRun(
                    SetText(value),
                    SetRunProperties(null, new Color { Val = "FF0000" }, new FontSize { Val = "20" })));
            return pTagNumberSub;
        }

        public static Paragraph GenerateSummaryRepair(string effectiveDate, string maintenanceFor)
        {
            var pSummaryRepairs = new Paragraph();
            pSummaryRepairs.Append(
                SetRun(SetText("    " + effectiveDate + "       "),
                SetRunProperties(new Bold(),
                                new Color { Val = "000000" },
                                new FontSize { Val = "20" })));
            pSummaryRepairs.Append(SetRun(SetText(maintenanceFor), null));
            return pSummaryRepairs;
        }

        public static Paragraph GenerateNormalText(string tag, string value)
        {
            Paragraph pMaintFor = new Paragraph();
            pMaintFor.Append(SetRun(SetText(tag), null));
            pMaintFor.Append(SetRun(SetText(value), null));
            return pMaintFor;
        }

        public static Paragraph GeneratePartInfo(string value)
        {
            Paragraph pPartUsedDetail = new Paragraph();
            pPartUsedDetail.Append(SetRun(SetText(value), null));
            return pPartUsedDetail;
        }

        public static StyleRunProperties GenerateStyleRunProperties(string fontColor, string fontSize)
        {
            StyleRunProperties styleRunPropertiesH1 = new StyleRunProperties();
            Color color1 = new Color() { Val = fontColor };
            FontSize fontSize1 = new FontSize();
            fontSize1.Val = new StringValue(fontSize);
            styleRunPropertiesH1.Append(color1);
            styleRunPropertiesH1.Append(fontSize1);
            return styleRunPropertiesH1;
        }

        public static void AddStyleToDoc(MainDocumentPart mainPart, string styleid, string stylename, StyleRunProperties styleRunProperties)
        {

            // Get the Styles part for this document.
            StyleDefinitionsPart part =
                mainPart.StyleDefinitionsPart;

            // If the Styles part does not exist, add it and then add the style.
            if (part == null)
            {
                part = AddStylesPartToPackage(mainPart);
                AddNewStyle(part, styleid, stylename, styleRunProperties);
            }
            else
            {
                //// If the style is not in the document, add it.
                //if (IsStyleIdInDocument(mainPart, styleid) != true)
                //{
                //    // No match on styleid, so let's try style name.
                //    string styleidFromName = GetStyleIdFromStyleName(mainPart, stylename);
                //    if (styleidFromName == null)
                //    {
                //        AddNewStyle(part, styleid, stylename, styleRunProperties);
                //    }
                //    else
                //        styleid = styleidFromName;
                //}
            }

        }

        // Add a StylesDefinitionsPart to the document.  Returns a reference to it.
        public static StyleDefinitionsPart AddStylesPartToPackage(MainDocumentPart mainPart)
        {
            StyleDefinitionsPart part;
            part = mainPart.AddNewPart<StyleDefinitionsPart>();
            DocumentFormat.OpenXml.Wordprocessing.Styles root = new DocumentFormat.OpenXml.Wordprocessing.Styles();
            root.Save(part);
            return part;
        }

        public static bool IsStyleIdInDocument(MainDocumentPart mainPart, string styleid)
        {
            // Get access to the Styles element for this document.
            DocumentFormat.OpenXml.Wordprocessing.Styles s = mainPart.StyleDefinitionsPart.Styles;

            // Check that there are styles and how many.
            int n = s.Elements<DocumentFormat.OpenXml.Wordprocessing.Style>().Count();
            if (n == 0)
                return false;

            // Look for a match on styleid.
            DocumentFormat.OpenXml.Wordprocessing.Style style = s.Elements<DocumentFormat.OpenXml.Wordprocessing.Style>()
                .Where(st => (st.StyleId == styleid) && (st.Type == StyleValues.Paragraph))
                .FirstOrDefault();
            if (style == null)
                return false;

            return true;
        }

        // Create a new style with the specified styleid and stylename and add it to the specified style definitions part.
        private static void AddNewStyle(StyleDefinitionsPart styleDefinitionsPart, string styleid, string stylename, StyleRunProperties styleRunProperties)
        {
            // Get access to the root element of the styles part.
            DocumentFormat.OpenXml.Wordprocessing.Styles styles = styleDefinitionsPart.Styles;

            // Create a new paragraph style and specify some of the properties.
            DocumentFormat.OpenXml.Wordprocessing.Style style = new DocumentFormat.OpenXml.Wordprocessing.Style()
            {
                Type = StyleValues.Paragraph,
                StyleId = styleid,
                CustomStyle = true
            };
            style.Append(new StyleName() { Val = stylename });
            style.Append(new BasedOn() { Val = "Normal" });
            style.Append(new NextParagraphStyle() { Val = "Normal" });
            style.Append(new UIPriority() { Val = 900 });

            // Create the StyleRunProperties object and specify some of the run properties.


            // Add the run properties to the style.
            // --- Here we use the OuterXml. If you are using the same var twice, you will get an error. So to be sure just insert the xml and you will get through the error.
            style.Append(new StyleRunProperties() { InnerXml = styleRunProperties.OuterXml });

            // Add the style to the styles part.
            styles.Append(style);
        }
    }
}
