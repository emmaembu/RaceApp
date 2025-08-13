using AutoMapper;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.Extensions.Logging;
using PawRace.DataAccess;
using PawRace.DataAccess.Models;
using PawRace.Models.DTO;
using PawRace.Models.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Dto = PawRace.Models.DTO;

namespace Offer
{
    public class OfferService : IOfferService
    {
        private readonly IPawRaceDataAccess _dataAccess;
        private readonly IMapper _mapper;
        private readonly ILogger<OfferService> _logger;
        public OfferService(IPawRaceDataAccess dataAccess,IMapper mapper, ILogger<OfferService> logger)
        {
            _dataAccess = dataAccess;
            _mapper = mapper;
            _logger = logger;

        }

        public async Task<List<Dto.Dog>> GetDogsAsync()
        {
            _logger.LogInformation("Get all upcoming dogs.");
            var dogs =  await _dataAccess.GetDogsAsync(); 

            return _mapper.Map<List<Dto.Dog>>(dogs);
        }

        public async Task<List<PawRace.Models.DTO.RacingDog>> GetUpcomingRacesAsync()
        {
            _logger.LogInformation("Get all upcoming races.");
            var races = await _dataAccess.GetUpcomingRacesAsync();

            return _mapper.Map<List<Dto.RacingDog>>(races);
        }

        public async Task<Dto.Race> GetRaceByIdAsync(int raceId)
        {
            _logger.LogInformation("Get race by id {Id}",raceId);
            var race = await _dataAccess.GetRaceByIdAsync(raceId);

            return _mapper.Map<Dto.Race>(race);
        }


        #region BackGround Service
        public async Task EnsureMinimumScheduledRacesAsync(int minimumCount, TimeSpan interval)
        {
            _logger.LogInformation("Ensure Minimum Scheduled Races.");
            var scheduledRaces = await _dataAccess.GetCountScheduledRacesAsync(); 

            int toCreate = minimumCount - scheduledRaces;

            if (toCreate <= 0) return; 

            _logger.LogInformation("Races to be created: {toCreate}", toCreate);

            DateTime? lastScheduledTime = await _dataAccess.GetLastRaceScheduledTime(); 

            var now = DateTime.UtcNow;

            for (int i = 0; i < toCreate; i++)
            {
                try 
                {
                    int raceId = await _dataAccess.AddRaceAsync((byte) RaceStatus.Scheduled, DateTime.UtcNow.AddSeconds( 30 *(toCreate + i +1)));

                    var dogs = await _dataAccess.GetDogsAsync();

                    var dtoDogs = _mapper.Map<List<Dto.Dog>>(dogs);

                    var selectedDogs = GetRandomDogs(dtoDogs, 6);

                    var racingDogs = CreateRacingDogs(raceId, selectedDogs);

                    foreach(var dog in racingDogs)
                    {
                        var dbDog = _mapper.Map<RaceDog>(dog);
                        await _dataAccess.AddRacingDogAsync(raceId, dbDog);
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogInformation("Error message {ex}", ex);
                }

            }


        }

        public async Task ActivateScheduledRacesAsync()
        {
            _logger.LogInformation("Activate Scheduled Races.");

            var scheduledRaces = await _dataAccess.GetRacesToActivate();

            _logger.LogInformation("Races to be activated { scheduledRaces.Count()} ", scheduledRaces.Count());

            foreach (var race in scheduledRaces)
            {
                try
                {
                    await _dataAccess.UpdateRaceStatusAndTimeAsync(race.Id, (byte) RaceStatus.InProgress, DateTime.UtcNow);
                }
                catch (Exception ex)
                {
                    _logger.LogInformation("Error message {ex}", ex);
                }

            }
        }

        public async Task FinishRacesAsync()
        {
            _logger.LogInformation("Finish Activated Races.");

            var activeRaces = await _dataAccess.GetRacesToFinish();

            foreach (var race in activeRaces)
            {
                try
                {
                    await _dataAccess.UpdateRaceStatusAndTimeAsync(race.Id, (byte)RaceStatus.Finished, DateTime.UtcNow);
                }
                catch (Exception ex)
                {
                    _logger.LogInformation("Error message {ex}", ex);
                }
            }
        }

        public async Task DetermineWinner()
        {
            _logger.LogInformation("Determine winner for each.");

            var now = DateTime.UtcNow;

            var inProgressRaces = await _dataAccess.GetInProgressRacesWithDogsAsync(now);

            var result = _mapper.Map<List<RaceDetails>>(inProgressRaces);

            var winners = new List<int>();

            foreach (var race in inProgressRaces)
            {
                if (race.RaceDogs == null)
                {
                    continue;
                }

                if (result.Any(race => race.RacingDogs.Any(d => d.IsWinner == true))) continue;

                var winner = race.RaceDogs.OrderBy(d => Guid.NewGuid()).FirstOrDefault();

                await _dataAccess.UpdateRaceDogWinnerAsync(winner!.RaceId, winner.DogId);


            }
        }

        private readonly Random _random = new Random();
        private List<Dto.Dog> GetRandomDogs(List<Dto.Dog> dogs, int count)
        {
            var random = new Random();  

            return dogs.OrderBy( x => random.Next()).Take(count).ToList();
        }

        private List<Dto.RacingDog> CreateRacingDogs (int raceId, List<Dto.Dog> selectedDogs)
        {
            var random = new Random();
            var usedPosition = new HashSet<int>();  
            var raceDogs = new List<Dto.RacingDog>();

            foreach(var dog in selectedDogs)
            {
                var oddsDouble = Math.Round(random.NextDouble() * 8.5 + 1.5, 2);
                decimal odds = Math.Round((decimal)oddsDouble, 2);

                int startPosition;
                do
                {
                    startPosition = _random.Next(1, 7);
                }
                while (usedPosition.Contains(startPosition));
                {
                    usedPosition.Add(startPosition);
                }
                raceDogs.Add(new Dto.RacingDog
                {
                    RaceId = raceId,
                    DogId = dog.Id,
                    Odds = odds,
                    StartPosition = startPosition
       
                });
            }
            return raceDogs;
        }
        #endregion
    }
}
