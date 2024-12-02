using Telegram.Bot;
using System.Timers;
using System.Security.Cryptography;
using System;
using System.Globalization;
using Telegram.Bot.Types;

namespace BallBotGui
{
    internal class PollGenerator
    {
        public static TelegramBotClient botClient;
        private static string chatId = Properties.Settings.Default.chatId;
        private static System.Timers.Timer timerFirst = new System.Timers.Timer();
        public void startGeneratePoll()
        {

            TriggerFunction3Poll();

        }
        
        // старая версия
        static async void TriggerFunction()
        {
            DateTime today = DateTime.Today;
            DateTime nextMonday = GetNextWeekday(today, DayOfWeek.Monday);
            DateTime nextSunday = GetNextWeekday(today.AddDays(2), DayOfWeek.Sunday);
            string curQuest = Properties.Settings.Default.pollQuestion + " "
                + nextMonday.ToString("dd.MM") 
                + " - "
                + nextSunday.ToString("dd.MM");
                

            await botClient.SendPoll(
                chatId: chatId, 
                question:  curQuest,
                options: new InputPollOption[]
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
            await botClient.SendPoll(
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
        }

        static DateTime GetNextWeekday(DateTime currentDate, DayOfWeek targetDay)
        {
            int daysUntilTargetDay = ((int)targetDay - (int)currentDate.DayOfWeek + 7) % 7;
            return currentDate.AddDays(daysUntilTargetDay);
        }
    }
    
}
