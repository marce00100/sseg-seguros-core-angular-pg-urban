'use strict';

angular
    .module('ApsApp')
    .run(['$rootScope', '$state', '$stateParams',
        function($rootScope, $state, $stateParams) {
            $rootScope.$state = $state;
            $rootScope.$stateParams = $stateParams;
            $rootScope.$on('$stateChangeSuccess', function() {
                window.scrollTo(0, 0);
            });
            FastClick.attach(document.body);
        },
    ])
    .config(['$stateProvider', '$urlRouterProvider',
        function($stateProvider, $urlRouterProvider) {

            // For unmatched routes
            $urlRouterProvider.otherwise('/');

            // Application routes
            $stateProvider
                .state('app', {
                    abstract: true,
                    templateUrl: 'app/common/layout.html',
                })


                .state('app.dashboard', {
                    url: '/',
                    templateUrl: 'app/dashboard/dashboard.html',
                    resolve: {
                        deps: ['$ocLazyLoad', function($ocLazyLoad) {
                                return $ocLazyLoad.load([
                                    {
                                        insertBefore: '#load_styles_before',
                                        files: [
                                            'styles/climacons-font.css',
                                            'vendor/rickshaw/rickshaw.min.css'
                                        ]
                                    },
                                    {
                                        serie: true,
                                        files: [
                                            'vendor/d3/d3.min.js',
                                            'vendor/rickshaw/rickshaw.min.js',
                                            'vendor/flot/jquery.flot.js',
                                            'vendor/flot/jquery.flot.resize.js',
                                            'vendor/flot/jquery.flot.pie.js',
                                            'vendor/flot/jquery.flot.categories.js',
                                        ]
                                    },
                                    {
                                        name: 'angular-flot',
                                        files: [
                                            'vendor/angular-flot/angular-flot.js'
                                        ]
                                    }]).then(function() {
                                    return $ocLazyLoad.load('app/dashboard/dashboardController.js');
                                });
                            }]
                    },
                    data: {
                        title: 'Dashboard',
                    }
                })


                // ///////////////////////////////////////// ENVIO //////////////////////////////////////////////
                .state('app.svd_envio', {
                    url: '/envio',
                    templateUrl: 'app/svd/envio/envio.html',
                    resolve: {
                        deps: ['$ocLazyLoad', function($ocLazyLoad) {
                                return $ocLazyLoad.load([
                                    {
                                        insertBefore: '#load_styles_before',
                                        files: [
                                        ]
                                    },
                                    {
                                        name: 'ngFileUpload',
                                        files: [
                                            'vendor/ng-file-upload-master/dist/ng-file-upload.min.js'
                                        ]
                                    }]).then(function() {
                                    return $ocLazyLoad.load('app/svd/envio/envioController.js');
                                });
                            }]
                    },
                    controller: 'envioCtrl',
                    data: {
                        title: 'Carga y envío de archivos',
                        contentClasses: 'full-height'
                    }
                })

                // ///////////////////////////////////////// CONTROL //////////////////////////////////////////////
                .state('app.svd_control', {
                    url: '/control',
                    templateUrl: 'app/svd/control/control.html',
                    resolve: {
                        deps: ['$ocLazyLoad', function($ocLazyLoad) {

                                return $ocLazyLoad.load([
                                    {
                                        insertBefore: '#load_styles_before',
                                        files: [
                                            'vendor/chosen_v1.4.0/chosen.min.css',
                                            'vendor/datatables/media/css/jquery.dataTables.css'
                                        ]
                                    },
                                    {
                                        serie: true,
                                        files: [
                                            'vendor/chosen_v1.4.0/chosen.jquery.min.js',
                                            'vendor/datatables/media/js/jquery.dataTables.js',
                                            'app/common/js/extentions/bootstrap-datatables.js',
                                            'app/svd/control/seguimientoController.js'
                                        ]
                                    }]).then(function() {
                                    return $ocLazyLoad.load('app/svd/control/controlController.js');
                                });
                            }]
                    },
                    controller: 'controlCtrl',
                    data: {
                        title: 'Control de envíos',
                    }
                })
                // ///////////////////////////////////////// cIERRE Y CONSOLIDACION           ////////
                .state('app.svd_consolidacion', {
                    url: '/consolidacion',
                    templateUrl: 'app/svd/consolidacion/consolidacion.html',
                    resolve: {
                        deps: ['$ocLazyLoad', function($ocLazyLoad) {
                                return $ocLazyLoad.load('app/svd/consolidacion/consolidacionController.js');
                            }]
                    },
                    controller: 'consolidacionCtrl',
                    data: {
                        title: 'CIERRE Y CONSOLIDACION',
                        contentClasses: 'full-height'
                    }
                })

                // ///////////////////////////////////////// ENVIOS HISTORICOS           ////////
                .state('app.svd_historicos', {
                    url: '/historicos',
                    templateUrl: 'app/svd/control/historicos.html',
                    resolve: {
                        deps: ['$ocLazyLoad', function($ocLazyLoad) {

                                return $ocLazyLoad.load([
                                    {
                                        insertBefore: '#load_styles_before',
                                        files: [
                                            'vendor/chosen_v1.4.0/chosen.min.css',
                                            'vendor/datatables/media/css/jquery.dataTables.css'
                                        ]
                                    },
                                    {
                                        serie: true,
                                        files: [
                                            'vendor/chosen_v1.4.0/chosen.jquery.min.js',
                                            'vendor/datatables/media/js/jquery.dataTables.js',
                                            'app/common/js/extentions/bootstrap-datatables.js',
                                            'app/svd/control/seguimientoController.js'
                                        ]
                                    }]).then(function() {
                                    return $ocLazyLoad.load('app/svd/control/historicosController.js');
                                });
                            }]
                    },
                    controller: 'historicosCtrl',
                    data: {
                        title: 'Control de envíos de históricos',
                    }
                })
                // ///////////////////////////////////////// ENTIDADES //////////////////////////////////////////////
                .state('app.svd_cla_entidades', {
                    url: '/clasificadores/entidades',
                    templateUrl: 'app/svd/clasificadores/entidades/entidades-lista.html',
                    resolve: {
                        deps: ['$ocLazyLoad', function($ocLazyLoad) {
                                return $ocLazyLoad.load('app/svd/clasificadores/entidades/entidadesController.js');
                            }]
                    },
                    controller: 'entidadesInicioCtrl',
                    data: {
                        title: 'CLASIFICADORES',
                        contentClasses: 'full-height'
                    }
                })

                // ///////////////////////////////////////// TIPO ENTIDADES //////////////////////////////////////////////
                .state('app.svd_cla_tipoEntidad', {
                    url: '/clasificadores/tipoEntidades',
                    templateUrl: 'app/svd/clasificadores/tipoEntidad/tipoEntidad-lista.html',
                    resolve: {
                        deps: ['$ocLazyLoad', function($ocLazyLoad) {
                                return $ocLazyLoad.load('app/svd/clasificadores/tipoEntidad/tipoEntidadController.js');
                            }]
                    },
                    controller: 'tipoEntidadInicioCtrl',
                    data: {
                        title: 'CLASIFICADORES',
                        contentClasses: 'full-height'
                    }
                })

                // ///////////////////////////////////////// ARCHIVOS ASCII//////////////////////////////////////////////
                .state('app.svd_cla_archivos', {
                    url: '/clasificadores/archivos',
                    templateUrl: 'app/svd/clasificadores/archivos/archivos-lista.html',
                    resolve: {
                        deps: ['$ocLazyLoad', function($ocLazyLoad) {
                                return $ocLazyLoad.load('app/svd/clasificadores/archivos/archivosController.js');
                            }]
                    },
                    controller: 'archivosInicioCtrl',
                    data: {
                        title: 'CLASIFICADORES',
                        contentClasses: 'full-height'
                    }
                })

                // ///////////////////////////////////////// COTIZACIONES //////////////////////////////////////////////
                .state('app.svd_cla_cotizaciones', {
                    url: '/clasificadores/cotizaciones',
                    templateUrl: 'app/svd/clasificadores/cotizaciones/cotizaciones-lista.html',
                    resolve: {
                        deps: ['$ocLazyLoad', function($ocLazyLoad) {
                                return $ocLazyLoad.load('app/svd/clasificadores/cotizaciones/cotizacionesController.js');
                            }]
                    },
                    controller: 'cotizacionesInicioCtrl',
                    data: {
                        title: 'CLASIFICADORES',
                        contentClasses: 'full-height'
                    }
                })

                // ///////////////////////////////////////// MODALIDADES //////////////////////////////////////////////
                .state('app.svd_cla_modalidades', {
                    url: '/clasificadores/modalidades',
                    templateUrl: 'app/svd/clasificadores/modalidades/modalidades-lista.html',
                    resolve: {
                        deps: ['$ocLazyLoad', function($ocLazyLoad) {
                                return $ocLazyLoad.load('app/svd/clasificadores/modalidades/modalidadesController.js');
                            }]
                    },
                    controller: 'modalidadesInicioCtrl',
                    data: {
                        title: 'CLASIFICADORES',
                        contentClasses: 'full-height'
                    }
                })

                // ///////////////////////////////////////// RAMOS //////////////////////////////////////////////
                .state('app.svd_cla_ramos', {
                    url: '/clasificadores/ramos',
                    templateUrl: 'app/svd/clasificadores/ramos/ramos-lista.html',
                    resolve: {
                        deps: ['$ocLazyLoad', function($ocLazyLoad) {
                                return $ocLazyLoad.load('app/svd/clasificadores/ramos/ramosController.js');
                            }]
                    },
                    controller: 'ramosInicioCtrl',
                    data: {
                        title: 'CLASIFICADORES',
                        contentClasses: 'full-height'
                    }
                })
                // ///////////////////////////////////////// CONFIGURACION //////////////////////////////////////////////
                .state('app.svd_configuracion', {
                    url: '/configuracion/form',
                    templateUrl: 'app/svd/configuracion/configuracion-form.html',
                    resolve: {
                        deps: ['$ocLazyLoad', function($ocLazyLoad) {
                                return $ocLazyLoad.load('app/svd/configuracion/configuracionController.js');
                            }]
                    },
                    controller: 'configuracionInicioCtrl',
                    data: {
                        title: 'CONFIGURACION',
                        contentClasses: 'full-height'
                    }
                })
                ;
        }
    ])
    .config(['$ocLazyLoadProvider', function($ocLazyLoadProvider) {
            $ocLazyLoadProvider.config({
                debug: false,
                events: false
            });
        }])
    ;
