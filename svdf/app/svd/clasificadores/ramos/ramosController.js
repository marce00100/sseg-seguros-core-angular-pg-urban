
angular
    .module('ApsApp')
    .controller('ramosInicioCtrl', ['$scope', 'comun', '$rootScope', '$http', '$uibModal', function($scope, comun, $rootScope, $http, $uibModal)
        {
            $http.get(comun.urlBackend + 'modalidades').success(function(res) {
                $scope.listaMods = res.data;
            });

            $http.get(comun.urlBackend + 'ramos').success(function(res) {
                $rootScope.lista = res.data;
            });

            $scope.muestraForm = function(item)
            {
                var objetos = {};
                objetos.listaMods = $scope.listaMods;
                objetos.tipoModSel = $scope.cmbFiltroMods;
                objetos.item = item;
                $rootScope.instanciaModal = $uibModal.open({
                    animation: true,
                    templateUrl: 'app/svd/clasificadores/ramos/ramos-form.html',
                    size: 'md',
//                backdrop : false,
                    resolve: {
                        objetos: function() {
                            return  objetos;
                        }
                    },
                    controller: item ? 'ramosEditarCtrl' : 'ramosNuevoCtrl'
                });
            }
        }])
    .controller('ramosNuevoCtrl', ['objetos', '$scope', 'comun', '$rootScope', '$http', function(objetos, $scope, comun, $rootScope, $http)
        {
            $scope.subtituloForm = 'Añadir nuevo Ramo';
            $scope.listaMods = objetos.listaMods;
            $scope.contexto = {};
            $scope.contexto.disabled = false;
            $scope.contexto.cModalidad = (objetos.tipoModSel == null) ? null : objetos.tipoModSel.cModalidad;  // si no se selecciona ninguno
            $scope.guardar = function() {
                $http.post(comun.urlBackend + 'ramos', $scope.contexto).success(function(res) {
                    if (res.mensaje == 'creado')
                    {
                        $rootScope.lista.push(res.data);
                        $scope.contexto = {};
                        $rootScope.instanciaModal.close();
                    }
                    else
                    {
                        angular.element('#mensajeErr').show(300)
                        $scope.mensajeErr = res.mensaje + '. Se está intentando ingresar un código que ya existe. Codigo de Modalidad y de Ramo.'
                    }
                });
            };
        }])
    .controller('ramosEditarCtrl', ['objetos', '$scope', 'comun', '$rootScope', '$http', function(objetos, $scope, comun, $rootScope, $http)
        {
            $scope.subtituloForm = 'Actualizar RAMO';
            $scope.listaMods = objetos.listaMods;
            $http.get(comun.urlBackend + 'ramos/' + objetos.item.cModalidad + '/' + objetos.item.cRamo).success(function(res) {
                $scope.contexto = res.data;
                $scope.contexto.disabled = true;
            });

            $scope.guardar = function() {
                $http.put(comun.urlBackend + 'ramos/' + objetos.item.cRamo, $scope.contexto).success(function(res) {
                    if (res.mensaje) {
                        var elemento = util.encontrarElemento(objetos.item.cRamo, $rootScope.lista, 'cRamo');
                        $rootScope.lista[elemento.indice] = res.data;
                        $scope.contexto = {};
                        $rootScope.instanciaModal.close();
                    }
                });
            };
        }]);
