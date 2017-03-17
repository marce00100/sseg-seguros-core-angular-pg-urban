
angular
    .module('ApsApp')
    .controller('tipoEntidadInicioCtrl', ['$scope', 'comun', '$rootScope', '$http', '$uibModal', function($scope, comun, $rootScope, $http, $uibModal)
        {
            $http.get(comun.urlBackend + 'tipoEntidad').success(function(res) {
                $rootScope.lista = res.data;
            });


            $scope.muestraForm = function(item)
            {
                var objetos = {};
                objetos.lista = $scope.lista;
                objetos.item = item;
                $rootScope.instanciaModal = $uibModal.open({
                    animation: true,
                    templateUrl: 'app/svd/clasificadores/tipoEntidad/tipoEntidad-form.html',
                    size: 'lg',
//                backdrop : false,
                    resolve: {
                        objetos: function() {
                            return  objetos;
                        }
                    },
                    controller: item ? 'tipoEntidadEditarCtrl' : 'tipoEntidadNuevoCtrl'
                });
            }
        }])
    .controller('tipoEntidadNuevoCtrl', ['objetos', '$scope', 'comun', '$rootScope', '$http', function(objetos, $scope, comun, $rootScope, $http)
        {
            $scope.subtituloForm = 'Añadir un Tipo de Entidad';
            $scope.lista = objetos.lista;
            $scope.contexto = {};
            $scope.contexto.disabled = false;
            $scope.guardar = function() {
                $http.post(comun.urlBackend + 'tipoEntidad', $scope.contexto).success(function(res) {
                    if (res.mensaje == 'creado')
                    {
                        $rootScope.lista.push(res.data);
                        $scope.contexto = {};
                        $rootScope.instanciaModal.close();
                    }
                    else
                    {
                        angular.element('#mensajeErr').show(300)
                        $scope.mensajeErr = res.mensaje + '. Se esta intentando ingresar un codigo que ya existe.'
                    }
                });
            };
        }])
    .controller('tipoEntidadEditarCtrl', ['objetos', '$scope', 'comun', '$rootScope', '$http', function(objetos, $scope, comun, $rootScope, $http)
        {
            $scope.subtituloForm = 'Actualizar Tipo de entidad';
            $http.get(comun.urlBackend + 'tipoEntidad/' + objetos.item.cTipoEntidad).success(function(res) {
                $scope.contexto = res.data;
                $scope.contexto.disabled = true;
            });
            $scope.guardar = function() {
                $http.put(comun.urlBackend + 'tipoEntidad/' + objetos.item.cTipoEntidad, $scope.contexto).success(function(res) {
                    if (res.mensaje) {
                        var elemento = util.encontrarElemento(objetos.item.cTipoEntidad, $rootScope.lista, 'cTipoEntidad');
                        $rootScope.lista[elemento.indice] = res.data;
                        $scope.contexto = {};
                        $rootScope.instanciaModal.close();
                    }
                });
            };
        }]);
