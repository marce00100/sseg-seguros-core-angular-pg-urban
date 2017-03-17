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
    public class ArchivosController : Controller
    {
        private static IDbConnection con = BdPG.instancia();

        // my code
        private SVD.Models.Settings AppSettings;

        public ArchivosController(IOptions<SVD.Models.Settings> settings)
        {
            AppSettings = settings.Value;
        }
        // end my code

        [HttpGet()]
        [Authorize(Roles = "administrador")]
        public object todos()
        {
            IEnumerable<object> archivosList = con.Query<object>(@"SELECT * FROM archivos where vigente AND NOT eliminado order by codigo ");
            con.Close();
            return new
            {
                status = "success",
                mensaje = "encontrados",
                codigo = "200",
                numero = archivosList.Count(),
                data = archivosList
            };
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "administrador")]
        public object archivoId(int id)
        {
            object archivoItem = con.Query<object>("SELECT * FROM archivos WHERE id_archivo  = @id", new { id = id }).FirstOrDefault();
            con.Close();
            string status, mensaje, codigo;
            if (archivoItem == null)
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
                data = archivoItem
            };
        }

        [HttpPost()]
        [Authorize(Roles = "administrador")]
        public object insertar([FromBody]Archivo archivo)
        {
            try
            {
                // my code
                string token = HttpHelpers.GetTokenFromHeader(HttpContext);
                if (token == "")
                    return Unauthorized();

                Base helper = new Base(AppSettings, token, HttpContext.Connection.RemoteIpAddress.ToString());



                archivo.id_registro = Guid.NewGuid().ToString();
                archivo.vigente = true;
                archivo.eliminado = false;
                archivo.creado_en = DateTime.Now;
                archivo.creado_por = helper.UsuarioId;
                archivo.id_archivo = funcionInsert(archivo);

                // my code
                helper.AddLog(Log.TipoOperaciones.Alta, typeof(ArchivosController), "insertar", archivo);
                // end my code

                return new
                {
                    status = "created",
                    mensaje = "insertado",
                    codigo = "201",
                    data = archivo
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
        public object actualizar([FromBody]Archivo archivo, int id)
        {
            try
            {
                // my code
                string token = HttpHelpers.GetTokenFromHeader(HttpContext);
                if (token == "")
                    return Unauthorized();

                Base helper = new Base(AppSettings, token, HttpContext.Connection.RemoteIpAddress.ToString());

                archivo.vigente = true;
                archivo.eliminado = false;
                archivo.modificado_en = DateTime.Now;
                archivo.modificado_por = helper.UsuarioId;
                if (Config.habilitarHistoricos)
                {
                    Archivo archivoOld = con.Query<Archivo>("SELECT * FROM archivos WHERE id_registro = @id_registro and vigente = true", new { id_registro = archivo.id_registro }).FirstOrDefault();
                    con.Close();
                    archivoOld.vigente = false;
                    funcionUpdate(archivoOld);
                    archivo.id_archivo = funcionInsert(archivo);
                }
                else
                {
                    funcionUpdate(archivo);
                    helper.AddLog(Log.TipoOperaciones.Modificacion, typeof(ArchivosController), "actualizar", archivo);
                }

                return new
                {
                    status = "success",
                    mensaje = "actualizado",
                    codigo = "200",
                    data = archivo
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

        [HttpDelete("{id}")]
        [Authorize(Roles = "administrador")]
        public object eliminar(int id)
        {
            Archivo archivoOld = con.Query<Archivo>("SELECT * FROM archivos WHERE id_archivo = @id_archivo and vigente = true", new { id_archivo = id }).FirstOrDefault();
            con.Close();
            if (Config.habilitarHistoricos)
            {
                archivoOld.vigente = false;
                funcionUpdate(archivoOld);
            }

            Archivo archivoEliminado = archivoOld;
            archivoEliminado.eliminado = true;
            archivoEliminado.vigente = true;
            archivoEliminado.modificado_en = DateTime.Now;
            archivoEliminado.modificado_por = 999;
            if (Config.habilitarHistoricos)
            {
                funcionInsert(archivoEliminado);
            }
            else
            {
                funcionUpdate(archivoEliminado);
            }
            return new
            {
                status = "success",
                mensaje = "eliminado",
                codigo = "200",
                data = archivoEliminado
            };
        }




        private int funcionInsert(Archivo archivo)
        {
            int id_archivo = con.Query<int>(@"INSERT INTO archivos(
                                                nombre, descripcion, periodos_envio, cods_tipo_entidad, 
                                                validacion, codigo, activo, id_registro, vigente, eliminado, 
                                                creado_por, creado_en, modificado_por, modificado_en)
                                            VALUES ( @nombre, @descripcion, @periodos_envio, @cods_tipo_entidad, 
                                                @validacion, @codigo, @activo, @id_registro, 
                                                @vigente, @eliminado, @creado_por, @creado_en, @modificado_por, @modificado_en) 
                                                RETURNING id_archivo", archivo).Single();
            con.Close();
            return id_archivo;
        }
        private Archivo funcionUpdate(Archivo archivo)
        {
            con.ExecuteScalar<int>(@"UPDATE archivos SET 
                                            nombre = @nombre, descripcion = @descripcion , periodos_envio = @periodos_envio, 
                                            cods_tipo_entidad =  @cods_tipo_entidad, validacion = @validacion, 
                                            codigo = @codigo, activo =  @activo, 
                                            id_registro =  @id_registro, vigente = @vigente, 
                                            eliminado = @eliminado, modificado_por = @modificado_por, modificado_en =  @modificado_en
                                            WHERE id_archivo = @id_archivo", archivo);
            con.Close();
            return archivo;
        }

        public static List<Archivo> getArchivosDeEntidadMes(string cod_entidad, int mes)
        {
            // ArchivosController archivosList = new ArchivosController(); 
            List<Archivo> archivosValidos = (List<Archivo>)con.Query<Archivo>(@"SELECT a.* 
                                                FROM archivos a, ""rstEmpresas"" e, ""rstTipoEntidad"" t
                                                WHERE  a.cods_tipo_entidad ILIKE  '%|'||t.""cTipoEntidad""||'|%' 
                                                    AND e.""cTipoEntidad"" = t.""cTipoEntidad""
                                                    AND e.""bHabilitado"" = 'S' 
                                                    and a.activo and a.vigente and not a.eliminado
                                                AND e.""cEmpresa"" = @codigo_entidad AND a.periodos_envio ILIKE '%|' || @mes_control ||'|%'
                                                AND a.codigo not in  (SELECT codigo from archivos WHERE entidades_excepciones ILIKE '%|' || @codigo_entidad || '|%' 
                                                                        and activo and vigente and not eliminado
                                                                        )
                                                ORDER BY a.codigo ", new { codigo_entidad = cod_entidad, mes_control = mes.ToString() });
            con.Close();
            return archivosValidos;
        }

        public static string encadenaArchivosDeEntidadMes(string cod_entidad, int mes)
        {
            List<Archivo> archivosList = ArchivosController.getArchivosDeEntidadMes(cod_entidad, mes);
            string cadenaArchivos = "";
            foreach (Archivo arch in archivosList)
            {
                cadenaArchivos += arch.codigo;
            }

            return cadenaArchivos;

        }

    }
}