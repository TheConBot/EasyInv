using System;
using System.IO;
using System.Collections.Generic;
using JitBit.Util;
using NDesk.Options;

namespace EasyInv
{
    class Program
    {
        static void Main(string[] args)
        {
            //Declare vars
            string csvPath = string.Empty;
            string exportPath = string.Empty;
            List<long> upcCodes = new List<long>();
            bool showHelp = false;

            var options = new OptionSet()
            {
                { "c|csv=", "The path to a CSV containing UPC codes in the first collumn.", v => csvPath = v },
                { "u|upc=", "A single UPC code to scan. Can be used multiple times in one execution (Example: EasyInv -u # -u #).",  (long v) => upcCodes.Add(v) },
                { "e|export=", "Export results to a csv.", v => exportPath = v },
                { "h|help", "Information on the commands for EasyInv.", v => showHelp = (v != null) }
            };

            //Parse args
            try
            {
                options.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("\nEasyInv: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `EasyInv --help' for more information.");
                return;
            }

            //Act on parsed args
            if (showHelp || args.Length == 0)
            {
                DisplayHelp(options);
                return;
            }

            if (!string.IsNullOrEmpty(csvPath))
            {
                string document = "";

                try
                {
                    using (StreamReader sr = new StreamReader(csvPath))
                    {
                        document = sr.ReadToEnd();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"ERROR: Could not read file from path.\n{e}");
                    return;
                }

                string[] rows = document.Split('\n');
                string[] cells;
                for (int i = 0; i < rows.Length; i++)
                {
                    cells = rows[i].Split(',');
                    if (long.TryParse(cells[0], out long upcCode))
                    {
                        upcCodes.Add(upcCode);
                    }
                }
            }

            //Get product information via UPC codes
            CsvExport csv = new CsvExport();
            foreach (long upcCode in upcCodes)
            {
                InventoryAssistant.GetItemInformation(upcCode, out string info);
                AddToCSV(csv, info, upcCode);
            }

            //Output results
            if (!string.IsNullOrEmpty(exportPath))
            {
                try
                {
                    csv.ExportToFile(exportPath);
                }
                catch
                {
                    Console.WriteLine("ERROR: Could not export to path.");
                    return;
                }
                Console.WriteLine($"EasyInv: Export succesful to path ({exportPath}).");
            }
            else
            {
                Console.WriteLine($"EasyInv: Results..\n{csv.Export()}");
            }
        }

        private static void AddToCSV(CsvExport csv, string title, long upcCode)
        {
            csv.AddRow();
            csv["Title"] = CsvExport.MakeValueCsvFriendly(title);
            csv["UPC"] = upcCode;
        }

        private static void DisplayHelp(OptionSet options)
        {
            Console.Write("\nEasyInv: ");
            Console.WriteLine("Welcome to EasyInv; a program designed to make mass inventorying of objects a little bit easier.\nYou can enter a single UPC code or import a spreadsheet exported from a barcode scanning app on your phone.\nHere are the program options: ");
            options.WriteOptionDescriptions(Console.Out);
        }
    }
}
