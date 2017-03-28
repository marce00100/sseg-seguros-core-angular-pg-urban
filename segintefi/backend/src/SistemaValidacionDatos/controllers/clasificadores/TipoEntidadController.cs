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
    public class TipoEntidadController : Controller
    {
        private IDbConnection con = BdPG.instancia();

        private SVD.Models.Settings AppSettings;

        //public TipoEntidadController()
        //{

        //}
        public TipoEntidadController(IOptions<SVD.Models.Settings> settings)
        {
            AppSettings = settings.Value;
        }

        [HttpGet()]
        [Authorize(Roles = "administrador")]
        public object todos()
        {
            IEnumerable<object> lista = con.Query<object>(@"SELECT * FROM ""rstTipoEntidad""
                                                                   ORDER BY ""cTipoEntidad"" ");
            con.Close();
            return new
            {
                status = "success",
                mensaje = "encontrados",
                codigo = "200",
                numero = lista.Count(),
                data = lista
            };
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "administrador")]
        public object entidadId(int id)
        {
            object elemento = con.Query<object>(@"SELECT * FROM ""rstTipoEntidad"" WHERE ""cTipoEntidad""  = @id", new { id = id }).FirstOrDefault();
            con.Close();
            string status, mensaje, codigo;
            if (elemento == null)
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
                data = elemento
            };
        }

        [HttpPost()]
        [Authorize(Roles = "administrador")]
        public object insertar([FromBody]TipoEntidad elem)
        {
            try
            {
                // my code
                string token = HttpHelpers.GetTokenFromHeader(HttpContext);
                if (token == "")
                    return Unauthorized();

                Base helper = new Base(AppSettings, token, HttpContext.Connection.RemoteIpAddress.ToString());

                dynamic entCod = con.Query<dynamic>(@"SELECT  * FROM ""rstTipoEntidad"" WHERE ""cTipoEntidad"" = @codigo", new { codigo = elem.cTipoEntidad }).FirstOrDefault();
                con.Close();
                string status, mensaje, codigo;
                object data;
                if (entCod == null)
                {
                    elem.creado_por = helper.UsuarioId;
                    elem.creado_en = DateTime.Now;
                    con.Execute(@"INSERT INTO ""rstTipoEntidad""(
                                    ""cTipoEntidad"",""tDescripcion"",""tDescripcionCorta"",""tCodigo"",
                                     creado_por, creado_en)
                                VALUES ( @cTipoEntidad, @tDescripcion, @tDescripcionCorta, @tCodigo,  
                                @creado_por, @creado_en)", elem);
                    con.Close();
                    helper.AddLog(Log.TipoOperaciones.Alta, typeof(TipoEntidadController), "insertar", elem);
                    // new
                    // {
                    //     cTipoEntidad = (int) elem.cTipoEntidad,
                    //     tDescripcion = elem.tDescripcion,
                    //     tDesripcionCorta = elem.tDesripcionCorta,
                    //     tCodigo = elem.tCodigo,
                    //     creado_por = elem.creado_por,
                    //     creado_en = elem.creado_en
                    // });
                    status = "created";
                    mensaje = "creado";
                    codigo = "201";
                    data = elem;
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
        public object actualizar([FromBody]dynamic elem, int id)
        {
            try
            {
                // my code
                string token = HttpHelpers.GetTokenFromHeader(HttpContext);
                if (token == "")
                    return Unauthorized();

                Base helper = new Base(AppSettings, token, HttpContext.Connection.RemoteIpAddress.ToString());

                object objeto = new
                {
                    cTipoEntidad = (int)elem.cTipoEntidad,
                    tDescripcion = elem.tDescripcion.ToString(),
                    tDescripcionCorta = elem.tDescripcionCorta.ToString(),
                    tCodigo = elem.tCodigo.ToString()
                };

                con.Execute(@"UPDATE ""rstTipoEntidad"" SET 
                               ""tDescripcion"" = @tDescripcion,
                                ""tDescripcionCorta"" =  @tDescripcionCorta, ""tCodigo"" = @tCodigo
                                WHERE  ""cTipoEntidad"" = @cTipoEntidad ", objeto
                                    );
                con.Close();
                helper.AddLog(Log.TipoOperaciones.Modificacion, typeof(TipoEntidadController), "actualizar", objeto);
                return new
                {
                    status = "success",
                    mensaje = "actualizado",
                    codigo = "200",
                    data = elem
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