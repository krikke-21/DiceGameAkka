namespace DiceGame.Akka.Domain
{
    public class GameId : Id<Game>
    {
        public GameId(string value)
            : base(value)
        {
        }
    }
}
