(function () {
    "use strict";

    var module = angular.module('dice-game');

    module.controller('StartGameController', function ($scope, commandService) {

        $scope.playersCount = 6;

        $scope.loading = false;

        $scope.hidePopoverTimeout = null;

        $scope.startGame = function () {
            $scope.loading = true;
            commandService.startGame($scope.gameId, $scope.playersCount, function (data) {
                $scope.loading = false;
            }, function () {
                alert('Unexpected error occurred, try again later.');
            });
        };

    });

})();