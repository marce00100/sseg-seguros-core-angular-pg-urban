angular
    .module('ApsApp')
    .controller('controlCtrl', ['$scope', 'comun', '$http', function($scope, comun, $http)
        {
            var user = usuarios.usuarioExt1;
            $scope.cot = {};
            $scope.aper = {};

            var fechaActual = new Date();
            var fechaCorte = '';

            $http.get(comun.urlBackend + 'apertura/activo').success(function(res) {
                if (res.data == null) //caso particular cuando no hay ninguna apertura activa, o la tabla esta vacia  en la base de datos
                {
                    $scope.aper.iniciado = false;
                    fechaCorte = util.getUltimoDiaMesAnterior(fechaActual);
                }
                else
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


            //busca las cotizaciones de la fecha, se llama al inicio y cuando se cambia la fecha de _corte
            $scope.buscafechaCotizaciones = function()
            {
                fechaCorte = new Date(parseInt($scope.cot.ano), parseInt($scope.cot.mes) - 1, parseInt($scope.cot.dia));
                $scope.cot.dia_semana = fechaCorte.getDay();

                $http.get(comun.urlBackend + 'cotizaciones/' + $scope.cot.ano + '-' + $scope.cot.mes + '-' + $scope.cot.dia).success(function(res) {
                    $scope.cot.cotizacion = res.data;
                    $scope.$broadcast('actualizar_envios', '');
                    ($scope.cot.cotizacion == null) ? angular.element("#msjNoCotizacion").show(200) : angular.element("#msjNoCotizacion").hide(200);
                });
            }


            $scope.iniciar_detener = function()
            {
                var opcion = confirm("Se procederá a  " + ($scope.aper.iniciado ? " DAR INICIO " : " DETENER ")
                    + "el envío de archivos para las entidades, para la FECHA DE CORTE configurada. \n"
                    + " ¿¿ Está seguro de continuar ??");
                if (opcion)
                {
                    if ($scope.aper.iniciado)
                    {
                        $http.post(comun.urlBackend + 'apertura/iniciar', {fecha_corte: $scope.cot.ano + '-' + $scope.cot.mes + '-' + $scope.cot.dia})
                            .success(function(resp) {
                                $http.get(comun.urlBackend + 'apertura/activo').success(function(res) {
                                    $scope.aper = res.data;
                                    $scope.$broadcast('actualizar_envios', '');
                                })
                            })
                    }
                    else
                    {
                        $http.put(comun.urlBackend + 'apertura/detener', $scope.aper.id_apertura)
                            .success(function(resp) {
                                $http.get(comun.urlBackend + 'apertura/activo').success(function(res) {
                                    $scope.aper = res.data;
                                    $scope.$broadcast('actualizar_envios', '');
                                })
                            })
                    }
                }
                else
                {
                    $scope.aper.iniciado = !$scope.aper.iniciado;
                }
            }



        }]);