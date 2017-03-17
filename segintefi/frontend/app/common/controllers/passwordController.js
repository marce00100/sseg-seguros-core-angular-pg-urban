'use strict';

function passwordController($localStorage, authService, Flash, $mdDialog) {
    var vm = this;

    vm.UpdatePassword = UpdatePassword;
    vm.Clear = Clear;

    (function initController(){
        vm.oldPassword = '';
        vm.newPassword = '';
        vm.newPassword2 = '';
    })();

    function myAlert(myMessage){
        $mdDialog.show(
              $mdDialog.alert({
                title: 'Error',
                textContent: myMessage,
                ok: 'Aceptar'
              })
            );
    }

    function checkStrength(value){
        var hasUpperCase = /[A-Z]/.test(value);
        var hasLowerCase = /[a-z]/.test(value);
        var hasNumbers = /\d/.test(value);
        var hasNonalphas = /\W/.test(value);
        var characterGroupCount = hasUpperCase + hasLowerCase + hasNumbers + hasNonalphas;

        var strength = 'weak';
        if ((value.length >= 8) && (characterGroupCount == 4))
            strength = 'strong';
        else if ((value.length >= 8) && (characterGroupCount >= 3))
            strength = 'medium';

        return strength;
    }

    function UpdatePassword() {
        
        // 1. hacer algunas validaciones
        if(vm.oldPassword == '' || vm.newPassword == '' || vm.newPassword2 == ''){
            myAlert('Ingrese valores para las contraseñas');
        }
        else if(vm.newPassword != vm.newPassword2){
            myAlert('Los valores de los campos de nueva contraseña son diferentes');
        }
        else if(checkStrength(vm.newPassword) == 'weak'){
            var msg = 'La nueva contraseña no reune los criterios mínimos de seguridad, ';
            msg += 'debe contener por lo menos 8 caracteres y por lo menos 3 tipos de caracteres diferentes '
            msg += '(letras minúsculas, letras mayúsculas, números o caracteres especiales)';
            myAlert(msg);
        }
        else{
            // 2. preparar el objeto
            if($localStorage.usuario === undefined || $localStorage.usuario.nombreUsuario === undefined){
                Flash.create('danger', '<strong>ERROR: </strong>el token ha expirado');
            }
            else{
                var data = {
                        Usuario: $localStorage.usuario.nombreUsuario,
                        OldPassword: vm.oldPassword,
                        NewPassword: vm.newPassword
                    };

                // 3. llamar al (los) servicio (s) que guarda la app
                authService.UpdatePassword(data).then(
                    function(response){
                        if(response.data.status == 'success'){
                            Flash.create('success', response.data.message);
                        }
                        else{
                            Flash.create('danger', '<strong>ERROR: </strong>' + response.data.message);
                        }
                    });
            }
        }
    }

    // Reset entidad details
    function Clear() {
        vm.oldPassword = '';
        vm.newPassword = '';
        vm.newPassword2 = '';
    }
}

angular
  .module('ApsApp')
  .controller('passwordController', ['$localStorage', 'authService', 'Flash', '$mdDialog', passwordController]);
