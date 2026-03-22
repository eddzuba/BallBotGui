using Newtonsoft.Json;

namespace BallBotGui
{
    public class TeamComposition
    {
        public DateTime Timestamp { get; set; }
        /// <summary>Список команд, где каждая команда — это список ID игроков</summary>
        public List<List<long>> Teams { get; set; } = new();
        /// <summary>ID сообщения в Telegram, в котором были опубликованы составы команд</summary>
        public int MessageId { get; set; }

        public TeamComposition(DateTime timestamp, List<List<long>> teams, int messageId = 0)
        {
            Timestamp = timestamp;
            Teams = teams;
            MessageId = messageId;
        }

        // Для обратной совместимости или удобства (если нужно)
        [JsonIgnore]
        public List<long> Team1PlayerIds => Teams.Count > 0 ? Teams[0] : new();
        [JsonIgnore]
        public List<long> Team2PlayerIds => Teams.Count > 1 ? Teams[1] : new();
    }
}
