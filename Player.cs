namespace BallBotGui
{
    public class Player
    {
        public Player(long id, string name, string firstName, string normalName, bool isFemale)
        {
            this.id = id;
            this.name = name;
            this.firstName = firstName;
            this.normalName = normalName;
            this.isFemale = isFemale;
            this.ratingRequestDate = null;
        }
        public long id { get; set; }  // уникальный код игрока
        public string name { get; set; }  // имя линка
        public string firstName { get; set; } // Имя игрока

        public string normalName { get; set; } // Нормальное имя игрока

        public int rating { get; set; } // Открыгый рейтинг игрока,для рейтинговых игр

        public string? ratingRequestDate { get; set; } // Дата запроса рейтинга

        public int group { get; set; } = 2;// Группа , по уровню игры

        /* 1 самые сильные
           2 средние
           3 начинающие
        */

        public bool isFemale = false;
        public int FailedMessageCount { get; set; } = 0;
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Player other = (Player)obj;
            return id == other.id;
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }
    }
}
