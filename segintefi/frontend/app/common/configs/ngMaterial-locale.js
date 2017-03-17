'use strict';

angular
  .module('ApsApp')
  .config(function($mdDateLocaleProvider) {

    // Example of a French localization.
    $mdDateLocaleProvider.months = ['enero', 'febrero', 'marzo','abril', 'mayo', 'junio','julio', 'agosto', 'septiembre','octubre', 'noviembre', 'diciembre'];
    $mdDateLocaleProvider.shortMonths = ['ene', 'feb', 'mar','abr', 'may', 'jun','jul', 'ago', 'sep','oct', 'nov', 'dic'];
    $mdDateLocaleProvider.days = ['domingo', 'lunes', 'martes', 'miércoles', 'jueves', 'viernes', 'sábado'];
    $mdDateLocaleProvider.shortDays = ['Do', 'Lu', 'Ma', 'Mi', 'Ju', 'Vi', 'Sa'];

    // Can change week display to start on Monday.
    $mdDateLocaleProvider.firstDayOfWeek = 1;

    // Example uses moment.js to parse and format dates.
    $mdDateLocaleProvider.parseDate = function(dateString) {
      var m = moment(dateString, 'L', true);
      return m.isValid() ? m.toDate() : new Date(NaN);
    };

    $mdDateLocaleProvider.formatDate = function(date) {
      var m = moment(date);
      return m.isValid() ? m.format('L') : '';
    };

    $mdDateLocaleProvider.monthHeaderFormatter = function(date) {
      //return myShortMonths[date.getMonth()] + ' ' + date.getFullYear();
      return $mdDateLocaleProvider.shortMonths[date.getMonth()] + ' ' + date.getFullYear();
    };

    // In addition to date display, date components also need localized messages
    // for aria-labels for screen-reader users.

    $mdDateLocaleProvider.weekNumberFormatter = function(weekNumber) {
      return 'Semana ' + weekNumber;
    };

    $mdDateLocaleProvider.msgCalendar = 'Calendrier';
    $mdDateLocaleProvider.msgOpenCalendar = 'Ouvrir le calendrier';
    })