'use strict';

angular
  .module('ApsApp')
  .run(['$rootScope', '$state', '$stateParams', 'PermPermissionStore', '$localStorage',
        function ($rootScope, $state, $stateParams, PermPermissionStore, $localStorage) {
            $rootScope.$state = $state;
            $rootScope.$stateParams = $stateParams;
            $rootScope.$on('$stateChangeSuccess', function () {
                window.scrollTo(0, 0);
            });
            FastClick.attach(document.body);

          // loading splash
          $rootScope.spinner = {
            active: false,
            on: function () {
              this.active = true;
            },
            off: function () {
              this.active = false;
            }
          };
          // loading splash

          // settings permissions
          if(!($localStorage.usuario == undefined || $localStorage.usuario.perfiles == undefined)){
            $rootScope.globals = {usuario : $localStorage.usuario };
            $localStorage.usuario.perfiles.forEach(function(item) {
              PermPermissionStore.definePermission(item.nombre, function () {
                return true;
              });
            });
          }
          // end setting permissions
        },
  ])
    .config(['$stateProvider', '$urlRouterProvider',
        function($stateProvider, $urlRouterProvider) {

            // For unmatched routes
            //$urlRouterProvider.otherwise('/');
            $urlRouterProvider.otherwise(function($injector, $location){
            var $state = $injector.get("$state");
            $state.go('user.login');
            });

            // Application routes
            $stateProvider

      ////////////////////////////////////////////////////
      // rutas que no requieren un usuario autenticado  //
      /////////////       INICIO           ///////////////
      .state('user', {
          templateUrl: 'app/common/session.html',
      })
      .state('user.login', {
          url: '/login',
          templateUrl: 'app/common/login.html',
          resolve: {
            deps: ['$ocLazyLoad', function ($ocLazyLoad) {
              return $ocLazyLoad.load('app/common/controllers/loginController.js');
                    }]
          },
          controller: 'loginController',
          controllerAs: "vm",
          data: {
            appClasses: 'bg-white usersession',
            contentClasses: 'full-height'
          }
      })
      ////////////////////////////////////////////////////
      // rutas que no requieren un usuario autenticado  //
      /////////////          FIN           ///////////////
                .state('app', {
                    abstract: true,
                    templateUrl: 'app/common/layout.html',
                })


      ////////////////////////////////////////////////////
      // ruta para la actualización del password        //
      /////////////       INICIO           ///////////////
      .state('app.updatePassword', {
          url: '/updatePassword',
          templateUrl: 'app/common/password.html',
          resolve: {
            deps: ['$ocLazyLoad', function ($ocLazyLoad) {
              return $ocLazyLoad.load('app/common/controllers/passwordController.js');
                    }]
          },
          controller: 'passwordController',
          controllerAs: "vm",
          data: {
            appClasses: 'bg-white usersession',
            contentClasses: 'full-height'
          }
      })
      ////////////////////////////////////////////////////
      // ruta para la actualización del password        //
      /////////////       FIN           ///////////////

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
                        contentClasses: 'full-height',
                        permissions: {
                            only: ['administrador'],
                            redirectTo: 'user.login'
                        }
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
                        contentClasses: 'full-height',
                        permissions: {
                            only: ['administrador'],
                            redirectTo: 'user.login'
                        }
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
                        contentClasses: 'full-height',
                        permissions: {
                            only: ['administrador'],
                            redirectTo: 'user.login'
                        }
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
                        contentClasses: 'full-height',
                        permissions: {
                            only: ['administrador'],
                            redirectTo: 'user.login'
                        }
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
                        contentClasses: 'full-height',
                        permissions: {
                            only: ['administrador'],
                            redirectTo: 'user.login'
                        }
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
                        contentClasses: 'full-height',
                        permissions: {
                            only: ['administrador'],
                            redirectTo: 'user.login'
                        }
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
                        contentClasses: 'full-height',
                        permissions: {
                            only: ['administrador'],
                            redirectTo: 'user.login'
                        }
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
                        contentClasses: 'full-height',
                        permissions: {
                            only: ['administrador'],
                            redirectTo: 'user.login'
                        }
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
                        contentClasses: 'full-height',
                        permissions: {
                            only: ['administrador'],
                            redirectTo: 'user.login'
                        }
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


  ////////////////////////////////////////////////////
  // intercaptadores de las respuestas y peticiones //
  /////////////       INICIO           ///////////////
  .factory('authHttpResponseInterceptor', ['$q', '$location', 'Flash', function($q, $location, Flash){
      return {
          response: function(response){
              if (response.status === 401) {
                  console.log("Error -> Response 401");
              }
              return response || $q.when(response);
          },
          responseError: function(rejection) {
              if (rejection.status === 401) {
                  $location.path('/login');
              }
              else if (rejection.status === 500) {
                  $location.path('/login');
              }
              else if (rejection.status === 403) {
                  Flash.create("danger", "Usted no está autorizado para acceder a este recurso");
              }
              else if (rejection.status === -1) {
                  Flash.create("danger", "Response Error , transaction aborted.");
                  $location.path('/login');
              }
              else{
                  Flash.create("danger", "Error al llamar al servicio " + rejection.status);
              }
              return $q.reject(rejection);
          }
      }
  }])
  .factory('httpRequestInterceptor', function ($localStorage, $location) {
    return {
      request: function (config) {
          if($location.path() == '/login'){
                return config;          
          }

          if($localStorage.access_token == undefined)
            config.headers['Authorization'] = '';
          else
            config.headers['Authorization'] = $localStorage.access_token;

          return config;
      }
    };
  })
  .config(['$httpProvider', function($httpProvider) {
      //initialize get if not there
      if (!$httpProvider.defaults.headers.get) {
          $httpProvider.defaults.headers.get = {};    
      }

      $httpProvider.defaults.headers.get['Cache-Control'] = 'no-cache';
      $httpProvider.defaults.headers.get['Pragma'] = 'no-cache';

      $httpProvider.interceptors.push('authHttpResponseInterceptor');
      $httpProvider.interceptors.push('httpRequestInterceptor');
  }])
  ////////////////////////////////////////////////////
  // intercaptadores de las respuestas y peticiones //
  /////////////          FIN           ///////////////
  
    ;
