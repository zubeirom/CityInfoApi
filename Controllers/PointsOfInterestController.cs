using AutoMapper;
using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CityInfo.API.Controllers
{
    [Route("api/cities")]
    public class PointsOfInterestController : Controller
    {
        private ILogger<PointsOfInterestController> _logger;
        private IMailService _mailService;
        private ICityInfoRepository _cityInfoRepository;
        private readonly IMapper _mapper;

        public PointsOfInterestController(ICityInfoRepository cityInfoRepository, IMapper mapper)
        {
            _cityInfoRepository = cityInfoRepository;
            _mapper = mapper;
        }

        // GET 
        [HttpGet("{cityId}/pointsofinterest")]
        public IActionResult GetPointsOfInterest(int cityId)
        {
            try
            {
                // var city = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);

                if(!_cityInfoRepository.CityExists(cityId))
                {
                    _logger.LogInformation($"City with id {cityId} wasn't found when accessing points of interest");
                    return NotFound();
                }

                var pointsOfInterestForCity = _cityInfoRepository.GetPointsOfInterestForCity(cityId);

                var results = _mapper.Map<IEnumerable<PointOfInterestDto>>(pointsOfInterestForCity);


                return Ok(results);

            }
            catch(Exception e)
            {
                _logger.LogCritical($"LOGGER CRITICAL: Exception while getting points of interest for city with id {cityId}.", e);
                return StatusCode(500, "A problem happened while handling request.");
            }

            
        }

        // GET BY ID
        [HttpGet("{cityId}/pointsofinterest/{id}", Name ="GetPoi")]
        public IActionResult GetPointOfInterest(int cityId, int id)
        {
            var pointOfInterest = _cityInfoRepository.GetPointOfInterestForCity(cityId, id);

            if (!_cityInfoRepository.CityExists(cityId))
            {
                return NotFound();
            }

            if (pointOfInterest == null)
            {
                return NotFound();
            }

            var poi = _mapper.Map<PointOfInterestDto>(pointOfInterest);

            return Ok(poi);
        }

        // POST
        [HttpPost("{cityId}/pointsofinterest")]
        public IActionResult CreatePointOfInterest(int cityId, [FromBody] PointOfInterestForCreationDto pointOfInterest)
        {
            if (pointOfInterest == null)
            {
                return BadRequest();
            }

            if (pointOfInterest.Description == pointOfInterest.Name)
            {
                ModelState.AddModelError("Description", "The provided description should be different from the name");
            }

            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if(!_cityInfoRepository.CityExists(cityId))
            {
                return NotFound();
            }

            var finalPoi = _mapper.Map<Entities.PointOfInterest>(pointOfInterest);

            _cityInfoRepository.AddPointForInterestForCity(cityId, finalPoi);

            if(!_cityInfoRepository.Save())
            {
                return StatusCode(500, "A problem happened while handling your request");
            }

            var savedPoi = _mapper.Map<Models.PointOfInterestDto>(finalPoi);

            return CreatedAtRoute("GetPoi", new { cityId = cityId, id = savedPoi.Id }, savedPoi);
        }

        // PUT
        [HttpPut("{cityId}/pointsofinterest/{id}")]
        public IActionResult UpdatePointOfInterest(int cityId, int id, [FromBody] PointOfInterestForUpdateDto pointOfInterest)
        {

            if (pointOfInterest == null)
            {
                return BadRequest();
            }

            if (pointOfInterest.Description == pointOfInterest.Name)
            {
                ModelState.AddModelError("Description", "The provided description should be different from the name");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if(!_cityInfoRepository.CityExists(cityId))
            {
                return NotFound();
            }

            var poiEntity = _cityInfoRepository.GetPointOfInterestForCity(cityId, id);
            if(poiEntity == null)
            {
                return NotFound();
            }

            _mapper.Map(pointOfInterest, poiEntity);

            if(!_cityInfoRepository.Save())
            {
                return StatusCode(500, "A problem happened while handling request");
            }

            return NoContent();

        }

        // PATCH
        [HttpPatch("{cityId}/pointsofinterest/{id}")]
        public IActionResult PartiallyUpdatePointOfInterest(int cityId, int id, [FromBody] JsonPatchDocument<PointOfInterestForUpdateDto> patchDoc)
        {

            if (patchDoc == null)
            {
                return BadRequest();
            }

            var city = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);
            if (city == null)
            {
                return NotFound();
            }

            var pointOfInterestFromStore = city.PointsOfInterest.FirstOrDefault(p => p.Id == id);

            if (pointOfInterestFromStore == null)
            {
                return NotFound();
            }

            var pointOfInterestToPatch = new PointOfInterestForUpdateDto()
            {
                Name = pointOfInterestFromStore.Name,
                Description = pointOfInterestFromStore.Description
            };

            patchDoc.ApplyTo(pointOfInterestToPatch, ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if(pointOfInterestToPatch.Description == pointOfInterestToPatch.Name)
            {
                ModelState.AddModelError("Description", "The provided description should be different from the name");
            }

            TryValidateModel(pointOfInterestToPatch);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            pointOfInterestFromStore.Name = pointOfInterestToPatch.Name;
            pointOfInterestFromStore.Description = pointOfInterestToPatch.Description;

            return NoContent();
        }

        // DELETE
        [HttpDelete("{cityId}/pointsofinterest/{id}")]
        public IActionResult DeletePointOfInterest(int cityId, int id)
        {
            var city = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);
            if (city == null)
            {
                return NotFound();
            }

            var pointOfInterestFromStore = city.PointsOfInterest.FirstOrDefault(p => p.Id == id);

            if (pointOfInterestFromStore == null)
            {
                return NotFound();
            }

            city.PointsOfInterest.Remove(pointOfInterestFromStore);
            return NoContent();
        }

    }

}
