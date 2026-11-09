using System;
using System.Collections.Generic;
using System.Threading.Tasks;

//PDFsharp is only used if it is installed AND the scripting define symbol PRINTPAL_PDFSHARP is set
//( Project Settings -> Player -> Other Settings -> Scripting Define Symbols ).
//Without the symbol this script still compiles, IsAvailable is false, and printing works exactly like before
//( the header and the document go to the printer as 2 separate jobs ).
#if PRINTPAL_PDFSHARP
using PdfSharp;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
#endif

public static class PdfMerger
{
    //True only when PDFsharp is available ( see the note at the top ).
    public static readonly bool IsAvailable =
#if PRINTPAL_PDFSHARP
        true;
#else
        false;
#endif



    //The copies are repeated INSIDE the merged PDF ( so the header is printed once ), which makes the file bigger.
    //Documents which would need more pages than this are printed the old way ( header + document as 2 jobs ).
    const int MaxMergedPages = 300;



    //Returns "" if the header of this print can be merged into its PDF, else the reason why it can't.
    //
    //Only these settings are NOT supported, because the printer would treat the merged header like any other page:
    //a. Odd / Even pages ( Sumatra's "odd" / "even" ) would drop or keep the header by accident.
    //b. More than 1 page per sheet would shrink the header together with the document's pages.
    //c. Landscape would rotate the header page as well.
    //Copies, collation and duplex ARE supported ( see MergeAsync ).
    public static string GetMergeBlocker(PrintDetails Print)
    {
        if (!IsAvailable)
        {
            return "PDFsharp is not enabled";
        }

        if (Print.PageSelection != PageSelectionEnum.AllPages)
        {
            return "odd / even pages are selected";
        }

        if (Print.PagesPerSheet != PagesPerSheetEnum._1)
        {
            return "more than 1 page per sheet is selected";
        }

        if (Print.Orientation != OrientationEnum.Portrait)
        {
            return "landscape is selected";
        }

        int Copies = Math.Max(1, Print.Copies);

        if (Print.Pages * Copies > MaxMergedPages)
        {
            return "too many pages to merge ( " + Print.Pages + " pages x " + Copies + " copies )";
        }

        return "";
    }



    //Creates ONE PDF at OutputPDFPath =
    //  1. The header page.
    //  2. ( If DuplexPrinting ) one blank page, so that the back of the header sheet stays empty and the document
    //     starts on a new sheet.
    //  3. The document's pages, with all the copies inside:
    //       Collated   : 1 2 3, 1 2 3, 1 2 3
    //       Uncollated : 1 1 1, 2 2 2, 3 3 3
    //     In duplex printing a document with an odd number of pages gets a blank page added, so that every copy
    //     ( collated ) / every sheet ( uncollated ) starts on a new sheet.
    //
    //The PDF is meant to be printed ONCE ( Copies = 1 ) - see SumatraArguments.Build( ..., CopiesAreInPDF ).
    //
    //Runs on a background thread. The returned Task faults if anything goes wrong ( e.g. a password protected PDF ),
    //and the caller then falls back to printing the header and the document separately.
    public static Task<string> MergeAsync
    (
        string HeaderPDFPath,
        string DocumentPDFPath,
        bool DuplexPrinting,
        int Copies,
        bool Collated,
        string OutputPDFPath
    )
    {
        return Task.Run<string>(() =>
        {
#if PRINTPAL_PDFSHARP
            int CopyCount = Math.Max(1, Copies);

            PdfDocument Output = null;
            PdfDocument Header = null;

            //Every copy uses its OWN opened document, so no page object is ever added to the output twice.
            List<PdfDocument> Bodies = new List<PdfDocument>();

            try
            {
                Output = new PdfDocument();

                Header = PdfReader.Open(HeaderPDFPath, PdfDocumentOpenMode.Import);

                for (int Copy = 0; Copy < CopyCount; Copy++)
                {
                    Bodies.Add(PdfReader.Open(DocumentPDFPath, PdfDocumentOpenMode.Import));
                }

                int BodyPageCount = Bodies[0].PageCount;

                if (BodyPageCount <= 0)
                {
                    throw new Exception("The document has no pages.");
                }

                if (BodyPageCount * CopyCount > MaxMergedPages)
                {
                    throw new Exception
                    (
                        "Too many pages to merge ( " + BodyPageCount + " pages x " + CopyCount + " copies )."
                    );
                }



                //----- The header -----

                foreach (PdfPage Page in Header.Pages)
                {
                    Output.AddPage(Page);
                }

                if (DuplexPrinting)
                {
                    PdfPage Blank = Output.AddPage();

                    Blank.Size = PageSize.A4;
                }



                //----- The document, with its copies -----

                bool PadToEven = DuplexPrinting && (BodyPageCount % 2 == 1);

                if (CopyCount == 1)
                {
                    foreach (PdfPage Page in Bodies[0].Pages)
                    {
                        Output.AddPage(Page);
                    }
                }
                else if (Collated)
                {
                    for (int Copy = 0; Copy < CopyCount; Copy++)
                    {
                        for (int Index = 0; Index < BodyPageCount; Index++)
                        {
                            Output.AddPage(Bodies[Copy].Pages[Index]);
                        }

                        if (PadToEven && Copy < CopyCount - 1)
                        {
                            AddBlankPageLike(Output, Bodies[Copy].Pages[BodyPageCount - 1]);
                        }
                    }
                }
                else if (!DuplexPrinting)
                {
                    for (int Index = 0; Index < BodyPageCount; Index++)
                    {
                        for (int Copy = 0; Copy < CopyCount; Copy++)
                        {
                            Output.AddPage(Bodies[Copy].Pages[Index]);
                        }
                    }
                }
                else
                {
                    //Uncollated duplex: every SHEET ( 2 pages ) is repeated for each copy.
                    for (int Index = 0; Index < BodyPageCount; Index += 2)
                    {
                        for (int Copy = 0; Copy < CopyCount; Copy++)
                        {
                            Output.AddPage(Bodies[Copy].Pages[Index]);

                            if (Index + 1 < BodyPageCount)
                            {
                                Output.AddPage(Bodies[Copy].Pages[Index + 1]);
                            }
                            else
                            {
                                AddBlankPageLike(Output, Bodies[Copy].Pages[Index]);
                            }
                        }
                    }
                }



                Output.Save(OutputPDFPath);
            }
            finally
            {
                //The source documents stay open until the merged PDF has been saved.
                foreach (PdfDocument Body in Bodies)
                {
                    Body.Dispose();
                }

                if (Header != null)
                {
                    Header.Dispose();
                }

                if (Output != null)
                {
                    Output.Dispose();
                }
            }

            return OutputPDFPath;
#else
            throw new NotSupportedException
            (
                "PDFsharp is not enabled ( PRINTPAL_PDFSHARP is not defined )."
            );
#endif
        });
    }



#if PRINTPAL_PDFSHARP
    //Adds an empty page of the same size as Reference.
    private static void AddBlankPageLike(PdfDocument Output, PdfPage Reference)
    {
        PdfPage Blank = Output.AddPage();

        Blank.Width = Reference.Width;
        Blank.Height = Reference.Height;
    }
#endif
}