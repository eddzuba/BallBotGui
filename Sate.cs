using System.ComponentModel;


namespace BallBotGui
{
    public class State
    {
        public List<Poll> pollList = new();
        public BindingList<Car> carList = new();
        public List<DislikedTeammates> dislikedTeammates = new();
        public List<string> spamStopWords = new();

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
}
