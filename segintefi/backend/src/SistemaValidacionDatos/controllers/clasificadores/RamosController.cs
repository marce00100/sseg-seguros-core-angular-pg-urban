using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Dapper;
using SVD.Models;
using System;
using Microsoft.Extensions.Options;
using SistemaValidacionDatos.controllers;
using SVD.Utils;
using BO.Aps.Auditoria;
using Microsoft.AspNetCore.Authorization;

namespace SVD.Controllers
{
    [Route("svd/api/[Controller]")]
    public class RamosController : Controller
    {
        private IDbConnection con = BdPG.instancia();

        private SVD.Models.Settings AppSettings;

        //public RamosController()
        //{

        //}
        public RamosController(IOptions<SVD.Models.Settings> settings)
        {
            AppSettings = settings.Value;
        }

        [HttpGet()]
        [Authorize(Roles = "administrador")]
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
        [Authorize(Roles = "administrador")]
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
        [Authorize(Roles = "administrador")]
        public object insertar([FromBody]Ramo ramo)
        {
            try
            {
                // my code
                string token = HttpHelpers.GetTokenFromHeader(HttpContext);
                if (token == "")
                    return Unauthorized();

                Base helper = new Base(AppSettings, token, HttpContext.Connection.RemoteIpAddress.ToString());

                dynamic entCod = con.Query<dynamic>(@"SELECT  * FROM ""iftRamo"" WHERE ""cModalidad"" = @cm AND ""cRamo""= @cr ", new { cm = ramo.cModalidad, cr = ramo.cRamo }).FirstOrDefault();
                con.Close(); string status, mensaje, codigo;
                object data;
                if (entCod == null)
                {
                    ramo.creado_en = DateTime.Now;
                    ramo.creado_por = helper.UsuarioId;

                    con.Execute(@"INSERT INTO ""iftRamo""(
                                                     ""cModalidad"", ""cRamo"", ""tDescripcion"", ""idBoletin"", 
                                                     ""tSigla"", creado_por, creado_en)
                                                VALUES ( @cModalidad, @cRamo, @tDescripcion, @idBoletin, 
                                                @tSigla, @creado_por, @creado_en)", ramo);
                    con.Close();
                    helper.AddLog(Log.TipoOperaciones.Alta, typeof(RamosController), "insertar", ramo);

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
            catch (Exception ex)
            {
                return new
                {
                    status = "error",
                    mensaje = ex.Message
                };
            }
        }


        [HttpPut("{id}")]
        [Authorize(Roles = "administrador")]
        public object actualizar([FromBody]Ramo ramo, int id)
        {
            try
            {
                // my code
                string token = HttpHelpers.GetTokenFromHeader(HttpContext);
                if (token == "")
                    return Unauthorized();

                Base helper = new Base(AppSettings, token, HttpContext.Connection.RemoteIpAddress.ToString());

                ramo.modificado_en = DateTime.Now;
                ramo.modificado_por = helper.UsuarioId;
                con.Execute(@"UPDATE ""iftRamo"" SET 
                                ""tDescripcion"" = @tDescripcion, ""tSigla"" = @tSigla,
                                modificado_por = @modificado_por, modificado_en =  @modificado_en
                                WHERE ""cModalidad"" = @cModalidad AND ""cRamo"" = @cRamo", ramo);

                con.Close();
                helper.AddLog(Log.TipoOperaciones.Modificacion, typeof(RamosController), "actualizar", ramo);
                return new
                {
                    status = "success",
                    mensaje = "actualizado",
                    codigo = "200",
                    data = ramo
                };
            }
            catch (Exception ex)
            {
                return new
                {
                    status = "error",
                    mensaje = ex.Message
                };
            }
        }

    }
}