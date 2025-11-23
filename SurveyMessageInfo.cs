using System.Collections.Generic;

namespace BallBotGui
{
    public class SurveyMessage
    {
        public int MessageId { get; set; }
        public string Nomination { get; set; }
    }

    public class SurveyMessageInfo
    {
        public long PlayerId { get; set; }
        public List<SurveyMessage> Messages { get; set; } = new List<SurveyMessage>();

        public SurveyMessageInfo() { }

        public SurveyMessageInfo(long playerId)
        {
            PlayerId = playerId;
        }
    }
}
