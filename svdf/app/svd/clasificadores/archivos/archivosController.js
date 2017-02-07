
angular
    .module('ApsApp')
    .controller('archivosInicioCtrl', ['$scope', 'comun', '$rootScope', '$http', '$uibModal', function($scope, comun, $rootScope, $http, $uibModal) {


            $http.get(comun.urlBackend + 'archivos').success(function(res) {
                $rootScope.listaArchivos = res.data;
            });

            $scope.muestraForm = function(item)
            {
                var objetos = {};
                objetos.item = item;
                $rootScope.instanciaModal = $uibModal.open({
                    animation: false,
                    templateUrl: 'app/svd/clasificadores/archivos/archivos-form-modal.html',
                    size: 'lg',
                    backdrop: true,
                    resolve: {
                        objetos: function() {
                            return  objetos;
                        }
                    },
                    controller: item ? 'archivosEditarCtrl' : 'archivosNuevoCtrl'
                });
            }
            $scope.eliminar = function(item)
            {
                var elimina = confirm("Está seguro de que quiere eliminar el registro " + item.nombre
                    + "???, ya no aparecerá, pero si existen registros vinculados con el, se mostrará solo con fines históricos. \n"
                    + " Recuerde que el usuario quedará registrado después de eliminar");
                if (elimina)
                {
                    item.eliminado = true;
                    var elemento = util.encontrarElemento(item.id_archivo, $rootScope.listaArchivos, 'id_archivo');
                    $http.delete(comun.urlBackend + 'archivos/' + item.id_archivo).success(function(res) {
                        $rootScope.listaArchivos.splice(elemento.indice, 1);
                        comun.colocaSubSubtitulo("ARCHIVOS", $rootScope.listaArchivos.length);
                    })
                }
            }
        }])
    .controller('archivosNuevoCtrl', ['$scope', 'comun', '$rootScope', '$http', function($scope, comun, $rootScope, $http) {
            $scope.subtituloForm = 'Añadir Tipo de Archivo ASCII';
            $scope.contexto = {};
            $scope.id_archivo = -1;
            $scope.contexto.activo = true;
            $scope.guardar = function() {
                $http.post(comun.urlBackend + 'archivos', $scope.contexto).success(function(res) {
                    if (res.mensaje)
                    {
                        $rootScope.listaArchivos.push(res.data);
                        comun.colocaSubSubtitulo("ARCHIVOS", $rootScope.listaArchivos.length);
                        $scope.contexto = {};
                        $rootScope.instanciaModal.close();

                    }
                });
            };
        }])
    .controller('archivosEditarCtrl', ['objetos', '$scope', 'comun', '$rootScope', '$http', function(objetos, $scope, comun, $rootScope, $http) {
            $scope.subtituloForm = 'Actualizar Tipo de Archivo ASCII';
            $http.get(comun.urlBackend + 'archivos/' + objetos.item.id_archivo).success(function(res) {
                $scope.contexto = res.data;
//                $scope.contexto.disabled = true;
            });
//            $scope.contexto = objetos.item;

            $scope.guardar = function() {
                $http.put(comun.urlBackend + 'archivos/' + objetos.item.id_archivo, $scope.contexto).success(function(res) {
                    if (res.mensaje) {
                        var elemento = util.encontrarElemento(objetos.item.id_archivo, $rootScope.listaArchivos, 'id_archivo');
                        $rootScope.listaArchivos[elemento.indice] = res.data;
                        $scope.contexto = {};
                        $rootScope.instanciaModal.close();
                    }
                });
            };
        }]);