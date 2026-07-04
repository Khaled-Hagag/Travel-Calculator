using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.OpenApi.Any;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // يضيف حقل X-Device-Id في كل endpoint في Swagger UI
    c.OperationFilter<DeviceIdHeaderFilter>();
});

// CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

// =============================================
// Swagger Filter: يضيف X-Device-Id header تلقائياً لكل endpoint
// =============================================
public class DeviceIdHeaderFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= new List<OpenApiParameter>();
        operation.Parameters.Add(new OpenApiParameter
        {
            Name        = "X-Device-Id",
            In          = ParameterLocation.Header,
            Required    = false,
            Description = "Device identifier (required for most endpoints)",
            Schema      = new OpenApiSchema
            {
                Type    = "string",
                Example = new OpenApiString("test-device-001")
            }
        });
    }
}
