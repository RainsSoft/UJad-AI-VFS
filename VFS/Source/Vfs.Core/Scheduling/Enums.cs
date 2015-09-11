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


namespace Vfs.Scheduling
{
  /// <summary>
  /// State flags for maintainted jobs.
  /// </summary>
  public enum JobState
  {
    /// <summary>
    /// The job is currently active.
    /// </summary>
    Active,
    /// <summary>
    /// The job was paused.
    /// </summary>
    Paused,
    /// <summary>
    /// The job was finished, either because it ran
    /// the designated amount of times or expired.
    /// </summary>
    Finished,
    /// <summary>
    /// The job was aborted.
    /// </summary>
    Canceled
  }


  /// <summary>
  /// Defines how the <see cref="Scheduler"/> class performs
  /// rescheduling if a changed system time was detected.
  /// </summary>
  public enum ReschedulingStrategy
  {
    /// <summary>
    /// Jobs are not rescheduled if a system time
    /// change was detected.
    /// </summary>
    KeepFixedTimes,
    /// <summary>
    /// Only the jobs' next execution time is rescheduled.
    /// The expiration time remains fixed.
    /// </summary>
    RescheduleNextExecution,
    /// <summary>
    /// Both the jobs' next execution times and the
    /// expiration times are being shifted according
    /// to the time change.
    /// </summary>
    RescheduleNextExecutionAndExpirationTime
  }
}