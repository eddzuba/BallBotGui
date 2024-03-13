using Telegram.Bot;
using System.Globalization;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using System.Text.RegularExpressions;
using System.ComponentModel;
using Telegram.Bot.Polling;
using Telegram.Bot.Exceptions;
using Microsoft.VisualBasic;
using System.Numerics;
using System.Windows.Forms;
using static System.Windows.Forms.AxHost;
using System.Diagnostics.Metrics;

namespace BallBotGui
{
    internal class TelegramConnector
    {
        private readonly TelegramBotClient botClient;
        private readonly StateManager stateManager;
        private Form1 curForm;

        private readonly string chatId = Properties.Settings.Default.curChatId;
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

        private void StartReadingMessage()
        {
            ReceiverOptions receiverOptions = new()
            {
                AllowedUpdates = Array.Empty<UpdateType>() // receive all update types except ChatMember related updates
            };
            using CancellationTokenSource cts = new();

            botClient.StartReceiving(
                    updateHandler: HandleUpdateAsync,
                    pollingErrorHandler: HandlePollingErrorAsync,
                    receiverOptions: receiverOptions,
                    cancellationToken: cts.Token
                );
        }


        async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var needToSave = OnNewUpdate(update);
            if(needToSave)
            {
                stateManager.SaveState();
                curForm.refreshGrids();
            }
        }

        Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }


        public void startGeneratePoll()
        {

            TriggerFunction3Poll();

        }
        
        // старая версия
        public async void TriggerFunction()
        {
            DateTime today = DateTime.Today;
            DateTime nextMonday = GetNextWeekday(today, DayOfWeek.Monday);
            DateTime nextSunday = GetNextWeekday(today.AddDays(2), DayOfWeek.Sunday);
            string curQuest = Properties.Settings.Default.pollQuestion + " "
                + nextMonday.ToString("dd.MM") 
                + " - "
                + nextSunday.ToString("dd.MM");
                

            await botClient.SendPollAsync(
                chatId: chatId, 
                question:  curQuest,
                options: new[]
                    {
                    Properties.Settings.Default.questTuesday,
                    Properties.Settings.Default.questThursday,
                    Properties.Settings.Default.questSunday,
                    Properties.Settings.Default.questSkip
                },
                allowsMultipleAnswers: true,
                isAnonymous: false
            );

            timerFirst.Stop();
            return;
        }

        // пока оставил старые версии
        public async void TriggerFunction3Poll()
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
            var poll = await botClient.SendPollAsync(
                            chatId: chatId, 
                            question: curQuest,
                            options: new[]
                                {
                    Properties.Settings.Default.mainQuestion,
                    Properties.Settings.Default.questSkip
                            },
                            allowsMultipleAnswers: false,
                            isAnonymous: false
                        );
            

            // сохранение опроса в статусе
            stateManager.state.AddNewPoll(poll.Poll.Id, curDay.ToString("dd.MM", new CultureInfo("ru-RU")), curQuest, poll.MessageId);
            stateManager.SaveState();
            // Закрепление опроса
            await botClient.PinChatMessageAsync(
                        chatId: chatId,
                        messageId: poll.MessageId
                    );
        }

        // открепляем poll
        public async Task UnpinPoll(int pollId)
        {
            await botClient.UnpinChatMessageAsync(
                        chatId: chatId,
                        messageId: pollId
                    );
        }

        private DateTime GetNextWeekday(DateTime currentDate, DayOfWeek targetDay)
        {
            int daysUntilTargetDay = ((int)targetDay - (int)currentDate.DayOfWeek + 7) % 7;
            return currentDate.AddDays(daysUntilTargetDay);
        }

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
                        
                        // Добавьте другие типы обновлений, которые вам нужны
                };


            var updates = await botClient.GetUpdatesAsync(offset  
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

        private void onNewPollUpdate(Update update)
        {
            stateManager.state.AddNewPoll(update.Poll.Id, string.Empty, update.Poll.Question);
        }

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
                        if(curTime.Hour >= 10 && curTime.Hour <= 19)
                        {
                            inviteNextPlayer(update.PollAnswer.PollId, update.PollAnswer.User);
                        }
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

                await botClient.SendTextMessageAsync(chatId, message);
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

            if (update.Type == UpdateType.Message)
            {
                var d = update;
                if(update.Message != null && update.Message.Text == "#teams")
                {
                    suggectTeams(update);
                }

                if (update.Message != null &&  update.Message.Type == MessageType.ChatMembersAdded && update.Message.NewChatMembers != null )
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

        private async void sendWellcomeMessage(User? member)
        {
            if(member != null)
            {
                await botClient.SendTextMessageAsync(chatId, $"Приветствуем, {member.FirstName}! Мы рады, что вы присоединились к нам. Напишите пару слов о вашем уровне игры в волейбол. Ближайшие игры, правила нашего сообщества и вся важная информация находятся в закрепленных сообщениях, ознакомьтесь с ними, пожалуйста. Ждем вас на играх.");
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
                await botClient.SendTextMessageAsync(chatId, message);
            }
        }

        internal void ArchPolls()
        {
            stateManager.ArchPolls(botClient);
        }

        internal async void sendInvitation(Poll? todayApprovedGamePoll)
        {
            int inviteCount = Math.Min(todayApprovedGamePoll.playrsList.Count, 14);
            await botClient.SendTextMessageAsync(chatId, "Сегодня на игру приглашаются: ");
            for (int i = 0; i < inviteCount; i++)
            {
                PlayerVote voter = todayApprovedGamePoll.playrsList[i];
                string message = $"Игрок # {i + 1} @{voter.name} {voter.firstName}";

                await botClient.SendTextMessageAsync(chatId, message);
            }
        }
    }
    
}
