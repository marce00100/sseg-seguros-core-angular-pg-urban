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
    public class EntidadesController : Controller
    {
        private static IDbConnection con = BdPG.instancia();

        private SVD.Models.Settings AppSettings;

        //public EntidadesController()
        //{

        //}
        public EntidadesController(IOptions<SVD.Models.Settings> settings)
        {
            AppSettings = settings.Value;
        }

        [HttpGet()]
        [Authorize(Roles = "administrador")]
        public object todos()
        {
            IEnumerable<object> entidadesList = con.Query<object>(@"SELECT t.""tDescripcionCorta"", e.*  FROM ""rstEmpresas"" e, ""rstTipoEntidad"" t 
                                                                    WHERE e.""cTipoEntidad"" = t.""cTipoEntidad"" 
                                                                   ORDER BY e.""cTipoEntidad"" ");
            con.Close();
            return new
            {
                status = "success",
                mensaje = "encontrados",
                codigo = "200",
                numero = entidadesList.Count(),
                data = entidadesList
            };
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "administrador")]
        public object entidadId(string id)
        {
            object entidadItem = con.Query<object>(@"SELECT * FROM ""rstEmpresas"" WHERE ""cEmpresa""  = @id", new { id = id }).FirstOrDefault();
            con.Close();
            string status, mensaje, codigo;
            if (entidadItem == null)
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
                data = entidadItem
            };
        }

        [HttpGet("datos/{cod}")]
        [Authorize(Roles = "administrador")]
        public dynamic endidaCodigoDatos(string cod)
        {
            dynamic entCtx = con.Query<dynamic>(@"SELECT e.""cEmpresa"" cod_entidad, e.""tSigla"" sigla, e.""tNombre"" nombre_entidad, e.""cTipoEntidad"" cod_tipo_entidad, 
                                                         te.""tDescripcionCorta"" desc_tipo_entidad 
                                                FROM ""rstEmpresas"" e, ""rstTipoEntidad"" te
                                                WHERE e.""cTipoEntidad"" = te.""cTipoEntidad""
                                                AND e.""cEmpresa"" = @cod ", new { cod = cod }).FirstOrDefault();
            con.Close();
            return new
            {
                status = "success",
                mensaje = "encontrados",
                codigo = "200",
                data = entCtx
            };
        }

        public static dynamic obtenerEntidadDeCodigo(string cod)
        {
            dynamic entCtx = con.Query<dynamic>(@"SELECT e.""cEmpresa"" cod_entidad, e.""tSigla"" sigla, e.""tNombre"" nombre_entidad, e.""cTipoEntidad"" cod_tipo_entidad, 
                                                         te.""tDescripcionCorta"" desc_tipo_entidad 
                                                FROM ""rstEmpresas"" e, ""rstTipoEntidad"" te
                                                WHERE e.""cTipoEntidad"" = te.""cTipoEntidad""
                                                AND e.""cEmpresa"" = @cod ", new { cod = cod }).FirstOrDefault();
            return entCtx;
        }

        [HttpPost()]
        [Authorize(Roles = "administrador")]
        public object insertar([FromBody]Entidad entidad)
        {
            try
            {
                // my code
                string token = HttpHelpers.GetTokenFromHeader(HttpContext);
                if (token == "")
                    return Unauthorized();

                Base helper = new Base(AppSettings, token, HttpContext.Connection.RemoteIpAddress.ToString());

                dynamic entCod = con.Query<dynamic>(@"SELECT  * FROM ""rstEmpresas"" WHERE ""cEmpresa"" = @codigo", new { codigo = entidad.cEmpresa }).FirstOrDefault();
                con.Close();
                string status, mensaje, codigo;
                object data;
                if (entCod == null)
                {
                    entidad.creado_en = DateTime.Now;
                    entidad.creado_por = helper.UsuarioId;
                    con.Execute(@"INSERT INTO ""rstEmpresas""(
                                                     ""cEmpresa"", ""tSigla"", ""tNombre"", ""cTipoEntidad"", 
                                                     ""bHabilitado"",  ""tNombreCorto"", 
                                                    ""tFuncionamiento"", ""tAdecuacion"", creado_por, creado_en)
                                                VALUES ( @cEmpresa, @tSigla, @tNombre, @cTipoEntidad, @bHabilitado, 
                                                @tNombreCorto, @tFuncionamiento, @tAdecuacion, 
                                                 @creado_por, @creado_en)", entidad);
                    con.Close();

                    helper.AddLog(Log.TipoOperaciones.Alta, typeof(EntidadesController), "insertar", entidad);
                    status = "created";
                    mensaje = "creado";
                    codigo = "201";
                    data = entidad;
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
        public object actualizar([FromBody]Entidad entidad, int id)
        {
            try
            {
                // my code
                string token = HttpHelpers.GetTokenFromHeader(HttpContext);
                if (token == "")
                    return Unauthorized();

                Base helper = new Base(AppSettings, token, HttpContext.Connection.RemoteIpAddress.ToString());

                entidad.modificado_en = DateTime.Now;
                entidad.modificado_por = helper.UsuarioId;
                con.Execute(@"UPDATE ""rstEmpresas"" SET 
                                ""tSigla"" = @tSigla, ""tNombre"" = @tNombre,
                                ""cTipoEntidad"" =  @cTipoEntidad, ""bHabilitado"" = @bHabilitado, 
                                ""tNombreCorto"" = @tNombreCorto, ""tFuncionamiento"" =  @tFuncionamiento, 
                                ""tAdecuacion"" = @tAdecuacion, 
                                modificado_por = @modificado_por, modificado_en =  @modificado_en
                                WHERE ""cEmpresa"" = @cEmpresa", entidad);

                con.Close();

                helper.AddLog(Log.TipoOperaciones.Modificacion, typeof(EntidadesController), "actualizar", entidad);
                return new
                {
                    status = "success",
                    mensaje = "actualizado",
                    codigo = "200",
                    data = entidad
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

        // [HttpDelete("{id}")]
        // public object eliminar(int id)
        // {
        //     Entidad entidadOld = con.Query<Entidad>("SELECT * FROM entidades WHERE id_entidad = @id_entidad and vigente = true", new { id_entidad = id }).FirstOrDefault();
        //     if (Valor.habilitarHistoricos)
        //     {
        //         entidadOld.vigente = false;
        //         funcionUpdate(entidadOld);
        //     }

        //     Entidad entidadEliminado = entidadOld;
        //     entidadEliminado.eliminado = true;
        //     entidadEliminado.vigente = true;
        //     entidadEliminado.modificado_en = DateTime.Now;
        //     entidadEliminado.modificado_por = 999;
        //     if (Valor.habilitarHistoricos)
        //     {
        //         funcionInsert(entidadEliminado);
        //     }
        //     else
        //     {
        //         funcionUpdate(entidadEliminado);
        //     }
        //     return new
        //     {
        //         status = "success",
        //         mensaje = "eliminado",
        //         codigo = "200",
        //         data = entidadEliminado
        //     };
        // }
    }
}