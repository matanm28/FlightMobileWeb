using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace FlightMobileWeb.Controllers {
    using System.Threading;
    using FlightMobileWeb.FlightGear;
    using FlightMobileWeb.Models;
    using FlightSimulatorApp.Model;

    [Route("api/[controller]")]
    [ApiController]
    public class CommandController : ControllerBase {
        private readonly IFlightGearClient client;
        private static readonly double Tolerance = Math.Pow(10, -4);
        private const string SentValuesValidationError = "Error";
        private const int NumOfValidations = 3;

        public CommandController(IFlightGearClient client) {
            this.client = client;
        }

        [HttpPost]
        public async Task<ActionResult<Command>> PostCommand(Command command) {
            if (!await this.EnginesRunning()) {
                this.startEngine();
            }

            await this.client.SetValueAsync(FlightGearDataVariable.Aileron, command.Aileron);
            await this.client.SetValueAsync(FlightGearDataVariable.Elevator, command.Elevator);
            await this.client.SetValueAsync(FlightGearDataVariable.Rudder, command.Rudder);
            await this.client.SetValueAsync(FlightGearDataVariable.Throttle, command.Throttle);
            if (this.client.HasError) {
                return Conflict(this.client.Error);
            }

            if (await this.ValidateCommand(command)) {
                return this.Ok(command);
            }

            if (this.client.HasError) {
                return this.Conflict(this.client.Error);
            }

            await this.client.Restart();
            return this.Conflict(SentValuesValidationError);
        }

        private async void startEngine() {
            await this.client.SetValueAsync(FlightGearDataVariable.Magnetos, 3);
            await this.client.SetValueAsync(FlightGearDataVariable.Throttle, 0.1);
            await this.client.SetValueAsync(FlightGearDataVariable.Mixture, 0.949);
            await this.client.SetValueAsync(FlightGearDataVariable.MasterBat, 1);
            await this.client.SetValueAsync(FlightGearDataVariable.MasterAlt, 1);
            await this.client.SetValueAsync(FlightGearDataVariable.MasterAvionics, 1);
            await this.client.SetValueAsync(FlightGearDataVariable.BrakeParking, 0);
            await this.client.SetValueAsync(FlightGearDataVariable.Primer, 3);
            await this.client.SetValueAsync(FlightGearDataVariable.Starter, 1);
            await this.client.SetValueAsync(FlightGearDataVariable.AutoStart, 1);
            while (!await this.EnginesRunning()) {
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
        }

        private async Task<bool> EnginesRunning() {
            double? rpmValue = await this.client.GetValueAsync(FlightGearDataVariable.Rpm);
            return rpmValue.HasValue && rpmValue.Value >= 100;
        }

        private async Task<bool> ValidateSentValue(FlightGearDataVariable var, double value) {
            for (int i = 0; i < NumOfValidations; i++) {
                double? varValue = await this.client.GetValueAsync(var);
                if (varValue.HasValue && Math.Abs(varValue.Value - value) <= Tolerance) {
                    return true;
                }

                await this.client.Restart();
            }

            return false;
        }

        private async Task<bool> ValidateCommand(Command command) {
            bool aileron = await this.ValidateSentValue(FlightGearDataVariable.Aileron, command.Aileron);
            bool elevator = await this.ValidateSentValue(FlightGearDataVariable.Elevator, command.Elevator);
            bool rudder = await this.ValidateSentValue(FlightGearDataVariable.Rudder, command.Rudder);
            bool throttle = await this.ValidateSentValue(FlightGearDataVariable.Throttle, command.Throttle);
            return aileron && elevator && rudder && throttle;
        }
    }
}
