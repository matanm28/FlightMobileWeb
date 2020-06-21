namespace FlightSimulatorApp.Model {
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using FlightMobileWeb.FlightGear;

    public interface IFlightGearClient {
        bool IsConnected { get; }
        string Error { get; }
        bool HasError { get; }
        string IP { get; set; }
        int Port { get; set; }

        Task SetValueAsync([NotNull] FlightGearDataVariable var, double value);

        Task<double?> GetValueAsync([NotNull] FlightGearDataVariable var);

        Task<bool> StartAsync();

        bool Stop();

        Task Restart();
    }
}
