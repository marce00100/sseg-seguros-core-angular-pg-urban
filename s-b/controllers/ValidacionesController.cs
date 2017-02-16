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
    [Route("svd/api/[Controller]")]
    public partial class ValidacionesController : Controller
    {
        private IDbConnection con = BdPG.instancia();
        private string codigoEntidad = "";
        private int anoControl;
        private int mesControl;
        private int diaControl;
        private DateTime fechaCorte = new DateTime();
        private DateTime fechaActual = DateTime.Now;
        private int nFechaCorte;
        private List<Error> errores = new List<Error>();
        private dynamic resultado = new ExpandoObject();
        private List<ArchivoCargado> archivosEnv = new List<ArchivoCargado>();
        private List<string> departamentos;
        private List<string> monedas;
        private List<string> monedasConIntegrador;
        private List<string> modalidades;
        private List<string> modalidadesRamos;
        private List<string> modalidadesRamosPolizas;
        private List<dynamic> erroresDiccionario = ConstantesController.obtieneConstantesDeDimension("error");
        private decimal cambio_2; //  dolarCompra
        private decimal cambio_4; // UFV
        private decimal cambio_5; // EURO
        private decimal factorTC; // Factor Tipo de cambio cuando la moneda cambia de valor
        private decimal margenEF; // margen de aceptacion para calculos la validacion EF 
        private List<dynamic> listaValEF = new List<dynamic>();
        private val_partes_produccion_totales partes_produccion_totales;
        private dynamic partes_sins_totales;
        private List<dynamic> listaValCont = new List<dynamic>();
        private dynamic aperturaDatos;
        private List<Archivo> archivosList = new List<Archivo>();
        private string archivosEntidadCadena = "";


        private void inicializaDatos(string codEntidad, DateTime? fechaCorte = null)
        {
            AperturaController objAper = new AperturaController();
            this.aperturaDatos = objAper.getAperturaActiva().data;
            this.fechaCorte = this.aperturaDatos.fecha_corte;
            this.anoControl = this.fechaCorte.Year;
            this.mesControl = this.fechaCorte.Month;
            this.diaControl = this.fechaCorte.Day;
            this.nFechaCorte = this.anoControl * 10000 + this.mesControl * 100 + this.diaControl;
            this.codigoEntidad = codEntidad;
        }

        private Error err(string cod, string arch, string descPuntual, int fil, int col)
        {
            dynamic errorDicc = this.erroresDiccionario.Where(x => x.codigo == cod).FirstOrDefault();
            Error e = new Error();
            e.cod_error = cod;
            e.error = cod == "" ? "" : errorDicc.valor + "-" + errorDicc.codigo;
            e.archivo = arch;
            e.fila = fil;
            e.columna = col;
            e.descripcion_puntual = descPuntual;
            e.valido = !e.error.Contains("E");
            e.desc_error = errorDicc == null ? "" : errorDicc.descripcion; // this.descError(cod);
            e.nombre_error = errorDicc == null ? "" : errorDicc.nombre;

            int validez = 4;
            if (errorDicc == null)
                validez = 4;
            else if (errorDicc.valor == "EE")
                validez = 1;
            else if (errorDicc.valor == "E")
                validez = 2;
            else if (errorDicc.valor == "A")
                validez = 3;

            e.estadoValidez = validez;
            return e;
        }

        //// #############################                         VALIDACION  F O R M A T O                              ############################################/////////////
        ///  la variable objetoRecibido tiene como estructura (tomar nota  que no se recibiran dia, mes , ano   del objeto datos estando en produccion )
        ///  {
        ///    cod_entidad : "cod",
        ///    archivosCargados : [
        ///         {nombre:"SAAAAAMM.CCC", texto:".."}, 
        ///         {nombre:"SBAAAAMM.CCC", texto:".."}, ..  
        ///         ]
        ///   }
        [HttpPost("formato")] // la ruta puede ser formato/efectivo (realiza guarda el seguimiento y guarda errores)  o formato/test (para pruebas)  
        public object validacionFormato([FromBody]dynamic objetoRecibido)
        {
            // this.real = accion != "efectivo" ? false : true;
            inicializaDatos(objetoRecibido.cod_entidad.ToString());
            bool correcto = true;
            int validezF = 4; //valido

            foreach (var item in objetoRecibido.archivosCargados)
            {
                ArchivoCargado env = new ArchivoCargado();
                env.nombre = item.nombre.ToString();
                env.texto = item.texto.ToString();
                archivosEnv.Add(env);
            }

            // ####  PROCESOS DE VALIDACION 

            this.resultado.erroresF = this.errores;
            // #### Verifica si la entidad con la que el usuario se hace log, existe y esta activa, caso contrario se sale con errorF:  error de Entida no existente
            if (correcto && !validarEntidad())
            {
                correcto = false;
                validezF = 1;
                return resultado;
            }

            // #### Verifica que los archivos enviados son los correctos correctos.  Si no estan todos los archivos correctos, retorna mostrando los errores
            if (correcto && !validarArchivosEnviadosNombres(this.archivosEnv))
            {
                correcto = false;
                validezF = 1;
            }

            // #### SI  LOS ARCHIVOS SON LOS CORRECTOS SE REALIZA ESTA PARTE
            // convierte el campo validaciones a un JSON y transforma el texto en un Array de filas y columnas
            if (correcto)
            {
                this.transformaDatos(this.archivosEnv);
            }

            // #### VERIFICA LAS VALIDACIONES JSON 
            // #### Se realiza la  Validacion de formato de texto (columnas, y typos de datos), 
            // llegando aqui se cargan las listas de departamentos, monedas, modalidades, modalidadesRamos, modalidadesRamosPolizas 
            // la funcion devuelve V EE o A; segun el error o advertencia en caso de las validaciones mayor que 0. 
            string estadoValidaFormato = "";
            if (correcto)
            {
                estadoValidaFormato = this.validarFormatoTexto(this.archivosEnv);
            }

            if (correcto && estadoValidaFormato != "V") //si es distinto de valido se setea el estadoEnvio segun el error //////////////////////////
            {
                validezF = estadoValidaFormato == "EE" ? 1 : 3; // es Error 1: EE o 3:  Advertencia
                correcto = estadoValidaFormato == "EE" ? false : true; // En esta validacion de formato solo puede ser EE A o V, si no es EE es correcto
            }

            // antes de insertar se eliminan los registros de los archivos , de igual manera si hubieron errors se eliminan los registros
            eliminarDeTablaVal("A", this.codigoEntidad, this.fechaCorte);
            eliminarDeTablaVal("B", this.codigoEntidad, this.fechaCorte);
            eliminarDeTablaVal("C", this.codigoEntidad, this.fechaCorte);
            eliminarDeTablaVal("D", this.codigoEntidad, this.fechaCorte);
            eliminarDeTablaVal("E", this.codigoEntidad, this.fechaCorte);
            eliminarDeTablaVal("F", this.codigoEntidad, this.fechaCorte);
            eliminarDeTablaVal("G", this.codigoEntidad, this.fechaCorte);
            eliminarDeTablaVal("H", this.codigoEntidad, this.fechaCorte);
            //ingresa los datos a las tablas 
            if (correcto)
            {
                foreach (var arch in this.archivosEnv)
                {
                    int i = 0;
                    foreach (var registro in arch.objetosFila)
                    {
                        try
                        {
                            insertarITF(arch.codigo, registro);
                            i++;
                        }
                        catch (System.Exception ex)
                        {
                            // EE - 00 "No se pudo guardar la información, por favor intente de enviar nuevamente todos los archivos, ",
                            errores.Add(err("000", arch.nombre, "Si continua el error, comuniquese con la APS, comunicando el siguiente mensaje:  \n Error de tabla al insertar, " + ex.Message, 0, 0));
                            eliminarDeTablaVal("A", this.codigoEntidad, this.fechaCorte);
                            eliminarDeTablaVal("B", this.codigoEntidad, this.fechaCorte);
                            eliminarDeTablaVal("C", this.codigoEntidad, this.fechaCorte);
                            eliminarDeTablaVal("D", this.codigoEntidad, this.fechaCorte);
                            eliminarDeTablaVal("E", this.codigoEntidad, this.fechaCorte);
                            eliminarDeTablaVal("F", this.codigoEntidad, this.fechaCorte);
                            eliminarDeTablaVal("G", this.codigoEntidad, this.fechaCorte);
                            eliminarDeTablaVal("H", this.codigoEntidad, this.fechaCorte);
                            correcto = false;
                            validezF = 1;
                            break;
                        }
                    }
                }
            }

            // borra los errores relacionados con seguiiento_envios no vigentes
            ErroresController.borrarErroresDelLaMismaApertura(this.codigoEntidad);
            int idSeguimiento = SgmntController.insertarSeguimientoEnvio(this.codigoEntidad, validezF);
            int estadoValidez = 4;
            foreach (Error errorF in this.errores)
            {
                errorF.id_seguimiento_envio = idSeguimiento;
                ErroresController.insertarError(errorF);

                errorF.desc_error = descError(errorF.cod_error);
                if (errorF.estadoValidez < estadoValidez)
                    estadoValidez = errorF.estadoValidez;
            }
            con.Close();
            List<dynamic> constantes = ConstantesController.obtieneConstantesDeDimension("estado_seguimiento");
            dynamic estadoSeg = constantes.Where(x => x.codigo == estadoValidez.ToString()).LastOrDefault();


            this.resultado.status = "success";
            this.resultado.validoF = correcto;
            this.resultado.estadoValidez = estadoValidez;
            this.resultado.estadoValidez_desc = estadoSeg.valor + " - " + estadoSeg.descripcion;
            this.resultado.tipoValidacion = "formato";
            this.resultado.erroresF = this.errores;
            // resultado.env = this.archivosEnv;
            return this.resultado;
        }

        //############################                        VALIDACIONES   C O N T E N I D O               #################################################/////////////////////////    
        [HttpPost("contenido")]
        public object validacionSIF([FromBody]dynamic objetoRecibido)// objetoRecibido = {cod_entidad:cod_entidad}
        {
            inicializaDatos(objetoRecibido.cod_entidad.ToString());
            this.archivosEntidadCadena = ArchivosController.encadenaArchivosDeEntidadMes(this.codigoEntidad, this.mesControl);
            // realiza la validacion_________________________________________________>>>>>>
            dynamic valContenido = this.realizarValidacionContenido();
            int estadoValidezC = valContenido.estadoValidez;

            // obtiene el estado de validez que tiene seguimiento_envio ya guardado en la validacion de formato
            dynamic segEntidad = SgmntController.seguimientoAperturaActivaEntidad(this.codigoEntidad);
            int estadoValidez = Convert.ToInt16(segEntidad.estado);

            ////------------------------------------- se añade val EF dentro del proceso por un error al llamar en paralelo  ----------------------------------------
            this.cambio_2 = this.aperturaDatos.mCompra;
            this.cambio_4 = this.aperturaDatos.mUFV;
            this.cambio_5 = this.aperturaDatos.mEuroBS;
            this.margenEF = this.aperturaDatos.margen_partes_ef;
            this.factorTC = this.aperturaDatos.factor_tc;

            this.archivosEntidadCadena = ArchivosController.encadenaArchivosDeEntidadMes(this.codigoEntidad, this.mesControl);
            ////////_____________________________>>>> Validacion de Partes de Producción y Siniestros  con Estados Financieros ///////////////////////////////////////////
            int estadoValidezEF = this.validaPartesEF();

            estadoValidez = Math.Min(estadoValidez, Math.Min(estadoValidezC, estadoValidezEF));
            SgmntController.modificaEstado(segEntidad.id_seguimiento_envio, estadoValidez);

            foreach (Error error in this.errores)
            {
                if (!error.valido)
                {
                    error.id_seguimiento_envio = segEntidad.id_seguimiento_envio;
                    ErroresController.insertarError(error);
                }
            }

            List<dynamic> constantes = ConstantesController.obtieneConstantesDeDimension("estado_seguimiento");
            dynamic estadoSeg = constantes.Where(x => x.codigo == estadoValidez.ToString()).FirstOrDefault();

            this.con.Close();
            this.resultado.status = "success";
            this.resultado.estadoValidez = estadoValidez;
            this.resultado.estadoValidez_desc = estadoSeg.valor + " - " + estadoSeg.descripcion;

            this.resultado.estadoValidezC = estadoValidezC;
            this.resultado.validoC = estadoValidezC > 2;
            this.resultado.datosC = this.listaValCont;

            this.resultado.estadoValidezEF = estadoValidezEF;
            this.resultado.validoEF = estadoValidezEF > 2;
            this.resultado.datosEF = listaValEF;

            return this.resultado;
        }

        /// #############################               VALIDACION    P A R T E  S    E F                          ############################################////////////// 
        [HttpPost("partesEF")]
        public object validacionPartesEF([FromBody]dynamic objetoRecibido) // objetoRecibido = {cod_entidad:cod_entidad}
        {
            inicializaDatos(objetoRecibido.cod_entidad.ToString());
            this.cambio_2 = this.aperturaDatos.mCompra;
            this.cambio_4 = this.aperturaDatos.mUFV;
            this.cambio_5 = this.aperturaDatos.mEuroBS;
            this.margenEF = this.aperturaDatos.margen_partes_ef;
            this.factorTC = this.aperturaDatos.factor_tc;

            this.archivosEntidadCadena = ArchivosController.encadenaArchivosDeEntidadMes(this.codigoEntidad, this.mesControl);
            /////////////////////////////// Validacion de Partes de Producción y Siniestros  con Estados Financieros ///////////////////////////////////////////
            int estadoValidezEF = this.validaPartesEF();

            dynamic segEntidad = SgmntController.seguimientoAperturaActivaEntidad(this.codigoEntidad);
            if (estadoValidezEF < Convert.ToInt16(segEntidad.estado)) // si el estado de error de la validacion es peor que el de seguimiento actual, entonces se guarda el peor en el estado del seguimiento:   estados 1 E grave, 2 error , 3 advertencia 4 valido 
                SgmntController.modificaEstado(segEntidad.id_seguimiento_envio, estadoValidezEF);

            foreach (Error error in this.errores)
            {
                if (!error.valido)
                {
                    error.id_seguimiento_envio = segEntidad.id_seguimiento_envio; ;
                    ErroresController.insertarError(error);
                }
            }
            con.Close();
            this.resultado.status = "success";
            this.resultado.validoEF = estadoValidezEF > 2;
            this.resultado.validacionEF = listaValEF; //cada item  contiene { validacion:'titulo', data:{datos de la validacion}, valido: true or false, error:{error con descripcion}, tipo_parte: 'produccion o siniestro' }

            return this.resultado;
        }






        /// #############################       VALIDACION  de    H I S t O R I C O        ////////////////////////////////////////////////////// 
        [HttpGet("consulta/{web_reporte}/{idSeguimientoEnvio}")] //  web_reporte puede ser "web" o "reporte"
        public object validacionConsultaPartesEF(string web_reporte, int idSeguimientoEnvio) //objetoRecibido = {id_seguimiento_envio }
        {
            List<Error> erroresLista = ErroresController.getErroresDeSeguimiento(idSeguimientoEnvio);
            dynamic seguimientoEntidadDatos = SgmntController.getSeguimientoEntidadDatos(idSeguimientoEnvio);
            this.codigoEntidad = seguimientoEntidadDatos.cod_entidad.ToString();
            this.erroresDiccionario = ConstantesController.obtieneConstantesDeDimension("error");
            this.aperturaDatos = AperturaController.getAperturaDeSeguimiento(idSeguimientoEnvio);

            this.fechaCorte = this.aperturaDatos.fecha_corte;
            this.anoControl = this.fechaCorte.Year;
            this.mesControl = this.fechaCorte.Month;
            this.diaControl = this.fechaCorte.Day;

            this.cambio_2 = this.aperturaDatos.mCompra;
            this.cambio_4 = this.aperturaDatos.mUFV;
            this.cambio_5 = this.aperturaDatos.mEuroBS;
            this.margenEF = this.aperturaDatos.margen_partes_ef;
            this.factorTC = this.aperturaDatos.factor_tc;

            this.archivosEntidadCadena = ArchivosController.encadenaArchivosDeEntidadMes(this.codigoEntidad, this.mesControl);

            ////////////////////// FORMATO ////////////////////////////////////////////////
            IEnumerable<Error> erroresF = erroresLista.Where(x => x.cod_error.Substring(0, 1) == "0").OrderBy(x => x.cod_error);
            bool validoF = true;
            int estadoValidez = 4;
            foreach (Error errorF in erroresF)
            {
                errorF.desc_error = descError(errorF.cod_error);
                errorF.estadoValidez = estadoValidezDeError(errorF);
                if (errorF.estadoValidez < estadoValidez)
                    estadoValidez = errorF.estadoValidez;
            }
            if (estadoValidez > 1) // si no hay EE
            {
                // //////////////////////////////Validacion contenidos SIf y varios
                dynamic valContenido = this.realizarValidacionContenido();
                int estadoValidezC = valContenido.estadoValidez;
                this.resultado.estadoValidezC = estadoValidezC;
                this.resultado.validoC = estadoValidezC > 2;
                if (web_reporte == "web") //si es WEB se carga en una lista
                {
                    this.resultado.datosC = this.listaValCont;
                    this.resultado.validacionC = null;
                }
                else if (web_reporte == "reporte") //  sie es en para reporte se separa en objetos que tinen la misma estuctura
                {
                    this.resultado.validacionC = valContenido;
                    this.resultado.datosC = null;
                }

                /////////////////////////////// Validacion de Partes de Producción y Siniestros  con Estados Financieros ///////////////////////////////////////////
                int estadoValidezEF = this.validaPartesEF();
                this.resultado.estadoValidezEF = estadoValidezEF;
                this.resultado.validoEF = estadoValidezEF > 2;
                this.resultado.datosEF = listaValEF;

                // del resultado gral
                estadoValidez = Math.Min(estadoValidez, Math.Min(estadoValidezC, estadoValidezEF));
            }
            else
                validoF = false;

            this.resultado.aperturaDatos = this.aperturaDatos;
            this.resultado.seguimientoEntidadDatos = seguimientoEntidadDatos;


            List<dynamic> constantes = ConstantesController.obtieneConstantesDeDimension("estado_seguimiento");
            dynamic estadoSeg = constantes.Where(x => x.codigo == estadoValidez.ToString()).FirstOrDefault();

            this.con.Close();
            this.resultado.status = "success";
            this.resultado.estadoValidez = estadoValidez;
            this.resultado.estadoValidez_desc = estadoSeg.valor + " - " + estadoSeg.descripcion;

            this.resultado.validoF = validoF;
            this.resultado.erroresF = erroresF;

            return this.resultado;
        }


        [HttpGet("prueba/{op}")]
        public object prueba(string op)
        {
            dynamic constantes = con.Query(@"select nombre as ""nombre de constante"" , codigo from constantes limit 10");
            List<dynamic> lista = new List<dynamic>();
            dynamic ex = new ExpandoObject();
            string[] cadena = new[] { "111", "2222", "44444" };
            foreach (string s in cadena)
            {
                dynamic vas = new ExpandoObject();
                vas.id = s;
                Random rnd = new Random();
                vas.nombre = rnd.Next(100).ToString() + " " + s;
                lista.Add(vas);

            }

            AperturaController aper = new AperturaController();
            dynamic objetoAper = aper.getAperturaActiva().data.fecha_corte;

            ex.lista = new { titulo = "Lista de pares", data = lista };
            // ex.constantes = new { titulo = "objetos Constantes", data = new List<dynamic>() }; 
            ex.constantes = new { titulo = "objetos de  Consts", data = constantes };
            ex.titulo = "ES EXPO";
            ex.subtitulo = "Este subtitulo";
            ex.opcion = op;
            ex.fechaCOOOO = objetoAper;
            return new
            {
                exp = ex,
            };
        }

        [HttpGet("prueba_")]
        public object prueba_(string op)
        {



            return new { data = 4 };
        }


    }


}
