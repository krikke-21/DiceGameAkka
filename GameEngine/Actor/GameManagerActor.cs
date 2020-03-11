using Akka.Actor;
using DiceGame.Akka.Domain;
using System;

namespace DiceGame.Akka.GameEngine.Actor
{
    #region Messages

    public class CreateGame
    { }

    public class SendCommand
    {
        public GameId GameId { get; private set; }
        public GameCommand Command { get; private set; }

        public SendCommand(GameId gameId, GameCommand command)
        {
            GameId = gameId;
            Command = command;
        }
    }


    public class GameCreated
    {
        public GameId GameId { get; private set; }

        public GameCreated(GameId gameId)
        {
            GameId = gameId;
        }
    }


    public class GameContinued
    { }


    public class GameDoesNotExist
    { }


    public class GameAlreadyExists
    { }

    #endregion

    public class GameManagerActor : ReceiveActor
    {
        public GameManagerActor()
        {
            Receive<CreateGame>(Handle);
            Receive<SendCommand>(Handle);
        }

        public static Props GetProps()
        {
            return Props.Create<GameManagerActor>();
        }

        private void Handle(CreateGame message)
        {
            var id = new GameId($"Game_{Guid.NewGuid().ToString()}");

            var game = Context.Child(id.Value);
            if (!game.Equals(ActorRefs.Nobody))
            {
                Sender.Tell(new GameAlreadyExists());
            }

            Context.ActorOf(GameActor.GetProps(id), id.Value);

            Sender.Tell(new GameCreated(id));
        }

        private void Handle(SendCommand message)
        {
            var game = Context.Child(message.GameId.Value);

            if (game.Equals(ActorRefs.Nobody))
            {
                if (message.Command is ContinueGame)
                {
                    game = Context.ActorOf(GameActor.GetProps(message.GameId), message.GameId.Value);
                }
                else
                {
                    Sender.Tell(new GameDoesNotExist());
                }
            }
            game.Forward(message.Command);
        }
    }
}
