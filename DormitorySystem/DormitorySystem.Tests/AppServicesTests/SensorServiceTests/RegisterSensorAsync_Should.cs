﻿using DormitorySystem.Common.Exceptions;
using DormitorySystem.Data.Abstractions;
using DormitorySystem.Data.Context;
using DormitorySystem.Data.Models;
using DormitorySystem.Services.AppServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DormitorySystem.Tests.AppServicesTests.SensorServiceTests
{
    [TestClass]
    public class RegisterSensorAsync_Should
    {
        [TestMethod]
        public async Task RegisterUserSensor_WhenValidParamsArePassed()
        {
            //Arrange
            var contextOptions = new DbContextOptionsBuilder<DormitorySystemContext>()
                .UseInMemoryDatabase("RegisterUserSensor_WhenValidParamsArePassed")
                .Options;

            var seedUsersMock = new Mock<ISeedUsers>();
            var seedApiDataMock = new Mock<ISeedApiData>();

            var measure = new Measure
            {
                Id = 1,
                MeasureType = "test"
            };
            var sensorType = new SensorType
            {
                Id = 1,
                Name = "Test"
            };
            var sampleSensor = new SampleSensor
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000222"),
                Tag = "Test Sensor",
                Description = "Test Sensor",
                MinPollingInterval = 20,
                MeasureId = measure.Id,
                ValueCurrent = 50,
                SensorTypeId = sensorType.Id,
                IsOnline = true
            };
            var userSensor = new UserSensor
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000111"),
                CreatedOn = DateTime.Now,
                isDeleted = false,
                SampleSensorId = sampleSensor.Id,
                PollingInterval = 100,
                SendNotification = true,
                IsPrivate = false
            };

            //Act
            using (var actContext = new DormitorySystemContext(contextOptions,
                seedUsersMock.Object, seedApiDataMock.Object))
            {
                actContext.Measures.Add(measure);
                actContext.SensorTypes.Add(sensorType);
                actContext.SampleSensors.Add(sampleSensor);     
                actContext.SaveChanges();
            }

            //Assert
            using (var assertContext = new DormitorySystemContext(contextOptions,
                seedUsersMock.Object, seedApiDataMock.Object))
            {
                var service = new SensorService(assertContext);
                var result = await service.RegisterSensorAsync(userSensor);

                Assert.AreEqual(1, assertContext.UserSensors.Count());
                Assert.IsTrue(assertContext.UserSensors.Contains(userSensor));
            }
        }

        [TestMethod]
        [ExpectedException(typeof(SensorNullableException))]
        public async Task ThrowSensorNullableException_WhenSensorIsNotPassed()
        {
            //Arrange
            var contextOptions = new DbContextOptionsBuilder<DormitorySystemContext>()
                .UseInMemoryDatabase("ThrowSensorNullableException_WhenSensorIsNotPassed")
                .Options;

            var seedUsersMock = new Mock<ISeedUsers>();
            var seedApiDataMock = new Mock<ISeedApiData>();

            //Assert
            using (var assertContext = new DormitorySystemContext(contextOptions,
                seedUsersMock.Object, seedApiDataMock.Object))
            {
                var service = new SensorService(assertContext);
                var result = await service.RegisterSensorAsync(null);
            }
        }
    }
}
