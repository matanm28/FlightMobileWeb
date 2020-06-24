namespace FlightSimulatorApp.Model {
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;
    using FlightMobileWeb.FlightGear;
    using FlightMobileWeb.FlightGear.Data_Stractures;
    using FlightMobileWeb.Models;

    public class FlightGearClient : IFlightGearClient {
        private const string FlightGearSendOnlyDataCommand = "data\r\n";
        private static readonly double Tolerance = Math.Pow(10, -4);
        private readonly ITelnetClient telnetClient;
        private readonly BlockingCollection<AsyncCommand> queue;
        private string ip;
        private int port;
        private Errors error = Errors.NoError;
        private Task processCommandTask;

        /// <inheritdoc />
        public string IP {
            get { return this.ip; }
            set {
                if (IsValidIP(value)) {
                    this.ip = value;
                } else {
                    this.Error = Errors.InvalidIP;
                }
            }
        }

        /// <inheritdoc />
        public int Port {
            get { return this.port; }
            set {
                if (IsValidPort(value)) {
                    this.port = value;
                } else {
                    this.Error = Errors.InvalidPort;
                }
            }
        }

        public FlightGearClient(ITelnetClient telnetClient) {
            this.telnetClient = telnetClient;
            this.queue = new BlockingCollection<AsyncCommand>();
        }

        /// <inheritdoc />
        public bool IsConnected {
            get { return this.telnetClient.IsConnected; }
        }

        /// <inheritdoc />
        private Errors Error {
            get {
                Errors temp = this.error;
                this.error = Errors.NoError;
                return temp;
            }

            set { this.error = value; }
        }

        /// <inheritdoc />
        private bool HasError => this.error != Errors.NoError;

        /// <inheritdoc />
        private void SetValue(FlightGearDataVariable var, double value) {
            //if (!this.EnsureConnected()) {
            //    return;
            //}

            string request = $"set {var.Path} {value} \r\n";
            this.telnetClient.Send(request);
        }

        private bool EnsureConnected() {
            if (this.IsConnected) {
                return true;
            }
            this.telnetClient.Connect(this.IP, this.Port);
            if (this.IsConnected) {
                if (this.processCommandTask == null || this.processCommandTask.Status == TaskStatus.Canceled
                                                    || this.processCommandTask.Status == TaskStatus.Faulted) {
                    this.processCommandTask.Dispose();
                    this.processCommandTask = Task.Factory.StartNew(this.ProcessCommands);
                }
                this.telnetClient.Send(FlightGearSendOnlyDataCommand);
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        private double? GetValue(FlightGearDataVariable var) {
            //if (!this.EnsureConnected()) {
            //    return null;
            //}

            string request = $"get {var.Path} \r\n";
            this.telnetClient.SendAsync(request);
            string answer = this.telnetClient.Read();
            if (double.TryParse(answer, out double value)) {
                return value;
            }

            this.Error = Errors.ParsingError;
            return null;
        }

        private async Task<double?> GetValueAsync(FlightGearDataVariable var) {

            string request = $"get {var.Path} \r\n";
            await this.telnetClient.SendAsync(request);
            string answer = this.telnetClient.Read();
            if (double.TryParse(answer, out double value)) {
                return value;
            }

            this.Error = Errors.ParsingError;
            return null;
        }

        /// <inheritdoc />
        public void Start() {
            try {
                this.processCommandTask = Task.Factory.StartNew(this.ProcessCommands);
                this.telnetClient.Connect(this.IP, this.Port);
                this.telnetClient.Send(FlightGearSendOnlyDataCommand);
            }
            catch (Exception e) {
                Console.Error.WriteLine(e);
                Console.Error.WriteLine(e.StackTrace);
                Console.Error.WriteLine("\n\n*****************************************************************************");
                Console.Error.WriteLine("Failed to connect to server.\nYou should run Flight Gear server before running this API server.");
                Console.Error.WriteLine("*****************************************************************************");
                throw;
            }
        }

        /// <summary>
        /// The process commands.
        /// </summary>
        private async void ProcessCommands() {
            foreach (AsyncCommand asyncCommand in this.queue.GetConsumingEnumerable()) {
                this.SetValue(FlightGearDataVariable.Aileron, asyncCommand.Command.Aileron);
                this.SetValue(FlightGearDataVariable.Elevator, asyncCommand.Command.Elevator);
                this.SetValue(FlightGearDataVariable.Rudder, asyncCommand.Command.Rudder);
                this.SetValue(FlightGearDataVariable.Throttle, asyncCommand.Command.Throttle);
                if (this.HasError) {
                    Console.WriteLine(this.Error);
                    asyncCommand.Completion.SetResult(Result.Invalid);
                } else if (await this.ValidateCommand(asyncCommand.Command) && !this.HasError) {
                    asyncCommand.Completion.SetResult(Result.OK);
                } else {
                    this.Restart();
                    asyncCommand.Completion.SetResult(Result.Invalid);
                }
            }
        }

        public Task<Result> ExecuteCommand(Command command) {
            var asyncCommand = new AsyncCommand(command);
            this.queue.Add(asyncCommand);
            return asyncCommand.Task;
        }

        private bool ValidateSentValue(FlightGearDataVariable var, double value) {
            double? varValue = this.GetValue(var);
            return varValue.HasValue && Math.Abs(varValue.Value - value) <= Tolerance;
        }
        private async Task<bool> ValidateSentValueAsync(FlightGearDataVariable var, double value) {
            double? varValue = await this.GetValueAsync(var);
            return varValue.HasValue && Math.Abs(varValue.Value - value) <= Tolerance;
        }

        private async Task<bool> ValidateCommand(Command command) {
            bool aileron = await this.ValidateSentValueAsync(FlightGearDataVariable.Aileron, command.Aileron);
            bool elevator = await this.ValidateSentValueAsync(FlightGearDataVariable.Elevator, command.Elevator);
            bool rudder = await this.ValidateSentValueAsync(FlightGearDataVariable.Rudder, command.Rudder);
            bool throttle = await this.ValidateSentValueAsync(FlightGearDataVariable.Throttle, command.Throttle);
            return aileron && elevator && rudder && throttle;
        }

        /// <inheritdoc />
        private void Restart() {
            this.telnetClient.Flush();
            this.Error = Errors.NoError;
        }

        /// <inheritdoc />
        public bool Stop() {
            if (!this.IsConnected) {
                return true;
            }

            this.telnetClient.Disconnect();
            this.Error = Errors.NoError;
            return true;
        }

        /// <summary>
        /// Validates the ip.
        /// </summary>
        /// <param name="ip">The ip.</param>
        /// <returns></returns>
        private static bool IsValidIP(string ip) {
            if (ip.ToUpper() == "localhost".ToUpper()) {
                return true;
            }

            string[] stringArr = ip.Split(".".ToCharArray());
            if (stringArr.Length != 4) {
                return false;
            }

            foreach (string octet in stringArr) {
                try {
                    int num = int.Parse(octet);
                    if (!(num >= 0 && num <= 255)) {
                        return false;
                    }
                }
                catch (Exception e) {
                    Console.WriteLine(e);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Validates the port.
        /// </summary>
        /// <param name="port">The port.</param>
        /// <returns></returns>
        private static bool IsValidPort(int port) {
            return port >= 1024 && port <= Math.Pow(2, 16);
        }
    }
}
