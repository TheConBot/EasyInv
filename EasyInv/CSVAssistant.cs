using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;

namespace EasyInv {

    public static class CSVAssistant {

        public static List<ItemInformation> GetRawCSV(string path) {
            using (StreamReader reader = new StreamReader(path)) {
                var csv = new CsvReader(reader);
                csv.Configuration.RegisterClassMap<ItemInformationMap>();
                try {
                    //This needs to return a list because IEnum won't be loaded into mem all at once and when it goes to read the IEnum the reader is already closed, have to return the whole var at once
                    return csv.GetRecords<ItemInformation>().ToList();
                }
                catch(CsvHelperException e) {
                    return null;
                }
            }
        }

        public static List<long> GetBarcodeCSV(string path) {
            using (StreamReader reader = new StreamReader(path)) {
                string document = reader.ReadToEnd().Trim();
                string[] rows = document.Split('\n');
                List<long> upcCodes = new List<long>();

                for (int i = 0; i < rows.Length; i++) {
                    var row = rows[i];
                    int index = row.IndexOf(',');
                    if(index > 0) row = new string(row.Substring(0, index).Where(c => char.IsDigit(c)).ToArray());
                if (long.TryParse(row, out long upcCode)) {
                        upcCodes.Add(upcCode);
                    }
                    else {
                        Console.WriteLine($"EasyInv: CSV file contains a non numerical entry in row ({i}).");
                    }
                }
                return upcCodes;
            }
        }

        public static void ExportCSV(IEnumerable<ItemInformation> items, string path) {
            using (StreamWriter writer = new StreamWriter(path)) {
                var csv = new CsvWriter(writer);
                csv.Configuration.RegisterClassMap<ItemInformationMap>();
                csv.WriteRecords(items);
            }
        }
    }

    public class ItemInformationMap : CsvClassMap<ItemInformation> {
        public ItemInformationMap() {
            Map(m => m.Title).Index(0);
            Map(m => m.UpcCode).Index(1);
            Map(m => m.Quantity).Index(2);
        }
    }
}
