
angular
    .module('ApsApp')
    .controller('envioCtrl', ['$scope', 'comun', '$rootScope', '$http', function($scope, comun, $rootScope, $http)
        {
            var user = usuarios.usuarioExt1;

            $scope.ctxApertura = {};
            $scope.ctxEntidad = {}; // cargar este valor para mostrarlo en la primera pantalla  pantalla actualmente se introduce el COD_ENTIDAD

            $http.get(comun.urlBackend + 'apertura/activo').success(function(res) {
                $scope.ctxApertura = res.data;
                var fecha = res.data.fecha_corte.toString().split('-');
                $scope.ctxApertura.ano = parseInt(fecha[0]);
                $scope.ctxApertura.mes = parseInt(fecha[1]);
                $scope.ctxApertura.dia = parseInt(fecha[2].toString().substring(0, 2));
                $scope.ctxApertura.iniciado ? angular.element('#divIniciado').show(300) : angular.element('#msjDetenido').show(300);
            });

            $scope.archivosCargados = [];
            $scope.archivosInvalidos = [];
            $scope.uploadFiles = function(files, invalidFiles)
            {
                for (var i in files) {
                    files[i].nombre = files[i].name;
                    //se lee el contenido de cada archivo y se aumenta una propiedad por cada item denominada texto
                    util.leerArchivo(files[i]);
                    $scope.archivosCargados.push(files[i]);
                }
                for (var j in invalidFiles)
                {
                    $scope.archivosInvalidos.push(invalidFiles[j]);
                }
            }

            $scope.eliminar = function(item)
            {
                elemento = util.encontrarElemento(item.name, $scope.archivosCargados, 'name');
                $scope.archivosCargados.splice(elemento.indice, 1);
            }
            $scope.quitarArchivos = function()
            {
                $scope.archivosCargados = [];
                $scope.archivosInvalidos = [];
                $scope.errores = [];
                ocultarResultados();
            }

            $scope.validarArchivos = function()
            {
                var cod_entidad = $scope.cod_entidad; /////////TODO: user.data.entidad.id; 

                $scope.val = {}
                $scope.val.ctxApertura = $scope.ctxApertura;
                ocultarResultados();
                angular.element(".mostrarProcesando").show();
                $scope.textoProcesando = "procesando ... espere por favor!"
                ////#################         1ra VALIDACION FORMATO                   #####################################/////////////////////////
                $http.post(comun.urlBackend + 'validaciones/formato/', {cod_entidad: cod_entidad, archivosCargados: $scope.archivosCargados})
                    .success(function(res)
                    {
                        //obtiene el contexto del seguimiento y entidad y la fecha de envio para la apertura activa
                        $http.get(comun.urlBackend + 'sgmnt/activo/' + cod_entidad).success(function(res) {
                            $scope.val.ctxEntidad = res.data;
                        });

                        $scope.val.estadoValidez = res.estadoValidez;
                        $scope.val.estadoValidez_desc = res.estadoValidez_desc;
                        $scope.val.validoF = res.validoF;
                        $scope.val.erroresF = res.erroresF;

                        if (!res.validoF)
                        {
                            $scope.val.mensajeFormato = 'Se encontraron ERRORES DE FORMATO en los archivos, que se está tratando de cargar. Debe corregir dichos errores.'
                            $scope.val.mensajeContenido = "No se realizó";
                        }
                        else
                        {
                            $scope.val.mensajeFormato = $scope.val.erroresF.length > 0 ? 'Se encontraron ADVERTENCIAS DE FORMATO.' : 'No se encontraron errores de FORMATO.';

                            ////////##################################         VALIDACION DE CONTENIDO                #################///////////////////////////////////
                            $http.post(comun.urlBackend + 'validaciones/contenido', {cod_entidad: cod_entidad}).success(function(res)
                            {
                                $scope.val.estadoValidez = res.estadoValidez;
                                $scope.val.estadoValidez_desc = res.estadoValidez_desc;
                                $scope.val.validoC = res.validoC;
                                $scope.val.datosContenido = comun.arreglaListaValContenido(res.datosC);
                                $scope.val.validoEF = res.validoEF;
                                $scope.val.datosEF = res.datosEF;
                                angular.element(".mostrarProcesando").hide();
                                $scope.val.muestraResultadosValidacion = true;
                            }).error(function() {
                                ocultarResultados();
                                angular.element("#errorEnvio").show(200);
                            });
                        }
                    }).error(function() {
                    ocultarResultados();
                    angular.element("#errorEnvio").show(200);
                });
            }
            function ocultarResultados()
            {
                $scope.val.muestraResultadosValidacion = false;
                angular.element("#errorEnvio").hide();
                angular.element(".mostrarProcesando").hide();
            }
        }])