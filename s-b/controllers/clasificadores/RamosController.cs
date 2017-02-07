using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Dapper;
using SVD.Models;
using System;

namespace SVD.Controllers
{
    [Route("svd/api/[Controller]")]
    public class RamosController : Controller
    {
        private IDbConnection con = BdPG.instancia();

        [HttpGet()]
        public object todos()
        {
            IEnumerable<object> ramosList = con.Query<object>(@"SELECT m.""tDescripcion"" as modalidad_descripcion, r.*  FROM ""iftRamo"" r, ""iftModalidad"" m
                                                                    WHERE r.""cModalidad"" = m.""cModalidad"" 
                                                                   ORDER BY r.""cModalidad"", r.""cRamo""  ");
            con.Close();
            return new
            {
                status = "success",
                mensaje = "encontrados",
                codigo = "200",
                numero = ramosList.Count(),
                data = ramosList
            };
        }

        [HttpGet("{idm}/{idr}")]
        public object elem(string idm, string idr)
        {
            object ramoItem = con.Query<object>(@"SELECT * FROM ""iftRamo"" WHERE ""cModalidad""= @idm AND ""cRamo""  = @idr", new { idm = idm, idr = idr }).FirstOrDefault();
            con.Close();
            string status, mensaje, codigo;
            if (ramoItem == null)
            {
                status = "no content";
                mensaje = "no encontrado";
                codigo = "204";
            }
            else
            {
                status = "ok";
                mensaje = "encontrado";
                codigo = "200";
            }

            return new
            {
                status = status,
                mensaje = mensaje,
                codigo = codigo,
                data = ramoItem
            };
        }

        [HttpPost()]
        public object insertar([FromBody]Ramo ramo)
        {
            dynamic entCod = con.Query<dynamic>(@"SELECT  * FROM ""iftRamo"" WHERE ""cModalidad"" = @cm AND ""cRamo""= @cr ", new { cm = ramo.cModalidad, cr = ramo.cRamo }).FirstOrDefault();
            con.Close();string status, mensaje, codigo;
            object data;
            if (entCod == null)
            {
                ramo.creado_en = DateTime.Now;
                ramo.creado_por = 999;
                con.Execute(@"INSERT INTO ""iftRamo""(
                                                     ""cModalidad"", ""cRamo"", ""tDescripcion"", ""idBoletin"", 
                                                     ""tSigla"", creado_por, creado_en)
                                                VALUES ( @cModalidad, @cRamo, @tDescripcion, @idBoletin, 
                                                @tSigla, @creado_por, @creado_en)", ramo);
                con.Close();

                status = "created";
                mensaje = "creado";
                codigo = "201";
                data = ramo;
            }
            else
            {
                status = "not created";
                mensaje = "codigo duplicado";
                codigo = "202";
                data = entCod;
            }
            return new
            {
                status = status,
                mensaje = mensaje,
                codigo = codigo,
                data = data
            };
        }


        [HttpPut("{id}")]
        public object actualizar([FromBody]Ramo ramo, int id)
        {
            ramo.modificado_en = DateTime.Now;
            ramo.modificado_por = 999;
            con.Execute(@"UPDATE ""iftRamo"" SET 
                                ""tDescripcion"" = @tDescripcion, ""tSigla"" = @tSigla,
                                modificado_por = @modificado_por, modificado_en =  @modificado_en
                                WHERE ""cModalidad"" = @cModalidad AND ""cRamo"" = @cRamo", ramo);

            con.Close();
            return new
            {
                status = "success",
                mensaje = "actualizado",
                codigo = "200",
                data = ramo
            };
        }

    }
}