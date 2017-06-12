using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
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
            string resultsHeader = "Header";
            List<long> upcCodes = new List<long>();
            bool showHelp = false;
            bool showSetup = false;

            //Declare args
            var options = new OptionSet()
            {
                { "c|csv=", "The path to a CSV containing UPC codes in the first collumn. Requires path to a '.csv' file.", v => csvPath = v },
                { "u|upc=", "A single UPC code to scan. Can be used multiple times in one execution. Requires numerical UPC code.",  (long v) => upcCodes.Add(v) },
                { "e|export=", "Export results to a csv. Requires a path to a '.csv' file (it will create a new one at the location with the given filename).", v => exportPath = v },
                { "h|header=", "A header for the results.", v => resultsHeader = v },
                { "setup", "Information on how to initalize EasyInv.", v => showSetup = (v != null) },
                { "help", "Information on the commands for EasyInv.", v => showHelp = (v != null) }
            };

            //Parse args
            try
            {
                options.Parse(args);
            }
            catch (OptionException e)
            {
                Console.WriteLine($"\nEasyInv: {e.Message}\nTry `EasyInv -help' for more information.");
                return;
            }

            //Display help if arg was passed or no arg was passed
            if (showHelp || args.Length == 0)
            {
                DisplayHelp(options);
                return;
            }
            else if (showSetup)
            {
                DisplaySetup();
                return;
            }

            //Read CSV if arg was passed
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
                catch (FileNotFoundException e)
                {
                    Console.WriteLine($"EasyInv: {e.Message}.\nMake sure there is a file in the given path and try again.");
                    return;
                }
                catch (DirectoryNotFoundException e)
                {
                    Console.WriteLine($"EasyInv: {e.Message}\nMake sure the given path is valid and try again.");
                    return;
                }
                catch(Exception e)
                {
                    Console.WriteLine($"EasyInv: {e.Message}");
                    return;
                }
                if (Path.GetExtension(csvPath) != ".csv")
                {
                    Console.WriteLine("EasyInv: Invalid file type. File must be a '.csv'.");
                    return;
                }

                string[] rows = document.Split('\n');
                string[] cells;
                for (int i = 0; i < rows.Length; i++)
                {
                    cells = rows[i].Split(',');
                    string cell = new string(cells[0].Where(c => char.IsDigit(c)).ToArray());
                if (long.TryParse(cell, out long upcCode))
                    {
                        upcCodes.Add(upcCode);
                    }
                    else
                    {
                        Console.WriteLine($"EasyInv: CSV file contains a non numerical entry in row ({i}).");
                    }
                }
            }

            //Get product information via UPC codes
            if(upcCodes.Count == 0)
            {
                Console.WriteLine("EasyInv: No UPC codes were provided.");
                return;
            }
            InventoryAssistant.APIInformation.Init();
            if (!InventoryAssistant.APIInformation.Initialized)
            {
                return;
            }
            CsvExport csv = new CsvExport();
            csv.AddRow();
            csv[""] = resultsHeader;
            foreach (long upcCode in upcCodes)
            {
                string info = InventoryAssistant.GetItemInformation(upcCode);
                AddToCSV(csv, info, upcCode);
            }

            //Output results
            if (!string.IsNullOrEmpty(exportPath))
            {
                try
                {
                    csv.ExportToFile(exportPath);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"EasyInv: {e.Message}");
                    return;
                }
                Console.WriteLine($"EasyInv: Export succesful to path ({exportPath}).");
            }
            else
            {
                Console.WriteLine($"EasyInv: Results.\n\n{csv.Export()}");
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
            Console.WriteLine("\nEasyInv: Welcome to EasyInv; a program designed to make mass inventorying of objects a little bit easier." +
                "\nYou can enter UPC codes manually or import a spreadsheet exported from a barcode scanning app on your phone (reccomended)." +
                "\nHere are the program options.");
            options.WriteOptionDescriptions(Console.Out);
        }

        private static void DisplaySetup()
        {
            Console.WriteLine($"\nEasyInv: Here are the steps to get started with EasyInv." +
                $"\n\t1. You will first need to sign up for the Digit-Eyes API (http://www.digit-eyes.com/api.html)." +
                $"\n\t2. Then you will need to create a file at the path ({Directory.GetCurrentDirectory()}) with the extension '.api'." +
                $"\n\t3. On the first line of this file, you will need to insert your application key provided by Digit-Eyes." +
                $"\n\t4. On the next line, insert your authentication key which is also provided by Digit-Eyes." +
                $"\n\t5. You are now ready to start using EasyInv! Try 'EasyInv -help' for more information.");
        }
    }
}
