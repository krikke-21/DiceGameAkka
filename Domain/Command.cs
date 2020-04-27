using System.Collections.Generic;
using System.Collections.Immutable;

namespace DiceGame.Akka.Domain
{
    public abstract class GameCommand
    { }



    public class StartGame : GameCommand
    {
        public ImmutableList<PlayerId> Players { get; private set; }

        public StartGame(ImmutableList<PlayerId> players)
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
