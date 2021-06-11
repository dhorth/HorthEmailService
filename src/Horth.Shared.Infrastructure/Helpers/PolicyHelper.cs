using Polly;
using System;
using Serilog;

namespace Horth.Service.Email.Shared
{
    public class PolicyHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="retryCount"></param>
        /// <param name="delayInSeconds"></param>
        /// <returns>
        /// policyResult.Outcome - whether the call succeeded or failed
        /// policyResult.FinalException - the final exception captured, will be null if the call succeeded
        /// policyResult.ExceptionType - was the final exception an exception the policy was defined to handle (like HttpRequestException above) or an unhandled one(say Exception). Will be null if the call succeeded.
        /// policyResult.Result - if executing a func, the result if the call succeeded or the type's default value
        /// </returns>
        public static PolicyResult Execute(Action action, int retryCount=5, int delayInSeconds=1)
        {
            Log.Logger.Debug($"Executing action ({action.Method.Name}) using retry policy 3,15,60");

                var rnd = new Random();
            var policyResult = Policy
                    .Handle<Exception>()
                        .WaitAndRetry(retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt*delayInSeconds))
                                                                              + TimeSpan.FromMilliseconds(rnd.Next(1, 2000)),
                            (exception, timeSpan, context) => {
                                Log.Logger.Warning(exception, $"Retrying operation {action.Method.Name} trying again in {timeSpan.TotalSeconds} seconds");
                            })
                            .ExecuteAndCapture(() =>
                        {
                            Log.Logger.Debug($"Executing action {action.Method.Name}");
                            action();
                            Log.Logger.Information($"Executing action ({action.Method.Name}) completed");
                        });

            return policyResult;
        }
    }
}
