using Microsoft.EntityFrameworkCore;
using PawRace.DataAccess;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using AutoMapper;
using PawRace.DataAccess.Models;
using Offer;
using Wallet;
using PawRace.Models.Map;
using Ticket;
using PawRaceAPI.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IPawRaceDataAccess, PawRaceDataAccess>();
builder.Services.AddScoped<IOfferService, OfferService>();
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddDbContext<PawRaceDbContext>
    (
        options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
    );
builder.Services.AddSingleton<IWalletService, WalletService>();

builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddControllers();

builder.Services.AddSignalR();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen( 
    c => {
        c.SwaggerDoc
                   (
                       "v1",
                       new Microsoft.OpenApi.Models.OpenApiInfo
                       {
                           Title = "PawRace API",
                           Version = "v1"
                       }
                   );
        c.EnableAnnotations(); });

var app = builder.Build();


// since it is in memory wallet , every check will fail without this first initialization
using (var scope = app.Services.CreateScope())
{
    var walletService = scope.ServiceProvider.GetService<IWalletService>();

    walletService!.Deposit(Guid.NewGuid(), 100M);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(
        c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "PawRace API v1"); });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapHub<MessageHub>("/messagehub");

app.Run();
