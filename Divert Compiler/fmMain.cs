using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Gavaghan.Geodesy;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using PdfSharp.Pdf.IO;
using System.IO;

namespace Divert_Compiler
{
    public partial class fmMain : Form
    {
        public fmMain()
        {
            InitializeComponent();
        }

        List<AirfieldInformation> airfieldInfo;
        Importer import = new Importer();
        string dafifFileLocation = @"D:\Desktop\DAFIF 1805";
        string flipFileLocation = @"D:\Desktop\FLIP 1806";

        private void fmMain_Load(object sender, EventArgs e)
        {
            airfieldInfo = import.fromDAFIF(dafifFileLocation);
        }
    
        private void btnFind_Click(object sender, EventArgs e)
        {
            // Store user inputs
            int maxRadius = (int)numRadius.Value;
            int minRWY = (int)numRWYmin.Value;
            int minPCN = (int)numPCNmin.Value;
            string[] mustHaveFields = txtIncludeFields.Text.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            // Find home field
            AirfieldInformation home = airfieldInfo.Find(x => x.Airfield.ICAO == txtHome.Text);
            // instantiate the calculator
            GeodeticCalculator geoCalc = new GeodeticCalculator();
            // select a reference elllipsoid
            Ellipsoid reference = Ellipsoid.WGS84;
            foreach (AirfieldInformation alt in airfieldInfo)
            {
                // calculate the geodetic measurement
                GeodeticMeasurement geoMeasurement;
                geoMeasurement = geoCalc.CalculateGeodeticMeasurement(reference, home.Airfield.Position, alt.Airfield.Position);
                alt.Airfield.RangeFromHome = geoMeasurement.PointToPointDistance * 0.000539957;
                alt.Airfield.RadialFromHome = geoMeasurement.Azimuth.Degrees - home.Airfield.MagneticDeclination.Degrees;
            }
            airfieldInfo.Sort((x, y) => x.Airfield.RangeFromHome.CompareTo(y.Airfield.RangeFromHome));

            //Get suitable diverts
            List<AirfieldInformation> suitableDiverts = new List<AirfieldInformation>();
            for (int z = 0; z < airfieldInfo.Count; z++)
            {
                //Field is suitable until proven otherwise
                bool suitField = true;

                if (airfieldInfo[z].Airfield.RangeFromHome > maxRadius) { suitField = false; }
                //Runway conditions
                bool suitRWY = false;
                foreach (Runway r in airfieldInfo[z].Runways)
                {
                    if(r.Length >= minRWY && (r.PCN >= minPCN || r.PCN == -1))
                    {
                        suitRWY = true;
                    }
                }
                if(!suitRWY) { suitField = false; }
                //More options later


                if(mustHaveFields.Contains(airfieldInfo[z].Airfield.ICAO)) { suitField = true; }
                //Add to suitable fields as required
                if(suitField)
                {
                    suitableDiverts.Add(airfieldInfo[z]);
                }
            }
            suitableDiverts = import.addSuppAndTerminals(suitableDiverts, flipFileLocation);

            //Create pdf
            createDivertPDF(suitableDiverts);
        }

        private void createDivertPDF(List<AirfieldInformation> suitableDiverts)
        {
            //Setup up fonts, brushes, etc
            const string facename = "Microsoft Sans Serif";
            XPdfFontOptions options = new XPdfFontOptions(PdfFontEncoding.WinAnsi, PdfFontEmbedding.Default);
            XFont[] fonts = new XFont[] { new XFont(facename, 6.8, XFontStyle.Regular, options),
                                              new XFont(facename, 6.8, XFontStyle.Bold, options),
                                              new XFont(facename, 6.8, XFontStyle.Italic, options)
                                              };
            XBrush brush = XBrushes.Black;
            XStringFormat format = new XStringFormat();
            format.Alignment = XStringAlignment.Near;
            //Create variables for drawwing
            double lineSpace;
            XRect rect = new XRect(0, 0, 0, 0);
            double topOfWriting, bottomOfWriting, leftOfWriting, columnSpacing;
            double x, y;

            //Create the PDF document
            PdfDocument doc = new PdfDocument();
            //Add the first page
            PdfPage p = new PdfPage();
            p.Size = PdfSharp.PageSize.A4;
            p.Orientation = PdfSharp.PageOrientation.Portrait;
            doc.AddPage(p);

            //add diverts bookmark
            int pageNo = 0;
            PdfOutline.PdfOutlineCollection outlines = doc.Outlines;
            outlines.Add("Divert Data", doc.Pages[pageNo]);

            XGraphics xgr = XGraphics.FromPdfPage(p);

            topOfWriting = p.Height * 0.02;
            bottomOfWriting = p.Height * 0.97;
            leftOfWriting = p.Width * 0.02;
            columnSpacing = p.Width * 0.90 * 0.26;
            y = topOfWriting;
            x = leftOfWriting;
            lineSpace = fonts[0].GetHeight(xgr);

            //Loop through and add each of the diverts
            for (int d = 0; d < suitableDiverts.Count; d++)
            {
                //Setup entry for printing
                string[][] printData = new string[][] { fieldPrint(suitableDiverts[d].Airfield), runwayPrint(suitableDiverts[d].Runways), navPrint(suitableDiverts[d].NavigationAids), commsPrint(suitableDiverts[d].Communications) };
                int maxDataLength = printData.Max(l => l.Length);
                int remainingRoom = Convert.ToInt32((bottomOfWriting - y) / lineSpace);
                //If entry won't fit on remainder of page, create a new page
                if (maxDataLength > remainingRoom)
                {
                    //Add function to prevent the same code appearing twice
                    p = new PdfPage();
                    p.Size = PdfSharp.PageSize.A4;
                    p.Orientation = PdfSharp.PageOrientation.Portrait;
                    doc.AddPage(p);

                    pageNo++;

                    xgr = XGraphics.FromPdfPage(p);

                    topOfWriting = p.Height * 0.03;
                    bottomOfWriting = p.Height * 0.97;
                    leftOfWriting = p.Width * 0.03;
                    columnSpacing = p.Width * 0.90 * 0.26;
                    y = topOfWriting;
                    x = leftOfWriting;
                }
                for (int a = 0; a < maxDataLength; a++)
                {
                    for (int b = 0; b < printData.Length; b++)
                    {
                        if (a < printData[b].Length)
                        {
                            //Improve the printing to remove this section
                            XFont f = fonts[0];
                            if (a == 0 && b == 0) { f = fonts[1]; }
                            if (a == 1 && b == 0) { f = fonts[2]; }
                            if (a % 2 == 1 && b == 2) { f = fonts[2]; }
                            //Improve!!!^^^
                            x = leftOfWriting + columnSpacing * b;
                            rect = new XRect(x, y, columnSpacing, lineSpace);
                            xgr.DrawString(printData[b][a], f, brush, rect, format);
                        }
                    }
                    y += lineSpace;
                }
                y += lineSpace;
            }

            //Get common divert plates
            List<Plate> commonPlates = new List<Plate>();

            foreach (AirfieldInformation div in suitableDiverts)
            {
                for (int f = 0; f < div.Plates.Count; f++)
                {
                    if (div.Plates[f].Type == "Minimums" && !div.Plates[f].Name.Contains(div.Airfield.Name))
                    {
                        if (!commonPlates.Exists(a => a.Location == div.Plates[f].Location))
                        {
                            commonPlates.Add(div.Plates[f]);
                        }
                        div.Plates.Remove(div.Plates[f]);
                        f--;
                    }
                }
            }
            commonPlates.Sort((a, b) => a.Name.CompareTo(b.Name));
            // Add common plates
            outlines = doc.Outlines;
            for (int f = 0; f < commonPlates.Count; f++)
            {
                PdfDocument inputDocument = CompatiblePdfReader.Open(commonPlates[f].Location);
                for (int k = 0; k < inputDocument.PageCount; k++)
                {
                    doc.AddPage(inputDocument.Pages[k]);

                    //bookmarks
                    pageNo++;
                    if (k == 0)
                    {
                        if (f == 0)
                        {
                            outlines.Add("Minimums", doc.Pages[pageNo]);
                            outlines = outlines[outlines.Count - 1].Outlines;
                        }
                        outlines.Add(commonPlates[f].Name, doc.Pages[pageNo]);
                    }
                }
            }


            //Add divert plates
            for (int d = 0; d < suitableDiverts.Count; d++)
            {
                
                for (int f = 0; f < suitableDiverts[d].Plates.Count; f++)
                {
                    outlines = doc.Outlines;
                    PdfDocument inputDocument = CompatiblePdfReader.Open(suitableDiverts[d].Plates[f].Location);
                    for (int k = 0; k < inputDocument.PageCount; k++)
                    {
                        doc.AddPage(inputDocument.Pages[k]);

                        //bookmarks
                        pageNo++;
                        if (k == 0)
                        {
                            if (f == 0)
                            {
                                outlines.Add(suitableDiverts[d].Airfield.Name, doc.Pages[pageNo]);
                                
                            }
                            outlines = outlines[outlines.Count - 1].Outlines;
                            string typ = suitableDiverts[d].Plates[f].Type;
                            if (f != 0 && !(typ == "Supplement" || typ == "Airport Diagram" || typ == "LAHSO" || typ == "Hotspot"))
                            {
                                if (typ != suitableDiverts[d].Plates[f - 1].Type)
                                {
                                    outlines.Add(typ, doc.Pages[pageNo]);                               
                                }
                                outlines = outlines[outlines.Count - 1].Outlines;
                            }
                            outlines.Add(suitableDiverts[d].Plates[f].Name, doc.Pages[pageNo]);
                        }
                    }
                }
            }

            doc.Save(@"C:\Users\Mike\Desktop\Output.pdf");
            doc.Close();
        }

        private string[] fieldPrint(Field inField)
        {
            string[] outFieldStrings = new string[4];
            outFieldStrings[0] = inField.ICAO;
            if(inField.SecICAO.Length > 0) { outFieldStrings[0] += "/" + inField.SecICAO; }
            outFieldStrings[0] += " - " + inField.Name;
            outFieldStrings[1] = doubleToStringDMS(inField.Position.Latitude.Degrees, true) + " " + doubleToStringDMS(inField.Position.Longitude.Degrees, false);
            outFieldStrings[2] = Convert.ToInt32(inField.Position.Elevation / .3048).ToString() + "ft";
            outFieldStrings[3] = "";
            if (inField.RadialFromHome > 0)
            {
                outFieldStrings[3] = Convert.ToInt32(Math.Round(inField.RadialFromHome, 0)).ToString("D3") + "\u00B0" + "/" + Convert.ToInt32(Math.Ceiling(inField.RangeFromHome)).ToString() + "NM";
            }

            return outFieldStrings;
        }
        private string[] runwayPrint(List<Runway> inRwy)
        {
            string[] outRunwayStrings = new string[inRwy.Count];
            for (int f = 0; f < inRwy.Count; f++)
            {
                string strPCN = inRwy[f].PCN.ToString();
                if(strPCN == "-1") { strPCN = ""; }
                outRunwayStrings[f] = inRwy[f].LowIdent + "/" + inRwy[f].HighIdent + " " + inRwy[f].Length.ToString() + "' x " + inRwy[f].Width.ToString() + "' " + strPCN + inRwy[f].Surface;
            }

            return outRunwayStrings;
        }
        private string[] navPrint(List<NavigationAid> inNav)
        {
            string[] outNavStrings = new string[inNav.Count * 2];
            for (int f = 0; f < inNav.Count; f++)
            {
                outNavStrings[2 * f] = inNav[f].Ident + " " + inNav[f].Runway + " " + inNav[f].Type + " " + inNav[f].Name + " " + readableFrequency(inNav[f].Frequency, inNav[f].Channel);
                outNavStrings[2 * f + 1] = doubleToStringDMS(inNav[f].Position.Latitude.Degrees, true) + " " + doubleToStringDMS(inNav[f].Position.Longitude.Degrees, false);
            }
            return outNavStrings;
        }
        private string[] commsPrint(List<Communication> inComms)
        {
            string[] outCommsStrings = new string[inComms.Count];
            for (int f = 0; f < inComms.Count; f++)
            {
                outCommsStrings[f] = inComms[f].Name + " " + readableFrequency(inComms[f].Frequency_Pri, "") + " " + readableFrequency(inComms[f].Frequency_Sec, "");
            }

            return outCommsStrings;
        }

        private string doubleToStringDMS(double degrees, bool lat)
        {
            string cardinal = "";
            if (lat)
            {
                if (degrees > 0) { cardinal = "N"; }
                else { cardinal = "S"; }
            }
            else
            {
                if (degrees > 0) { cardinal = "E"; }
                else { cardinal = "W"; }
            }
            degrees = Math.Abs(degrees);

            int d = Convert.ToInt32(Math.Floor(degrees));
            double minutes = (degrees - d) * 60;
            int m = Convert.ToInt32(Math.Floor(minutes));
            double seconds = (minutes - m) * 60;
            int s = Convert.ToInt32(Math.Round(seconds, 0));            

            return cardinal + d.ToString("D2") + "\u00B0 " + m.ToString("D2") + "' " + s.ToString("D2") + "\"";
        }
        private string readableFrequency(string freq, string chan)
        {
            string freqOutput = "";

            if (freq.Length > 0)
            {
                string multi = freq.Substring(freq.Length - 1);
                freq = freq.Replace(".", "");
                switch (multi)
                {
                    case "K":
                        freqOutput += freq.Substring(0, 3);
                        break;
                    case "M":
                        freqOutput += freq.Substring(0, 3) + "." + freq.Substring(3, 2);
                        break;
                }
            }
            if(chan.Length > 0)
            {
                if(freqOutput.Length > 0) { freqOutput += "/"; }
                if(chan.StartsWith("0")) { chan = chan.Remove(0, 1); }
                freqOutput += chan;
            }

            return freqOutput;
        }

        static public class CompatiblePdfReader
        {
            /// <summary>
            /// uses itextsharp 4.1.6 to convert any pdf to 1.4 compatible pdf, called instead of PdfReader.open
            /// </summary>
            static public PdfDocument Open(string pdfPath)
            {
                using (var fileStream = new FileStream(pdfPath, FileMode.Open, FileAccess.Read))
                {
                    var len = (int)fileStream.Length;
                    var fileArray = new Byte[len];
                    fileStream.Read(fileArray, 0, len);
                    fileStream.Close();

                    return Open(fileArray);
                }
            }

            /// <summary>
            /// uses itextsharp 4.1.6 to convert any pdf to 1.4 compatible pdf, called instead of PdfReader.open
            /// </summary>
            static public PdfDocument Open(byte[] fileArray)
            {
                return Open(new MemoryStream(fileArray));
            }

            /// <summary>
            /// uses itextsharp 4.1.6 to convert any pdf to 1.4 compatible pdf, called instead of PdfReader.open
            /// </summary>
            static public PdfDocument Open(MemoryStream sourceStream)
            {
                PdfDocument outDoc;
                sourceStream.Position = 0;

                try
                {
                    outDoc = PdfReader.Open(sourceStream, PdfDocumentOpenMode.Import);
                }
                catch (PdfReaderException)
                {
                    //workaround if pdfsharp doesn't support this pdf
                    sourceStream.Position = 0;
                    var outputStream = new MemoryStream();
                    var reader = new iTextSharp.text.pdf.PdfReader(sourceStream);
                    var pdfStamper = new iTextSharp.text.pdf.PdfStamper(reader, outputStream) { FormFlattening = true };
                    pdfStamper.Writer.SetPdfVersion(iTextSharp.text.pdf.PdfWriter.PDF_VERSION_1_4);
                    pdfStamper.Writer.CloseStream = false;
                    pdfStamper.Close();

                    outDoc = PdfReader.Open(outputStream, PdfDocumentOpenMode.Import);
                }

                return outDoc;
            }
        }
    }
}
