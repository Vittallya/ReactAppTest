using LiteDB;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using TestApp.Server.Infrastructure;

public class DbService(IServiceScopeFactory factory, IConfiguration configuration)
{
    //static соединение для более быстрй работы (специфика sqlite)
    private DbConnection _connection = null!;

    public void InitDb()
    {        
        var connectionString = configuration.GetConnectionString("Main");

        _connection = new SqliteConnection(connectionString);
        _connection.Open();

        using (var command = _connection.CreateCommand())
        {
            //wal режим для бд
            command.CommandText = "PRAGMA journal_mode=WAL; PRAGMA synchronous=NORMAL;";
            command.ExecuteNonQuery();
        }

        using var scope = factory.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();

        _ = db.Model;
        _ = db.Set<Order>().AsNoTracking().FirstOrDefault(x => x.Id == 1);
    }
}