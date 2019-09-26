namespace DatingApp.Api.Model
{
    public class Like
    {
        // userId of the user clicking the like button
        public int LikerId{get;set;}

        //userId of the user being liked
        public int LikeeId{get;set;}

        public User Liker {get;set;}

        public User Likee {get;set;}
    }
}