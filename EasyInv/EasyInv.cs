using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDesk.Options;

namespace EasyInv {

    class EasyInv {

        static void Main(string[] args) {
            //Declare vars
            string csvPath = string.Empty;
            string exportPath = string.Empty;
            string resultsHeader = "Header";
            List<long> upcCodes = new List<long>();
            bool showHelp = false;
            bool showSetup = false;

            //Declare args
            var options = new OptionSet() {
                { "c|csv=", "The path to a CSV containing UPC codes in the first collumn. Requires path to a '.csv' file.", v => csvPath = v },
                { "u|upc=", "A single UPC code to scan. Can be used multiple times in one execution. Requires numerical UPC code.",  (long v) => upcCodes.Add(v) },
                { "e|export=", "Export results to a csv. Requires a path to a '.csv' file (it will create a new one at the location with the given filename).", v => exportPath = v },
                { "h|header=", "A header for the results.", v => resultsHeader = v },
                { "setup", "Information on how to initalize EasyInv.", v => showSetup = (v != null) },
                { "help", "Information on the commands for EasyInv.", v => showHelp = (v != null) }
            };

            //Parse args
            try {
                options.Parse(args);
            }
            catch (OptionException e) {
                Console.WriteLine($"\nEasyInv: {e.Message}\nTry `EasyInv -help' for more information.");
                return;
            }

            //Display help if arg was passed or no arg was passed
            if (showHelp || args.Length == 0) {
                DisplayHelp(options);
                return;
            }
            //Display setup is the arg was passed
            else if (showSetup) {
                DisplaySetup();
                return;
            }

            //Make sure API keys are available
            InventoryAssistant.APIInformation.Init();
            if (!InventoryAssistant.APIInformation.Initialized) {
                return;
            }

            //Reads CSV containing barcodes if valid path was passed as an arg
            if (!string.IsNullOrEmpty(csvPath)) {
                if (csvPath.EndsWith(".csv")) {
                    try {
                        upcCodes.AddRange(CSVAssistant.GetBarcodeCSV(csvPath));
                    }
                    catch (FileNotFoundException e) {
                        Console.WriteLine($"EasyInv: {e.Message}.\nMake sure there is a file in the given path and try again.");
                        return;
                    }
                    catch (DirectoryNotFoundException e) {
                        Console.WriteLine($"EasyInv: {e.Message}\nMake sure the given path is valid and try again.");
                        return;
                    }
                    catch (Exception e) {
                        Console.WriteLine($"EasyInv: {e.Message}");
                        return;
                    }
                }
                else {
                    Console.WriteLine("EasyInv: Invalid file type. File must be a '.csv'.");
                    return;
                }
            }

            //If export path already exists and is a csv, pull in the information
            List<ItemInformation> items = new List<ItemInformation>();
            if (File.Exists(exportPath) && exportPath.EndsWith(".csv")) {
                items = CSVAssistant.GetRawCSV(exportPath).ToList();
            }
            //Get product information via UPC codes
            foreach (long upcCode in upcCodes) {
                //First need to make sure that if their are duplicates we dont add a new instance and up the quantity on the existing object
                bool flag = false;
                foreach (ItemInformation item in items) {
                    if (upcCode == item.UpcCode) {
                        item.Quantity++;
                        flag = true;
                        break;
                    }
                }
                if (flag) continue;

                string title = InventoryAssistant.GetItemInformation(upcCode);
                ItemInformation newItem = new ItemInformation(title, upcCode);
                items.Add(newItem);
            }

            //Output results
            if (!string.IsNullOrEmpty(exportPath)) {
                try {
                    CSVAssistant.ExportCSV(items, exportPath);
                }
                catch (Exception e) {
                    Console.WriteLine($"EasyInv: {e.Message}");
                    return;
                }
                Console.WriteLine($"EasyInv: Export succesful to path ({exportPath}).");
            }
            else {
                StringBuilder results = new StringBuilder();
                foreach (ItemInformation item in items) {
                    results.Append(item.ToString());
                }
                Console.WriteLine($"EasyInv: Results.\n\n{results}");
            }
        }

        private static void DisplayHelp(OptionSet options) {
            Console.WriteLine("\nEasyInv: Welcome to EasyInv; a program designed to make mass inventorying of objects a little bit easier." +
                "\nYou can enter UPC codes manually or import a spreadsheet (exported from a barcode scanning app on your phone (reccomended))." +
                "\nHere are the program options.");
            options.WriteOptionDescriptions(Console.Out);
        }

        private static void DisplaySetup() {
            Console.WriteLine($"\nEasyInv: Here are the steps to get started with EasyInv." +
                $"\n\t1. You will first need to sign up for the Digit-Eyes API (http://www.digit-eyes.com/api.html)." +
                $"\n\t2. Then you will need to create a file at the path ({Directory.GetCurrentDirectory()}) with the extension '.api'." +
                $"\n\t3. On the first line of this file, you will need to insert your application key provided by Digit-Eyes." +
                $"\n\t4. On the next line, insert your authentication key which is also provided by Digit-Eyes." +
                $"\n\t5. You are now ready to start using EasyInv! Try 'EasyInv -help' for more information.");
        }
    }
}
