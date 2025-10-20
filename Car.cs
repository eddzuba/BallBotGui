using System.ComponentModel;


namespace BallBotGui
{
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
}
