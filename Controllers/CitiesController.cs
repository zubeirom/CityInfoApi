using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CityInfo.API.Controllers
{
    [Route("api/cities")]
    public class CitiesController : Controller
    {
        private ICityInfoRepository _cityInfoRepository;

        public CitiesController(ICityInfoRepository cityInfoRepository)
        {
            _cityInfoRepository = cityInfoRepository;
        }

        [HttpGet()]
        public IActionResult GetCities()
        {
            // return Ok(CitiesDataStore.Current.Cities);
            var cityEntities = _cityInfoRepository.GetCities();
            var results = new List<CityWithoutPointsOfInterestDto>();

            foreach (var cityEntity in cityEntities)
            {
                results.Add(new CityWithoutPointsOfInterestDto
                {
                    Id = cityEntity.Id,
                    Description = cityEntity.Description,
                    Name = cityEntity.Name
                });
            }

            return Ok(cityEntities);
        }

        [HttpGet("{id}")]
        public IActionResult GetCity(int id, bool includePointsOfInterest = false)
        {
            var city = _cityInfoRepository.GetCity(id, includePointsOfInterest);

            if(city == null)
            {
                return NotFound();
            }

            if(includePointsOfInterest)
            {
                var cityResults = new CityDto()
                {
                    Id = city.Id,
                    Name = city.Name,
                    Description = city.Description
                };

                foreach (var poi in city.PointsOfInterest)
                {
                    cityResults.PointsOfInterest.Add(
                        new PointOfInterestDto()
                        {
                            Id = poi.Id,
                            Name = poi.Name,
                            Description = poi.Description
                        });
                }

                return Ok(cityResults);
            }

            var singleCity = new CityWithoutPointsOfInterestDto()
            {
                Id = city.Id,
                Name = city.Name,
                Description = city.Description
            };

            //find city
            // var cityToReturn = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == id);
            //if(cityToReturn == null)
            //{
            //    return NotFound();
            //}

            return Ok(singleCity);
        }
    }
}
