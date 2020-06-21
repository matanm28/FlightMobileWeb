namespace FlightSimulatorApp.Model {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Autofac.Core;
    using Autofac.Extensions.DependencyInjection;
    using FlightMobileWeb.FlightGear;
    using Microsoft.CodeAnalysis.Host;
    using Microsoft.Extensions.DependencyInjection;

    public class FlightGearClient : IFlightGearClient {
        private const string NoError = "No error";
        private const string ClientNotConnectedError = "A connection to Flight Gear simulator has to be established before sending/recieving data.";
        private const string AlreadyConnectedError =
                "Client already connected to another server.\nDisconnect before trying to instanciate an other connection.";
        private const string VarValueCastingError = "Failed to parse a returned value";
        private const string ParsingError = "Error while parsing server's answer to double";
        private const string FlightGearSendOnlyDataCommand = "data\r\n";
        private readonly ITelnetClient telnetClient;
        private string ip;
        private int port;
        private string error = NoError;

        /// <inheritdoc />
        public string IP {
            get { return this.ip; }
            set {
                if (isValidIP(value)) {
                    this.ip = value;
                } else {
                    this.Error = "Invalid IP";
                }
            }
        }

        /// <inheritdoc />
        public int Port {
            get { return this.port; }
            set {
                if (isValidPort(value)) {
                    this.port = value;
                } else {
                    this.Error = "Invalid Port";
                }
            }
        }

        public FlightGearClient(ITelnetClient telnetClient) {
            this.telnetClient = telnetClient;
        }

        /// <inheritdoc />
        public bool IsConnected {
            get { return this.telnetClient.IsConnected; }
        }

        /// <inheritdoc />
        public string Error {
            get {
                string temp = this.error;
                this.error = NoError;
                return temp;
            }

            private set {
                if (!string.IsNullOrWhiteSpace(value)) {
                    this.error = value;
                }
            }
        }

        /// <inheritdoc />
        public bool HasError => !this.Error.Equals(NoError);

        /// <inheritdoc />
        public async Task SetValueAsync(FlightGearDataVariable var, double value) {
            if (!await this.EnsureConnection()) {
                return;
            }

            string request = $"set {var.Path} {value} \r\n";
            await this.telnetClient.SendAsync(request);
        }

        /// <inheritdoc />
        public async Task<double?> GetValueAsync(FlightGearDataVariable var) {
            if (!await this.EnsureConnection()) {
                return null;
            }

            string request = $"get {var.Path} \r\n";
            await this.telnetClient.SendAsync(request);
            string answer = await this.telnetClient.ReadAsync();
            if (double.TryParse(answer, out double value)) {
                return value;
            }
            this.Error = ParsingError;
            return null;
        }

        /// <inheritdoc />
        public async Task<bool> StartAsync() {
            if (this.HasError) {
                return false;
            }

            if (this.IsConnected) {
                this.Error = AlreadyConnectedError;
                return false;
            }
            await this.telnetClient.ConnectAsync(this.IP, this.Port);
            if (this.IsConnected) {
                await this.telnetClient.SendAsync(FlightGearSendOnlyDataCommand);
            }

            return this.IsConnected;
        }

        /// <inheritdoc />
        public async Task Restart() {
            await this.telnetClient.FlushAsync();
        }

        /// <inheritdoc />
        public bool Stop() {
            if (!this.IsConnected) {
                return true;
            }
            this.telnetClient.Disconnect();
            this.Error = NoError;
            return true;
        }

        private async Task<bool> EnsureConnection() {
            if (this.IsConnected) {
                return true;
            }

            bool connected = await this.StartAsync();
            if (connected) {
                return true;
            }

            this.Error = ClientNotConnectedError;
            return false;
        }

        /// <summary>
        /// Validates the ip.
        /// </summary>
        /// <param name="ip">The ip.</param>
        /// <returns></returns>
        private static bool isValidIP(string ip) {
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
        private static bool isValidPort(int port) {
            return port >= 1024 && port <= Math.Pow(2, 16);
        }
    }
}
