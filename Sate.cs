using System.ComponentModel;


namespace BallBotGui
{
    public class State
    {
        public List<Poll> pollList = new();
        public BindingList<Car> carList = new();
        public List<DislikedTeammates> dislikedTeammates = new();
        public List<string> spamStopWords = new();

        public List<SkillCheckRequest> skillCheckRequests = new();

        public Poll AddNewPoll(string idPoll, string date, string question, int messageId, VolleybollGame? curGame, int ratingMessageId)
        {

            Poll oldPoll = pollList.FirstOrDefault(poll => poll.idPoll == idPoll);
            if (oldPoll == null)
            {
                var newPoll = new Poll(idPoll, date, question, messageId, curGame, ratingMessageId);
                pollList.Add(newPoll);
                return newPoll;
            }

            return oldPoll;

        }
    }

    public class SkillCheckRequest
    {
        public long RequesterId { get; set; }
        public long TargetUserId { get; set; }
        public DateTime RequestDate { get; set; }

        public SkillCheckRequest(long requesterId, long targetUserId)
        {
            RequesterId = requesterId;
            TargetUserId = targetUserId;
            RequestDate = DateTime.Now;
        }
    }
}
