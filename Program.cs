using MinimalAPIProject.Endpoint;

var builder = WebApplication.CreateBuilder(args);

//add services and repositories
builder.Services.AddStudentApi();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
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

// Add health check endpoint
app.MapHealthChecks("/health");


app.Run();