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
            ConfiguracionesController conf = new ConfiguracionesController();
            return conf.configuracionVigente().data;
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
            return new
            {
                status = "success",
                data = elem
            };

        }


        // [HttpGet("{id}")]
        // public object errorId(int id)
        // {
        //     object errorItem = con.Query<object>(@"SELECT * FROM errores_envios WHERE id_error_envio  = @id", new { id = id }).FirstOrDefault();
        //     con.Close();
        //     string status, mensaje, codigo;
        //     if (errorItem == null)
        //     {
        //         status = "no content";
        //         mensaje = "no encontrado";
        //         codigo = "204";
        //     }
        //     else
        //     {
        //         status = "ok";
        //         mensaje = "encontrado";
        //         codigo = "200";
        //     }

        //     return new
        //     {
        //         status = status,
        //         mensaje = mensaje,
        //         codigo = codigo,
        //         data = errorItem
        //     };
        // }

        // [HttpPost()]
        // public object insertar([FromBody]Error error)
        // {
        //     error.creado_en = DateTime.Now;
        //     error.creado_por = 999;
        //     error.id_error_envio = this.funcionInsertar(error);

        //     return new
        //     {
        //         status = "created",
        //         mensaje = "creado",
        //         codigo = "201",
        //         data = error
        //     };
        // }


        // [HttpPut("valida/{id}")]
        // public object validaError([FromBody]Error error, int id)
        // {
        //     error.modificado_en = DateTime.Now;
        //     error.modificado_por = 999;
        //     con.Execute(@"UPDATE errores_envios SET 
        //                         valido = @valido, 
        //                         observaciones =  @observaciones, 
        //                         modificado_por = @modificado_por, modificado_en =  @modificado_en
        //                         WHERE id_error_envio = @id_error_envio", error);
        //     con.Close();
        //     return new
        //     {
        //         status = "success",
        //         mensaje = "actualizado",
        //         codigo = "200",
        //         data = error
        //     };
        // }

        // public static void insertarError(Error error)
        // {
        //     error.creado_en = DateTime.Now;
        //     error.creado_por = 999;
        //     ErroresController obj = new ErroresController();
        //     obj.funcionInsertar(error);

        // }

        // // public static string mensajesError(string codError)
        // // {

        // // }

        // private int funcionInsertar(Error error)
        // {
        //     int id = con.Execute(@"INSERT INTO errores_envios(
        //                                         id_seguimiento_envio, cod_error, archivo, fila, columna,
        //                                         descripcion_puntual, valido, observaciones, 
        //                                         creado_por, creado_en)
        //                                         VALUES ( @id_seguimiento_envio, @cod_error, @archivo, @fila, @columna, 
        //                                         @descripcion_puntual, @valido, @observaciones, 
        //                                          @creado_por, @creado_en)
        //                                          RETURNING id_error_envio", error);
        //     con.Close();
        //     return id;
        // }



    }
}