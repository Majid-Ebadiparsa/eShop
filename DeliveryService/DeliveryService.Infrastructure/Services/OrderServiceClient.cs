using DeliveryService.Application.Abstractions.Services;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace DeliveryService.Infrastructure.Services
{
	public class OrderServiceClient : IOrderServiceClient
	{
		private readonly HttpClient _httpClient;
		private readonly ILogger<OrderServiceClient> _logger;

		public OrderServiceClient(HttpClient httpClient, ILogger<OrderServiceClient> logger)
		{
			_httpClient = httpClient;
			_logger = logger;
		}

		public async Task<OrderDetailsDto?> GetOrderDetailsAsync(Guid orderId, CancellationToken cancellationToken)
		{
			try
			{
				var response = await _httpClient.GetAsync($"/api/order/{orderId}", cancellationToken);
				if (!response.IsSuccessStatusCode)
				{
					_logger.LogWarning("Failed to fetch order {OrderId}: {StatusCode}", orderId, response.StatusCode);
					return null;
				}

				var orderDto = await response.Content.ReadFromJsonAsync<OrderServiceResponseDto>(cancellationToken);
				if (orderDto == null)
					return null;

				return new OrderDetailsDto(
					orderDto.Id,
					orderDto.CustomerId,
					orderDto.Street,
					orderDto.City,
					orderDto.PostalCode,
					orderDto.Items.Select(i => new OrderItemDetailsDto(i.ProductId, i.Quantity)).ToList()
				);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error fetching order details for {OrderId}", orderId);
				return null;
			}
		}

		private record OrderServiceResponseDto(
			Guid Id,
			Guid CustomerId,
			string Street,
			string City,
			string PostalCode,
			List<OrderItemResponseDto> Items
		);

		private record OrderItemResponseDto(Guid ProductId, int Quantity, decimal UnitPrice);
	}
}

