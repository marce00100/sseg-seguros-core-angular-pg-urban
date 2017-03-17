angular
    .module('ApsApp')
    .controller('historicosCtrl', ['$scope', 'comun', '$http', function($scope, comun, $http)
        {
            var user = usuarios.usuarioExt1;
            $scope.cot = {};
            $scope.aper = {};
            $http.get(comun.urlBackend + 'apertura/anosAperturas').success(function(res) {
                $scope.anosApertura = res.data;
                $scope.mesesApertura = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12];
            })

//            var fechaActual = new Date();
            var fechaCorte = '';
            $scope.buscarApertura = function()
            {
                $http.get(comun.urlBackend + 'apertura/ultima_de_fecha_corte/' + $scope.mesApertura + '/' + $scope.anoApertura).success(function(res) {
                    $scope.aper = res.data;
                    fechaCorte = new Date($scope.aper.fecha_corte);
                    $scope.cot.ano = fechaCorte.getFullYear();
                    $scope.cot.mes = fechaCorte.getMonth() + 1;
                    $scope.cot.dia = fechaCorte.getDate();
                    $scope.cot.dia_semana = fechaCorte.getDay();
                    $scope.buscafechaCotizaciones();

                    $scope.fechaCorteSeguimiento = util.getFecha($scope.aper.fecha_corte, 'yyyy-MM-dd');
                    $scope.$broadcast('actualizar_envios', $scope.fechaCorteSeguimiento);
                });
            }

            //busca las cotizaciones de la fecha, se llama al inicio y cuando se cambia la fecha de _corte
            $scope.buscafechaCotizaciones = function()
            {
                fechaCorte = new Date(parseInt($scope.cot.ano), parseInt($scope.cot.mes) - 1, parseInt($scope.cot.dia));
                $scope.cot.dia_semana = fechaCorte.getDay();
                $http.get(comun.urlBackend + 'cotizaciones/' + $scope.cot.ano + '-' + $scope.cot.mes + '-' + $scope.cot.dia).success(function(res) {
                    $scope.cot.cotizacion = res.data;
                    ($scope.cot.cotizacion == null) ? angular.element("#msjNoCotizacion").show(200) : angular.element("#msjNoCotizacion").hide(200);
                });
            }






        }]);