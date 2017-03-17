/**
 * angular-permission
 * Fully featured role and permission based access control for your angular applications
 * @version v4.0.3 - 2016-08-18
 * @link https://github.com/Narzerus/angular-permission
 * @author Rafael Vidaurre <narzerus@gmail.com> (http://www.rafaelvidaurre.com), Blazej Krysiak <blazej.krysiak@gmail.com>
 * @license MIT License, http://www.opensource.org/licenses/MIT
 */
(function () {
  'use strict';

  var permission = require('./dist/angular-permission.js'),
    ngPermission = require('./dist/angular-permission-ng.js'),
    uiPermission = require('./dist/angular-permission-ui.js');

  module.exports = {
    permission: permission,
    ngPermission: ngPermission,
    uiPermission: uiPermission
  };
})();
