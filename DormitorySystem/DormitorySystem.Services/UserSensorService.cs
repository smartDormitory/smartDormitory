﻿using DormitorySystem.Data.Context;
using DormitorySystem.Data.Models;
using DormitorySystem.Services.Abstractions;
using DormitorySystem.Services.Exceptions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DormitorySystem.Services
{
    public class UserSensorService : IUserSensorService
    {
        private readonly DormitorySystemContext context;

        public UserSensorService(DormitorySystemContext context)
        {
            this.context = context;
        }

        public IEnumerable<UserSensor> GetPublicSensors()
        {
            var sensors = this.context.UserSensors
                .Where(s => s.IsPrivate == false)
                .Include(us => us.User)
                    .ThenInclude(u => u.Email)
                .Include(us => us.SampleSensor)
                    .ThenInclude(ss => ss.Tag)
                .ToList();

            return sensors;
        }

        public UserSensor GetSensor(Guid id)
        {
            var sensor = this.context.UserSensors
                .Include(us => us.User)
                    .ThenInclude(u => u.Email)
                .Include(us => us.SampleSensor)
                    .ThenInclude(ss => ss.Tag)
                 .SingleOrDefault(s => s.Id == id);
            if (sensor == null)
            {
                throw new SensorNullableException("There is no such sensor.");
            }

            return sensor;
        }

        public IDictionary<string, IEnumerable<SampleSensor>> GetSensorByType()
        {
            var sensorList = new Dictionary<string, IEnumerable<SampleSensor>>();

            var sensorTypes = this.context.SensorTypes
                .Include(s => s.SampleSensors)
                .ThenInclude(m => m.Measure)
                .ToArray();

            foreach (var sensorType in sensorTypes)
            {
                string typeName = sensorType.Name;

                if (!sensorList.ContainsKey(typeName))
                {
                    sensorList.Add(typeName, sensorType.SampleSensors);
                }
                else
                {
                    sensorList[typeName] = sensorType.SampleSensors;
                }
            }

            return sensorList;
        }
    }
}
