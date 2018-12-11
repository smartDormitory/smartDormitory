﻿using DormitorySystem.Data.Context;
using DormitorySystem.Data.Models;
using DormitorySystem.Services.Abstractions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DormitorySystem.Services.BackgroundService
{
    public class NotificationsService : INotificationsService
    {
        private readonly DormitorySystemContext context;
        private readonly IHubService hubService;

        public NotificationsService(DormitorySystemContext context, IHubService hubService)
        {
            this.context = context;
            this.hubService = hubService;
        }

        public async Task CheckForOutOfRangeSensorsAsync(IDictionary<string, SampleSensor> listOfSensors)
        {
            foreach (var sampleSensor in listOfSensors.Values)
            {
                var userSensors = this.context.UserSensors
                    .Where(us => us.SampleSensorId == sampleSensor.Id).ToList();

                foreach (var userSensor in userSensors)
                {
                    if ((sampleSensor.ValueCurrent > userSensor.UserMaxValue
                        || sampleSensor.ValueCurrent < userSensor.UserMinValue)
                        && userSensor.SendNotification)
                    {
                        await hubService.Notify(userSensor.UserId, userSensor.Name);
                    }
                }
            }
        }
    }
}
