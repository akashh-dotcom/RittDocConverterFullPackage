#region

using R2Library.Data.ADO.R2Utility;

#endregion

namespace R2Utilities.Tasks
{
    public interface ITask
    {
        TaskResult TaskResult { get; }
        string TaskName { get; }
        string TaskDescription { get; }
        string TaskSwitch { get; }
        string TaskSwitchSmall { get; }
        TaskGroup TaskGroup { get; }
        string[] CommandLineArguments { get; }
        bool IsEnabled { get; }

        void Init(string[] commandLineArguments);

        void Run();

        void Cleanup();
    }
}