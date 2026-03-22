using System;

namespace BallBotGui
{
    public class GameScore
    {
        public int Team1Score { get; set; }
        public int Team2Score { get; set; }
        public bool IsFinished { get; set; }
        public DateTime Timestamp { get; set; }

        public GameScore()
        {
            Timestamp = DateTime.Now;
        }

        public GameScore(int team1Score, int team2Score, bool isFinished)
        {
            Team1Score = team1Score;
            Team2Score = team2Score;
            IsFinished = isFinished;
            Timestamp = DateTime.Now;
        }
    }
}
