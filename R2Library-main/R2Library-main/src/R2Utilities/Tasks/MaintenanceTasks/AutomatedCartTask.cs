#region

using System;
using System.Text;
using R2Library.Data.ADO.R2Utility;
using R2V2.Core.AutomatedCart;

#endregion

namespace R2Utilities.Tasks.MaintenanceTasks
{
    public class AutomatedCartTask : TaskBase, ITask
    {
        private readonly AutomatedCartFactory _automatedCartFactory;
        private readonly AutomatedCartQueueService _automatedCartQueueService;
        private int _automatedCartId;

        public AutomatedCartTask(
            AutomatedCartFactory automatedCartFactory
            , AutomatedCartQueueService automatedCartQueueService
        )
            : base("AutomatedCartTask", "-AutomatedCartTask", "81", TaskGroup.ContentLoading,
                "Re-Sends Automated Cart RabbitMqMessage (Requires -automatedCartId=XXXX)", true)
        {
            _automatedCartFactory = automatedCartFactory;
            _automatedCartQueueService = automatedCartQueueService;
        }

        public new void Init(string[] commandLineArguments)
        {
            base.Init(commandLineArguments);

            _automatedCartId = GetArgumentInt32("automatedCartId", 0);


            Log.Info($"-job: AutomatedCartTask, -automatedCartId: {_automatedCartId}");
        }

        public override void Run()
        {
            TaskResult.Information = "This task will re-send the AutomatedCartMessage to the RabbitMq Queue.";
            var step = new TaskResultStep { Name = "AutomatedCartTask", StartTime = DateTime.Now };
            TaskResult.AddStep(step);
            UpdateTaskResult();

            try
            {
                var success = false;
                if (_automatedCartId > 0)
                {
                    var automatedCart = _automatedCartFactory.GetAutomatedCart(_automatedCartId);
                    var acm = new AutomatedCartMessage
                    {
                        AutomatedCartId = automatedCart.Id,
                        Period = automatedCart.Period,
                        StartDate = automatedCart.StartDate,
                        EndDate = automatedCart.EndDate,
                        NewEdition = automatedCart.NewEdition,
                        TriggeredPda = automatedCart.TriggeredPda,
                        Reviewed = automatedCart.Reviewed,
                        Turnaway = automatedCart.Turnaway,
                        Discount = automatedCart.Discount,
                        AccountNumbers = automatedCart.AccountNumbers,
                        CartName = automatedCart.CartName,
                        EmailText = automatedCart.EmailText
                    };

                    success = _automatedCartQueueService.WriteDataToMessageQueue(acm);
                }


                var stepResults = new StringBuilder();
                stepResults.AppendFormat("AutomatedCartId: {0}  has {1}.", _automatedCartId,
                    _automatedCartId > 0 ? "been resent" : "failed").AppendLine();

                step.Results = stepResults.ToString();
                step.CompletedSuccessfully = success;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                step.CompletedSuccessfully = false;
                step.Results = ex.Message;
                throw;
            }
            finally
            {
                step.EndTime = DateTime.Now;
                UpdateTaskResult();
            }
        }
    }
}