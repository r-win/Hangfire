﻿﻿// This file is part of HangFire.
// Copyright © 2013 Sergey Odinokov.
// 
// HangFire is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// HangFire is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with HangFire.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using HangFire.Client;
using HangFire.Common.States;
using HangFire.States;
using ServiceStack.Redis;

namespace HangFire
{
    /// <summary>
    /// <p>The top-level class of the HangFire Client part. Provides several
    /// static methods to create jobs using guids as a unique identifier.</p>
    /// <p>All methods are thread-safe and use the <see cref="PooledRedisClientManager"/> 
    /// to take pooled Redis connections when creating a job.</p>
    /// </summary>
    [Obsolete("Old Client API is obsolete. Use static methods of the BackgroundJob class instead.")]
    public static class Perform
    {
        /// <summary>
        /// Enqueues a new argumentless job of the <typeparamref name="TJob"/> 
        /// type to its queue.
        /// </summary>
        /// 
        /// <typeparam name="TJob">Type of the job.</typeparam>
        /// <returns>The unique identifier of the job.</returns>
        /// 
        /// <exception cref="ArgumentException">The <see cref="BackgroundJob"/> type is not assignable from the given <typeparamref name="TJob"/>.</exception>
        /// <exception cref="InvalidOperationException">The <typeparamref name="TJob"/> has invalid queue name.</exception>
        /// <exception cref="CreateJobFailedException">Creation of the job was failed.</exception>
        public static string Async<TJob>()
            where TJob : BackgroundJob
        {
            return Async<TJob>((Type)null);
        }

        /// <summary>
        /// Enqueues a new job of the <typeparamref name="TJob"/> type to its
        /// queue with the specified arguments in the <paramref name="args"/> parameter.
        /// </summary>
        /// 
        /// <typeparam name="TJob">Type of the job</typeparam>
        /// <param name="args">Job arguments.</param>
        /// <returns>The unique identifier of the job.</returns>
        /// 
        /// <exception cref="ArgumentException">The <see cref="BackgroundJob"/> type is not assignable from the given <typeparamref name="TJob"/>.</exception>
        /// <exception cref="InvalidOperationException">The <typeparamref name="TJob"/> has invalid queue name.</exception>
        /// <exception cref="InvalidOperationException">Could not serialize one or more properties of the <paramref name="args"/> object using its <see cref="TypeConverter"/>.</exception>
        /// <exception cref="CreateJobFailedException">Creation of the job was failed.</exception>
        public static string Async<TJob>(object args)
            where TJob : BackgroundJob
        {
            return Async(typeof(TJob), args);
        }

        /// <summary>
        /// Enqueues a new argumentless job of the specified type to its queue.
        /// </summary>
        /// 
        /// <param name="type">Type of the job.</param>
        /// <returns>The unique identifier of the job.</returns>
        /// 
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is null.</exception>
        /// <exception cref="ArgumentException">The <see cref="BackgroundJob"/> type is not assignable from the given <paramref name="type"/>.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the <paramref name="type"/> has invalid queue name.</exception>
        /// <exception cref="CreateJobFailedException">Thrown when job creation was failed.</exception>
        public static string Async(Type type)
        {
            return Async(type, (object)null);
        }

        /// <summary>
        /// Enqueues a new job of the specified type to its queue with the 
        /// given arguments in the <paramref name="args"/> parameter.
        /// </summary>
        /// 
        /// <param name="type">Type of the job.</param>
        /// <param name="args">Job arguments.</param>
        /// <returns>The unique identifier of the job.</returns>
        /// 
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is null.</exception>
        /// <exception cref="ArgumentException">The <see cref="BackgroundJob"/> type is not assignable from the given <paramref name="type"/>.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the <paramref name="type"/> has invalid queue name.</exception>
        /// <exception cref="InvalidOperationException">Could not serialize one or more properties of the <paramref name="args"/> object using its <see cref="TypeConverter"/>.</exception>
        /// <exception cref="CreateJobFailedException">Thrown when job creation was failed.</exception>
        public static string Async(Type type, object args)
        {
            using (var client = new JobClient(RedisFactory.PooledManager))
            {
                var enqueuedState = new EnqueuedState("Enqueued by the Сlient");
                var uniqueId = GenerateId();
                
                client.CreateJob(uniqueId, type, enqueuedState, PropertiesToDictionary(args));
                return uniqueId;
            }
        }

        /// <summary>
        /// Schedules a new argumentless job of the specified type to perform 
        /// after the given <paramref name="delay"/>.
        /// </summary>
        /// 
        /// <typeparam name="TJob">The type of the job.</typeparam>
        /// <param name="delay">Delay, after which the job should be performed.</param>
        /// <returns>The unique identifier of the job.</returns>
        /// 
        /// <exception cref="ArgumentException">The <see cref="BackgroundJob"/> type is not assignable from the given <typeparamref name="TJob"/>.</exception>
        /// <exception cref="CreateJobFailedException">Thrown when job creation was failed.</exception>
        public static string In<TJob>(TimeSpan delay)
            where TJob : BackgroundJob
        {
            return In<TJob>(delay, (Type)null);
        }

        /// <summary>
        /// Schedules a new job of the specified type to perform after the 
        /// given <paramref name="delay"/> with the arguments defined in 
        /// the <paramref name="args"/> parameter.
        /// </summary>
        /// 
        /// <typeparam name="TJob">The type of the job.</typeparam>
        /// <param name="delay">Delay, after which the job should be performed.</param>
        /// <param name="args">Job arguments.</param>
        /// <returns>The unique identifier of the job.</returns>
        /// 
        /// <exception cref="ArgumentException">The <see cref="BackgroundJob"/> type is not assignable from the given <typeparamref name="TJob"/>.</exception>
        /// <exception cref="InvalidOperationException">Could not serialize one or more properties of the <paramref name="args"/> object using its <see cref="TypeConverter"/>.</exception>
        /// <exception cref="CreateJobFailedException">Thrown when job creation was failed.</exception>
        public static string In<TJob>(TimeSpan delay, object args)
            where TJob : BackgroundJob
        {
            return In(delay, typeof(TJob), args);
        }

        /// <summary>
        /// Schedules a new argumentless job of the specified type to perform 
        /// after the given <paramref name="delay"/>.
        /// </summary>
        /// 
        /// <param name="delay">Delay, after which the job should be performed.</param>
        /// <param name="type">The type of the job.</param>
        /// <returns>The unique identifier of the job.</returns>
        /// 
        /// <exception cref="ArgumentException">The <see cref="BackgroundJob"/> type is not assignable from the given <paramref name="type"/>.</exception>
        /// <exception cref="CreateJobFailedException">Thrown when job creation was failed.</exception>
        public static string In(TimeSpan delay, Type type)
        {
            return In(delay, type, null);
        }

        /// <summary>
        /// Schedules a new job of the specified type to perform after the given
        /// <paramref name="delay"/> with the arguments defined in the
        /// <paramref name="args"/> parameter.
        /// </summary>
        /// 
        /// <param name="delay">Delay, after which the job should be performed.</param>
        /// <param name="type">The type of the job.</param>
        /// <param name="args">Job arguments.</param>
        /// <returns>The unique identifier of the job.</returns>
        /// 
        /// <exception cref="ArgumentException">The <see cref="BackgroundJob"/> type is not assignable from the given <paramref name="type"/>.</exception>
        /// <exception cref="InvalidOperationException">Could not serialize one or more properties of the <paramref name="args"/> object using the <see cref="TypeConverter"/>.</exception>
        /// <exception cref="CreateJobFailedException">Thrown when job creation was failed.</exception>
        public static string In(TimeSpan delay, Type type, object args)
        {
            using (var client = new JobClient(RedisFactory.BasicManager))
            {
                var scheduledState = new ScheduledState("Scheduled by the Client", DateTime.UtcNow.Add(delay));
                var uniqueId = GenerateId();

                client.CreateJob(uniqueId, type, scheduledState, PropertiesToDictionary(args));
                return uniqueId;
            }
        }

        /// <summary>
        /// Generates a unique identifier for the job.
        /// </summary>
        /// <returns>Unique identifier for the job.</returns>
        private static string GenerateId()
        {
            return Guid.NewGuid().ToString();
        }

        private static IDictionary<string, string> PropertiesToDictionary(object obj)
        {
            var result = new Dictionary<string, string>();
            if (obj == null) return result;

            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(obj))
            {
                var propertyValue = descriptor.GetValue(obj);
                string value = null;

                if (propertyValue != null)
                {
                    var converter = TypeDescriptor.GetConverter(propertyValue.GetType());

                    try
                    {
                        value = converter.ConvertToInvariantString(propertyValue);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException(
                            String.Format(
                                "Could not convert property '{0}' of type '{1}' to a string using the '{2}'. See the inner exception for details.",
                                descriptor.Name,
                                descriptor.PropertyType,
                                converter.GetType()),
                            ex);
                    }
                }

                result.Add(descriptor.Name, value);
            }

            return result;
        }
    }
}
