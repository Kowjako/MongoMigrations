using Mediatr.Api;
using Mediatr.Api.Context;
using Mediatr.Api.Filters.ActionFilters;
using Mediatr.Api.Filters.ResourceFilters;
using Mediatr.Api.Middleware;
using Mediatr.Api.Migration;
using MediatR.Application;
using MongoDB.Bson.Serialization.Conventions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(cfg =>
{
    cfg.Filters.Add<CorrelationIdFilter>();
    cfg.Filters.Add<LogUserActivityFilter>();
});

// required so mongo lowercase can be mapped to C# records titlecase
var pack = new ConventionPack() { new CamelCaseElementNameConvention() };
ConventionRegistry.Register(nameof(CamelCaseElementNameConvention), pack, _ => true);

builder.Services.AddScoped<MongoContext>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.RegisterApplicationServices();
builder.Services.AddScoped<ExceptionMiddleware>();
builder.Services.AddScoped<GuidGenerator>();
builder.Services.Configure<MongoSettings>(builder.Configuration.GetSection("MongoSettings"));
builder.Services.AddScoped(typeof(IGenericMongoRepository<>), typeof(GenericMongoRepository<>));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

await app.MigrateMongoDbAsync();

app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
