using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Dapper;
using SVD.Models;
using System;
using System.Dynamic;
using System.Threading;

namespace SVD.Controllers
{
    [Route("svd/api/[Controller]")]
    public partial class ConsolidacionController : Controller
    {
        private static IDbConnection con = BdPG.instancia();
        private static IDbConnection conSegSql = BdSegurosSQL.instancia();
        private static IDbConnection conOLAPSql = BdSegurosOLAPSQL.instancia();

        private dynamic ctxSeguimiento;
        private DateTime fechaCorte;
        private string anoControl;
        private string mesControl;
        private string diaControl;
        private int anoControlInt;
        private int mesControlInt;
        private int diaControlInt;
        private int FechaCorteInt;
        private string codigoEntidad;
        private bool esMesSolvencia;
        private bool poblarSQLSeguros = Config.poblarSQLTablasDbSeguros;
        private bool poblarSQLSegOLAP = Config.poblarSQLTablasDbSegurosOLAP;
        private int paso = 0;



        private dynamic[] comodinesParaInsertar = {
            new {
                codigo_entidad = "201",
                query = @"INSERT INTO  ""iftBalanceEstadoResultados"" VALUES('201', @fecha_corte_int, '308', '0', '', 0, 0, 0, 0, 0, 0, 0, 0, 0 ) "
            },
            new {
                codigo_entidad = "",
                query = ""
            }
        };


        private void inicializaGlobalVars()
        {
            // this.fechaCorte = this.ctxSeguimiento.fecha_corte;
            this.anoControl = this.fechaCorte.ToString("yyyy");
            this.mesControl = this.fechaCorte.ToString("MM");
            this.diaControl = this.fechaCorte.ToString("dd");
            this.anoControlInt = this.fechaCorte.Year;
            this.mesControlInt = this.fechaCorte.Month;
            this.diaControlInt = this.fechaCorte.Day;
            this.FechaCorteInt = this.anoControlInt * 10000 + this.mesControlInt * 100 + this.diaControlInt;
            dynamic mesesSolvencia = ConstantesController.obtieneConstantesDeDimension("meses_calculo_solvencia").FirstOrDefault();
            this.esMesSolvencia = mesesSolvencia.valor.Contains("|" + this.mesControlInt.ToString() + "|");

        }

        //##############################################################################################################################
        /////////////////////////////////////////////  1. CONSOLIDACION /////////////////////////////////////////////////////////////

        [HttpPost("envios_validos")]
        public object consolidacionDeEntidad([FromBody]dynamic objObservaciones)
        {
            SgmntController segObj = new SgmntController();
            List<dynamic> enviosAct = segObj.getSeguimientoEntidades("apertura_activa").data;
            enviosAct = enviosAct.Where(x => x.valido && (x.estado_cierre == null || x.estado_cierre == "")).ToList();

            this.fechaCorte = con.Query<DateTime>("Select fecha_corte from aperturas where activo").FirstOrDefault();
            con.Close();
            this.inicializaGlobalVars();

            string estadoCierre = this.esMesSolvencia ? "consolidado" : "cerrado";
            List<dynamic> listaRes = new List<dynamic>();
            bool procesoExitoso = true;
            Exception excepcion = null; // TO DO habilitar error para visualizar errores, deshabilitar en produccion 

            if (enviosAct.Count > 0)
            {
                // dynamic consolidacionActiva = this.obtieneUltimaConsolidacion().data;
                //INICIALIZA PROCESO CONSOLIDACION 
                int id_consolidacion = this.insertConsolidacion(objObservaciones.observaciones.ToString());

                foreach (dynamic envio in enviosAct)
                {
                    try
                    {
                        // Realiza la consolidacion por cada entidad que ha eviado y la info sea valida y activa, y no haya sido consolidaa
                        if (envio.valido && (envio.estado_cierre == null || envio.estado_cierre == ""))
                        {
                            this.ctxSeguimiento = envio;
                            this.codigoEntidad = ctxSeguimiento.cod_entidad;

                            // llena las tablas iftPartesProduccionSiniestros, 
                            this.llenaIftPartesProduccionSiniestros(this.codigoEntidad);
                            // llena las tablas iftPartesProduccionRamos, 
                            this.llenaIftPartesProduccionRamos(this.codigoEntidad);
                            // llena iftPartesSiniestrosRamos 
                            this.llenaIftPartesSiniestrosRamos(this.codigoEntidad);
                            // llena tabla , iftPartesSegurosLargoPlazo
                            this.llenaIftPartesSegurosLargoPlazo(this.codigoEntidad);
                            // Se realiza la consolidacion
                            // carga los saldos totales de la tabla val_balance_estado_resultados
                            this.llenaSaldosTotales(this.codigoEntidad);
                            //  1  realiza la carga a la tabla iftBalanceEstadoResultados  sin duplicados
                            this.consolidaInfoMensualAntesDeCierre(this.codigoEntidad);
                            // 2. Consolida PartesProduccionCorredoras  iftPartesProduccionCorredoras
                            this.consolidaProduccionCorredoras(this.codigoEntidad);

                            // 3 inserta las excepciones  Caso BUPA 201
                            this.insertarComodines();

                            if (esMesSolvencia)
                            {
                                //inserta campos vacios por la entidad en las tablas iftPatrimonioTecnico y tMSPrevisionales
                                this.insertaPatrimonioTecnicoVacio();
                                this.insertaMargenSolvenciaPrevisionalesVacio();
                            }
                            this.paso = 1;

                            // llama al metodo que realiza pasos para el cierre
                            this.procedimientosHastaMargenSolvencia(this.codigoEntidad);
                            // llama  a los procedimientos siguientes para informacion estadistica WEB y SIG
                            this.procedimientosCierreHastaSIG(this.codigoEntidad);

                            this.informacionFinancieraOLAP();


                            ////_____________________________ se crea la respuesta  que es una lista de las compañias enviadas ________________________                    
                            object seguimientoConsolidacion = new
                            {
                                id_seguimiento_envio = envio.id_seguimiento_envio,
                                cod_entidad = envio.cod_entidad,
                                id_consolidacion = id_consolidacion,
                                estado_cierre = estadoCierre
                            };
                            // asigna el valor de id_consolidacion al seguimiento y le asigna su estadoCierre
                            con.Execute(@"UPDATE seguimiento_envios SET id_consolidacion = @id_consolidacion, estado_cierre = @estado_cierre
                            WHERE id_seguimiento_envio = @id_seguimiento_envio", seguimientoConsolidacion);
                            con.Close();

                            listaRes.Add(seguimientoConsolidacion);

                        }
                    }
                    catch (Exception e)
                    {
                        procesoExitoso = false;
                        excepcion = e;
                        break;
                    };
                };

                estadoCierre = procesoExitoso ? estadoCierre : ""; //  si no hubo error se cambi al estado Consolidado o cerrado , segun si fue normal o trimestral, caso contrario "consolidacion.estado" vuelve al valor que tenia originalmente
                this.estadoConsolidacion(id_consolidacion, estadoCierre, false);
            }
            con.Close();
            conSegSql.Close();
            return new
            {
                status = "success",
                calculo_solvencia = this.esMesSolvencia,
                errorEnCierre = !procesoExitoso,
                excepcion = excepcion,
                paso = this.paso,
                data = new
                {
                    lista = listaRes,
                    estadoCierre = estadoCierre
                }
            };
        }

        //##############################################################################################################################################
        ////////////////////////////////////////// 2 CIERRE  en caso de necesitar margen de Solvencia (TRIMESTRAL)  ///////////////////////////////////////////////////////////////////////
        [HttpPost("re_calcular_margen_solvencia")] // este ptroceso es llamado cuando es el caso del Margen de solvencia, cada trimestre, se realiza esta segunda llamada a los procesos del 1 al 4b
        public object cerrarConsolidados([FromBody]List<dynamic> consolidadosOb) //consolidadosOB tiene los objetos envio  con mPatTecnico y mMSPrevisional de cada entidad
        {
            SgmntController segObj = new SgmntController();
            List<dynamic> enviosAct = segObj.getSeguimientoEntidades("apertura_activa").data;
            List<dynamic> consolidacionesList = enviosAct.Where(x => x.estado_cierre != null && x.estado_cierre == "consolidado").Select(x => x.id_consolidacion).Distinct().ToList();

            this.fechaCorte = con.Query<DateTime>("Select fecha_corte from aperturas where activo").FirstOrDefault();
            con.Close();
            this.inicializaGlobalVars();
            List<dynamic> listaRes = new List<dynamic>();

            bool procesoExitoso = true;
            Exception excepcion = null;

            // Ya no se realiza este paso y aque pueden ser varias consolidaciones para un periodo
            // dynamic consolidacionActiva = this.obtieneUltimaConsolidacion().data;
            // //INICIALIZA PROCESO CONSOLIDACION 
            // int id_consolidacion = consolidacionActiva.id_consolidacion;

            string estadoCierre = "cerrado";
            foreach (dynamic consolidacionId in consolidacionesList)
            {
                int id_consolidacion = Convert.ToInt32(consolidacionId);
                List<dynamic> enviosConsolidacion = enviosAct.Where(x => x.id_consolidacion == id_consolidacion).ToList();
                // return consolidacionesList;
                foreach (dynamic envio in enviosConsolidacion)
                {
                    try
                    {
                        if ((bool)envio.valido == true && envio.estado_cierre == "consolidado" && envio.id_consolidacion == id_consolidacion)
                        {
                            this.ctxSeguimiento = envio;
                            this.codigoEntidad = envio.cod_entidad;

                            dynamic consolidadoOBItem = consolidadosOb.Where(x => x.id_seguimiento_envio == envio.id_seguimiento_envio).FirstOrDefault();
                            //inserta los valores de PAtrimonioTecnico y MSprevisional 
                            decimal valorPT = Convert.ToDecimal(consolidadoOBItem.mPatTecnico.ToString().Replace(".", ",")); //envio.mPatTecnico.ToString().ToDecimal();
                            decimal valorMSprevisional = Convert.ToDecimal(consolidadoOBItem.mMSPrevisional.ToString().Replace(".", ","));

                            this.insertarPatrimonioTecnico(valorPT);
                            this.insertarMSolvenciaPrevisional(valorMSprevisional);

                            //______________________ Continua con los pasos Hasta Margen de Solvencia _______________________
                            // llama al metodo que  realiza solo los pasos hasta el calculo de Margen de solvencia e indicadores
                            this.procedimientosHastaMargenSolvencia(this.codigoEntidad);


                            // Todos los consolidados pasan a estado cerrado                  
                            object seguimientoConsolidacion = new
                            {
                                id_seguimiento_envio = (int)envio.id_seguimiento_envio,
                                id_consolidacion = (int)envio.id_consolidacion,
                                estado_cierre = estadoCierre.ToString()
                            };

                            // asigna  al seguimiento su estadoCierre
                            con.Execute(@"UPDATE seguimiento_envios SET id_consolidacion = @id_consolidacion, estado_cierre = @estado_cierre
                            WHERE id_seguimiento_envio = @id_seguimiento_envio", seguimientoConsolidacion);
                            con.Close();

                            listaRes.Add(seguimientoConsolidacion);
                        }
                    }
                    catch (Exception e)
                    {
                        procesoExitoso = false;
                        excepcion = e;
                        break;
                    }
                }

                string estado_cierre = procesoExitoso ? estadoCierre : "consolidado"; //  si no hubo error se cambi al estado Consolidado o cerrado , segun si fue normal o trimestral, caso contrario "consolidacion.estado" vuelve al valor que tenia originalmente
                this.estadoConsolidacion(id_consolidacion, estado_cierre, false);
            }

            con.Close();
            conSegSql.Close();
            return new
            {
                status = "success",
                calculo_solvencia = this.esMesSolvencia,
                errorEnCierre2 = !procesoExitoso,
                excepcion = excepcion,
                data = new
                {
                    lista = listaRes,
                    estadoCierre = procesoExitoso ? estadoCierre : "consolidado"
                }
            };
        }


        [HttpGet("ultimo")]
        public dynamic obtieneUltimaConsolidacion()
        {
            dynamic consolidacion = con.Query(@"SELECT * from consolidaciones
                                                    WHERE id_consolidacion = (select max(id_consolidacion) from consolidaciones) ").FirstOrDefault();
            con.Close();

            return new
            {
                status = "success",
                data = consolidacion
            };
        }



        [HttpGet("obtieneconsolidados")]
        public object obtieneConsolidados()
        {
            List<dynamic> consolidados = new List<dynamic>(con.Query(@"SELECT a.id_apertura, a.fecha_corte, e.""tNombre"" as entidad_nombre, s.* , c.nombre as desc_estado, ms.""mMSPrevisional"", pt.""mPatTecnico""
                                                                        FROM seguimiento_envios  s, 
                                                                            ""rstEmpresas"" e, 
                                                                            aperturas a, 
                                                                            constantes c, 
                                                                            ""tMSPrevisionales"" ms, 
                                                                            ""iftPatrimonioTecnico"" pt
                                                                        WHERE
                                                                        a.fecha_corte = (SELECT fecha_corte FROM aperturas WHERE activo) 
                                                                        AND s.cod_entidad = e.""cEmpresa"" AND a.id_apertura = s.id_apertura 
                                                                        AND s.estado = c.codigo AND c.dimension = 'estado_seguimiento' 
                                                                        AND ms.""cEmpresa"" = s.cod_entidad and ms.""fInformacion"" = to_char(a.fecha_corte,'YYYYMMDD')::int
                                                                        and pt.""cEmpresa"" =  s.cod_entidad and  pt.""cGestion"" = to_char(a.fecha_corte,'YYYY') and pt.""cMes"" = to_char(a.fecha_corte,'MM') 
                                                                        AND s.activo 
                                                                        AND s.estado_cierre = 'consolidado'						
                                                                        order by s.fecha_envio, s.cod_entidad  "));
            return new
            {
                status = "success",
                data = consolidados
            };
        }


        public int insertConsolidacion([FromBody]string observaciones)
        {
            // AperturaController aper = new AperturaController();
            dynamic aperturaActiva = AperturaController.getAperturaActiva();
            object consolidacion = new
            {
                fecha_consolidacion = DateTime.Now,
                fecha_corte = aperturaActiva.fecha_corte,
                estado = "",
                procesando = true,
                observaciones = observaciones,

                creado_por = 999,
                creado_en = DateTime.Now
            };
            int id = con.Query<int>(@"INSERT INTO consolidaciones( fecha_consolidacion,
                                                        estado, fecha_corte, procesando, observaciones, creado_por, creado_en)
                                    VALUES (@fecha_consolidacion,
                                    @estado, @fecha_corte,  @procesando, @observaciones,  @creado_por, @creado_en) RETURNING id_consolidacion", consolidacion).Single();
            con.Close();
            return id;
        }

        [HttpPut()]
        public void estadoConsolidacion(int idConsolidacion, string estadoCierre, bool procesando)
        {
            object consolidacion = new
            {
                id_consolidacion = idConsolidacion,
                estado = estadoCierre,
                procesando = procesando
            };
            con.Execute(@"UPDATE consolidaciones SET estado = @estado, procesando = @procesando WHERE id_consolidacion = @id_consolidacion ", consolidacion);
            con.Close();
        }

        [HttpGet("pruebaETL")]
        public object prueba_ETL()
        {
            List<object> misql = new List<object>(conSegSql.Query<object>("select * from iftPlanUnicoCuentas "));
            foreach (object item in misql)
            {
                con.Execute(@"insert into ""iftPlanUnicoCuentas"" 
                    values (@cTipoEntidad
                    ,@cCuentaFinanciera
                    ,@cMoneda
                    ,@cCuentaTecnica
                    ,@tDescripcion
                    ,@cNivel
                    ,@cCuentaPadre) ", item);


            }
            return "OKI";
        }


        [HttpGet("prueba")]
        public object prueba()
        {
            try
            {
                int a = 3;
                int b = 4;

                decimal h = b / (a - 3);

            }
            catch (Exception e)
            {
                return e;
            }


            return "OKI";
        }


    }
}