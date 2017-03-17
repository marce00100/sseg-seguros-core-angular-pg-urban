'use strict';
function loginController($location, $localStorage, authService, PermPermissionStore, Flash, Configs) {
        var vm = this;
        vm.login = Configs.Enviroment == "dev"? loginDev: login;
        vm.username = "jalvarez";
        vm.password = "Tnp2016.";

        (function initController() {
            // reset login status
            authService.ClearCredentials();
            PermPermissionStore.clearStore();
        })();

        function login() {         
            vm.dataLoading = true;
            authService.Login(vm.username, vm.password)
                .then( function(resLogin) {
                    if (resLogin.data.status == 'success') {
                        var token = resLogin.data.data; // Almacena el token
                        $localStorage.access_token = token.token_type + ' ' + token.access_token;
                        $localStorage.refresh_token = token.refresh_token;
                        $localStorage.expires_on = moment().unix() + token.expires_in;
                        $localStorage.check_refresh_token = $localStorage.expires_on - token.check_refresh_token;
                        authService.GetUsuarioDetalle(vm.username).then (
                            function (response){
                                if (response.data.status == 'success' && response.data.data.perfiles.length > 0) {
                                    authService.SetCredentials(vm.username, vm.password, response.data.data);

                                    response.data.data.perfiles.forEach(function(item) {
                                        PermPermissionStore
                                            .definePermission(item.nombre, function () {
                                              return true;
                                      });
                                    });

                                    Flash.create('success', 'Se autenticó correctamente');
                                    $location.path(Configs.App.DefaultPath);
                                    console.log('yendo a: ' + Configs.App.DefaultPath);
                                }
                                else{
                                    Flash.create('danger', '<strong>ERROR: </strong>NO TIENE PERMISOS PARA ACCEDER A LA APLICACIÓN');
                                    vm.dataLoading = false;
                                }
                            }, function(responseError){
                                Flash.create('danger', '<strong>ERROR: </strong>' + responseError.data.message);
                                vm.dataLoading = false;
                            }
                        );
                    } else {                        
                        Flash.create('danger', '<strong>ERROR: </strong>' + resLogin.data.message);
                        vm.dataLoading = false;
                    }
                }, function(resLoginError){                   
                    Flash.create('danger', '<strong>ERROR: </strong>' + resLoginError.data.message);
                    vm.dataLoading = false;
                });
        };

        function loginDev() {
            vm.dataLoading = true;
            //vm.username, vm.password
            //var token = resLogin.data.data; // Almacena el token
            $localStorage.access_token = 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJ1c3VhcmlvIiwianRpIjoiM2QxZTE3YzQtNjI3My00MjM3LTgwZjItNGVlN2RjNzA5MmU3IiwiaWF0IjoxNDc1Njc0ODk5LCJ1c3VhcmlvIjoidXN1YXJpbyIsIm5iZiI6MTQ3NTY3NDg5OSwiZXhwIjoxNDc1Njc1MDE5LCJpc3MiOiJodHRwOi8vbG9jYWxob3N0OjYyNjgvIiwiYXVkIjoiaHR0cDovL2xvY2FsaG9zdDo4MDgwLyJ9.2pMTKVt8upqm9d6_tJa9-KrmT61KO3yQ_IReStyDC34';
            $localStorage.refresh_token = 'ABCDEFGFGHIJK';
            $localStorage.expires_on = moment().unix() + 3600;
            $localStorage.check_refresh_token = $localStorage.expires_on - 300;

            var data = {
                        success: 'success',
                        message: 'Se autenticó',
                        nombreUsuario: 'jperez',
                        nombreCompleto: 'Juan Perez',
                        entidad: {id: 1, nombre: 'APS'},
                        cargo: {id: 1, nombre: 'Analista'},
                        perfiles: [
                            {id:1, nombre:'verAplicaciones'}, 
                            {id:2, nombre:'verDemo'},
                            {id:3, nombre:'verDemoRegistrar'}
                            ],
                        rol: {id:1, nombre:'admin'},
                    };
            authService.SetCredentials(vm.username, vm.password, data);
            data.perfiles.forEach(function(item) {
                                    PermPermissionStore
                                        .definePermission(item.nombre, function () {
                                          return true;
                                  });
                                });
            $location.path(Configs.App.DefaultPath);
        };
}
angular
  .module('ApsApp')
  .controller('loginController', [ '$location', '$localStorage', 'authService', 'PermPermissionStore', 'Flash', 'Configs', loginController]);