using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSubscriber.Console.Inbox
{
	public sealed class SqliteInboxStore : IInboxStore, IDisposable
	{
		private readonly string _connectionString;


		public SqliteInboxStore(string connectionString = "Data Source=inbox.db")
		{
			_connectionString = connectionString;
			using var con = new SqliteConnection(_connectionString);
			con.Open();
			using var cmd = con.CreateCommand();
			cmd.CommandText = @"CREATE TABLE IF NOT EXISTS ProcessedMessages (
														MessageId TEXT PRIMARY KEY,
														ProcessedAt TEXT NOT NULL
													);";
			cmd.ExecuteNonQuery();
		}


		public async Task<bool> ExistsAsync(string messageId, CancellationToken ct = default)
		{
			await using var con = new SqliteConnection(_connectionString);
			await con.OpenAsync(ct);
			await using var cmd = con.CreateCommand();
			cmd.CommandText = "SELECT 1 FROM ProcessedMessages WHERE MessageId = $id LIMIT 1";
			cmd.Parameters.AddWithValue("$id", messageId);
			var result = await cmd.ExecuteScalarAsync(ct);
			return result != null;
		}


		public async Task MarkProcessedAsync(string messageId, CancellationToken ct = default)
		{
			await using var con = new SqliteConnection(_connectionString);
			await con.OpenAsync(ct);
			await using var cmd = con.CreateCommand();
			cmd.CommandText = "INSERT OR IGNORE INTO ProcessedMessages (MessageId, ProcessedAt) VALUES ($id, $ts)";
			cmd.Parameters.AddWithValue("$id", messageId);
			cmd.Parameters.AddWithValue("$ts", DateTime.UtcNow.ToString("O"));
			await cmd.ExecuteNonQueryAsync(ct);
		}


		public void Dispose() { }
	}
}
