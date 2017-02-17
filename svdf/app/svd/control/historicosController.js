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

            var fechaActual = new Date();
            var fechaCorte = '';
            $scope.buscarApertura = function()
            {



                $http.get(comun.urlBackend + 'apertura/ultima_de_fecha_corte/' + $scope.mesApertura + '/' + $scope.anoApertura).success(function(res) {

                    {
                        $scope.aper = res.data;
                        var periodoActual = util.getFecha(fechaActual, 'yyyyMM');
                        var periodoApertura = util.getFecha($scope.aper.fecha_inicio_envios, 'yyyyMM');
                        if (periodoApertura == periodoActual)
                            fechaCorte = new Date($scope.aper.fecha_corte);
                        else if (periodoApertura < periodoActual)
                        {
                            if ($scope.aper.iniciado)
                            {
                                fechaCorte = new Date($scope.aper.fecha_corte);
                                $scope.aper.mensajeError = 'La apertura de envio de archivos actual esta iniciada, y se refiere a una fecha de corte antigua o con al menos dos meses pasada: ' + util.getFecha($scope.aper.fecha_corte) + '. Deberá cerrar la apertura actual, para iniciar los envios del periodo correcto';
                            }
                            else
                            {
                                fechaCorte = util.getUltimoDiaMesAnterior(fechaActual);
                            }
                        }
                    }
                    $scope.cot.ano = fechaCorte.getFullYear();
                    $scope.cot.mes = fechaCorte.getMonth() + 1;
                    $scope.cot.dia = fechaCorte.getDate();
                    $scope.cot.dia_semana = fechaCorte.getDay();
                    $scope.buscafechaCotizaciones();
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