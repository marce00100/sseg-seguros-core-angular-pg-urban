using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Dapper;
using SVD.Models;
using System;

namespace SVD.Controllers
{

    /*********************************************************************************************************
        Clase con Funciones para el poblado en las tablas de SQL
        Estas funciones van a la par con las funciones de consolidacion en POstGRes y son llamadas si la opcion de poblado SQL en la clase Config del archivo Valores.cs esta habilitada
    */
    public partial class ConsolidacionController
    {

        // Funcion que pobla la tabla iftBalanceEstadoResultados en SQL
        public void poblarIftBalanceEstadoResultados()
        {
            
        }


    }
}