/**
 * Author:          Samantha Cook-Chong
 * Date:            3/11/2014
 * Description:     A reporting tool that reads xml, txt and csv tax data files 
 *                  and creates an output file in csv format that summarizes all 
 *                  the data. 
 *               
 *                  The output file is written to the current directory (the 
 *                  directory the exe file is in) and is called "output.csv".
 *               
 *                  For the output file, only records that have the same Country, 
 *                  State, County, City, Tax Type, and Tax Rate are grouped 
 *                  together. 
 *               
 *                  The Net Sales and Tax Amount for all records that are grouped 
 *                  are listed below the entries in their group in the output file. 
 *                  
 * Execution:       Open a terminal and navigate to the folder TaxesProblem.exe is
 *                  in.  
 *                  
 *                  Type "TaxesProblem [filename1] [filename2] [filename3]..." 
 *                  into the command prompt.  For example 
 *                  "TaxesProblem TaxData.xml TaxData.txt TaxData.csv".
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;

namespace TaxesProblem
{
    class TaxesProblem
    {
        static void Main(string[] args)
        {
            // To keep track of the entries in all
            // the files we have read.
            List<Entry> list = new List<Entry>();

            // Instances of the classes we are going
            // to use to read the tax files.
            xmlFile xml = null;
            txtFile txt = null;
            csvFile csv = null;

            string extension = "";

            for (int i = 0; i < args.Length; i++)
            {
                // Check to make sure the file exists
                if (File.Exists(args[i]))
                {
                    extension = Path.GetExtension(args[i]);

                    // If the current tax file is an xml file
                    if (extension == ".xml")
                    {
                        xml = new xmlFile(args[i]);
                        list.AddRange(xml.getEntries());
                    }
                    // If the current tax file is a text file
                    else if (extension == ".txt")
                    {
                        txt = new txtFile(args[i]);
                        list.AddRange(txt.getEntries());
                    }
                    // If the current tax file is a csv file
                    else if (extension == ".csv")
                    {
                        csv = new csvFile(args[i]);
                        list.AddRange(csv.getEntries());
                    }
                    // No else.  If the current file is not in a 
                    // valid format we skip it.
                }
                else
                {
                    // If the file does not exist notify the user
                    Console.Write("File " + args[i] + " does not exist.\n\n");
                    Console.WriteLine("Press any key to exit.");
                    Console.ReadKey();
                }
            }

            // Using grouping functionality to group the entries by country, state, county, city, tax type and tax
            // rate.
            var orderedList = list.GroupBy(x => new { x.Country, x.State, x.County, x.City, x.TaxType, x.TaxRate })
                .Select(y => new Group()
                {
                    Country = y.Key.Country,
                    State = y.Key.State,
                    County = y.Key.County,
                    City = y.Key.City,
                    TaxType = y.Key.TaxType,
                    TaxRate = y.Key.TaxRate,
                    Entries = y.ToList()
                });

            // Path and name of the output file
            string filePath = @"output.csv";

            // Delimiter for output file.
            string delimiter = ",";

            // Variable to store text to be written to output file
            StringBuilder sb = new StringBuilder();

            // Keep track of the total net sales for each group
            decimal totalNetSales = 0;

            // Keep track of the total tax amount for each group
            decimal totalTaxAmount = 0;

            // For each group
            foreach (var item in orderedList)
            {
                // For each item in each group
                foreach (var anEntry in item.Entries)
                {
                    // Adding each entry to our output string
                    sb.AppendLine(string.Join(delimiter, 
                        item.Country, item.State, 
                        item.County, item.City, 
                        item.TaxType, item.TaxRate, 
                        anEntry.NetSales, anEntry.TaxAmount));

                    // Adding the net sales and tax amount 
                    // value for each entry to our total.
                    totalNetSales += anEntry.NetSales;
                    totalTaxAmount += anEntry.TaxAmount;
                }

                // Adding the net sales and tax amount totals for 
                // each group to the output string.
                sb.AppendLine("");
                sb.AppendLine("Total Net Sales: " + totalNetSales);
                sb.AppendLine("Total Tax Amount:    " + totalTaxAmount);
                sb.AppendLine("");

                // Clearing the values in the total variables.
                totalNetSales = 0;
                totalTaxAmount = 0;
            }

            // Writting the output string to the output file.
            File.WriteAllText(filePath, sb.ToString());     
        }

        // A class to hold each tax entry
        public class Entry 
        {
            public string Country;
            public string State;
            public string County;
            public string City;
            public string TaxType;

            public decimal TaxRate;
            public decimal NetSales;
            public decimal TaxAmount;

            // Constructor
            public Entry(string country, string state, string county, string city, 
                string taxType, decimal taxRate, decimal netSales, decimal taxAmount)
            {
                this.Country = country;
                this.State = state;
                this.County = county;
                this.City = city;
                this.TaxType = taxType;
                this.TaxRate = taxRate;
                this.NetSales = netSales;
                this.TaxAmount = taxAmount;               
            }
        }

        // Since all three of these classes use lists of entrys and are pretty much
        // the same I could have made one class called File with a function called
        // fileProcessor to do the work that is currently being done in the 
        // constructor of each of these three classes and have each of these classes
        // inherit from the File class and simply implement their own version of
        // fileProcessor but since this program is so small and the classes are so
        // small I thought it would be easier to understand/read this way.

        // Class to process XML tax files.
        class xmlFile
        {
            // A list to hold the entrys created when the XML file was processed.
            List<Entry> list = new List<Entry>();

            // Constructor
            public xmlFile(string fileName)
            {
                XmlDocument taxFile = new XmlDocument();
                taxFile.Load(fileName);

                XmlNodeList taxEntries = taxFile.SelectNodes("ArrayOfTaxData/TaxData");

                XmlNode currentEntry = null;

                // Getting the values from the XML tax file and using them to create
                // instances of the entry class which are then added to the list.
                for (int i = 0; i < taxEntries.Count; i++)
                {
                    currentEntry = taxEntries[i];

                    list.Add(new Entry(currentEntry.ChildNodes[0].InnerText,
                        currentEntry.ChildNodes[1].InnerText, currentEntry.ChildNodes[2].InnerText,
                        currentEntry.ChildNodes[3].InnerText, currentEntry.ChildNodes[4].InnerText,
                        Convert.ToDecimal(currentEntry.ChildNodes[5].InnerText),
                        Convert.ToDecimal(currentEntry.ChildNodes[6].InnerText),
                        Convert.ToDecimal(currentEntry.ChildNodes[7].InnerText)));
                }
            }

            // Public accessor for the list
            public List<Entry> getEntries() { return list; }
        }

        // Class to process the text tax files.
        class txtFile
        {
            // A list to hold the entrys created when the text file was processed.
            List<Entry> list = new List<Entry>();

            // Constructor
            public txtFile(string fileName)
            {
                string line = "";
                System.IO.StreamReader file = new System.IO.StreamReader(fileName);

                // Getting the values from the text tax file and using them to create
                // instances of the entry class which are then added to the list.
                while ((line = file.ReadLine()) != null)
                {
                    list.Add(new Entry(line.Substring(0, 3).Trim(), 
                        line.Substring(3, 2).Trim(), line.Substring(5, 10).Trim(), 
                        line.Substring(15, 20).Trim(), line.Substring(35, 20).Trim(),
                        Convert.ToDecimal(line.Substring(55, 6).Trim()),
                        Convert.ToDecimal(line.Substring(61, 6).Trim()),
                        Convert.ToDecimal(line.Substring(67, 9).Trim())));
                }

                file.Close();
            }

            // Public accessor for the list
            public List<Entry> getEntries() { return list; }
        }

        // Class to process the CSV tax files.
        class csvFile
        {
            // A list to hold the entrys created when the CSV file was processed.
            List<Entry> list = new List<Entry>();

            // Constructor
            public csvFile(string fileName)
            {
                string line;
                Array temp = null;
                System.IO.StreamReader file = new System.IO.StreamReader(fileName);

                // Getting the values from the CSV tax file and using them to create
                // instances of the entry class which are then added to the list.
                while ((line = file.ReadLine()) != null)
                {
                    temp = line.Split(',');

                    list.Add(new Entry(temp.GetValue(0).ToString(), 
                        temp.GetValue(1).ToString(), temp.GetValue(2).ToString(), 
                        temp.GetValue(3).ToString(), temp.GetValue(4).ToString(),
                        Convert.ToDecimal(temp.GetValue(5)), 
                        Convert.ToDecimal(temp.GetValue(6)), 
                        Convert.ToDecimal(temp.GetValue(7))));
                }

                file.Close();
            }

            // Public accessor for the list
            public List<Entry> getEntries() { return list; }
        }

        // Class to hold the information for each group
        // of entrys.
        public class Group
        {
            public string Country { get; set; }
            public string State { get; set; }
            public string County { get; set; }
            public string City { get; set; }
            public string TaxType { get; set; }
            public decimal TaxRate { get; set; }
            public List<Entry> Entries { get; set; }
        }
    }
}
