namespace RestClient;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Retry handler.
/// </summary>
public class Retry
    : IRetry
{
    /// <summary>
    /// A Random value generator.
    /// </summary>
    protected static readonly Random Randomizer = new Random();

    /// <summary>
    /// A list of retry exceptions.
    /// </summary>
    private readonly List<Type> exceptions = new List<Type>();

    /// <summary>
    /// A list of exception retry handlers.
    /// </summary>
    private readonly List<RetryHandler> exceptionHandlers = new List<RetryHandler>();

    /// <summary>
    /// A list of result retry handlers.
    /// </summary>
    private readonly List<RetryHandler> resultHandlers = new List<RetryHandler>();

    /// <summary>
    /// The operation completed handler.
    /// </summary>
    private Action<TimeSpan> operationCompletedHandler;

    /// <summary>
    /// Gets the retry limit.
    /// </summary>
    public int Limit { get; private set; } = 3;

    /// <summary>
    /// Gets the retry limit.
    /// </summary>
    public TimeSpan WaitTime { get; private set; } = TimeSpan.FromMilliseconds(500);

    /// <summary>
    /// Gets the maximum time to wait between retries.
    /// </summary>
    public TimeSpan MaxWaitTime { get; private set; }  = TimeSpan.MaxValue;

    /// <summary>
    /// Gets the wait time increment.
    /// </summary>
    public TimeSpan WaitTimeIncrement { get; private set; } = TimeSpan.FromMilliseconds(500);

    /// <summary>
    /// Gets the wait time variance.
    /// </summary>
    public TimeSpan WaitTimeVariance { get; private set; } = TimeSpan.FromMilliseconds(500);

    /// <summary>
    /// Gets a value indicating whether or not to double the wait time on retry.
    /// </summary>
    public bool DoubleWaitTimeOnRetry { get; private set; } = true;

    /// <summary>
    /// Gets a value indicating whether or not to vary the wait time on retry.
    /// </summary>
    public bool VaryWaitTimeOnRetry { get; private set; } = false;

    /// <summary>
    /// Gets a value indicating whether or not to randomize the initial wait time.
    /// </summary>
    public bool RandomizeInitialWaitTime { get; private set; } = false;

    /// <summary>
    /// Gets the list of exception handlers.
    /// </summary>
    public IEnumerable<RetryHandler> ExceptionHandlers => this.exceptionHandlers;

    /// <summary>
    /// Sets the retry limit.
    /// </summary>
    /// <param name="limit">The limit.</param>
    /// <returns>The <see cref="Retry"/> instance.</returns>
    public Retry SetRetryLimit(int limit)
    {
        this.Limit = limit;
        return this;
    }

    /// <summary>
    /// Sets a value indicating whether or not to tom randomize the initial wait time.
    /// </summary>
    /// <param name="value">A value indicating whether or not to randomize the initial wait time.</param>
    /// <returns>The <see cref="Retry"/> instance.</returns>
    public Retry SetRandomWaitTime(bool value)
    {
        this.RandomizeInitialWaitTime = value;
        return this;
    }

    /// <summary>
    /// Sets a value indicating whether or not to vary the wait time on retry.
    /// </summary>
    /// <param name="value">A value indicating whether or not to vary the wait time on retry.</param>
    /// <returns>The <see cref="Retry"/> instance.</returns>
    public Retry SetVaryWaitTimeOnRetry(bool value)
    {
        this.VaryWaitTimeOnRetry = value;
        return this;
    }

    /// <summary>
    /// Sets the amount of time to increment the wait by when retrying.
    /// </summary>
    /// <param name="value">The amount of time to increment the wait time by when retrying.</param>
    /// <returns>The <see cref="Retry"/> instance.</returns>
    public Retry SetWaitTimeIncrement(TimeSpan value)
    {
        this.WaitTimeIncrement = value;
        return this;
    }

    /// <summary>
    /// Sets the amount of time to wait before retrying.
    /// </summary>
    /// <param name="waitTime">The amount of time to wait before retrying.</param>
    /// <returns>The <see cref="Retry"/> instance.</returns>
    public Retry SetWaitTime(TimeSpan waitTime)
    {
        this.WaitTime = waitTime;
        return this;
    }

    /// <summary>
    /// Sets the maximum wait time if the wait time is set to increase upon retry.
    /// </summary>
    /// <param name="maxWaitTime">Sets the maximum wait time.</param>
    /// <returns>The <see cref="RetryHandler"/> instance.</returns>
    public Retry SetMaxWaitTime(TimeSpan maxWaitTime)
    {
        this.MaxWaitTime = maxWaitTime;
        return this;
    }

    /// <summary>
    /// Sets a value indicating whether or not to double the wait time on retry.
    /// </summary>
    /// <param name="value">A value indicating whether or not to double the wait time on retry.</param>
    /// <returns>The <see cref="Retry"/> instance.</returns>
    public Retry SetDoubleWaitTimeOnRetry(bool value)
    {
        this.DoubleWaitTimeOnRetry = value;
        return this;
    }

    /// <summary>
    /// Adds an exception to retry on.
    /// </summary>
    /// <typeparam name="TException">The exception type.</typeparam>
    /// <returns>The <see cref="Retry"/> instance.</returns>
    public Retry AddException<TException>()
        where TException : Exception
    {
        this.exceptions.Add(typeof(TException));
        return this;
    }

    /// <summary>
    /// Adds an exception retry handler.
    /// </summary>
    /// <typeparam name="TException">The exception type.</typeparam>
    /// <param name="handler">The handler function.</param>
    /// <returns>The <see cref="Retry"/> instance.</returns>
    public Retry AddExceptionHandler<TException>(Func<TException, Task<bool>> handler)
        where TException : Exception
    {
        this.exceptionHandlers.Add(new RetryHandler<TException>(handler));
        return this;
    }

    /// <summary>
    /// Adds an result retry handler.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="handler">The handler function.</param>
    /// <returns>The <see cref="Retry"/> instance.</returns>
    public Retry AddResultHandler<TResult>(Func<TResult, Task<bool>> handler)
    {
        this.resultHandlers.Add(new RetryHandler<TResult>(handler));
        return this;
    }

    /// <summary>
    /// Sets an operation completed handler.
    /// </summary>
    /// <param name="handler">The handler to execute on completion.</param>
    /// <returns>The <see cref="Retry"/> instance.</returns>
    public Retry SetOperationCompletedHandler(Action<TimeSpan> handler)
    {
        this.operationCompletedHandler = handler;
        return this;
    }

    /// <summary>
    /// Executes an operation with retry.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="function">The function to execute.</param>
    /// <returns>The result of the function.</returns>
    public virtual async Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> function)
    {
        Exception lastException = null;
        TResult lastResult = default;

        var stopwatch = Stopwatch.StartNew();

        try
        {
            int retry = 0;
            var wait = this.SetInitialWaitTime();

            while (retry < this.Limit)
            {
                try
                {
                    lastResult = await function();
                    if (this.resultHandlers.Any() == false)
                    {
                        return lastResult;
                    }

                    if (await this.ShouldRetryAsync<TResult>(this.resultHandlers, lastResult) == false)
                    {
                        return lastResult;
                    }
                }
                catch (Exception ex)
                {
                    if (await this.ShouldRetryAsync(ex) == false)
                    {
                        throw;
                    }

                    lastException = ex;
                }

                await Task.Delay(wait);

                retry++;

                wait = this.SetNextWaitTime(retry, wait);
            }
        }
        finally
        {
            this.CompleteOperation(stopwatch.Elapsed);
        }

        if (lastException != null)
        {
            throw lastException;
        }

        return lastResult;
    }

    /// <summary>
    /// Sets the initial wait time.
    /// </summary>
    /// <returns>The initial wait time.</returns>
    protected virtual TimeSpan SetInitialWaitTime()
    {
        var wait = this.WaitTime;

        if (this.RandomizeInitialWaitTime == true)
        {
            wait = TimeSpan.FromMilliseconds(Randomizer.Next(0, (int)wait.TotalMilliseconds));
        }

        if (this.VaryWaitTimeOnRetry == true)
        {
            wait = wait.VaryTime(this.WaitTimeVariance, this.WaitTime);
        }

        return wait;
    }

    /// <summary>
    /// Sets the wait time for the next iteration.
    /// </summary>
    /// <param name="retry">The retry iteration.</param>
    /// <param name="currentWaitTime">The current wait time.</param>
    /// <returns>The next wait time.</returns>
    protected virtual TimeSpan SetNextWaitTime(int retry, TimeSpan currentWaitTime)
    {
        TimeSpan wait;
        if (this.DoubleWaitTimeOnRetry == true)
        {
            wait = TimeSpan.FromTicks(currentWaitTime.Ticks * 2);
        }
        else
        {
            wait = currentWaitTime.Add(this.WaitTimeIncrement);
        }

        if (this.VaryWaitTimeOnRetry == true)
        {
            wait = wait.VaryTime(this.WaitTimeVariance, this.WaitTime);
        }

        return wait;
    }

    /// <summary>
    /// Completes the operation.
    /// </summary>
    /// <param name="elapsed">The operations elapsed time.</param>
    private void CompleteOperation(TimeSpan elapsed)
    {
        this.OperationCompleted(elapsed);

        this.operationCompletedHandler?.Invoke(elapsed);
    }

    /// <summary>
    /// Signals that the operation has been completed.
    /// </summary>
    /// <param name="elapsed">The operations elapsed time.</param>
    protected virtual void OperationCompleted(TimeSpan elapsed)
    {
    }

    /// <summary>
    /// Tests whether an operation should retry.
    /// </summary>
    /// <param name="ex">The exception that occurred.</param>
    /// <returns>True if it should retry; otherwise false.</returns>
    protected virtual async Task<bool> ShouldRetryAsync(Exception ex)
    {
        if (this.exceptionHandlers.Any() == true)
        {
            if (await this.ShouldRetryAsync<Exception>(this.exceptionHandlers, ex) == false)
            {
                return false;
            }
        }
        else
        {
            if (this.exceptions.Any() == true &&
                this.exceptions.Contains(ex.GetType()) == false)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Tests whether an operation should be retried.
    /// </summary>
    /// <typeparam name="T">The handler result type.</typeparam>
    /// <param name="handlers">The handler list.</param>
    /// <param name="value">The handler value.</param>
    /// <returns>True if it should be retried; otherwise false.</returns>
    protected virtual async Task<bool> ShouldRetryAsync<T>(IEnumerable<RetryHandler> handlers, T value)
    {
        foreach (var handler in handlers.Where(h => h.IsMatch(value.GetType())))
        {
            if (await handler.ShouldRetryAsync(value) == true)
            {
                return true;
            }
        }

        return false;
    }
}