﻿using System;
using System.Linq;
using DormitorySystem.Data.Models;
using DormitorySystem.Services.Abstractions;
using DormitorySystem.Services.Exceptions;
using DormitorySystem.Web.Areas.Users.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DormitorySystem.Web.Areas.Users.Controllers
{
    [Area("Users")]
    [Authorize(Roles = "User, Admin")]
    public class SensorsController : Controller
    {
        private readonly ISensorsService sensorsService;
        private readonly UserManager<User> userManager;
        private readonly IUsersService usersService;

        public SensorsController(
            ISensorsService userSensorService,
            UserManager<User> userManager,
            IUsersService userService)
        {
            this.sensorsService = userSensorService;
            this.usersService = userService;
            this.userManager = userManager;
        }

        public IActionResult Index()
        {
            var user = this.userManager.GetUserId(HttpContext.User);
            var userSensors = this.sensorsService.ListSensors(user);
            var model = userSensors.Select(us => new HomeIndexViewModel(us)).ToList();

            return View(model);
        }

        public IActionResult SensorDetails(Guid userSensorid)
        {
            var model = new UserSensorViewModel(this.sensorsService.GetUserSensor(userSensorid));
            return View(model);
        }

        public IActionResult ListSampleSensors()
        {
            var sampleSensors = this.sensorsService.ListSampleSensors()
                .Select(s => new SampleSensorViewModel(s));

            if (sampleSensors == null)
            {
                throw new SensorNullableException("No sensors at the moment.");
            }

            return View(new ListSampleSensorsViewModel(sampleSensors));
        }

        [HttpGet]
        public IActionResult RegisterNewSensor(string tag, string description)
        {
            var model = new UserSensorViewModel()
            {
                SampleSensor = new SampleSensor { Tag = tag, Description = description }
            };
            return View(model);
        }


        // TO DO Add min polling interval in model and Sensor Type in View
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RegisterNewSensor(Guid id, UserSensorViewModel model)
        {
            var userId = this.userManager.GetUserId(HttpContext.User);

            if (!ModelState.IsValid)
            {
                return View();
            }
            var sensor = this.sensorsService.RegisterSensor(userId, id, model.Name,
                           model.UserPollingInterval, model.Latitude, model.Longitude,
                           model.SendNotification, model.IsPrivate);

            this.TempData["Success-Message"] = $"Sensor {sensor.Name} was registered successfully!";
            return this.RedirectToAction("Index", "Sensors");
        }
        [HttpGet]
        public IActionResult EditSensor(Guid id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            var userSensor = this.sensorsService.GetUserSensor(id);

            if (userSensor == null)
            {
                return NotFound();
            }
            return View(new UserSensorViewModel(userSensor));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditSensor([Bind(include: "Id, Name, UserPollingInterval, Latitude, Longitude, SendNotification, IsPrivate")]UserSensorViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return View();
            }

            var sensor = this.sensorsService.EditSensor(model.Id, model.Name, model.UserPollingInterval,
                model.Latitude, model.Longitude, model.SendNotification, model.IsPrivate);

            return this.RedirectToAction("Index", "Sensors");
        }
    }
}