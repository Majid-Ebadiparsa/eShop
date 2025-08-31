using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using InvoiceSubscriber.Console.Abstractions;

namespace InvoiceSubscriber.Console.Infrastructure.Inbox
{
	public sealed class SqliteInboxStore : IInboxStore, IDisposable
	{
		private readonly string _connectionString;

		public SqliteInboxStore(string connectionString)
		{
			if (string.IsNullOrWhiteSpace(connectionString))
				throw new ArgumentException("Connection string is required.", nameof(connectionString));

			_connectionString = connectionString;
			InitializeSchema();
		}

		public async Task<bool> ExistsAsync(string messageId, CancellationToken ct = default)
		{
			var key = NormalizeKey(messageId);
			await using var con = new SqliteConnection(_connectionString);
			await con.OpenAsync(ct);

			await using var cmd = con.CreateCommand();
			cmd.CommandText = @"SELECT 1 FROM ProcessedMessages WHERE MessageId = $id LIMIT 1;";
			cmd.Parameters.AddWithValue("$id", key);

			var result = await cmd.ExecuteScalarAsync(ct);
			return result != null;
		}

		public async Task MarkProcessedAsync(string messageId, CancellationToken ct = default)
		{
			var key = NormalizeKey(messageId);
			var ts = DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture);

			await using var con = new SqliteConnection(_connectionString);
			await con.OpenAsync(ct);

			await using var cmd = con.CreateCommand();
			cmd.CommandText = @"
                INSERT OR IGNORE INTO ProcessedMessages (MessageId, ProcessedAt)
                VALUES ($id, $ts);";
			cmd.Parameters.AddWithValue("$id", key);
			cmd.Parameters.AddWithValue("$ts", ts);

			await cmd.ExecuteNonQueryAsync(ct);
		}

		public void Dispose() { }

		private void InitializeSchema()
		{
			using var con = new SqliteConnection(_connectionString);
			con.Open();

			using var cmd = con.CreateCommand();
			cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS ProcessedMessages (
                    MessageId   TEXT PRIMARY KEY,
                    ProcessedAt TEXT NOT NULL
                );";
			cmd.ExecuteNonQuery();
		}

		private static string NormalizeKey(string messageId)
		{
			if (string.IsNullOrWhiteSpace(messageId))
				throw new ArgumentException("MessageId cannot be null or empty.", nameof(messageId));
			return messageId.Trim();
		}
	}
}
