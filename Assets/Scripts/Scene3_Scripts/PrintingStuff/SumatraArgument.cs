using System.Text;

public static class SumatraArguments
{
    public static string Build
    (
        PrintDocument Document,
        Printer Printer
    )
    {
        PrintDetails Print = Document.Print;

        StringBuilder Settings = new StringBuilder();

        Settings.Append($"printer=\"{Printer.Name}\"");

        if (Print.Copies > 1)
            Settings.Append($",copies={Print.Copies}");

        if (Print.SideSelection == SideSelectionEnum.DoubleSided)
            Settings.Append(",duplex");

        if (Print.CollatedOrNot == CollatedOrNotEnum.Collated)
            Settings.Append(",collate");

        if (Print.Orientation == OrientationEnum.Landscape)
            Settings.Append(",landscape");

        if (Print.Color == ColorEnum.BlackAndWhite)
            Settings.Append(",monochrome");

        switch (Print.PagesPerSheet)
        {
            case PagesPerSheetEnum._2:
                Settings.Append(",2x");
                break;

            case PagesPerSheetEnum._4:
                Settings.Append(",4x");
                break;

            case PagesPerSheetEnum._8:
                Settings.Append(",8x");
                break;

            case PagesPerSheetEnum._16:
                Settings.Append(",16x");
                break;
        }

        return
            $"-silent " +
            $"-print-to \"{Printer.Name}\" " +
            $"-print-settings \"{Settings}\" " +
            $"\"{Print.LocalPDFPath}\"";
    }
}