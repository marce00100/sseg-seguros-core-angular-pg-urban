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
    public class EntidadesController : Controller
    {
        private static IDbConnection con = BdPG.instancia();

        [HttpGet()]
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
        public object insertar([FromBody]Entidad entidad)
        {
            dynamic entCod = con.Query<dynamic>(@"SELECT  * FROM ""rstEmpresas"" WHERE ""cEmpresa"" = @codigo", new { codigo = entidad.cEmpresa }).FirstOrDefault();
            con.Close();
            string status, mensaje, codigo;
            object data;
            if (entCod == null)
            {
                entidad.creado_en = DateTime.Now;
                entidad.creado_por = 999;
                con.Execute(@"INSERT INTO ""rstEmpresas""(
                                                     ""cEmpresa"", ""tSigla"", ""tNombre"", ""cTipoEntidad"", 
                                                     ""bHabilitado"",  ""tNombreCorto"", 
                                                    ""tFuncionamiento"", ""tAdecuacion"", creado_por, creado_en)
                                                VALUES ( @cEmpresa, @tSigla, @tNombre, @cTipoEntidad, @bHabilitado, 
                                                @tNombreCorto, @tFuncionamiento, @tAdecuacion, 
                                                 @creado_por, @creado_en)", entidad);
                con.Close();
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


        [HttpPut("{id}")]
        public object actualizar([FromBody]Entidad entidad, int id)
        {
            entidad.modificado_en = DateTime.Now;
            entidad.modificado_por = 999;
            con.Execute(@"UPDATE ""rstEmpresas"" SET 
                                ""tSigla"" = @tSigla, ""tNombre"" = @tNombre,
                                ""cTipoEntidad"" =  @cTipoEntidad, ""bHabilitado"" = @bHabilitado, 
                                ""tNombreCorto"" = @tNombreCorto, ""tFuncionamiento"" =  @tFuncionamiento, 
                                ""tAdecuacion"" = @tAdecuacion, 
                                modificado_por = @modificado_por, modificado_en =  @modificado_en
                                WHERE ""cEmpresa"" = @cEmpresa", entidad);

            con.Close();
            return new
            {
                status = "success",
                mensaje = "actualizado",
                codigo = "200",
                data = entidad
            };
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