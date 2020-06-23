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

        public CommandController(IFlightGearClient client) {
            this.client = client;
        }

        [HttpPost]
        public async Task<ActionResult<Command>> PostCommand(Command command) {
            var result = await this.client.ExecuteCommand(command);
            switch (result) {
                case Result.OK:
                    return this.Ok(command);
                case Result.Invalid:
                    return this.Conflict();
                default:
                    return this.BadRequest();
            }
        }

    }
}
