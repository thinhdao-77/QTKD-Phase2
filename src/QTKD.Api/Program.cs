using Microsoft.EntityFrameworkCore;
using QTKD.Api.Data;
using QTKD.Api.Exceptions;
using QTKD.Api.Repositories;
using QTKD.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ---- Error handling thống nhất: mọi lỗi trả về ProblemDetails (RFC 7807) ----
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// ---- Data (PostgreSQL + EF Core) ----
var connectionString = builder.Configuration.GetConnectionString("QtkdDb")
    ?? throw new InvalidOperationException("Connection string 'QtkdDb' is not configured.");

builder.Services.AddDbContext<QtkdDbContext>(options => options.UseNpgsql(connectionString));

// ---- Dependency Injection: Controller -> Service -> Repository -> DbContext ----
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IContractRepository, ContractRepository>();
builder.Services.AddScoped<IPriceListRepository, PriceListRepository>();

builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IContractService, ContractService>();
builder.Services.AddScoped<IPriceListService, PriceListService>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
