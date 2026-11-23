using Bastina3D.Infrastructure;
using Bastina3D.Application;
using Microsoft.AspNetCore.StaticFiles;
using NetTopologySuite.IO.Converters;
using System.Text.Json.Serialization;
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.Converters.Add(new GeoJsonConverterFactory());
    o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddCors(o => o.AddPolicy("frontend", p => p
    .WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? ["http://localhost:5173"])
    .AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    await app.Services.InitializeDatabaseAsync();
}

app.UseCors("frontend");

var contentTypes = new FileExtensionContentTypeProvider();
contentTypes.Mappings[".glb"] = "model/gltf-binary";
contentTypes.Mappings[".gltf"] = "model/gltf+json";
app.UseStaticFiles(new StaticFileOptions { ContentTypeProvider = contentTypes });

app.MapControllers();
app.Run();