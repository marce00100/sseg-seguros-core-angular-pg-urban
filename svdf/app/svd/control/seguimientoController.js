angular
    .module('ApsApp')
    .controller('seguimientoCtrl', ['$scope', 'comun', '$rootScope', '$http', '$uibModal', function($scope, comun, $rootScope, $http, $uibModal)
        {
            CargaEnvios = function()
            {
                $http.get(comun.urlBackend + 'sgmnt/envios/apertura_activa').success(function(res) {
                    $scope.listaSeguimiento = res.data;
                });
            }
            CargaEnvios();
            $scope.$on('actualizar_envios', function(event, data){
                CargaEnvios();
            })

            $scope.muestraEnvios = function(elem, form)
            {
                elem.estadoApertura = $scope.$parent.aper.estado;
                var objetos = {};
                objetos.envEntidad = elem;
                $rootScope.instanciaModal = $uibModal.open({
                    animation: true,
                    templateUrl: form == "envios" ? 'app/svd/control/seg-envios-ent.html' : 'app/svd/control/res-validacion-ent.html',
                    controller: form == "envios" ? 'seguimientoEnviosCtrl' : 'resultadoValidacionEnvioCtrl',
                    size: 'lg',
////                    backdrop: false,
                    resolve: {
                        objetos: function() {
                            return  objetos;
                        }
                    },
                });
            }

            $rootScope.cancelar = function() {
                $rootScope.instanciaModal.dismiss('cancel');
            }
        }])
    .controller('seguimientoEnviosCtrl', ['$scope', 'objetos', function($scope, objetos)
        {
            $scope.envEntidad = objetos.envEntidad;
            $scope.subtituloForm = 'Histórico de Envios';// + elem.fecha_corte;
        }])
    .controller('resultadoValidacionEnvioCtrl', ['$scope', 'objetos', '$http', 'comun', function($scope, objetos, $http, comun)
        {
            $scope.subtituloForm = 'Información Técnica Financiera de archivos enviados';
            $scope.val = {}
            $scope.modValidez = {};
            $scope.entEnvio = objetos.envEntidad;
            $scope.modValidez.valido = $scope.entEnvio.valido;
            $scope.modValidez.observaciones = $scope.entEnvio.observaciones;

            $http.get(comun.urlBackend + 'validaciones/consulta/web/' + $scope.entEnvio.id_seguimiento_envio) //  , {id_seguimiento_envio: $scope.entEnvio.id_seguimiento_envio})
                .success(function(res)
                {
                    $scope.val.mensajeContenidoEF = (res.validoEF) ? "No se encontraron errores." : "Existen errores en la validación de las Partes Con Estados Financieros.";
                    if (!res.validoF)
                    {
                        $scope.val.mensajeFormato = 'Existen ERRORES DE FORMATO en los archivos'
                        $scope.val.mensajeSaldosIniFin = "No se realizó";
                        $scope.val.mensajeContenidoEF = "No se realizó";
                    }
                    else
                    {
                        $scope.val.mensajeFormato = res.erroresF.length > 0 ? 'Se encontraron ADVERTENCIAS DE FORMATO.' : 'No se encontraron errores de FORMATO.';
                    }

                    $scope.val.ctxEntidad = res.seguimientoEntidadDatos;
                    $scope.val.ctxApertura = res.aperturaDatos;

                    $scope.val.validoF = res.validoF;
                    $scope.val.erroresF = res.erroresF;
                    if (res.validoF)
                    {
                        $scope.val.validoEF = res.validoEF;
                        $scope.val.datosEF = res.validacionEF;
                        $scope.val.validoC = res.validoC;
                        $scope.val.datosContenido = comun.arreglaListaValContenido(res.datosC);
                    }

                });

//            $http.post(comun.urlBackend + 'validaciones/consulta/reporte', {id_seguimiento_envio: $scope.entEnvio.id_seguimiento_envio}).success(function(res)
//            {});




            $scope.mostrarObservaciones = function()
            {
                angular.element("#divValidarSeguimiento").toggle(300);
            }

            $scope.modificarValidez = function()
            {
                $scope.entEnvio.observaciones = $scope.modValidez.observaciones;
                $scope.entEnvio.valido = $scope.modValidez.valido;
                $http.put(comun.urlBackend + 'sgmnt/validez', {
                    id_seguimiento_envio: $scope.entEnvio.id_seguimiento_envio,
                    valido: $scope.entEnvio.valido, observaciones: $scope.entEnvio.observaciones}).success(function(res) {
                });
                $scope.instanciaModal.dismiss('cancel');
            }


        }])
    ;
