using Npgsql;
using System.Data;
using System.Data.SqlClient;
// using Microsoft.Extensions.Configuration;

namespace SVD.Models
{
    public class BdPG
    {
        private IDbConnection _instancia;
        // IConfiguration config;
        public BdPG()
        {
            if (this._instancia == null)
            {
                // string conectionString = config.GetValue<string>("DBInfo:ConnectionString");
                this._instancia = new NpgsqlConnection(Config.connectionStringPG);
                this._instancia.Open();
            }
        }

        public static IDbConnection instancia()
        {
            BdPG bd = new BdPG();
            return bd._instancia;
        }
    }

    public class BdSegurosSQL
    {
        private IDbConnection _instancia;
        // IConfiguration config;
        public BdSegurosSQL()
        {
            if (this._instancia == null)
            {
                // string conectionString = config.GetValue<string>("DBInfo:ConnectionString");
                this._instancia = new SqlConnection(Config.connStrDbSegurosSQL);
                this._instancia.Open();
            }
        }

        public static IDbConnection instancia()
        {
            BdSegurosSQL bd = new BdSegurosSQL();
            return bd._instancia;
        }
    }

    public class BdSegurosOLAPSQL
    {
        private IDbConnection _instancia;
        // IConfiguration config;
        public BdSegurosOLAPSQL()
        {
            if (this._instancia == null)
            {
                // string conectionString = config.GetValue<string>("DBInfo:ConnectionString");
                this._instancia = new SqlConnection(Config.connStrSegurosOlapSQL);
                this._instancia.Open();
            }
        }

        public static IDbConnection instancia()
        {
            BdSegurosOLAPSQL bd = new BdSegurosOLAPSQL();
            return bd._instancia;
        }
    }
}