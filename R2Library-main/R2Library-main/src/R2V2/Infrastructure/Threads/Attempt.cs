#region

using System;
using System.Linq.Expressions;
using System.Threading;
using Common.Logging;

#endregion

namespace R2V2.Infrastructure.Threads
{
    public class Attempt
    {
        protected static readonly ILog Log = LogManager.GetLogger("R2V2.Infrastructure.Threads.Attempt");

        public static void Execute(Expression<Action> actionExpression, int maxTries, int retryMillisecondsSeed)
        {
            string actionName = null;

            var attempt = 1;
            while (ReadyForAttempt(attempt, maxTries, retryMillisecondsSeed))
            {
                try
                {
                    Execute(actionExpression);
                    return;
                }
                catch (Exception ex)
                {
                    actionName = actionName ?? GetActionName(actionExpression);

                    if (attempt < maxTries)
                    {
                        Log.WarnFormat(Environment.NewLine + "Action: {0} - Failed - Exception message: {1}",
                            actionName, ex.Message);
                    }
                    else
                    {
                        Log.WarnFormat(
                            Environment.NewLine +
                            "Action: {0} - Failed - Max tries reached! Max tries: {1}, Exception message: {2}",
                            actionName, maxTries, ex.Message);
                        throw;
                    }
                }

                attempt++;
            }
        }

        private static bool ReadyForAttempt(int attempt, int maxTries, int retryMillisecondsSeed)
        {
            if (attempt == 1) return true;

            var withinMaxAttempts = attempt <= maxTries;

            if (withinMaxAttempts)
            {
                var millisecondsTimeout = retryMillisecondsSeed * (int)Math.Pow(2, attempt - 2);
                Log.WarnFormat(Environment.NewLine + "Waiting {0} milliseconds before retrying...",
                    millisecondsTimeout);
                Thread.Sleep(millisecondsTimeout);
            }

            return withinMaxAttempts;
        }

        private static string GetActionName(Expression<Action> actionExpression)
        {
            return ((MethodCallExpression)actionExpression.Body).Method.Name;
        }

        private static void Execute(Expression<Action> actionExpression)
        {
            actionExpression.Compile()();
        }
    }
}