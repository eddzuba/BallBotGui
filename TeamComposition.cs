namespace BallBotGui
{
    public class TeamComposition
    {
        public DateTime Timestamp { get; set; }
        public List<long> Team1PlayerIds { get; set; } = new();
        public List<long> Team2PlayerIds { get; set; } = new();

        public TeamComposition(DateTime timestamp, List<long> team1PlayerIds, List<long> team2PlayerIds)
        {
            Timestamp = timestamp;
            Team1PlayerIds = team1PlayerIds;
            Team2PlayerIds = team2PlayerIds;
        }
    }
}
