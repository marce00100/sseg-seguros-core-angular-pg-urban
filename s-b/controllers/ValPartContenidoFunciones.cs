using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Dapper;
using SVD.Models;
using System;
using Newtonsoft.Json;
using System.Dynamic;



namespace SVD.Controllers
{
    public partial class ValidacionesController
    {


        //////////////////////////  0 a Ecuacion Contable
        private dynamic ecuacionContable()
        {
            List<dynamic> datosValidacion = new List<dynamic>(con.Query<dynamic>(@"                                                                                                                                                
                                                            select x1.entidad,  x1.c1 activo, x2.c2 pasivo,x3.c3 patrimonio,x4.c4 ingresos,x5.c5 egresos, (c1+c5) as suma1, (c2+c3+c4) as suma2,((c1+c5)-(c2+c3+c4))as diferencia  
                                                            from 
                                                                    (select entidad, abs( saldo_haber_actual - saldo_debe_actual) as c1 
                                                                    from val_balance_estado_resultados 
                                                                        WHERE fecha = @fecha
                                                                        and entidad = @codEntidad and cuenta_financiera = '1'
                                                                        and cuenta_tecnica = ''
                                                                        and moneda = '0'
                                                                        ) x1,
                                                                        (select abs( saldo_haber_actual - saldo_debe_actual) as c2 from val_balance_estado_resultados 
                                                                        WHERE fecha =  @fecha
                                                                        and entidad = @codEntidad and cuenta_financiera = '2'
                                                                        and cuenta_tecnica =  ''
                                                                        and moneda = '0'
                                                                        ) x2,
                                                                        (select abs( saldo_haber_actual - saldo_debe_actual) as  c3 from val_balance_estado_resultados 
                                                                        WHERE fecha =  @fecha
                                                                        and entidad = @codEntidad and cuenta_financiera = '3'
                                                                        and cuenta_tecnica = ''
                                                                        and moneda =  '0'
                                                                        ) x3,
                                                                        ( select abs( saldo_haber_actual - saldo_debe_actual) as c4 from val_balance_estado_resultados 
                                                                        WHERE fecha = @fecha
                                                                        and entidad = @codEntidad and cuenta_financiera = '4'
                                                                        and cuenta_tecnica = ''
                                                                        and moneda =  '0'
                                                                        ) x4, 
                                                                        (  select abs( saldo_haber_actual - saldo_debe_actual) as c5 from val_balance_estado_resultados 
                                                                        WHERE fecha = @fecha
                                                                        and entidad = @codEntidad and cuenta_financiera ='5'
                                                                        and cuenta_tecnica = ''
                                                                        and moneda = '0') x5 
                                                                        where  abs((c1+c5)-(c2+c3+c4)) > 0",
                                                            new { codEntidad = this.codigoEntidad, fecha = this.fechaCorte }));

            con.Close();
            dynamic val = new ExpandoObject();
            val.valido = datosValidacion.Count == 0;
            Error error = err("123", "", "", 0, 0); // error generico : variable para obtener los nombres y titulos y descripcoion  del tipo de error 
            val.titulo = "Validación para la Ecuacion Contable";
            val.validacion = error.nombre_error; // "Saldos ecuacion contable";
            val.data = new List<dynamic>();
            val.estadoValidez = val.valido ? 4 : error.estadoValidez; //segun el tipo de error V o EE  
            val.descripcionError = val.valido ? "sin error" : error.error + " " + error.desc_error;
            this.errores.Add(error);
            this.listaValCont.Add(val);
            return val;



        }

        //////////////////////////////////////////////////////// 0b CUENTAS que no estan en PUC ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private dynamic cuentasQueNoEstanPUC()
        {
            dynamic entidad = EntidadesController.obtenerEntidadDeCodigo(this.codigoEntidad);
            List<dynamic> datosValidacion = new List<dynamic>(con.Query<dynamic>(@"SELECT 
                                                                    entidad, cuenta_financiera, moneda, cuenta_tecnica, saldo_debe_anterior, saldo_haber_anterior, 
                                                                    movimiento_haber, movimiento_debe, saldo_debe_actual, saldo_haber_actual, index_fila
                                                                    FROM 
                                                                    (
                                                                    SELECT ber.* , puc.""cCuentaFinanciera"" as cuenta
                                                                    FROM val_balance_estado_resultados  ber LEFT JOIN ""iftPlanUnicoCuentas"" puc 
                                                                    on  ( trim(ber.cuenta_financiera) = trim(puc.""cCuentaFinanciera"") 
                                                                    and trim(ber.cuenta_tecnica) = trim(puc.""cCuentaTecnica"") and trim(ber.moneda) = trim(puc.""cMoneda"") 
                                                                    and puc.""cTipoEntidad"" = @tipoEntidad    ) 
                                                                    where  fecha = @fecha and entidad = @codEntidad ) as rel
                                                                WHERE cuenta is null 
                                                                order by cuenta_financiera ", new { codEntidad = this.codigoEntidad, fecha = this.fechaCorte, tipoEntidad = entidad.cod_tipo_entidad }));

            con.Close();
            dynamic val = new ExpandoObject();
            val.valido = datosValidacion.Count == 0;
            Error error = err("124", "", "", 0, 0); // error generico : variable para obtener los nombres y titulos y descripcoion  del tipo de error 
            val.titulo = "Cuentas que no estan en el PUC";
            val.validacion = error.nombre_error; // "Cuentas que no estan en el PUC";
            val.data = datosValidacion;
            val.estadoValidez = val.valido ? 4 : error.estadoValidez; //segun el tipo de error V o EE  
            val.descripcionError = val.valido ? "sin error" : error.error + " " + error.desc_error;

            foreach (dynamic item in datosValidacion)
            {
                // EE-124 Cuentas que no estan en el PUC"
                Error errorItem = err("124", codArchivo(item.cuenta_financiera), "", (int)item.index_fila, 0);
                item.filaError = errorItem.error + ", arch: " + errorItem.archivo + ", fila: " + errorItem.fila;
                this.errores.Add(errorItem);
            }
            this.listaValCont.Add(val);
            return val;
        }

        //////////////////////////////////////////////////////// 1 . SALDOS INICIALES Y FINALES ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private dynamic saldosInicialesFinales()
        {
            DateTime fechaFinMesAnterior = this.fechaCorte.AddDays(-this.fechaCorte.Day);
            int nFechaFinMesAnterior = fechaFinMesAnterior.Year * 10000 + fechaFinMesAnterior.Month * 100 + fechaFinMesAnterior.Day;
            List<dynamic> mesAnteriorDatos = new List<dynamic>(con.Query<dynamic>(@"
                                                                     SELECT  b0.""cEmpresa"", b0.""cCuentaFinanciera"", b0.""cMoneda"", b0.""cCuentaTecnica"", b0.""mSaldoActual""   
                                                                        FROM ""iftBalanceEstadoResultados"" b0 
                                                                        WHERE b0.""cEmpresa"" = @cod_entidad AND b0.""fInformacion"" = @fecha_mes_anterior
                                                                         ", new { cod_entidad = this.codigoEntidad, fecha_corte = this.fechaCorte, fecha_mes_anterior = nFechaFinMesAnterior }));
            if (mesAnteriorDatos.Count > 0)
            {
                List<dynamic> saldosDiferentes = new List<dynamic>(con.Query<dynamic>(
                                                            @"SELECT m1.entidad, m1.cuenta_financiera, m1.moneda, m1.cuenta_tecnica, 
                                                                m0.""mSaldoActual"" AS saldo_final_mes_anterior,  m1.saldo_inicial_mes_actual, (coalesce(m0.""mSaldoActual"",0) - m1.saldo_inicial_mes_actual) AS diferencia
                                                                ,m1.index_fila
                                                                FROM 
                                                                    (
                                                                        SELECT b1.entidad, b1.cuenta_financiera, b1.moneda, b1.cuenta_tecnica, (CASE 
                                                                        WHEN (substring(b1.cuenta_financiera,1,1)in ('1','5','6') /* or (b1.cuenta_financiera='30702') or (b1.cuenta_financiera not in ('10287','10288','10289','10389','10480','10489','10580','10689','10885'))*/ )
                                                                        THEN (b1.saldo_debe_anterior -b1.saldo_haber_anterior)
                                                                        ELSE (b1.saldo_haber_anterior -b1.saldo_debe_anterior) 
                                                                        END
                                                                        ) as saldo_inicial_mes_actual	
                                                                        ,b1.index_fila 		
                                                                    FROM val_balance_estado_resultados b1  WHERE b1.entidad = @cod_entidad AND  b1.fecha = @fecha_corte) AS m1
                                                                LEFT JOIN 
                                                                    (
                                                                        SELECT  b0.""cEmpresa"", b0.""cCuentaFinanciera"", b0.""cMoneda"", b0.""cCuentaTecnica"", b0.""mSaldoActual""   
                                                                        FROM ""iftBalanceEstadoResultados"" b0 
                                                                        WHERE b0.""cEmpresa"" = @cod_entidad AND b0.""fInformacion"" = @fecha_mes_anterior
                                                                        ) AS m0
                                                                ON m1.cuenta_financiera || m1.moneda || m1.cuenta_tecnica =  m0.""cCuentaFinanciera"" || m0.""cMoneda"" || m0.""cCuentaTecnica""
                                                                WHERE (coalesce(m0.""mSaldoActual"",0) - m1.saldo_inicial_mes_actual) <> 0 
                                                                ORDER BY m1.index_fila", new { cod_entidad = this.codigoEntidad, fecha_corte = this.fechaCorte, fecha_mes_anterior = nFechaFinMesAnterior }));
                con.Close();
                dynamic val = new ExpandoObject();
                val.valido = saldosDiferentes.Count == 0;
                Error error = err("101", "", "", 0, 0); // error generico : variable para obtener los nombres y titulos y descripcoion  del tipo de error 
                val.titulo = "Validación de saldos iniciales";
                val.validacion = error.nombre_error; // "Saldos iniciales con finales";
                val.data = saldosDiferentes;
                val.estadoValidez = val.valido ? 4 : error.estadoValidez; //segun el tipo de error V o EE  
                val.descripcionError = val.valido ? "sin error" : error.error + " " + error.desc_error;

                foreach (dynamic item in saldosDiferentes)
                {
                    // EE-101 "Error con saldos iniciales"
                    Error errorItem = err("101", codArchivo(item.cuenta_financiera), "", (int)item.index_fila, 0);
                    item.filaError = errorItem.error + ", arch: " + errorItem.archivo + ", fila: " + errorItem.fila;
                    this.errores.Add(errorItem);
                }
                this.listaValCont.Add(val);
                return val;
            }
            else
            {
                dynamic val = new ExpandoObject();
                val.valido = false;
                Error error = err("101", "", "", 0, 0); // error generico : variable para obtener los nombres y titulos y descripcoion  del tipo de error 
                error.desc_error = "No Existen Saldos del Mes Anterior";
                val.titulo = "Validación de saldos iniciales";
                val.validacion = error.nombre_error; // "Saldos iniciales con finales";
                val.data = new List<dynamic>();
                val.estadoValidez = val.valido ? 4 : error.estadoValidez; //segun el tipo de error V o EE  
                val.descripcionError = error.error + " " + error.desc_error;
                this.errores.Add(error);
                this.listaValCont.Add(val);
                return val;
            }


        }
        ////////////////////////////////////////////////////// 2.1  SUMA PARTES PRODUCCION RAMOS TRIMESTRAL  Separado en 5 partes   /////////////////////////////////////////////////////////////////////////////////

        private dynamic partesProduccionRamos_1()
        {
            List<dynamic> datosValidacion = new List<dynamic>(con.Query<dynamic>(@"
                                           SELECT entidad, modalidad, ramo, primas_netas_anulaciones as primas_netas, 
                                                 primas_directas,  anulado_primas_directas as primas_anuladas,  
                                                 primas_netas_anulaciones - (primas_directas - anulado_primas_directas ) as diferencia, index_fila
                                            FROM val_partes_produccion_ramos 
                                            WHERE ( primas_netas_anulaciones <> primas_directas - anulado_primas_directas )
                                                AND entidad = @codigoEntidad and fecha= @fechaCorte",
                                                           new { codigoEntidad = this.codigoEntidad, fechaCorte = this.fechaCorte }));
            con.Close();
            dynamic val = new ExpandoObject();
            val.titulo = "Sumas en Partes Produccion por Ramos (TRIMESTRAL)";
            val.valido = datosValidacion.Count == 0;
            Error error = err("106", "", "", 0, 0); //  error genérico para este tipo de validacion
            val.validacion = error.nombre_error; // "Sumas en Parte Produccion Mensuales, Capital asegurado neto = capital asegurado - capital anulado";
            val.estadoValidez = val.valido ? 4 : error.estadoValidez;
            val.data = datosValidacion;
            val.descripcionError = val.valido ? "sin error" : error.error + " " + error.desc_error;
            foreach (dynamic item in datosValidacion)
            {
                // EE-106 "Suma partes produccion por ramo: primas netas
                Error errorItem = err("106", "F", "", (int)item.index_fila, 0);
                item.filaError = errorItem.error + ", arch: " + errorItem.archivo + ", fila: " + errorItem.fila;
                this.errores.Add(errorItem);
            }
            this.listaValCont.Add(val);
            return val;
        }
        private dynamic partesProduccionRamos_2()
        {
            List<dynamic> datosValidacion = new List<dynamic>(con.Query<dynamic>(@"
                                           SELECT entidad, modalidad, ramo, total_primas_aceptadas_reaseguro as total_primas_aceptada_reas, 
                                                primas_aceptadas_reaseguro_nacional as primas_aceptadas_reas_nal,  
                                                primas_aceptadas_reaseguro_extranjero as primas_aceptadas_reas_ext,  
                                                total_primas_aceptadas_reaseguro - (primas_aceptadas_reaseguro_nacional + primas_aceptadas_reaseguro_extranjero) as diferencia, index_fila
                                                FROM val_partes_produccion_ramos 
                                                WHERE ( total_primas_aceptadas_reaseguro <> primas_aceptadas_reaseguro_nacional + primas_aceptadas_reaseguro_extranjero )
                                                AND entidad = @codigoEntidad and fecha= @fechaCorte",
                                                           new { codigoEntidad = this.codigoEntidad, fechaCorte = this.fechaCorte }));
            con.Close();
            dynamic val = new ExpandoObject();
            val.valido = datosValidacion.Count == 0;
            Error error = err("107", "", "", 0, 0); //  error genérico para este tipo de validacion
            val.validacion = error.nombre_error; // "Sumas en Parte Produccion Mensuales, Capital asegurado neto = capital asegurado - capital anulado";
            val.estadoValidez = val.valido ? 4 : error.estadoValidez;
            val.data = datosValidacion;
            val.descripcionError = val.valido ? "sin error" : error.error + " " + error.desc_error;
            foreach (dynamic item in datosValidacion)
            {
                // EE-107 "Suma partes produccion por ramo: total_primas_aceptada_reas
                Error errorItem = err("107", "F", "", (int)item.index_fila, 0);
                item.filaError = errorItem.error + ", arch: " + errorItem.archivo + ", fila: " + errorItem.fila;
                this.errores.Add(errorItem);
            }
            this.listaValCont.Add(val);
            return val;
        }
        private dynamic partesProduccionRamos_3()
        {
            List<dynamic> datosValidacion = new List<dynamic>(con.Query<dynamic>(@"
                                           SELECT entidad, modalidad, ramo, total_primas_netas, 
                                            primas_netas_anulaciones as primas_netas_anul,  total_primas_aceptadas_reaseguro as total_primas_aceptadas_reas,  
                                            total_primas_netas - (primas_netas_anulaciones + total_primas_aceptadas_reaseguro ) as diferencia, index_fila
                                            FROM val_partes_produccion_ramos 
                                            WHERE ( total_primas_netas <> primas_netas_anulaciones + total_primas_aceptadas_reaseguro )
                                                AND entidad = @codigoEntidad and fecha= @fechaCorte",
                                                           new { codigoEntidad = this.codigoEntidad, fechaCorte = this.fechaCorte }));
            con.Close();
            dynamic val = new ExpandoObject();
            val.valido = datosValidacion.Count == 0;
            Error error = err("108", "", "", 0, 0); //  error genérico para este tipo de validacion
            val.validacion = error.nombre_error; // "Sumas en Parte Produccion Mensuales, Capital asegurado neto = capital asegurado - capital anulado";
            val.estadoValidez = val.valido ? 4 : error.estadoValidez;
            val.data = datosValidacion;
            val.descripcionError = val.valido ? "sin error" : error.error + " " + error.desc_error;
            foreach (dynamic item in datosValidacion)
            {
                // EE-108 "Suma partes produccion por ramo: total_primas_netas
                Error errorItem = err("108", "F", "", (int)item.index_fila, 0);
                item.filaError = errorItem.error + ", arch: " + errorItem.archivo + ", fila: " + errorItem.fila;
                this.errores.Add(errorItem);
            }
            this.listaValCont.Add(val);
            return val;
        }
        private dynamic partesProduccionRamos_4()
        {
            List<dynamic> datosValidacion = new List<dynamic>(con.Query<dynamic>(@"
                                           SELECT entidad, modalidad, ramo, total_primas_cedidas, 
                                                primas_cedidas_reaseguro_nacional as primas_cedidas_reas_nal,  primas_cedidas_reaseguro_extranjero as primas_cedidas_reas_ext,  
                                                total_primas_cedidas - (primas_cedidas_reaseguro_nacional + primas_cedidas_reaseguro_extranjero ) as diferencia, index_fila
                                                FROM val_partes_produccion_ramos 
                                                WHERE ( total_primas_cedidas <> primas_cedidas_reaseguro_nacional + primas_cedidas_reaseguro_extranjero )
                                                AND entidad = @codigoEntidad and fecha= @fechaCorte",
                                                           new { codigoEntidad = this.codigoEntidad, fechaCorte = this.fechaCorte }));
            con.Close();
            dynamic val = new ExpandoObject();
            val.valido = datosValidacion.Count == 0;
            Error error = err("109", "", "", 0, 0); //  error genérico para este tipo de validacion
            val.validacion = error.nombre_error; // "Sumas en Parte Produccion Mensuales, Capital asegurado neto = capital asegurado - capital anulado";
            val.estadoValidez = val.valido ? 4 : error.estadoValidez;
            val.data = datosValidacion;
            val.descripcionError = val.valido ? "sin error" : error.error + " " + error.desc_error;
            foreach (dynamic item in datosValidacion)
            {
                // EE-109 "Suma partes produccion por ramo: total_primas_cedidas
                Error errorItem = err("109", "F", "", (int)item.index_fila, 0);
                item.filaError = errorItem.error + ", arch: " + errorItem.archivo + ", fila: " + errorItem.fila;
                this.errores.Add(errorItem);
            }
            this.listaValCont.Add(val);
            return val;
        }
        private dynamic partesProduccionRamos_5()
        {
            List<dynamic> datosValidacion = new List<dynamic>(con.Query<dynamic>(@"
                                           SELECT entidad, modalidad, ramo, total_primas_netas_retenidas as primas_netas_ret, 
                                                total_primas_netas,  total_primas_cedidas ,  
                                                total_primas_netas_retenidas - (total_primas_netas - total_primas_cedidas  ) as diferencia, index_fila
                                                FROM val_partes_produccion_ramos 
                                                WHERE ( total_primas_netas_retenidas <> total_primas_netas - total_primas_cedidas )
                                                AND entidad = @codigoEntidad and fecha= @fechaCorte",
                                                           new { codigoEntidad = this.codigoEntidad, fechaCorte = this.fechaCorte }));
            con.Close();
            dynamic val = new ExpandoObject();
            val.valido = datosValidacion.Count == 0;
            Error error = err("110", "", "", 0, 0); //  error genérico para este tipo de validacion
            val.validacion = error.nombre_error; // "Sumas en Parte Produccion Mensuales, Capital asegurado neto = capital asegurado - capital anulado";
            val.estadoValidez = val.valido ? 4 : error.estadoValidez;
            val.data = datosValidacion;
            val.descripcionError = val.valido ? "sin error" : error.error + " " + error.desc_error;
            foreach (dynamic item in datosValidacion)
            {
                // EE-110 "Suma partes produccion por ramo: total_primas_netas_retenidas
                Error errorItem = err("110", "F", "", (int)item.index_fila, 0);
                item.filaError = errorItem.error + ", arch: " + errorItem.archivo + ", fila: " + errorItem.fila;
                this.errores.Add(errorItem);
            }
            this.listaValCont.Add(val);
            return val;
        }

        ////////////////////////////////////////////////////// 2.2  SUMA PARTES SINIESTROS RAMOS TRIMESTRAL  Separado en 4 partes   /////////////////////////////////////////////////////////////////////////////////

        private dynamic partesSiniestrosRamos_1()
        {
            List<dynamic> datosValidacion = new List<dynamic>(con.Query<dynamic>(@" SELECT entidad, modalidad, ramo, 
                                                                total_sins_reaseguro_aceptado as total_sins_reas_aceptado, sins_reaseguro_aceptado_nacional as sins_reas_aceptado_nal,
                                                                sins_reaseguro_aceptado_extranjero as  sins_reas_aceptado_ext,
                                                                total_sins_reaseguro_aceptado - ( sins_reaseguro_aceptado_nacional + sins_reaseguro_aceptado_extranjero ) as diferencia , index_fila
                                                                FROM val_partes_siniestros_ramos
                                                            WHERE (total_sins_reaseguro_aceptado <> sins_reaseguro_aceptado_nacional + sins_reaseguro_aceptado_extranjero ) 
                                                            AND entidad = @codigoEntidad and fecha= @fechaCorte",
                                                           new { codigoEntidad = this.codigoEntidad, fechaCorte = this.fechaCorte }));
            con.Close();
            dynamic val = new ExpandoObject();
            val.titulo = "Sumas en Partes Siniestros por Ramos (TRIMESTRAL)";
            val.valido = datosValidacion.Count == 0;
            Error error = err("111", "", "", 0, 0); //  error genérico para este tipo de validacion
            val.validacion = error.nombre_error; // "Sumas en Parte Produccion Mensuales, Capital asegurado neto = capital asegurado - capital anulado";
            val.estadoValidez = val.valido ? 4 : error.estadoValidez;
            val.data = datosValidacion;
            val.descripcionError = val.valido ? "sin error" : error.error + " " + error.desc_error;
            foreach (dynamic item in datosValidacion)
            {
                // EE-111 "Suma partes produccion por ramo: total_sins_reaseguro_aceptado
                Error errorItem = err("111", "G", "", (int)item.index_fila, 0);
                item.filaError = errorItem.error + ", arch: " + errorItem.archivo + ", fila: " + errorItem.fila;
                this.errores.Add(errorItem);
            }
            this.listaValCont.Add(val);
            return val;
        }
        private dynamic partesSiniestrosRamos_2()
        {
            List<dynamic> datosValidacion = new List<dynamic>(con.Query<dynamic>(@" SELECT entidad, modalidad, ramo, 
                                                                	sins_totales, sins_directos ,
                                                                    total_sins_reaseguro_aceptado as  total_sins_reas_aceptado,
                                                                    sins_totales - ( sins_directos + total_sins_reaseguro_aceptado) as diferencia , index_fila
                                                            FROM val_partes_siniestros_ramos
                                                            WHERE ( sins_totales <> sins_directos + total_sins_reaseguro_aceptado ) 
                                                            AND entidad = @codigoEntidad and fecha= @fechaCorte",
                                                           new { codigoEntidad = this.codigoEntidad, fechaCorte = this.fechaCorte }));
            con.Close();
            dynamic val = new ExpandoObject();
            val.valido = datosValidacion.Count == 0;
            Error error = err("112", "", "", 0, 0); //  error genérico para este tipo de validacion
            val.validacion = error.nombre_error; // "Sumas en Parte Produccion Mensuales, Capital asegurado neto = capital asegurado - capital anulado";
            val.estadoValidez = val.valido ? 4 : error.estadoValidez;
            val.data = datosValidacion;
            val.descripcionError = val.valido ? "sin error" : error.error + " " + error.desc_error;
            foreach (dynamic item in datosValidacion)
            {
                // EE-112 "Suma partes produccion por ramo: sins_totales
                Error errorItem = err("112", "G", "", (int)item.index_fila, 0);
                item.filaError = errorItem.error + ", arch: " + errorItem.archivo + ", fila: " + errorItem.fila;
                this.errores.Add(errorItem);
            }
            this.listaValCont.Add(val);
            return val;
        }
        private dynamic partesSiniestrosRamos_3()
        {
            List<dynamic> datosValidacion = new List<dynamic>(con.Query<dynamic>(@" SELECT entidad, modalidad, ramo, 
                                                                total_sins_reembolsados as total_sins_reemb, sins_reembolsados_nacional as sins_reemb_nal,
                                                                sins_reembolsados_extranjero as  sins_reemb_ext,
                                                                total_sins_reembolsados - ( sins_reembolsados_nacional + sins_reembolsados_extranjero ) as diferencia , index_fila
                                                            FROM val_partes_siniestros_ramos
                                                            WHERE (total_sins_reembolsados <> sins_reembolsados_nacional + sins_reembolsados_extranjero ) 
                                                            AND entidad = @codigoEntidad and fecha= @fechaCorte",
                                                           new { codigoEntidad = this.codigoEntidad, fechaCorte = this.fechaCorte }));
            con.Close();
            dynamic val = new ExpandoObject();
            val.valido = datosValidacion.Count == 0;
            Error error = err("113", "", "", 0, 0); //  error genérico para este tipo de validacion
            val.validacion = error.nombre_error; // "Sumas en Parte Produccion Mensuales, Capital asegurado neto = capital asegurado - capital anulado";
            val.estadoValidez = val.valido ? 4 : error.estadoValidez;
            val.data = datosValidacion;
            val.descripcionError = val.valido ? "sin error" : error.error + " " + error.desc_error;
            foreach (dynamic item in datosValidacion)
            {
                // EE-113 "Suma partes produccion por ramo: total_sins_reembolsados
                Error errorItem = err("113", "G", "", (int)item.index_fila, 0);
                item.filaError = errorItem.error + ", arch: " + errorItem.archivo + ", fila: " + errorItem.fila;
                this.errores.Add(errorItem);
            }
            this.listaValCont.Add(val);
            return val;
        }
        private dynamic partesSiniestrosRamos_4()
        {
            List<dynamic> datosValidacion = new List<dynamic>(con.Query<dynamic>(@" SELECT entidad, modalidad, ramo, 
                                                                    total_sins_retenidos , sins_totales as siniestros_totales,
                                                                    total_sins_reembolsados as  total_sins_reemb,
                                                                    total_sins_retenidos - ( sins_totales - total_sins_reembolsados ) as diferencia , index_fila
                                                            FROM val_partes_siniestros_ramos
                                                            WHERE (total_sins_retenidos <> sins_totales - total_sins_reembolsados ) 
                                                            AND entidad = @codigoEntidad and fecha= @fechaCorte",
                                                           new { codigoEntidad = this.codigoEntidad, fechaCorte = this.fechaCorte }));
            con.Close();
            dynamic val = new ExpandoObject();
            val.valido = datosValidacion.Count == 0;
            Error error = err("114", "", "", 0, 0); //  error genérico para este tipo de validacion
            val.validacion = error.nombre_error; // "Sumas en Parte Produccion Mensuales, Capital asegurado neto = capital asegurado - capital anulado";
            val.estadoValidez = val.valido ? 4 : error.estadoValidez;
            val.data = datosValidacion;
            val.descripcionError = val.valido ? "sin error" : error.error + " " + error.desc_error;
            foreach (dynamic item in datosValidacion)
            {
                // EE-114 "Suma partes produccion por ramo: total_sins_retenidos
                Error errorItem = err("114", "G", "", (int)item.index_fila, 0);
                item.filaError = errorItem.error + ", arch: " + errorItem.archivo + ", fila: " + errorItem.fila;
                this.errores.Add(errorItem);
            }
            this.listaValCont.Add(val);
            return val;
        }

        ////////////////////////////////////////////////////// 2.3 . SUMAS EN PARTES SEGUROS a LARGO PLAZO TRIMESTRAL  Separado en 2 partes  ///////////////////////////////////////////////////////////////////////////////

        private dynamic partesSegLargoPlazo_1()
        {
            List<dynamic> datosValidacion = new List<dynamic>(con.Query<dynamic>(@" SELECT entidad, modalidad, ramo, 
                                                                capital_asegurado_retenido, capital_asegurado_total, capital_asegurado_cedido, 
                                                                capital_asegurado_retenido - (  capital_asegurado_total - capital_asegurado_cedido  ) as diferencia, index_fila
                                                            FROM val_partes_seguros_largo_plazo
                                                            WHERE  ( capital_asegurado_retenido <> capital_asegurado_total - capital_asegurado_cedido  )
                                                            AND entidad = @codigoEntidad and fecha= @fechaCorte",
                                                           new { codigoEntidad = this.codigoEntidad, fechaCorte = this.fechaCorte }));
            con.Close();
            dynamic val = new ExpandoObject();
            val.titulo = "Suma de Partes Seguros a Largo Plazo (TRIMESTRAL)";
            val.valido = datosValidacion.Count == 0;
            Error error = err("115", "", "", 0, 0); //  error genérico para este tipo de validacion
            val.validacion = error.nombre_error; // "Sumas en Parte Produccion Mensuales, Capital asegurado neto = capital asegurado - capital anulado";
            val.estadoValidez = val.valido ? 4 : error.estadoValidez;
            val.data = datosValidacion;
            val.descripcionError = val.valido ? "sin error" : error.error + " " + error.desc_error;
            foreach (dynamic item in datosValidacion)
            {
                // EE-115 "Suma partes produccion por ramo: capital_asegurado_retenido
                Error errorItem = err("115", "H", "", (int)item.index_fila, 0);
                item.filaError = errorItem.error + ", arch: " + errorItem.archivo + ", fila: " + errorItem.fila;
                this.errores.Add(errorItem);
            }
            this.listaValCont.Add(val);
            return val;
        }
        private dynamic partesSegLargoPlazo_2()
        {
            List<dynamic> datosValidacion = new List<dynamic>(con.Query<dynamic>(@" SELECT entidad, modalidad, ramo, 
                                                                reserva_matematica_retenida as reserva_mat_retenida, reserva_matematica_total as reserva_mat_total, reserva_matematica_reasegurador as reserva_mat_reas, 
                                                                reserva_matematica_retenida - (  reserva_matematica_total - reserva_matematica_reasegurador  ) as diferencia, index_fila
                                                            FROM val_partes_seguros_largo_plazo
                                                            WHERE  ( reserva_matematica_retenida <> reserva_matematica_total - reserva_matematica_reasegurador )
                                                            AND entidad = @codigoEntidad and fecha= @fechaCorte",
                                                           new { codigoEntidad = this.codigoEntidad, fechaCorte = this.fechaCorte }));
            con.Close();
            dynamic val = new ExpandoObject();
            val.valido = datosValidacion.Count == 0;
            Error error = err("116", "", "", 0, 0); //  error genérico para este tipo de validacion
            val.validacion = error.nombre_error; // "Sumas en Parte Produccion Mensuales, Capital asegurado neto = capital asegurado - capital anulado";
            val.estadoValidez = val.valido ? 4 : error.estadoValidez;
            val.data = datosValidacion;
            val.descripcionError = val.valido ? "sin error" : error.error + " " + error.desc_error;
            foreach (dynamic item in datosValidacion)
            {
                // EE-116 "Suma partes produccion por ramo: reserva_matematica_retenida
                Error errorItem = err("116", "H", "", (int)item.index_fila, 0);
                item.filaError = errorItem.error + ", arch: " + errorItem.archivo + ", fila: " + errorItem.fila;
                this.errores.Add(errorItem);
            }
            this.listaValCont.Add(val);
            return val;
        }

        ////////////////////////////////////////////////////// 3 . Validacio de CUENTAS EN CORTO PLAZO TRIMESTRAL EN PARTES DE LARGO PLAZO  ///////////////////////////////////////////////////////////////////////////////

        private dynamic partesCortoPlazoLargoPlazo()
        {
            List<dynamic> datosValidacion = new List<dynamic>(con.Query<dynamic>(@" SELECT entidad, modalidad, ramo, 
                                                                capital_asegurado_total as cap_aseg_total, capital_asegurado_cedido as cap_aseg_cedido, 
                                                                capital_asegurado_retenido as cap_aseg_retenido, reserva_matematica_total as reserva_mat_total, 
                                                                reserva_matematica_reasegurador as reserva_mat_reas, reserva_matematica_retenida as reserva_mat_retenida, index_fila
                                                            FROM  val_partes_seguros_largo_plazo
                                                            WHERE ramo in ('42','45','46','47','49','50')
                                                            AND entidad = @codigoEntidad and fecha= @fechaCorte",
                                                           new { codigoEntidad = this.codigoEntidad, fechaCorte = this.fechaCorte }));
            con.Close();
            dynamic val = new ExpandoObject();
            val.titulo = "Cuentas de Corto Plazo en Partes de Largo Plazo (TRIMESTRAL)";
            val.valido = datosValidacion.Count == 0;
            Error error = err("117", "", "", 0, 0); //  error genérico para este tipo de validacion
            val.validacion = error.nombre_error; // "Sumas en Parte Produccion Mensuales, Capital asegurado neto = capital asegurado - capital anulado";
            val.estadoValidez = val.valido ? 4 : error.estadoValidez;
            val.data = datosValidacion;
            val.descripcionError = val.valido ? "sin error" : error.error + " " + error.desc_error;
            foreach (dynamic item in datosValidacion)
            {
                // EE-117 "ramos ('42','45','46','47','49','50') no pertenece a seguros a largo plazo
                Error errorItem = err("117", "H", "", (int)item.index_fila, 0);
                item.filaError = errorItem.error + ", arch: " + errorItem.archivo + ", fila: " + errorItem.fila;
                this.errores.Add(errorItem);
            }
            this.listaValCont.Add(val);
            return val;
        }

        ////////////////////////////////////////////////////// 4 . SUMAS EN PARTES PRODUCCION  Separado en 4 partes  ///////////////////////////////////////////////////////////////////////////////

        private dynamic sumasEnPartesProduccion_1_CapitalAsegurado()
        {
            List<dynamic> datosValidacion = new List<dynamic>(con.Query<dynamic>(@"
                                           SELECT entidad, modalidad, ramo, poliza, capital_asegurado_neto, capital_asegurado, capital_anulado,
                                                capital_asegurado_neto - (capital_asegurado - capital_anulado ) AS diferencia , index_fila
                                                FROM val_partes_produccion 
                                                WHERE ( capital_asegurado_neto <> capital_asegurado - capital_anulado )
                                                AND entidad = @codigoEntidad and fecha= @fechaCorte",
                                                new { codigoEntidad = this.codigoEntidad, fechaCorte = this.fechaCorte }));
            con.Close();
            dynamic val = new ExpandoObject();
            val.titulo = "Sumas en Partes Produccion Mensuales";
            val.valido = datosValidacion.Count == 0;
            Error error = err("102", "", "", 0, 0); //  error genérico para este tipo de validacion
            val.validacion = error.nombre_error; // "Sumas en Parte Produccion Mensuales, Capital asegurado neto = capital asegurado - capital anulado";
            val.estadoValidez = val.valido ? 4 : error.estadoValidez;
            val.data = datosValidacion;
            val.descripcionError = val.valido ? "sin error" : error.error + " " + error.desc_error;
            foreach (dynamic item in datosValidacion)
            {
                // EE-102 "Suma partes produccion para Capital asegurado neto"
                Error errorItem = err("102", "A", "", (int)item.index_fila, 0);
                item.filaError = errorItem.error + ", arch: " + errorItem.archivo + ", fila: " + errorItem.fila;
                this.errores.Add(errorItem);
            }
            this.listaValCont.Add(val);
            return val;
        }
        private dynamic sumasEnPartesProduccion_2_PolizasNetas()
        {
            List<dynamic> datosValidacion = new List<dynamic>(con.Query<dynamic>(@"
                                            SELECT entidad, modalidad, ramo, poliza, polizas_netas, polizas_nuevas, polizas_renovadas, polizas_anuladas,
                                                    polizas_netas -( polizas_nuevas + polizas_renovadas - polizas_anuladas ) AS diferencia , index_fila
                                                    FROM val_partes_produccion 
                                                    WHERE ( polizas_netas <> polizas_nuevas + polizas_renovadas - polizas_anuladas )
                                                AND entidad = @codigoEntidad and fecha= @fechaCorte",
                                                new { codigoEntidad = this.codigoEntidad, fechaCorte = this.fechaCorte }));
            con.Close();
            dynamic val = new ExpandoObject();
            val.valido = datosValidacion.Count == 0;
            Error error = err("103", "", "", 0, 0); //  error genérico para este tipo de validacion
            val.validacion = error.nombre_error; // "Suma partes produccion mensuales: Pólizas Netas =  Pólizas Netas + Polizas renovadas - Polizas anula";
            val.estadoValidez = val.valido ? 4 : error.estadoValidez;
            val.data = datosValidacion;
            val.descripcionError = val.valido ? "sin error" : error.error + " " + error.desc_error;
            foreach (dynamic item in datosValidacion)
            {
                // E-103 "Sumas en Parte Produccion Mensuales, Polizas Netas"
                Error errorItem = err("103", "A", "", (int)item.index_fila, 0);
                item.filaError = errorItem.error + ", arch: " + errorItem.archivo + ", fila: " + errorItem.fila;
                this.errores.Add(errorItem);
            }
            this.listaValCont.Add(val);
            return val;
        }
        private dynamic sumasEnPartesProduccion_3_PrimaNetaAnulacionesM()
        {
            List<dynamic> datosValidacion = new List<dynamic>(con.Query<dynamic>(@"
                                            SELECT entidad, modalidad, ramo, poliza, prima_neta_m, prima_directa_m, prima_anulada_m,
                                                prima_neta_m - ( prima_directa_m - prima_anulada_m ) AS diferencia , index_fila
                                                FROM val_partes_produccion 
                                                WHERE ( prima_neta_m <> prima_directa_m - prima_anulada_m )
                                                AND entidad = @codigoEntidad and fecha= @fechaCorte",
                                                new { codigoEntidad = this.codigoEntidad, fechaCorte = this.fechaCorte }));
            con.Close();
            dynamic val = new ExpandoObject();
            val.valido = datosValidacion.Count == 0;
            Error error = err("104", "", "", 0, 0); //  error genérico para este tipo de validacion
            val.validacion = error.nombre_error;// "Suma partes produccion mensuales: Prima neta de anulaciones =  Prima Directa + Prima anulada en moneda extranjera";
            val.estadoValidez = val.valido ? 4 : error.estadoValidez;
            val.data = datosValidacion;
            val.descripcionError = val.valido ? "sin error" : error.error + " " + error.desc_error;
            foreach (dynamic item in datosValidacion)
            {
                // E-104 "Sumas en Parte Produccion Mensuales, Primas Netas Anulaciones en mon extranjera"
                Error errorItem = err("104", "A", "", (int)item.index_fila, 0);
                item.filaError = errorItem.error + ", arch: " + errorItem.archivo + ", fila: " + errorItem.fila;
                this.errores.Add(errorItem);
            }
            this.listaValCont.Add(val);
            return val;
        }
        private dynamic sumasEnPartesProduccion_4_PrimaNetaAnulaciones()
        {
            List<dynamic> datosValidacion = new List<dynamic>(con.Query<dynamic>(@"
                                            SELECT entidad, modalidad, ramo, poliza, prima_neta, prima_directa, prima_anulada,
                                                prima_neta - ( prima_directa - prima_anulada ) AS diferencia , index_fila
                                                FROM val_partes_produccion 
                                                WHERE ( prima_neta <> prima_directa - prima_anulada  )
                                                AND entidad = @codigoEntidad and fecha= @fechaCorte",
                                                new { codigoEntidad = this.codigoEntidad, fechaCorte = this.fechaCorte }));
            con.Close();
            dynamic val = new ExpandoObject();
            val.valido = datosValidacion.Count == 0;
            Error error = err("105", "", "", 0, 0); //  error genérico para este tipo de validacion
            val.validacion = error.nombre_error;// "Suma partes produccion mensuales: Prima neta de anulaciones =  Prima Directa + Prima anulada en moneda nacional";
            val.estadoValidez = val.valido ? 4 : error.estadoValidez;
            val.data = datosValidacion;
            val.descripcionError = val.valido ? "sin error" : error.error + " " + error.desc_error;
            foreach (dynamic item in datosValidacion)
            {
                // E-105 "Sumas en Parte Produccion Mensuales, Primas Netas Anulaciones en mon nacional"
                Error errorItem = err("105", "A", "", (int)item.index_fila, 0);
                item.filaError = errorItem.error + ", arch: " + errorItem.archivo + ", fila: " + errorItem.fila;
                this.errores.Add(errorItem);
            }
            this.listaValCont.Add(val);
            return val;
        }

        ///////////////////////////////////////////////////// 6.  CUENTAS DE NIVEL 4 SIN CUENTA DE NIVEL 5         /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private dynamic cuentasNivel4Sin5()
        {
            List<dynamic> datosValidacion = new List<dynamic>(con.Query<dynamic>(@"
                                                SELECT DISTINCT entidad,  moneda, cuenta_financiera, cuenta_tecnica, index_fila
                                                FROM val_balance_estado_resultados 
                                                WHERE cuenta_financiera || cuenta_tecnica not in 
                                                    (
                                                    SELECT DISTINCT cuenta_financiera || substring(cuenta_tecnica,1,2) as padre
                                                    FROM val_balance_estado_resultados
                                                    WHERE char_length(cuenta_tecnica)=4 
                                                    )
                                                AND char_length(cuenta_tecnica)=2
                                                AND entidad = @codigoEntidad and fecha= @fechaCorte",
                                                new { codigoEntidad = this.codigoEntidad, fechaCorte = this.fechaCorte }));
            con.Close();
            dynamic val = new ExpandoObject();
            val.titulo = "Cuentas de Nivel 4 que no tienen Cuenta en Nivel 5";
            val.valido = datosValidacion.Count == 0;
            Error error = err("118", "", "", 0, 0); //  error genérico para este tipo de validacion
            val.validacion = error.nombre_error;
            val.estadoValidez = val.valido ? 4 : error.estadoValidez;
            val.data = datosValidacion;
            val.descripcionError = val.valido ? "sin error" : error.error + " " + error.desc_error;
            foreach (dynamic item in datosValidacion)
            {
                Error errorItem = err("118", codArchivo(item.cta_nivel4_Sin5), "", (int)item.index_fila, 0);
                item.filaError = errorItem.error + ", arch: " + errorItem.archivo + ", fila: " + errorItem.fila;
                this.errores.Add(errorItem);
            }
            this.listaValCont.Add(val);
            return val;
        }

        ///////////////////////////////////////////////////// 7. EQUIVALENCIAS DE CUENTAS DE ORDEN   //////////////////////////////////////////////////////////////////////////////////////////

        private dynamic cuentasDeOrden()
        {
            List<dynamic> datosValidacion = new List<dynamic>(con.Query<dynamic>(@"
                                                                            SELECT entidad, cuenta_financiera || ' ' || moneda || ' ' || cuenta_tecnica AS cuenta_a,
                                                                                    saldo_debe_actual AS saldo_cuenta_a,'70901 0 0101' cuenta_b, (select saldo_haber_actual
                                                                                    from val_balance_estado_resultados where entidad=@codigoEntidad AND moneda='0' AND
                                                                                    fecha=@fechaCorte AND cuenta_financiera || moneda || cuenta_tecnica='7090100101') as saldo_cuenta_b,
                                                                                    abs(saldo_debe_actual-(select saldo_haber_actual
                                                                                    from val_balance_estado_resultados where entidad=@codigoEntidad AND moneda='0' AND
                                                                                    fecha=@fechaCorte AND cuenta_financiera || moneda || cuenta_tecnica='7090100101')) as Diferencia, index_fila
                                                                                FROM val_balance_estado_resultados 
                                                                                WHERE entidad= @codigoEntidad AND moneda='0' AND
                                                                                    fecha=@fechaCorte
                                                                                    AND (cuenta_financiera || cuenta_tecnica = '60101')
                                                                                    AND (saldo_debe_actual = 0 OR saldo_debe_actual<>(select saldo_haber_actual from val_balance_estado_resultados
                                                                                    where cuenta_financiera || moneda || cuenta_tecnica='7090100101' AND entidad= @codigoEntidad AND moneda='0' AND
                                                                                    fecha=@fechaCorte))

                                                                                UNION
                                                                                SELECT entidad, cuenta_financiera || ' ' || moneda || ' ' || cuenta_tecnica AS cuenta_a,
                                                                                    saldo_debe_actual AS saldo_cuenta_a,'70901 0 0102' cuenta_b,(select saldo_haber_actual
                                                                                    from val_balance_estado_resultados where entidad=@codigoEntidad AND moneda='0' AND
                                                                                    fecha=@fechaCorte AND cuenta_financiera || moneda || cuenta_tecnica='7090100102') as saldo_cuenta_b,
                                                                                    abs(saldo_debe_actual-(select saldo_haber_actual
                                                                                    from val_balance_estado_resultados where entidad=@codigoEntidad AND moneda='0' AND
                                                                                    fecha=@fechaCorte AND cuenta_financiera || moneda || cuenta_tecnica='7090100102')) as Diferencia, index_fila
                                                                                FROM val_balance_estado_resultados 
                                                                                WHERE entidad= @codigoEntidad AND moneda='0' AND
                                                                                    fecha=@fechaCorte
                                                                                    AND (cuenta_financiera || cuenta_tecnica = '60102')
                                                                                    AND (saldo_debe_actual= 0 OR saldo_debe_actual<>(select saldo_haber_actual from val_balance_estado_resultados
                                                                                    where cuenta_financiera || moneda || cuenta_tecnica='7090100102' AND entidad= @codigoEntidad AND moneda='0' AND
                                                                                    fecha=@fechaCorte))

                                                                                UNION
                                                                                SELECT entidad, cuenta_financiera || ' ' || moneda || ' ' || cuenta_tecnica AS cuenta_a,
                                                                                    saldo_haber_actual AS saldo_cuenta_a,'60901 0 0101' cuenta_b, (select saldo_debe_actual
                                                                                    from val_balance_estado_resultados where entidad=@codigoEntidad AND moneda='0' AND
                                                                                    fecha=@fechaCorte AND cuenta_financiera || moneda || cuenta_tecnica='6090100101')  as saldo_cuenta_b,
                                                                                    abs(saldo_haber_actual-(select saldo_debe_actual
                                                                                    from val_balance_estado_resultados where entidad=@codigoEntidad AND moneda='0' AND
                                                                                    fecha=@fechaCorte AND cuenta_financiera || moneda || cuenta_tecnica='6090100101')) as Diferencia, index_fila
                                                                                FROM val_balance_estado_resultados 
                                                                                WHERE entidad= @codigoEntidad AND moneda='0' AND
                                                                                    fecha=@fechaCorte
                                                                                    AND (cuenta_financiera || cuenta_tecnica = '70101')
                                                                                    AND (saldo_haber_actual= 0 OR saldo_haber_actual<>(select saldo_debe_actual from val_balance_estado_resultados
                                                                                    where cuenta_financiera || moneda || cuenta_tecnica='6090100101' AND entidad= @codigoEntidad AND moneda='0' AND
                                                                                    fecha=@fechaCorte))

                                                                                UNION
                                                                                SELECT entidad, cuenta_financiera || ' ' || moneda || ' ' || cuenta_tecnica AS cuenta_a,
                                                                                    saldo_haber_actual AS saldo_cuenta_a,'60901 0 0102' cuenta_b, (select saldo_debe_actual
                                                                                    from val_balance_estado_resultados where entidad=@codigoEntidad AND moneda='0' AND
                                                                                    fecha=@fechaCorte AND cuenta_financiera || moneda || cuenta_tecnica='6090100102')  as saldo_cuenta_b,
                                                                                    abs(saldo_haber_actual-(select saldo_debe_actual
                                                                                    from val_balance_estado_resultados where entidad=@codigoEntidad AND moneda='0' AND
                                                                                    fecha=@fechaCorte AND cuenta_financiera || moneda || cuenta_tecnica='6090100102')) as Diferencia, index_fila
                                                                                FROM val_balance_estado_resultados 
                                                                                WHERE entidad= @codigoEntidad AND moneda='0' AND
                                                                                    fecha=@fechaCorte
                                                                                    AND (cuenta_financiera || cuenta_tecnica = '70102')
                                                                                    AND (saldo_haber_actual= 0 OR saldo_haber_actual<>(select saldo_debe_actual from val_balance_estado_resultados
                                                                                    where cuenta_financiera || moneda || cuenta_tecnica='6090100102' AND entidad= @codigoEntidad AND moneda='0' AND
                                                                                    fecha=@fechaCorte))

                                                                                UNION
                                                                                SELECT entidad, cuenta_financiera || ' ' || moneda || ' ' || cuenta_tecnica AS cuenta_a,
                                                                                    saldo_haber_actual AS saldo_cuenta_a,'60901 0 0103' cuenta_b, (select saldo_debe_actual
                                                                                    from val_balance_estado_resultados where entidad=@codigoEntidad AND moneda='0' AND
                                                                                    fecha=@fechaCorte AND cuenta_financiera || moneda || cuenta_tecnica='6090100103') as saldo_cuenta_b,
                                                                                    abs(saldo_haber_actual-(select saldo_debe_actual
                                                                                    from val_balance_estado_resultados where entidad=@codigoEntidad AND moneda='0' AND
                                                                                    fecha=@fechaCorte AND cuenta_financiera || moneda || cuenta_tecnica='6090100103')) as Diferencia, index_fila
                                                                                FROM val_balance_estado_resultados 
                                                                                WHERE entidad= @codigoEntidad AND moneda='0' AND
                                                                                    fecha=@fechaCorte
                                                                                    AND (cuenta_financiera || cuenta_tecnica = '70103')
                                                                                    AND (saldo_haber_actual= 0 OR saldo_haber_actual<>(select saldo_debe_actual from val_balance_estado_resultados
                                                                                    where cuenta_financiera || moneda || cuenta_tecnica='6090100103' AND entidad= @codigoEntidad AND moneda='0' AND
                                                                                    fecha=@fechaCorte))

                                                                                UNION
                                                                                SELECT entidad, cuenta_financiera || ' ' || moneda || ' ' || cuenta_tecnica AS cuenta_a,
                                                                                    saldo_haber_actual AS saldo_cuenta_a,'60901 0 0104' cuenta_b, (select saldo_debe_actual
                                                                                    from val_balance_estado_resultados where entidad=@codigoEntidad AND moneda='0' AND
                                                                                    fecha=@fechaCorte AND cuenta_financiera || moneda || cuenta_tecnica='6090100104') as saldo_cuenta_b,
                                                                                    abs(saldo_haber_actual-(select saldo_debe_actual
                                                                                    from val_balance_estado_resultados where entidad=@codigoEntidad AND moneda='0' AND
                                                                                    fecha=@fechaCorte AND cuenta_financiera || moneda || cuenta_tecnica='6090100104')) as Diferencia, index_fila
                                                                                FROM val_balance_estado_resultados 
                                                                                WHERE entidad= @codigoEntidad AND moneda='0' AND
                                                                                    fecha=@fechaCorte
                                                                                    AND (cuenta_financiera || cuenta_tecnica = '70104')
                                                                                    AND (saldo_haber_actual= 0 OR saldo_haber_actual<>(select saldo_debe_actual from val_balance_estado_resultados
                                                                                    where cuenta_financiera || moneda || cuenta_tecnica='6090100104' AND entidad= @codigoEntidad AND moneda='0' AND
                                                                                    fecha=@fechaCorte))

                                                                                UNION
                                                                                SELECT entidad, cuenta_financiera || ' ' || moneda || ' ' || cuenta_tecnica AS cuenta_a,
                                                                                    saldo_haber_actual  AS saldo_cuenta_a,'60901 0 0105' cuenta_b, (select saldo_debe_actual
                                                                                    from val_balance_estado_resultados where entidad=@codigoEntidad AND moneda='0' AND
                                                                                    fecha=@fechaCorte AND cuenta_financiera || moneda || cuenta_tecnica='6090100105') as saldo_cuenta_b,
                                                                                    abs(saldo_haber_actual-(select saldo_debe_actual
                                                                                    from val_balance_estado_resultados where entidad=@codigoEntidad AND moneda='0' AND
                                                                                    fecha=@fechaCorte AND cuenta_financiera || moneda || cuenta_tecnica='6090100105')) as Diferencia, index_fila
                                                                                FROM val_balance_estado_resultados 
                                                                                WHERE entidad= @codigoEntidad AND moneda='0' AND
                                                                                    fecha=@fechaCorte
                                                                                    AND (cuenta_financiera || cuenta_tecnica = '70105')
                                                                                    AND (saldo_haber_actual= 0 OR saldo_haber_actual<>(select saldo_debe_actual from val_balance_estado_resultados
                                                                                    where cuenta_financiera || moneda || cuenta_tecnica='6090100105' AND entidad= @codigoEntidad AND moneda='0' AND
                                                                                    fecha=@fechaCorte))

                                                                                UNION	
                                                                                SELECT entidad, cuenta_financiera || ' ' || moneda || ' ' || cuenta_tecnica AS cuenta_a,
                                                                                    saldo_haber_actual AS saldo_cuenta_a,'60901 0 0106' cuenta_b, (select saldo_debe_actual
                                                                                    from val_balance_estado_resultados where entidad=@codigoEntidad AND moneda='0' AND
                                                                                    fecha=@fechaCorte AND cuenta_financiera || moneda || cuenta_tecnica='6090100106') as saldo_cuenta_b,
                                                                                    abs(saldo_haber_actual-(select saldo_debe_actual
                                                                                    from val_balance_estado_resultados where entidad=@codigoEntidad AND moneda='0' AND
                                                                                    fecha=@fechaCorte AND cuenta_financiera || moneda || cuenta_tecnica='6090100106')) as Diferencia, index_fila
                                                                                FROM val_balance_estado_resultados 
                                                                                WHERE entidad= @codigoEntidad AND moneda='0' AND
                                                                                    fecha=@fechaCorte
                                                                                    AND (cuenta_financiera || cuenta_tecnica = '70106')
                                                                                    AND (saldo_haber_actual= 0 OR saldo_haber_actual<>(select saldo_debe_actual from val_balance_estado_resultados
                                                                                    where cuenta_financiera || moneda || cuenta_tecnica='6090100106' AND entidad= @codigoEntidad AND moneda='0' AND
                                                                                    fecha=@fechaCorte))  ",
                                                new { codigoEntidad = this.codigoEntidad, fechaCorte = this.fechaCorte }));
            con.Close();
            dynamic val = new ExpandoObject();
            val.titulo = "Equivalencias entre Cuentas de Orden";
            val.valido = datosValidacion.Count == 0;
            Error error = err("119", "", "", 0, 0); //  error genérico para este tipo de validacion
            val.validacion = error.nombre_error;
            val.estadoValidez = val.valido ? 4 : error.estadoValidez;
            val.data = datosValidacion;
            val.descripcionError = val.valido ? "sin error" : error.error + " " + error.desc_error;
            foreach (dynamic item in datosValidacion)
            {
                Error errorItem = err("119", this.codArchivo(item.cuenta_a), "", (int)item.index_fila, 0);
                item.filaError = errorItem.error + ", arch: " + errorItem.archivo + ", fila: " + errorItem.fila;
                this.errores.Add(errorItem);
            }
            this.listaValCont.Add(val);
            return val;
        }

        /////////////////////////////////////////////////////      8. DIFERENCIAS SUMAS HACIA ARRIBA   ///////////////////////////////////////////////////////////

        private dynamic diferenciasSumasHaciaArrriba()
        {
            List<dynamic> datosValidacion = new List<dynamic>();
            foreach (string moneda in new[] { "0", "1", "2", "4" }) // por cada moneda 0,1,2,4
            {

                List<dynamic> sumasMoneda = new List<dynamic>(con.Query<dynamic>(@"
                                                                SELECT 
                                                                    res.index_fila,
                                                                	res.cuenta_financiera,
                                                                    res.cuenta_tecnica,
                                                                    res.moneda,
                                                                    res.saldo_debe_anterior ,
                                                                    res.saldo_haber_anterior,
                                                                    res.movimiento_debe,
                                                                    res.movimiento_haber,
                                                                    res.saldo_debe_actual,
                                                                    res.saldo_haber_actual,

                                                                    res.saldo_debe_anterior_suma,
                                                                    res.saldo_haber_anterior_suma,
                                                                    res.movimiento_debe_suma,
                                                                    res.movimiento_haber_suma,
                                                                    res.saldo_debe_actual_suma,
                                                                    res.saldo_haber_actual_suma,

                                                                    res.saldo_debe_anterior - res.saldo_debe_anterior_suma AS saldo_debe_anterior_dif,
                                                                    res.saldo_haber_anterior - res.saldo_haber_anterior_suma AS saldo_haber_anterior_dif,
                                                                    res.movimiento_debe - res.movimiento_debe_suma AS movimiento_debe_dif,
                                                                    res.movimiento_haber - res.movimiento_haber_suma AS movimiento_haber_dif,
                                                                    res.saldo_debe_actual - res.saldo_debe_actual_suma AS saldo_debe_actual_dif,
                                                                    res.saldo_haber_actual - res.saldo_haber_actual_suma AS saldo_haber_actual_dif

                                                            FROM
                                                            (
                                                                ----------------------------- sumas nivel 5 en cuenta padre nivel 4  ----------------------------
                                                                select * from 
                                                                    ( select 
                                                                            4 as nivel_padre,
                                                                            index_fila,
                                                                            cuenta_financiera,
                                                                            cuenta_tecnica,
                                                                            moneda,
                                                                            saldo_debe_anterior,
                                                                            saldo_haber_anterior,
                                                                            movimiento_debe,
                                                                            movimiento_haber,
                                                                            saldo_debe_actual,
                                                                            saldo_haber_actual
                                                                    from val_balance_estado_resultados
                                                                    where entidad=@codigoEntidad and fecha=@fechaCorte
                                                                    and char_length(cuenta_financiera)=5 and char_length(cuenta_tecnica)=2 and moneda=@moneda
                                                                    )as t4,
                                                                    ( select 
                                                                            (cuenta_financiera || substring(cuenta_tecnica,1,2) ) as cuenta_padre,
                                                                            sum(saldo_debe_anterior) as saldo_debe_anterior_suma,
                                                                            sum(saldo_haber_anterior) as saldo_haber_anterior_suma,
                                                                            sum(movimiento_debe) as movimiento_debe_suma,
                                                                            sum(movimiento_haber) as movimiento_haber_suma ,
                                                                            sum(saldo_debe_actual) as saldo_debe_actual_suma,
                                                                            sum(saldo_haber_actual) as saldo_haber_actual_suma
                                                                            from val_balance_estado_resultados
                                                                            where entidad=@codigoEntidad and fecha= @fechaCorte
                                                                            and char_length(cuenta_financiera)=5 and char_length(cuenta_tecnica)=4 and moneda=@moneda
                                                                            group by cuenta_financiera || substring(cuenta_tecnica,1,2)
                                                                    ) as ts5
                                                                WHERE	ts5.cuenta_padre = t4.cuenta_financiera || t4.cuenta_tecnica AND 
                                                                    (t4.saldo_debe_anterior <> ts5.saldo_debe_anterior_suma or
                                                                    t4.saldo_haber_anterior <> ts5.saldo_haber_anterior_suma or
                                                                    t4.movimiento_debe <> ts5.movimiento_debe_suma or
                                                                    t4.movimiento_haber <> ts5.movimiento_haber_suma or
                                                                    t4.saldo_debe_actual <> ts5.saldo_debe_actual_suma or
                                                                    t4.saldo_haber_actual <> ts5.saldo_haber_actual_suma)	
                                                                            
                                                                -------------------------------  sumas nivel 4 en cuenta padre nivel 3  -------------------------------------------

                                                                UNION
                                                                SELECT * from
                                                                    (select 
                                                                            3 as nivel_padre,
                                                                            index_fila,
                                                                            cuenta_financiera,
                                                                            cuenta_tecnica,
                                                                            moneda,
                                                                            saldo_debe_anterior,
                                                                            saldo_haber_anterior,
                                                                            movimiento_debe,
                                                                            movimiento_haber,
                                                                            saldo_debe_actual,
                                                                            saldo_haber_actual
                                                                        from val_balance_estado_resultados
                                                                        where entidad=@codigoEntidad and fecha=@fechaCorte
                                                                        and char_length(cuenta_financiera)=5 and char_length(cuenta_tecnica)=0 and moneda=@moneda) t3, 
                                                                    ( select 
                                                                            rtrim(cuenta_financiera) as cuenta_padre,
                                                                            sum(saldo_debe_anterior) as saldo_debe_anterior_suma,
                                                                            sum(saldo_haber_anterior) as saldo_haber_anterior_suma,
                                                                            sum(movimiento_debe) as movimiento_debe_suma,
                                                                            sum(movimiento_haber) as movimiento_haber_suma ,
                                                                            sum(saldo_debe_actual) as saldo_debe_actual_suma,
                                                                            sum(saldo_haber_actual) as saldo_haber_actual_suma
                                                                            from val_balance_estado_resultados
                                                                            where entidad=@codigoEntidad and fecha=@fechaCorte
                                                                            and char_length(cuenta_financiera)=5 and char_length(cuenta_tecnica)=2 and moneda=@moneda
                                                                            group by rtrim(cuenta_financiera)
                                                                    )as ts4
                                                                where 
                                                                ts4.cuenta_padre=ltrim(t3.cuenta_financiera) || ltrim(t3.cuenta_tecnica) AND
                                                                (	t3.saldo_debe_anterior <> ts4.saldo_debe_anterior_suma or
                                                                    t3.saldo_haber_anterior <> ts4.saldo_haber_anterior_suma or
                                                                    t3.movimiento_debe <> ts4.movimiento_debe_suma or
                                                                    t3.movimiento_haber <> ts4.movimiento_haber_suma or
                                                                    t3.saldo_debe_actual <> ts4.saldo_debe_actual_suma or
                                                                    t3.saldo_haber_actual <> ts4.saldo_haber_actual_suma)
                                                                    
                                                                ---------------------------- sumas nivel 3 en cuenta padre nivel 2  ----------------------------------------------------------

                                                                UNION
                                                                SELECT * from
                                                                    (select 
                                                                            2 as nivel_padre,
                                                                            index_fila,
                                                                            cuenta_financiera,
                                                                            cuenta_tecnica,
                                                                            moneda,
                                                                            saldo_debe_anterior,
                                                                            saldo_haber_anterior,
                                                                            movimiento_debe,
                                                                            movimiento_haber,
                                                                            saldo_debe_actual,
                                                                            saldo_haber_actual
                                                                    from val_balance_estado_resultados
                                                                    where entidad=@codigoEntidad and fecha=@fechaCorte
                                                                    and char_length(cuenta_financiera)=3 and char_length(cuenta_tecnica)=0 and moneda=@moneda ) t2,
                                                                    ( select 
                                                                            substring(rtrim(cuenta_financiera),1,3) as cuenta_padre,
                                                                            sum(saldo_debe_anterior) as saldo_debe_anterior_suma,
                                                                            sum(saldo_haber_anterior) as saldo_haber_anterior_suma,
                                                                            sum(movimiento_debe) as movimiento_debe_suma,
                                                                            sum(movimiento_haber) as movimiento_haber_suma ,
                                                                            sum(saldo_debe_actual) as saldo_debe_actual_suma,
                                                                            sum(saldo_haber_actual) as saldo_haber_actual_suma
                                                                            from val_balance_estado_resultados
                                                                            where entidad=@codigoEntidad and fecha=@fechaCorte
                                                                            and char_length(cuenta_financiera)=5 and char_length(cuenta_tecnica)=0 and moneda=@moneda
                                                                            group by substring(rtrim(cuenta_financiera),1,3)
                                                                    )as ts3
                                                                where 
                                                                ts3.cuenta_padre=ltrim(t2.cuenta_financiera) || ltrim(t2.cuenta_tecnica) AND
                                                                (	t2.saldo_debe_anterior <> ts3.saldo_debe_anterior_suma or
                                                                    t2.saldo_haber_anterior <> ts3.saldo_haber_anterior_suma or
                                                                    t2.movimiento_debe <> ts3.movimiento_debe_suma or
                                                                    t2.movimiento_haber <> ts3.movimiento_haber_suma or
                                                                    t2.saldo_debe_actual <> ts3.saldo_debe_actual_suma or
                                                                    t2.saldo_haber_actual <> ts3.saldo_haber_actual_suma)
                                                                    
                                                                ------------------------------ sumas nivel 2 en cuenta padre nivel 1  ---------------------------------------------------------

                                                                UNION
                                                                SELECT * from
                                                                    (select 
                                                                            1 as nivel_padre,
                                                                            index_fila,
                                                                            cuenta_financiera,
                                                                            cuenta_tecnica,
                                                                            moneda,
                                                                            saldo_debe_anterior,
                                                                            saldo_haber_anterior,
                                                                            movimiento_debe,
                                                                            movimiento_haber,
                                                                            saldo_debe_actual,
                                                                            saldo_haber_actual
                                                                    from val_balance_estado_resultados
                                                                    where entidad=@codigoEntidad and fecha=@fechaCorte
                                                                    and char_length(cuenta_financiera)=1 and char_length(cuenta_tecnica)=0 and moneda=@moneda ) t1,
                                                                    ( select 
                                                                            substring(rtrim(cuenta_financiera),1,1) as cuenta_padre,
                                                                            sum(saldo_debe_anterior) as saldo_debe_anterior_suma,
                                                                            sum(saldo_haber_anterior) as saldo_haber_anterior_suma,
                                                                            sum(movimiento_debe) as movimiento_debe_suma,
                                                                            sum(movimiento_haber) as movimiento_haber_suma ,
                                                                            sum(saldo_debe_actual) as saldo_debe_actual_suma,
                                                                            sum(saldo_haber_actual) as saldo_haber_actual_suma
                                                                            from val_balance_estado_resultados
                                                                            where entidad=@codigoEntidad and fecha=@fechaCorte
                                                                            and char_length(cuenta_financiera)=3 and char_length(cuenta_tecnica)=0 and moneda=@moneda
                                                                            group by substring(rtrim(cuenta_financiera),1,1)
                                                                    )as ts2
                                                                where 
                                                                ts2.cuenta_padre=ltrim(t1.cuenta_financiera) || ltrim(t1.cuenta_tecnica) AND
                                                                (	t1.saldo_debe_anterior <> ts2.saldo_debe_anterior_suma or
                                                                    t1.saldo_haber_anterior <> ts2.saldo_haber_anterior_suma or
                                                                    t1.movimiento_debe <> ts2.movimiento_debe_suma or
                                                                    t1.movimiento_haber <> ts2.movimiento_haber_suma or
                                                                    t1.saldo_debe_actual <> ts2.saldo_debe_actual_suma or
                                                                    t1.saldo_haber_actual <> ts2.saldo_haber_actual_suma)
                                                                ) as res 
                                                            ORDER BY nivel_padre desc ,  cuenta_financiera, cuenta_tecnica ",
                                                    new { codigoEntidad = this.codigoEntidad, fechaCorte = this.fechaCorte, moneda = moneda }));








                foreach (dynamic elem in sumasMoneda)
                {
                    dynamic elemBal = new ExpandoObject();
                    elemBal.entidad = this.codigoEntidad;
                    elemBal.tipo = "Valores en balance";
                    elemBal.cuenta_financiera = elem.cuenta_financiera;
                    elemBal.moneda = elem.moneda;
                    elemBal.cuenta_tecnica = elem.cuenta_tecnica;
                    elemBal.saldo_debe_anterior = elem.saldo_debe_anterior;
                    elemBal.saldo_haber_anterior = elem.saldo_haber_anterior;
                    elemBal.movimiento_debe = elem.movimiento_debe;
                    elemBal.movimiento_haber = elem.movimiento_haber;
                    elemBal.saldo_debe_actual = elem.saldo_debe_actual;
                    elemBal.saldo_haber_actual = elem.saldo_haber_actual;
                    elemBal.index_fila = elem.index_fila;
                    Error errorItem = err("120", codArchivo(elem.cuenta_financiera), "", (int)elem.index_fila, 0);
                    elemBal.filaError = errorItem.error + ", arch: " + errorItem.archivo + ", fila: " + errorItem.fila;
                    this.errores.Add(errorItem);

                    dynamic elemSum = new ExpandoObject();
                    elemSum.entidad = this.codigoEntidad;
                    elemSum.tipo = "Valores en sumas";
                    elemSum.cuenta_financiera = elem.cuenta_financiera;
                    elemSum.moneda = elem.moneda;
                    elemSum.cuenta_tecnica = elem.cuenta_tecnica;
                    elemSum.saldo_debe_anterior = elem.saldo_debe_anterior_suma;
                    elemSum.saldo_haber_anterior = elem.saldo_haber_anterior_suma;
                    elemSum.movimiento_debe = elem.movimiento_debe_suma;
                    elemSum.movimiento_haber = elem.movimiento_haber_suma;
                    elemSum.saldo_debe_actual = elem.saldo_debe_actual_suma;
                    elemSum.saldo_haber_actual = elem.saldo_haber_actual_suma;
                    elemSum.index_fila = "";
                    elemSum.filaError = "";

                    dynamic elemDif = new ExpandoObject();
                    elemDif.entidad = this.codigoEntidad;
                    elemDif.tipo = "diferencias";
                    elemDif.cuenta_financiera = elem.cuenta_financiera;
                    elemDif.moneda = elem.moneda;
                    elemDif.cuenta_tecnica = elem.cuenta_tecnica;
                    elemDif.saldo_debe_anterior = elem.saldo_debe_anterior_dif;
                    elemDif.saldo_haber_anterior = elem.saldo_haber_anterior_dif;
                    elemDif.movimiento_debe = elem.movimiento_debe_dif;
                    elemDif.movimiento_haber = elem.movimiento_haber_dif;
                    elemDif.saldo_debe_actual = elem.saldo_debe_actual_dif;
                    elemDif.saldo_haber_actual = elem.saldo_haber_actual_dif;
                    elemDif.index_fila = "";
                    elemDif.filaError = "";

                    if ((elemDif.saldo_debe_anterior != elemDif.saldo_haber_anterior) || (elemDif.movimiento_debe != elemDif.movimiento_haber) || (elemDif.saldo_debe_actual != elemDif.saldo_haber_actual))
                    {
                        datosValidacion.Add(elemBal);
                        datosValidacion.Add(elemSum);
                        datosValidacion.Add(elemDif);
                    }
                }
            };
            con.Close();
            dynamic val = new ExpandoObject();
            val.titulo = "Diferencias en Sumas Hacia Arriba";
            val.valido = datosValidacion.Count == 0;
            Error error = err("120", "", "", 0, 0); //  error genérico para este tipo de validacion
            val.validacion = error.nombre_error;
            val.estadoValidez = val.valido ? 4 : error.estadoValidez;
            val.data = datosValidacion;
            val.descripcionError = val.valido ? "sin error" : error.error + " " + error.desc_error;
            this.listaValCont.Add(val);
            return val;
        }


        ///////////////////////////////////////////////////// 9.1|. ERRORES EN EL CALCULO DE RESERVA PARA RIESGOS EN CURSO SOAT   //////////////////////////////////////////////////////////////////////////////////////////

        private dynamic calculoReservasRiesgosSoat()
        {
            List<dynamic> datosValidacion = new List<dynamic>(con.Query<dynamic>(@"
                                                                             SELECT 
                                                                            tbase.entidad,
                                                                            tbase.monto1 as prod_neta_anulacion_soat,
                                                                            tbase.monto2 as prima_cedida_reas_soat,
                                                                            tbase.monto3 as anulacion_primas_cedidas_reaseguro,
                                                                            tbase.monto1 - tbase.monto2 + tbase.monto3 AS prod_neta_reaseguro_soat,
                                                                            tbase.factor as factor_calculo,
                                                                            cast((tbase.monto1 - tbase.monto2 + tbase.monto3) * tbase.factor_decimal  as decimal(16,2)) AS multiplica_factor,
                                                                            tbase.monto_cuenta_204019455,
                                                                            cast(tbase.monto_cuenta_204019455 - (tbase.monto1 - tbase.monto2 + tbase.monto3) * tbase.factor_decimal as decimal(16,2))  AS diferencia,
                                                                            cast((tbase.monto_cuenta_204019455 - (tbase.monto1 - tbase.monto2 + tbase.monto3) * tbase.factor_decimal) / tbase.monto_cuenta_204019455 as decimal(16,5)) As porcentaje,
                                                                            '' as validacion,
                                                                            0 as index_fila
                                                                            
                                                                            FROM 
                                                                            (
                                                                                select 
                                                                                    entidad,
                                                                                    fecha,
                                                                                    sum(
                                                                                    case
                                                                                        when substring(cuenta_financiera,1,1)='5'
                                                                                            and cuenta_financiera in ('50101','50102','50103')
                                                                                            and cuenta_tecnica ='9455'
                                                                                            then (saldo_debe_actual - saldo_haber_actual) * (-1)
                                                                                        when cuenta_financiera in ('40101','40102','40103')
                                                                                            and cuenta_tecnica ='9455'
                                                                                            then (saldo_haber_actual - saldo_debe_actual)
                                                                                        ELSE 0
                                                                                    end
                                                                                    ) AS monto1,

                                                                                    sum(
                                                                                    case
                                                                                        when cuenta_financiera in ('50601','50602','50701','50702')
                                                                                            and cuenta_tecnica ='9455'
                                                                                        then (saldo_debe_actual - saldo_haber_actual)
                                                                                        ELSE 0

                                                                                    end
                                                                                    ) AS monto2,

                                                                                    sum(
                                                                                    case
                                                                                        when cuenta_financiera in ('40601','40602','40701','40702')
                                                                                            and cuenta_tecnica ='9455'
                                                                                        then (saldo_haber_actual - saldo_debe_actual)
                                                                                        ELSE 0

                                                                                    end
                                                                                    ) AS monto3,

                                                                                    (
                                                                                    case
                                                                                        when extract(month from fecha) = '1' then '11/12'
                                                                                        when extract(month from fecha) = '2' then '10/12'
                                                                                        when extract(month from fecha) = '3' then '9/12'
                                                                                        when extract(month from fecha) = '4' then '8/12'
                                                                                        when extract(month from fecha) = '5' then '7/12'
                                                                                        when extract(month from fecha) = '6' then '6/12'
                                                                                        when extract(month from fecha) = '7' then '5/12'
                                                                                        when extract(month from fecha) = '8' then '4/12'
                                                                                        when extract(month from fecha) = '9' then '3/12'
                                                                                        when extract(month from fecha) = '10' then '2/12'
                                                                                        when extract(month from fecha) = '11' then '1/12'
                                                                                        when extract(month from fecha) = '12' then '1/12'
                                                                                    end
                                                                                    ) AS  factor,

                                                                                    (
                                                                                    case
                                                                                        when extract(month from fecha) = '1' then 11
                                                                                        when extract(month from fecha) = '2' then 10
                                                                                        when extract(month from fecha) = '3' then 9
                                                                                        when extract(month from fecha) = '4' then 8
                                                                                        when extract(month from fecha) = '5' then 7
                                                                                        when extract(month from fecha) = '6' then 6
                                                                                        when extract(month from fecha) = '7' then 5
                                                                                        when extract(month from fecha) = '8' then 4
                                                                                        when extract(month from fecha) = '9' then 3
                                                                                        when extract(month from fecha) = '10' then 2
                                                                                        when extract(month from fecha) = '11' then 1
                                                                                        when extract(month from fecha) = '12' then 1
                                                                                    end
                                                                                    )/12.0  AS  factor_decimal,	

                                                                                    sum(
                                                                                    case
                                                                                        when cuenta_financiera = '20401'
                                                                                            and cuenta_tecnica ='9455'
                                                                                        then (saldo_haber_actual - saldo_debe_actual)
                                                                                    end
                                                                                    ) as monto_cuenta_204019455 
                                                                                

                                                                                FROM val_balance_estado_resultados
                                                                                WHERE entidad= @codigoEntidad
                                                                                and fecha= @fechaCorte  and moneda='0'
                                                                                group by entidad,fecha
                                                                            ) AS tbase 
                                                                         -- WHERE
                                                                         -- ABS ( (tbase.monto_cuenta_204019455 - (tbase.monto1 - tbase.monto2 + tbase.monto3) * tbase.factor_decimal) / tbase.monto_cuenta_204019455  ) > @margen",
                                                new { codigoEntidad = this.codigoEntidad, fechaCorte = this.fechaCorte, margen = this.aperturaDatos.margen_validacion_soat }));
            con.Close();
            decimal margen = this.aperturaDatos.margen_validacion_soat;
            foreach (dynamic item in datosValidacion)
            {
                Error errorItem = err("121", "C o D", "", 0, 0);
                item.validacion = Math.Abs(Convert.ToDecimal(item.porcentaje)) > margen ? "ERROR" : "ACEPTABLE";
                item.filaError = Math.Abs(Convert.ToDecimal(item.porcentaje)) > margen ? errorItem.error + ", arch: " + errorItem.archivo : "";
                this.errores.Add(errorItem);
            }

            dynamic val = new ExpandoObject();
            val.titulo = "Validaciones SOAT";
            val.valido = datosValidacion.Where(x => x.validacion == "ERROR").Count() == 0;
            Error error = err("121", "", "", 0, 0); //  error genérico para este tipo de validacion
            val.validacion = error.nombre_error + " ; Margen aceptable: | porcentaje diferencia | < " + margen;
            val.estadoValidez = val.valido ? 4 : error.estadoValidez;
            val.data = datosValidacion;
            val.descripcionError = val.valido ? "sin error" : error.error + " " + error.desc_error;


            this.listaValCont.Add(val);
            return val;
        }


        ///////////////////////////////////////////////////// 9.2.ERRORES EN EL CALCULO DE RESERVA PARA SINIESTROS OCURRIDOS Y NO REPORTADOS SOAT  //////////////////////////////////////////////////////////////////////////////////////////

        private dynamic calculoReservasSiniestrosReportadosSoat()
        {
            List<dynamic> datosValidacion = new List<dynamic>(con.Query<dynamic>(@"
                                                                             SELECT 
                                                                            tbase.entidad,
                                                                            tbase.monto1 as prod_neta_anulacion_soat,
                                                                            tbase.monto2 as prima_cedida_reas_soat,
                                                                            tbase.monto3 as anulacion_primas_cedidas_reaseguro,
                                                                            tbase.monto1 - tbase.monto2 + tbase.monto3 AS prod_neta_reaseguro_soat,
                                                                            tbase.factor as factor_calculo,
                                                                            cast((tbase.monto1 - tbase.monto2 + tbase.monto3) * tbase.factor_decimal  as decimal(16,2)) AS multiplica_factor,
                                                                            tbase.monto_cuenta_205019455,
                                                                            cast(tbase.monto_cuenta_205019455 - (tbase.monto1 - tbase.monto2 + tbase.monto3) * tbase.factor_decimal as decimal(16,2))  AS diferencia,
                                                                            case 
                                                                                when tbase.monto_cuenta_205019455 = 0
                                                                                then 0
                                                                                else cast((tbase.monto_cuenta_205019455 - (tbase.monto1 - tbase.monto2 + tbase.monto3) * tbase.factor_decimal) / tbase.monto_cuenta_205019455 
                                                                                    as decimal(16,5)) 
                                                                            end As porcentaje,
                                                                            '' as validacion,
                                                                            0 as index_fila
                                                                            
                                                                            FROM 
                                                                            (
                                                                            select 
                                                                                entidad,
                                                                                fecha,
                                                                                sum(
                                                                                case
                                                                                when substring(cuenta_financiera,1,1)='5'
                                                                                    and cuenta_financiera in ('50101','50102','50103')
                                                                                    and cuenta_tecnica ='9455'
                                                                                    then (saldo_debe_actual - saldo_haber_actual) * (-1)
                                                                                when cuenta_financiera in ('40101','40102','40103')
                                                                                    and cuenta_tecnica ='9455'
                                                                                    then (saldo_haber_actual - saldo_debe_actual)
                                                                                ELSE 0
                                                                                end
                                                                                ) AS monto1,

                                                                                sum(
                                                                                case
                                                                                when cuenta_financiera in ('50601','50602','50701','50702')
                                                                                    and cuenta_tecnica ='9455'
                                                                                then (saldo_debe_actual - saldo_haber_actual)
                                                                                ELSE 0

                                                                                end
                                                                                ) AS monto2,

                                                                                sum(
                                                                                case
                                                                                when cuenta_financiera in ('40601','40602','40701','40702')
                                                                                    and cuenta_tecnica ='9455'
                                                                                then (saldo_haber_actual - saldo_debe_actual)
                                                                                ELSE 0

                                                                                end
                                                                                ) AS monto3,

                                                                                '1/24' AS  factor,

                                                                            1/24.0  AS  factor_decimal,	

                                                                                sum(
                                                                                case
                                                                                    when cuenta_financiera = '20501'
                                                                                    and cuenta_tecnica ='9455'
                                                                                    then (saldo_haber_actual - saldo_debe_actual)
                                                                                end
                                                                                ) as monto_cuenta_205019455 
                                                                                

                                                                                FROM val_balance_estado_resultados
                                                                                WHERE entidad= @codigoEntidad
                                                                                and fecha= @fechaCorte  and moneda='0'
                                                                                group by entidad,fecha
                                                                            ) AS tbase
                                                                        -- WHERE 
                                                                        --  ABS( (tbase.monto_cuenta_205019455 - (tbase.monto1 - tbase.monto2 + tbase.monto3) * tbase.factor_decimal) / tbase.monto_cuenta_205019455 ) > @margen ",
                                                new { codigoEntidad = this.codigoEntidad, fechaCorte = this.fechaCorte, margen = this.aperturaDatos.margen_validacion_soat }));
            con.Close();
            decimal margen = this.aperturaDatos.margen_validacion_soat;
            foreach (dynamic item in datosValidacion)
            {
                Error errorItem = err("122", "C o D", "", 0, 0);
                item.validacion = Math.Abs(Convert.ToDecimal(item.porcentaje)) > margen ? "ERROR" : "ACEPTABLE";
                item.filaError = Math.Abs(Convert.ToDecimal(item.porcentaje)) > margen ? errorItem.error + ", arch: " + errorItem.archivo : "";
                this.errores.Add(errorItem);
            }

            dynamic val = new ExpandoObject();
            // val.titulo = "Reserva para Riesgos en Curso SOAT";
            val.valido = datosValidacion.Where(x => x.validacion == "ERROR").Count() == 0;
            Error error = err("122", "", "", 0, 0); //  error genérico para este tipo de validacion
            val.validacion = error.nombre_error + " ; Margen aceptable: | porcentaje diferencia | < " + margen;
            val.estadoValidez = val.valido ? 4 : error.estadoValidez;
            val.data = datosValidacion;
            val.descripcionError = val.valido ? "sin error" : error.error + " " + error.desc_error;


            this.listaValCont.Add(val);
            return val;
        }





        ///////////////////////////===============================================================================================================================///////////////////////////
        ///////////////////////////===============================================================================================================================///////////////////////////

        // realiza las llamadas a las funciones de validacion de contenidos, 
        private dynamic realizarValidacionContenido()
        {
            /*
            Este metodo llama a las funciones de validacion , en cada una de las funciones se guarada la validacion correspondiente
            en una lista global this.listaValCont , ademas este metodo devuelve un objeto cRespuesta con todas las validaciones como subojetos y el estado de validez
            */
            dynamic cRespuesta = new ExpandoObject();

            //0.a. Ecuacion Contable
            if (this.archivosEntidadCadena.Contains("C") && this.archivosEntidadCadena.Contains("D"))
                cRespuesta.EcuacionContable = this.ecuacionContable();

            // 0.b. Cuentas que no estan en el PUC
            if (this.archivosEntidadCadena.Contains("C") && this.archivosEntidadCadena.Contains("D"))
                cRespuesta.CuentasNoPUC = this.cuentasQueNoEstanPUC();

            //1. valida SALDOS INICIALES Y FINALES
            if (this.archivosEntidadCadena.Contains("C") && this.archivosEntidadCadena.Contains("D"))
                cRespuesta.vSaldosInicialesFinales = this.saldosInicialesFinales();

            // 2.1 valida SUMA PARTES PRODUCCION POR RAMOS TRIMESTRAL
            if (this.archivosEntidadCadena.Contains("F"))
            {
                cRespuesta.partesProduccionRamos_1 = this.partesProduccionRamos_1();
                cRespuesta.partesProduccionRamos_2 = this.partesProduccionRamos_2();
                cRespuesta.partesProduccionRamos_3 = this.partesProduccionRamos_3();
                cRespuesta.partesProduccionRamos_4 = this.partesProduccionRamos_4();
                cRespuesta.partesProduccionRamos_5 = this.partesProduccionRamos_5();
            }

            // 2.2 valida SUMA PARTES SINIESTROS POR RAMOS TRIMESTRAL
            if (this.archivosEntidadCadena.Contains("G"))
            {
                cRespuesta.partesSiniestrosRamos_1 = this.partesSiniestrosRamos_1();
                cRespuesta.partesSiniestrosRamos_2 = this.partesSiniestrosRamos_2();
                cRespuesta.partesSiniestrosRamos_3 = this.partesSiniestrosRamos_3();
                cRespuesta.partesSiniestrosRamos_4 = this.partesSiniestrosRamos_4();
            }

            // 2.3 valida SUMA PARTES SINIESTROS POR RAMOS TRIMESTRAL
            if (this.archivosEntidadCadena.Contains("H"))
            {
                cRespuesta.partesSegLargoPlazo_1 = this.partesSegLargoPlazo_1();
                cRespuesta.partesSegLargoPlazo_2 = this.partesSegLargoPlazo_2();
            }

            // 3. valida Cuentas de corto plazo en partes Largo Plazo TRIMESTRAL
            if (this.archivosEntidadCadena.Contains("H"))
            {
                cRespuesta.partesSegLargoPlazo = this.partesCortoPlazoLargoPlazo();
            }

            //4. Valida SUMA PARTES PRODUCCIONn
            if (this.archivosEntidadCadena.Contains("A"))
            {
                cRespuesta.sumasEnPartesProduccion_1 = this.sumasEnPartesProduccion_1_CapitalAsegurado();
                cRespuesta.sumasEnPartesProduccion_2 = this.sumasEnPartesProduccion_2_PolizasNetas();
                cRespuesta.sumasEnPartesProduccion_3 = this.sumasEnPartesProduccion_3_PrimaNetaAnulacionesM();
                cRespuesta.sumasEnPartesProduccion_4 = this.sumasEnPartesProduccion_4_PrimaNetaAnulaciones();
            }

            // 5 no figura o esta comentado en el script original
            // 6. Verifica existencias cuentas nivel 5
            if (this.archivosEntidadCadena.Contains("C") && this.archivosEntidadCadena.Contains("D"))
                cRespuesta.cuentasNivel4Sin5 = this.cuentasNivel4Sin5();

            //7. EQUIVALENCIA CUENTAS DE ORDEN
            if (this.archivosEntidadCadena.Contains("C") && this.archivosEntidadCadena.Contains("D"))
                cRespuesta.cuentasDeOrden = this.cuentasDeOrden();

            // 8 . DIFERENCIAS EN SUMAS HACIA ARRIBA
            if (this.archivosEntidadCadena.Contains("C") && this.archivosEntidadCadena.Contains("D"))
                cRespuesta.diferenciasSumasHaciaArrriba = this.diferenciasSumasHaciaArrriba();


            // 9 . VALIDACIONES SOAT - CALCULO RESERVAS RIESGO SOAT - CALCULO DE SINIESTROS REPORTADOS
            if (this.archivosEntidadCadena.Contains("C") && this.archivosEntidadCadena.Contains("D"))
            {
                cRespuesta.calculoReservasRiesgosSoat = this.calculoReservasRiesgosSoat();
                cRespuesta.calculoReservasSiniestrosReportadosSoat = this.calculoReservasSiniestrosReportadosSoat();
            }




            int estadoValidez = 4;
            foreach (dynamic item in this.listaValCont)
            {
                if (item.estadoValidez < estadoValidez)  // si el error es  peor entonces ese se lo asigna, sera el estado del seguimiento
                    estadoValidez = item.estadoValidez;
            }
            cRespuesta.estadoValidez = estadoValidez;
            return cRespuesta;

        }

        private string codArchivo(object cuenta)
        {
            string grupo = cuenta.ToString().Substring(0, 1);
            string codigo = (grupo == "4" || grupo == "5") ? "D" : "C";
            return codigo;
        }

        private string descError(string cod_error)
        {
            return this.erroresDiccionario.Where(e => e.codigo == cod_error).Select(o => o.descripcion).FirstOrDefault();
        }
    }


}