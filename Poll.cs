using System.Text.RegularExpressions;


namespace BallBotGui
{
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

        public int ratingMessageId { get; set; } // id рейтингового сообщения

        public List<OccupiedPlace> occupiedPlaces { get; set; } = new List<OccupiedPlace>(); // доставка, брони

        /********************************************************************************************/
        // Debounce config
        private const int UpdateDelaySeconds = 3;
        private readonly object _notifyLock = new();
        private bool _updateScheduled = false;

        // External subscriber (UI, telegram sender, etc.)
        // Subscriber should marshal to UI thread if needed.
        public Action<Poll>? PlayersUpdated { get; set; }

        // Call this after you change playrsList (AddPlayerToList, DeletePlayerFromList)
        private void SchedulePlayersUpdate()
        {
            lock (_notifyLock)
            {
                if (_updateScheduled) return; // already scheduled, coalesce events
                _updateScheduled = true;
                _ = DelayedNotifyAsync(); // fire-and-forget
            }
        }

        private async Task DelayedNotifyAsync()
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(UpdateDelaySeconds)).ConfigureAwait(false);

                lock (_notifyLock)
                {
                    // create a snapshot to pass to subscribers
                    
                    _updateScheduled = false;
                }

                // Invoke subscriber on threadpool. Subscriber must handle UI marshaling.
                PlayersUpdated?.Invoke(this);
            }
            catch
            {
                // ignore exceptions from delay/notify to keep debounce resilient
                lock (_notifyLock)
                {
                    _updateScheduled = false;
                }
            }
        }
        /********************************************************************************************/


        public bool isTimeToSendBeforeGameInvite(DateTime currentTime)
        {
            return (currentTime.Minute == curGame.GameStartMinute
                    && currentTime.Hour == curGame.GameStartHour - 2); // 1 час это смещение часового пояса и ещё один час это за сколько предупреждать
                       
        }
        public Poll(string idPoll, string date, string question, int idMessage, VolleybollGame curGame, int ratingMessageId) {
            this.idPoll = idPoll;
            this.question = question;
            this.idMessage = idMessage;
            this.curGame = curGame;
            this.ratingMessageId = ratingMessageId;

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

                if (curGame.RatingGame)
                {
                    // schedule coalesced update
                    SchedulePlayersUpdate();
                }

                // Проверяем, был ли удален игрок из первых maxPlayersCount
                return index < maxPlayersCount;
            }

            // Игрок с указанным Id не найден
            return false;

        }

        public PlayerVote AddPlayerToList(long id, string name, string firstName, long idVote, int rating)
        {
            PlayerVote curPerson = playrsList.FirstOrDefault(p => p.id == id);
            if (curPerson != null)
            {
                return curPerson;
            }
            else
            {
                var newPlayer = new PlayerVote(id, name, firstName, idVote, rating);
              
                if (curGame.RatingGame 
                    && rating > 0 
                    && GetGameDate().ToString("ddMMyyyy") != DateTime.Now.ToString("ddMMyyyy"))
                {
                    int newPlayerRating = rating;
                    
                        // Т.е. это рейтингованный участник, он должен быть до всех игроков с нулевым рейтингом

                        // Найти последнего игрока рейтингованного с такимже рейтингом или сильнее
                        int lastSameRatingIdx = playrsList
                                .FindLastIndex(p => (p is PlayerVote pv && pv.rating <= newPlayerRating && pv.rating != 0 ));

                        playrsList.Insert(lastSameRatingIdx + 1, newPlayer); // если -1 ,то в нуль запишем, а если не -1 то в правильное место
                } else
                {
                    playrsList.Add(newPlayer);
                }

                if(curGame.RatingGame){
                    // schedule coalesced update
                    SchedulePlayersUpdate();
                }

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
}
