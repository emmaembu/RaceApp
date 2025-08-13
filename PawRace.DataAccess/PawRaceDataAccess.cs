using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualBasic;
using PawRace.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Dog = PawRace.DataAccess.Models.Dog;
using Race = PawRace.DataAccess.Models.Race;
using User = PawRace.DataAccess.Models.User;


namespace PawRace.DataAccess
{
    public class PawRaceDataAccess : IPawRaceDataAccess
    {

        private readonly int _dbTimeout;
        private readonly string _connectionString;
        private readonly PawRaceDbContext _dbContext;
        private readonly IMapper _mapper;

        public PawRaceDataAccess(IConfiguration configuration, IMapper mapper)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException("Missing connection string.");
            if (!Int32.TryParse(configuration["DbOptions:TimeoutInSeconds"], out _dbTimeout))
            {
                _dbTimeout = 60;
            }

            DbContextOptionsBuilder<PawRaceDbContext> optionsBuilder = new();
            optionsBuilder.UseSqlServer
                (
                    _connectionString,
                    sqlServerOptions => sqlServerOptions
                    .CommandTimeout(_dbTimeout)
                    .UseCompatibilityLevel(120) 

                );
            _dbContext = new PawRaceDbContext(optionsBuilder.Options);
            _mapper = mapper;
        }

        public async Task<List<Dog>> GetDogsAsync()
        {
            var dogs = await _dbContext.Dogs.ToListAsync();

            return dogs;
        }

        public async Task<int> GetCountScheduledRacesAsync()
        {
            return await _dbContext.Races.CountAsync(r => r.RaceStatusId == 0 && r.ScheduledAt > DateTime.UtcNow);
        }

        public async Task<DateTime?> GetLastRaceScheduledTime()
        {
            return await _dbContext.Races.OrderByDescending(r => r.ScheduledAt).Select( r => r.ScheduledAt).FirstOrDefaultAsync();
        }

        public async Task<List<Race>> GetRacesToActivate()
        {
            var races = await _dbContext.Races.Where(r => r.RaceStatusId == 0 && r.ScheduledAt <= DateTime.UtcNow).ToListAsync();

            return races;
        }
        
        public async Task <bool> UpdateRaceStatusAndTimeAsync(int raceId, byte status, DateTime datetime)
        {
            var race = await _dbContext.Races.FirstOrDefaultAsync(r => r.Id == raceId);

            if(race == null)
            {
                throw new Exception($"Race with Id {raceId} is not found.");
            }

            race.RaceStatusId = (byte)status;
            if (status == 1)
            { 
                race.StartedAt = datetime;
                race.RaceStatusId = 1;
            }
            else 
            { 
                race.FinishedAt = datetime;
                race.RaceStatusId = 2;
            }

            _dbContext.Entry(race).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<int> AddRaceAsync(byte status, DateTime? datetime)
        {
            try
            { 
                var race = new Race
                {
                    RaceStatusId = status,
                    ScheduledAt = datetime
                };

                await _dbContext.AddAsync(race);

                await _dbContext.SaveChangesAsync();

                return race.Id;
            }
            catch(Exception ex) 
            {
                //TODO
                return 0; 
            }
        }

        public async Task<List<Race>> GetRacesToFinish()
        {
            var races = await _dbContext.Races.Where(r => r.RaceStatusId == 1 && DateTime.UtcNow >= r.StartedAt!.Value.AddMinutes(2)).ToListAsync(); // dohvaćaj iz appsetigns.jsona

            return races;
        }

        public async Task<Race> GetRaceByIdAsync(int raceId)
        {
            var race = await _dbContext.Races.Where(r => r.Id == raceId).FirstOrDefaultAsync(); 

            return race!;
        }

        public async Task<Dog?> GetDogByIdAsync(int dogId)
        {
            return await _dbContext.Dogs.FindAsync(dogId);

        }

        public async Task<bool> CheckIfDogAndRaceValidbyIdAsync(int dogId, int raceId)
        {
            var dog = await _dbContext.RaceDogs.Where(r => r.DogId == dogId && r.RaceId == raceId).FirstOrDefaultAsync();

            return dog== null ? false : true;
        }

        public async Task<bool> AddRacingDogAsync(int raceId, RaceDog dog)
        {
            try
            {

                await _dbContext.AddAsync(dog);

                await _dbContext.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                //TODO
                return false; 
            }

        }

        public async Task<List<Race>> GetInProgressRacesWithDogsAsync(DateTime currentTime)
        {
            var raceAndDogs = await  _dbContext.Races.Where(r=> r.RaceStatusId == (byte) 1 && r.StartedAt!.Value.AddMinutes(2).AddSeconds(-5) <= currentTime).OrderBy(r=>r.ScheduledAt).Include(r=>r.RaceDogs).ThenInclude(rd=>rd.Dog).ToListAsync();

            return raceAndDogs;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<int> AddTicketAsync(Guid user, decimal amount, DateTime datetime, byte status)
        {
            try
            {
                var bet = new Ticket
                {
                    UserId = user,
                    TotalAmount = amount,
                    CreatedAt = datetime,
                    StatusId = status
                };

                await _dbContext.Tickets.AddAsync(bet);

                await _dbContext.SaveChangesAsync();

                return bet.Id;

            }
            catch
            {
                return 0;
            }
        }

        public async Task<bool> UpdateTicketAsync(int ticketId, byte status)
        {
            var ticket = await _dbContext.Tickets.FirstOrDefaultAsync(r => r.Id == ticketId);

            if (ticket == null)
            {
                throw new Exception($"ticket with Id {ticketId} is not found.");
            }

            ticket.StatusId = (byte)status;

            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> InsertPotentialWinningAsync(int ticketId, decimal potentialWinning)
        {
            var ticket = await _dbContext.Tickets.FirstOrDefaultAsync(r => r.Id == ticketId);

            if (ticket == null)
            {
                throw new Exception($"ticket with Id {ticketId} is not found.");
            }

            ticket.PotentialWinning = potentialWinning;

            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<int> AddTicketDetailsAsync(TicketDetail ticket)
        {
            try
            {
                var details = new TicketDetail
                {
                    DogId = ticket.DogId,
                    RaceId = ticket.RaceId,
                    TicketId = ticket.TicketId
                };

                await _dbContext.TicketDetails.AddAsync(details);

                await _dbContext.SaveChangesAsync();

                return details.Id;

            }
            catch
            {
                return 0;
            }
        }

        public async Task<List<Race>> GetRacesByIdsAsync(List<int> raceIds)
        {
            if (raceIds == null || !raceIds.Any())
                return new List<Race>();

            return await _dbContext.Races.Where(r => raceIds.Contains(r.Id)).ToListAsync();
        }

        public async Task<User> GetUserAsync(string nickname, string ipAddress)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.NickName == nickname && u.IpAddress == ipAddress);

            return user!;
        }

        public async Task<bool> AddUserAsync(User user)
        {
            try
            {

                var regUser = await _dbContext.Users.AddAsync(_mapper.Map<Models.User>(user));

                await _dbContext.SaveChangesAsync();

                return true;

            }
            catch
            {
                return false;//TODO
            }
        }

        public async Task<List<RaceDog>> GetRaceDogsForTicketAsync(int ticketId)
        {
            return await     (from td in _dbContext.TicketDetails
                             join rd in _dbContext.RaceDogs on new { td.RaceId, td.DogId }
                             equals new { rd.RaceId, rd.DogId }
                             where td.TicketId == ticketId
                             select new RaceDog { DogId = rd.DogId, RaceId = rd.RaceId, Odds = rd.Odds, StartPosition = rd.StartPosition }).ToListAsync();
        }

        public async Task<RaceDog> GetRaceDogByIdAsync(int raceId)
        {
            return await _dbContext.RaceDogs.FirstOrDefaultAsync(r => r.RaceId == raceId) ?? new RaceDog();
        }

        public async Task<List<RaceDog>> GetUpcomingRacesAsync()
        {
            return await (from r in _dbContext.Races
                          join rd in _dbContext.RaceDogs on r.Id equals rd.RaceId
                          where r.RaceStatusId == 0 && r.ScheduledAt > DateTime.UtcNow
                          select new RaceDog
                          {
                              DogId = rd.DogId,
                              RaceId = rd.RaceId,
                              Odds = rd.Odds,
                              StartPosition = rd.StartPosition
                          }).ToListAsync();
        }

        public async Task<List<Race>> GetRacesByTicketIdAsync (int ticketId)
        {
 
            return await _dbContext.TicketDetails.Where(r => r.TicketId == ticketId).Include(td => td.Race).Select(td => td.Race).ToListAsync();
        }

        public async Task<Ticket?> GetTicketByTicketIdAsync(int ticketId)
        {
            return await   _dbContext.Tickets.Include(r => r.User).FirstOrDefaultAsync(r => r.Id == ticketId);
        }

        public async Task<List<Ticket>> GetPendingTicketsAsync()
        {
            return await _dbContext.Tickets.Where(r => r.StatusId == 0).ToListAsync();
        }

        public async Task<List<Ticket>> GetActiveTicketsAsync()
        {
            return await _dbContext.Tickets.Where(r => r.StatusId == 2).ToListAsync();
        }

        public async Task<bool> UpdateRaceDogWinnerAsync(int raceId, int dogId)
        {
            var dog = await _dbContext.RaceDogs.Where(r => r.DogId == dogId && r.RaceId == raceId).FirstOrDefaultAsync();

            if (dog == null) return false;

            dog.IsWinner = true;

            await _dbContext.SaveChangesAsync();

            return true;
        }

    }
}
