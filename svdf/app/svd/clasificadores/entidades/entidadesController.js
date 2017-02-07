
angular
    .module('ApsApp')
    .controller('entidadesInicioCtrl', ['$scope', 'comun', '$rootScope', '$http', '$uibModal', function($scope, comun, $rootScope, $http, $uibModal)
        {
            $http.get(comun.urlBackend + 'tipoEntidad').success(function(res) {
                $scope.listaTiposEnts = res.data;
            });

            $http.get(comun.urlBackend + 'entidades').success(function(res) {
                $rootScope.listaEntidades = res.data;
            });

            $scope.muestraForm = function(item)
            {
                var objetos = {};
                objetos.listaTiposEnts = $scope.listaTiposEnts;
                objetos.tipoEntidadSel = $scope.cmbFiltroTiposEntidades;
                objetos.item = item;
                $rootScope.instanciaModal = $uibModal.open({
                    animation: true,
                    templateUrl: 'app/svd/clasificadores/entidades/entidades-form-modal.html',
                    size: 'lg',
//                backdrop : false,
                    resolve: {
                        objetos: function() {
                            return  objetos;
                        }
                    },
                    controller: item ? 'entidadesEditarCtrl' : 'entidadesNuevoCtrl'
                });
            }
        }])
    .controller('entidadesNuevoCtrl', ['objetos', '$scope', 'comun', '$rootScope', '$http', function(objetos, $scope, comun, $rootScope, $http)
        {
            $scope.subtituloForm = 'Añadir una nueva Entidad';
            $scope.listaTiposEnts = objetos.listaTiposEnts;
            $scope.contexto = {};
            $scope.contexto.disabled = false;
            $scope.contexto.bHabilitado = 'S';
            $scope.contexto.cTipoEntidad = (objetos.tipoEntidadSel == null) ? null : objetos.tipoEntidadSel.cTipoEntidad;  // si no se selecciona ninguno
            $scope.guardar = function() {
                $http.post(comun.urlBackend + 'entidades', $scope.contexto).success(function(res) {
                    if (res.mensaje == 'creado')
                    {
                        $rootScope.listaEntidades.push(res.data);
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
    .controller('entidadesEditarCtrl', ['objetos', '$scope', 'comun', '$rootScope', '$http', function(objetos, $scope, comun, $rootScope, $http)
        {
            $scope.subtituloForm = 'Actualizar Entidad';
            $scope.listaTiposEnts = objetos.listaTiposEnts;
            $http.get(comun.urlBackend + 'entidades/' + objetos.item.cEmpresa).success(function(res) {
                $scope.contexto = res.data;
                $scope.contexto.disabled = true;
            });

            $scope.guardar = function() {
                $http.put(comun.urlBackend + 'entidades/' + objetos.item.cEmpresa, $scope.contexto).success(function(res) {
                    if (res.mensaje) {
                        var elemento = util.encontrarElemento(objetos.item.cEmpresa, $rootScope.listaEntidades, 'cEmpresa');
                        $rootScope.listaEntidades[elemento.indice] = res.data;
                        $scope.contexto = {};
                        $rootScope.instanciaModal.close();
                    }
                });
            };
        }]);
