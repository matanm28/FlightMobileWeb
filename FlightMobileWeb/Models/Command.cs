namespace FlightMobileWeb.Models {
    using System.ComponentModel.DataAnnotations;
    using Newtonsoft.Json;

    public class Command {
        private const string Error = "{0} value must be between {1} to {2}";

        [Required]
        [JsonProperty("aileron")]
        [Range(-1,1, ErrorMessage = Error)]
        public double Aileron { get; set; }
        [Required]
        [JsonProperty("rudder")]
        [Range(-1, 1, ErrorMessage = Error)]
        public double Rudder { get; set; }
        [Required]
        [JsonProperty("elevator")]
        [Range(-1, 1, ErrorMessage = Error)]
        public double Elevator { get; set; }
        [Required]
        [JsonProperty("throttle")]
        [Range(0, 1, ErrorMessage = Error)]
        public double Throttle { get; set; }
    }
}
