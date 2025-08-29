using Microsoft.Data.Sqlite;
using System.Globalization;

namespace InvoiceSubscriber.Console.Inbox
{
	public sealed class SqliteInboxStore : IInboxStore, IDisposable
	{
		private readonly string _connectionString;
		private readonly SqliteInboxOptions _options;

		public SqliteInboxStore(SqliteInboxOptions options)
		{
			_options = options ?? throw new ArgumentNullException(nameof(options));
			InitializeSchema(); // idempotent
		}

		public SqliteInboxStore(string connectionString)
				: this(new SqliteInboxOptions(connectionString))
		{ }

		public async Task<bool> ExistsAsync(string messageId, CancellationToken ct = default)
		{
			var key = NormalizeKey(messageId);

			await using var con = await CreateOpenConnectionAsync(ct);
			await ApplyPragmasAsync(con, ct);

			await using var cmd = con.CreateCommand();
			cmd.CommandText = Sql.Exists;
			cmd.Parameters.AddWithValue("$id", key);

			var result = await cmd.ExecuteScalarAsync(ct);
			return result != null;
		}

		public async Task MarkProcessedAsync(string messageId, CancellationToken ct = default)
		{
			var key = NormalizeKey(messageId);
			var timestamp = DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture);

			await using var con = await CreateOpenConnectionAsync(ct);
			await ApplyPragmasAsync(con, ct);

			await using var cmd = con.CreateCommand();
			cmd.CommandText = Sql.InsertIfNotExists;
			cmd.Parameters.AddWithValue("$id", key);
			cmd.Parameters.AddWithValue("$ts", timestamp);

			await cmd.ExecuteNonQueryAsync(ct);
		}

		public void Dispose() { }

		private void InitializeSchema()
		{
			using var con = new SqliteConnection(_options.ConnectionString);
			con.Open();

			using var pragmaTimeout = con.CreateCommand();
			pragmaTimeout.CommandText = $"PRAGMA busy_timeout = {(int)_options.BusyTimeout.TotalMilliseconds};";
			pragmaTimeout.ExecuteNonQuery();

			using var pragmaJournal = con.CreateCommand();
			pragmaJournal.CommandText = $"PRAGMA journal_mode = {_options.JournalMode};";
			pragmaJournal.ExecuteNonQuery();

			using var cmd = con.CreateCommand();
			cmd.CommandText = Sql.CreateTable;
			cmd.ExecuteNonQuery();
		}

		private async Task<SqliteConnection> CreateOpenConnectionAsync(CancellationToken ct)
		{
			var con = new SqliteConnection(_options.ConnectionString);
			await con.OpenAsync(ct);
			return con;
		}

		private async Task ApplyPragmasAsync(SqliteConnection con, CancellationToken ct)
		{
			await using var pragmaTimeout = con.CreateCommand();
			pragmaTimeout.CommandText = $"PRAGMA busy_timeout = {(int)_options.BusyTimeout.TotalMilliseconds};";
			await pragmaTimeout.ExecuteNonQueryAsync(ct);

			await using var pragmaJournal = con.CreateCommand();
			pragmaJournal.CommandText = $"PRAGMA journal_mode = {_options.JournalMode};";
			await pragmaJournal.ExecuteNonQueryAsync(ct);
		}

		private static string NormalizeKey(string messageId)
		{
			if (string.IsNullOrWhiteSpace(messageId))
				throw new ArgumentException("MessageId cannot be null or empty.", nameof(messageId));
			
			return messageId.Trim();
		}
	}
}
