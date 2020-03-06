using System.Collections.Generic;

namespace DiceGame.Akka.Domain
{
    public abstract class GameCommand
    { }



    public class StartGame : GameCommand
    {
        public List<PlayerId> Players { get; private set; }

        public StartGame(List<PlayerId> players)
        {
            Players = players;
        }
    }


    public class ContinueGame : GameCommand
    {
    }


    public class RollDice : GameCommand
    {
        public PlayerId Player { get; private set; }

        public RollDice(PlayerId player)
        {
            Player = player;
        }
    }
}
