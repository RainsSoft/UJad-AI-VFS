// Lightweight Job Scheduling Library for .NET
// Copyright (c) 2010 Philipp Sumi
// Contact and Information: http://www.hardcodet.net
//
// This library is free software; you can redistribute it and/or
// modify it in any way you see fit as long as this copyright
// notice is not being removed.
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
//
// THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE


using System;


namespace Vfs.Scheduling
{
  /// <summary>
  /// Provides the fluent API to configure a given <see cref="Job"/>.
  /// </summary>
  public class JobSchedule
  {
    private readonly Job job;

    /// <summary>
    /// Creates the instance object with the job to be
    /// configured.
    /// </summary>
    public JobSchedule(Job job)
    {
      this.job = job;
    }


    /// <summary>
    /// Sets the job's starting time.
    /// </summary>
    /// <param name="startTime">The first execution of the job.</param>
    public JobSchedule From(DateTimeOffset startTime)
    {
      job.StartTime = startTime;
      return this;
    }

    /// <summary>
    /// Configures the job to run only once.
    /// </summary>
    public JobSchedule Once()
    {
      job.Loops = 1;
      job.ExpirationTime = null;
      job.Interval = null;
      return this;
    }


    /// <summary>
    /// Defines an interval for periodic execution of the
    /// job. The interval needs to be set if the job is
    /// supposed to run more than once.
    /// </summary>
    /// <param name="jobInterval">The job's planned
    /// interval.</param>
    internal JobSchedule EveryInternal(TimeSpan jobInterval)
    {
      job.Interval = jobInterval;
      return this;
    }

    /// <summary>
    /// Defines an interval for periodic execution of the
    /// job. The interval needs to be set if the job is
    /// supposed to run more than once.
    /// </summary>
    public Intervals Every
    {
      get { return new Intervals(this);  }
    }

    /// <summary>
    /// Configures the job to run a fixed number of times.
    /// </summary>
    /// <param name="loops">The number of times the job
    /// is supposed to run.</param>
    public JobSchedule Times(int loops)
    {
      job.Loops = loops;
      return this;
    }


    /// <summary>
    /// Specifies an <see cref="Job.ExpirationTime"/> for the job. Not
    /// required it the job runs only a number of times, or is supposed
    /// to run indefinitely with the specified <see cref="Job.Interval"/>.
    /// </summary>
    /// <param name="jobExpirationTime">The specified expiration time.</param>
    public JobSchedule Until(DateTimeOffset jobExpirationTime)
    {
      job.ExpirationTime = jobExpirationTime;
      return this;
    }
  }


  /// <summary>
  /// Helper struct that simplifies interval configuration.
  /// </summary>
  public struct Intervals
  {

    private readonly JobSchedule schedule;


    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.Object"/> class.
    /// </summary>
    public Intervals(JobSchedule schedule)
    {
      this.schedule = schedule;
    }

    /// <summary>
    /// Sets an interval of a given number of milliseconds.
    /// </summary>
    /// <param name="value">The interval in milliseconds.</param>
    public JobSchedule Milliseconds(long value)
    {
      return schedule.EveryInternal(System.TimeSpan.FromMilliseconds(value));
    }


    /// <summary>
    /// Sets an interval of a given number of seconds.
    /// </summary>
    /// <param name="value">The interval in seconds.</param>
    public JobSchedule Seconds(double value)
    {
      return schedule.EveryInternal(System.TimeSpan.FromSeconds(value));
    }


    /// <summary>
    /// Sets an interval of a given number of minutes.
    /// </summary>
    /// <param name="value">The interval in minutes.</param>
    public JobSchedule Minutes(double value)
    {
      return schedule.EveryInternal(System.TimeSpan.FromMinutes(value));
    }

    public JobSchedule Hours(double value)
    {
      return schedule.EveryInternal(System.TimeSpan.FromHours(value));
    }
    

    /// <summary>
    /// Sets an interval of a given number of days.
    /// </summary>
    /// <param name="value">The interval in days.</param>
    public JobSchedule Days(double value)
    {
      return schedule.EveryInternal(System.TimeSpan.FromDays(value));
    }


    /// <summary>
    /// Sets an interval of a given time span.
    /// </summary>
    /// <param name="value">The interval.</param>
    public JobSchedule TimeSpan(TimeSpan value)
    {
      return schedule.EveryInternal(value);
    }


  }


}
