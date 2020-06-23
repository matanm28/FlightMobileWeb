namespace FlightMobileWeb.FlightGear.Data_Stractures {
    using System.Threading.Tasks;
    using FlightMobileWeb.Models;
    using FlightSimulatorApp.Model;

    public class AsyncCommand {
        public Command Command { get; private set; }
        public Task<Result> Task {
            get => Completion.Task;
        }
        public TaskCompletionSource<Result> Completion { get; private set; }

        public AsyncCommand(Command command) {
            this.Command = command;
            this.Completion = new TaskCompletionSource<Result>(TaskCreationOptions.RunContinuationsAsynchronously);
        }
    }
}
