﻿using Akka.Actor;
using Akka.Event;
using AkkaMjrTwo.Domain;
using System.Linq;
using AkkaMjrTwo.StatisticsEngine.ReadModels;

namespace AkkaMjrTwo.StatisticsEngine.Projectors
{
    public class StatisticsProjectorActor : ReceiveActor
    {
        public StatisticsProjectorActor()
        {
            Initialize();
        }

        public static Props GetProps()
        {
            return Props.Create<StatisticsProjectorActor>();
        }

        private void Initialize()
        {
            Receive<GameStarted>(Project);
            Receive<DiceRolled>(Project);
            Receive<GameFinished>(Project);
        }

        private static void Project(GameStarted @event)
        {
            var gameId = @event.Id.Value;

            using (var db = new GameStatisticsContext())
            {
                foreach (var player in @event.Players)
                {
                    db.Add(new GameStatistic
                    {
                        GameId = gameId,
                        PlayerId = player.Value
                    });
                }
                db.SaveChanges();
            }
        }
        
        private static void Project(DiceRolled @event)
        {
            var gameId = @event.Id.Value;
            var player = @event.Player.Value;

            using (var db = new GameStatisticsContext())
            {
                var statistic = db.Statistics.FirstOrDefault(s => s.GameId.Equals(gameId) && s.PlayerId.Equals(player));
                if (statistic != null)
                {
                    statistic.NumberRolled = @event.RolledNumber;
                    db.SaveChanges();
                }
                else
                {
                    Context.GetLogger().Warning("Unable to find GameStatistic readmodel for game id {0} and player id {1}", gameId, player);
                }
            }
        }

        private static void Project(GameFinished @event)
        {
            var gameId = @event.Id.Value;

            using (var db = new GameStatisticsContext())
            {
                foreach (var player in @event.Winners)
                {
                    var statistic = db.Statistics.FirstOrDefault(s => s.GameId.Equals(gameId) && s.PlayerId.Equals(player.Value));
                    if (statistic != null)
                    {
                        statistic.Winner = true;
                    }
                    else
                    {
                        Context.GetLogger().Warning("Unable to find GameStatistic readmodel for game id {0} and player id {1}", gameId, player);
                    }
                }
                db.SaveChanges();
            }
        }
    }
}
