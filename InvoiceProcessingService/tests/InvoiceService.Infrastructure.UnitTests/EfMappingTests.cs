using FluentAssertions;
using InvoiceService.Domain.Entities;
using InvoiceService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InvoiceService.Infrastructure.UnitTests
{
	public class EfMappingTests
	{
		[Fact]
		public async Task Save_Invoice_With_Lines_Then_Cascade_Delete()
		{
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
					.UseSqlite("DataSource=:memory:")
					.Options;

			await using var db = new ApplicationDbContext(options);
			await db.Database.OpenConnectionAsync();
			await db.Database.EnsureCreatedAsync();

			var inv = new Invoice("desc", DateTime.UtcNow.AddDays(1), "ACME",
					new[] { new InvoiceLine("A", 1, 1), new InvoiceLine("B", 2, 2) });

			db.Invoices.Add(inv);
			await db.SaveChangesAsync();

			var stored = await db.Invoices.Include("_lines").FirstAsync();
			stored.Lines.Should().HaveCount(2);

			db.Remove(stored);
			await db.SaveChangesAsync();

			(await db.InvoiceLines.CountAsync()).Should().Be(0);
		}
	}
}
