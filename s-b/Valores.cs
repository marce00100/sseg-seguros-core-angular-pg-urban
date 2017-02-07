namespace SVD
{
    public class Config
    {
        public static string connectionStringPG = "User ID=postgres;Password=6656;Host=localhost;Port=5432;Database=segurosDB;CommandTimeout=30;";
        public static string connStrDbSegurosSQL = "server=(local);database=dbSeguros_rabbit;integrated security = yes;connection timeout=120;";
        public static string connStrSegurosOlapSQL = "server=(local);database=dbSegurosOlap_rabbit;integrated security = yes;connect timeout=120;";

        //Variables de estado ----------------
        public static bool habilitarHistoricos = false;
        public static string[] estadosApertura = { "no existe", "iniciado", "detenido", "concluido" };
        public static string[] estadosSeguimientoEnvio = { "No Enviado", "E. Grave", "Error", "Advertencia", "Valido" };
        public static string[] estadosCierre = { "ninguno", "consolidado", "en confirmacion", "cerrado" };



        // Determina si se deben poblar las tablas (true) o no (false) de la base de datos en SQL de la base de datos dbSeguros
        public static bool poblarSQLTablasDbSeguros = true;

        // Determina si se deben poblar las tablas (true) o no (false) de la base de datos en SQL de la base de datos dbSeguros_OLAP
        public static bool poblarSQLTablasDbSegurosOLAP = true;

    }
}