using Microsoft.Data.SqlClient;
using System.Data;

namespace ExpenseManagementAPI.Data
{
    /// <summary>
    /// Database Helper class for SQL Server connection management
    /// Uses ADO.NET (SqlConnection, SqlCommand, SqlDataReader, SqlDataAdapter)
    /// Provides reusable methods for database operations
    /// </summary>
    public class DatabaseHelper
    {
        private readonly string _connectionString;

        /// <summary>
        /// Constructor - Initializes with connection string from appsettings.json
        /// </summary>
        public DatabaseHelper(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        /// <summary>
        /// Get SQL Server connection
        /// </summary>
        public SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        /// <summary>
        /// Execute SELECT query and return SqlDataReader
        /// Use this for reading data (SELECT statements)
        /// IMPORTANT: Caller must close the connection after use
        /// </summary>
        public SqlDataReader ExecuteReader(string query, Dictionary<string, object>? parameters = null)
        {
            var connection = GetConnection();
            var command = new SqlCommand(query, connection);
            command.CommandType = CommandType.Text;

            // Add parameters if provided
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                }
            }

            connection.Open();
            return command.ExecuteReader(CommandBehavior.CloseConnection);
        }

        /// <summary>
        /// Execute INSERT, UPDATE, DELETE queries
        /// Returns number of affected rows
        /// </summary>
        public int ExecuteNonQuery(string query, Dictionary<string, object>? parameters = null)
        {
            using var connection = GetConnection();
            using var command = new SqlCommand(query, connection);
            command.CommandType = CommandType.Text;

            // Add parameters if provided
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                }
            }

            connection.Open();
            return command.ExecuteNonQuery();
        }

        /// <summary>
        /// Execute INSERT query and return the newly inserted ID (SCOPE_IDENTITY())
        /// Use this for INSERT operations where you need the generated ID
        /// </summary>
        public int ExecuteInsertWithId(string query, Dictionary<string, object>? parameters = null)
        {
            using var connection = GetConnection();
            using var command = new SqlCommand(query + "; SELECT CAST(SCOPE_IDENTITY() AS INT);", connection);
            command.CommandType = CommandType.Text;

            // Add parameters if provided
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                }
            }

            connection.Open();
            var result = command.ExecuteScalar();
            return result != null ? Convert.ToInt32(result) : 0;
        }

        /// <summary>
        /// Execute scalar query (returns single value)
        /// Use this for COUNT, SUM, AVG, MAX, MIN queries
        /// </summary>
        public object? ExecuteScalar(string query, Dictionary<string, object>? parameters = null)
        {
            using var connection = GetConnection();
            using var command = new SqlCommand(query, connection);
            command.CommandType = CommandType.Text;

            // Add parameters if provided
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                }
            }

            connection.Open();
            return command.ExecuteScalar();
        }

        /// <summary>
        /// Execute query and return DataTable
        /// Use this when you need all data at once (not recommended for large datasets)
        /// </summary>
        public DataTable ExecuteDataTable(string query, Dictionary<string, object>? parameters = null)
        {
            using var connection = GetConnection();
            using var command = new SqlCommand(query, connection);
            command.CommandType = CommandType.Text;

            // Add parameters if provided
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                }
            }

            using var adapter = new SqlDataAdapter(command);
            var dataTable = new DataTable();
            adapter.Fill(dataTable);
            return dataTable;
        }

        /// <summary>
        /// Test database connection
        /// Returns true if connection successful, false otherwise
        /// </summary>
        public bool TestConnection()
        {
            try
            {
                using var connection = GetConnection();
                connection.Open();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Execute multiple queries in a transaction
        /// All queries must succeed, otherwise all will be rolled back
        /// </summary>
        public bool ExecuteTransaction(List<(string query, Dictionary<string, object>? parameters)> queries)
        {
            using var connection = GetConnection();
            connection.Open();

            using var transaction = connection.BeginTransaction();
            try
            {
                foreach (var (query, parameters) in queries)
                {
                    using var command = new SqlCommand(query, connection, transaction);
                    command.CommandType = CommandType.Text;

                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                        }
                    }

                    command.ExecuteNonQuery();
                }

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                return false;
            }
        }

        /// <summary>
        /// Check if a record exists based on query
        /// Returns true if at least one row exists
        /// </summary>
        public bool RecordExists(string query, Dictionary<string, object>? parameters = null)
        {
            var result = ExecuteScalar(query, parameters);
            return result != null && Convert.ToInt32(result) > 0;
        }

        /// <summary>
        /// Get count of records
        /// </summary>
        public int GetCount(string query, Dictionary<string, object>? parameters = null)
        {
            var result = ExecuteScalar(query, parameters);
            return result != null ? Convert.ToInt32(result) : 0;
        }

        /// <summary>
        /// Get sum of a column
        /// </summary>
        public decimal GetSum(string query, Dictionary<string, object>? parameters = null)
        {
            var result = ExecuteScalar(query, parameters);
            return result != null && result != DBNull.Value ? Convert.ToDecimal(result) : 0;
        }
    }
}