namespace EasyInv {
    public class ItemInformation {

        public ItemInformation(){
            Title = null;
            UPCCode = -1;
            Quantity = 0;
        }
        public ItemInformation(string title, long upc){
            Title = title;
            UPCCode = upc;
            Quantity = 1;
        }
        public string Title;
        public long UPCCode;
        public int Quantity;
    }
}