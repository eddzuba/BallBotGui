using System.Text.RegularExpressions;


namespace BallBotGui
{
    public class Teams
    {
        public List<Player> Team1 = new();
        public List<Player> Team2 = new();
    }
    public class Player
    {
        public Player(long id, string name, string firstName) { 
            this.id = id;
            this.name = name;
            this.firstName = firstName;
        }
        public long id { get; set; }  // уникальный код игрока
        public string name { get; set; }  // имя линка
        public string firstName { get; set; } // Имя игрока

        public int group { get; set; } = 2;// Группа , по уровню игры

        /* 1 самые сильные
           2 средние
           3 начинающие
        */
    }

    public class PlayerVote
    {
        public long id { get; set; }
        public string name { get; set; }
        public string firstName { get; set; }
        public long idVote { get; set; }

        public PlayerVote(long id, string name, string firstName, long idVote)
        {
            this.id = id;
            this.name = name;
            this.firstName = firstName;
            this.idVote = idVote;
        }
    }

    public class Poll
    { 
        public bool approved { get; set; } = false;
        public string date { get; set; }    // дата игры
        public string question { get; set; } // текст опроса

        public int idMessage { get; set; }
        public string idPoll { get; set; }   // код опроса



        public Poll(string idPoll, string date, string question, int idMessage = 0) {
            this.idPoll = idPoll;
            this.question = question;
            this.idMessage = idMessage;

            if (date == string.Empty)
            {
                this.date = GetGameDateFromQuestion(question);
            } else
            {
              this.date = date;
            }
          
        }

        public List<PlayerVote> playrsList { get; set; } = new();

        public void DeletePlayerFromList(long idPlayer)
        {
                PlayerVote personToRemove = playrsList.FirstOrDefault(p => p.id == idPlayer);
                if (personToRemove != null)
                {
                    playrsList.Remove(personToRemove);
                }
        }

        public PlayerVote AddPlayerToList(long id, string name, string firstName, long idVoute)
        {
            PlayerVote curPerson = playrsList.FirstOrDefault(p => p.id == id);
            if (curPerson != null)
            {
               return curPerson;
            }
            else
            {
                var newPlayer = new PlayerVote(id, name, firstName, idVoute);
                playrsList.Add(newPlayer);
                return newPlayer;
            }
        }

        // Переопределение метода Equals для сравнения опросов по их характеристикам
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Poll other = (Poll)obj;
            return idPoll == other.idPoll;
        }

        public override int GetHashCode()
        {
            return idPoll.GetHashCode() ^ date.GetHashCode();
        }

        private string GetGameDateFromQuestion(string question)
        {
            string result = string.Empty;
            string pattern = @"\b\d{2}\.\d{2}\b";

            // Создание объекта Regex
            Regex regex = new(pattern);

            // Поиск первого совпадения
            Match match = regex.Match(question);

            // Вывод первой найденной даты в формате "dd.mm"
            if (match.Success)
            {
                result = match.Value;
            }
            return result;
        }

        public  DateTime GetGameDate()
        {
            try
            {
                DateTime currentDate = DateTime.Now;
                DateTime result;
                if (DateTime.TryParseExact(this.date + "." + currentDate.Year, "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out result))
                {
                    // Вычислить разницу между полученной датой и текущей датой
                    TimeSpan difference = result - currentDate;

                    // Если дата ближе к следующему году, использовать следующий год
                    if (difference.Days < -180)
                    {
                        result = result.AddYears(1);
                    }
                    if(difference.Days > 180 )
                    {
                        result = result.AddYears(-1);
                    }
                    // Если дата ближе к текущему году или дата в этом году, то использовать текущий год
                    return result;
                }

            } catch {
                
            }
            return DateTime.MinValue;
        }
    }
    public class State
    {
        public List<Poll> pollList = new();

        public Poll AddNewPoll(string idPoll, string date, string question, int messageId = 0)
        {

            Poll oldPoll = pollList.FirstOrDefault(poll => poll.idPoll == idPoll);
            if (oldPoll == null)
            {
                var newPoll = new Poll(idPoll, date, question, messageId);
                pollList.Add(newPoll);
                return newPoll;
            }
           
            return oldPoll;

        }
         
         
        public bool RemovePoll(string idPoll, string date, string question) {
            
            var newPoll = new Poll(idPoll, date, question);
            if (!pollList.Contains(newPoll))
            {
                pollList.Remove(newPoll);
                return true;
            }
            return false;
        }
     

    }
}
