(function () {
    "use strict";

    var module = angular.module('dice-game');

    module.controller('CreateGameController', function ($scope, $rootScope, commandService, eventService) {

        $scope.loading = false;
        $scope.continue = false;
        $scope.gameId = null;

        $scope.createNewGame = function () {
            $scope.loading = true;
            commandService.createGame(function (data) {
                $scope.loading = false;
                $rootScope.gameId = data.gameId.value;
                $rootScope.page = "choose_players";
                eventService.connect($scope.gameId);
            }, function () {
                $scope.loading = false;
                alert('Unexpected error occurred, try again later.');
            });
        };

        $scope.continueGame = function () {
            $scope.loading = true;
            commandService.continueGame($scope.gameId, function () {
                $rootScope.gameId = $scope.gameId;
                eventService.connect($scope.gameId);
            }, function () {
                $scope.loading = false;
                alert('Unexpected error occurred, try again later.');
            });
        };
    });

})();
