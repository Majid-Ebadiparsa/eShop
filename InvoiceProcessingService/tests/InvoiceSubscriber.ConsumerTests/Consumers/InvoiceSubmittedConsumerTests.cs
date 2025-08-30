using FluentAssertions;
using InvoiceSubscriber.Console.Messaging.Consumers;
using InvoiceSubscriber.ConsumerTests.Fakes;
using MassTransit;
using Moq;
using Shared.Contracts.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using SysConsole = System.Console;

namespace InvoiceSubscriber.ConsumerTests.Consumers
{
	public class InvoiceSubmittedConsumerTests
	{
		private static (InvoiceSubmittedConsumer sut, FakeInboxStore inbox, TestLogger<InvoiceSubmittedConsumer> logger)
				CreateSut()
		{
			var inbox = new FakeInboxStore();
			var logger = new TestLogger<InvoiceSubmittedConsumer>();
			var sut = new InvoiceSubmittedConsumer(inbox, logger);
			return (sut, inbox, logger);
		}

		private static ConsumeContext<InvoiceSubmitted> BuildContext(InvoiceSubmitted msg, Guid? msgId = null, Guid? corrId = null, CancellationToken? ct = null)
		{
			var mock = new Mock<ConsumeContext<InvoiceSubmitted>>();
			mock.SetupGet(c => c.Message).Returns(msg);
			mock.SetupGet(c => c.MessageId).Returns(msgId);
			mock.SetupGet(c => c.CorrelationId).Returns(corrId);
			mock.SetupGet(c => c.CancellationToken).Returns(ct ?? CancellationToken.None);
			return mock.Object;
		}

		[Fact]
		public async Task HappyPath_With_MessageId_Should_MarkProcessed_And_Print()
		{
			var (sut, inbox, logger) = CreateSut();
			var evt = EventFactory.CreateInvoice(lines: new[] { EventFactory.Line("Paper", 5.99, 10) });
			var msgId = Guid.NewGuid();

			var ctx = BuildContext(evt, msgId: msgId);

			var sw = new StringWriter();
			var original = SysConsole.Out;
			SysConsole.SetOut(sw);
			try
			{
				await sut.Consume(ctx);
			}
			finally
			{
				SysConsole.SetOut(original);
			}

			var expectedKey = msgId.ToString("D");
			inbox.LastExistsKey.Should().Be(expectedKey);
			inbox.LastProcessedKey.Should().Be(expectedKey);
			inbox.WasMarkedProcessed(expectedKey).Should().BeTrue();

			// Console should contain a summary
			sw.ToString().Should().Contain("Invoice:")
									 .And.Contain(evt.Description)
									 .And.Contain(evt.Supplier);
		}

		[Fact]
		public async Task Duplicate_Message_Should_Be_Skipped_And_Not_Marked_Processed()
		{
			var (sut, inbox, logger) = CreateSut();
			var evt = EventFactory.CreateInvoice();
			var msgId = Guid.NewGuid();
			inbox.ExistsReturnValue = true; // simulate duplicate

			var ctx = BuildContext(evt, msgId: msgId);

			var sw = new StringWriter();
			var original = SysConsole.Out;
			SysConsole.SetOut(sw);
			try
			{
				await sut.Consume(ctx);
			}
			finally
			{
				SysConsole.SetOut(original);
			}

			var expectedKey = msgId.ToString("D");
			inbox.LastExistsKey.Should().Be(expectedKey);
			inbox.WasMarkedProcessed(expectedKey).Should().BeFalse();
			inbox.LastProcessedKey.Should().BeNull();

			// Should not print anything on duplicate path
			sw.ToString().Should().BeNullOrEmpty();
		}

		[Fact]
		public async Task Should_Handle_Null_Lines_Safely_And_Print_Count_Zero()
		{
			var (sut, inbox, logger) = CreateSut();
			var evt = EventFactory.CreateInvoice(lines: null); // Lines is empty list in factory
			evt.Lines = null!; // force null to test null-guard branch

			var ctx = BuildContext(evt, msgId: Guid.NewGuid());

			var sw = new StringWriter();
			var original = SysConsole.Out;
			SysConsole.SetOut(sw);
			try
			{
				await sut.Consume(ctx);
			}
			finally
			{
				SysConsole.SetOut(original);
			}

			var output = sw.ToString();
			output.Should().Contain("Lines: 0");
		}

		[Fact]
		public async Task Key_Should_Fall_Back_To_CorrelationId_When_MessageId_Is_Null()
		{
			var (sut, inbox, logger) = CreateSut();
			var evt = EventFactory.CreateInvoice();
			Guid? msgId = null;
			var corrId = Guid.NewGuid();

			var ctx = BuildContext(evt, msgId: msgId, corrId: corrId);
			await sut.Consume(ctx);

			inbox.LastExistsKey.Should().Be(corrId.ToString("D"));
			inbox.LastProcessedKey.Should().Be(corrId.ToString("D"));
		}

		[Fact]
		public async Task Key_Should_Fall_Back_To_InvoiceId_When_No_Ids_Present()
		{
			var (sut, inbox, logger) = CreateSut();
			var id = Guid.NewGuid();
			var evt = EventFactory.CreateInvoice(id: id);

			var ctx = BuildContext(evt, msgId: null, corrId: null);
			await sut.Consume(ctx);

			var expected = $"InvoiceSubmitted::{id}";
			inbox.LastExistsKey.Should().Be(expected);
			inbox.LastProcessedKey.Should().Be(expected);
		}

		[Fact]
		public async Task Should_Skip_Null_Line_Items_Without_Throwing()
		{
			var (sut, inbox, logger) = CreateSut();
			var evt = EventFactory.CreateInvoice(lines: new InvoiceLineItem[] { null, EventFactory.Line("Pen", 1.99, 5), null });

			var ctx = BuildContext(evt, msgId: Guid.NewGuid());

			Func<Task> act = async () => await sut.Consume(ctx);
			await act.Should().NotThrowAsync();

			// Expect processed with correct key
			inbox.LastProcessedKey.Should().NotBeNullOrWhiteSpace();
		}
	}
}
