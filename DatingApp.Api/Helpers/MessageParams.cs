namespace DatingApp.Api.Helpers
{
    public class MessageParams
    {
         private const  int MaxPageSize = 50;

        // setting the default page Number
        public int PageNumber {get;set;} = 1;

        // setting the default page Size

        private int pageSize = 10;

        //allowing users to set the page size from the client side
        public int PageSize{
            get {return pageSize;}
            set{pageSize = (value > MaxPageSize) ? MaxPageSize : value;}
        }

        //Filtering Parameters
        public int UserId{get;set;}

        public string MessageContainer{get; set;} = "Unread";

    }
}