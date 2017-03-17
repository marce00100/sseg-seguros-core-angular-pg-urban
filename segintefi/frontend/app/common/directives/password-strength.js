'use strict';

/*
 * passwordStrength directive, based on: http://jsfiddle.net/jaredwilli/5RrX7/
 */

function passwordStrength(){
    return {        
        restrict: 'A',
        link: function(scope, element, attrs){
            scope.$watch(attrs.passwordStrength, function(value) {
                //console.log(value);
                if(angular.isDefined(value)){
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

                    if(element[0].name == 'oldPassword')
                        scope.strengthOldPassword = strength;
                    else if(element[0].name == 'newPassword')
                        scope.strengthNewPassword = strength;
                    else
                        scope.strengthNewPassword2 = strength;
                }
            });
        }
    };
}

angular.module('ApsApp').directive('passwordStrength', passwordStrength);