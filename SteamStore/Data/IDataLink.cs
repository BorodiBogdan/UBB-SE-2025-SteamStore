using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamStore.Data
{
    public interface IDataLink
    {
        void OpenConnection();
        void CloseConnection();
        T? ExecuteScalar<T>(string storedProcedure, SqlParameter[]? sqlParameters = null);
        DataTable ExecuteReader(string storedProcedure, SqlParameter[]? sqlParameters = null);

        int ExecuteNonQuery(string storedProcedure, SqlParameter[]? sqlParameters = null);
    }
}
