using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Dapper;
using SVD.Models;
using System;
using System.Dynamic;
using SistemaValidacionDatos.controllers;
using SVD.Utils;
using Microsoft.Extensions.Options;
using BO.Aps.Auditoria;
using Microsoft.AspNetCore.Authorization;

namespace SVD.Controllers
{
    [Route("svd/api/[Controller]")]
    public class AperturaController : Controller
    {
        private static IDbConnection con = BdPG.instancia();

        // my code
        private SVD.Models.Settings AppSettings;

        //public AperturaController()
        //{ }

        public AperturaController(IOptions<SVD.Models.Settings> settings)
        {
            AppSettings = settings.Value;
        }
        // end my code


        // obtiene la apertura activa (la ultima)
        // se lo usa para obtener el estado (iniciado, detenido o concluido ), y los tipos de cambio del mismo o la fechaCorte de la misma
        [HttpGet("activo")]
        [Authorize(Roles = "administrador,carga_informacion,operador")]  // definir s�lo los roles que pueden invocar a esta acci�n
        public dynamic obtieneAperturaActiva()
        {
            try
            {
                // my code
                string token = HttpHelpers.GetTokenFromHeader(HttpContext);
                if (token == "")
                    return Unauthorized();

                Base helper = new Base(AppSettings, token, HttpContext.Connection.RemoteIpAddress.ToString());


                dynamic seg = con.Query<dynamic>(@"SELECT a.id_apertura, a.fecha_corte, a.iniciado, a.fecha_inicio_envios, a.fecha_detiene_envios,
                                                    a.estado, a.activo, a.factor_tc, conf.id_configuracion, conf.usa_factor_tc, conf.moneda_factor_tc, conf.margen_partes_ef, 
                                                    conf.margen_validacion_soat,
                                                    c.""mCompra"", c.""mVenta"", ""mEuroBS"", ""mUFV""
                                                FROM aperturas a, ""cmtTipoCambio"" c , configuraciones conf
                                                WHERE a.activo  AND c.""fTipoCambio"" = a.fecha_corte  
                                                AND  a.id_configuracion = conf.id_configuracion").FirstOrDefault();
                con.Close();


                // my code
                // helper.AddLog(Log.TipoOperaciones.Modificacion, typeof(AperturaController), "getAperturaActiva", seg);
                // end my code

                if (seg != null)
                {
                    seg.dia_control = seg.fecha_corte.Day;
                    seg.mes_control = seg.fecha_corte.Month;
                    seg.ano_control = seg.fecha_corte.Year;
                    seg.dia_actual = DateTime.Now.Day.ToString();
                    seg.mes_actual = DateTime.Now.Month.ToString();
                    seg.ano_actual = DateTime.Now.Year.ToString();
                }
                return new
                {
                    status = "success",
                    mensaje = "encontrado",
                    codigo = "200",
                    data = seg
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

        public static dynamic getAperturaActiva()
        {
            dynamic seg = con.Query<dynamic>(@"SELECT a.id_apertura, a.fecha_corte, a.iniciado, a.fecha_inicio_envios, a.fecha_detiene_envios,
                                                    a.estado, a.activo, a.factor_tc, conf.id_configuracion, conf.usa_factor_tc, conf.moneda_factor_tc, conf.margen_partes_ef, 
                                                    conf.margen_validacion_soat,
                                                    c.""mCompra"", c.""mVenta"", ""mEuroBS"", ""mUFV""
                                                FROM aperturas a, ""cmtTipoCambio"" c , configuraciones conf
                                                WHERE a.activo  AND c.""fTipoCambio"" = a.fecha_corte  
                                                AND  a.id_configuracion = conf.id_configuracion").FirstOrDefault();
            con.Close();
            if (seg != null)
            {
                seg.dia_control = seg.fecha_corte.Day;
                seg.mes_control = seg.fecha_corte.Month;
                seg.ano_control = seg.fecha_corte.Year;
                seg.dia_actual = DateTime.Now.Day.ToString();
                seg.mes_actual = DateTime.Now.Month.ToString();
                seg.ano_actual = DateTime.Now.Year.ToString();
            }
            return seg;
        }

        [HttpGet("ultima_de_fecha_corte/{mes}/{ano}")]
        [Authorize(Roles = "administrador,carga_informacion,operador")]
        public dynamic getAperturaFechaCorte(int mes, int ano)
        {
            dynamic seg = con.Query<dynamic>(@"SELECT a.id_apertura, a.fecha_corte, a.iniciado, a.fecha_inicio_envios, a.fecha_detiene_envios,
                                                    a.estado, a.activo, a.factor_tc, conf.id_configuracion, conf.usa_factor_tc, conf.moneda_factor_tc, conf.margen_partes_ef,
                                                    conf.margen_validacion_soat,
                                                    c.""mCompra"", c.""mVenta"", ""mEuroBS"", ""mUFV""
                                                FROM aperturas a, ""cmtTipoCambio"" c, configuraciones conf
                                                WHERE
                                                a.id_apertura = (select  MAX(id_apertura)
                                    from aperturas where extract(year from fecha_corte) = @ano and extract(month from fecha_corte) = @mes)
                                                AND c.""fTipoCambio"" = a.fecha_corte
                                                AND a.id_configuracion = conf.id_configuracion", new { mes = mes, ano = ano }).FirstOrDefault();
            con.Close();
            if (seg != null)
            {
                seg.dia_control = seg.fecha_corte.Day;
                seg.mes_control = seg.fecha_corte.Month;
                seg.ano_control = seg.fecha_corte.Year;
                seg.dia_actual = DateTime.Now.Day.ToString();
                seg.mes_actual = DateTime.Now.Month.ToString();
                seg.ano_actual = DateTime.Now.Year.ToString();
            }
            return new
            {
                status = "success",
                mensaje = "encontrado",
                codigo = "200",
                data = seg
            };
        }



        public static dynamic getAperturaDeSeguimiento(int id_seguimiento)
        {
            dynamic aper = con.Query<dynamic>(@"SELECT a.id_apertura, a.fecha_corte, a.iniciado, a.fecha_inicio_envios, a.fecha_detiene_envios,
                                                    a.estado, a.activo, a.factor_tc, conf.id_configuracion, conf.usa_factor_tc, conf.moneda_factor_tc, conf.margen_partes_ef,
                                                    conf.margen_validacion_soat,
                                                    c.""mCompra"", c.""mVenta"", ""mEuroBS"", ""mUFV""
                                                FROM aperturas a, ""cmtTipoCambio"" c , configuraciones conf, seguimiento_envios s
                                                WHERE a.id_apertura = s.id_apertura AND c.""fTipoCambio"" = a.fecha_corte  AND  a.id_configuracion = conf.id_configuracion
                                                AND id_seguimiento_envio = @idSeg", new { idSeg = id_seguimiento }).FirstOrDefault();
            con.Close();
            return aper;
        }


        //debe  llegar un objeto con minimamente fecha_corte
        [HttpPost("iniciar")]
        [Authorize(Roles = "administrador,operador")]
        public object crearApertura([FromBody]Apertura obj)
        {
            try
            {
                // my code
                string token = HttpHelpers.GetTokenFromHeader(HttpContext);
                if (token == "")
                    return Unauthorized();

                Base helper = new Base(AppSettings, token, HttpContext.Connection.RemoteIpAddress.ToString());

                Configuracion confActiva = ConfiguracionesController.configuracionActiva();
                // se obtiene de configuracion si esta habilitada=1 o deshabilitada=0 la opcion de FactorTC variable
                decimal factorTC = 0;
                if (confActiva.usa_factor_tc)
                {
                    DateTime fechaFinMesAnterior = obj.fecha_corte.AddDays(-obj.fecha_corte.Day); // se calcula el ultima dia del mes anterior
                    //CotizacionesController cot = new CotizacionesController();
                    dynamic tc = CotizacionesController.cotizacionFecha(obj.fecha_corte);
                    dynamic tcFinMesAnterior = CotizacionesController.cotizacionFecha(fechaFinMesAnterior); // tipos de cambio del ultimo dia del mes anterior
                    if (confActiva.moneda_factor_tc == "2")
                        factorTC = (tc.mCompra / tcFinMesAnterior.mCompra) - 1;
                    else if (confActiva.moneda_factor_tc == "4")
                        factorTC = (tc.mUFV / tcFinMesAnterior.mUFV) - 1;
                    else if (confActiva.moneda_factor_tc == "5")
                        factorTC = (tc.mEuroBS / tcFinMesAnterior.mEuroBS) - 1;
                }

                obj.fecha_corte = obj.fecha_corte;
                obj.iniciado = true;
                obj.activo = true;
                obj.fecha_inicio_envios = DateTime.Now;
                obj.estado = "iniciado";  // iniciado, detenido, concluido
                obj.id_configuracion = confActiva.id_configuracion;
                obj.factor_tc = factorTC;
                obj.creado_en = DateTime.Now;
                obj.creado_por = helper.UsuarioId;

                con.Query<int>(@"UPDATE aperturas set activo = false  WHERE activo = true");
                con.Close();
                obj.id_apertura = con.Query<int>(@"INSERT INTO aperturas(
                                                    fecha_corte, iniciado, fecha_inicio_envios, fecha_detiene_envios, 
                                                    activo, estado, id_configuracion, factor_tc, creado_por, creado_en)
                                                VALUES ( @fecha_corte, @iniciado, @fecha_inicio_envios, @fecha_detiene_envios, @activo,
                                                @estado, @id_configuracion, @factor_tc, @creado_por,  @creado_en) 
                                                RETURNING id_apertura", obj).Single();
                con.Close();

                helper.AddLog(Log.TipoOperaciones.Alta, typeof(AperturaController), "crearApertura", obj);

                return new
                {
                    status = "success",
                    mensaje = "creado",
                    codigo = "201",
                    data = obj
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

        [HttpPut("detener")]
        [Authorize(Roles = "administrador,operador")]
        public object cerrarApertura([FromBody]int id_apertura)
        {
            try
            {
                // my code
                string token = HttpHelpers.GetTokenFromHeader(HttpContext);
                if (token == "")
                    return Unauthorized();

                Base helper = new Base(AppSettings, token, HttpContext.Connection.RemoteIpAddress.ToString());

                Apertura obj = new Apertura();
                obj.id_apertura = id_apertura;
                obj.iniciado = false;
                obj.activo = true;
                obj.fecha_detiene_envios = DateTime.Now;
                obj.estado = "detenido";
                obj.modificado_por = helper.UsuarioId;
                obj.modificado_en = DateTime.Now;
                con.ExecuteScalar<int>(@"UPDATE aperturas SET 
                                            iniciado = @iniciado, fecha_detiene_envios = @fecha_detiene_envios, estado = @estado, 
                                            modificado_por = @modificado_por, modificado_en = @modificado_en
                                            WHERE id_apertura = @id_apertura", obj);
                con.Close();
                helper.AddLog(Log.TipoOperaciones.Modificacion, typeof(AperturaController), "cerrarApertura", obj);
                return new
                {
                    status = "success",
                    mensaje = "modificado",
                    codigo = "200",
                    data = obj
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


        [HttpGet("anosAperturas")]
        [Authorize(Roles = "administrador,operador")]
        public object obtieneAnosAperturas()
        {
            List<int> anos = new List<int>(con.Query<int>("select distinct extract( year from fecha_corte) as ano from aperturas order by ano desc"));
            con.Close();
            return new
            {
                status = "success",
                data = anos
            };
        }
    }
}