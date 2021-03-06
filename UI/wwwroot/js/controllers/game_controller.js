﻿(function () {
    "use strict";

    var module = angular.module('dice-game');

    module.controller('GameController', function ($scope, $rootScope, commandService) {

        $scope.selectedPlayer = $rootScope.game.turn.currentPlayer.value;

        $scope.winners = [];

        $scope.gameFinished = false;

        $scope.playAgain = function () {
            $rootScope.gameId = null;
            $rootScope.game = null;
            $rootScope.page = "create";
        };

        $scope.hidePopoverTimeout = null;

        $scope.roll = function () {
            commandService.roll($scope.gameId, $scope.selectedPlayer, function (data) {
                if (data.result) {
                    if ($scope.hidePopoverTimeout != null) {
                        clearTimeout($scope.hidePopoverTimeout);
                    }
                    $('#roll').popover({
                        content: function () {
                            return cheeckytalk();
                        },
                        placement: 'right',
                        trigger: 'manual'
                    }).popover('show');
                    $scope.hidePopoverTimeout = setTimeout(function () {
                        $('#roll').popover('hide');
                    }, 1500);
                } else {
                    alert('Unexpected error occurred, try again later.');
                }
            });
        };

        $scope.isWinner = function (player) {
            return $.inArray(player.value, $scope.winners) !== -1;
        };

        $rootScope.$on('events.TurnChanged', function (event, data) {
            $scope.selectedPlayer = data.turn.currentPlayer.value;
            $scope.$apply();
        });

        $rootScope.$on('events.GameFinished', function (event, data) {
            $scope.gameFinished = true;
            $scope.winners = data.winners.map(p => p.value);
            $scope.$apply();
        });

        function cheeckytalk() {
            var talk = [
                'Is this the best u can do??',
                'No way!!',
                'You better get some practice...',
                'Fo realzzz??',
                'Get outta here!!',
                'No luck today.',
                'No way you are going to win.',
                'The dice must been compromised.',
                'Cheater ... :)'
            ];
            return talk[Math.random() * talk.length | 0];
        }

    });

})();