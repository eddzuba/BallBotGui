using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BallBotGui
{

    public class VolleybollGame
    {
        public int PullHour { get; set; } // час когда создаем голосование  23
        public int PullMinute { get; set; } // минуты когда создаем голосование  00
        public int GameDay { get; set; } // день недели когда играме 1- Понедельник
        public int PullBeforeDay { get; set; } // за сколько дней выбрасывать голосование, для рассчета В какой день играем
        
        public int GameStartHour { get; set; } // час когда играем 20
        public int GameStartMinute { get; set; } // минуты времени когда начинаем играть 00

        public bool ActiveGame { get; set; } = true;


        public string? Location { get; set; } // Место где играем, ссылка на карте
        public string? Title { get; set; } // Шаблон Заголовка в голосовалке

        // Проверка, совпадает ли текущее время с временем голосования
        public bool IsTimeToCreatePoll(DateTime currentTime)
        {
            // проверяем сначала время, а потом уже все остальное
            if( currentTime.Hour == PullHour && currentTime.Minute == PullMinute )
            {
                // Вычисляем дату игры
                var daysToAdd = GameDay - (int)currentTime.DayOfWeek;
                if (daysToAdd < 0) daysToAdd += 7; // Переход на следующую неделю

                var gameDate = currentTime.Date.AddDays(daysToAdd);

                // Вычитаем PullBeforeDay, чтобы узнать дату голосования
                var pollDate = gameDate.AddDays(-PullBeforeDay);

                // Проверяем совпадение даты, часа и минуты
                return currentTime.Date == pollDate;
            }
            return false;
        }

        public string GetQuest(DateTime curTime)
        {
            string curQuest = Title;
            if (!string.IsNullOrEmpty(curQuest))
            {
                var gameTime = curTime.AddDays(PullBeforeDay);
                string formattedDate = gameTime.ToString("dd.MM", new CultureInfo("ru-RU"));
                curQuest = curQuest.Replace("@GameDayName", formattedDate);

            }
            else {
                curQuest = "Игра";
            }
            return curQuest;

            /* string formattedDate = curDay.ToString("dddd, dd.MM", new CultureInfo("ru-RU"));

           // Модифицируем строку, чтобы первая буква дня недели была заглавной
           formattedDate = formattedDate.ToUpper();
           string curQuest = formattedDate + "! " + Properties.Settings.Default.pollQuestion;
*/
        }
    }
}
