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
using Vfs.Util;

namespace Vfs.Scheduling
{
  /// <summary>
  /// Encapsulates the scheduling for a given job.
  /// </summary>
  public class Job
  {
    /// <summary>
    /// A unique identifier for the job.
    /// </summary>
    public string JobId { get; set; }

    /// <summary>
    /// Gets a synchronization token for the job.
    /// </summary>
    public object SyncRoot
    {
      get { return this; }
    }

    /// <summary>
    /// The designated start time for the job. If not set, the
    /// job starts immediately.
    /// </summary>
    public virtual DateTimeOffset StartTime { get; set; }

    private TimeSpan? interval;

    /// <summary>
    /// The interval of the job. Required if the job is to supposed multiple
    /// times.
    /// </summary>
    public virtual TimeSpan? Interval
    {
      get { return interval; }
      set
      {
        if (interval.HasValue && interval.Value.TotalMilliseconds < 0)
        {
          string msg = "Invalid interval of {0} milliseconds. Interval must be a positive value or [null].";
          msg = String.Format(msg, interval.Value.TotalMilliseconds);
          throw new ArgumentOutOfRangeException("value", msg);
        }

        lock (SyncRoot)
        {
          interval = value;
        }
      }
    }

    private int? loops;

    /// <summary>
    /// The number of executed loops. If set, the
    /// job runs the specified number of times, unless
    /// it aborts because the <see cref="ExpirationTime"/>
    /// was set and causes the job to be cancelled earlier.
    /// </summary>
    public virtual int? Loops
    {
      get { return loops; }
      set
      {
        if (value.HasValue && value < 1)
        {
          string msg = "Invalid number of loops: {0}. Only numbers above zero or [null] are allowed.";
          msg = String.Format(msg, value);
          throw new ArgumentOutOfRangeException("value", msg);
        }

        lock (SyncRoot)
        {
          loops = value;
        }
      }
    }


    private DateTimeOffset? expirationTime;

    /// <summary>
    /// The expiration time of the job. This date is optional
    /// in case the job runs only a specified number of times,
    /// or indefinitely.
    /// </summary>
    public virtual DateTimeOffset? ExpirationTime
    {
      get { return expirationTime; }
      set
      {
        if (value.HasValue && value < SystemTime.Now())
        {
          const string msg = "Expiration time cannot be in the past";
          throw new ArgumentOutOfRangeException("value", msg);
        }

        lock (SyncRoot)
        {
          expirationTime = value;
        }
      }
    }

    /// <summary>
    /// The job's current state.
    /// </summary>
    public virtual JobState State { get; protected internal set; }


    /// <summary>
    /// Creates a new job, and generates a unique job ID on the fly.
    /// </summary>
    public Job() : this(Guid.NewGuid().ToString())
    {
    }

    /// <summary>
    /// Initializes a new job with a given identifier.
    /// </summary>
    /// <param name="jobId">A unique ID for the job. Unique
    /// job IDs are not evaluated by the scheduler, the calling
    /// party must ensure unique IDs.</param>
    public Job(string jobId)
    {
      JobId = jobId;
    }


    /// <summary>
    /// Configures how often the job is being repeated.
    /// </summary>
    public virtual JobSchedule Run
    { 
      get { return new JobSchedule(this); }
    }

    /// <summary>
    /// Cancels the job in order to have it removed
    /// during the next processing loop. This method
    /// can be invoked by clients in order to
    /// cancel job execution without having to reference
    /// the job's <see cref="Scheduler"/>.
    /// </summary>
    public virtual void Cancel()
    {
      lock (this)
      {
        State = JobState.Canceled;
      }
    }


    /// <summary>
    /// Pauses the current job.
    /// </summary>
    /// <returns>True if the job's <see cref="State"/>
    /// was <see cref="JobState.Active"/> and was changed
    /// to <see cref="JobState.Paused"/>. False if the job's
    /// <see cref="State"/> is not <see cref="JobState.Active"/>.</returns>
    /// <exception cref="InvalidOperationException">If the job has no
    /// interval, and can thus not be rescheduled. For a job that runs
    /// just once, set the <see cref="StartTime"/> accordingly.</exception>
    public virtual bool Pause()
    {
      if(!Interval.HasValue)
      {
        const string msg = "Jobs without interval cannot be paused " 
                           + "- the scheduler does not know how to schedule it once it is resumed.";
        throw new InvalidOperationException(msg);
      }

      lock (this)
      {
        if (State != JobState.Active) return false;
        State = JobState.Paused;
        return true;
      }
    }

    /// <summary>
    /// Resumes a paused job.
    /// </summary>
    /// <returns>True if the job's <see cref="State"/>
    /// was <see cref="JobState.Paused"/> and was changed
    /// to <see cref="JobState.Active"/>. False if the job's
    /// <see cref="State"/> is not <see cref="JobState.Paused"/>.</returns>
    public virtual bool Resume()
    {
      lock (this)
      {
        if (State != JobState.Paused) return false;
        State = JobState.Active;
        return true;
      }
    }
  }


  /// <summary>
  /// A generic job implementation which allows to attach
  /// strongly typed state information directly to the
  /// job.
  /// </summary>
  /// <typeparam name="T">The type of the job's
  /// <see cref="Data"/> object.</typeparam>
  public class Job<T> : Job
  {
    /// <summary>
    /// Gets or sets the state object that is attached to the job.
    /// </summary>
    public T Data { get; set; }

    /// <summary>
    /// Creates a new job, and generates a unique job identifier on the fly.
    /// </summary>
    public Job()
    {
    }

    /// <summary>
    /// Initializes a new job with a given identifier.
    /// </summary>
    public Job(string id) : base(id)
    {
    }
  }
}