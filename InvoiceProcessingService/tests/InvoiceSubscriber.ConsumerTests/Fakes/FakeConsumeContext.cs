//using GreenPipes;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace InvoiceSubscriber.ConsumerTests.Fakes
{
	public class FakeConsumeContext<T> : ConsumeContext<T> where T : class
	{
		public T Message { get; }
		public Guid? MessageId { get; set; }

		public FakeConsumeContext(T message)
		{
			Message = message;
		}

		public T MessageObject => Message;

		public CancellationToken CancellationToken => CancellationToken.None;

		#region NotImplemented members
		public Task NotifyConsumed(TimeSpan duration, string consumerType) => Task.CompletedTask;
		public Task NotifyFaulted(TimeSpan duration, string consumerType, Exception exception) => Task.CompletedTask;

		public Guid? CorrelationId => null;
		public Guid? ConversationId => null;
		public Guid? InitiatorId => null;
		public Guid? RequestId => null;
		public Guid? FaultMessageId => null;
		public Uri SourceAddress => null;
		public Uri DestinationAddress => null;
		public Uri ResponseAddress => null;
		public Uri FaultAddress => null;
		public Headers Headers => null;
		public HostInfo Host => null;
		public Task RespondAsync<TResponse>(TResponse message, IPipe<SendContext<TResponse>> sendPipe) where TResponse : class => Task.CompletedTask;
		public Task RespondAsync<TResponse>(TResponse message) where TResponse : class => Task.CompletedTask;
		public Task RespondAsync(object message) => Task.CompletedTask;
		public Task RespondAsync(object message, Type messageType) => Task.CompletedTask;
		public Task<ISendEndpoint> GetSendEndpoint(Uri address) => throw new NotImplementedException();
		public Task Publish<T1>(T1 message, CancellationToken cancellationToken) where T1 : class => Task.CompletedTask;
		public Task Publish<T1>(T1 message, IPipe<PublishContext<T1>> publishPipe, CancellationToken cancellationToken) where T1 : class => Task.CompletedTask;
		public Task Publish<T1>(object message, CancellationToken cancellationToken) where T1 : class => Task.CompletedTask;
		public Task Publish<T1>(object message, IPipe<PublishContext<T1>> publishPipe, CancellationToken cancellationToken) where T1 : class => Task.CompletedTask;
		public Task Publish(object message, CancellationToken cancellationToken) => Task.CompletedTask;
		public Task Publish(object message, Type messageType, CancellationToken cancellationToken) => Task.CompletedTask;
		public Task Publish(object message, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken) => Task.CompletedTask;
		public Task Publish(object message, Type messageType, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken) => Task.CompletedTask;
		public bool HasMessageType(Type messageType) => true;
		public bool TryGetMessage<T1>(out ConsumeContext<T1> message) where T1 : class
		{
			message = null;
			return false;
		}

		public void AddConsumeTask(Task task)
		{
			throw new NotImplementedException();
		}

		public Task RespondAsync<T1>(T1 message, IPipe<SendContext> sendPipe) where T1 : class
		{
			throw new NotImplementedException();
		}

		public Task RespondAsync(object message, IPipe<SendContext> sendPipe)
		{
			throw new NotImplementedException();
		}

		public Task RespondAsync(object message, Type messageType, IPipe<SendContext> sendPipe)
		{
			throw new NotImplementedException();
		}

		public Task RespondAsync<T1>(object values) where T1 : class
		{
			throw new NotImplementedException();
		}

		public Task RespondAsync<T1>(object values, IPipe<SendContext<T1>> sendPipe) where T1 : class
		{
			throw new NotImplementedException();
		}

		public Task RespondAsync<T1>(object values, IPipe<SendContext> sendPipe) where T1 : class
		{
			throw new NotImplementedException();
		}

		public void Respond<T1>(T1 message) where T1 : class
		{
			throw new NotImplementedException();
		}

		public Task NotifyConsumed<T1>(ConsumeContext<T1> context, TimeSpan duration, string consumerType) where T1 : class
		{
			throw new NotImplementedException();
		}

		public Task NotifyFaulted<T1>(ConsumeContext<T1> context, TimeSpan duration, string consumerType, Exception exception) where T1 : class
		{
			throw new NotImplementedException();
		}

		public bool HasPayloadType(Type payloadType)
		{
			throw new NotImplementedException();
		}

		public bool TryGetPayload<T1>(out T1 payload) where T1 : class
		{
			throw new NotImplementedException();
		}

		public T1 GetOrAddPayload<T1>(PayloadFactory<T1> payloadFactory) where T1 : class
		{
			throw new NotImplementedException();
		}

		public T1 AddOrUpdatePayload<T1>(PayloadFactory<T1> addFactory, UpdatePayloadFactory<T1> updateFactory) where T1 : class
		{
			throw new NotImplementedException();
		}

		public Task Publish<T1>(T1 message, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = default) where T1 : class
		{
			throw new NotImplementedException();
		}

		public Task Publish<T1>(object values, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = default) where T1 : class
		{
			throw new NotImplementedException();
		}

		public ConnectHandle ConnectPublishObserver(IPublishObserver observer)
		{
			throw new NotImplementedException();
		}

		public ConnectHandle ConnectSendObserver(ISendObserver observer)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<string> SupportedMessageTypes => new[] { typeof(T).FullName };
		public Uri InputAddress => null;
		public DateTime? ExpirationTime => null;
		public DateTime? SentTime => null;
		public Headers MessageHeaders => null;
		public ClaimsPrincipal? Principal => null;

		public ReceiveContext ReceiveContext => throw new NotImplementedException();
		public Task ConsumeCompleted => Task.CompletedTask;

		public SerializerContext SerializerContext => throw new NotImplementedException();
		#endregion
	}
}
