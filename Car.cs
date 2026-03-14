using System.ComponentModel;


namespace BallBotGui
{
    public class Car
    {
        public long idPlayer { get; set; } // хозяин машины
        
        [Newtonsoft.Json.JsonIgnore]
        public string name { get; set; }   // nik (lookup)
        
        [Newtonsoft.Json.JsonIgnore]
        public string firstName { get; set; } // Имя (lookup)
        
        public int placeCount { get; set; } = 2; // количество мест

        public BindingList<CarStops> carStops { get; set; } = new();    // где может забрать людей

        public Car(long idPlayer, string name, string firstName, int placeCount)
        {
            this.idPlayer = idPlayer;
            this.name = name;
            this.firstName = firstName;
            this.placeCount = placeCount;
        }

        public void UpdateFromPlayer(Player p)
        {
            if (p != null)
            {
                this.name = p.name;
                this.firstName = p.firstName;
            }
        }
    }
}
