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
using System.Threading;
using Vfs.Util;

namespace Vfs.Scheduling
{
  /// <summary>
  /// Contains the context of a managed job. Internally used by
  /// the scheduler.
  /// </summary>
  public class JobContext
  {
    /// <summary>
    /// The callback action that is being invoked.
    /// </summary>
    public Action<Job> CallbackAction { get; protected set; }

    /// <summary>
    /// The maintained job.
    /// </summary>
    public Job ManagedJob { get; set; }

    /// <summary>
    /// The timestamp of the last job execution. Returns null
    /// if the job hasn't been executed yet.
    /// </summary>
    public DateTimeOffset? LastJobEvaluation { get; set; }

    /// <summary>
    /// The next execution time. Returns null if the job is done.
    /// After initialization, this is the job's <see cref="Job.StartTime"/>.
    /// </summary>
    public DateTimeOffset? NextExecution { get; set; }

    /// <summary>
    /// The remaining executions, if the job was configured
    /// to run a specific number of times.
    /// </summary>
    public int? RemainingExecutions { get; set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.Object"/> class.
    /// </summary>
    /// <exception cref="ArgumentNullException">If one of the parameters is
    /// is a null reference.</exception>
    /// <exception cref="InvalidOperationException">If the configuration does not allow
    /// proper scheduling, e.g. because several loops were specified, but the inverval is
    /// missing.</exception>
    public JobContext(Job managedJob, Action<Job> callbackAction)
    {
      if (managedJob == null) throw new ArgumentNullException("managedJob");
      if (callbackAction == null) throw new ArgumentNullException("callbackAction");

      //make sure we have an interval if we have more than one loop
      TimeSpan? interval = managedJob.Interval;
      if (interval == null)
      {
        if (managedJob.Loops == null || managedJob.Loops.Value > 1)
        {
          string msg = "Job [{0}] is invalid: Specifiy either a single run, or a loop interval.";
          msg = String.Format(msg, managedJob.JobId);
          throw new InvalidOperationException(msg);
        }
      }

      ManagedJob = managedJob;
      CallbackAction = callbackAction;
      NextExecution = managedJob.StartTime;

      //adjust starting time if the initial time was in the past,
      //but rather start immediately
      var now = SystemTime.Now();
      if (NextExecution.Value < now) NextExecution = now;

      RemainingExecutions = managedJob.Loops;
    }



    /// <summary>
    /// Invokes the managed job's <see cref="CallbackAction"/> through
    /// the thread pool, and updates the job's internal state.
    /// </summary>
    public virtual void ExecuteAsync(Scheduler scheduler)
    {
      //only execute if the job is neither aborted, expired, or paused
      if (ManagedJob.State == JobState.Active && (ManagedJob.ExpirationTime == null || ManagedJob.ExpirationTime >= SystemTime.Now()))
      {
        ThreadPool.QueueUserWorkItem(s =>
                                       {
                                         try
                                         {
                                           CallbackAction(ManagedJob);
                                         }
                                         catch (Exception e)
                                         {
                                           //do not reference the scheduler instance to avoid closure
                                           Scheduler sch = (Scheduler) s;
                                           if(!sch.SubmitJobException(ManagedJob, e)) throw;
                                         }
                                       }, scheduler);
      }

      UpdateState();
    }


    /// <summary>
    /// Updates the internal state after an execution, and updates the
    /// <see cref="LastJobEvaluation"/> and <see cref="NextExecution"/>
    /// values. If the job is not supposed to run anymore, the
    /// <see cref="NextExecution"/> property is set to null.
    /// </summary>
    protected virtual void UpdateState()
    {
      LastJobEvaluation = SystemTime.Now();

      lock (ManagedJob.SyncRoot)
      {
        if (ManagedJob.State == JobState.Canceled)
        {
          NextExecution = null;
          return;
        }


        if (RemainingExecutions.HasValue)
        {
          //only decrease the loop counter if the job is not paused
          if (ManagedJob.State == JobState.Active)
          {
            RemainingExecutions--;
          }

          //we're done if we peformend the last loop
          if (RemainingExecutions == 0)
          {
            //we have a loop, and completed it
            ManagedJob.State = JobState.Finished;
            NextExecution = null;
            return;
          }
        }


        //if there is no reoccurrence interval, we cannot calculate a new run
        //-> cancel
        if (!ManagedJob.Interval.HasValue)
        {
          ManagedJob.State = JobState.Canceled;
          NextExecution = null;
          return;
        }

        //schedule next execution - even if this is beyond expiration
        //(we don't cancel as long as expiration happened)
        NextExecution = (NextExecution.Value).Add(ManagedJob.Interval.Value);

        //if the next job is beyond expiration, reset again
        if (ManagedJob.ExpirationTime.HasValue && LastJobEvaluation.Value > ManagedJob.ExpirationTime)
        {
          ManagedJob.State = JobState.Finished;
          NextExecution = null;
        }
      }
    }



  }
}