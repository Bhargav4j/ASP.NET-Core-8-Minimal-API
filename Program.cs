using MinimalAPIProject.Endpoint;

var builder = WebApplication.CreateBuilder(args);

// Explicitly add environment variable support with custom prefix
builder.Configuration.AddEnvironmentVariables(prefix: "APP_");

//add services and repositories
builder.Services.AddStudentApi();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add health checks for container orchestration
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//all api's endpoints
app.MapStudentApiRoutes();

// Map health check endpoints for container orchestration
app.MapHealthChecks("/health");
app.MapHealthChecks("/ready");

app.Run();