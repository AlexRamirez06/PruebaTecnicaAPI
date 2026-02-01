using Microsoft.EntityFrameworkCore;
using PruebaTecnicaAPI.Extensions;
using PruebaTecnicaAPI.BusinessLogic;
using PruebaTecnicaAPI.DataAccess.Context;
using PruebaTecnicaAPI.Entities.Entities;

var builder = WebApplication.CreateBuilder(args);

var ConnectionString = builder.Configuration.GetConnectionString("con");

builder.Services.AddDbContext<db39761Context>(option => option.UseSqlServer(ConnectionString));

builder.Services.AddHttpContextAccessor();
builder.Services.DataAccess(ConnectionString);
builder.Services.BusinessLogic();

builder.Services.AddAutoMapper(config =>
{
    config.AddProfile(typeof(MappingProfileExtensions));
});

// ? Agregar CORS ANTES de `var app = builder.Build();`
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ? Habilitar CORS ANTES de `UseAuthorization()`
app.UseCors("AllowAll");

app.UseAuthorization();
app.MapControllers();

app.Run();
