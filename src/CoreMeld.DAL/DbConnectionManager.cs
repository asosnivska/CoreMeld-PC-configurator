using Npgsql;

namespace CoreMeld.DAL;

public class DbConnectionManager
{
    private readonly string _connectionString;

    public DbConnectionManager(string connectionString)
    {
        _connectionString = connectionString;
    }

    public NpgsqlConnection CreateConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }
}
