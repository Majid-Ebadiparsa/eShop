using Microsoft.Data.Sqlite;

namespace InvoiceSubscriber.Console.Inbox
{
	public sealed class SqliteInboxOptions
	{
		public string ConnectionString { get; init; }

		public TimeSpan BusyTimeout { get; init; } = TimeSpan.FromSeconds(5);

		public string JournalMode { get; init; } = "WAL";

		public SqliteInboxOptions(string connectionString)
		{
			if (string.IsNullOrWhiteSpace(connectionString))
				throw new ArgumentException("Connection string is required.", nameof(connectionString));

			ConnectionString = NormalizeAndEnsureDirectory(connectionString);
		}

		private static string NormalizeAndEnsureDirectory(string raw)
		{
			var csb = new SqliteConnectionStringBuilder(raw);

			// sane defaults
			if (!csb.ContainsKey("Mode")) csb.Mode = SqliteOpenMode.ReadWriteCreate;
			if (!csb.ContainsKey("Cache")) csb.Cache = SqliteCacheMode.Shared;
			if (!csb.ContainsKey("Pooling")) csb.Pooling = true;

			var dbPath = Path.GetFullPath(csb.DataSource);
			var dir = Path.GetDirectoryName(dbPath);
			if (!string.IsNullOrEmpty(dir))
				Directory.CreateDirectory(dir!);

			return csb.ToString();
		}
	}
}
