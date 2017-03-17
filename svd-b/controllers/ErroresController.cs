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
    public class ErroresController : Controller
    {
        private static IDbConnection con = BdPG.instancia();

        // [HttpGet("envio/{idSeg}")]
        public dynamic erroresDeSeguimientoEnvio(int idSeg)
        {
            List<Error> erroresList = new List<Error>(con.Query<Error>(@"SELECT *  FROM errores_envios  
                                                                    WHERE id_seguimiento_envio  = @idSeg   
                                                                   ORDER BY cod_error ", new { idSeg = idSeg }));
            con.Close();
            return new
            {
                status = "success",
                mensaje = "encontrados",
                codigo = "200",
                data = erroresList
            };
        }
        public static List<Error> getErroresDeSeguimiento(int idSEg)
        {
            ErroresController err = new ErroresController();
            return err.erroresDeSeguimientoEnvio(idSEg).data;
        }

        // [HttpGet("{id}")]
        public object errorId(int id)
        {
            object errorItem = con.Query<object>(@"SELECT * FROM errores_envios WHERE id_error_envio  = @id", new { id = id }).FirstOrDefault();
            con.Close();
            string status, mensaje, codigo;
            if (errorItem == null)
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
                data = errorItem
            };
        }

        // [HttpPost()]
        public object insertar([FromBody]Error error)
        {
            error.creado_en = DateTime.Now;
            error.creado_por = 999;
            error.id_error_envio = this.funcionInsertar(error);

            return new
            {
                status = "created",
                mensaje = "creado",
                codigo = "201",
                data = error
            };
        }


        // [HttpPut("valida/{id}")]
        public object validaError([FromBody]Error error, int id)
        {
            error.modificado_en = DateTime.Now;
            error.modificado_por = 999;
            con.Execute(@"UPDATE errores_envios SET 
                                valido = @valido, 
                                observaciones =  @observaciones, 
                                modificado_por = @modificado_por, modificado_en =  @modificado_en
                                WHERE id_error_envio = @id_error_envio", error);
            con.Close();
            return new
            {
                status = "success",
                mensaje = "actualizado",
                codigo = "200",
                data = error
            };
        }

        public static void insertarError(Error error)
        {
            error.creado_en = DateTime.Now;
            error.creado_por = 999;
            ErroresController obj = new ErroresController();
            obj.funcionInsertar(error);

        }

        public static void borrarErroresDelLaMismaApertura(string codEntidad)
        {
            con.Execute(@"DELETE from errores_envios where id_error_envio in 
                (select e.id_error_envio from errores_envios e, seguimiento_envios s, aperturas a, aperturas ac
                where e.id_seguimiento_envio = s.id_seguimiento_envio and s.id_apertura = a.id_apertura
                and  ac.activo and ac.fecha_corte = a.fecha_corte
                and s.cod_entidad =  @codigoEntidad )", new { codigoEntidad = codEntidad });
            con.Close();
        }

        private int funcionInsertar(Error error)
        {
            int id = con.Execute(@"INSERT INTO errores_envios(
                                                id_seguimiento_envio, cod_error, error, archivo, fila, columna,
                                                descripcion_puntual, valido, observaciones, 
                                                creado_por, creado_en)
                                                VALUES ( @id_seguimiento_envio, @cod_error, @error, @archivo, @fila, @columna, 
                                                @descripcion_puntual, @valido, @observaciones,
                                                 @creado_por, @creado_en)
                                                 RETURNING id_error_envio", error);
            con.Close();
            return id;
        }



    }
}