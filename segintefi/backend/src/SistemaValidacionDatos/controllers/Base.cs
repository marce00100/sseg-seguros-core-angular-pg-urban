using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaValidacionDatos.controllers
{
    public class Base
    {

        /// <summary>
        /// Identificador numérico de la aplicación
        /// </summary>
        public int AppId { set; get; }

        /// <summary>
        /// Identificador de la aplicación, se define este valor de forma estática
        /// </summary>
        public Guid AppGuid { set; get; } = new Guid("97f7c025-2a79-4197-9538-5a7acd3d3114");

        /// <summary>
        /// Identificador del servicio web (cuando corresponda)
        /// </summary>
        public Guid ApiId { set; get; }

        /// <summary>
        /// Ip del cliente
        /// </summary>
        public string IpCliente { set; get; }

        /// <summary>
        /// Nombre del Usuario, ej: jperez
        /// </summary>
        public string Usuario { set; get; }

        /// <summary>
        /// Id del Usuario, ej: 1
        /// </summary>
        public int UsuarioId { set; get; }


        /// <summary>
        /// Nombre del rol del usuario
        /// </summary>
        public string Role { set; get; }


        /// <summary>
        /// Id de la entidad o compañia
        /// </summary>
        public string CiaId { set; get; }

        /// <summary>
        /// Nombre de la entidad o compañia
        /// </summary>
        public string Cia { set; get; }

        /// <summary>
        /// Atributo que permite realizar conversión de objetos a formato json
        /// </summary>
        public readonly JsonSerializerSettings SerializerSettings;

        /// <summary>
        /// Constructor que inicializa los parametros para los logs de auditoria
        /// </summary>
        /// <param name="settings">Objeto de la clase BO.Aps.AdmApp.BL.Data.Settings</param>
        /// <param name="tokenString">String que contiene una cadena con formato Json Web Token (JWT)</param>
        /// <param name="ipCliente">String con el ip del cliente</param>
        public Base(SVD.Models.Settings settings, string tokenString, string ipCliente)
        {
            string message = "";
            IpCliente = ipCliente;
            AppGuid = new Guid(settings.AppGuid);

            BO.Aps.Auth.Models.Payload payload = BO.Aps.Auth.Token.GetPayload(tokenString, settings.TokenIssuer, settings.TokenAudience, settings.TokenSigningKey, ref message);

            if (message != "")
            {
                //AddLog(Auditoria.Data.LogData.TipoOperaciones.Lectura, typeof(Base), "Base", null);
                throw new Exception(message);
            }
            Usuario = payload.sub;
            UsuarioId = Convert.ToInt32(payload.uid);
            Cia = payload.cia;
            CiaId = payload.cid;
            Role = payload.role;

            SerializerSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };
        }

        /// <summary>
        /// Registra una pista de auditoría
        /// </summary>
        /// <param name="tipo">Enum Auditoria.Data.LogData.TipoOperaciones {Lectura,Alta,Baja,Modificacion,Ingreso,Salida}</param>
        /// <param name="method"></param>
        /// <param name="classType"></param>
        /// <param name="data"></param>
        public void AddLog(BO.Aps.Auditoria.Log.TipoOperaciones tipo, Type classType, string method, object data)
        {
            var json = JsonConvert.SerializeObject(data, SerializerSettings);
            BO.Aps.Auditoria.Log.AddLog(AppGuid, ApiId, IpCliente, Usuario, tipo, classType, method, json);
        }
    }
}
