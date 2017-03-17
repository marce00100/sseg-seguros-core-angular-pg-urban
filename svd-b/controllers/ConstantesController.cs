using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Dapper;
using SVD.Models;
using System;

namespace SVD.Controllers
{
    public class ConstantesController : Controller
    {
        static IDbConnection con = BdPG.instancia();


        public static List<dynamic> obtieneConstantesDeDimension(string dimension)
        {
            List<dynamic> erroresList = new List<dynamic>(con.Query<dynamic>(@"SELECT *  FROM constantes WHERE dimension = @dimension", new { dimension = dimension }));
            con.Close();
            return erroresList;
        }






    }
}