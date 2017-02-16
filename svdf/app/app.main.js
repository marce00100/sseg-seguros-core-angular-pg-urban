'use strict';

angular
    .module('ApsApp')
    .factory('comun', ['$location', '$rootScope', function($location, $rootScope) {
            var fac = {
                urlBackend: 'http://localhost:5000/svd/api/',
                arreglaListaValContenido: function(listaValContenido)
                {
                    for (var i = 0; i < listaValContenido.length; i++)
                    {
                        var valObj = listaValContenido[i];
                        valObj.cabeceras = [];
                        valObj.propiedades = []
                        valObj.cabeceras.push('err fila'); // se agrega un primer campo error donde se mostraraá el error
                        valObj.propiedades.push('error');
                        if (valObj.data.length > 0)
                        {
                            // empieza desde uno para no tomar en cuenta el primer campo que llega que es entidad
                            for (var c = 1; c < Object.keys(valObj.data[0]).length - 2; c++) //  se llenan las cabeceras desde uno porque la primera es Entidad y hasta length -2 porque las dos ultimas propiedades son indexFila y FillaError 
                            {
                                valObj.propiedades.push(Object.keys(valObj.data[0])[c]);
                                var encabezado = Object.keys(valObj.data[0])[c].split('_').join(' ');
                                valObj.cabeceras.push(encabezado);
                            }
                            for (var row = 0; row < valObj.data.length; row++)
                            {
                                var fila = valObj.data[row];
                                //a cada uno se le agrega el campo definido en primera propiedad de la cabecera
                                fila['error'] = fila.filaError; //.error + ', arch: ' + fila.filaError.archivo +', fila:' + fila.filaError.fila;
                            }
                        }
                    }
                    return listaValContenido;
                }
            };


            return fac;
        }])
    .run(function($rootScope, $http, comun)
    {
        $rootScope.util = util;
        $rootScope.usuarios = usuarios;

        $rootScope.cancelar = function() {
            $rootScope.instanciaModal.dismiss('cancel');
        }
    });
angular
    .module('ApsApp')
    .controller('AppCtrl', ['$scope', '$http', '$localStorage', '$rootScope', 'comun',
        function AppCtrl($scope, $http, $localStorage, $rootScope, comun)
        {
            $scope.mobileView = 767;

            $scope.app = {
                name: 'APS - Sistema de Validación de Datos',
                author: 'Dirección de Sistemas',
                version: '1.0.0',
                year: (new Date()).getFullYear(),
                layout: {
                    isSmallSidebar: false,
                    isChatOpen: false,
                    isFixedHeader: false,
                    isFixedFooter: false,
                    isBoxed: false,
                    isStaticSidebar: false,
                    isRightSidebar: false,
                    isOffscreenOpen: false,
                    isConversationOpen: false,
                    isQuickLaunch: false,
                    sidebarTheme: '',
                    headerTheme: ''
                },
                isMessageOpen: false,
                isConfigOpen: false
            };

//            $scope.user = {
//                fname: 'Samuel',
//                lname: 'Perkins',
//                jobDesc: 'Human Resources Guy',
//                avatar: 'styles/images/avatar.jpg',
//            };

            $rootScope.estilo = {
                tableHead: 'bg-dark-dark',
                modalTitle: 'bg-dark-light', //'bg-dark-light'  
                modalHeader: ' bg-bluegrey'
            }
            $rootScope.estiloAlert = {
                '1': 'bg-danger-dark',
                '2': 'bg-orange',
                '3': 'bg-orange',
                '4': 'bg-success-dark'
            }

            $rootScope.meses = ['', 'enero', 'febrero', 'marzo', 'abril', 'mayo', 'junio', 'julio', 'agosto', 'septiembre', 'octubre', 'noviembre', 'diciembre']
            $rootScope.dias = ['domingo', 'lunes', 'martes', 'miércoles', 'jueves', 'viernes', 'sábado'];

            $rootScope.esNumero = function(valor)
            {
                return angular.isNumber(valor);
            }

            $rootScope.mostrarProcesando = function(textoProcesando)
            {                
                if(textoProcesando == null)
                    textoProcesando = " procesando ..."
                $rootScope.textoProcesando = textoProcesando;
                angular.element(".mostrarProcesando").show();
                angular.element(".mostrarCargado").hide();
            }
            $rootScope.mostrarContenidoCargado = function()
            {
                angular.element(".mostrarProcesando").hide(200);
                angular.element(".mostrarCargado").show();
            }



            if (angular.isDefined($localStorage.layout)) {
                $scope.app.layout = $localStorage.layout;
            }
            else {
                $localStorage.layout = $scope.app.layout;
            }

            $scope.$watch('app.layout', function() {
                $localStorage.layout = $scope.app.layout;
            }, true);

            $scope.getRandomArbitrary = function() {
                return Math.round(Math.random() * 100);
            };


        }
    ])

