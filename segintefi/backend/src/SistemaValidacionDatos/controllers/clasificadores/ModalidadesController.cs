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
    public class ModalidadesController : Controller
    {
        private IDbConnection con = BdPG.instancia();

        private SVD.Models.Settings AppSettings;

        public ModalidadesController()
        {

        }
        public ModalidadesController(IOptions<SVD.Models.Settings> settings)
        {
            AppSettings = settings.Value;
        }


        [HttpGet()]
        [Authorize(Roles = "administrador")]
        public object todos()
        {
            string token = HttpHelpers.GetTokenFromHeader(HttpContext);
            if (token == "")
                return Unauthorized();

            IEnumerable<object> lista = con.Query<object>(@"SELECT *  FROM ""iftModalidad""
                                                                   ORDER BY ""cModalidad"" ");
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
        public object objetoId(string id)
        {
            object item = con.Query<object>(@"SELECT * FROM ""iftModalidad"" WHERE ""cModalidad""  = @id", new { id = id }).FirstOrDefault();
            con.Close();
            string status, mensaje, codigo;
            if (item == null)
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
                data = item
            };
        }

        [HttpPost()]
        [Authorize(Roles = "administrador")]
        public object insertar([FromBody]Modalidad objeto)
        {
            try
            {
                // my code
                string token = HttpHelpers.GetTokenFromHeader(HttpContext);
                if (token == "")
                    return Unauthorized();

                Base helper = new Base(AppSettings, token, HttpContext.Connection.RemoteIpAddress.ToString());

                dynamic enCod = con.Query<dynamic>(@"SELECT  * FROM ""iftModalidad"" WHERE ""cModalidad"" = @codigo", new { codigo = (string)objeto.cModalidad }).FirstOrDefault();
                con.Close();
                string status, mensaje, codigo;
                object data;
                if (enCod == null)
                {
                    objeto.creado_en = DateTime.Now;
                    objeto.creado_por = helper.UsuarioId;
                    con.Execute(@"INSERT INTO ""iftModalidad""(
                                        ""cModalidad"", ""tDescripcion"", 
                                        creado_por, creado_en)
                                        VALUES ( @cModalidad, @tDescripcion,  
                                            @creado_por, @creado_en)", objeto);
                    //  new
                    //  {
                    //      cModalidad = (string)objeto.cModalidad,
                    //      tDescripcion = (string)objeto.tDescripcion,
                    //      creado_por = (int)objeto.creado_por,
                    //      creado_en = (DateTime)objeto.creado_en
                    //  });
                    con.Close();
                    helper.AddLog(Log.TipoOperaciones.Alta, typeof(ModalidadesController), "insertar", objeto);


                    status = "created";
                    mensaje = "creado";
                    codigo = "201";
                    data = objeto;
                }
                else
                {
                    status = "not created";
                    mensaje = "codigo duplicado";
                    codigo = "202";
                    data = enCod;
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
        public object actualizar([FromBody]Modalidad elem, int id)
        {
            try
            {
                // my code
                string token = HttpHelpers.GetTokenFromHeader(HttpContext);
                if (token == "")
                    return Unauthorized();

                Base helper = new Base(AppSettings, token, HttpContext.Connection.RemoteIpAddress.ToString());

                elem.modificado_en = DateTime.Now;
                elem.modificado_por = helper.UsuarioId;
                con.Execute(@"UPDATE ""iftModalidad"" SET 
                                ""tDescripcion"" = @tDescripcion,
                                modificado_por = @modificado_por, modificado_en =  @modificado_en
                                WHERE ""cModalidad"" = @cModalidad", elem);

                con.Close();

                helper.AddLog(Log.TipoOperaciones.Modificacion, typeof(ModalidadesController), "actualizar", elem);
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