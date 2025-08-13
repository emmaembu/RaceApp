using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using PawRace.DataAccess;
using PawRace.DataAccess.Models;
using PawRace.Models.DTO;
using PawRace.Models.Enums;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using Wallet;
namespace Ticket
{
    public class TicketService : ITicketService
    {
        public required IConfiguration _config;
        private readonly IPawRaceDataAccess _dataAccess;
        private readonly IWalletService _walletService;
        private readonly IMapper _mapper;
        private readonly ILogger<TicketService> _logger;
        private readonly decimal _minStake;
        private readonly decimal _maxStake;
        private readonly decimal _maxPotentialWin;

        public TicketService(IPawRaceDataAccess dataAccess, IWalletService walletService,IMapper mapper, ILogger<TicketService> logger,  IConfiguration config)
        {
            _dataAccess = dataAccess;
            _walletService = walletService;
            _mapper = mapper;
            _logger = logger;
            _minStake = config.GetValue<decimal>("TicketRules:MinStake");
            _maxStake = config.GetValue<decimal>("TicketRules:MaxStake");
            _maxPotentialWin = config.GetValue<decimal>("TicketRules:MaxPotentialWin");
        }

        public async Task<TicketStatus> GetTicketById(int ticketId)
        {
            var result = await _dataAccess.GetTicketByTicketIdAsync(ticketId);         

            return _mapper.Map<TicketStatus>(result);
        }

        public async Task<int> PlaceBetAsync( decimal amount,  List<RaceDogSelection> selections, string nickName, string ipAddress)
        {
            int ticketId = 0; 
            if (amount < _minStake || amount > _maxStake)
            {
                throw new ArgumentOutOfRangeException("Amount must be between{_minStake} and {_maxStake}");               
            }
                
            foreach(var selection in selections)
            {
                var race = await _dataAccess.GetRaceByIdAsync(selection.RaceId);

                if (race == null)
                    return 0;
                if (!await _dataAccess.CheckIfDogAndRaceValidbyIdAsync(selection.DogId, selection.RaceId))
                    return 0;
                if (race.StartedAt != null || race.ScheduledAt < DateTime.UtcNow)
                    return 0;
            }

            var dbUser = await _dataAccess.GetUserAsync(nickName, ipAddress);

            var regUser = _mapper.Map<PawRace.Models.DTO.User>(dbUser);

            var guidId = Guid.NewGuid();
            if (regUser == null)
            {
                
                var newUser = new PawRace.DataAccess.Models.User
                {
                    Guidid = guidId,
                    NickName = nickName,
                    IpAddress = ipAddress
                };

                var user = await _dataAccess.AddUserAsync(newUser);

            }
            else
            {
                guidId = regUser.GuidId;
            }

            decimal? existsAmount =  _walletService.CheckIfWalletExists(guidId);

            if (existsAmount == null)
            {
                _walletService.Deposit(guidId, 0M);
            }

            var success = _walletService.TryWithdraw(guidId, amount);

            if (success)
            {
                ticketId = await _dataAccess.AddTicketAsync(guidId, amount, DateTime.UtcNow, (byte) BetTicketStatus.Pending);

                _logger.LogInformation("Ticket {ticketId} is added", ticketId);

                foreach (var selection in selections)
                {
                    var ticketDetails = new TicketDetail
                    {
                        TicketId = ticketId,
                        RaceId = selection.RaceId,
                        DogId = selection.DogId
                    };

                    int inserted = await _dataAccess.AddTicketDetailsAsync( ticketDetails);

                    if (inserted == 0)
                    {
                        _walletService.Refund(guidId, amount);
                    }
                }

                List<decimal> odds = await GetOddsForEachRaceDogByTicketIdAsync(ticketId);

                decimal potentialWinning = CalculatePotentialWinnings(amount, odds);

                bool detailId = await _dataAccess.InsertPotentialWinningAsync(ticketId, potentialWinning);

                if(!detailId)
                {
                    _walletService.Refund(guidId, amount);
                    return 0;
                }

            }
            return ticketId;
        }

        private async Task<List<decimal>> GetOddsForEachRaceDogByTicketIdAsync(int ticketId)
        {
            var dogs = await _dataAccess.GetRaceDogsForTicketAsync(ticketId);

            return dogs.Select(d => d.Odds).ToList();
        }

        private decimal CalculatePotentialWinnings(decimal amount, List<decimal> odds)
        {
            int safetyCounter = 0;

            if (amount <= 0 || odds.Count == 0) return 0;

            decimal totalMultiplier = 1;          

            foreach(var odd in odds)
            {
                totalMultiplier *= odd;
            }

            decimal potential = amount * totalMultiplier;

            while (potential > _maxPotentialWin && safetyCounter < 10)
            {
                amount = _maxPotentialWin / totalMultiplier; 
                potential = amount * totalMultiplier; 
                safetyCounter++;
            }

            return Math.Round(potential, 2);
        }



        #region Background Service
        public async Task<bool> ProcessTickets()
        {
            var activeTickets = await _dataAccess.GetActiveTicketsAsync();

            foreach (var dbTicket in activeTickets)
            {
                bool refundNeeded = false;
                bool ticketWon = false;
                byte statusId ;

                // get ticket races
                var raceDetails = await _dataAccess.GetRaceDogsForTicketAsync(dbTicket.Id);

                // get ticket
                var ticket = _mapper.Map<TicketStatus>(dbTicket);

                // var user guid 

                var dbUser = await _dataAccess.GetUserAsync(ticket.NickName, ticket.IpAddress);

                var regUser = _mapper.Map<PawRace.Models.DTO.User>(dbUser);

                foreach (var tr in raceDetails)
                {
                    var race = await _dataAccess.GetRaceByIdAsync(tr.RaceId);

                    if (race.RaceStatusId != (byte)RaceStatus.Finished) break;

                    if (race.StartedAt <= dbTicket.CreatedAt)
                    {
                        refundNeeded = true;
                    }

                    var winningDog = await _dataAccess.GetRaceDogByIdAsync(race.Id);

                    if (winningDog == null || winningDog.IsWinner == false)
                    {
                        ticketWon = false;
                    }

                    if(refundNeeded)
                    {
                        _walletService.Refund(regUser.GuidId, ticket.Amount);
                        statusId = (byte)BetTicketStatus.Rejected;
                    }
                    if (ticketWon)
                    {
                        _walletService.Deposit(regUser.GuidId, (ticket.Amount*winningDog!.Odds));// potential winning is only when entire ticket is successfull
                        statusId = (byte)BetTicketStatus.Won;

                    }
                    else
                    {
                        statusId = (byte)BetTicketStatus.Lost;
                    }

                    await _dataAccess.UpdateTicketAsync(dbTicket.Id, statusId);


                    _logger.LogInformation("Ticket {Id} has been {statusId}", dbTicket.Id, statusId);
                }

               
            }
            return true;
        }


        public async Task<bool> ActivateTickets()
        {
            var pendingTickets = await _dataAccess.GetPendingTicketsAsync();

            foreach (var dbTicket in pendingTickets)
            {
                var raceDetails = await _dataAccess.GetRaceDogsForTicketAsync(dbTicket.Id);

                foreach (var tr in raceDetails)
                {
                    bool check = await CheckIfRaceIsInProgressAsync(tr.RaceId);

                    if (check)
                    { 
                        await _dataAccess.UpdateTicketAsync(dbTicket.Id, (byte)BetTicketStatus.Success);

                        _logger.LogInformation("Ticket {Id} has been {statusId}", dbTicket.Id, (byte)BetTicketStatus.Success);
                    }
                }
            }
            return true;
        }

        private async Task<bool> CheckIfRaceIsInProgressAsync(int raceId)
        {
            var race = await _dataAccess.GetRaceByIdAsync(raceId);

            return race.RaceStatusId == 1 ? true : false;
        }
    #endregion
    }
}
