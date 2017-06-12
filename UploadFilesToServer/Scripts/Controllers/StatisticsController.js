(function () {

    var app = angular.module("MyStatistic", ["ui.bootstrap"]);
        console.log("auto detect");

    app.controller("MainCtrl", function ($scope, $http) {
        console.log("YO!!!!!!!!!!!");

        $http({
            method: "GET",
            url: "/api/getstatistics"
        }).then(function mySucces(response) {

            $scope.allCandidates = response.data;
            $scope.totalItems = $scope.allCandidates.length;
            $scope.currentPage = 1;
            $scope.itemsPerPage = 20;
            $scope.maxSize = 6;

        }, function myError(response) {
            alert(response.statusText);
        });



        $scope.show = function () {
            $scope.$watch("currentPage", function () {
                setPagingData($scope.currentPage);
            });
        }


        $scope.show();

        function setPagingData(page) {
            $scope.aCandidates = $scope.allCandidates.slice(
                (page - 1) * $scope.itemsPerPage,
                page * $scope.itemsPerPage
            );
        }
    });


})();