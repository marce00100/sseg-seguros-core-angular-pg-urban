using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace SVD
{
    public class Config
    {
        public static string connectionStringPG = Config.configAppSetting().GetValue<string>("DBPostgres:ConnectionString"); // "User ID=postgres;Password=6656;Host=localhost;Port=5432;Database=segurosDB;CommandTimeout=30;";
        public static string connStrDbSegurosSQL = Config.configAppSetting().GetValue<string>("DBSql:ConnectionString"); // "server=(local);database=dbSeguros_rabbit;integrated security = yes;connection timeout=120;";
        public static string connStrSegurosOlapSQL = Config.configAppSetting().GetValue<string>("DBSqlOLAP:ConnectionString"); // "server=(local);database=dbSegurosOlap_rabbit;integrated security = yes;connect timeout=120;";

        //Variables de estado ----------------
        public static bool habilitarHistoricos = false;
        public static string[] estadosApertura = { "no existe", "iniciado", "detenido", "concluido" };
        public static string[] estadosSeguimientoEnvio = { "No Enviado", "E. Grave", "Error", "Advertencia", "Valido" };
        public static string[] estadosCierre = { "ninguno", "consolidado", "en confirmacion", "cerrado" };



        // Determina si se deben poblar las tablas (true) o no (false) de la base de datos en SQL de la base de datos dbSeguros
        // Una vez que se tenga la base de dtaos completa en Postgres y no dependan otros sistemas de SQL esta opcion se debe colocar a False
        public static bool poblarSQLTablasDbSeguros = true;

        // Determina si se deben poblar las tablas (true) o no (false) de la base de datos en SQL de la base de datos dbSeguros_OLAP
        // Una vez que se tenga la base de dtaos completa en Postgres y no dependan otros sistemas de SQL esta opcion se debe colocar a False
        public static bool poblarSQLTablasDbSegurosOLAP = true;

        // Determina si se deben poblar las tablas (true) o no (false) de la base de datos Postgres para cierre 
        // por motivos de velocidad es mejor que esta opcion este e apagada mientras no se tenga completamente funcional y migrada  la base de datos en POstgres 
        public static bool poblarPGTablasdeCierre = true;

        // Determina si se deben poblar las tablas (true) o no (false) de la base de datos en  POSTGREs de la base de datos dbSeguros_OLAP
        // por motivos de velocidad es mejor que esta opcion este e apagada mientras no se tenga completamente funcional y migrada  la base de datos en POstgres 
        public static bool poblarPGTablasDbSegurosOLAP = true;

        public static IConfigurationRoot configAppSetting()
        {
            var builder = new ConfigurationBuilder()
                                .SetBasePath(Directory.GetCurrentDirectory())
                                .AddJsonFile("appsettings.json");

            return builder.Build();
        }

    }
}