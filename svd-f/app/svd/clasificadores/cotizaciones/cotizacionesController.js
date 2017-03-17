

angular
    .module('ApsApp')
    .controller('cotizacionesInicioCtrl', ['$scope', 'comun', '$rootScope', '$http', '$uibModal', function($scope, comun, $rootScope, $http, $uibModal) {
            
            
            $http.get(comun.urlBackend + 'cotizaciones/ultimas').success(function(res) {
                $rootScope.listaUltimasCots = res.data;
            });

            $scope.muestraForm = function(item)
            {
                var objetos = {};
                objetos.listaMonedas = $scope.listaMonedas;
                objetos.item = item;
                $rootScope.instanciaModal = $uibModal.open({
                    animation: true,
                    templateUrl: 'app/svd/clasificadores/cotizaciones/cotizaciones-form-modal.html',
//                    size: 'lg',
//                backdrop : false,
                    resolve: {
                        objetos: function() {
                            return  objetos;
                        }
                    },
                    controller: item ? 'cotizacionesEditarCtrl' : 'cotizacionesNuevoCtrl'
                });
            }
        }])
    .controller('cotizacionesNuevoCtrl', ['objetos', '$scope', 'comun', '$rootScope', '$http', function(objetos, $scope, comun, $rootScope, $http) {
            $scope.subtituloForm = 'Añadir cotización de monedas';
            $scope.contexto = {};
            $scope.guardar = function() {

                $scope.contexto.fTipoCambio = util.setFecha($scope.contexto.fTipoCambio);
                $http.post(comun.urlBackend + 'cotizaciones', $scope.contexto).success(function(res) {
                    if (res.mensaje)
                    {
                        $rootScope.listaUltimasCots.push(res.data);
                        $scope.contexto = {};
                        $rootScope.instanciaModal.close();
                    }
                });
            };
        }])
    .controller('cotizacionesEditarCtrl', ['objetos', '$scope', 'comun', '$rootScope', '$http', function(objetos, $scope, comun, $rootScope, $http) {
            $scope.subtituloForm = 'Modificar cotización de monedas';
            var fTipoCambio = util.getFecha(objetos.item.fTipoCambio, 'yyyy-MM-dd');
            $http.get(comun.urlBackend + 'cotizaciones/' + fTipoCambio).success(function(res) {
                res.data.fTipoCambio = util.getFecha(res.data.fTipoCambio, 'dd/MM/yyyy')
                $scope.contexto = res.data;
                $scope.contexto.disabledFecha = true;
            });

            $scope.guardar = function() {
                $scope.contexto.fTipoCambio = util.setFecha($scope.contexto.fTipoCambio);
                $http.put(comun.urlBackend + 'cotizaciones/' + fTipoCambio, $scope.contexto).success(function(res) {
                    if (res.mensaje) {
                        var cotizacion = res.data;
                        var elemento = util.encontrarElemento(objetos.item.fTipoCambio, $rootScope.listaUltimasCots, 'fTipoCambio');
                        $rootScope.listaUltimasCots[elemento.indice] = cotizacion;
                        $scope.contexto = {};
                        $rootScope.instanciaModal.close();
                    }
                });
            };
        }]);