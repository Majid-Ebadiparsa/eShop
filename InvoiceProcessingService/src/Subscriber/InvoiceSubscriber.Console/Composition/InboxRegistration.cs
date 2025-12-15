using System;
using System.IO;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using InvoiceSubscriber.Console.Abstractions;
using InvoiceSubscriber.Console.Infrastructure.Inbox;
using InvoiceSubscriber.Console.Options;

namespace InvoiceSubscriber.Console.Composition
{
	public static class InboxRegistration
	{
		const string INBOX_DB_FILE = "inbox.db";
		const string INBOX_DB_FOLDER = "data";

		public static IServiceCollection AddInboxStore(this IServiceCollection services, IConfiguration cfg, IHostEnvironment env)
		{
			services.Configure<InboxOptions>(cfg.GetSection("ConnectionStrings"));

			services.PostConfigure<InboxOptions>(opt =>
			{
				var envConn = Environment.GetEnvironmentVariable("ConnectionStrings__InboxDb");
				if (!string.IsNullOrWhiteSpace(envConn))
					opt.ConnectionString = envConn;

				var csb = new SqliteConnectionStringBuilder(opt.ConnectionString);
				var dataSource = csb.DataSource;

				var fileName = string.IsNullOrWhiteSpace(dataSource)
											 ? INBOX_DB_FILE
											 : Path.GetFileName(dataSource);

				if (!Path.IsPathRooted(dataSource))
				{
					var basePath = AppContext.BaseDirectory;     
					var dataDir = Path.Combine(basePath, INBOX_DB_FOLDER);
																												
					Directory.CreateDirectory(dataDir);
					csb.DataSource = Path.Combine(dataDir, fileName);
				}
				else
				{
					if (string.IsNullOrWhiteSpace(fileName))
						csb.DataSource = Path.Combine(AppContext.BaseDirectory, INBOX_DB_FOLDER, INBOX_DB_FILE);
				}

				var targetFile = new FileInfo(csb.DataSource);
				targetFile.Directory?.Create();

				opt.ConnectionString = csb.ToString();
			});

			services.AddSingleton<IInboxStore>(sp =>
			{
				var options = sp.GetRequiredService<IOptions<InboxOptions>>().Value;
				return new SqliteInboxStore(options.ConnectionString);
			});

			return services;
		}
	}
}
