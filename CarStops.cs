namespace BallBotGui
{
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
}
