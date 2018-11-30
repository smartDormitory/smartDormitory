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
    public class HomeController : Controller
    {
        private readonly IUserSensorService _userSensorService;
        private readonly UserManager<User> _userManager;
        private readonly IUserService _userService;

        public HomeController(
            IUserSensorService userSensorService,
            UserManager<User> userManager,
            IUserService userService)
        {
            this._userSensorService = userSensorService;
            this._userService = userService;
            this._userManager = userManager;
        }

        public IActionResult Index()
        {
            var user = this._userManager.GetUserId(HttpContext.User);
            var userSensors = this._userService.ListSensors(user);
            var model = userSensors.Select(us => new HomeIndexViewModel(us)).ToList();

            return View(model);
        }

        public IActionResult SensorDetails(Guid userSensorid)
        {

            return View();
        } 

        public IActionResult ListSampleSensors()
        {
            var sampleSensors = this._userSensorService.ListSampleSensors()
                .Select(s => new SampleSensorViewModel(s));

            if (sampleSensors == null)
            {
                throw new SensorNullableException("No sensors at the moment.");
            }

            return View(new ListSampleSensorsViewModel(sampleSensors));
        }

        [HttpGet]
        public IActionResult RegisterNewSensor()
        {         
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RegisterNewSensor(Guid id, UserSensorViewModel model)
        {
            var userId = this._userManager.GetUserId(HttpContext.User);
           
            if (ModelState.IsValid)
            {
                this._userService.RegisterSensor(userId, id, model.Name,
                               model.UserPollingInterval, model.Latitude, model.Longitude,
                               model.SendNotification, model.IsPrivate);
            }

            return this.RedirectToAction("Index", "Home");
        }
    }
}