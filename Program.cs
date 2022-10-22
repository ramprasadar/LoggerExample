using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog.Sinks.MSSqlServer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//-----------------------section-----------------------------------------------------------------------------------
//following section is added for logging to Azure log analytics with serilog
var workspaceId = builder.Configuration.GetValue<string>("workspaceId");
var primaryKey = builder.Configuration["workspacePrimaryKey"];
/*var workspaceId = "345d766a-5e1f-4030-81ae-55f5c042c483";
var primaryKey = "zoabHjWFPbC/8Bu681nqKgIGj+PSCc3+c1ZuVOx73Kk+Of2aO25F0gN9QIiP93AK4AuwkGyDWngYd+sMXJIJsA==";*/

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .Enrich.FromLogContext() // this adds more information to the output of the log
    .WriteTo.Console()
    /*.WriteTo.Async(a =>
    {
        a.AzureAnalytics(workspaceId, primaryKey, "LoggerExampleWebAPI");
    }, bufferSize: 500)*/
    .WriteTo.AzureAnalytics(workspaceId, primaryKey, "LoggerExampleWebAPI")
    .WriteTo.File(new JsonFormatter(), "log.txt")
    .WriteTo.MSSqlServer("Server=(local);database=LoggingDb;trusted_connection=True",
                         new MSSqlServerSinkOptions
                         {
                             TableName = "Logs",
                             SchemaName = "dbo",
                             AutoCreateSqlTable = true
                         })
    .CreateLogger();
builder.Logging.ClearProviders();
builder.Logging.AddSerilog(Log.Logger);

//-----------------------------------section-----------------------------------------------------------------

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
                    
var app = builder.Build();

//for logging http request/response
app.UseHttpLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
