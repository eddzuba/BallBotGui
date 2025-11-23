using Newtonsoft.Json;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static System.Windows.Forms.AxHost;



namespace BallBotGui
{
    internal class TelegramConnector
    {
        // Глобальная переменная для ID администратора
        private const long AdminId = 245566701;

        private readonly TelegramBotClient botClient;
        private readonly StateManager stateManager;
        private Form1 curForm;

        private readonly string chatId = Properties.Settings.Default.chatId;
        private System.Timers.Timer timerFirst = new System.Timers.Timer();

        public TelegramConnector(TelegramBotClient botClient, StateManager stateManager, Form1 form)
        {
            this.botClient = botClient;
            this.stateManager = stateManager;
            this.curForm = form;

            stateManager.LoadState();
            stateManager.LoadPlayers();

            ReadAllUpdates();
            /*****************************/
            /* подписываем на изменения рейтинговых игр */
            foreach (var curPoll in stateManager.state.pollList)
            {
                if (curPoll.curGame != null && curPoll.curGame.RatingGame)
                {
                    curPoll.PlayersUpdated = curPoll =>
                    {
                        updateRatingGameListMessage(curPoll);
                    };
                }
            }



        }

        struct StopInfo
        {
            public string IdPoll;
            public long DriverCode;
            public int StopNumber;
            public int SequentialNumber;

            public StopInfo(string idPoll, long driverCode, int stopNumber, int sequentialNumber)
            {
                IdPoll = idPoll;
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
            // botClient.OnMessage += OnMessage;
            botClient.OnUpdate += OnUpdate;



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


        public async Task createOnePoll(DateTime curDay, VolleybollGame curGame)
        {
            int idRatingMsg = 0;

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

            var gameTime = curDay.AddDays(curGame.PullBeforeDay);

            if (curGame != null && curGame.RatingGame)
            {
                // Создаем пустое сообщение с текстом-заглушкой для рейтингового списка
                var ratingMsg = await botClient.SendMessage(
                    chatId: chatId,
                    text: "Здесь появится рейтинговый список.\n" + curGame.Title,
                    parseMode: ParseMode.Html
                );
                idRatingMsg = ratingMsg.Id;
            }



            // сохранение опроса в статусе
            stateManager.state.AddNewPoll(poll.Poll.Id, gameTime.ToString("dd.MM", new CultureInfo("ru-RU")), curQuest, poll.MessageId, curGame, idRatingMsg);
            stateManager.SaveState();

            if (curGame != null && curGame.RatingGame)
            {
                var curPollInstance = stateManager.state.pollList.Find(p => p.idPoll == poll.Poll.Id);
                if (curPollInstance != null)
                {
                    curPollInstance.PlayersUpdated = curPoll =>
                    {
                        updateRatingGameListMessage(curPoll);
                    };
                }
            }

            await Task.Delay(30000); // ждем 30 секунд, чтобы не прыгал чат
            // Закрепление опроса
            await botClient.PinChatMessage(
                        chatId: chatId,
                        messageId: poll.MessageId
                    );
        }

        private async void updateRatingGameListMessage(Poll curPoll)
        {
            try
            {
                if (curPoll == null) return;

                var sb = new StringBuilder();
                sb.AppendLine("<b>Рейтинговый список</b>");
                if (curPoll.curGame != null && !string.IsNullOrWhiteSpace(curPoll.curGame.Title))
                {
                    sb.AppendLine(curPoll.curGame.Title.Replace("@GameDayName", curPoll.date));
                }
                sb.AppendLine();

                var players = curPoll.playrsList ?? new List<PlayerVote>();
                if (!players.Any())
                {
                    sb.AppendLine("Список пока пуст.");
                }
                else
                {
                    int idx = 1;
                    foreach (var p in players)
                    {
                        var statePlayer = stateManager.players.FirstOrDefault(pl => pl.id == p.id);
                        var displayName = !string.IsNullOrWhiteSpace(statePlayer?.normalName) ? statePlayer.normalName : p.firstName;
                        var ratingText = p.rating > 0 ? $"{GetLetterRating(p.rating)} ({p.rating})" : "—";

                        // Экранируем пользовательские строки для HTML
                        string nameHtml = System.Net.WebUtility.HtmlEncode(displayName);
                        string usernameHtml = System.Net.WebUtility.HtmlEncode(p.name);

                        sb.AppendLine($"{idx}. {nameHtml} @{usernameHtml}");
                        idx++;
                    }
                }

                sb.AppendLine();
                sb.AppendLine(DateTime.Now.ToString("HH:mm"));

                string text = sb.ToString();

                try
                {
                    if (curPoll.ratingMessageId > 0)
                    {
                        await botClient.EditMessageText(
                            chatId: chatId,
                            messageId: curPoll.ratingMessageId,
                            text: text,
                            parseMode: ParseMode.Html
                        );
                    }
                    else
                    {
                        var sent = await botClient.SendMessage(
                            chatId: chatId,
                            text: text,
                            parseMode: ParseMode.Html
                        );

                        if (sent != null)
                        {
                            curPoll.ratingMessageId = sent.MessageId;
                            stateManager.SaveState();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при обновлении сообщения рейтинга: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Критическая ошибка в updateRatingGameListMessage: {ex.Message}");
            }
        }



        /// <summary>
        /// Создать один опрос
        /// </summary>
        /// <param name="curDay">Дата на которую создаем опрос</param>
        /// <returns></returns>
        /* public async Task createOnePoll(DateTime curDay)
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
         }*/


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
                try
                {
                    offset = await ReadMessages(offset);
                }
                catch (Exception e)
                {

                }

                if (preOffset == offset)
                {
                    break;
                }
                else
                {
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
                        UpdateType.Message,
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
                    if (stateManager.RemoteVote(update.PollAnswer.PollId, update.PollAnswer.User.Id))
                    {
                        // если мы сняли игрока то идем дальше
                        DateTime curTime = DateTime.Now;
                        // после объявления состава до момента игры
                        if (curTime.Hour >= 7 && curTime.Hour <= 19)
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

                        // test

                        // botClient?.SendMessage(AdminId, user.Username + " проголосовал ЗА! PollId:" + update.PollAnswer.PollId.ToString());
                    }
                }
            }
        }

        private async void removeFromCars(string idPoll, long idPlayer)
        {
            try
            {
                // удаляем как пасажира
                var curPoll = stateManager.state.pollList.FirstOrDefault(x => x.idPoll == idPoll);
                if (curPoll != null)
                {
                    Poll? todayApprovedGamePoll = stateManager.getTodayApprovedGamePoll().FirstOrDefault(poll => poll.idPoll == idPoll);
                    if (todayApprovedGamePoll != null)
                    {
                        // сегодняшний опрос и снялся человек и было уже сообщение
                        if (curPoll.idPoll == todayApprovedGamePoll.idPoll && todayApprovedGamePoll.idCarsMessage > 0)
                        {
                            freeSeat(idPlayer, todayApprovedGamePoll);
                            // если есть машина которую ведет удаляемый игрок то нужно ее удалить и все пассажирма написать сообщение
                            deleteCar(idPlayer, todayApprovedGamePoll);
                            // обновляем сообщение с машинами
                            await sendCarsMessage(todayApprovedGamePoll);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в removeFromCars: {ex.Message}");
            }

        }

        private async void deleteCar(long ownerId, Poll todayApprovedGamePoll)
        {
            try
            {


                // Найти машину владельца
                var car = stateManager.state.carList.FirstOrDefault(c => c.idPlayer == ownerId);
                if (car == null) return;

                // Найти список пассажиров, записанных к этой машине (по сегодняшнему опросу)
                var passengerPlaces = todayApprovedGamePoll.occupiedPlaces
                    .Where(op => op.idCarOwner == ownerId)
                    .ToList();

                // Сообщение для пассажиров
                string passengerMessage = $"Внимание: машина, в которую вы записывались, больше недоступна — поездка отменена. Пожалуйста, найдите другой вариант.";
                // Уведомляем пассажиров
                foreach (var place in passengerPlaces)
                {
                    try
                    {
                        await botClient.SendMessage(place.idPlayer, passengerMessage);
                    }
                    catch
                    {
                        // Игнорируем ошибки отправки отдельным пользователям
                    }
                }

                // Удаляем записи о пассажирах для этой машины из состояния
                todayApprovedGamePoll.occupiedPlaces.RemoveAll(op => op.idCarOwner == ownerId);
            }
            catch (Exception ex)
            {
                // просто гасим ошибку, плохо но пока так для надежности
            }


        }

        private async void inviteNextPlayer(string idPoll, User oldUser)
        {
            try
            {
                // если сегодня игровой день
                // и у нас время после объявления состава игроков ( проверили ранее перед функцией )
                // и у нас игроков maxGameSpots как минимум осталось
                // то отправляем приглашение maxPlayersCount-ому игроку




                var now = DateTime.Now.ToString("dd.MM");

                Poll? poll = this.stateManager.state.pollList.FirstOrDefault(x =>
                        x.date == now && x.approved && x.idPoll == idPoll);

                if (poll != null)
                {
                    string gameTime = $"{poll.curGame.GameStartHour}:{poll.curGame.GameStartMinute:D2}";
                    int maxGameSpots = poll.maxPlayersCount;
                    if (poll.playrsList.Count >= maxGameSpots)
                    {
                        PlayerVote voter = poll.playrsList[maxGameSpots - 1]; // берем последнего игрока
                        string message = $"Игра в {gameTime}. Снялся @{oldUser.Username}. В игру вступает @{voter.name} {voter.firstName}!";

                        await botClient.SendMessage(chatId, message);
                        await sendPlayerInvitation(poll, voter);

                        if (poll != null)
                        {
                            // сегодняшний опрос и снялся человек и было уже сообщение
                            if (poll.idCarsMessage > 0)
                            {
                                // обновляем сообщение с машинами
                                await sendCarsMessage(poll);
                            }
                        }

                    }
                    else
                    {
                        int freeSpots = maxGameSpots - poll.playrsList.Count;
                        string message = $"Игра в {gameTime}. Снялся @{oldUser.Username}. Свободных мест: {freeSpots} ";
                        await botClient.SendMessage(chatId, message);
                    }

                    if (poll?.playrsList.Count < 12)
                    {
                        string message = $"Игра в {gameTime}. После снятия  @{oldUser.Username} игроков осталось меньше 12. Штрафные санкции! ";
                        await botClient.SendMessage(chatId, message);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в inviteNextPlayer: {ex.Message}");
            }


        }

        private bool OnNewUpdate(Update update)
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
                if (update != null && update.CallbackQuery != null && update.CallbackQuery.Data != null)
                {
                    if (update.CallbackQuery.Data.StartsWith("takeaseat:"))
                    {
                        takeSeat(update);
                    }
                    else if (update.CallbackQuery.Data.StartsWith("vote|") || update.CallbackQuery.Data.StartsWith("submit|"))
                    {
                        HandleCallbackQuery(update.CallbackQuery);
                    }
                }
                return false;
            }

            if (update.Type == UpdateType.Message)
            {
                var d = update;

                if (update.Message != null)
                {
                    // Вызов обработки команды spam:... от админа
                    if (update.Message.Text != null && update.Message.Text.Trim().StartsWith("spam:"))
                    {
                        ProcessAdminSpamWordMessage(update);
                        return false;
                    }

                    // Вызов обработки команды rate:... от админа
                    if (update.Message.Text != null && update.Message.Text.Trim().StartsWith("rate:"))
                    {
                        ProcessAdminRateMessage(update);
                        return false;
                    }

                    if (update.Message.Chat.Type == ChatType.Private)
                    {
                        try
                        {
                            if (update.Message.Text?.Trim() == "/start")
                            {
                                sendDirectMessage(update.Message.From, "Приветствую, я просто БОТ. Я буду напоминать вам о волейболе!");
                            }

                        }
                        catch (Exception)
                        {


                        }

                    }
                    if (update.Message.Text?.Trim() == "#teams")
                    {
                        suggectTeams(update);
                    }

                    if (update.Message.Text?.Trim() == "#teams4")
                    {
                        suggect4Teams(update);
                    }

                    if ((update.Message?.Text == "#mystat" || update.Message?.Text == "#mystats" || update.Message?.Text == "/mystat"))
                    {
                        writePlayerStat(update);
                    }

                    if ((update.Message?.Text == "/rating"))
                    {
                        sendRatingMessage(update);
                    }

                    if ((update.Message?.Text == "/getrating"))
                    {
                        sendRatingRequestMessage(update);
                    }

                    if (update.Message?.Type == MessageType.NewChatMembers && update.Message.NewChatMembers != null)
                    {
                        foreach (var member in update.Message.NewChatMembers)
                        {
                            sendWellcomeMessage(member);
                        }
                    }
                }
                // Banned words
                if (update.Message?.Text != null && update.Message.Chat.Id.ToString() == chatId)
                {
                    BanUser(update);
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

        private async void sendRatingRequestMessage(Update update)
        {
            try
            {
                // Ensure update.Message and update.Message.Chat are not null before dereferencing
                if (update.Message?.Chat?.Id > 0)
                {
                    if (update.Message.Chat.Id.ToString() != chatId)
                    {
                        var (message, type, player) = stateManager.getRatingRequestStatusText(update);

                        if (type == "success" && player != null)
                        {
                            await RequestPlayerRating(player);
                        }

                        if (botClient != null)
                        {
                            await botClient.SendMessage(update.Message.Chat.Id, message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в sendRatingRequestMessage: {ex.Message}");
            }
        }

        // Заглушка для функции запроса рейтинга
        private async Task RequestPlayerRating(Player player)
        {
            // Используем глобальную переменную AdminId
            string message = $"Пользователь запросил рейтинг:\nНик: @{player.name}\nID: {player.id}";

            try
            {
                if (botClient != null)
                {
                    await botClient.SendMessage(AdminId, message);
                }
            }
            catch
            {
                // Игнорируем ошибки отправки
            }
        }

        private async void sendRatingMessage(Update update)
        {
            if (update.Message?.Chat?.Id > 0)
            {
                if (update.Message.Chat.Id.ToString() != chatId)
                {
                    string message = "Ваш рейтинг: " + stateManager.getPlayerRatingText(update) + ". ( Справочно: A - сильный, D - начинающий )";
                    try
                    {
                        await botClient.SendMessage(update.Message.Chat.Id, message);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при отправке рейтинга: {ex.Message}");
                    }
                }
            }
        }

        private void BanUser(Update update)
        {
            if (update.Message?.Text == null)
                return;

            string messageText = update.Message.Text.ToLowerInvariant();
            long messChatId = update.Message.Chat.Id;

            if (update.Message.From != null)
            {
                long messUserId = update.Message.From.Id;

                if (stateManager.state.spamStopWords.Any(word => messageText.Contains(word)))
                {
                    try
                    {
                        botClient.DeleteMessage(chatId, update.Message.MessageId);
                        botClient.BanChatMember(chatId, messUserId);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при бане пользователя: {ex.Message}");
                    }

                    // Используем глобальную переменную AdminId
                    string userName = update.Message.From.Username ?? "";
                    string firstName = update.Message.From.FirstName ?? "";
                    string lastName = update.Message.From.LastName ?? "";
                    string notify = $"Пользователь забанен: @{userName} ({firstName} {lastName}), id: {messUserId}";

                    try
                    {
                        botClient.SendMessage(AdminId, notify);
                    }
                    catch
                    {
                        // Игнорируем ошибку отправки админу
                    }
                }
            }
        }

        private async void sendDirectMessage(User? from, string message)
        {
            if (from != null)
            {
                try
                {
                    await botClient.SendMessage(from.Id, message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при отправке прямого сообщения: {ex.Message}");
                }
            }
        }

        private async void writePlayerStat(Update update)
        {
            try
            {
                if (update != null)
                {
                    var statist = new StatisticsManager();
                    string message = statist.getPlayerStat(update);
                    // await botClient.SendMessage(chatId, message);
                    if (update.Message?.Chat.Id > 0)
                    {
                        try
                        {
                            await botClient.SendMessage(update.Message.Chat.Id, message);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Ошибка при отправке статистики: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в writePlayerStat: {ex.Message}");
            }

        }

        private void takeSeat(Update update)
        {
            try
            {
                if (update.CallbackQuery?.Data == null)
                {
                    // Handle the case where Data is null, or simply return if it's not expected
                    return;
                }

                var todayApprovedGamePollList = stateManager.getTodayApprovedGamePoll();
                foreach (var todayApprovedGamePoll in todayApprovedGamePollList)
                {
                    if (todayApprovedGamePoll != null)  // сегодня есть игра....
                    {
                        // Разделяем строку по символу ':'
                        // проверяем что у нас строка в правильном формате
                        var values = update.CallbackQuery.Data.Split(":");
                        // проверяем, что это текущее голосование
                        if (values.Length != 4 || values[3] != todayApprovedGamePoll.idPoll)
                        {
                            continue;
                        }
                        var idCurUser = update.CallbackQuery.From.Id;
                        if (update.CallbackQuery.Data.StartsWith("takeaseat:0:0:"))
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

                        long driverId = 0;
                        int stopIdx = 0;

                        var firstMaxPlayersCountIds = todayApprovedGamePoll.playrsList
                                          .Take(todayApprovedGamePoll.maxPlayersCount)
                                          .Select(player => player.id);

                        // проверяем что остановка это число
                        if (!int.TryParse(values[2], out stopIdx) || !long.TryParse(values[1], out driverId)) { return; }

                        // проверяем что тот кто просится тоже среди первых todayApprovedGamePoll.maxPlayersCount
                        if (update.CallbackQuery.From != null && idCurUser > 0)
                        {
                            if (!firstMaxPlayersCountIds.Contains(idCurUser) && 245566701 != idCurUser) { return; }
                        }

                        // проверяем что водитель есть среди первых todayApprovedGamePoll.maxPlayersCount
                        if (!firstMaxPlayersCountIds.Contains(driverId)) { return; }

                        // проверяем что данного водителя ещё есть места, без учета просящегося
                        // Подсчет количества записей для конкретного пользователя
                        int count = todayApprovedGamePoll.occupiedPlaces.Count(place => place.idCarOwner == driverId);
                        Car foundCar = stateManager.state.carList.FirstOrDefault(car => car.idPlayer == driverId);
                        if (foundCar == null || count >= foundCar.placeCount)
                        {
                            return;
                        }

                        // проверяем что он ещё не занял место в данной точке
                        if (todayApprovedGamePoll.occupiedPlaces.Any(o => o.idCarOwner == driverId && o.stopIdx == stopIdx && o.idPlayer == idCurUser))
                        {
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
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в takeSeat: {ex.Message}");
            }
        }

        private static int freeSeat(long idMember, Poll todayApprovedGamePoll)
        {
            // Удаление объектов, относящихся к определенному игроку
            return todayApprovedGamePoll.occupiedPlaces.RemoveAll(place => place.idPlayer == idMember);
        }

        private async void sendWellcomeMessage(User? member)
        {
            if (member != null)
            {
                var inviteMessage = BallBotGui.Properties.Settings.Default.inviteMessage;
                inviteMessage = inviteMessage.Replace("@Player", member.FirstName);

                try
                {
                    await botClient.SendMessage(chatId, inviteMessage);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при отправке приветствия: {ex.Message}");
                }
            }
        }



        internal async void suggectTeams(Update update)
        {
            try
            {
                var teams = stateManager.Take2Teams(update);
                if (teams.Team1.Count > 5 && teams.Team2.Count > 5)
                {


                    string team1Players = string.Join("\n", teams.Team1.Select(p => $"{(string.IsNullOrWhiteSpace(p.normalName) ? p.firstName : p.normalName)} @{p.name}"));
                    string team2Players = string.Join("\n", teams.Team2.Select(p => $"{(string.IsNullOrWhiteSpace(p.normalName) ? p.firstName : p.normalName)} @{p.name}"));


                    string message = $"Предлагаются команды:\n\nКоманда 1:\n{team1Players}\n\nКоманда 2:\n{team2Players}";
                    try
                    {
                        await botClient.SendMessage(chatId, message);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при отправке команд: {ex.Message}");
                    }

                    // Сохраняем составы команд в опрос для истории
                    var poll = stateManager.GetClosestApprovedPollForToday();
                    if (poll != null)
                    {
                        var teamComposition = new TeamComposition(
                            DateTime.Now,
                            teams.Team1.Select(p => p.id).ToList(),
                            teams.Team2.Select(p => p.id).ToList()
                        );
                        poll.TeamCompositions.Add(teamComposition);
                        stateManager.SaveState();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в suggectTeams: {ex.Message}");
            }
        }
        internal async void suggect4Teams(Update update)
        {
            try
            {
                var teams = stateManager.Take4Teams(update);
                if (teams.Team1.Count > 5 && teams.Team2.Count > 5)
                {


                    string team1Players = string.Join("\n", teams.Team1.Select(p => $"@{p.name} {p.firstName}"));
                    string team2Players = string.Join("\n", teams.Team2.Select(p => $"@{p.name} {p.firstName}"));
                    string team3Players = string.Join("\n", teams.Team3.Select(p => $"@{p.name} {p.firstName}"));
                    string team4Players = string.Join("\n", teams.Team4.Select(p => $"@{p.name} {p.firstName}"));

                    string message = $"!Предлагаются следующие составы команд:\n\nКоманда 1:\n{team1Players}\n\nКоманда 2:\n{team2Players}\n\nКоманда 3:\n{team3Players}\n\nКоманда 4:\n{team4Players}";
                    try
                    {
                        await botClient.SendMessage(chatId, message);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при отправке 4 команд: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в suggect4Teams: {ex.Message}");
            }
        }

        internal void ArchPolls()
        {
            foreach (var poll in stateManager.state.pollList.Where(p => p.idCarsMessage > 0).ToList())
            {
                deleteCarMessage(poll);

            }
            stateManager.ArchPolls(botClient);

        }

        internal async Task DeleteUnansweredSurveys()
        {
            if (stateManager.state.pollList == null) return;

            foreach (var poll in stateManager.state.pollList)
            {
                if (poll.SurveyMessages == null) continue;

                var toRemove = new List<SurveyMessageInfo>();

                foreach (var info in poll.SurveyMessages.ToList())
                {
                    var messagesToRemove = new List<SurveyMessage>();

                    foreach (var msg in info.Messages.ToList())
                    {
                        bool answered = false;
                        if (msg.Nomination == "intro")
                        {
                            answered = false;
                        }
                        else
                        {
                            answered = poll.PostGameVotes != null && poll.PostGameVotes.Any(v => v.VoterId == info.PlayerId && v.Nomination == msg.Nomination);
                        }

                        if (!answered)
                        {
                            try
                            {
                                await botClient.DeleteMessage(info.PlayerId, msg.MessageId);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Ошибка при удалении сообщения опроса: {ex.Message}");
                            }
                            messagesToRemove.Add(msg);
                        }
                    }

                    foreach (var msg in messagesToRemove)
                    {
                        info.Messages.Remove(msg);
                    }

                    if (info.Messages.Count == 0)
                    {
                        toRemove.Add(info);
                    }
                }

                foreach (var item in toRemove)
                {
                    poll.SurveyMessages.Remove(item);
                }
            }
            stateManager.SaveState();
        }

        internal async Task sendInvitation(Poll todayApprovedGamePoll)
        {
            int inviteCount = Math.Min(todayApprovedGamePoll.playrsList.Count, todayApprovedGamePoll.maxPlayersCount);
            string gameTime = $"{todayApprovedGamePoll.curGame.GameStartHour}:{todayApprovedGamePoll.curGame.GameStartMinute:D2}";

            try
            {
                await botClient.SendMessage(chatId, $"Игра в {gameTime} ");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отправке времени игры: {ex.Message}");
            }
            for (int i = 0; i < inviteCount; i += 5)
            {
                StringBuilder messageBuilder = new StringBuilder();

                for (int j = 0; j < 5 && (i + j) < inviteCount; j++)
                {
                    PlayerVote voter = todayApprovedGamePoll.playrsList[i + j];
                    var normalName = stateManager.players.FirstOrDefault(p => p.id == voter.id)?.normalName ?? string.Empty;
                    messageBuilder.AppendLine($"# {i + j + 1} {(string.IsNullOrWhiteSpace(normalName) ? voter.firstName : normalName)} @{voter.name}");
                }

                string message = messageBuilder.ToString();
                try
                {
                    await botClient.SendMessage(chatId, message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при отправке списка игроков: {ex.Message}");
                }
            }

            for (int i = 0; i < inviteCount; i++)
            {
                PlayerVote voter = todayApprovedGamePoll.playrsList[i];
                await sendPlayerInvitation(todayApprovedGamePoll, voter);
            }

            // Send notifications to players in queue
            for (int i = inviteCount; i < todayApprovedGamePoll.playrsList.Count; i++)
            {
                PlayerVote voter = todayApprovedGamePoll.playrsList[i];
                int queueNumber = i - inviteCount + 1;
                await sendPlayerQueueNotification(todayApprovedGamePoll, voter, queueNumber);
            }
        }

        private async Task sendPlayerInvitation(Poll todayApprovedGamePoll, PlayerVote voter)
        {
            string message = $" {voter.firstName}. Вы сегодня играете в волейбол в {todayApprovedGamePoll.curGame.GameStartHour}:{todayApprovedGamePoll.curGame.GameStartMinute:D2}";
            try
            {
                await botClient.SendMessage(voter.id, message);

                string[] phrases = Properties.Settings.Default.MotivationPhrases.Split('|');

                // Генерируем случайный индекс
                Random rand = new Random();
                string randomPhrase = phrases[rand.Next(phrases.Length)];
                await botClient.SendMessage(voter.id, randomPhrase);

            }
            catch (Exception ex)
            {
                var player = stateManager.players.FirstOrDefault(p => p.id == voter.id);
                if (player != null)
                {
                    player.FailedMessageCount++;
                    stateManager.SavePlayers();

                    if (player.FailedMessageCount <= 5)
                    {
                        message = $"Привет, @{voter.name} {voter.firstName}. Я скучаю, начни со мной общаться, пожалуйста! \n\nТвой @GadensVolleyballBot";
                        await botClient.SendMessage(chatId, message);
                    }
                }
            }
        }

        private async Task sendPlayerQueueNotification(Poll poll, PlayerVote voter, int queueNumber)
        {
            string message = $"Привет, {voter.firstName}! Ты сейчас в очереди на сегодняшнюю игру под номером {queueNumber}.\n" +
                             "Если твои планы изменятся, пожалуйста, обязательно снимись с голосования!";
            try
            {
                await botClient.SendMessage(voter.id, message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Не удалось отправить сообщение игроку {voter.id}: {ex.Message}");
            }
        }



        internal async Task sendCarsMessage(Poll todayApprovedGamePoll)
        {

            var stops = new List<StopInfo>();

            // Получаем id игроков из первых maxPlayersCount в голосовании
            var firstMaxPlayersCountIds = todayApprovedGamePoll.playrsList
                                        .Take(todayApprovedGamePoll.maxPlayersCount)
                                        .Select(player => player.id);

            // Выбираем машины, у которых idPlayer есть в carsWithOwnersInFirstMaxPlayersCount
            var carsWithOwnersInFirstMaxPlayersCount = stateManager.state.carList.Where(car => firstMaxPlayersCountIds.Contains(car.idPlayer)).ToList();

            if (carsWithOwnersInFirstMaxPlayersCount.Count > 0)
            {
                StringBuilder messageBuilder = new StringBuilder();
                string gameTime = $"{todayApprovedGamePoll.curGame.GameStartHour}:{todayApprovedGamePoll.curGame.GameStartMinute:D2}";
                messageBuilder.AppendLine($"К {gameTime}  помогают добраться:");

                int stopIdx = 1;
                foreach (var car in carsWithOwnersInFirstMaxPlayersCount)
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
                    try
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
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при отправке сообщения о машинах: {ex.Message}");
                    }
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
            try
            {
                await botClient.DeleteMessage(chatId, curPoll.idCarsMessage);
                curPoll.idCarsMessage = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении сообщения о машинах: {ex.Message}");
            }
        }

        private static int addStops(Poll todayApprovedGamePoll, List<StopInfo> stops, StringBuilder messageBuilder, int stopIdx, Car? car, PlayerVote? owner)
        {
            if (car.carStops.Any())
            {
                messageBuilder.AppendLine("<b>Остановки:</b>");
                var curDriverStopIdx = 1;

                foreach (var stop in car.carStops)
                {
                    var newPoint = new StopInfo(todayApprovedGamePoll.idPoll, owner.id, curDriverStopIdx, stopIdx);
                    stops.Add(newPoint);

                    TimeSpan gameTime = new TimeSpan(20, 0, 0); // Время игры: 20:00
                    int minutesToSubtract = 30;

                    if (stop.minBefore > 0)
                    {
                        minutesToSubtract = stop.minBefore;

                    }
                    var curGame = todayApprovedGamePoll.curGame;
                    if (curGame != null
                        && curGame.GameStartHour > 0)
                    {

                        gameTime = new TimeSpan(curGame.GameStartHour, curGame.GameStartMinute, 0);
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
                                    .Select(stop => InlineKeyboardButton.WithCallbackData(stop.SequentialNumber.ToString(), $"takeaseat:{stop.DriverCode}:{stop.StopNumber}:{stop.IdPoll}"))
                                    .ToArray();
                buttons.Add(rowButtons);
            }

            // добавляем последнюю кнопку
            var byMySelf = InlineKeyboardButton.WithCallbackData("Добираюсь самостоятельно", $"takeaseat:0:0:{stops.First().IdPoll}");
            buttons.Add(new[] { byMySelf });
            return new InlineKeyboardMarkup(buttons);

        }

        internal async Task sendTestMessageAsync()
        {
            // ID пользователя, к3оторого вы хотите упомянуть
            int userId = 3485184;
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
        /*
         
         
          internal List<PlayerVote> GetFirstTimePlayers(List<Poll> polls, Poll todayApprovedGamePoll)
        {
            var previousPlayerIds = new HashSet<long>();

            // Iterate through all polls to collect unique player IDs  
            foreach (var poll in polls)
            {
                if (poll != null && poll.approved)
                {
                    foreach (var player in poll.playrsList.Take(poll.maxPlayersCount))
                    {
                        previousPlayerIds.Add(player.id);
                    }
                }
            }

            // Find players in today's poll who are not in the previousPlayerIds set  
            var curPlayersList = todayApprovedGamePoll.playrsList.Take(todayApprovedGamePoll.maxPlayersCount);
            var newPlayers = curPlayersList.Where(p => !previousPlayerIds.Contains(p.id)).ToList();

            return newPlayers;
        }
         
         */

        internal List<PlayerVote> askNewPlayers(List<Poll> todayApprovedGamePoll)
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

                if (poll != null && poll.approved)
                {
                    foreach (var player in poll.playrsList.Take(poll.maxPlayersCount))
                    {
                        previousPlayerIds.Add(player.id);
                    }
                }

            }

            // Находим новых игроков

            //  var curPlayersList = todayApprovedGamePoll.playrsList.Take(todayApprovedGamePoll.maxPlayersCount);
            var uniquePlayers = todayApprovedGamePoll
                .SelectMany(poll => poll.playrsList.Take(poll.maxPlayersCount))
                .GroupBy(player => player.id)
                .Select(group => group.First())
                .ToList();

            var newPlayers = uniquePlayers.Where(player => !previousPlayerIds.Contains(player.id)).ToList();
            // var newPlayers = curPlayersList.Where(p => !previousPlayerIds.Contains(p.id)).ToList();
            return newPlayers;
        }

        internal async Task askAboutFirstGameAsync(PlayerVote player)
        {
            string message = $"Добрый день, @{player.name} {player.firstName}. Вы сегодня первый раз с нами играете! Как добираетесь? ";

            await botClient.SendMessage(chatId, message);
        }

        internal async Task sendBeforeGameInvite(Poll poll)
        {
            int inviteCount = Math.Min(poll.playrsList.Count, poll.maxPlayersCount);


            for (int i = 0; i < inviteCount; i++)
            {
                PlayerVote voter = poll.playrsList[i];
                await sendPlayerBeforeGameInvitation(poll, voter);
            }
        }

        private async Task sendPlayerBeforeGameInvitation(Poll poll, PlayerVote voter)
        {
            try
            {
                string message = $"Через час волейбол! Пора собираться!";
                // Проверяем, есть ли пассажиры у текущего водителя
                var passengers = poll.occupiedPlaces
                    .Where(p => p.idCarOwner == voter.id)
                    .ToList();

                if (passengers.Any())
                {
                    message += "\n\n🚗 У вас есть пассажиры:";

                    foreach (var passenger in passengers)
                    {
                        string name = !string.IsNullOrEmpty(passenger.nickname)
                                        ? "@" + passenger.nickname
                                        : passenger.firstName ?? "Игрок";

                        message += $"\n - {name}";
                    }
                }

                await botClient.SendMessage(voter.id, message);
            }
            catch (Exception)
            {

            }
        }

        internal async Task sendAfterGameSurvey(Poll poll)
        {
            /*   // ОТЛАДКА: Отправляем опрос только администратору
               if (poll.playrsList != null && poll.playrsList.Any())
               {
                   // Чтобы список кандидатов не менялся при клике (когда срабатывает HandleVoteCallback с AdminId),
                   // мы должны сформировать начальное сообщение так, как будто оно для AdminId.
                   // Если админа нет в списке игроков, то просто используем фейкового игрока с AdminId.
                   // Тогда логика исключения (p.id != voter.id) будет работать одинаково и при отправке, и при клике.

                   var adminVoter = poll.playrsList.FirstOrDefault(p => p.id == AdminId)
                                    ?? new PlayerVote(AdminId, "Admin", "", 0, 0);

                   await sendPlayerAfterGameSurvey(poll, adminVoter);
               }*/

            int inviteCount = Math.Min(poll.playrsList.Count, poll.maxPlayersCount);

            for (int i = 0; i < inviteCount; i++)
            {
                PlayerVote voter = poll.playrsList[i];
                await sendPlayerAfterGameSurvey(poll, voter);
            }

        }

        private List<PlayerVote> GetSortedCandidates(Poll poll, long voterId)
        {
            // Берем только первых maxPlayersCount игроков (приглашенных на игру), исключая текущего пользователя
            var candidates = poll.playrsList
                .Take(poll.maxPlayersCount)  // Берем только приглашенных игроков
                .Where(p => p.id != voterId)  // Исключаем текущего пользователя
                .ToList();

            // Если есть составы команд, сортируем игроков по последнему составу
            if (poll.TeamCompositions != null && poll.TeamCompositions.Any())
            {
                var lastTeams = poll.TeamCompositions.Last();
                var team1Ids = lastTeams.Team1PlayerIds;
                var team2Ids = lastTeams.Team2PlayerIds;

                // Разделяем игроков по командам
                var team1Players = candidates.Where(p => team1Ids.Contains(p.id)).ToList();
                var team2Players = candidates.Where(p => team2Ids.Contains(p.id)).ToList();
                var otherPlayersList = candidates.Where(p => !team1Ids.Contains(p.id) && !team2Ids.Contains(p.id)).ToList();

                // Чередуем игроков из команд для создания двух столбцов
                var sortedPlayers = new List<PlayerVote>();
                int maxCount = Math.Max(team1Players.Count, team2Players.Count);

                for (int i = 0; i < maxCount; i++)
                {
                    if (i < team1Players.Count) sortedPlayers.Add(team1Players[i]);
                    if (i < team2Players.Count) sortedPlayers.Add(team2Players[i]);
                }

                // Добавляем остальных игроков в конец
                sortedPlayers.AddRange(otherPlayersList);
                return sortedPlayers;
            }

            return candidates;
        }

        private async Task sendPlayerAfterGameSurvey(Poll poll, PlayerVote voter)
        {
            try
            {
                var otherPlayers = GetSortedCandidates(poll, voter.id);

                // Определяем номинации с пиктограммами
                var nominations = new[] {
                    ("mood", "😊 За хорошее настроение"),
                    ("support", "🤝 За поддержку на площадке"),
                    ("skill", "⭐ За отличную игру")
                };

                // Вступительное сообщение
                string introText = "🙏 <b>Спасибо за игру!</b>\n\n" +
                                   "Кому из игроков вы хотите выразить благодарность?\n" +
                                   "Можно выбрать до 2 человек в каждой категории.";

                var sentIntro = await botClient.SendMessage(voter.id, introText, parseMode: ParseMode.Html);

                var surveyInfo = new SurveyMessageInfo(voter.id);
                surveyInfo.Messages.Add(new SurveyMessage { MessageId = sentIntro.MessageId, Nomination = "intro" });

                // Отправляем три отдельных сообщения - по одному для каждой номинации
                foreach (var (key, name) in nominations)
                {
                    // Формируем текст только с заголовком категории
                    string text = $"<b>{name}</b>:";

                    // Создаем клавиатуру для этой номинации с кнопкой ОТПРАВИТЬ в конце
                    var replyMarkup = BuildKeyboardForNomination(poll.idPoll, key, otherPlayers, new Dictionary<string, HashSet<long>>());

                    // Отправка опроса пользователю
                    var sentPoll = await botClient.SendMessage(voter.id, text, parseMode: ParseMode.Html, replyMarkup: replyMarkup);
                    surveyInfo.Messages.Add(new SurveyMessage { MessageId = sentPoll.MessageId, Nomination = key });

                }

                poll.SurveyMessages.Add(surveyInfo);
            }
            catch (Exception)
            {

            }
        }



        private InlineKeyboardMarkup BuildKeyboardForNomination(string gameId, string nomination, List<PlayerVote> players,
            Dictionary<string, HashSet<long>> selected)
        {
            var keyboard = new List<List<InlineKeyboardButton>>();
            var currentRow = new List<InlineKeyboardButton>();

            foreach (var p in players)
            {
                bool sel = selected.ContainsKey(nomination) && selected[nomination].Contains(p.id);

                // Ищем игрока в stateManager.players для получения normalName
                var player = stateManager.players.FirstOrDefault(pl => pl.id == p.id);
                string normalName = player?.normalName ?? "";

                // Формируем текст кнопки: "NormalName/FirstName/Name @username"
                string displayName = !string.IsNullOrEmpty(normalName) ? normalName :
                                     (!string.IsNullOrEmpty(p.firstName) ? p.firstName : p.name);

                string username = !string.IsNullOrEmpty(p.name) ? $"@{p.name}" : "";

                // Собираем итоговую строку, убирая лишние пробелы
                string txt = (sel ? "✅ " : "") + $"{displayName} {username}".Trim();

                string data = $"vote|{gameId}|{nomination}|{p.id}";

                currentRow.Add(InlineKeyboardButton.WithCallbackData(txt, data));

                // Если в строке набралось 2 кнопки, добавляем строку в клавиатуру и начинаем новую
                if (currentRow.Count == 2)
                {
                    keyboard.Add(currentRow);
                    currentRow = new List<InlineKeyboardButton>();
                }
            }

            // Если осталась неполная строка (1 кнопка), добавляем её
            if (currentRow.Count > 0)
            {
                keyboard.Add(currentRow);
            }

            // Добавляем кнопку ОТПРАВИТЬ в конец списка (всегда отдельной строкой)
            keyboard.Add(new List<InlineKeyboardButton> {
                InlineKeyboardButton.WithCallbackData("📩 ОТПРАВИТЬ", $"submit|{gameId}|{nomination}")
            });

            // Добавляем кнопку СЕГОДНЯ ТАКИХ НЕТ
            keyboard.Add(new List<InlineKeyboardButton> {
                InlineKeyboardButton.WithCallbackData("🙅 СЕГОДНЯ ПРОПУЩУ", $"submit|{gameId}|{nomination}|none")
            });

            return new InlineKeyboardMarkup(keyboard);
        }

        private string NominationName(string key) =>
       key switch
       {
           "mood" => "😊 За хорошее настроение",
           "support" => "🤝 За поддержку на площадке",
           "skill" => "⭐ За отличную игру",
           _ => key
       };


        private async void HandleCallbackQuery(CallbackQuery callbackQuery)
        {
            try
            {
                var data = callbackQuery.Data;
                if (string.IsNullOrEmpty(data)) return;

                var parts = data.Split('|');
                var action = parts[0];

                if (action == "vote")
                {
                    await HandleVoteCallback(callbackQuery, parts);
                }
                else if (action == "submit")
                {
                    await HandleSubmitCallback(callbackQuery, parts);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling callback: {ex.Message}");
            }
        }

        private async Task HandleVoteCallback(CallbackQuery callbackQuery, string[] parts)
        {
            // vote|gameId|nomination|playerId
            if (parts.Length != 4) return;

            string gameId = parts[1];
            string nomination = parts[2];
            if (!long.TryParse(parts[3], out long playerId)) return;

            // Восстанавливаем текущее состояние из клавиатуры
            var markup = callbackQuery.Message?.ReplyMarkup;
            if (markup == null) return;

            var currentSelection = GetSelectionFromKeyboard(markup, nomination);

            // Обновляем выбор
            if (currentSelection.Contains(playerId))
            {
                currentSelection.Remove(playerId);
            }
            else
            {
                if (currentSelection.Count >= 2)
                {
                    await botClient.AnswerCallbackQuery(callbackQuery.Id, "Можно выбрать не более 2 игроков!", showAlert: true);
                    return;
                }
                currentSelection.Add(playerId);
            }

            // Получаем список игроков из опроса для перестройки клавиатуры
            var poll = stateManager.state.pollList.FirstOrDefault(p => p.idPoll == gameId);
            if (poll == null) return;

            // Исключаем голосующего (себя) из списка кандидатов
            var voterId = callbackQuery.From.Id;
            var candidates = GetSortedCandidates(poll, voterId);

            var selectedDict = new Dictionary<string, HashSet<long>> { { nomination, currentSelection } };
            var newKeyboard = BuildKeyboardForNomination(gameId, nomination, candidates, selectedDict);

            await botClient.EditMessageReplyMarkup(
                callbackQuery.Message.Chat.Id,
                callbackQuery.Message.MessageId,
                replyMarkup: newKeyboard
            );
        }

        private async Task HandleSubmitCallback(CallbackQuery callbackQuery, string[] parts)
        {
            // submit|gameId|nomination|none?
            if (parts.Length < 3) return;

            string gameId = parts[1];
            string nomination = parts[2];
            bool isNone = parts.Length > 3 && parts[3] == "none";
            long voterId = callbackQuery.From.Id;

            // Получаем выбранных игроков из клавиатуры
            var markup = callbackQuery.Message?.ReplyMarkup;
            if (markup == null) return;

            var selectedIds = GetSelectionFromKeyboard(markup, nomination);

            if (!isNone && selectedIds.Count == 0)
            {
                await botClient.AnswerCallbackQuery(callbackQuery.Id, "Выберите хотя бы одного игрока или нажмите 'СЕГОДНЯ ПРОПУЩУ'!", showAlert: true);
                return;
            }

            if (isNone)
            {
                selectedIds.Clear();
            }

            var poll = stateManager.state.pollList.FirstOrDefault(p => p.idPoll == gameId);
            if (poll != null)
            {
                // Сохраняем голос
                var vote = new PostGameVote(voterId, nomination, selectedIds.ToList());

                // Удаляем старый голос этого юзера за эту номинацию, если был
                poll.PostGameVotes.RemoveAll(v => v.VoterId == voterId && v.Nomination == nomination);
                poll.PostGameVotes.Add(vote);

                stateManager.SaveState();

                // Отправка уведомлений выбранным игрокам
                if (!isNone && selectedIds.Count > 0)
                {
                    // Определяем имя голосующего
                    var voter = stateManager.players.FirstOrDefault(p => p.id == voterId);
                    string voterNormalName = voter?.normalName ?? "";
                    string voterFirstName = voter?.firstName ?? callbackQuery.From.FirstName;
                    string voterUserNameVal = voter?.name ?? callbackQuery.From.Username;

                    string voterDisplayName = !string.IsNullOrEmpty(voterNormalName) ? voterNormalName : voterFirstName;
                    string voterName = $"{voterDisplayName} {(!string.IsNullOrEmpty(voterUserNameVal) ? "@" + voterUserNameVal : "")}".Trim();

                    string nominationTitle = NominationName(nomination);

                    foreach (var recipientId in selectedIds)
                    {
                        try
                        {
                            if (recipientId == voterId) continue;

                            string msg = $"🙏 <b>{voterName}</b> отметил(а) вас в номинации:\n✨ <b>{nominationTitle}</b>";
                            await botClient.SendMessage(recipientId, msg, parseMode: ParseMode.Html);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Не удалось отправить уведомление игроку {recipientId}: {ex.Message}");
                        }
                    }
                }
            }

            // Формируем сообщение подтверждения
            var selectedNames = new List<string>();
            if (isNone)
            {
                selectedNames.Add("Никого");
            }
            else
            {
                foreach (var pid in selectedIds)
                {
                    var player = stateManager.players.FirstOrDefault(p => p.id == pid);
                    if (player != null)
                    {
                        string normalName = player.normalName ?? "";
                        string displayName = !string.IsNullOrEmpty(normalName) ? normalName :
                                             (!string.IsNullOrEmpty(player.firstName) ? player.firstName : player.name);
                        string username = !string.IsNullOrEmpty(player.name) ? $"@{player.name}" : "";

                        selectedNames.Add($"{displayName} {username}".Trim());
                    }
                }
            }

            string confirmText = $"<b>{NominationName(nomination)}</b>\n" +
                                 $"Вы выбрали: {string.Join(", ", selectedNames)}";

            await botClient.EditMessageText(
                callbackQuery.Message.Chat.Id,
                callbackQuery.Message.MessageId,
                confirmText,
                parseMode: ParseMode.Html
            );
        }

        private HashSet<long> GetSelectionFromKeyboard(InlineKeyboardMarkup markup, string nomination)
        {
            var selected = new HashSet<long>();
            if (markup == null) return selected;

            foreach (var row in markup.InlineKeyboard)
            {
                foreach (var btn in row)
                {
                    // vote|gameId|nomination|playerId
                    if (btn.CallbackData != null && btn.CallbackData.StartsWith($"vote|"))
                    {
                        var parts = btn.CallbackData.Split('|');
                        if (parts.Length == 4 && parts[2] == nomination)
                        {
                            // Проверяем наличие галочки в тексте
                            if (btn.Text.Contains("✅"))
                            {
                                if (long.TryParse(parts[3], out long pid))
                                {
                                    selected.Add(pid);
                                }
                            }
                        }
                    }
                }
            }
            return selected;
        }

        private string GetLetterRating(int rate)
        {
            return rate switch
            {
                1 => "A",
                2 => "B",
                3 => "C",
                4 => "D",
                _ => "Не задан"
            };
        }

        private async void ProcessAdminRateMessage(Update update)
        {
            try
            {
                // Проверяем, что сообщение от администратора
                if (update.Message?.From?.Id != 245566701)
                    return;

                var text = update.Message?.Text?.Trim();
                if (string.IsNullOrEmpty(text) || !text.StartsWith("rate:"))
                    return;

                var parts = text.Split(':');
                if (parts.Length != 3)
                    return;

                Player? player = null;
                string playerParam = parts[1];

                // Проверяем, что второй параметр - username (начинается с @)
                if (playerParam.StartsWith("@"))
                {
                    string username = playerParam.Substring(1);
                    player = stateManager.players
                        .FirstOrDefault(p => !string.IsNullOrEmpty(p.name) && p.name.Equals(username, StringComparison.OrdinalIgnoreCase));
                }
                else if (long.TryParse(playerParam, out long playerId))
                {
                    player = stateManager.players.FirstOrDefault(p => p.id == playerId);
                }

                if (!int.TryParse(parts[2], out int rate))
                    return;

                if (player == null)
                {
                    string notFoundMsg = $"Ошибка: игрок с {(playerParam.StartsWith("@") ? "username" : "id")} {playerParam} не найден.";
                    await botClient.SendMessage(update.Message.Chat.Id, notFoundMsg);

                    // Дополнительно уведомляем администратора в личку
                    try
                    {
                        await botClient.SendMessage(245566701, notFoundMsg);
                    }
                    catch { /* ignore */ }

                    return;
                }

                player.rating = rate;
                player.group = rate;
                stateManager.SavePlayers();

                string letterRating = GetLetterRating(rate);

                await botClient.SendMessage(
                    update.Message.Chat.Id,
                    $"Рейтинг игрока @{player.name} ({player.id}) установлен: {rate} ({letterRating})"
                );

                // Уведомление игрока
                await botClient.SendMessage(player.id, $"Ваш рейтинг установлен администратором: {letterRating} . ( Справочно: A - сильный, D - начинающий )");
            }
            catch
            {
                // Игнорируем ошибку, если не удалось отправить сообщение игроку
            }
        }

        private async void ProcessAdminSpamWordMessage(Update update)
        {
            try
            {
                // Проверяем, что сообщение от администратора
                if (update.Message?.From?.Id != 245566701)
                    return;

                var text = update.Message?.Text?.Trim();
                if (string.IsNullOrEmpty(text) || !text.StartsWith("spam:"))
                    return;

                var parts = text.Split(':', 2);
                if (parts.Length != 2)
                    return;

                string word = parts[1].Trim().ToLower();
                if (string.IsNullOrWhiteSpace(word))
                    return;

                if (!stateManager.state.spamStopWords.Contains(word))
                {
                    stateManager.state.spamStopWords.Add(word);
                    stateManager.SaveState();
                    await botClient.SendMessage(update.Message.Chat.Id, $"Слово \"{word}\" добавлено в список стоп-слов.");
                }
                else
                {
                    await botClient.SendMessage(update.Message.Chat.Id, $"Слово \"{word}\" уже есть в списке стоп-слов.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в ProcessAdminSpamWordMessage: {ex.Message}");
            }
        }
    }

}
