namespace BallBotGui
{
    public class PostGameVote
    {
        public long VoterId { get; set; }
        public string Nomination { get; set; }
        public List<long> SelectedPlayerIds { get; set; } = new();

        public PostGameVote(long voterId, string nomination, List<long> selectedPlayerIds)
        {
            VoterId = voterId;
            Nomination = nomination;
            SelectedPlayerIds = selectedPlayerIds;
        }
    }
}
