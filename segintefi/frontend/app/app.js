'use strict';

/**
 * @ngdoc overview
 * @name ApsApp
 * @description
 * # ApsApp
 *
 * Main module of the application.
 */
var app = angular
    .module('ApsApp', [
        'ui.router',
        'ngAnimate',
        'ui.bootstrap',
        'oc.lazyLoad',
        'ngStorage',
        'ngSanitize',
        'ui.utils',
        'ngRoute',
        'ngCookies',
        
        'permission',
        'permission.ui',  
        'treasure-overlay-spinner',  
        'ngMaterial',                
        'ngFlash'
    ])
    .constant('COLORS', {
        'default': '#e2e2e2',
        primary: '#09c',
        success: '#2ECC71',
        warning: '#ffc65d',
        danger: '#d96557',
        info: '#4cc3d9',
        white: 'white',
        dark: '#4C5064',
        border: '#e4e4e4',
        bodyBg: '#e0e8f2',
        textColor: '#6B6B6B',
    })
    .constant("Configs", {      
        Enviroment: "prod", 
        App:{
            "Nombre": "Sistema de Validación de Datos SVD",
            "Sigla": "SEGINTEFI",
            "DefaultPath": "/",
            "Guid": "b0357c97-a45b-4fe6-b876-a37fd70e0b05"
        },                /* Servicios de autenticacion */
        Auth:{
            /**/
            "Token": 'http://192.168.59.226/seguridad/api/v1/auth/token',
            "TokenRefresh": 'http://192.168.59.226/seguridad/api/v1/auth/refreshToken',
            "Usuario": 'http://192.168.59.226/seguridad/api/v1/usuarios'
            /*
            "Token": 'http://localhost:60754/api/v1/auth/token',
            "TokenRefresh": 'http://localhost:60754/api/v1/auth/refreshToken',
            "Usuario": 'http://localhost:60754/api/v1/usuarios'
            /**/
        },



        AppWebApi: 'http://localhost:5000/svd/api/',
        AppWebJasper: 'http://192.168.58.87:8080',
        usuarioJasper: 'app_boletin2',
        passwordJasper: 'GXS4FUtfP6R8,?x'
    });
