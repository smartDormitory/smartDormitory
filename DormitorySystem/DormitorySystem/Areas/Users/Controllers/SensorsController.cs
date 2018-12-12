﻿using System;
using System.Linq;
using System.Threading.Tasks;
using DormitorySystem.Common.Constants;
using DormitorySystem.Data.Models;
using DormitorySystem.Services.Abstractions;
using DormitorySystem.Services.Exceptions;
using DormitorySystem.Web.Areas.Users.Models;
using DormitorySystem.Web.Areas.Users.Models.SampleSensorsModels;
using DormitorySystem.Web.Areas.Users.Models.UserSensorsModels;
using DormitorySystem.Web.Models.SensorsViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace DormitorySystem.Web.Areas.Users.Controllers
{
    [Area("Users")]
    [Authorize(Roles = "User, Admin")]
    public class SensorsController : Controller
    {
        private readonly ISensorsService sensorsService;
        private readonly UserManager<User> userManager;
        private readonly IMemoryCache memoryCache;

        public SensorsController(
            ISensorsService userSensorService,
            UserManager<User> userManager,
            IMemoryCache memoryCache)
        {
            this.sensorsService = userSensorService;
            this.userManager = userManager;
            this.memoryCache = memoryCache;
        }

        public async Task<IActionResult> Index()
        {
            var user = await this.userManager.GetUserAsync(HttpContext.User);
            var userSensors = await this.sensorsService.ListSensorsAsync(user.Id);

            var model = userSensors.Select(us => new UserIndexModel(us)).ToList();

            return View(model);
        }

        public async Task<JsonResult> ViewSensorsOnMap()
        {
            var user = await this.userManager.GetUserAsync(HttpContext.User);
            var listSensors = await this.sensorsService.ListSensorsAsync(user.Id);

            var data = listSensors.Select(s => new SensorsCoordinatesModel(s));

            return Json(data);
        }

        public async Task<IActionResult> SensorDetails(Guid userSensorid)
        {
            var userSensor = await this.sensorsService.GetUserSensorAsync(userSensorid);

            var model = new UserSensorDetailsModel(userSensor);

            return View("SensorDetails", model);
        }

        public async Task<IActionResult> ListSampleSensors(string userId)
        {
            var listSensors = await this.memoryCache.GetOrCreate
                ("List sample sensors", async entry =>
            {
                entry.AbsoluteExpiration = DateTime.UtcNow.AddHours(12);
                return await this.sensorsService.ListSampleSensorsAsync();
            });

            if (listSensors == null)
            {
               return NoContent();
            }

            var sampleSensorsModel = listSensors.Select(s => new SampleSensorViewModel(s));

            var model = new ListSampleSensorViewModel(sampleSensorsModel, userId);

            return View("ListSampleSensors", model);
        }

        [HttpGet]
        public async Task<IActionResult> RegisterNewSensor(Guid sampleSensorId, string userId)
        {
            var sensor = await this.sensorsService.GetSampleSensorAsync(sampleSensorId);

            var model = new RegisterSensorModel(sensor, userId);

            return View("RegisterNewSensor", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterNewSensor
            ([Bind(include: WebConstants.UserSensorViewModelBindingString)] RegisterSensorModel model)
        {
            if (!ModelState.IsValid)
            {
                this.TempData["Lng-Lat"] = "Please set the location of the sensor";
                return View(model);
            }
            if (model.UserMinValue >= model.UserMaxValue)
            {
                this.TempData["Invalid-Min-Max-Value"] = "MinValue can not be equal to or greater than MaxValue.";
                return View(model);
            }

            if (model.UserId == null)
            {
                var currentUser = await this.userManager.GetUserAsync(HttpContext.User);
                model.UserId = currentUser.Id;
            }

            var registrationData = new UserSensor()
            {
                Name = model.Name,
                PollingInterval = model.UserPollingInterval,
                SampleSensorId = model.SampleSensorId,
                UserId = model.UserId,
                UserMinValue = model.UserMinValue,
                UserMaxValue = model.UserMaxValue,
                Latitude = model.Latitude,
                Longitude = model.Longitude,
                SendNotification = model.SendNotification,
                IsPrivate = model.IsPrivate
            };

            var sensor = await this.sensorsService.RegisterSensorAsync(registrationData);

            this.TempData["Success-Message"] = $"Sensor {sensor.Name} was registered successfully!";

            return this.RedirectToAction("SensorDetails", new { userSensorid = sensor.Id });
        }

        [HttpGet]
        public async Task<IActionResult> EditSensor(Guid id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            var userSensor = await this.sensorsService.GetUserSensorAsync(id);

            if (userSensor == null)
            {
                return NotFound();
            }

            var model = new EditSensorModel(userSensor);

            return View("EditSensor", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSensor(
            [Bind(include: WebConstants.UserSensorViewModelBindingString)] EditSensorModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return View(model);
            }
            if (model.UserMinValue >= model.UserMaxValue)
            {
                this.TempData["Invalid-Min-Max-Value"] = "MinValue can not be equal to or greater than MaxValue.";
                return View(model);
            }

            var editedSensor = new UserSensor()
            {
                Id = model.Id,
                Name = model.Name,
                PollingInterval = model.UserPollingInterval,
                SampleSensorId = model.SampleSensorId,
                UserId = model.UserId,
                UserMinValue = model.UserMinValue,
                UserMaxValue = model.UserMaxValue,
                Latitude = model.Latitude,
                Longitude = model.Longitude,
                SendNotification = model.SendNotification,
                IsPrivate = model.IsPrivate,
                CreatedOn = model.RegisteredOn
            };

            var sensor = await this.sensorsService.EditSensorAsync(editedSensor);

            return this.RedirectToAction("SensorDetails", new { userSensorid = sensor.Id });
        }

        public async Task<IActionResult> DeleteSensor(Guid id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            var sensor = await this.sensorsService.DeleteUserSensorAsync(id);

            return this.RedirectToAction("SensorDetails", new { userSensorid = id });
        }
    }
}