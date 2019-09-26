namespace DatingApp.Api.Helpers
{
    public class UserParams
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

        public string Gender{get;set;}

        public int MinAge{get;set;} = 18;

        public int MaxAge{get;set;} = 99;

        //Sorting Parameters

        public string OrderBy{get;set;}

        //getting the list of likees and likers
        public bool Likees {get;set;} = false;

        public bool Likers{get;set;} = false;

    }
}