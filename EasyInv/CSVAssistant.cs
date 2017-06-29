using System;
using System.Collections.Generic;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;

namespace EasyInv {
    public static class CSVAssistant {

        public static IEnumerable<ItemInformation> GetCSV(string path) {
            using (StreamReader reader = new StreamReader(path)) {
                var csv = new CsvReader(reader);
                var records = csv.GetRecords<ItemInformation>();
                return records;
            }
        }

        public static void WriteCSV(IEnumerable<ItemInformation> items, string path){
            using(StreamWriter writer = new StreamWriter(path)){
                var csv = new CsvWriter(writer);
                csv.WriteRecords(items);
            }
        }
    }

    public class ItemInformationMap : CsvClassMap<ItemInformation> {
        public ItemInformationMap(){
            Map(m => m.Title);
            Map(m => m.UPCCode);
            Map(m => m.Quantity);
        }
    }
}
