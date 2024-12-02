using Telegram.Bot;
using System.Globalization;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot.Polling;
using System.Text;
using Telegram.Bot.Types.ReplyMarkups;
using Newtonsoft.Json;


namespace BallBotGui
{
    internal class TelegramConnector
    {
        private readonly TelegramBotClient botClient;
        private readonly StateManager stateManager;
        private Form1 curForm;

        private readonly string chatId = Properties.Settings.Default.chatId;
        private  System.Timers.Timer timerFirst = new System.Timers.Timer();

        public TelegramConnector(TelegramBotClient botClient, StateManager stateManager, Form1 form)
        {
            this.botClient = botClient;
            this.stateManager = stateManager;
            this.curForm = form;

            stateManager.LoadState();
            stateManager.LoadPlayers();

            ReadAllUpdates();
            /*****************************/

            

        }

        struct StopInfo
        {
            public long DriverCode;
            public int StopNumber;
            public int SequentialNumber;

            public StopInfo(long driverCode, int stopNumber, int sequentialNumber)
            {
                DriverCode = driverCode;
                StopNumber = stopNumber;
                SequentialNumber = sequentialNumber;
            }
        }

        private void StartReadingMessage()
        {
            ReceiverOptions receiverOptions = new()
            {
                AllowedUpdates = Array.Empty<UpdateType>() // receive all update types except ChatMember related updates
            };

            botClient.OnError += OnError;
            /* botClient.OnMessage += OnMessage;*/
            botClient.OnUpdate += OnUpdate;

            /*    botClient.StartReceiving(
                        updateHandler: HandleUpdateAsync,
                        pollingErrorHandler: HandlePollingErrorAsync,
                        receiverOptions: receiverOptions,
                        cancellationToken: cts.Token
                    );
            }
            */

            /*async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)*/
            
        }
       
        async Task OnUpdate(Update update)
        {
            var needToSave = OnNewUpdate(update);
            if (needToSave)
            {
                stateManager.SaveState();
                curForm.refreshGrids();
            }
        }
        async Task OnError(Exception exception, HandleErrorSource source)
        {
                Console.WriteLine(exception); // just dump the exception to the console
        }

        /* public void startGeneratePoll()
         {

             TriggerFunction3Poll();

         }*/

        /*  public async void TriggerFunction3Poll()
         {
             DateTime today = DateTime.Today;


             // Вторник
             await createOnePoll(GetNextWeekday(today, DayOfWeek.Tuesday));
             // Пятница
             await createOnePoll(GetNextWeekday(today.AddDays(4), DayOfWeek.Friday));
             // Воскресенье
             await createOnePoll(GetNextWeekday(today.AddDays(4), DayOfWeek.Sunday));

             timerFirst.Stop();
             return;
         }*/

        public async Task createOnePoll(DateTime curDay, VolleybollGame curGame)
        {
           /* string formattedDate = curDay.ToString("dddd, dd.MM", new CultureInfo("ru-RU"));

            // Модифицируем строку, чтобы первая буква дня недели была заглавной
            formattedDate = formattedDate.ToUpper();
            string curQuest = formattedDate + "! " + Properties.Settings.Default.pollQuestion;
*/
            string curQuest = curGame.GetQuest(curDay);
            var poll = await botClient.SendPoll(
                            chatId: chatId,
                            question: curQuest,

                            options: new InputPollOption[]
                                {
                                    Properties.Settings.Default.mainQuestion,
                                    Properties.Settings.Default.questSkip
                                 },
                            allowsMultipleAnswers: false,
                            isAnonymous: false
                        );


            // сохранение опроса в статусе
            stateManager.state.AddNewPoll(poll.Poll.Id, curDay.ToString("dd.MM", new CultureInfo("ru-RU")), curQuest, poll.MessageId, curGame);
            stateManager.SaveState();
            await Task.Delay(60000); // ждем минуту, чтобы не прыгал чат
            // Закрепление опроса
            await botClient.PinChatMessage(
                        chatId: chatId,
                        messageId: poll.MessageId
                    );
        }



        /// <summary>
        /// Создать один опрос
        /// </summary>
        /// <param name="curDay">Дата на которую создаем опрос</param>
        /// <returns></returns>
        public async Task createOnePoll(DateTime curDay)
        {
            string formattedDate = curDay.ToString("dddd, dd.MM", new CultureInfo("ru-RU"));
            // Модифицируем строку, чтобы первая буква дня недели была заглавной
            formattedDate = formattedDate.ToUpper();
            string curQuest = formattedDate + "! " + Properties.Settings.Default.pollQuestion;
            var poll = await botClient.SendPoll(
                            chatId: chatId, 
                            question: curQuest,
                            options: new InputPollOption[]
                                {
                                    Properties.Settings.Default.mainQuestion,
                                    Properties.Settings.Default.questSkip
                            },
                            allowsMultipleAnswers: false,
                            isAnonymous: false
                        );
            

            // сохранение опроса в статусе
            stateManager.state.AddNewPoll(poll.Poll.Id, curDay.ToString("dd.MM", new CultureInfo("ru-RU")), curQuest, poll.MessageId, null);
            stateManager.SaveState();
            await Task.Delay(60000); // ждем минуту, чтобы не прыгал чат
            // Закрепление опроса
            await botClient.PinChatMessage(
                        chatId: chatId,
                        messageId: poll.MessageId
                    );
        }

      
/*
        private DateTime GetNextWeekday(DateTime currentDate, DayOfWeek targetDay)
        {
            int daysUntilTargetDay = ((int)targetDay - (int)currentDate.DayOfWeek + 7) % 7;
            return currentDate.AddDays(daysUntilTargetDay);
        }*/

        public async void ReadAllUpdates()
        {
            var offset = 0;
            var preOffset = 0;
            while (true)
            {
                try { 
                    offset = await ReadMessages(offset);
                } catch ( Exception e )
                {

                }

                if (preOffset == offset) {
                    break;
                } else {
                    preOffset = offset;
                }

                // await Task.Delay(15000);
            }

            StartReadingMessage();
        }

        public async Task<int> ReadMessages(int offset)
        {

            IEnumerable<UpdateType> allowedUpdatesValue = new List<UpdateType>
                {
                        UpdateType.Poll,
                        UpdateType.PollAnswer,
                
                
                        UpdateType.CallbackQuery
                        
                        // Добавьте другие типы обновлений, которые вам нужны
                };


            var updates = await botClient.GetUpdates(offset  
                 , allowedUpdates: allowedUpdatesValue
                 );
            var needToSave = false;

            foreach (var update in updates)
            {
                needToSave = OnNewUpdate(update);
                offset = update.Id + 1;
            }
            if (needToSave)
            {
                stateManager.SaveState();
            }
            
            return offset;

        }

       /* private void onNewPollUpdate(Update update)
        {
            stateManager.state.AddNewPoll(update.Poll.Id, string.Empty, update.Poll.Question);
        }*/

        private void onNewPollAnswer(Update update)
        {
            if (update.PollAnswer != null)
            {
                if (update.PollAnswer.OptionIds.Length == 0)
                {
                    // снятие голоса
                    if( stateManager.RemoteVote(update.PollAnswer.PollId, update.PollAnswer.User.Id) )
                    {
                        // если мы сняли игрока то идем дальше
                        DateTime curTime = DateTime.Now;
                        // после объявления состава до момента игры
                        if(curTime.Hour >= 7 && curTime.Hour <= 19)
                        {
                            inviteNextPlayer(update.PollAnswer.PollId, update.PollAnswer.User);
                        }

                        removeFromCars(update.PollAnswer.PollId, update.PollAnswer.User.Id);
                    }
                    
                }
                else
                {
                    bool containsZero = Array.Exists(update.PollAnswer.OptionIds, element => element == 0);
                    if (containsZero)
                    {
                        var user = update.PollAnswer.User;
                        stateManager.AddVote(update.PollAnswer.PollId, user.Id, user.Username, user.FirstName, update.Id);
                     
                    }
                }
            }
        }

        private void removeFromCars(string idPoll, long idPlayer)
        {
            // удаляем как пасажира
            var curPoll = stateManager.state.pollList.FirstOrDefault(x => x.idPoll == idPoll);
            if (curPoll != null)
            {
                Poll? todayApprovedGamePoll = stateManager.getTodayApprovedGamePoll();
                if (todayApprovedGamePoll != null)
                {
                    // сегодняшний опрос и снялся человек и было уже сообщение
                    if(curPoll.idPoll == todayApprovedGamePoll.idPoll && todayApprovedGamePoll.idCarsMessage > 0) { 
                        freeSeat(idPlayer, todayApprovedGamePoll);
                        // обновляем сообщение с машинами
                        sendCarsMessage(todayApprovedGamePoll);
                    }
                }
                
            }

        }

        private async void inviteNextPlayer(string idPoll, User oldUser)
        {
            // если сегодня игровой день
            // и у нас время после объявления состава игроков ( проверили ранее перед функцией )
            // и у нас игроков 14 как минимум осталось
            // то отправляем приглашение 14-ому игроку


            var now = DateTime.Now.ToString("dd.MM");

            Poll? poll = this.stateManager.state.pollList.FirstOrDefault(x => 
                    x.date == now && x.approved && x.idPoll == idPoll && x.playrsList.Count >= 14 );

            if( poll != null)
            {
                PlayerVote voter = poll.playrsList[13]; // берем последнего игрока
                string message = $"Снялся @{oldUser.Username}. В игру вступает @{voter.name} {voter.firstName}!";

                await botClient.SendMessage(chatId, message);
            }

        }

        private  bool OnNewUpdate(Update update)
        {
           /* ВО ВРЕМЯ ТЕСТА МОЖНО ВКЛЮЧАТЬ
            * f (update.Type == UpdateType.Poll)
            {
                if (update.Poll != null && update.Poll.Question.Contains("Волейбол в ЗАЛЕ"))
                {
                    onNewPollUpdate(update);
                    return true;
                }

            }*/
           if (update.Type == UpdateType.CallbackQuery)
            {
                if(update != null && update.CallbackQuery != null && update.CallbackQuery.Data != null && update.CallbackQuery.Data.StartsWith("takeaseat:"))
                {
                    takeSeat(update);
                }
                return false;
            }

            if (update.Type == UpdateType.Message)
            {
                var d = update;
                if(update.Message != null && update.Message.Text == "#teams")
                {
                    suggectTeams(update);
                }

                if (update.Message != null && (update.Message.Text == "#mystat" || update.Message.Text == "#mystats"))
                {
                    writePlayerStat(update);
                }
                // TODO проверить
                if (update.Message != null &&  update.Message.Type == MessageType.NewChatMembers && update.Message.NewChatMembers != null )
                {
                    foreach (var member in update.Message.NewChatMembers)
                    {
                       sendWellcomeMessage(member);
                    }
                }
                return false;
            }
            if (update.Type == UpdateType.PollAnswer)
            {
                onNewPollAnswer(update);
                return true;
            }

            
            return false;
        }

        private async void writePlayerStat(Update update)
        {
            if (update != null)
            {
                var statist = new StatisticsManager();
                string message = statist.getPlayerStat(update);
                await botClient.SendMessage(chatId, message);

            }
            
        }

        private void takeSeat(Update update)
        {
            Poll? todayApprovedGamePoll = stateManager.getTodayApprovedGamePoll();
            if (todayApprovedGamePoll != null)  // сегодня ест игра....
            {
                var idCurUser = update.CallbackQuery.From.Id;

                if (update.CallbackQuery.Data.StartsWith("takeaseat:0:0"))
                {
                    // если просящий есть среди тех кто уже занял место то удаляем его 
                    if (freeSeat(idCurUser, todayApprovedGamePoll) > 0)
                    {
                        stateManager.SaveState();
                        // обновляем сообщение с машинами
                        sendCarsMessage(todayApprovedGamePoll);
                    }

                    return;
                }



                // проверяем что у нас трока в проавильно формате
                var values = update.CallbackQuery.Data.Split(":");
                if (values.Length == 3 )
                {
                    long driverId = 0;
                    int stopIdx = 0;

                    var first14Ids = todayApprovedGamePoll.playrsList.OrderBy(player => player.idVote)
                                      .Take(14)
                                      .Select(player => player.id);

                    // проверяем что остановка это число
                    if (!int.TryParse(values[2], out stopIdx) || !long.TryParse(values[1], out driverId)) { return; }

                    // проверяем что тот что просится тоже срежи первых 14
                    if (update.CallbackQuery.From != null && idCurUser > 0)
                    {
                        if (!first14Ids.Contains(idCurUser)) { return; }
                    }

                    // проверяем что водитель есть среди первых 14
                    if (!first14Ids.Contains(driverId)) { return; }

                    

                    // проверяем что данного водителя ещё есть места, без учета просящегося
                    // Подсчет количества записей для конкретного пользователя
                    int count = todayApprovedGamePoll.occupiedPlaces.Count(place => place.idCarOwner == driverId);
                    Car foundCar = stateManager.state.carList.FirstOrDefault(car => car.idPlayer == driverId);
                    if (foundCar == null || count >= foundCar.placeCount)
                    {
                        return;
                    }

                    // проверяем что он ещё не занял место в данной точке
                    if (todayApprovedGamePoll.occupiedPlaces.Any(o => o.idCarOwner == driverId && o.stopIdx == stopIdx && o.idPlayer == idCurUser) ) {
                        return;
                    }
                 

                    // если просящий есть среди тех кто уже занял место то удаляем его 
                    freeSeat(idCurUser, todayApprovedGamePoll);

                    // записываем просящегося 
                    var newTake = new OccupiedPlace(idCurUser, driverId, stopIdx, nickname: update.CallbackQuery.From.Username, update.CallbackQuery.From.FirstName);
                    todayApprovedGamePoll.occupiedPlaces.Add(newTake);
                    stateManager.SaveState();

                    // обновляем сообщение с машинами
                    sendCarsMessage(todayApprovedGamePoll);
                }



            }
        }

        private static int freeSeat(long idMember, Poll todayApprovedGamePoll)
        {
            // Удаление объектов, относящихся к определенному игроку
            return todayApprovedGamePoll.occupiedPlaces.RemoveAll(place => place.idPlayer == idMember);
        }

        private async void sendWellcomeMessage(User? member)
        {
            if(member != null)
            {
                var inviteMessage = BallBotGui.Properties.Settings.Default.inviteMessage;
                inviteMessage = inviteMessage.Replace("@Player", member.FirstName);

                await botClient.SendMessage(chatId, inviteMessage);
            }
        }

        internal async void suggectTeams(Update update)
        {

            var teams = stateManager.Take2Teams(update);
            if (teams.Team1.Count > 5 && teams.Team2.Count > 5)
            {
               

                string team1Players = string.Join("\n", teams.Team1.Select(p => $"@{p.name} {p.firstName}"));
                string team2Players = string.Join("\n", teams.Team2.Select(p => $"@{p.name} {p.firstName}"));

                string message = $"Предлагаются следующие составы команд:\n\nКоманда 1:\n{team1Players}\n\nКоманда 2:\n{team2Players}";
                await botClient.SendMessage(chatId, message);
            }
        }

        internal void ArchPolls()
        {
            stateManager.ArchPolls(botClient);
            foreach (var poll in stateManager.state.pollList.Where(p => p.idCarsMessage > 0).ToList())
            {
                deleteCarMessage(poll);

            }
        }

        internal async void sendInvitation(Poll? todayApprovedGamePoll)
        {
            int inviteCount = Math.Min(todayApprovedGamePoll.playrsList.Count, 14);
            await botClient.SendMessage(chatId, "Сегодня на игру приглашаются: ");
            for (int i = 0; i < inviteCount; i++)
            {
                PlayerVote voter = todayApprovedGamePoll.playrsList[i];
                string message = $"Игрок # {i + 1} @{voter.name} {voter.firstName}";

                await botClient.SendMessage(chatId, message);
            }
        }

        internal async void sendCarsMessage(Poll todayApprovedGamePoll)
        {

            var stops = new List<StopInfo>();
 
            // Получаем id игроков из первых 14 в голосовании
            var first14Ids = todayApprovedGamePoll.playrsList.OrderBy(player => player.idVote)
                                        .Take(14)
                                        .Select(player => player.id);

            // Выбираем машины, у которых idPlayer есть в first14Ids
            var carsWithOwnersInFirst14 = stateManager.state.carList.Where(car => first14Ids.Contains(car.idPlayer)).ToList();

            if (carsWithOwnersInFirst14.Count > 0)
            {
                StringBuilder messageBuilder = new StringBuilder();

                messageBuilder.AppendLine("Сегодня нам помогают добраться:");

                int stopIdx = 1;
                foreach (var car in carsWithOwnersInFirst14)
                {
                   var owner = todayApprovedGamePoll.playrsList.FirstOrDefault(player => player.id == car.idPlayer);
                    if (owner != null)
                    {
                        messageBuilder.AppendLine($"🚗 <b> {owner.firstName} @{owner.name}, мест в машине: {car.placeCount}</b>");
                        stopIdx = addStops(todayApprovedGamePoll, stops, messageBuilder, stopIdx, car, owner);

                        messageBuilder.AppendLine(); // Добавляем пустую строку между информацией о машинах
                    }
                }
                string time = DateTime.Now.AddHours(1).ToString("HH:mm:ss"); // Получаем текущее время
                messageBuilder.AppendLine($"{time}");
                messageBuilder.AppendLine($"На какой точке вас забрать?");
                string message = messageBuilder.ToString();
                var keyboard = addButtons(stops);

                if (todayApprovedGamePoll.idCarsMessage > 0)
                {
                    bool success = false;
                    int retryCount = 0;
                    while (!success && retryCount < 3) // Попытаемся три раза
                    {
                        try
                        {
                            var carInfoMessage = await botClient.EditMessageText(
                                chatId: chatId, 
                                messageId: todayApprovedGamePoll.idCarsMessage, 
                                text: message, 
                                parseMode: ParseMode.Html,
                                linkPreviewOptions: true, 
                                replyMarkup: keyboard);
                            success = true;
                        }
                        catch (Exception ex)
                        {
                            retryCount++;
                            await Task.Delay(60000); // Подождем 1 минуту перед следующей попыткой
                        }
                    }

                   
                }
                else
                {
                    var carInfoMessage = await botClient.SendMessage(
                        chatId: chatId, 
                        text: message, 
                        parseMode: ParseMode.Html,
                        linkPreviewOptions: true,
                        replyMarkup: keyboard);
                    todayApprovedGamePoll.idCarsMessage = carInfoMessage.MessageId;
                    
                    // Закрепление опроса по машинам
                    await botClient.PinChatMessage(
                                chatId: chatId,
                                messageId: carInfoMessage.MessageId
                            );
                    stateManager.SaveState();
                }
            }
            else
            {
                if (todayApprovedGamePoll.idCarsMessage > 0)
                {
                    // у нас нет машин, но похоже раньше они были и нужно удалить сообщение
                    deleteCarMessage(todayApprovedGamePoll);
                    stateManager.SaveState();
                }
            }
        }

        public async void startScoreBoard()
        {
            string webAppUrl = "https://your-web-app-url.com";
       
        }

        public async void deleteCarMessage(Poll curPoll)
        {
            await botClient.DeleteMessage(chatId, curPoll.idCarsMessage);
            curPoll.idCarsMessage = 0;
        }

        private static int addStops(Poll todayApprovedGamePoll, List<StopInfo> stops, StringBuilder messageBuilder, int stopIdx, Car? car, PlayerVote? owner)
        {
            if (car.carStops.Any())
            {
                messageBuilder.AppendLine("<b>Остановки:</b>");
                var curDriverStopIdx = 1;

                foreach (var stop in car.carStops)
                {
                    var newPoint = new StopInfo(owner.id, curDriverStopIdx, stopIdx);
                    stops.Add(newPoint);

                    TimeSpan gameTime = new TimeSpan(20, 0, 0); // Время игры: 20:00
                    int minutesToSubtract = 30;

                    if (stop.minBefore > 0 )
                    {
                        minutesToSubtract = stop.minBefore;

                    }

                    TimeSpan adjustedTime = gameTime.Subtract(TimeSpan.FromMinutes(minutesToSubtract));
                    // Преобразуем в строку в 24-часовом формате
                    string adjustedTimeString = adjustedTime.ToString(@"hh\:mm");

                    var stopName = stop.name.Replace("@Time", adjustedTimeString);

                    if (stop.link == null)
                    {
                        messageBuilder.AppendLine($" {stopIdx}: {stopName}");
                    }
                    else
                    {
                        messageBuilder.AppendLine($" {stopIdx}: {stopName} - <a href=\"{stop.link}\">тут</a>");
                    }

                    // пишет пассажиров на данной точке
                    // находим список пассажиров на данной точке 
                    // Фильтрация списка по idCarOwner и stopIdx
                    var filteredPlaces = todayApprovedGamePoll.occupiedPlaces.Where(place => place.idCarOwner == owner.id && place.stopIdx == curDriverStopIdx).ToList();

                    // Вывод результатов
                    if (filteredPlaces.Count > 0)
                    {
                 
                        foreach (var place in filteredPlaces)
                        {
                            messageBuilder.AppendLine($" . 🙋‍ {place.firstName} @{place.nickname}");
                        }
                        
                    }

                    stopIdx++;
                    curDriverStopIdx++;
                }
            }
            else
            {
                messageBuilder.AppendLine("У машины нет указанных остановок.");
            }

            return stopIdx;
        }

        private InlineKeyboardMarkup addButtons(List<StopInfo> stops)
        {
            var buttons = new List<InlineKeyboardButton[]>();
            for (int i = 0; i < stops.Count; i += 4)
            {
                var rowButtons = stops.Skip(i).Take(4)
                                    .Select(stop => InlineKeyboardButton.WithCallbackData(stop.SequentialNumber.ToString(), $"takeaseat:{stop.DriverCode}:{stop.StopNumber}"))
                                    .ToArray();
                buttons.Add(rowButtons);
            }

            // добовляем последнюю кнопку
            var byMySelf = InlineKeyboardButton.WithCallbackData("Добираюсь самостоятельно", "takeaseat:0:0");
            buttons.Add(new[] { byMySelf });
            return new InlineKeyboardMarkup(buttons);

        }

        internal async Task sendTestMessageAsync()
        {
            // ID пользователя, которого вы хотите упомянуть
            int userId = 348518374;
         //string message = $"<a href=\"tg://user?id={userId}\"> inline mention of a user</a>";
            string username = "[" + "Sergey" + "](tg://user?id=" + userId + ")";
            string message = $"Hello {username}";
            try
            {
                var testMessage = await botClient.SendMessage(chatId: chatId, text: message, parseMode: ParseMode.Markdown);
            }
            catch (Exception ex)
            {
                var dd = ex;
                throw;
            }
           

        }

        internal List<PlayerVote> askNewPlayers(Poll todayApprovedGamePoll)
        {
            var previousPlayerIds = new HashSet<long>();


            // Получаем все архивные файлы
            var archiveFolderName = "Arch";
            var archiveFolderPath = Path.Combine(Directory.GetCurrentDirectory(), archiveFolderName);
            var files = Directory.GetFiles(archiveFolderPath, "Arch*.json");

            foreach (var file in files)
            {
                var json = System.IO.File.ReadAllText(file);
                var poll = JsonConvert.DeserializeObject<Poll>(json);
              
                if(poll != null && poll.approved)
                {
                    foreach (var player in poll.playrsList)
                    {
                        previousPlayerIds.Add(player.id);
                    }
                }
               
            }

            // Находим новых игроков

            var curPlayersList = todayApprovedGamePoll.playrsList.Take(14);
            var newPlayers = curPlayersList.Where(p => !previousPlayerIds.Contains(p.id)).ToList();
            return newPlayers;
        }

        internal async Task askAboutFirstGameAsync(PlayerVote player)
        {
            string message = $"Добрый день, @{player.name} {player.firstName}. Вы сегодны первый раз с нами играте! Как добираетесь? ";

            await botClient.SendMessage(chatId, message);
        }
    }
    
}
