using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Linq;

namespace InvoiceSubscriber.Console.Inbox
{
	public sealed class SqliteInboxOptions
	{
		public string ConnectionString { get; private set; }
		public TimeSpan BusyTimeout { get; set; } = TimeSpan.FromSeconds(5);
		public string JournalMode { get; set; } = "WAL";

		public SqliteInboxOptions(string connectionString)
		{
			if (string.IsNullOrWhiteSpace(connectionString))
				throw new ArgumentException("Connection string is required.", nameof(connectionString));

			ConnectionString = NormalizeAndEnsureDirectory(connectionString);
		}

		public static SqliteInboxOptions FromConfiguration(
				 IConfiguration cfg,
				 IHostEnvironment env,
				 string sectionName = "InboxDb",
				 Action<SqliteInboxOptions> configure = null)
		{
			var raw = cfg.GetConnectionString(sectionName) ?? "Data Source=../data/inbox.db";

			var resolved = ResolveConnectionString(raw, env);

			var options = new SqliteInboxOptions(resolved)
			{
				BusyTimeout = TimeSpan.FromSeconds(5),
				JournalMode = "WAL"
			};

			if (configure != null)
				configure(options);

			return options;
		}

		private static string NormalizeAndEnsureDirectory(string raw)
		{
			var csb = new SqliteConnectionStringBuilder(raw);

			// sane defaults
			if (!csb.ContainsKey("Mode")) csb.Mode = SqliteOpenMode.ReadWriteCreate;
			if (!csb.ContainsKey("Cache")) csb.Cache = SqliteCacheMode.Shared;			

			var dbPath = Path.GetFullPath(csb.DataSource);
			var dir = Path.GetDirectoryName(dbPath);
			if (!string.IsNullOrEmpty(dir))
				Directory.CreateDirectory(dir!);

			csb.DataSource = dbPath;
			return csb.ToString();
		}

		private static string ResolveConnectionString(string raw, IHostEnvironment env)
		{
			var csb = new SqliteConnectionStringBuilder(raw);
			var dataSource = csb.DataSource;

			if (Path.IsPathRooted(dataSource))
			{
				EnsureDirectoryFor(dataSource);
				if (!csb.ContainsKey("Mode")) csb.Mode = SqliteOpenMode.ReadWriteCreate;
				if (!csb.ContainsKey("Cache")) csb.Cache = SqliteCacheMode.Shared;
				return csb.ToString();
			}

			var inContainer = IsRunningInContainer();

			string rootedPath = GetRootedPath(env, dataSource, inContainer);
			EnsureDirectoryFor(rootedPath);
			csb.DataSource = rootedPath;

			if (!csb.ContainsKey("Mode")) csb.Mode = SqliteOpenMode.ReadWriteCreate;
			if (!csb.ContainsKey("Cache")) csb.Cache = SqliteCacheMode.Shared;

			return csb.ToString();
		}

		private static void EnsureDirectoryFor(string filePath)
		{
			var dir = Path.GetDirectoryName(filePath);
			if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
				Directory.CreateDirectory(dir);
		}

		private static string GetRootedPath(IHostEnvironment env, string dataSource, bool inContainer)
		{
			if (inContainer)
			{
				return Path.GetFullPath(Path.Combine("/app/data", dataSource.TrimStart('.', '/', '\\')));
			}
			else
			{
				var projectRoot = FindProjectRoot(env.ContentRootPath) ?? env.ContentRootPath;
				return Path.GetFullPath(Path.Combine(projectRoot, dataSource));
			}
		}

		private static string FindProjectRoot(string start)
		{
			var dir = new DirectoryInfo(start);
			while (dir != null)
			{
				if (dir.EnumerateFiles("*.csproj", SearchOption.TopDirectoryOnly).Any())
					return dir.FullName;
				dir = dir.Parent;
			}
			return start;
		}

		private static bool IsRunningInContainer()
		{
			return string.Equals(
					Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"),
					"true",
					StringComparison.OrdinalIgnoreCase);
		}
	}
}
