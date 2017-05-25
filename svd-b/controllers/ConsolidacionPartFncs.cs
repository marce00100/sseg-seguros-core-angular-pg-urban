using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Dapper;
using SVD.Models;
using System;

namespace SVD.Controllers
{
    public partial class ConsolidacionController
    {

        // Llena iftPartesProduccionSiniestros
        private void llenaIftPartesProduccionSiniestros(string codEntidad)
        {
            List<object> partesProduccionSins = new List<object>(con.Query<object>(@"
                                                                    SELECT * FROM
                                                                        (
                                                                            SELECT entidad, tipo_parte, departamento, sector, moneda,  to_char(fecha, 'YYYYMMDD')::int as fecha, modalidad, 
                                                                                ramo, poliza, 
                                                                                capital_asegurado, polizas_nuevas, polizas_renovadas, 
                                                                                prima_directa_m, prima_directa, capital_anulado, polizas_anuladas, 
                                                                                prima_anulada_m, prima_anulada, capital_asegurado_neto, polizas_netas, 
                                                                                prima_neta_m, prima_neta, prima_aceptada_reaseguro_m, prima_aceptada_reaseguro, 
                                                                                prima_cedida_reaseguro_m, prima_cedida_reaseguro, anulacion_prima_cedida_reaseguro_m, 
                                                                                anulacion_prima_cedida_reaseguro 

                                                                                , 0 as num_sins_denunciados, 0 as sins_denunciados, 0 as num_sins_liquidados
                                                                                , 0 as sins_liquidados_m, 0 as sins_liquidados, 0 as sins_reaseguro_aceptado_m
                                                                                , 0 as sins_reaseguro_aceptado, 0 as sins_reaseguro_cedido_m, 0 as sins_reaseguro_cedido
                                                                            FROM public.val_partes_produccion where fecha =  @fecha and entidad = @codEntidad

                                                                            UNION 
                                                                            SELECT entidad, tipo_parte, departamento, sector, moneda,  to_char(fecha, 'YYYYMMDD')::int as fecha, modalidad, 
                                                                                ramo, poliza, 
                                                                                0, 0, 0, 0,    0, 0, 0, 0,    0, 0, 0, 0,     0, 0, 0, 0,   0, 0, 0,
                                                                                num_sins_denunciados, sins_denunciados, num_sins_liquidados, 
                                                                                sins_liquidados_m, sins_liquidados, sins_reaseguro_aceptado_m, 
                                                                                sins_reaseguro_aceptado, sins_reaseguro_cedido_m, sins_reaseguro_cedido
                                                                            FROM public.val_partes_siniestros
                                                                            where fecha =  @fecha and entidad = @codEntidad
                                                                        ) as ps
                                                                        order by tipo_parte, modalidad   ",
                                                                     new { fecha = this.fechaCorte, codEntidad = this.codigoEntidad }));

            string queryDel = @"DELETE FROM ""iftPartesProduccionSiniestros"" 
                                WHERE ""cEmpresa"" = @codEntidad AND ""fInformacion"" = @fecha ";

            string queryIns = @"INSERT INTO  ""iftPartesProduccionSiniestros""  
                                VALUES ( @entidad, @tipo_parte, @departamento, @sector, @moneda, @fecha, @modalidad, 
                                        @ramo, @poliza, 
                                        
                                        @capital_asegurado, @polizas_nuevas, @polizas_renovadas, 
                                        @prima_directa_m, @prima_directa, @capital_anulado, @polizas_anuladas, 
                                        @prima_anulada_m, @prima_anulada, @capital_asegurado_neto, @polizas_netas, 
                                        @prima_neta_m, @prima_neta, @prima_aceptada_reaseguro_m, @prima_aceptada_reaseguro, 
                                        @prima_cedida_reaseguro_m, @prima_cedida_reaseguro, @anulacion_prima_cedida_reaseguro_m, 
                                        @anulacion_prima_cedida_reaseguro,

                                        @num_sins_denunciados, @sins_denunciados, @num_sins_liquidados,
                                        @sins_liquidados_m, @sins_liquidados, @sins_reaseguro_aceptado_m,
                                        @sins_reaseguro_aceptado, @sins_reaseguro_cedido_m, @sins_reaseguro_cedido
                                            ) ";

            con.Execute(queryDel, new { codEntidad = this.codigoEntidad, fecha = this.FechaCorteInt });
            //-- si es SQL
            if (this.poblarSQLSeguros)
                conSegSql.Execute(queryDel, new { codEntidad = this.codigoEntidad, fecha = this.FechaCorteInt });

            foreach (object reg in partesProduccionSins)
            {
                con.Execute(queryIns, reg);

                // si esta habilitada la opcion para SQL 
                if (this.poblarSQLSeguros)
                    conSegSql.Execute(queryIns, reg);
            }
            con.Close();
            conSegSql.Close();
        }



        // llena iftPartesProduccionRamos
        private void llenaIftPartesProduccionRamos(string codEntidad)
        {
            List<object> partesProduccionRamos = new List<object>(con.Query<object>(@"SELECT 
                                                                    entidad,  to_char(fecha, 'YYYYMMDD')::int as fecha, modalidad, ramo, primas_directas, anulado_primas_directas, 
                                                                    primas_netas_anulaciones, primas_aceptadas_reaseguro_nacional, 
                                                                    primas_aceptadas_reaseguro_extranjero, total_primas_aceptadas_reaseguro, 
                                                                    total_primas_netas, primas_cedidas_reaseguro_nacional, primas_cedidas_reaseguro_extranjero, 
                                                                    total_primas_cedidas, total_primas_netas_retenidas
                                                                    FROM val_partes_produccion_ramos 
                                                                    WHERE  fecha = @fecha AND entidad = @codEntidad",
                                                                     new { fecha = this.fechaCorte, codEntidad = this.codigoEntidad }));

            string queryDel = @"DELETE FROM ""iftPartesProduccionRamos"" 
                                WHERE ""cEmpresa"" = @codEntidad AND ""fInformacion"" = @fecha ";
            string queryIns = @"INSERT INTO  ""iftPartesProduccionRamos""  
                                VALUES (@entidad, @fecha, @modalidad, @ramo, @primas_directas, @anulado_primas_directas, 
                                @primas_netas_anulaciones, @primas_aceptadas_reaseguro_nacional, 
                                @primas_aceptadas_reaseguro_extranjero, @total_primas_aceptadas_reaseguro, 
                                @total_primas_netas, @primas_cedidas_reaseguro_nacional, @primas_cedidas_reaseguro_extranjero, 
                                @total_primas_cedidas, @total_primas_netas_retenidas ) ";

            con.Execute(queryDel, new { codEntidad = this.codigoEntidad, fecha = this.FechaCorteInt });
            //-- si es SQL
            if (this.poblarSQLSeguros)
                conSegSql.Execute(queryDel, new { codEntidad = this.codigoEntidad, fecha = this.FechaCorteInt });

            foreach (object reg in partesProduccionRamos)
            {
                con.Execute(queryIns, reg);

                // si esta habilitada la opcion para SQL 
                if (this.poblarSQLSeguros)
                    conSegSql.Execute(queryIns, reg);
            }
            con.Close();
            conSegSql.Close();
        }



        // llena PartesSieniestrosRamos
        private void llenaIftPartesSiniestrosRamos(string codEntidad)
        {
            List<object> partesProduccionRamos = new List<object>(con.Query<object>(@"SELECT 
                                                                    entidad,  to_char(fecha, 'YYYYMMDD')::int as fecha, modalidad, ramo, sins_directos, sins_reaseguro_aceptado_nacional, 
                                                                    sins_reaseguro_aceptado_extranjero, total_sins_reaseguro_aceptado, 
                                                                    sins_totales, sins_reembolsados_nacional, sins_reembolsados_extranjero, 
                                                                    total_sins_reembolsados, total_sins_retenidos
                                                                    FROM val_partes_siniestros_ramos 
                                                                    WHERE  fecha = @fecha AND entidad = @codEntidad",
                                                                     new { fecha = this.fechaCorte, codEntidad = this.codigoEntidad }));

            string queryDel = @"DELETE FROM ""iftPartesSiniestrosRamos"" 
                                WHERE ""cEmpresa"" = @codEntidad AND ""fInformacion"" = @fecha ";
            string queryIns = @"INSERT INTO  ""iftPartesSiniestrosRamos""  
                                VALUES (
                                @entidad, @fecha, @modalidad, @ramo, @sins_directos, @sins_reaseguro_aceptado_nacional, 
                                @sins_reaseguro_aceptado_extranjero, @total_sins_reaseguro_aceptado, 
                                @sins_totales, @sins_reembolsados_nacional, @sins_reembolsados_extranjero, 
                                @total_sins_reembolsados, @total_sins_retenidos ) ";

            con.Execute(queryDel, new { codEntidad = this.codigoEntidad, fecha = this.FechaCorteInt });
            //-- si es SQL
            if (this.poblarSQLSeguros)
                conSegSql.Execute(queryDel, new { codEntidad = this.codigoEntidad, fecha = this.FechaCorteInt });

            foreach (object reg in partesProduccionRamos)
            {
                con.Execute(queryIns, reg);

                // si esta habilitada la opcion para SQL 
                if (this.poblarSQLSeguros)
                    conSegSql.Execute(queryIns, reg);
            }
            con.Close();
            conSegSql.Close();
        }


        // llena PartesSieniestrosRamos
        private void llenaIftPartesSegurosLargoPlazo(string codEntidad)
        {
            List<object> partesSegurosLargoPlazo = new List<object>(con.Query<object>(@"SELECT 
                                                                    entidad, to_char(fecha, 'YYYYMMDD')::int as fecha, modalidad, ramo, capital_asegurado_total, capital_asegurado_cedido, 
                                                                    capital_asegurado_retenido, reserva_matematica_total, reserva_matematica_reasegurador, 
                                                                    reserva_matematica_retenida
                                                                    FROM val_partes_seguros_largo_plazo 
                                                                    WHERE  fecha = @fecha AND entidad = @codEntidad",
                                                                     new { fecha = this.fechaCorte, codEntidad = this.codigoEntidad }));

            string queryDel = @"DELETE FROM ""iftPartesSegurosLargoPlazo"" 
                                WHERE ""cEmpresa"" = @codEntidad AND ""fInformacion"" = @fecha ";
            string queryIns = @"INSERT INTO  ""iftPartesSegurosLargoPlazo""  
                                VALUES (
                                 @entidad, @fecha, @modalidad, @ramo, @capital_asegurado_total, @capital_asegurado_cedido, 
                                @capital_asegurado_retenido, @reserva_matematica_total, @reserva_matematica_reasegurador, 
                                @reserva_matematica_retenida ) ";

            con.Execute(queryDel, new { codEntidad = this.codigoEntidad, fecha = this.FechaCorteInt });
            //-- si es SQL
            if (this.poblarSQLSeguros)
                conSegSql.Execute(queryDel, new { codEntidad = this.codigoEntidad, fecha = this.FechaCorteInt });

            foreach (object reg in partesSegurosLargoPlazo)
            {
                con.Execute(queryIns, reg);

                // si esta habilitada la opcion para SQL 
                if (this.poblarSQLSeguros)
                    conSegSql.Execute(queryIns, reg);
            }
            con.Close();
            conSegSql.Close();
        }


        // Metodo que calcula los saldos resultantes en la tabla iftBalanceEstadoResultados tambien para sql
        private void llenaSaldosTotales(string codEntidad)
        {
            con.Execute(@"UPDATE val_balance_estado_resultados set
                            saldo_anterior = case 
                                    when ((substring(cuenta_financiera,1,1)in ('1','5','6')) 
                                    --or (c.cCuentaFinanciera='30702') 
                                    --or (c.cCuentaFinanciera not in ('10287','10288','10289','10389','10480','10489','10580','10689','10885'))
                                    )
                                    then (saldo_debe_anterior - saldo_haber_anterior)
                                    else (saldo_haber_anterior - saldo_debe_anterior)
                                    end ,
                            movimiento = case 
                                    when ((substring(cuenta_financiera,1,1)in ('1','5','6')) 
                                    --or (c.cCuentaFinanciera='30702') 
                                    --or (c.cCuentaFinanciera not in ('10287','10288','10289','10389','10480','10489','10580','10689','10885'))
                                    )
                                    then (movimiento_debe - movimiento_haber)
                                    else (movimiento_haber - movimiento_debe)
                                    end ,
                            saldo_actual = case 
                                    when ((substring(cuenta_financiera,1,1)in ('1','5','6')) 
                                    --or (c.cCuentaFinanciera='30702') 
                                    --or (c.cCuentaFinanciera not in ('10287','10288','10289','10389','10480','10489','10580','10689','10885'))
                                    )
                                    then (saldo_debe_actual - saldo_haber_actual)
                                    else (saldo_haber_actual - saldo_debe_actual)
                                    end
                            where entidad = @codigoEntidad and fecha = @fecha ", new { codigoEntidad = this.codigoEntidad, fecha = this.fechaCorte });
            con.Close();
        }

        public void consolidaInfoMensualAntesDeCierre(string codEntidad)
        {
            List<object> balanceERSinDuplicados = new List<object>(con.Query<object>(@"SELECT  entidad, to_char(fecha, 'YYYYMMDD')::int as fecha, cuenta_financiera , moneda,  cuenta_tecnica,
                                                                                sum(saldo_debe_anterior) as saldo_debe_anterior,
                                                                                sum(saldo_haber_anterior) as saldo_haber_anterior,
                                                                                sum(movimiento_debe) as movimiento_debe,
                                                                                sum(movimiento_haber) as movimiento_haber,
                                                                                sum(saldo_debe_actual) as saldo_debe_actual,
                                                                                sum(saldo_haber_actual) as saldo_haber_actual,
                                                                                sum(saldo_anterior) as saldo_anterior,
                                                                                sum(movimiento) as movimiento,
                                                                                sum(saldo_actual) as saldo_actual

                                                                                FROM val_balance_estado_resultados 
                                                                                    WHERE entidad = @cod_entidad AND fecha = @fechaCorte  
                                                                                GROUP BY
                                                                                entidad, fecha, cuenta_financiera, moneda,  cuenta_tecnica 
                                                                                ORDER BY cuenta_financiera, moneda,  cuenta_tecnica  ",
                                                                                new { cod_entidad = this.codigoEntidad, fechaCorte = this.fechaCorte })
                                                                                );

            // Realiza el vaciado y posteriormente el poblado de la tabla iftBalanceEstadoResultados  (en Postgres y tambien verifica si esta habilitada la opcion para la base de datos SQL)
            string queryDel = @"DELETE FROM ""iftBalanceEstadoResultados"" 
                                WHERE ""cEmpresa"" = @codEntidad AND ""fInformacion"" = @fecha ";
            con.Execute(queryDel, new { codEntidad = this.codigoEntidad, fecha = this.FechaCorteInt });
            //-- si es SQL
            if (this.poblarSQLSeguros)
                conSegSql.Execute(queryDel, new { codEntidad = this.codigoEntidad, fecha = this.FechaCorteInt });

            string queryIns = @"INSERT INTO ""iftBalanceEstadoResultados"" 
                                    VALUES ( @entidad, @fecha, @cuenta_financiera, @moneda, @cuenta_tecnica, 
                                            @saldo_debe_anterior, @saldo_haber_anterior, @movimiento_debe, @movimiento_haber, 
                                            @saldo_debe_actual, @saldo_haber_actual, @saldo_anterior, @movimiento, @saldo_actual) ";

            foreach (object item in balanceERSinDuplicados)
            {
                con.Execute(queryIns, item);

                // si esta habilitada la opcion para SQL 
                if (this.poblarSQLSeguros)
                    conSegSql.Execute(queryIns, item);
            }

            con.Close();
            conSegSql.Close();
        }


        public void consolidaProduccionCorredoras(string codEntidad)
        {
            // se obtienen los datos de la tabla VAL_Partes_produccion_corredoras de la entidad
            List<dynamic> partesProdCorredoras = new List<dynamic>(con.Query<dynamic>(@"SELECT  entidad, corredora, moneda, fecha, modalidad, ramo, poliza, produccion 
                                                                                            FROM val_partes_produccion_corredoras
                                                                                             WHERE entidad = @cod_entidad AND fecha = @fechaCorte  ",
                                                                                              new { cod_entidad = this.codigoEntidad, fechaCorte = this.fechaCorte }));

            string queryDel = @"DELETE FROM ""iftPartesProduccionCorredoras"" 
                                WHERE ""cEmpresa"" = @codEntidad AND ""fInformacion"" = @fecha ";
            con.Execute(queryDel, new { codEntidad = this.codigoEntidad, fecha = this.FechaCorteInt });

            //Opcion SQL                                
            if (this.poblarSQLSeguros)
                conSegSql.Execute(queryDel, new { codEntidad = this.codigoEntidad, fecha = this.FechaCorteInt });

            string queryIns = @"INSERT INTO ""iftPartesProduccionCorredoras"" 
                                    VALUES( @entidad, @corredora, @moneda, @fecha, @modalidad, @ramo, @poliza, @produccion )";

            foreach (dynamic item in partesProdCorredoras)
            {
                object regProdCorredoras = new
                {
                    entidad = item.entidad.ToString(),
                    corredora = item.corredora,
                    moneda = item.moneda,
                    fecha = Convert.ToInt32(item.fecha.ToString("yyyyMMdd")),
                    modalidad = item.modalidad,
                    ramo = item.ramo,
                    poliza = item.poliza,
                    produccion = item.produccion
                };
                con.Execute(queryIns, regProdCorredoras);

                // si  SQL 
                if (this.poblarSQLSeguros)
                    conSegSql.Execute(queryIns, regProdCorredoras);
            }
            con.Close();
            conSegSql.Close();
        }

        // Funcion que realiza las inserciones excepcionales declaradas en la Variable Globa excepcionesParaInsertar
        public void insertarComodines()
        {
            foreach (dynamic comodin in this.comodinesParaInsertar)
            {
                if (comodin.codigo_entidad == this.codigoEntidad)
                {
                    string qry = comodin.query.ToString();
                    con.Execute(qry, new { fecha_corte_int = this.FechaCorteInt });
                    if (this.poblarSQLSeguros)
                        conSegSql.Execute(qry, new { fecha_corte_int = this.FechaCorteInt });
                }
            }
            con.Close();
            conSegSql.Close();
        }

        public void insertaPatrimonioTecnicoVacio()
        {
            string queryDel = @"DELETE FROM ""iftPatrimonioTecnico""  
                                WHERE  ""cEmpresa"" = @codEntidad AND ""cGestion"" = @gestion AND ""cMes"" = @mes ";

            string queryPatrimonioTecnicoVacio = @"INSERT INTO ""iftPatrimonioTecnico""(
                                                ""cEmpresa"", ""cGestion"", ""cMes"", ""mPatTecnico"", ""cIndicador"")
                                        VALUES (@codEntidad, @gestion, @mes, 0, 62) ";

            object valores = new { codEntidad = this.codigoEntidad.ToString(), gestion = this.anoControl, mes = this.mesControl };

            con.Execute(queryDel, valores);
            con.Execute(queryPatrimonioTecnicoVacio, valores);
            if (this.poblarSQLSeguros)
            {
                conSegSql.Execute(queryDel, valores);
                conSegSql.Execute(queryPatrimonioTecnicoVacio, valores);
            }


            con.Close();
            conSegSql.Close();
        }

        public void insertaMargenSolvenciaPrevisionalesVacio()
        {
            string queryDel = @"DELETE FROM ""tMSPrevisionales""  
                                WHERE  ""cEmpresa"" = @codEntidad AND ""cGEstion"" = @gestion AND ""cMes"" = @mes ";

            string queryMargenSolvenciaVacio = @"INSERT INTO ""tMSPrevisionales""(
                                                    ""cEmpresa"", ""cGEstion"", ""cMes"", ""fInformacion"", ""mMSPrevisional"")
                                            VALUES (@codEntidad, @gestion, @mes, @fInformacion, 0)";
            object valores = new { codEntidad = this.codigoEntidad.ToString(), gestion = this.anoControl, mes = this.mesControl, fInformacion = this.FechaCorteInt };

            con.Execute(queryDel, valores);
            con.Execute(queryMargenSolvenciaVacio, valores);
            if (this.poblarSQLSeguros)
            {
                conSegSql.Execute(queryDel, valores);
                conSegSql.Execute(queryMargenSolvenciaVacio, valores);
            }

            con.Close();
            conSegSql.Close();
        }

        public void insertarPatrimonioTecnico(decimal mPatTecnico)
        {
            string queryPT = @"Update ""iftPatrimonioTecnico"" set  ""mPatTecnico"" = @mPatTecnico 
                                        WHERE ""cEmpresa"" = @codEntidad AND ""cGestion"" = @gestion AND ""cMes"" = @mes     ";
            Decimal mPT = mPatTecnico;

            object objPT = new
            {
                codEntidad = this.codigoEntidad,
                gestion = this.anoControl,
                mes = this.mesControl,
                mPatTecnico = mPT
            };
            // return objPT;
            con.Execute(queryPT, objPT);
            if (this.poblarSQLSeguros)
                conSegSql.Execute(queryPT, objPT);

            con.Close();
            conSegSql.Close();

        }

        public void insertarMSolvenciaPrevisional(decimal mMSPrevisional)
        {
            string queryMS = @"Update ""tMSPrevisionales"" set  ""mMSPrevisional"" = @mMSPrevisional 
                WHERE ""cEmpresa"" = @codEntidad AND ""cGEstion"" = @gestion AND ""cMes"" = @mes     ";
            Decimal MS = mMSPrevisional;
            object objMS = new
            {
                codEntidad = this.codigoEntidad,
                gestion = this.anoControl,
                mes = this.mesControl,
                mMSPrevisional = MS
            };
            con.Execute(queryMS, objMS);
            if (this.poblarSQLSeguros)
                conSegSql.Execute(queryMS, objMS);

            con.Close();
            conSegSql.Close();
        }

        public void procedimientosHastaMargenSolvencia(string cod_entidad)
        {
            if (this.poblarSQLSeguros)
            {

                // 1. ********************** afecta tablas iftIndicadores , iftVariable
                conSegSql.Execute("svdp_ifpIndicador_CalcularTodo",
                                new { fInformacion = this.FechaCorteInt, cEmpresa = this.codigoEntidad }, commandTimeout: 120, commandType: CommandType.StoredProcedure);
                this.paso = 1010;

                // 2. ******************************       No se realiza

                // 3. ***********************                      afecta tabla    iftCapitalMinimo
                conSegSql.Execute("svdp_ifpLLenaCapitalMinimo",
                new { fInformacion = this.FechaCorteInt, cEmpresa = this.codigoEntidad }, commandType: CommandType.StoredProcedure);
                this.paso = 1030;

                // 4a. ******************* Calcula margen de solvencia por trimestre  iftMargensolvencia
                if (this.esMesSolvencia)
                {
                    conSegSql.Execute("svdp_ifpCalculaMargenSolvencia_xTrimestre",
                                    new { fInformacion = this.FechaCorteInt, cEmpresa = this.codigoEntidad }, commandType: CommandType.StoredProcedure);
                }
                this.paso = 1041;
                // 4.b****************** llena indicadores de cumplimiento    inserta en tabla iftIndicadores
                conSegSql.Execute("svdp_ifpLlenaIndicadoresCumplimiento_a_iftIndicadores",
                new { fInformacion = this.FechaCorteInt, cEmpresa = this.codigoEntidad }, commandType: CommandType.StoredProcedure);
                this.paso = 1042;
            }
            con.Close();
            conSegSql.Close();

        }

        public void procedimientosCierreHastaSIG(string cod_entidad)
        {
            if (this.poblarSQLSeguros)
            {
                // 5. ****************** llena  tmpInfFinancieraWeb
                conSegSql.Execute("svdp_ifpCargaInformacionEstadisticaWeb",
                new { fInformacion = this.FechaCorteInt, cEmpresa = this.codigoEntidad }, commandType: CommandType.StoredProcedure);

                // 6. ****************** 
                conSegSql.Execute("svdp_ifpCargaIndicadoresParaCuadroSIG",
                new { cEmpresa = this.codigoEntidad }, commandTimeout: 120, commandType: CommandType.StoredProcedure);

                // 7 ******************
                conSegSql.Execute("[svdp_ifpCargaCuadroInformacionEstadisticaSIGxFecha]",
                new { fInformacion = this.FechaCorteInt, cEmpresa = this.codigoEntidad }, commandType: CommandType.StoredProcedure);

                this.procedimientosAlertaTemprana();
            }
            con.Close();
            conSegSql.Close();
        }



        // Procedimientos para Alerta Temprana deben ser enviados con todas las compañias o las que hayan enviado
        public void procedimientosAlertaTemprana()
        {
            if (this.poblarSQLSeguros)
            {
                // 1.
                conSegSql.Execute("atpAP_actualizartablas",
                    new { pfecha = this.FechaCorteInt }, commandType: CommandType.StoredProcedure);
                this.paso = 2010;

                // TODO descomentar cuando este conectada con la base de datos real SQL:

                // // 2.
                // conSegSql.Execute("atpIICalcularIndicadores",
                //     new { pfecha = this.FechaCorteInt }, commandTimeout: 90, commandType: CommandType.StoredProcedure);
                // this.paso = 2020;


                // // 3.
                // conSegSql.Execute("atpRG_CargarIndicadores",
                //     new { pfechaini = this.FechaCorteInt ,pfechafin = this.FechaCorteInt }, commandTimeout: 90, commandType: CommandType.StoredProcedure);
                // this.paso = 2030;

                // // 4.
                // conSegSql.Execute("atpCargaTablaHechosSISALT", commandType: CommandType.StoredProcedure);
                // this.paso = 2040;

                // // 5.
                // conSegSql.Execute("pSetIndiceVariacionBoletin",
                //     new { fecha = this.FechaCorteInt}, commandTimeout: 90, commandType: CommandType.StoredProcedure);
                // this.paso = 2050;

                // 6.
                conSegSql.Execute("pFillBER_BoletinEstadoResultados",
                    new { fecha = this.FechaCorteInt }, commandTimeout: 300, commandType: CommandType.StoredProcedure);
                this.paso = 2060;

                // 7.
                conSegSql.Execute("pFillBER_ModalidadRamo",
                    new { fecha = this.FechaCorteInt }, commandTimeout: 300, commandType: CommandType.StoredProcedure);
                this.paso = 2070;

                // 8.
                conSegSql.Execute("pFillDescripcionRamosATR", commandTimeout: 300, commandType: CommandType.StoredProcedure);
                this.paso = 2080;


                // 9.
                conSegSql.Execute("pFill_PPS_Zeros",
                    new { fecha = this.FechaCorteInt }, commandTimeout: 300, commandType: CommandType.StoredProcedure);
                this.paso = 2090;
            }
            con.Close();
            conSegSql.Close();

        }

        public void informacionFinancieraOLAP_PUC()
        {
            ////////////////////////////////  PUCS Niveles ////////// tiempo aprox: 20 s /////////////// 

            // conOLAPSql.Execute("truncate table iffBalanceEstadoResultados");

            conOLAPSql.Execute("delete from iffPUC_Nivel_5");
            conOLAPSql.Execute("delete from iffPUC_Nivel_4");
            conOLAPSql.Execute("delete from iffPUC_Nivel_3");
            conOLAPSql.Execute("delete from iffPUC_Nivel_2");
            conOLAPSql.Execute("delete from iffPUC_Nivel_1");

            ////////////////////////////////// iffPUC_Nivel_1 //////////////////////////////////////////

            List<dynamic> puc1 = new List<dynamic>(con.Query<dynamic>(@" SELECT
                                                                            TRIM(""cCuentaFinanciera"") || TRIM(""cCuentaTecnica"")  ""cCuenta"",
                                                                            '[' || TRIM (""cCuentaFinanciera"") || TRIM (""cCuentaTecnica"") || '] ' || MIN (""tDescripcion"") ""tDescripcion""
                                                                        FROM   ""iftPlanUnicoCuentas""
                                                                        WHERE length(RTRIM (""cCuentaFinanciera"")) = 1  AND ""cMoneda"" = '0'       AND length (RTRIM (""cCuentaTecnica""))  = 0
                                                                        GROUP BY TRIM(""cCuentaPadre""), TRIM(""cCuentaFinanciera""),  TRIM(""cCuentaTecnica"")
                                                                        order by ""cCuenta""            "));

            string queryIns = @" INSERT INTO ""iffPUC_Nivel_1"" 
                                  VALUES (  @cCuenta , @tDescripcion )";

            foreach (dynamic item in puc1)
            {
                object regPuc1 = new
                {
                    cCuenta = item.cCuenta.ToString(),
                    tDescripcion = item.tDescripcion.ToString()
                };
                conOLAPSql.Execute(queryIns, regPuc1);

            }
            con.Close();
            conOLAPSql.Close();

            ////////////////////////////////// iffPUC_Nivel_2 //////////////////////////////////////////
            List<dynamic> puc2 = new List<dynamic>(con.Query<dynamic>(@"SELECT        ""cCuentaPadre"", TRIM(""cCuentaFinanciera"") AS ""cCuenta"", '[' || TRIM(""cCuentaFinanciera"") || '] ' || MIN(""tDescripcion"") AS ""tDescripcion""
                                                                        FROM            ""iftPlanUnicoCuentas""
                                                                        WHERE        length(RTRIM(""cCuentaFinanciera"")) = 3 
                                                                            AND ""cMoneda"" = '0' AND length(RTRIM(""cCuentaTecnica"")) = 0
                                                                        GROUP BY (""cCuentaPadre""), (""cCuentaFinanciera"")
                                                                        order by ""cCuenta"" "));
            queryIns = @" INSERT INTO ""iffPUC_Nivel_2"" 
                                  VALUES ( @cCuentaPadre, @cCuenta , @tDescripcion )";

            foreach (dynamic item in puc2)
            {
                object regPuc2 = new
                {
                    cCuentaPadre = item.cCuentaPadre.ToString(),
                    cCuenta = item.cCuenta.ToString(),
                    tDescripcion = item.tDescripcion.ToString()
                };
                conOLAPSql.Execute(queryIns, regPuc2);
            }
            con.Close();
            conOLAPSql.Close();


            ////////////////////////////////// iffPUC_Nivel_3 //////////////////////////////////////////

            List<dynamic> puc3 = new List<dynamic>(con.Query<dynamic>(@"SELECT        TRIM(""cCuentaPadre"") as ""cCuentaPadre"", TRIM(""cCuentaFinanciera"") AS ""cCuenta"", '[' || TRIM(""cCuentaFinanciera"") || '] ' || MIN(""tDescripcion"") AS ""tDescripcion""
                                                                        FROM          ""iftPlanUnicoCuentas""
                                                                        WHERE        (LENGTH(RTRIM(""cCuentaFinanciera"")) = 5) AND (""cMoneda"" = '0') AND (length(RTRIM(""cCuentaTecnica"")) = 0)
                                                                        GROUP BY TRIM(""cCuentaPadre""), TRIM(""cCuentaFinanciera"")
                                                                        order by ""cCuenta""  "));

            queryIns = @" INSERT INTO ""iffPUC_Nivel_3"" 
                                  VALUES ( @cCuentaPadre, @cCuenta , @tDescripcion )";

            foreach (dynamic item in puc3)
            {
                object regPuc3 = new
                {
                    cCuentaPadre = item.cCuentaPadre.ToString(),
                    cCuenta = item.cCuenta.ToString(),
                    tDescripcion = item.tDescripcion.ToString()
                };
                conOLAPSql.Execute(queryIns, regPuc3);

            }
            con.Close();
            conOLAPSql.Close();

            // ////////////////////////////////// iffPUC_Nivel_4 //////////////////////////////////////////

            List<dynamic> puc4 = new List<dynamic>(con.Query<dynamic>(@"SELECT DISTINCT   TRIM(""cCuentaPadre"") as ""cCuentaPadre"", TRIM(""cCuentaFinanciera"") || TRIM(""cCuentaTecnica"") AS ""cCuenta"", '[' || TRIM(""cCuentaFinanciera"") || TRIM(""cCuentaTecnica"") || '] ' || MIN(""tDescripcion"") 
                                                                                                AS ""tDescripcion""
                                                                        FROM           ""iftPlanUnicoCuentas""
                                                                        WHERE        (LENGTH(TRIM(""cCuentaFinanciera"")) = 5) AND (""cMoneda"" = '0') AND (LENGTH(TRIM(""cCuentaTecnica"")) = 2)
                                                                        GROUP BY TRIM(""cCuentaPadre""), TRIM(""cCuentaFinanciera""), TRIM(""cCuentaTecnica"")
                                                                        order by ""cCuenta""  "));

            string queryIns4 = @" INSERT INTO ""iffPUC_Nivel_4"" 
                                  VALUES ( @cCuentaPadre, @cCuenta , @tDescripcion )";

            foreach (dynamic item in puc4)
            {
                object regPuc4 = new
                {
                    cCuentaPadre = item.cCuentaPadre.ToString(),
                    cCuenta = item.cCuenta.ToString(),
                    tDescripcion = item.tDescripcion.ToString()
                };
                conOLAPSql.Execute(queryIns4, regPuc4);

            }
            con.Close();
            conOLAPSql.Close();



            // ////////////////////////////////// iffPUC_Nivel_5 //////////////////////////////////////////

            List<dynamic> puc5 = new List<dynamic>(con.Query<dynamic>(@"  SELECT        TRIM(""cCuentaPadre"") as ""cCuentaPadre"", TRIM(""cCuentaFinanciera"") || TRIM(""cCuentaTecnica"") AS ""cCuenta"", '[' || TRIM(""cCuentaFinanciera"") || TRIM(""cCuentaTecnica"") || '] ' || MIN(""tDescripcion"") 
                                                                                                AS ""tDescripcion""
                                                                        FROM        ""iftPlanUnicoCuentas""
                                                                        WHERE        (LENGTH(TRIM(""cCuentaFinanciera"")) = 5) AND (""cMoneda"" = '0') AND (LENGTH(TRIM(""cCuentaTecnica"")) = 4)
                                                                        GROUP BY TRIM(""cCuentaPadre""), TRIM(""cCuentaFinanciera""), TRIM(""cCuentaTecnica"")
                                                                        order by ""cCuenta"" "));

            queryIns = @" INSERT INTO ""iffPUC_Nivel_5"" 
                                  VALUES ( @cCuentaPadre, @cCuenta , @tDescripcion )";

            foreach (dynamic item in puc5)
            {
                object regPuc5 = new
                {
                    cCuentaPadre = item.cCuentaPadre.ToString(),
                    cCuenta = item.cCuenta.ToString(),
                    tDescripcion = item.tDescripcion.ToString()
                };
                conOLAPSql.Execute(queryIns, regPuc5);

            }
            con.Close();
            conOLAPSql.Close();

        }

        public void informacionFinancieraOLAP_iffPartesProduccionsiniestros()
        {
            ///////////////////////////////////////  iffPartesProduccionSiniestros ////////tiempo aprox: 5 m /////////////////////////////////
            conOLAPSql.Execute("truncate table iffPartesProduccionSiniestros");

            List<dynamic> iftPartesProduccionSiniestros = new List<dynamic>(con.Query<dynamic>(@"
                                                                SELECT ""cEmpresa"",
                                                                ""cTipoParte"",
                                                                ""cDepartamento"",
                                                                ""cSector"",
                                                                ""cMoneda"",
                                                                SUBSTRING ( CAST ( ""fInformacion"" AS CHAR(8) ), 1, 4)::INT  as ""cGestion"",
                                                                SUBSTRING ( CAST ( ""fInformacion"" AS CHAR(8) ), 5, 2)::INT  AS ""cMes"" ,
                                                                ""cModalidad"",
                                                                case when ""cRamo"" in ('1','2','3','4','5','6','7','8','9') then '0' || rtrim(""cRamo"") else ""cRamo"" end as ""cRamo"",
                                                                ""cPoliza"",
                                                                ""mCapitalAsegurado"",
                                                                ""qPolizaNuevas"",
                                                                ""qPolizasRenovadas"",
                                                                ""mPrimaDirecta"",
                                                                ""mPrimaDirectaUFV"",
                                                                ""mCapitalAnulado"",
                                                                ""qPolizasAnuladas"",
                                                                ""mPrimaAnulada"",
                                                                ""mPrimaAnuladaUFV"",
                                                                ""mCapitalAseguradoNeto"",
                                                                ""qPolizasNetas"",
                                                                ""mPrimaNeta"",
                                                                ""mPrimaNetaUFV"",
                                                                ""mPrimaAceptadaReaseguro"",
                                                                ""mPrimaAceptadaReaseguroUFV"",
                                                                ""mPrimaCedidaReaseguro"",
                                                                ""mPrimaCedidaReaseguroUFV"",
                                                                ""mAnulacionPrimaCedidaReaseguro"",
                                                                ""mAnulacionPrimaCedidaReaseguroUFV"",
                                                                ""qSiniestrosDenunciados"",
                                                                ""mSiniestrosDenunciados"",
                                                                ""qSiniestrosLiquidados"",
                                                                ""mSiniestrosLiquidados"",
                                                                ""mSiniestrosLiquidadosUFV"",
                                                                ""mSiniestrosPagadosReaseguroAceptado"",
                                                                ""mSiniestrosPagadosReaseguroAceptadoUFV"",
                                                                ""mSiniestroReembolsadoReaseguroCedido"",
                                                                ""mSiniestroReembolsadoReaseguroCedidoUFV""

                                                                FROM ""iftPartesProduccionSiniestros"" "));

            string queryIns = @" INSERT INTO ""iffPartesProduccionSiniestros"" 
                                  VALUES (
                                            @cEmpresa, @cTipoParte, @cDepartamento, @cSector, @cMoneda, 
                                            @cGestion, @cMes, @cModalidad, @cRamo, @cPoliza, @mCapitalAsegurado, 
                                            @qPolizaNuevas, @qPolizasRenovadas, @mPrimaDirecta, @mPrimaDirectaUFV, 
                                            @mCapitalAnulado, @qPolizasAnuladas, @mPrimaAnulada, @mPrimaAnuladaUFV, 
                                            @mCapitalAseguradoNeto, @qPolizasNetas, @mPrimaNeta, @mPrimaNetaUFV, 
                                            @mPrimaAceptadaReaseguro, @mPrimaAceptadaReaseguroUFV, @mPrimaCedidaReaseguro, 
                                            @mPrimaCedidaReaseguroUFV, @mAnulacionPrimaCedidaReaseguro, 
                                            @mAnulacionPrimaCedidaReaseguroUFV, 
                                            @qSiniestrosDenunciados, 
                                            @mSiniestrosDenunciados, @qSiniestrosLiquidados, @mSiniestrosLiquidados, 
                                            @mSiniestrosLiquidadosUFV, @mSiniestrosPagadosReaseguroAceptado, 
                                            @mSiniestrosPagadosReaseguroAceptadoUFV, @mSiniestroReembolsadoReaseguroCedido, 
                                            @mSiniestroReembolsadoReaseguroCedidoUFV) ";

            foreach (dynamic item in iftPartesProduccionSiniestros)
            {
                object reg = new
                {
                    cEmpresa = item.cEmpresa,
                    cTipoParte = item.cTipoParte,
                    cDepartamento = item.cDepartamento,
                    cSector = item.cSector,
                    cMoneda = item.cMoneda,
                    cGestion = item.cGestion,
                    cMes = item.cMes,
                    cModalidad = item.cModalidad,
                    cRamo = item.cRamo,
                    cPoliza = item.cPoliza,
                    mCapitalAsegurado = item.mCapitalAsegurado,
                    qPolizaNuevas = item.qPolizaNuevas,
                    qPolizasRenovadas = item.qPolizasRenovadas,
                    mPrimaDirecta = item.mPrimaDirecta,
                    mPrimaDirectaUFV = item.mPrimaDirectaUFV,
                    mCapitalAnulado = item.mCapitalAnulado,
                    qPolizasAnuladas = item.qPolizasAnuladas,
                    mPrimaAnulada = item.mPrimaAnulada,
                    mPrimaAnuladaUFV = item.mPrimaAnuladaUFV,
                    mCapitalAseguradoNeto = item.mCapitalAseguradoNeto,
                    qPolizasNetas = item.qPolizasNetas,
                    mPrimaNeta = item.mPrimaNeta,
                    mPrimaNetaUFV = item.mPrimaNetaUFV,
                    mPrimaAceptadaReaseguro = item.mPrimaAceptadaReaseguro,
                    mPrimaAceptadaReaseguroUFV = item.mPrimaAceptadaReaseguroUFV,
                    mPrimaCedidaReaseguro = item.mPrimaCedidaReaseguro,
                    mPrimaCedidaReaseguroUFV = item.mPrimaCedidaReaseguroUFV,
                    mAnulacionPrimaCedidaReaseguro = item.mAnulacionPrimaCedidaReaseguro,
                    mAnulacionPrimaCedidaReaseguroUFV = item.mAnulacionPrimaCedidaReaseguroUFV,
                    qSiniestrosDenunciados = item.qSiniestrosDenunciados,
                    mSiniestrosDenunciados = item.mSiniestrosDenunciados,
                    qSiniestrosLiquidados = item.qSiniestrosLiquidados,
                    mSiniestrosLiquidados = item.mSiniestrosLiquidados,
                    mSiniestrosLiquidadosUFV = item.mSiniestrosLiquidadosUFV,
                    mSiniestrosPagadosReaseguroAceptado = item.mSiniestrosPagadosReaseguroAceptado,
                    mSiniestrosPagadosReaseguroAceptadoUFV = item.mSiniestrosPagadosReaseguroAceptadoUFV,
                    mSiniestroReembolsadoReaseguroCedido = item.mSiniestroReembolsadoReaseguroCedido,
                    mSiniestroReembolsadoReaseguroCedidoUFV = item.mSiniestroReembolsadoReaseguroCedidoUFV
                };
                conOLAPSql.Execute(queryIns, reg);

            }
            con.Close();
            conOLAPSql.Close();

        }


        public void informacionFinancieraOLAP_iffIndicadores()
        {
            ///////////////////////////////////////  iffIndicadores ///////////////tiempo aprox: 3 m //////////////////////////
            conOLAPSql.Execute("truncate table iffIndicadores");

            List<dynamic> iftIndicadores = new List<dynamic>(conSegSql.Query<dynamic>(@"
                                                                SELECT ""cEmpresa"", 
                                                                CAST ( SUBSTRING ( CAST ( fInformacion AS CHAR(8) ), 1, 4) AS INT ) as ""cGestion"",
                                                                CAST ( SUBSTRING ( CAST ( fInformacion AS CHAR(8) ), 5, 2) AS INT ) as ""cMes"",
                                                                ""cIndicador"", 
                                                                ""mIndicador""
                                                                FROM 
                                                                ""iftIndicadores""
                                                                WHERE (""cError"" = 0) "));

            string queryIns = @" INSERT INTO ""iffIndicadores"" 
                                  VALUES (
                                            @cEmpresa,
                                            @cGestion, @cMes, 
                                            @cIndicador,
                                            @mIndicador ) ";

            foreach (dynamic item in iftIndicadores)
            {
                object reg = new
                {
                    cEmpresa = item.cEmpresa,
                    cGestion = item.cGestion,
                    cMes = item.cMes,
                    cIndicador = item.cIndicador,
                    mIndicador = item.mIndicador
                };
                conOLAPSql.Execute(queryIns, reg);

            }
            conSegSql.Close();
            conOLAPSql.Close();

        }

        public void informacionFinancieraOLAP_iffBalanceEstadoResultados_T()
        {
            ///////////////////////////////////////  iffBalanceEstadoResultados_T /////// tiempo aprox: 23 m //////////////////////////////////
            conOLAPSql.Execute("truncate table iffBalanceEstadoResultados_T");

            List<dynamic> iffBER_T = new List<dynamic>(con.Query<dynamic>(@"
                                                                        SELECT	SUBSTRING(CAST(""fInformacion"" AS CHAR(8)), 1, 4) AS ""cGestion"", 	
                                                                        SUBSTRING(CAST(""fInformacion"" AS CHAR(8)), 5, 2) AS ""cMes"", 
                                                                        ""cEmpresa"", 
                                                                        RTRIM(""cCuentaFinanciera"") || RTRIM(""cCuentaTecnica"") AS ""cCuenta"", 
                                                                        ""cMoneda"", 
                                                                        ""mSaldoAnterior"", 
                                                                        ""mMovimiento"", 
                                                                        ""mSaldoActual""
                                                                    FROM    ""iftBalanceEstadoResultados""
                                                                    WHERE	LENGTH(RTRIM(""cCuentaFinanciera"") || RTRIM(""cCuentaTecnica"")) = 9 "));

            string queryIns = @" INSERT INTO ""iffBalanceEstadoResultados_T"" 
                                  VALUES (                                            
                                            @cGestion, @cMes, 
                                            @cEmpresa,
                                            @cCuenta,
                                            @cMoneda,
                                            @mSaldoAnterior,
                                            @mMovimiento,
                                            @mSaldoActual
                                             ) ";

            foreach (dynamic item in iffBER_T)
            {
                object reg = new
                {
                    cGestion = item.cGestion,
                    cMes = item.cMes,
                    cEmpresa = item.cEmpresa,
                    cCuenta = item.cCuenta,
                    cMoneda = item.cMoneda,
                    mSaldoAnterior = item.mSaldoAnterior,
                    mMovimiento = item.mMovimiento,
                    mSaldoActual = item.mSaldoActual
                };
                conOLAPSql.Execute(queryIns, reg);

            }
            con.Close();
            conOLAPSql.Close();
        }


        public void informacionFinancieraOLAP_iftBalanceEstadoResultadosOLAP_T()
        {
            ///////////////////////////////////////  iftBalanceEstadoResultadosOLAP_T /////// tiempo aprox: 43 m //////////////////////////////////
            conOLAPSql.Execute("truncate table iftBalanceEstadoResultadosOLAP_T");

            List<dynamic> iftBerOLAP_T = new List<dynamic>(con.Query<dynamic>(@"
                                                                        SELECT	SUBSTRING(CAST(""fInformacion"" AS CHAR(8)), 1, 4) AS ""cGestion"", 	
                                                                        SUBSTRING(CAST(""fInformacion"" AS CHAR(8)), 5, 2) AS ""cMes"", 
                                                                        ""cEmpresa"", 
                                                                        RTRIM(""cCuentaFinanciera"") || RTRIM(""cCuentaTecnica"") AS ""cCuenta"", 
                                                                        ""cMoneda"", 
                                                                        ""mSaldoDebeAnterior"",
                                                                        ""mSaldoHaberAnterior"",
                                                                        ""mMovimientoDebe"",
                                                                        ""mMovimientoHaber"",
                                                                        ""mSaldoDebeActual"",
                                                                        ""mSaldoHaberActual"",
                                                                        ""mSaldoAnterior"", 
                                                                        ""mMovimiento"", 
                                                                        ""mSaldoActual""
                                                                    FROM    ""iftBalanceEstadoResultados""
                                                                    WHERE	LENGTH(RTRIM(""cCuentaFinanciera"") || RTRIM(""cCuentaTecnica"")) = 9 "));
            con.Close();
            string queryIns = @" INSERT INTO ""iftBalanceEstadoResultadosOLAP_T"" 
                                  VALUES (                                            
                                            @cGestion, @cMes, 
                                            @cEmpresa,
                                            @cCuenta,
                                            @cMoneda,
                                            @mSaldoDebeAnterior,
                                            @mSaldoHaberAnterior,
                                            @mMovimientoDebe,
                                            @mMovimientoHaber,
                                            @mSaldoDebeActual,
                                            @mSaldoHaberActual,                                            
                                            @mSaldoAnterior,
                                            @mMovimiento,
                                            @mSaldoActual
                                             ) ";

            foreach (dynamic item in iftBerOLAP_T)
            {
                object reg = new
                {
                    cGestion = item.cGestion,
                    cMes = item.cMes,
                    cEmpresa = item.cEmpresa,
                    cCuenta = item.cCuenta,
                    cMoneda = item.cMoneda,
                    mSaldoDebeAnterior = item.mSaldoDebeAnterior,
                    mSaldoHaberAnterior = item.mSaldoHaberAnterior,
                    mMovimientoDebe = item.mMovimientoDebe,
                    mMovimientoHaber = item.mMovimientoHaber,
                    mSaldoDebeActual = item.mSaldoDebeActual,
                    mSaldoHaberActual = item.mSaldoHaberActual,
                    mSaldoAnterior = item.mSaldoAnterior,
                    mMovimiento = item.mMovimiento,
                    mSaldoActual = item.mSaldoActual
                };
                conOLAPSql.Execute(queryIns, reg);
                conOLAPSql.Close();
            }
            con.Close();
            conOLAPSql.Close(); 
        }


    }
}