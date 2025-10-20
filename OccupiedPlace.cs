namespace BallBotGui
{
    public class OccupiedPlace
    {
        public long idPlayer { get; set; } // кто занял место
        public long idCarOwner { get; set; } // у кого в машине

        public int stopIdx { get; set; } = 1; // Номер остановки на которой нужно его забрать 

        public string? nickname { get; set; }  // имя пользователя телеграм
        public string? firstName { get; set; } // Имя

        public OccupiedPlace(long idPlayer, long idCarOwner, int stopIdx, string? nickname, string? firstName)
        {
            this.idPlayer = idPlayer;
            this.idCarOwner = idCarOwner;
            this.stopIdx = stopIdx;
            this.nickname = nickname;
            this.firstName = firstName;
        }
    }
}
