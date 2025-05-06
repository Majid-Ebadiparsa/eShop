using InventoryService.Shared;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace InventoryService.API.Configuration
{
	public class AddDefaultExampleSchemaFilter : IOperationFilter
	{
		public void Apply(OpenApiOperation operation, OperationFilterContext context)
		{
			var actionName = context.ApiDescription.ActionDescriptor.RouteValues["action"];
			var controllerName = context.ApiDescription.ActionDescriptor.RouteValues["controller"];

			if (controllerName == "Inventory" && actionName == "GetByProductId")
			{
				var parameter = operation.Parameters.FirstOrDefault(p => p.Name == "productId");
				if (parameter != null)
				{
					parameter.Description = "Test ID to check the inventory of the product in the warehouse";
					parameter.Example = new Microsoft.OpenApi.Any.OpenApiString(InventoryDefaults.DefaultProductId.ToString());
				}
			}
		}
	}
}
