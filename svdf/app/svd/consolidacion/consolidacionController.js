
angular
    .module('ApsApp')
    .controller('consolidacionCtrl', ['$scope', 'comun', '$rootScope', '$http', function($scope, comun, $rootScope, $http)
        {
            $scope.ctxApertura = {};
            $scope.ctxConsolidacion = {};
            $scope.errorEnCierre = false;
            $scope.errorEnCierre2 = false;

            $http.get(comun.urlBackend + 'apertura/activo').success(function(res) {
                $scope.ctxApertura = res.data;
                $scope.ctxApertura.iniciado ? angular.element('#msjAperInicializado').show(200) : angular.element('#divCierreConsolidación').show(200);
            });
            $scope.iteraccion = 0;
            obtieneCtxConsolidacion();
            function obtieneCtxConsolidacion()
            {
                //para crear iteracciones oyentes que escuchen los cambios en los registeos y se actualicen los consolidados
                clearTimeout($rootScope.timer1)
                $scope.iteraccion++;
//                console.log($scope.iteraccion);

                $http.get(comun.urlBackend + 'consolidacion/ultimo').success(function(res) {
                    $scope.ctxConsolidacion = res.data == null ? {} : res.data;
                    habilitaBotones();
                    obtieneContextoEnvios();

                    if ($scope.ctxConsolidacion.estado == 'consolidado')
                    {
                        $http.get(comun.urlBackend + 'consolidacion/obtieneconsolidados').success(function(res)
                        {
                            $scope.ctxConsolidados = res.data;
                        })
                    }

//                    if ($scope.ctxConsolidacion.estado == "procesando")
//                        $rootScope.timer1 = setTimeout(obtieneCtxConsolidacion, 4000);
                });
            }

            function obtieneContextoEnvios()
            {
                $http.get(comun.urlBackend + 'sgmnt/envios/apertura_activa').success(function(res) {
                    $scope.listaSeguimiento = res.data;
                    var validos = 0;
                    var cerrados = 0;
                    for (var i = 0; i < res.data.length; i++)
                    {
                        item = res.data[i];
                        if (item.valido)
                            validos++
                        if (item.estado_cierre == "cerrado")
                            cerrados++;
                    }
                    $scope.validos = validos;
                    $scope.cerrados = cerrados;
                });
            }

            $scope.consolidacion = function()
            {
                $scope.errorEnCierre = null;
                var opcion = confirm("Confirma que desea realizar la consolidación y cierre de la información enviada ?? ");
                if (opcion)
                {
                    $scope.ctxConsolidacion.estado = "procesando";
                    habilitaBotones();
                    $http.post(comun.urlBackend + 'consolidacion/envios_validos').success(function(res) {
                        $scope.errorEnCierre = res.errorEnCierre;
                        console.log(res.excepcion);
                        var consolidadosLista = res.data.lista;
                        for (i = 0; i < consolidadosLista.length; i++)
                        {
                            var consolidado = consolidadosLista[i];
                            var elem = util.encontrarElemento(consolidado.id_seguimiento_envio, $scope.listaSeguimiento, 'id_seguimiento_envio');
                            $scope.listaSeguimiento[elem.indice].id_consolidacion = consolidado.id_consolidacion;
                            $scope.listaSeguimiento[elem.indice].estado_cierre = consolidado.estado_cierre;
                        }

                        $scope.ctxConsolidacion.estado = res.data.estadoCierre;
                        habilitaBotones();
                        obtieneCtxConsolidacion();
                    });
                }
                else
                {
                }
            }

            $scope.continuarCierre = function()
            {
                $scope.errorEnCierre2 = null;
                var opcion = confirm("Confirma que desea realizar la consolidación y cierre de la información enviada ?? ");
                if (opcion)
                {
                    $scope.ctxConsolidacion.estado = "procesando";
                    habilitaBotones();
                    $http.post(comun.urlBackend + 'consolidacion/re_calcular_margen_solvencia', $scope.ctxConsolidados).success(function(res) {
                        $scope.errorEnCierre2 = res.errorEnCierre2;
                        console.log(res.excepcion);
                        var consolidadosLista = res.data.lista;
                        for (i = 0; i < consolidadosLista.length; i++)
                        {
                            var consolidado = consolidadosLista[i];
                            var elem = util.encontrarElemento(consolidado.id_seguimiento_envio, $scope.listaSeguimiento, 'id_seguimiento_envio');
                            $scope.listaSeguimiento[elem.indice].id_consolidacion = consolidado.id_consolidacion;
                            $scope.listaSeguimiento[elem.indice].estado_cierre = consolidado.estado_cierre;
                        }

                        $scope.ctxConsolidacion.estado = res.data.estadoCierre;
                        habilitaBotones();
                        obtieneContextoEnvios();
                        obtieneCtxConsolidacion();
                    })
                }
            }




            function habilitaBotones() {
                if ($scope.ctxConsolidacion == null)
                    $scope.ctxConsolidacion.estado = "";
                if ($scope.ctxConsolidacion.estado == 'procesando')
                {
                    angular.element('#divProcesando').show();
                    angular.element('.btnProcesar').hide();
//                    angular.element('#divMargenSolvencia').hide();
                }
                else if ($scope.ctxConsolidacion.estado == 'cerrado')
                {

                    angular.element('#divProcesando').hide();
                    angular.element('.btnProcesar').show();
                    angular.element('#divMargenSolvencia').hide();
                }
                else if ($scope.ctxConsolidacion.estado == 'consolidado')
                {
                    angular.element('#divProcesando').hide();
                    angular.element('.btnProcesar').show();
                    angular.element('#divMargenSolvencia').show();
                }
            }


        }])