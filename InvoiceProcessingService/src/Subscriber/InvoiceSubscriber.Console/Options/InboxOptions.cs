using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSubscriber.Console.Options
{
	public class InboxOptions
	{
		public string ConnectionString { get; set; } = "Data Source=./data/inbox.db;Cache=Shared";
	}
}
