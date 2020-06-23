namespace FlightSimulatorApp.Model {
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using FlightMobileWeb.FlightGear;
    using FlightMobileWeb.Models;

    public interface IFlightGearClient {
        bool IsConnected { get; }
        string IP { get; set; }
        int Port { get; set; }

        void Start();

        bool Stop();

        Task<Result> ExecuteCommand(Command command);
    }
}
