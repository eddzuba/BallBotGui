using System.Configuration;
using Telegram.Bot;

namespace BallBotGui
{
    public partial class Form1 : Form
    {
        private TelegramBotClient botClient;
        private PollGenerator? pollGen = new();
        private string? botKey;

        public Form1() => InitializeComponent();

        private void Form1_Load(object sender, EventArgs e)
        {
            botKey = Properties.Settings.Default.curBotKey;
            botClient = new TelegramBotClient(botKey);
            PollGenerator.botClient = botClient;

        }

        private async void minuteTimer_Tick(object sender, EventArgs e)
        {
            DateTime curTime = DateTime.Now;

            int pollHour = Properties.Settings.Default.pollHour;
            int pollBeforeGame = Properties.Settings.Default.pollBeforeGame;

            int[] days = getPollDays(); 
            if( Array.Exists(days, el => el == ((int)curTime.DayOfWeek))
                && curTime.Hour == pollHour && curTime.Minute == 00) {
                 await pollGen.createOnePoll(curTime.AddDays(pollBeforeGame));
            }
        }

        private int[] getPollDays()
        {
            // get day number when we need to grate a poll
            var stringArray = Properties.Settings.Default.pollDayList.Split(',');
            // Convert string array to integer array
            int[] intArray = new int[stringArray.Length];
            for (int i = 0; i < stringArray.Length; i++)
            {
                 // Use int.TryParse if strings may contain non-integer values
                if (int.TryParse(stringArray[i], out int result))
                {
                    intArray[i] = result;
                }
                else
                {
                    Console.WriteLine($"Unable to parse the string {stringArray[i]} to an integer.");
                }
            }
            
            return intArray;
        }
    }
}