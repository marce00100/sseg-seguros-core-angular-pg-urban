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
    public class ConfiguracionesController : Controller
    {
        private static IDbConnection con = BdPG.instancia();

        private SVD.Models.Settings AppSettings;

        public ConfiguracionesController(IOptions<SVD.Models.Settings> settings)
        {
            AppSettings = settings.Value;
        }


        [HttpGet("activo")]
        [Authorize(Roles = "administrador")]
        public dynamic configuracionVigente()
        {
            Configuracion conf = con.Query<Configuracion>(@"SELECT * FROM configuraciones WHERE activo ").FirstOrDefault();
            con.Close();
            return new
            {
                data = conf
            };
        }

        public static Configuracion configuracionActiva()
        {
            Configuracion conf = con.Query<Configuracion>(@"SELECT * FROM configuraciones WHERE activo ").FirstOrDefault();
            con.Close();
            return conf;
        }

        [HttpPost("modifica")]
        [Authorize(Roles = "administrador")]
        public object modifica([FromBody] Configuracion elem)
        {

            try
            {
                // my code
                string token = HttpHelpers.GetTokenFromHeader(HttpContext);
                if (token == "")
                    return Unauthorized();

                Base helper = new Base(AppSettings, token, HttpContext.Connection.RemoteIpAddress.ToString());


                Configuracion old = con.Query<Configuracion>("select * from configuraciones where id_configuracion = @id", new { id = elem.id_configuracion }).FirstOrDefault();

                old.modificado_por = 999;
                old.modificado_en = DateTime.Now;
                old.activo = false;
                con.Execute(@"Update configuraciones set usa_factor_tc=@usa_factor_tc, moneda_factor_tc=@moneda_factor_tc, margen_partes_ef=@margen_partes_ef, 
                                margen_validacion_soat=@margen_validacion_soat, activo=@activo, modificado_por=@modificado_por, 
                                modificado_en=@modificado_en
                                WHERE  id_configuracion=@id_configuracion ", old);

                elem.activo = true;
                elem.id_configuracion = con.Query<int>(@"INSERT INTO configuraciones(
                                                    usa_factor_tc, moneda_factor_tc, margen_partes_ef, 
                                                        margen_validacion_soat, activo)
                                                VALUES (@usa_factor_tc, @moneda_factor_tc, @margen_partes_ef, 
                                                        @margen_validacion_soat, @activo)
                                                         RETURNING id_configuracion", elem).Single();
                con.Close();
                helper.AddLog(Log.TipoOperaciones.Modificacion, typeof(ConfiguracionesController), "modifica", elem);
                return new
                {
                    status = "success",
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