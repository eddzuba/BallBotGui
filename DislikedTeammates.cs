using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BallBotGui
{
    public class DislikedTeammates
    {
        public long idPlayer { get; set; }
        public List<long> dislikedPlayers { get; set; } = new();
    }
}
