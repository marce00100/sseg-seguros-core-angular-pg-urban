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
    public class ConfiguracionesController : Controller
    {
        private static IDbConnection con = BdPG.instancia();


        [HttpGet("activo")]
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
        public object modifica([FromBody] Configuracion elem)
        {

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
            return new
            {
                status = "success",
                data = elem
            };

        }




    }
}