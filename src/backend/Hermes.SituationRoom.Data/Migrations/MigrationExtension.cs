namespace Hermes.SituationRoom.Data.Migrations;

using System.Data;
using System.Data.SqlClient;
using EvolveDb;
using Microsoft.Extensions.DependencyInjection;

public static class MigrationExtension
{
    public static IServiceCollection AddDatabaseMigrations(this IServiceCollection services,
        string? connectionString,
        params string[] locations
    )
    {
        try
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            var databaseName = builder.InitialCatalog;

            // use master first
            var masterBuilder = new SqlConnectionStringBuilder(connectionString)
            {
                InitialCatalog = "master"
            };

            // wait until SQL Server is ready
            WaitForSqlServer(masterBuilder.ConnectionString);

            using (var masterConnection = new SqlConnection(masterBuilder.ConnectionString))
            {
                masterConnection.Open();

                var cmd = masterConnection.CreateCommand();
                cmd.CommandText = $"SELECT db_id('{databaseName}')";
                var result = cmd.ExecuteScalar();
                var dbExists = result != DBNull.Value && result != null;

                if (!dbExists)
                {
                    var createCmd = masterConnection.CreateCommand();
                    createCmd.CommandText = $"CREATE DATABASE [{databaseName}]";
                    createCmd.ExecuteNonQuery();
                }
            }

            // run migrations on the target DB
            using (var sqlConnection = new SqlConnection(connectionString))
            {
                var evolve = new Evolve(sqlConnection)
                {
                    Schemas = ["dbo"],
                    Locations = locations,
                    IsEraseDisabled = true,
                    MetadataTableName = "Changelog",
                    MetadataTableSchema = "dbo",
                };

                evolve.Migrate();
            }
        }
        catch (Exception ex)
        {
            throw new DataException($"Error while migrating database. Connection String: {connectionString}", ex);
        }

        return services;
    }

    private static void WaitForSqlServer(string connectionString, int maxRetries = 10, int delaySeconds = 3)
    {
        var retries = 0;
        while (true)
        {
            try
            {
                using var conn = new SqlConnection(connectionString);
                conn.Open();
                return; // success
            }
            catch (SqlException)
            {
                retries++;
                Console.WriteLine($"[MigrationExtension] SQL Server not ready yet... retry {retries}/{maxRetries}");
                if (retries >= maxRetries)
                    throw;
                Thread.Sleep(TimeSpan.FromSeconds(delaySeconds));
            }
        }
    }
}