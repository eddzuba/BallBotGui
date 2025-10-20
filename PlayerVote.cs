namespace BallBotGui
{
    public class PlayerVote
    {
        public long id { get; set; }
        public string name { get; set; }
        public string firstName { get; set; }
        public long idVote { get; set; }

        public int rating { get; set; }

        public PlayerVote(long id, string name, string firstName, long idVote, int rating)
        {
            this.id = id;
            this.name = name;
            this.firstName = firstName;
            this.idVote = idVote;
            this.rating = rating;
        }
    }
}
