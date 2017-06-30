namespace EasyInv {

    public class ItemInformation {

        private string title;
        private long upcCode;
        private int quantity;

        public string Title { get => title; set => title = value; }
        public long UpcCode { get => upcCode; set => upcCode = value; }
        public int Quantity { get => quantity; set => quantity = value; }

        public ItemInformation() {
            Title = null;
            UpcCode = -1;
            Quantity = 0;
        }
        public ItemInformation(string title, long upc) {
            Title = title;
            UpcCode = upc;
            Quantity = 1;
        }

        public override string ToString() {
            return $"{Title}, {UpcCode}, {Quantity}\n";
        }
    }
}