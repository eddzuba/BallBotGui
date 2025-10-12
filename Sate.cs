using System.ComponentModel;
using System.Text.RegularExpressions;


namespace BallBotGui
{
    public class Teams
    {
        public List<Player> Team1 = new();
        public List<Player> Team2 = new();
        public List<Player> Team3 = new();
        public List<Player> Team4 = new();
    }
    public class Player
    {
        public Player(long id, string name, string firstName, string normalName, bool isFemale) { 
            this.id = id;
            this.name = name;
            this.firstName = firstName;
            this.normalName = normalName;
            this.isFemale = isFemale;
        }
        public long id { get; set; }  // уникальный код игрока
        public string name { get; set; }  // имя линка
        public string firstName { get; set; } // Имя игрока

        public string normalName { get; set; } // Нормальное имя игрока

        public int rating { get; set; } // Открыгый рейтинг игрока,для рейтинговых игр

        public string ratingRequestDate { get; set; } // Дата запроса рейтинга

        public int group { get; set; } = 2;// Группа , по уровню игры

        /* 1 самые сильные
           2 средние
           3 начинающие
        */

        public bool isFemale = false;
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

    public class CarStops
    {
        public string name { get; set; }

        public string link { get; set; }

        public int minBefore { get; set; }


        public CarStops(string name, string link, int minBefore)
        {
            this.name = name;
            this.link = link;
            this.minBefore = minBefore;
        }
       
    }
    public class Car
    {
        public long idPlayer { get; set;  } // хозяин машины
        public string name { get; set; }   // nik
        public string firstName { get; set; } // Имя
        public int placeCount { get; set; } = 2; // количество мест

        public BindingList<CarStops> carStops { get; set; } = new();    // где может забрать людей

        public Car(long idPlayer, string name, string firstName, int placeCount )
        {
            this.idPlayer = idPlayer;
            this.name = name;
            this.firstName = firstName;
            this.placeCount = placeCount;
        }
    }
    public class OccupiedPlace
    {
        public long idPlayer { get; set; } // кто занял место
        public long idCarOwner { get; set; } // у кого в машине

        public int stopIdx { get; set; } = 1; // Номер остановки на которой нужно его забрать 

        public string? nickname { get; set; }  // имя пользователя телеграм
        public string? firstName { get; set; } // Имя

        public OccupiedPlace(long idPlayer, long idCarOwner, int stopIdx, string? nickname, string? firstName)
        {
            this.idPlayer = idPlayer;
            this.idCarOwner = idCarOwner;
            this.stopIdx = stopIdx;
            this.nickname = nickname;
            this.firstName = firstName;
        }
    }

    public class Poll
    {
        public int maxPlayersCount { get; set; } = 14; // Максимальное количество игроков
        public bool approved { get; set; } = true;
        public string date { get; set; }    // дата игры
        public string question { get; set; } // текст опроса

        public int idMessage { get; set; }
        public string idPoll { get; set; }   // код опроса

        public int idCarsMessage { get; set; } = -1; // код сообщения о доступных машинах

        public List<long> idleDrivers { get; set; } = new(); // список водителей кто не может сегодня подвозить

        public VolleybollGame curGame { get; set; }

        public List<OccupiedPlace> occupiedPlaces { get; set; } = new List<OccupiedPlace>(); // доставка, брони

       
        public bool isTimeToSendBeforeGameInvite(DateTime currentTime)
        {
            return (currentTime.Minute == curGame.GameStartMinute
                    && currentTime.Hour == curGame.GameStartHour - 2); // 1 час это смещение часового пояса и ещё один час это за сколько предупреждать
                       
        }
        public Poll(string idPoll, string date, string question, int idMessage, VolleybollGame curGame) {
            this.idPoll = idPoll;
            this.question = question;
            this.idMessage = idMessage;
            this.curGame = curGame;

            if (date == string.Empty)
            {
                this.date = GetGameDateFromQuestion(question);
            } else
            {
              this.date = date;
            }
          
        }

        public List<PlayerVote> playrsList { get; set; } = new();

        // Возвращаем True если удалился игрок из первых maxPlayersCount
        public bool DeletePlayerFromList(long idPlayer)
        {
            int index = this.playrsList.FindIndex(player => player.id == idPlayer);

            if (index != -1)
            {
                // Удаляем игрока из списка
                playrsList.RemoveAt(index);

                // Проверяем, был ли удален игрок из первых maxPlayersCount
                return index < maxPlayersCount;
            }

            // Игрок с указанным Id не найден
            return false;

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

        internal bool isTimeToSendAfterGameSurvey(DateTime curTime)
        {
            throw new NotImplementedException();
        }
    }
    public class State
    {
        public List<Poll> pollList = new();
        public BindingList<Car> carList = new();
        public List<DislikedTeammates> dislikedTeammates = new();
        public List<string> spamStopWords = new();

        public Poll AddNewPoll(string idPoll, string date, string question, int messageId, VolleybollGame? curGame)
        {

            Poll oldPoll = pollList.FirstOrDefault(poll => poll.idPoll == idPoll);
            if (oldPoll == null)
            {
                var newPoll = new Poll(idPoll, date, question, messageId, curGame);
                pollList.Add(newPoll);
                return newPoll;
            }
           
            return oldPoll;

        }
         
         
     /*   public bool RemovePoll(string idPoll, string date, string question) {
            
            var newPoll = new Poll(idPoll, date, question);
            if (!pollList.Contains(newPoll))
            {
                pollList.Remove(newPoll);
                return true;
            }
            return false;
        }*/
     

    }
}
