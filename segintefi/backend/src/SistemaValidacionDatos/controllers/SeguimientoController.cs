using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Dapper;
using SVD.Models;
using System;

using SistemaValidacionDatos.controllers;
using SVD.Utils;
using Microsoft.Extensions.Options;
using BO.Aps.Auditoria;
using Microsoft.AspNetCore.Authorization;

namespace SVD.Controllers
{
    [Route("svd/api/[Controller]")]
    public class SgmntController : Controller
    {
        private static IDbConnection con = BdPG.instancia();

        private SVD.Models.Settings AppSettings;

        public SgmntController(IOptions<SVD.Models.Settings> settings)
        {
            AppSettings = settings.Value;
        }


        [HttpGet("envios/{aperActiva_fCorte}")]
        [Authorize(Roles = "administrador,operador")]
        // puede ser   envios/2016-03-31    o    envios/apertura_activa  
        public dynamic get_SeguimientoEntidades(string aperActiva_fCorte)
        {

            List<dynamic> listaActivos = SgmntController.getSeguimientoEntidades(aperActiva_fCorte);

            return new
            {
                status = "success",
                mensaje = "encontrados",
                codigo = "200",
                data = listaActivos
            };
        }

        public static List<dynamic> getSeguimientoEntidades(string aperActiva_fCorte)
        {
            string queryFechaCorte = aperActiva_fCorte == "apertura_activa" ? "(SELECT fecha_corte FROM aperturas WHERE activo)" : "'" + aperActiva_fCorte + "'";
            //obtiene todos los envios de todas las entidades para una fecha de corte (o apertura_actual)

            List<dynamic> lista = new List<dynamic>(con.Query<dynamic>(@"SELECT a.id_apertura, a.fecha_corte, e.""tNombre"" as entidad_nombre, s.* , c.nombre as desc_estado
                                                FROM seguimiento_envios  s, ""rstEmpresas"" e, aperturas a, constantes c
                                                WHERE s.cod_entidad = e.""cEmpresa"" AND a.id_apertura = s.id_apertura 
                                                AND s.estado = c.codigo AND c.dimension = 'estado_seguimiento'
                                                AND  a.fecha_corte = " + queryFechaCorte +
                                                "order by s.fecha_envio"));
            con.Close();
            List<dynamic> listaActivos = new List<dynamic>(con.Query<dynamic>(@"SELECT a.id_apertura, a.fecha_corte, e.""tNombre"" as entidad_nombre, s.* , c.nombre as desc_estado
                                                FROM seguimiento_envios  s, ""rstEmpresas"" e, aperturas a, constantes c
                                                WHERE s.cod_entidad = e.""cEmpresa"" AND a.id_apertura = s.id_apertura 
                                                 AND s.estado = c.codigo AND c.dimension = 'estado_seguimiento'
                                                AND s.activo
                                                AND  a.fecha_corte = " + queryFechaCorte +
                                                "order by s.fecha_envio"));
            con.Close();
            foreach (dynamic item in listaActivos)
            {
                item.envios = lista.Where(o => o.cod_entidad == item.cod_entidad).OrderBy(x => x.fecha_envio);
            }
            return listaActivos;
        }

        //Obtiene todo el contexto de entidad y seguimiento activo de una entidad  de la apertura activa
        [HttpGet("activo/{cod_entidad}")]
        [Authorize(Roles = "administrador,carga_informacion,operador")] //Obtiene todo el contexto de entidad y seguimiento activo de una entidad  de la apertura activa
        public dynamic getSeguimientoAperturaActivaEntidad(string cod_entidad)
        {
            string token = HttpHelpers.GetTokenFromHeader(HttpContext);
            if (token == "")
                return Unauthorized();

            Base helper = new Base(AppSettings, token, HttpContext.Connection.RemoteIpAddress.ToString());

            if (helper.Role == "carga_informacion")
                cod_entidad = helper.CiaId;

            dynamic seguimientoActivoEntidad = SgmntController.seguimientoAperturaActivaEntidad(cod_entidad);
            return new
            {
                status = "success",
                mensaje = "encontrado",
                codigo = "200",
                data = seguimientoActivoEntidad
            };
        }

        public static dynamic seguimientoAperturaActivaEntidad(string cod_entidad)
        {
            dynamic seguimientoActivoEntidad = con.Query<dynamic>(@"SELECT a.fecha_corte, s.id_seguimiento_envio, s.fecha_envio, s.estado, s.activo, s.valido,
                                                                        e.""cEmpresa"" cod_entidad, e.""tSigla"" sigla, e.""tNombre"" nombre_entidad, e.""cTipoEntidad"" cod_tipo_entidad, 
                                                                        te.""tDescripcionCorta"" desc_tipo_entidad
                                                                        FROM seguimiento_envios s, ""rstEmpresas"" e, ""rstTipoEntidad"" te, aperturas a
                                                                        WHERE s.id_apertura = a.id_apertura AND s.cod_entidad = e.""cEmpresa"" and e.""cTipoEntidad"" = te.""cTipoEntidad""
                                                                        AND a.activo AND s.activo
                                                                        AND e.""cEmpresa"" = @cod_entidad ", new { cod_entidad = cod_entidad }).FirstOrDefault();
            con.Close();
            return seguimientoActivoEntidad;
        }

        // obtiene el contexto de seguimiento y entidad de un cualquier seguimiento
        public static dynamic getSeguimientoEntidadDatos(int id_seguimiento)
        {
            dynamic seguimientoActivoEntidad = con.Query<dynamic>(@"SELECT a.id_apertura, a.fecha_corte, a.estado, s.id_seguimiento_envio, s.fecha_envio, s.estado, s.activo, s.valido,
                                                                        e.""cEmpresa"" cod_entidad, e.""tSigla"" sigla, e.""tNombre"" nombre_entidad, e.""cTipoEntidad"" cod_tipo_entidad, 
                                                                        te.""tDescripcionCorta"" desc_tipo_entidad
                                                                        FROM seguimiento_envios s, ""rstEmpresas"" e, ""rstTipoEntidad"" te, aperturas a
                                                                        WHERE s.cod_entidad = e.""cEmpresa"" and e.""cTipoEntidad"" = te.""cTipoEntidad"" AND a.id_apertura = s.id_apertura
                                                                        AND s.id_seguimiento_envio = @idSeg ", new { idSeg = id_seguimiento }).FirstOrDefault();
            con.Close();
            return seguimientoActivoEntidad;
        }


        public static int insertarSeguimientoEnvio(string codEntidad, int estado)
        {
            dynamic aperturaActiva = AperturaController.getAperturaActiva();

            con.Execute(@"UPDATE seguimiento_envios SET activo = false 
                            WHERE activo AND cod_entidad = @cod_entidad AND 
                            id_apertura IN (SELECT id_apertura FROM aperturas WHERE fecha_corte = @fecha_corte )",
                            new { cod_entidad = codEntidad, fecha_corte = (DateTime)aperturaActiva.fecha_corte });
            con.Close();
            SeguimientoEnvio seg = new SeguimientoEnvio();
            seg.id_apertura = aperturaActiva.id_apertura;
            seg.cod_entidad = codEntidad;
            seg.fecha_envio = DateTime.Now;
            seg.estado = estado.ToString();
            seg.activo = true;
            seg.valido = estado > 2;  // si es 3: advertencia o 4: valido
            seg.creado_por = 999;
            seg.creado_en = DateTime.Now;
            seg.id_seguimiento_envio = con.Query<int>(@"INSERT INTO seguimiento_envios(
                                                             id_apertura, cod_entidad, fecha_envio, estado, 
                                                            activo, valido, creado_por, creado_en)
                                                            VALUES ( @id_apertura, @cod_entidad, @fecha_envio, @estado,
                                                            @activo, @valido, @creado_por,  @creado_en) 
                                                            RETURNING id_seguimiento_envio", seg).Single();
            con.Close();
            return seg.id_seguimiento_envio;
        }

        public static void modificaEstado(int idSeguimientoEnvio, int codEstado)
        {
            con.Execute(@"UPDATE seguimiento_envios SET estado = @estado, valido = @valido
                            WHERE id_seguimiento_envio = @id_seguimiento_envio",
                            new { id_seguimiento_envio = idSeguimientoEnvio, estado = codEstado, valido = (codEstado > 2) });
            con.Close();
        }

        [HttpPut("validez")]
        [Authorize(Roles = "administrador,operador")]
        public object modificarSeguimiento([FromBody]dynamic obj)
        {
            string token = HttpHelpers.GetTokenFromHeader(HttpContext);
            if (token == "")
                return Unauthorized();

            Base helper = new Base(AppSettings, token, HttpContext.Connection.RemoteIpAddress.ToString());

            SeguimientoEnvio seg = con.Query<SeguimientoEnvio>(@"Select * from seguimiento_envios 
                                                                WHERE activo AND id_seguimiento_envio = @id ", new { id = (Int32)obj.id_seguimiento_envio }).FirstOrDefault();
            con.Close();
            seg.observaciones = obj.observaciones;
            seg.valido = obj.valido;
            seg.modificado_por = helper.UsuarioId;
            seg.modificado_en = DateTime.Now;

            if (!seg.valido)
            {
                seg.id_consolidacion = null;
                seg.estado_cierre = null;
            }

            con.Execute(@"UPDATE  seguimiento_envios SET observaciones = @observaciones, valido = @valido, 
                            id_consolidacion = @id_consolidacion, estado_cierre = @estado_cierre,
                            modificado_por = @modificado_por, modificado_en = @modificado_en 
                            WHERE id_seguimiento_envio = @id_seguimiento_envio ", seg);
            con.Close();

            helper.AddLog(Log.TipoOperaciones.Modificacion, typeof(SgmntController), "modificarSeguimiento", seg);
            return new
            {
                status = "success",
            };
        }


    }
}