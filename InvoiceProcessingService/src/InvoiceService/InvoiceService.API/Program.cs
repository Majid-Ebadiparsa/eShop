using InvoiceService.API.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using InvoiceService.Application;
using InvoiceService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(x =>
	 x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

builder.Services
.AddCustomSwagger()
.AddApplication()
.AddInfrastructure(builder.Configuration)
.AddAuthorization()
.RegisterJwtBearer(builder.Configuration)
.AddEndpointsApiExplorer()
.AddSwaggerGen();


builder.Services.AddApiVersioning(o =>
{
	o.DefaultApiVersion = new ApiVersion(1, 0);
	o.AssumeDefaultVersionWhenUnspecified = true;
	o.ReportApiVersions = true;
});


var app = builder.Build();

app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");

app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }