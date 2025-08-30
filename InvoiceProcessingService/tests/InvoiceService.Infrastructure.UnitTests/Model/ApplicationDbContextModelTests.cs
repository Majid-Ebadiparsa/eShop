using FluentAssertions;
using InvoiceService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InvoiceService.Infrastructure.UnitTests.Model
{
	public class ApplicationDbContextModelTests
	{
		private static ApplicationDbContext CreateContext()
		{
			var opts = new DbContextOptionsBuilder<ApplicationDbContext>()
					.UseSqlite("Data Source=:memory:")
					.Options;

			var ctx = new ApplicationDbContext(opts);
			// Open SQLite in-memory connection to keep schema lifetime within this context
			ctx.Database.OpenConnection();
			return ctx;
		}

		[Fact]
		public void Model_Should_Contain_Invoice_And_Line_Entities()
		{
			using var ctx = CreateContext();
			var model = ctx.Model;

			model.FindEntityType(typeof(InvoiceService.Domain.Entities.Invoice)).Should().NotBeNull();
			model.FindEntityType(typeof(InvoiceService.Domain.Entities.InvoiceLine)).Should().NotBeNull();
		}

		[Fact]
		public void Invoice_Mapping_Should_Enforce_Constraints_And_Relationships()
		{
			using var ctx = CreateContext();
			var model = ctx.Model;
			var invoiceType = model.FindEntityType(typeof(InvoiceService.Domain.Entities.Invoice))!;

			var description = invoiceType.FindProperty("Description")!;
			description.IsNullable.Should().BeFalse();
			description.GetMaxLength().Should().Be(500);

			var supplier = invoiceType.FindProperty("Supplier")!;
			supplier.IsNullable.Should().BeFalse();
			supplier.GetMaxLength().Should().Be(200);

			var dueDate = invoiceType.FindProperty("DueDate")!;
			dueDate.IsNullable.Should().BeFalse();

			// Relationship: Invoice has many Lines with Cascade delete, field-backed collection
			var linesNav = invoiceType.FindNavigation("Lines")!;
			linesNav.ForeignKey.DeleteBehavior.Should().Be(DeleteBehavior.Cascade);

			linesNav.GetPropertyAccessMode().Should().Be(PropertyAccessMode.Field);
			linesNav.FieldInfo.Should().NotBeNull();                               
			linesNav.FieldInfo!.Name.Should().Be("_lines");                        

			linesNav.PropertyInfo.Should().NotBeNull();
		}

		[Fact]
		public void InvoiceLine_Mapping_Should_Enforce_Constraints()
		{
			using var ctx = CreateContext();
			var model = ctx.Model;
			var lineType = model.FindEntityType(typeof(InvoiceService.Domain.Entities.InvoiceLine))!;

			var description = lineType.FindProperty("Description")!;
			description.IsNullable.Should().BeFalse();
			description.GetMaxLength().Should().Be(500);

			var price = lineType.FindProperty("Price")!;
			price.IsNullable.Should().BeFalse();
			price.GetPrecision().Should().Be(18);
			price.GetScale().Should().Be(2);

			var quantity = lineType.FindProperty("Quantity")!;
			quantity.IsNullable.Should().BeFalse();

			var invoiceId = lineType.FindProperty("InvoiceId")!;
			invoiceId.IsNullable.Should().BeFalse();

			// Index on InvoiceId
			lineType.GetIndexes().Any(i => i.Properties.Any(p => p.Name == "InvoiceId")).Should().BeTrue();
		}
	}
}
