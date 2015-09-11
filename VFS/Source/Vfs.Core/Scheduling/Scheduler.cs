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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Vfs.Util;

namespace Vfs.Scheduling
{
  /// <summary>
  /// Provides simple mechanisms to submit and run jobs.
  /// </summary>
  public class Scheduler : IDisposable
  {
    /// <summary>
    /// The last interval that was assigned to the timer.
    /// This is used along with the <see cref="LastTimerRun"/>
    /// value to detect whether the system time was changed.
    /// </summary>
    protected long LastTimerInterval { get; set; }

    /// <summary>
    /// A timestamp that marks the last timer run. Internally used along with
    /// the <see cref="LastTimerInterval"/> in order to detect
    /// system time changes.
    /// </summary>
    protected DateTimeOffset LastTimerRun { get; private set; }

    /// <summary>
    /// Set to true as soon as the scheduler is disposed.
    /// </summary>
    protected bool IsDisposed { get; set; }

    /// <summary>
    /// Indicates whether the execution time of jobs should
    /// be rescheduled in case a change of the system time
    /// was detected. This can avoid delays or untimely execution
    /// of jobs where no fixed execution times are desired, but
    /// rather an execution within a time span.
    /// </summary>
    public ReschedulingStrategy SystemTimeChangeRescheduling { get; set; }

    /// <summary>
    /// An optional callback that handles exceptions that occur during
    /// the execution of scheduled jobs. The handler receives both the
    /// failing job and the exception that occurred.
    /// </summary>
    public Action<Job, Exception> JobExceptionHandler { get; set; }


    private int selfTestInterval = 120000;

    /// <summary>
    /// The minimum interval of the underlying timer in milliseconds.
    /// If the next job's planned execution time is above this interval,
    /// the timer runs anyway in order to reschedule or run suddenly
    /// pending jobs in case the system time was changed.<br/>
    /// Defaults to 120000 (2 minutes).
    /// </summary>
    public int SelfTestInterval
    {
      get { return selfTestInterval; }
      set
      {
        if (value < 100)
        {
          const string msg = "Self testing interval must be greater than 100 milliseconds.";
          throw new ArgumentOutOfRangeException("value", msg);
        }

        selfTestInterval = value;

        //reschedule processing
        lock(SyncRoot)
        {
          Reschedule();
        }

      }
    }

    private int minJobInterval = 100;

    /// <summary>
    /// The minimum interval in milliseconds that is being used within the internal
    /// job processing loop. Submitting a job with an interval
    /// below this threshold causes an exception.<br/>
    /// Defaults to 100 milliseconds.
    /// </summary>
    public int MinJobInterval
    {
      get { return minJobInterval; }
      set
      {
        if(value < 1)
        {
          const string msg = "Interval must be greater than zero milliseconds.";
          throw new ArgumentOutOfRangeException("value", msg);
        }
        minJobInterval = value;
      }
    }

    /// <summary>
    /// The internally used timer that is used to trigger
    /// execution of the individual callbacks.
    /// </summary>
    protected Timer Timer { get; set; }

    /// <summary>
    /// An internal list of maintained jobs. The list is sorted
    /// before processing.
    /// </summary>
    protected List<JobContext> Jobs { get; set; }

    /// <summary>
    /// Locking token.
    /// </summary>
    private readonly object syncRoot = new object();

    /// <summary>
    /// The scheduler's locking token.
    /// </summary>
    public object SyncRoot
    {
      get { return syncRoot; }
    }

    /// <summary>
    /// Indicates whether the internal job list needs sorting or not.
    /// </summary>
    protected bool IsSorted { get; set; }

    /// <summary>
    /// The time of the next execution.
    /// </summary>
    public DateTimeOffset? NextExecution { get; protected set; }


    /// <summary>
    /// Creates a new scheduler instance.
    /// </summary>
    public Scheduler()
    {
      Jobs = new List<JobContext>();
      Timer = new Timer(ProcessJobs, null, Timeout.Infinite, Timeout.Infinite);
    }


    /// <summary>
    /// Submits a generic job along with a callback that delivers the
    /// job's <see cref="Job{T}.Data"/>.
    /// </summary>
    /// <typeparam name="T">The type of the state object that can be
    /// attached to the job.</typeparam>
    /// <param name="job">The job to be submitted.</param>
    /// <param name="callback">A callback action that is being invoked
    /// every time the job runs.</param>
    /// <exception cref="ArgumentNullException">If one of the parameters
    /// is a null reference.</exception>
    /// <exception cref="InvalidOperationException">If the configuration does not allow
    /// proper scheduling, e.g. because several loops were specified, but the inverval is
    /// missing.</exception>
    public virtual void SubmitJob<T>(Job<T> job, Action<Job<T>, T> callback)
    {
      //wrap action and forward
      Action<Job> action = j =>
                             {
                               Job<T> genericJob = (Job<T>) j;
                               callback(genericJob, genericJob.Data);
                             };

      SubmitJob(job, action);
    }



    /// <summary>
    /// Submits a job to be executed.
    /// </summary>
    /// <param name="job">The job to be submitted.</param>
    /// <param name="callback">A callback action that is being invoked
    /// every time the job runs.</param>
    /// <exception cref="ArgumentNullException">If one of the parameters
    /// is a null reference.</exception>
    /// <exception cref="InvalidOperationException">If the configuration does not allow
    /// proper scheduling, e.g. because several loops were specified, but the inverval is
    /// missing.</exception>
    public virtual void SubmitJob(Job job, Action<Job> callback)
    {
      if (job == null) throw new ArgumentNullException("job");
      if (callback == null) throw new ArgumentNullException("callback");

      //make sure we have a valid interval
      TimeSpan? interval = job.Interval;
      if (interval.HasValue && interval.Value.TotalMilliseconds < MinJobInterval)
      {
        string msg = "Interval of {0} ms is too small - a minimum interval of {1} ms is accepted.";
        msg = String.Format(msg, interval.Value.TotalMilliseconds, MinJobInterval);
        throw new InvalidOperationException(msg);
      }

      JobContext context = new JobContext(job, callback);
      
      lock(SyncRoot)
      {
        //if this job is going to be the next one, we need to reconfigure
        //the timer. Do not reschedule if the next execution is imminent
        if (NextExecution == null || context.NextExecution <= NextExecution.Value)
        {
          //insert at index 0, which makes sure the new job runs first next timer event
          Jobs.Insert(0, context);

          //only reschedule if the next execution is not imminent
          if (NextExecution == null || NextExecution.Value.Subtract(SystemTime.Now()).TotalMilliseconds > MinJobInterval)
          {
            //no sorting required, but we need to adjust the timer
            Reschedule();
          }
        }
        else
        {
          //add at end of the list and mark list as unsorted
          //the job will be sorted and rescheduled on the next run (which is before
          //this job's execution time)
          Jobs.Add(context);
          IsSorted = false;
        }
      }
    }


    /// <summary>
    /// Tries to get a job from the internal cache.
    /// </summary>
    /// <param name="jobId">The job identifier.</param>
    /// <returns>The first job that matches the submitted ID, or null
    /// if no matching job was found.</returns>
    /// <remarks>Querying the scheduler for jobs might affect performance
    /// if done frequently.</remarks>
    public virtual Job TryGetJob(string jobId)
    {
      lock(SyncRoot)
      {
        var context = Jobs.FirstOrDefault(jc => jc.ManagedJob.JobId == jobId);
        return context == null ? null : context.ManagedJob;
      }
    }


    /// <summary>
    /// Pauses the current job.
    /// </summary>
    /// <param name="jobId">The job identifier.</param>
    /// <returns>True if the job's <see cref="Job.State"/>
    /// was <see cref="JobState.Active"/> and was changed
    /// to <see cref="JobState.Paused"/>. False if the job was not found, or if the job's
    /// <see cref="Job.State"/> is not <see cref="JobState.Active"/>.</returns>
    /// <exception cref="InvalidOperationException">If the job has no
    /// interval, and can thus not be rescheduled. For a job that runs
    /// just once, set the <see cref="Job.StartTime"/> accordingly.</exception>
    public virtual bool PauseJob(string jobId)
    {
      var job = TryGetJob(jobId);
      return job == null ? false : job.Pause();
    }


    /// <summary>
    /// Resumes a paused job.
    /// </summary>
    /// <param name="jobId">The job identifier.</param>
    /// <returns>True if the job's <see cref="Job.State"/>
    /// was <see cref="JobState.Paused"/> and was changed
    /// to <see cref="JobState.Active"/>. False if the job was not found, or if the job's
    /// <see cref="Job.State"/> is not <see cref="JobState.Paused"/>.</returns>
    public virtual bool ResumeJob(string jobId)
    {
      var job = TryGetJob(jobId);
      return job == null ? false : job.Resume();
    }



    /// <summary>
    /// Cancels a job with a given ID and removes it from the scheduler's internal
    /// cache.
    /// </summary>
    /// <param name="jobId">The job identifier.</param>
    /// <returns>True if a matching job was found and removed from
    /// the scheduler. False in case of an unknown job ID.</returns>
    public virtual bool CancelJob(string jobId)
    {
      lock (SyncRoot)
      {
        for (int i = 0; i < Jobs.Count; i++)
        {
          var job = Jobs[i];
          if(job.ManagedJob.JobId == jobId)
          {
            Jobs.RemoveAt(i);
            job.ManagedJob.Cancel();

            //if we just removed the next job, reschedule
            if(i == 0) Reschedule();

            return true;
          }
        }
      }

      return false;
    }


    /// <summary>
    /// Cancels all jobs.
    /// </summary>
    public virtual void CancelAll()
    {
      lock(SyncRoot)
      {
        if(IsDisposed) return;

        Timer.Change(Timeout.Infinite, Timeout.Infinite);
        NextExecution = null;
        Jobs.Clear();
      }
    }


    /// <summary>
    /// Timer event handler, which processes all currently
    /// active jobs.
    /// </summary>
    protected virtual void ProcessJobs(object state)
    {
      lock (SyncRoot)
      {
        if (IsDisposed) return;

        if(SystemTimeChangeRescheduling != ReschedulingStrategy.KeepFixedTimes)
        {
          VerifySystemTime();
        }

        //run and execute jobs
        RunPendingJobs();

        //if the list is not sorted, do so now to have the most
        //pending job at index 0
        if (!IsSorted) SortJobs();

        //plan the next execution
        Reschedule();
      }
    }


    /// <summary>
    /// Verifies the system time was not changed. If a changed time was
    /// detected, all jobs are being rescheduled in order to maintain the
    /// relative execution time.
    /// </summary>
    protected virtual void VerifySystemTime()
    {
      //we didn't have an interval at all
      if(LastTimerInterval == Timeout.Infinite) return;

      //get the time between now and the last timer we sent the timer to sleep
      var now = SystemTime.Now();
      var pauseDuration = now.Subtract(LastTimerRun);

      //subtract the timer's sleeping time - this value should be close to 0
      var delta = pauseDuration.TotalMilliseconds - LastTimerInterval;

      //the difference between the current time and the last execution
      //should roughly match the timer interval
      if(delta > 1000 || delta < 1000)
      {
        //reschedule all jobs
        bool changeExpirationTime = SystemTimeChangeRescheduling ==
                                    ReschedulingStrategy.RescheduleNextExecutionAndExpirationTime;
        Jobs.ForEach(jc =>
                       {
                         jc.NextExecution = jc.NextExecution.Value.AddMilliseconds(delta);
                         if(changeExpirationTime && jc.ManagedJob.ExpirationTime.HasValue)
                         {
                           jc.ManagedJob.ExpirationTime = jc.ManagedJob.ExpirationTime.Value.AddMilliseconds(delta);
                         }
                       });
      }

    }


    /// <summary>
    /// Sorts the job list. This method does not perform any locking, this must
    /// be done by the invoking party.
    /// </summary>
    protected virtual void SortJobs()
    {
      Jobs.Sort((first, second) => first.NextExecution.Value.CompareTo(second.NextExecution.Value));
      IsSorted = true;
    }




    /// <summary>
    /// Processes all currently pending jobs. If a job is not
    /// supposed to run anymore, it is being removed from the
    /// internal cache.
    /// </summary>
    protected virtual void RunPendingJobs()
    {
      //remove all jobs 
      DateTimeOffset now = SystemTime.Now();
      List<JobContext> dueJobs = new List<JobContext>();

      for (int i = 0; i < Jobs.Count; i++)
      {
        var job = Jobs[i];

        //if the job is not ready yet, break
        if (job.NextExecution.Value > now) break;

        //add to list of due jobs
        dueJobs.Add(job);
      }

      JobContext currentJob = null;
      try
      {
        for (int i = dueJobs.Count-1; i >=0; i--)
        {
          currentJob = dueJobs[i];
        
          //execute jobs
          currentJob.ExecuteAsync(this);

          //if the job will run again, set the IsSorted flag to false in order
          //to move the job to its new position in the list
          if (currentJob.NextExecution.HasValue)
          {
            IsSorted = false;
          }
          else
          {
            //remove the job from the job list
            Jobs.RemoveAt(i);
          }
        }
      }
      catch (Exception e)
      {
        // ReSharper disable PossibleNullReferenceException
        if(!SubmitJobException(currentJob.ManagedJob, e)) throw;
        // ReSharper restore PossibleNullReferenceException
      }
    }





    /// <summary>
    /// Reconfigures the timer according to the
    /// next pending job execution time.
    /// </summary>
    protected virtual void Reschedule()
    {
      if(IsDisposed) return;

      if(Jobs.Count == 0)
      {
        //disable the timer if we don't have any pending jobs
        NextExecution = null;
        Timer.Change(Timeout.Infinite, Timeout.Infinite);
        LastTimerInterval = Timeout.Infinite;
      }
      else
      {
        //schedule next event
        var executionTime = Jobs[0].NextExecution;

        DateTimeOffset now = SystemTime.Now();
        TimeSpan delay = executionTime.Value.Subtract(now);

        //in case the next execution is already pending, add a safe delay
        long dueTime = Math.Max(MinJobInterval, (long)delay.TotalMilliseconds);

        //change the timer - run at least with the self testing interval
        dueTime = Math.Min(dueTime, SelfTestInterval);

        NextExecution = SystemTime.Now().AddMilliseconds(dueTime);
        Timer.Change(dueTime, Timeout.Infinite);
        LastTimerInterval = dueTime;
      }

      LastTimerRun = SystemTime.Now();
    }


    /// <summary>
    /// Performs application-defined tasks associated with freeing,
    /// releasing, or resetting unmanaged resources.
    /// </summary>
    public virtual void Dispose()
    {
      lock(SyncRoot)
      {
        if(IsDisposed) return;

        IsDisposed = true;
        Timer.Change(Timeout.Infinite, Timeout.Infinite);
        Timer.Dispose();
      }
    }


    /// <summary>
    /// Forwards an exception to a registered <see cref="JobExceptionHandler"/>,
    /// if possible.
    /// </summary>
    /// <param name="job">The currently processed job. May be null.</param>
    /// <param name="exception">The exception that occurred.</param>
    /// <returns>True if the exception was forwarded to a registered
    /// <see cref="JobExceptionHandler"/>. False if no callback handler
    /// was registered.</returns>
    internal virtual bool SubmitJobException(Job job, Exception exception)
    {
      var handler = JobExceptionHandler;
      if (handler != null)
      {
        handler(job, exception);
        return true;
      }

      return false;
    }

  }
}
