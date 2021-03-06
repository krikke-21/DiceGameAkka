﻿using DiceGame.Akka.Domain.Config;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DiceGame.Akka.Domain
{
    public abstract class Game : AggregateRoot<Game, GameEvent>
    {
        public bool IsFinished => this is FinishedGame;
        public bool IsRunning => this is RunningGame;

        protected GameId GameId => Id as GameId;

        protected Game(GameId id)
            : base(id)
        {
        }

        public static UninitializedGame Create(GameId id)
        {
            return new UninitializedGame(id);
        }

        public Game HandleCommand(GameCommand command)
        {
            if (command is StartGame startCmd)
            {
                if (this is UninitializedGame uninitializedGame)
                {
                    return uninitializedGame.Start(startCmd.Players);
                }
                else throw new GameAlreadyStartedViolation();
            }

            if (command is ContinueGame)
            {
                if (this is RunningGame runningGame)
                {
                    return runningGame.Continue();
                }
                else throw new GameNotRunningViolation();

            }

            if (command is RollDice rollCmd)
            {
                if (this is RunningGame runningGame)
                {
                    return runningGame.Roll(rollCmd.Player);
                }
                else throw new GameNotRunningViolation();
            }
            return this;
        }
    }



    public class UninitializedGame : Game
    {
        public UninitializedGame(GameId id)
            : base(id)
        {
        }

        public Game Start(ImmutableList<PlayerId> players)
        {
            if (players.Count() < 2)
            {
                throw new NotEnoughPlayersViolation();
            }

            var firstPlayer = players.First();

            RegisterUncommitedEvents(
                new GameStarted(GameId, 
                                players, 
                                new Turn(firstPlayer, GlobalSettings.TurnTimeoutSeconds))
                );

            return this;
        }

        public override Game ApplyEvent(GameEvent @event)
        {
            Game game = this;
            if (@event is GameStarted gameStarted)
            {
                game = new RunningGame(GameId, gameStarted.Players, gameStarted.InitialTurn, UncommitedEvents);
            }

            MarkCommitted(@event);

            return game;
        }
    }



    public class RunningGame : Game
    {
        private readonly Random _random;
        private readonly Dictionary<PlayerId, int> _rolledNumbers;
        private readonly ImmutableList<PlayerId> _players;

        private Turn _turn;

        public RunningGame(GameId id, ImmutableList<PlayerId> players, Turn turn, List<GameEvent> uncommitedEvents)
            : base(id)
        {
            _random = new Random();
            _rolledNumbers = new Dictionary<PlayerId, int>();
            _players = players;
            _turn = turn;

            UncommitedEvents = uncommitedEvents;
        }

        public Game Continue()
        {
            _turn.SecondsLeft = GlobalSettings.TurnTimeoutSeconds;

            RegisterUncommitedEvents(
                new GameContinued(GameId,
                                  _players.ToImmutableList<PlayerId>(),
                                  _rolledNumbers.ToImmutableList<KeyValuePair<PlayerId, int>>(),
                                  _turn)
                );

            return this;
        }

        public Game Roll(PlayerId player)
        {
            if (_turn.CurrentPlayer.Equals(player))
            {
                var rolledNumber = _random.Next(1, 7);
                var diceRolled = new DiceRolled(GameId, rolledNumber, player);

                var nextPlayer = GetNextPlayer();
                if (nextPlayer != null)
                {
                    RegisterUncommitedEvents(
                        diceRolled, new TurnChanged(
                            GameId, 
                            new Turn(nextPlayer, GlobalSettings.TurnTimeoutSeconds))
                        );
                }
                else
                {
                    //collect all previous rolls to finish the game
                    var rolls = _rolledNumbers.ToList();
                    //add the last roll
                    rolls.Add(new KeyValuePair<PlayerId, int>(diceRolled.Player, diceRolled.RolledNumber));

                    var bestPlayers = BestPlayers(rolls);
                    RegisterUncommitedEvents(diceRolled, new GameFinished(GameId, bestPlayers.ToImmutableList<PlayerId>()));
                }
                return this;
            }
            else throw new NotCurrentPlayerViolation();
        }

        public Game TickCountDown()
        {
            var countdownUpdated = new TurnCountdownUpdated(GameId, _turn.SecondsLeft - 1);
            if (_turn.SecondsLeft <= 1)
            {
                var timedOut = new TurnTimedOut(GameId);
                var nextPlayer = GetNextPlayer();
                if (nextPlayer != null)
                {
                    RegisterUncommitedEvents(timedOut, new TurnChanged(GameId, new Turn(nextPlayer, GlobalSettings.TurnTimeoutSeconds)));
                }
                else
                {
                    var bestPlayers = BestPlayers(_rolledNumbers);
                    RegisterUncommitedEvents(timedOut, new GameFinished(GameId, bestPlayers.ToImmutableList<PlayerId>()));
                }
            }
            else
            {
                RegisterUncommitedEvents(countdownUpdated);
            }
            return this;
        }

        public override Game ApplyEvent(GameEvent @event)
        {
            Game game = this;
            if (@event is TurnChanged turnChanged)
            {
                _turn = turnChanged.Turn;
            }
            if (@event is DiceRolled diceRolled)
            {
                if (!_rolledNumbers.Any(x => x.Key.Equals(diceRolled.Player)))
                {
                    _rolledNumbers.Add(diceRolled.Player, diceRolled.RolledNumber);
                }
            }
            if (@event is TurnCountdownUpdated turnCountdownUpdated)
            {
                _turn.SecondsLeft = turnCountdownUpdated.SecondsLeft;
            }
            if (@event is GameFinished gameFinished)
            {
                game = new FinishedGame(GameId, _players, gameFinished.Winners, UncommitedEvents);
            }

            MarkCommitted(@event);

            return game;
        }

        private static List<PlayerId> BestPlayers(IEnumerable<KeyValuePair<PlayerId, int>> rolls)
        {
            var best = new List<PlayerId>();

            if (!rolls.Any())
                return best;

            var highest = rolls.Select(x => x.Value).Max();
            best = rolls.Where(x => x.Value == highest).Select(x => x.Key).ToList();

            return best;
        }

        private PlayerId GetNextPlayer()
        {
            var currentPlayerIndex = _players.IndexOf(_turn.CurrentPlayer);
            var nextPlayerIndex = currentPlayerIndex + 1;

            return _players.ElementAtOrDefault(nextPlayerIndex);
        }
    }



    public class FinishedGame : Game
    {
        public ImmutableList<PlayerId> Players { get; private set; }
        public ImmutableList<PlayerId> Winners { get; private set; }

        public FinishedGame(GameId id, ImmutableList<PlayerId> players, ImmutableList<PlayerId> winners, List<GameEvent> uncommitedEvents)
            : base(id)
        {
            Players = players;
            Winners = winners;
            UncommitedEvents = uncommitedEvents;
        }

        public override Game ApplyEvent(GameEvent arg)
        {
            return this;
        }
    }



    public class Turn
    {
        public PlayerId CurrentPlayer { get; set; }
        public int SecondsLeft { get; set; }

        public Turn(PlayerId currentPlayer, int secondsLeft)
        {
            CurrentPlayer = currentPlayer;
            SecondsLeft = secondsLeft;
        }
    }
}
