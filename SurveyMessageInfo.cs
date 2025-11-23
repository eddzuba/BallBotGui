using System.Collections.Generic;

namespace BallBotGui
{
    public class SurveyMessageInfo
    {
        public long PlayerId { get; set; }
        public List<int> MessageIds { get; set; } = new List<int>();

        public SurveyMessageInfo(long playerId)
        {
            PlayerId = playerId;
        }
    }
}
