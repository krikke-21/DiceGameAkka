﻿(function () {
    "use strict";

    var module = angular.module('dice-game', []);

    module.run(function ($rootScope, eventService) {

        $rootScope.page = "create"; // one of: "create", "choose_players", "game"
        $rootScope.gameId = null;
        $rootScope.game = null;

        $rootScope.$on('events.GameStarted', function (event, data) {
            $rootScope.game = {
                players: data.players.map(p => p.value),
                turn: data.initialTurn,
                scores: []
            };
            $rootScope.page = "game";
            $rootScope.$apply();
        });

        $rootScope.$on('events.GameContinued', function (event, data) {
            var scores = [];

            for (var i in data.rolledNumbers) {
                var kvp = data.rolledNumbers[i];
                var rolledNumber = kvp.Value;
                scores.push({
                    player: kvp.Key,
                    score: rolledNumber
                });
            }
            $rootScope.gameId = data.id.value;
            $rootScope.game = {
                players: data.players.map(p => p.value),
                turn: data.currentTurn,
                scores: scores
            };
            $rootScope.page = "game";
            $rootScope.$apply();
        });

        $rootScope.$on('events.TurnCountdownUpdated', function (event, data) {
            $rootScope.game.turn.secondsLeft = data.secondsLeft;
            $rootScope.$apply();
        });

        $rootScope.$on('events.TurnChanged', function (event, data) {
            $rootScope.game.turn = data.turn;
            $rootScope.$apply();
        });

        $rootScope.$on('events.TurnTimedOut', function (event, data) {
            $rootScope.game.scores.push({
                player: $rootScope.game.turn.currentPlayer,
                score: "Timed out!"
            });
            $rootScope.$apply();
        });

        $rootScope.$on('events.DiceRolled', function (event, data) {
            $rootScope.game.scores.push({
                player: $rootScope.game.turn.currentPlayer,
                score: data.rolledNumber
            });
            $rootScope.$apply();
        });

        $rootScope.$on('events.GameFinished', function (event) {
            eventService.disconnect();
            $rootScope.$apply();
        });

    });

})();