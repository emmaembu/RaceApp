using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Offer;
using PawRace.DataAccess;
using PawRace.DataAccess.Models;
using PawRace.Models.Map;
using RaceOfferMaintainer;
using Ticket;
using Wallet;

IHost host = Host.CreateDefaultBuilder(args).ConfigureServices((hostContext, services) =>
{
    services.AddDbContext<PawRaceDbContext>(options => options.UseSqlServer(hostContext.Configuration.GetConnectionString("DefaultConnection")));
    services.AddScoped<IPawRaceDataAccess, PawRaceDataAccess>();
    services.AddAutoMapper(typeof(MappingProfile));
    services.AddScoped<IOfferService, OfferService>();
    services.AddScoped<IWalletService, WalletService>();
    services.AddScoped<ITicketService, TicketService>();
    services.AddHostedService<MaintainService>();
}).Build();


await host.RunAsync();
