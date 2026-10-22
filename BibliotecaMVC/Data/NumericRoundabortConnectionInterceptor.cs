
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BibliotecaMVC.Data
{
    public sealed class NumericRoundabortConnectionInterceptor : DbConnectionInterceptor
    {
        private const string CommandText = "SET NUMERIC_ROUNDABORT OFF;";

        public override void ConnectionOpened(
            DbConnection connection,
            ConnectionEndEventData eventData)
        {
            using var command = connection.CreateCommand();
            command.CommandText = CommandText;
            command.ExecuteNonQuery();

            base.ConnectionOpened(connection, eventData);
        }

        public override async Task ConnectionOpenedAsync(
            DbConnection connection,
            ConnectionEndEventData eventData,
            CancellationToken cancellationToken = default)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = CommandText;

            await command.ExecuteNonQueryAsync(cancellationToken);

            await base.ConnectionOpenedAsync(
                connection,
                eventData,
                cancellationToken);
        }
    }
}
