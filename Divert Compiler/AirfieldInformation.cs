using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using Gavaghan.Geodesy;

namespace Divert_Compiler
{
    public class Importer
    {

        /// <summary>
        /// Import information from databases contained in the DAFIF, will return Field, Runway, Nav and Comms information
        /// </summary>
        /// <param name="folderLocation">Location of the DAFIF in file system</param>
        /// <returns>Imported data</returns>
        public List<AirfieldInformation> fromDAFIF(string folderLocation)
        {
            //
            List<AirfieldInformation> importData = new List<AirfieldInformation>();

            // Set location of folders on disc
            string arptDir = folderLocation + @"\AVDAFIF\ARPT\ARPT.dbf";
            string rwyDir = folderLocation + @"\AVDAFIF\RWY\RWY.dbf";
            string navhDir = folderLocation + @"\AVDAFIF\NAV\NAVH.dbf";
            string navlDir = folderLocation + @"\AVDAFIF\NAV\NAVL.dbf";
            string navtDir = folderLocation + @"\AVDAFIF\NAV\NAVT.dbf";
            string ilsDir = folderLocation + @"\AVDAFIF\RWY\ILS.dbf";
            string acomDir = folderLocation + @"\AVDAFIF\ARPT\ACOM.dbf";

            // Import data from database using ParseDBF
            DataTable dtARPT = ParseDBF.ReadDBF(arptDir);
            DataTable dtRWY = ParseDBF.ReadDBF(rwyDir);
            DataTable dtNAVH = ParseDBF.ReadDBF(navhDir);
            DataTable dtNAVL = ParseDBF.ReadDBF(navlDir);
            DataTable dtNAVT = ParseDBF.ReadDBF(navtDir);
            DataTable dtILS = ParseDBF.ReadDBF(ilsDir);
            DataTable dtACOM = ParseDBF.ReadDBF(acomDir);

            // Import data
            foreach (DataRow rowL1 in dtARPT.Rows)
            {
                // Get field data
                Field fieldData = new Field(rowL1, dtARPT.Columns);

                // Create lists to store multiple data
                List<Runway> runwaysData = new List<Runway>();
                List<NavigationAid> navData = new List<NavigationAid>();
                List<Communication> commsData = new List<Communication>();

                List<DataRow> foundRows = new List<DataRow>();

                // Get runway info for current field
                foundRows.Clear();
                foundRows.AddRange(dtRWY.Select("ARPT_IDENT = '" + fieldData.Ident + "'"));
                foreach (DataRow rowL2 in foundRows)
                {
                    runwaysData.Add(new Runway(rowL2, dtRWY.Columns));
                }

                // Nav
                foundRows.Clear();
                foundRows.AddRange(dtNAVH.Select("ARPT_ICAO = '" + fieldData.ICAO + "'"));
                foundRows.AddRange(dtNAVL.Select("ARPT_ICAO = '" + fieldData.ICAO + "'"));
                foundRows.AddRange(dtNAVT.Select("ARPT_ICAO = '" + fieldData.ICAO + "'"));
                foreach (DataRow rowL2 in foundRows)
                {
                    navData.Add(new NavigationAid(rowL2, dtNAVH.Columns, "NAV"));
                    if (fieldData.Country == null) { fieldData.Country = rowL2.ItemArray[dtNAVH.Columns.IndexOf("CTRY")].ToString().Trim(); }
                }
                // ILS
                foundRows.Clear();
                foundRows.AddRange(dtILS.Select("ARPT_IDENT = '" + fieldData.Ident + "'"));
                foreach (DataRow rowL2 in foundRows)
                {
                    navData.Add(new NavigationAid(rowL2, dtILS.Columns, "ILS"));
                }
                //Tidy nav list
                List<NavigationAid> navToRemove = new List<NavigationAid>();
                for (int i = 0; i < navData.Count - 1; i++)
                {
                    for (int j = i + 1; j < navData.Count; j++)
                    {
                        if (i == j) { continue; }
                        if (navData[i].Ident == navData[j].Ident && navData[i].Type == navData[j].Type) { navToRemove.Add(navData[i]); }
                        if (navData[i].Type == "GLIDE" && navData[j].Type == "LOC" && navData[i].Runway == navData[j].Runway) { navData[j].Type = "ILS"; navToRemove.Add(navData[i]); }
                    }
                }
                foreach (NavigationAid n in navToRemove)
                {
                    navData.Remove(n);
                }

                // Comms
                foundRows.Clear();
                foundRows.AddRange(dtACOM.Select("ARPT_IDENT = '" + fieldData.Ident + "'"));
                foreach (DataRow rowL2 in foundRows)
                {
                    commsData.Add(new Communication(rowL2, dtACOM.Columns));
                }
                //Tidy Comms list
                List<Communication> commsToRemove = new List<Communication>();
                for (int i = 0; i < commsData.Count - 1; i++)
                {
                    for (int j = i + 1; j < commsData.Count; j++)
                    {
                        if (i == j) { continue; }
                        if (commsData[i].Name == commsData[j].Name && commsData[i].Frequency_Pri == commsData[j].Frequency_Pri) { commsToRemove.Add(commsData[i]); }
                    }
                }
                foreach (Communication c in commsToRemove)
                {
                    commsData.Remove(c);
                }
                commsData.Sort((x, y) => x.Name.CompareTo(y.Name));

                // Add current airfield to complete list of airfields
                importData.Add(new AirfieldInformation(fieldData, runwaysData, navData, commsData));

            }

            return importData;
        }

        public List<AirfieldInformation> addSuppAndTerminals(List<AirfieldInformation> currentData, string folderLocation)
        {
            //Supplement
            string suppInfoDir = folderLocation + @"\xml\_supplements_split.xml";

            DataSet dsSuppInfo = new DataSet();
            dsSuppInfo.ReadXml(suppInfoDir);
            DataTable dtSuppInfo = dsSuppInfo.Tables[0];

            //Terminals
            string aeroTermInfoDir = folderLocation + @"\xml\aeroplates.xml";

            DataSet dsTermInfo = new DataSet();
            dsTermInfo.ReadXml(aeroTermInfoDir);

            foreach (AirfieldInformation afld in currentData)
            {
                afld.Plates = new List<Plate>();
                DataRow[] foundRows =  dtSuppInfo.Select("pdf_name = '" + afld.Airfield.Name.Replace(" ", "_").ToLower() + ".pdf'");
                foreach (DataRow d in foundRows)
                {
                    if (d.ItemArray[dtSuppInfo.Columns.IndexOf("wac_innr")].ToString().StartsWith(afld.Airfield.WAC))
                    {
                        string folder = folderLocation + @"\splitdocs\supplements\" + foundRows[0].ItemArray[dtSuppInfo.Columns.IndexOf("type")].ToString().Trim();
                        string fileName = folder + @"\" + foundRows[0].ItemArray[dtSuppInfo.Columns.IndexOf("pdf_name")].ToString().Trim();
                        afld.Plates.Add(new Plate("Supplement", fileName, "Supplement"));
                    }
                }

                foundRows = dsTermInfo.Tables[0].Select("ID = '" + afld.Airfield.ICAO + "'");
                // Temp blank for just supps
                foreach (DataRow d in foundRows)
                {
                    DataRow[] moreRows = dsTermInfo.Tables[1].Select("airport_code_Id = '" + d.ItemArray[dsTermInfo.Tables[0].Columns.IndexOf("airport_code_Id")].ToString().Trim() + "'");
                    foreach (DataRow md in moreRows)
                    {
                        string folder = folderLocation + @"\";
                        if (md.ItemArray[md.Table.Columns.IndexOf("dataset")].ToString().Trim() == "DOD") { folder += @"terminals\"; }
                        folder += md.ItemArray[md.Table.Columns.IndexOf("pdf_path")].ToString().Trim();
                        string fileName = folder + @"\" + md.ItemArray[md.Table.Columns.IndexOf("pdf_name")].ToString().Trim();
                        string plateType = getWordPlateType(md.ItemArray[md.Table.Columns.IndexOf("chart_code")].ToString().Trim());
                        afld.Plates.Add(new Plate(md.ItemArray[md.Table.Columns.IndexOf("chart_name")].ToString().Trim().Replace("_"," ") , fileName, plateType));
                    }
                }
                //End temp blank for just supps
                afld.Plates.Sort((a, b) => a.Name.CompareTo(b.Name));

                int currentPlatePos = 0;
                int currentSearchTerm = 0;
                string[] sortOrder = new string[] { "Supplement", "Airport Diagram", "LAHSO", "Hotspot", "Departure", "Obstacle Departure", "Instrument Approach", "Arrivals", "Minimums" };
                //string[] sortOrder = new string[] { "supplement", "airport diagram", "arpt dia", "takeoff", "ifrtkoff", "diverse", "radar", "obstacle", "dep", "ils", "loc", "tacan", "vor", "ndb", "dme", "radmin", "altmin", "rnav" };
                while (currentSearchTerm < sortOrder.Length && currentPlatePos < afld.Plates.Count)
                {
                    for (int q = currentPlatePos; q < afld.Plates.Count(); q++)
                    {
                        if (afld.Plates[q].Type.ToUpper().Contains(sortOrder[currentSearchTerm].ToUpper()))
                        {
                            Plate tempVal = afld.Plates[currentPlatePos];
                            afld.Plates[currentPlatePos] = afld.Plates[q];
                            afld.Plates[q] = tempVal;
                            currentPlatePos++;
                            break;
                        }
                        else { if (q == afld.Plates.Count - 1) { currentSearchTerm++; } }
                    }
                }
            }  

            return currentData;
        }

        private string getWordPlateType(string inputType)
        {
            switch (inputType)
            {
                case "SUP":
                    return "Supplement";
                case "APD":
                    return "Airport Diagram";
                case "LAH":
                    return "LAHSO";
                case "HOT":
                    return "Hotspot";
                case "DEP":
                    return "Departure";
                case "ODP":
                    return "Obstacle Departure";
                case "IAP":
                    return "Instrument Approach";
                case "ARR":
                    return "Arrivals";
                case "MIN":
                    return "Minimums";
                default:
                    return inputType;
            }
        }
    }

    /// <summary>
    /// Information on a airfield that would be rquired to decide if a field is a suitable divert and assist in planning.
    /// </summary>
    public class AirfieldInformation
    {
        /// <summary>General information on the airfield.</summary>
        public Field Airfield;

        /// <summary>Runway data for airfields runways.</summary>
        public List<Runway> Runways;

        /// <summary>Navaids located at the airfield.</summary>
        public List<NavigationAid> NavigationAids;

        /// <summary>Comms facilities associated with the airfield.</summary>
        public List<Communication> Communications;

        public List<Plate> Plates; 

        /// <summary>
        /// Initialize class
        /// </summary>
        public AirfieldInformation() { }

        public AirfieldInformation(Field fld, List<Runway> rwys, List<NavigationAid> navs, List<Communication> comms)
        {
            Airfield = fld;
            Runways = rwys;
            NavigationAids = navs;
            Communications = comms;
            Plates = new List<Plate>();
        }       
    }

    public class Field
    {
        public string ICAO, Ident, Name, Country, SecICAO, WAC;
        public GlobalPosition Position;
        public double RadialFromHome, RangeFromHome;
        public Angle MagneticDeclination;

        public Field() { }

        public Field(DataRow DAFIFrow, DataColumnCollection DAFIFheader)
        {
            ICAO = DAFIFrow.ItemArray[DAFIFheader.IndexOf("ICAO")].ToString().Trim();
            Ident = DAFIFrow.ItemArray[DAFIFheader.IndexOf("ARPT_IDENT")].ToString().Trim();
            Name = DAFIFrow.ItemArray[DAFIFheader.IndexOf("NAME")].ToString().Trim();
            SecICAO = DAFIFrow.ItemArray[DAFIFheader.IndexOf("SEC_ICAO")].ToString().Trim();
            WAC = DAFIFrow.ItemArray[DAFIFheader.IndexOf("WAC")].ToString().Trim();
            //Country is not included in this database
            double lat = Convert.ToDouble(DAFIFrow.ItemArray[DAFIFheader.IndexOf("WGS_DLAT")].ToString().Trim());
            double lon = Convert.ToDouble(DAFIFrow.ItemArray[DAFIFheader.IndexOf("WGS_DLONG")].ToString().Trim());
            double elev = Convert.ToDouble(DAFIFrow.ItemArray[DAFIFheader.IndexOf("ELEV")].ToString().Trim()) * .3048;
            Position = new GlobalPosition(new GlobalCoordinates(new Angle(lat), new Angle(lon)), elev);
            //Radial and range calculated later in reference to home field
            string magInputString = DAFIFrow.ItemArray[DAFIFheader.IndexOf("MAG_VAR")].ToString().Trim();
            int magVarDegrees = Convert.ToInt32(magInputString.Substring(1, 3));
            double magVarMinutes = Convert.ToDouble(magInputString.Substring(4, 3))/10;
            if(magInputString.StartsWith("W")) { magVarDegrees *= -1; }
            MagneticDeclination = new Angle(magVarDegrees, magVarMinutes);
        }
    }

    public class Runway
    {
        public string LowIdent, HighIdent, Surface;
        public int Length, Width, PCN;

        public Runway() { }

        public Runway(DataRow DAFIFrow, DataColumnCollection DAFIFheader)
        {
            LowIdent = DAFIFrow.ItemArray[DAFIFheader.IndexOf("LOW_IDENT")].ToString().Trim();
            HighIdent = DAFIFrow.ItemArray[DAFIFheader.IndexOf("HIGH_IDENT")].ToString().Trim();
            Surface = DAFIFrow.ItemArray[DAFIFheader.IndexOf("SURFACE")].ToString().Trim();
            Length = Convert.ToInt32(DAFIFrow.ItemArray[DAFIFheader.IndexOf("LENGTH")].ToString().Trim());
            Width = Convert.ToInt32(DAFIFrow.ItemArray[DAFIFheader.IndexOf("RWY_WIDTH")].ToString().Trim());
            string importPCN = DAFIFrow.ItemArray[DAFIFheader.IndexOf("PCN")].ToString().Trim();
            if(importPCN.Length > 0)
            {
                PCN = Convert.ToInt32(importPCN.Remove(3));
            }
            else { PCN = -1; }
        }
    }

    public class NavigationAid
    {
        public string Ident, Name, Type, Frequency, Channel, Runway;
        public GlobalPosition Position;

        public NavigationAid() { }

        public NavigationAid(DataRow DAFIFrow, DataColumnCollection DAFIFheader, string type)
        {
            Ident = DAFIFrow.ItemArray[DAFIFheader.IndexOf("NAV_IDENT")].ToString().Trim();
            Name = DAFIFrow.ItemArray[DAFIFheader.IndexOf("NAME")].ToString().Trim();
            Frequency = DAFIFrow.ItemArray[DAFIFheader.IndexOf("FREQ")].ToString().Trim();
            Channel = DAFIFrow.ItemArray[DAFIFheader.IndexOf("CHAN")].ToString().Trim();
            double lat = Convert.ToDouble(DAFIFrow.ItemArray[DAFIFheader.IndexOf("WGS_DLAT")].ToString().Trim());
            double lon = Convert.ToDouble(DAFIFrow.ItemArray[DAFIFheader.IndexOf("WGS_DLONG")].ToString().Trim());
            double elev = 0;
            try { elev = Convert.ToDouble(DAFIFrow.ItemArray[DAFIFheader.IndexOf("ELEV")].ToString().Trim()) * .3048; } catch { }
            Position = new GlobalPosition(new GlobalCoordinates(new Angle(lat), new Angle(lon)), elev);

            if (type == "NAV")
            {              
                Type = navaidType(DAFIFrow.ItemArray[DAFIFheader.IndexOf("TYPE")].ToString().Trim());            
            }
            if (type == "ILS")
            {
                string compType = DAFIFrow.ItemArray[DAFIFheader.IndexOf("COMP_TYPE")].ToString().Trim();
                if (compType == "Z") { Type = "LOC"; }
                if (compType == "G") { Type = "GLIDE"; }
                if (compType == "D") { Type = "DME"; }
                if(compType == "O" && Frequency != "75000M") { Type = "NDB"; }
                Runway = DAFIFrow.ItemArray[DAFIFheader.IndexOf("RWY_IDENT")].ToString().Trim();
            }
        }

        private string navaidType(string typeNo)
        {
            string outputType = null;
            switch (typeNo)
            {
                case "1":
                    outputType = "VOR";
                    break;
                case "2":
                    outputType = "VORDME";
                    break;
                case "3":
                    outputType = "TAC";
                    break;
                case "4":
                    outputType = "VORDME";
                    break;
                case "5":
                    outputType = "NDB";
                    break;
                case "6":
                    outputType = typeNo;
                    break;
                case "7":
                    outputType = typeNo;
                    break;
                case "8":
                    outputType = typeNo;
                    break;
                case "9":
                    outputType = "DME";
                    break;
                default:
                    outputType = typeNo;
                    break;
            }
            return outputType;
        }
    }

    public class Communication
    {
        public string Name, Type, Frequency_Pri, Frequency_Sec;

        public Communication() { }

        public Communication(DataRow DAFIFrow, DataColumnCollection DAFIFheader)
        {
            Name = DAFIFrow.ItemArray[DAFIFheader.IndexOf("COMM_NAME")].ToString().Trim();
            Type = DAFIFrow.ItemArray[DAFIFheader.IndexOf("COMM_TYPE")].ToString().Trim();
            Frequency_Pri = DAFIFrow.ItemArray[DAFIFheader.IndexOf("FREQ_1")].ToString().Trim();
            Frequency_Sec = DAFIFrow.ItemArray[DAFIFheader.IndexOf("FREQ_2")].ToString().Trim();
        }
    }

    public class Plate
    {
        public string Name, Location, Type;

        public Plate(string name, string loc, string type)
        {
            Name = name;
            Location = loc;
            Type = type;
        }
    }
}
