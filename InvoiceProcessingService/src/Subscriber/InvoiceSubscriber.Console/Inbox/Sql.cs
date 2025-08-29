namespace InvoiceSubscriber.Console.Inbox
{
	internal static class Sql
	{
		public const string CreateTable = @"
			CREATE TABLE IF NOT EXISTS ProcessedMessages (
					MessageId   TEXT PRIMARY KEY,
					ProcessedAt TEXT NOT NULL
			);";

		public const string Exists = @"
			SELECT 1
			FROM ProcessedMessages
			WHERE MessageId = $id
			LIMIT 1;";

		public const string InsertIfNotExists = @"
			INSERT OR IGNORE INTO ProcessedMessages (MessageId, ProcessedAt)
			VALUES ($id, $ts);";
	}
}
