
angular
    .module('ApsApp')
    .controller('modalidadesInicioCtrl', ['$scope', 'comun', '$rootScope', '$http', '$uibModal', function($scope, comun, $rootScope, $http, $uibModal)
        {
//            $http.get(comun.urlBackend + 'tipoEntidad').success(function(res) {
//                $scope.listaTiposEnts = res.data;
//            });

            $http.get(comun.urlBackend + 'modalidades').success(function(res) {
                $rootScope.lista = res.data;
            });

            $scope.muestraForm = function(item)
            {
                var objetos = {};
                objetos.item = item;
                $rootScope.instanciaModal = $uibModal.open({
                    animation: true,
                    templateUrl: 'app/svd/clasificadores/modalidades/modalidades-form.html',
                    size: 'md',
//                backdrop : false,
                    resolve: {
                        objetos: function() {
                            return  objetos;
                        }
                    },
                    controller: item ? 'modalidadesEditarCtrl' : 'modalidadesNuevoCtrl'
                });
            }
        }])
    .controller('modalidadesNuevoCtrl', ['objetos', '$scope', 'comun', '$rootScope', '$http', function(objetos, $scope, comun, $rootScope, $http)
        {
            $scope.subtituloForm = 'Añadir una nueva Modalidad';
            $scope.contexto = {};
            $scope.contexto.disabled = false;
            $scope.guardar = function() {
                $http.post(comun.urlBackend + 'modalidades', $scope.contexto).success(function(res) {
                    if (res.mensaje == 'creado')
                    {
                        $rootScope.lista.push(res.data);
                        $scope.contexto = {};
                        $rootScope.instanciaModal.close();
                    }
                    else
                    {
                        angular.element('#mensajeErr').show(300)
                        $scope.mensajeErr = res.mensaje + '. Se está intentando ingresar un código que ya existe.'
                    }
                });
            };
        }])
    .controller('modalidadesEditarCtrl', ['objetos', '$scope', 'comun', '$rootScope', '$http', function(objetos, $scope, comun, $rootScope, $http)
        {
            $scope.subtituloForm = 'Actualizar Modalidad';
            $http.get(comun.urlBackend + 'modalidades/' + objetos.item.cModalidad).success(function(res) {
                $scope.contexto = res.data;
                $scope.contexto.disabled = true;
            });

            $scope.guardar = function() {
                $http.put(comun.urlBackend + 'modalidades/' + objetos.item.cModalidad, $scope.contexto).success(function(res) {
                    if (res.mensaje) {
                        var elemento = util.encontrarElemento(objetos.item.cModalidad, $rootScope.lista, 'cModalidad');
                        $rootScope.lista[elemento.indice] = res.data;
                        $scope.contexto = {};
                        $rootScope.instanciaModal.close();
                    }
                });
            };
        }]);
