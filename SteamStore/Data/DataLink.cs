using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;

public class DataLink : SteamStore.Data.IDataLink
{
    private SqlConnection sqlConnection;
    private readonly string connectionString;

    public DataLink(IConfiguration configuration)
    {
        //string? localDataSource = configuration["LocalDataSource"];
        //connectionString = "Data Source=" + localDataSource + ";" +
        //                "Initial Catalog=SteamStore;" +
        //                "Integrated Security=True;";
        connectionString = "Server=DESKTOP-618UFK0\\SQLEXPRESS;Initial Catalog=Steam;Integrated Security=True;TrustServerCertificate=True";


        try
        {
            sqlConnection = new SqlConnection(connectionString);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error initializing SQL connection: {connectionString}", ex);
        }
    }

    public void OpenConnection()
    {
        if (sqlConnection.State != ConnectionState.Open)
        {
            sqlConnection.Open();
        }
    }

    public void CloseConnection()
    {
        if (sqlConnection.State != ConnectionState.Closed)
        {
            sqlConnection.Close();
        }
    }

    public T? ExecuteScalar<T>(string storedProcedure, SqlParameter[]? sqlParameters = null)
    {
        try
        {
            OpenConnection();
            using (SqlCommand command = new SqlCommand(storedProcedure, sqlConnection))
            {
                command.CommandType = CommandType.StoredProcedure;

                if (sqlParameters != null)
                {
                    command.Parameters.AddRange(sqlParameters);
                }

                var result = command.ExecuteScalar();
                if (result == DBNull.Value || result == null)
                {
                    return default;
                }

                return (T)Convert.ChangeType(result, typeof(T));
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error - ExecutingScalar: {ex.Message}");
        }
        finally
        {
            CloseConnection();
        }
    }

    public DataTable ExecuteReader(string storedProcedure, SqlParameter[]? sqlParameters = null)
    {
        try
        {
            OpenConnection();
            using (SqlCommand command = new SqlCommand(storedProcedure, sqlConnection))
            {
                command.CommandType = CommandType.StoredProcedure;

                if (sqlParameters != null)
                {
                    command.Parameters.AddRange(sqlParameters);
                }

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    DataTable dataTable = new DataTable();
                    dataTable.Load(reader);
                    return dataTable;
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error - ExecuteReader: {ex.Message}");
        }
        finally
        {
            CloseConnection();
        }
    }

    public int ExecuteNonQuery(string storedProcedure, SqlParameter[]? sqlParameters = null)
    {
        try
        {
            OpenConnection();
            using (SqlCommand command = new SqlCommand(storedProcedure, sqlConnection))
            {
                command.CommandType = CommandType.StoredProcedure;

                if (sqlParameters != null)
                {
                    command.Parameters.AddRange(sqlParameters);
                }

                return command.ExecuteNonQuery();
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error - ExecuteNonQuery: {ex.Message}");
        }
        finally
        {
            CloseConnection();
        }
    }
}