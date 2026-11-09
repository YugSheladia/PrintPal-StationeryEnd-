using System.Collections.Generic;
using System.Text;

public static class SumatraArguments
{
    //PDFPathOverride is optional: when it is given, that PDF is printed instead of Print.LocalPDFPath
    //( used for the document's PDF with the header merged into it ).
    //
    //CopiesAreInPDF = true means the copies ( and their collation ) are already inside that PDF, so Sumatra must print
    //it ONCE - otherwise the header would be printed once per copy, and the copies would be multiplied again.
    //Everything else is unchanged.
    public static string Build
    (
        PrintDocument Document,
        Printer Printer,
        string PDFPathOverride = null,
        bool CopiesAreInPDF = false
    )
    {
        PrintDetails Print = Document.Print;

        List<string> Settings = new List<string>();




        // Copies ( Works )
        if (Print.Copies > 1 && !CopiesAreInPDF)
            Settings.Add($"{Print.Copies}x");



        // Duplex
        if (Print.SideSelection == SideSelectionEnum.DoubleSided)
            Settings.Add("duplex");



        // Collation
        if (Print.CollatedOrNot == CollatedOrNotEnum.Collated && !CopiesAreInPDF)
            Settings.Add("collate");



        // Orientation ( Works )
        if (Print.Orientation == OrientationEnum.Landscape)
            Settings.Add("landscape");



        // Color
        if (Print.Color == ColorEnum.BlackAndWhite)
            Settings.Add("monochrome");




        // Pages Per Sheet ( Doesn't work )
        switch (Print.PagesPerSheet)
        {
            case PagesPerSheetEnum._2:
                Settings.Add("2x");
                break;

            case PagesPerSheetEnum._4:
                Settings.Add("4x");
                break;

            case PagesPerSheetEnum._8:
                Settings.Add("8x");
                break;

            case PagesPerSheetEnum._16:
                Settings.Add("16x");
                break;
        }



        // Page Selection (if supported by Sumatra)
        switch (Print.PageSelection)
        {
            case PageSelectionEnum.OddPages:
                Settings.Add("odd");
                break;

            case PageSelectionEnum.EvenPages:
                Settings.Add("even");
                break;
        }




        string PrintSettings = string.Join(",", Settings);

        StringBuilder Arguments = new StringBuilder();

        Arguments.Append("-silent ");
        Arguments.Append($"-print-to \"{Printer.Name}\" ");

        if (!string.IsNullOrEmpty(PrintSettings))
        {
            Arguments.Append($"-print-settings \"{PrintSettings}\" ");
        }

        string PDFPath =
            string.IsNullOrEmpty(PDFPathOverride)
                ? Print.LocalPDFPath
                : PDFPathOverride;

        Arguments.Append($"\"{PDFPath}\"");

        return Arguments.ToString();
    }
}