using Akka.Actor;
using DiceGame.Akka.Domain;
using DiceGame.Akka.GameEngine.Actor;
using DiceGame.Akka.GameEngine.Attributes;
using DiceGame.Akka.GameEngine.Infrastructure;
using DiceGame.Akka.GameEngine.Model;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace DiceGame.Akka.GameEngine.Controllers
{
    [Route("api/game")]
    [ApiController]
    public class GameController : ControllerBase
    {
        private readonly IActorRef _gameManagerActor;

        public GameController(GameManagerActorProvider gameManagerActorProvider)
        {
            _gameManagerActor = gameManagerActorProvider();
        }

        [RequestLoggingActionFilter]
        [Route("create")]
        [HttpPost]
        public async Task<ActionResult> Create()
        {
            var feedback = await _gameManagerActor.Ask<GameCreated>(new CreateGame());
            return Ok(feedback);
        }

        [RequestLoggingActionFilter]
        [Route("start")]
        [HttpPost]
        public async Task<ActionResult> Start(StartGameRequest request)
        {
            var playerIds = new List<PlayerId>();
            foreach (var str in request.Players)
            {
                playerIds.Add(new PlayerId(str));
            }

            var msg = new SendCommand(new GameId(request.GameId), new StartGame(playerIds.ToImmutableList()));

            var feedback = await _gameManagerActor.Ask<object>(msg);
            return Ok(new { Result = feedback.GetType().Name });
        }

        [RequestLoggingActionFilter]
        [Route("continue")]
        [HttpPost]
        public async Task<ActionResult> Continue(ContinueGameRequest request)
        {
            var msg = new SendCommand(new GameId(request.GameId), new ContinueGame());
            
            var feedback = await _gameManagerActor.Ask<object>(msg);
            return Ok(feedback);
        }

        [RequestLoggingActionFilter]
        [Route("roll")]
        [HttpPost]
        public async Task<ActionResult> Roll(RollDiceRequest request)
        {
            var msg = new SendCommand(new GameId(request.GameId), new RollDice(new PlayerId(request.PlayerId)));

            var feedback = await _gameManagerActor.Ask<object>(msg);
            return Ok(new { Result = feedback.GetType().Name });
        }
    }
}
