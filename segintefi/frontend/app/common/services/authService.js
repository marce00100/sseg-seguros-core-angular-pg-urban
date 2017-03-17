//https://medium.com/@kevinle/exchange-refresh-token-for-json-web-token-jwt-in-angularjs-using-promise-453da9127cd7#.qz28hcmgo
(function() {
  'use strict';

  angular
        .module('ApsApp')
        .factory('authService', authService);

  authService.$inject = ['$http', '$q', '$localStorage', '$rootScope', 'Configs'];

  function authService($http, $q, $localStorage, $rootScope, Configs) {
      var cacheToken = {};
      var service = {};

      service.getAuthorizationHeader = getAuthorizationHeader;
      service.Login = Login;
      service.GetUsuarioDetalle = GetUsuarioDetalle;
      service.SetCredentials = SetCredentials;
      service.ClearCredentials = ClearCredentials;
      service.UpdatePassword = UpdatePassword;

      return service;

      function checkLocalStorage() {
          if($localStorage.access_token == undefined || $localStorage.refresh_token == undefined ||
              $localStorage.expires_on == undefined || $localStorage.check_refresh_token == undefined ||
              $localStorage.usuario == undefined){
              return false;
          }
          else{
              return true;
          }
      }
      
      function getAuthorizationHeader() {
          if(!checkLocalStorage())
            return $q.when({'Authorization': ''});


          cacheToken.access_token = $localStorage.access_token;
          cacheToken.refresh_token = $localStorage.refresh_token;
          cacheToken.expires_on = $localStorage.expires_on;
          cacheToken.check_refresh_token = $localStorage.check_refresh_token;

          if (cacheToken.check_refresh_token > moment().unix()) {
            //console.log('El token NO ha expirado');
            return $q.when({'Authorization': cacheToken.access_token});        
          } else {
            //console.log('El token YA ha expirado');
            //console.log('---'+ cacheToken.expires_on + '>' + moment().unix())
            if (cacheToken.expires_on > moment().unix()) {
              //console.log('El token de refresqueo esta siendo utilizado');
              var data_usuario = $localStorage.usuario;              
              var params = {usuario: data_usuario.nombreUsuario, app: Configs.App.Guid, refreshToken: $localStorage.refresh_token };
              return $http.post(
                  Configs.Auth.TokenRefresh,
                  params, {
                    headers:{
                      'Content-Type': 'application/json'
                      }
                    }
                ).then(
                  function(response){
                    var token = response.data.data;
                    cacheToken.access_token = token.token_type + ' ' + token.access_token;
                    cacheToken.refresh_token = token.refresh_token;
                    cacheToken.expires_on = moment().unix() + token.expires_in;
                    cacheToken.check_refresh_token = cacheToken.expires_on - token.check_refresh_token;

                    $localStorage.access_token = cacheToken.access_token;
                    $localStorage.refresh_token = cacheToken.refresh_token;
                    $localStorage.expires_on = cacheToken.expires_on;
                    $localStorage.check_refresh_token = cacheToken.check_refresh_token;

                    //console.log('El token ha sido refrescado .... nuevo token: ' + $localStorage.access_token);
                    return {'Authorization': cacheToken.access_token};    
                  },
                  function(err){
                    return {'Authorization': cacheToken.access_token}; 
                  }
              );
            }
            else {
              //console.log('El token ya ha expirado');
              return $q.when({'Authorization': cacheToken.access_token});        
            }
          }
      } // fin funcion

      function Login(username, password) {
            var params = {usuario: username, password: password, app: Configs.App.Guid};
            return $http.post(
                Configs.Auth.Token,
                params, {
                  headers: {'Content-Type': 'application/json'}
                }
            )
            .then(function(response) { // success
                return response;
            },
            function(error) { // optional
                // failed
                if(error.data == null)
                  error.data = "No se puede acceder al servicio de autenticación";

                return {
                    data:{
                        success: false,
                        message: error.data
                    }
                };
            });
      }

      function GetUsuarioDetalle(username) {
          return $http.get(
              Configs.Auth.Usuario + '/' + username + '/app/' + Configs.App.Guid,
              {
                  headers:{ 'Authorization': $localStorage.access_token }
              }
          )
          .then(function(response) { // success
              return response;
          },
          function(error) { // optional
              // failed
              if(error.data == null)
                error.data = "No se puede acceder al servicio de autenticación";

              return {
                      data:{
                          success: false,
                          message: error.data,
                          permissions: [],
                          rol: null,
                      }
                    };
          });
      }

      function SetCredentials(username, password, data) {
            var _perfilesLista = [];
            data.perfiles.forEach(function(item) {
                var _per = {};
                _per.id = item.id;
                _per.nombre = item.nombre;
                _perfilesLista.push(_per);
            });
            $rootScope.globals = {
                usuario: {
                    nombreUsuario: data.nombreUsuario,
                    nombreCompleto: data.nombreCompleto,
                    avatar: data.avatar,
                    entidad: {id: data.entidad.id, nombre: data.entidad.nombre},
                    cargo: {id: data.cargo.id, nombre: data.cargo.nombre},
                    perfiles: _perfilesLista,
                    rol: {id: data.rol.id, nombre: data.rol.nombre}
                }
            };

            $localStorage.usuario = data;
      }

      function ClearCredentials() {
          $rootScope.globals = {};
          delete $localStorage.access_token;
          delete $localStorage.refresh_token;
          delete $localStorage.expires_on;
          delete $localStorage.check_refresh_token;
          delete $localStorage.usuario;

          $http.defaults.headers.common.Authorization = 'Basic';
      }

      function UpdatePassword(data) {
          return getAuthorizationHeader().then(
              function(authHeader) {
                //console.log('authHeader');
                //console.log(authHeader);
                //console.log('authHeader');
                  return $http.put(
                        // url para acceso a la actualización del password
                        Configs.Auth.Usuario + '/'+ data.Usuario + '/updatePassword',
                        
                        // params
                        {
                          usuario: data.Usuario,
                          oldPassword: data.OldPassword,
                          newPassword: data.NewPassword,
                          app: Configs.App.Guid
                        },
                        
                        // headers
                        { headers: authHeader }

                  ).then(
                      function(response) { // success
                          //console.log('response');
                          //console.log(response);
                          return response;
                      }, 
                      function(error) { // optional
                          // failed
                          //console.log('error');
                          if(error.data == null)
                            error.data = "No se puede acceder al servicio de actualización del password";

                          return {
                              data:{
                                  success: false,
                                  message: error.data
                              }
                          };
                      }); 
            });
      }
  }

})()