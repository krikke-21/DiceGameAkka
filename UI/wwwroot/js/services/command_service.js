(function () {
    "use strict";

    var module = angular.module("dice-game");

    module.service("commandService", function ($http) {

        var baseUrl = "http://localhost:7636/api/";

        return {
            createGame: function (success, error) {
                $http.post(baseUrl + "game/create")
                    .then(function (response) {
                        success(response.data);
                    }, function (resp) {
                        error();
                    });
            },
            continueGame: function (gameId, success, error) {
                $http.post(baseUrl + "game/continue", { GameId: gameId })
                    .then(function (response) {
                        success(response.data);
                    }, function (resp) {
                        error();
                    });
            },
            startGame: function (gameId, playersCount, success, error) {
                var players = [];
                for (var i = 0; i < playersCount; i++) {
                    players[i] = "player " + (i + 1);
                }

                $http.post(baseUrl + "game/start", { GameId: gameId, Players: players })
                    .then(function (response) {
                        success(response.data);
                    }, function (resp) {
                        error();
                    });
            },
            roll: function (gameId, player, success, error) {
                $http.post(baseUrl + "game/roll", { GameId: gameId, PlayerId: player })
                    .then(function (response) {
                        success(response.data);
                    }, function (resp) {
                        error();
                    });
            }
        };
    });

})();