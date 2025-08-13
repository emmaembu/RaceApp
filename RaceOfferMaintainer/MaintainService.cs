using Offer;
using Ticket;

namespace RaceOfferMaintainer
{
    public class MaintainService : BackgroundService
    {
        private readonly ILogger<MaintainService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly int _minimumRaces;

        public MaintainService( IServiceScopeFactory scopeFactory,ILogger<MaintainService> logger,IConfiguration configuration)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _minimumRaces = configuration.GetValue<int>("BackgroundService:minimumRaces");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Race scheduler started.");

            while(!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();

                    var raceService = scope.ServiceProvider.GetService<IOfferService>();
                    var ticketService = scope.ServiceProvider.GetService<ITicketService>();

                    await raceService!.EnsureMinimumScheduledRacesAsync(_minimumRaces, TimeSpan.FromMinutes(2)); 
                    await raceService.ActivateScheduledRacesAsync();
                    await ticketService!.ActivateTickets();
                    await raceService.DetermineWinner();
                    await raceService.FinishRacesAsync();
                    await ticketService.ProcessTickets();

                }
                catch (Exception ex) 
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                    _logger.LogError("Something went wrong: {ex}", ex);

                }
            }
        }
    }
}
