
angular
    .module('ApsApp')
    .controller('configuracionInicioCtrl', ['$scope', 'comun', '$rootScope', '$http', '$uibModal', function($scope, comun, $rootScope, $http, $uibModal) {

            $rootScope.contexto = {};
            $http.get(comun.urlBackend + 'configuraciones/activo').success(function(res) {
                $rootScope.contexto = res.data;
            });

            $scope.guardar = function() {
                console.log($scope.contexto);
                $http.post(comun.urlBackend + 'configuraciones/modifica', $scope.contexto).success(function(res) {
                    $scope.contexto = res.data;

                });
            };
        }]);