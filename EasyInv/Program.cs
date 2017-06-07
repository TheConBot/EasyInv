using System;
using System.IO;
using JitBit.Util;
using System.Collections.Generic;

namespace EasyInv
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.Write("Please enter a UPC, a CSV Path, or type quit: ");
                string input = Console.ReadLine();

                if (input.ToLower().StartsWith("quit"))
                {
                    break;
                }
                else if (long.TryParse(input, out long upc))
                {
                    InventoryAssistant.GetItemInformation(upc, out string info);
                    Console.WriteLine(info);
                }
                else
                {
                    string document = "";
                    try
                    {
                        using (StreamReader sr = new StreamReader(input))
                        {
                            document = sr.ReadToEnd();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"ERROR: Something went wrong with your input.\n{e}");
                        continue;
                    }

                    string[] lines = document.Split('\n');
                    string[] entrys;
                    List<CsvExport> csv = new List<CsvExport>();
                    for (int i = 0; i < lines.Length; i++)
                    {
                        entrys = lines[i].Split(',');
                        if (long.TryParse(entrys[0], out upc))
                        {
                            InventoryAssistant.GetItemInformation(upc, out string output);
                            Console.WriteLine($"Writing Item to Spreadsheet: {output}");
                            var newEntry = new CsvExport();
                            newEntry.AddRow()
                            newEntry[]
                            //entrys[0] = output;
                            //string rejoinedLine = String.Join(",", entrys);
                            //lines[i] = rejoinedLine;
                        }
                    }
                    Console.Write("Please enter a path to write the new CSV to: ");
                    string newPath = Console.ReadLine();
                    File.WriteAllLines(newPath, lines);
                }
            }
        }
    }

    class InventoryItem
    {
        public string title;

    }
}
