﻿using System.Collections.Generic;

namespace DiceGame.Akka.Domain
{
    public abstract class GameEvent
    {
        public GameId Id { get; private set; }

        protected GameEvent(GameId id)
        {
            Id = id;
        }
    }

    public class DiceRolled : GameEvent
    {
        public int RolledNumber { get; private set; }

        public PlayerId Player { get; private set; }

        public DiceRolled(GameId id, int rolledNumber, PlayerId player)
            : base(id)
        {
            RolledNumber = rolledNumber;
            Player = player;
        }
    }

    public class GameStarted : GameEvent
    {
        public List<PlayerId> Players { get; private set; }
        public Turn InitialTurn { get; private set; }

        public GameStarted(GameId id, List<PlayerId> players, Turn initialTurn)
            : base(id)
        {
            Players = players;
            InitialTurn = initialTurn;
        }
    }

    public class GameContinued : GameEvent
    {
        public List<PlayerId> Players { get; private set; }
        public List<KeyValuePair<PlayerId, int>> RolledNumbers { get; private set; }
        public Turn CurrentTurn { get; private set; }

        public GameContinued(GameId id, List<PlayerId> players, List<KeyValuePair<PlayerId, int>> rolledNumbers, Turn currentTurn)
            : base(id)
        {
            Players = players;
            RolledNumbers = rolledNumbers;
            CurrentTurn = currentTurn;
        }
    }

    public class TurnChanged : GameEvent
    {
        public Turn Turn { get; private set; }

        public TurnChanged(GameId id, Turn turn)
            : base(id)
        {
            Turn = turn;
        }
    }

    public class TurnCountdownUpdated : GameEvent
    {
        public int SecondsLeft { get; private set; }

        public TurnCountdownUpdated(GameId id, int secondsLeft)
            : base(id)
        {
            SecondsLeft = secondsLeft;
        }
    }

    public class TurnTimedOut : GameEvent
    {
        public TurnTimedOut(GameId id)
            : base(id)
        {
        }
    }

    public class GameFinished : GameEvent
    {
        public List<PlayerId> Winners { get; private set; }

        public GameFinished(GameId id, List<PlayerId> winners)
            : base(id)
        {
            Winners = winners;
        }
    }
}
