using System.Collections.Generic;

namespace DiceGame.Akka.GameEngine.Model
{
    public class StartGameRequest
    {
        public string GameId { get; set; }
        public List<string> Players { get; set; }
    }
}
