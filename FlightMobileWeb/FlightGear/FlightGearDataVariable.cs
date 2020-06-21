namespace FlightMobileWeb.FlightGear {
    public class FlightGearDataVariable {
        public static readonly FlightGearDataVariable Aileron = new FlightGearDataVariable(nameof(Aileron).ToLower(), "/controls/flight/aileron");
        public static readonly FlightGearDataVariable Rudder = new FlightGearDataVariable(nameof(Rudder).ToLower(), "/controls/flight/rudder");
        public static readonly FlightGearDataVariable Elevator = new FlightGearDataVariable(nameof(Elevator).ToLower(), "/controls/flight/elevator");
        public static readonly FlightGearDataVariable Throttle = new FlightGearDataVariable(nameof(Throttle).ToLower(),
                "/controls/engines/current-engine/throttle");
        public static readonly FlightGearDataVariable AutoStart = new FlightGearDataVariable(nameof(AutoStart).ToLower(), "/engines/active-engine/auto-start");
        public static readonly FlightGearDataVariable Magnetos = new FlightGearDataVariable(nameof(Magnetos).ToLower(), "/controls/switches/magnetos");
        public static readonly FlightGearDataVariable MasterBat = new FlightGearDataVariable(nameof(MasterBat).ToLower(), "/controls/switches/master-bat");
        public static readonly FlightGearDataVariable Mixture = new FlightGearDataVariable(nameof(Mixture).ToLower(),
                "/controls/engines/current-engine/mixture");
        public static readonly FlightGearDataVariable MasterAlt = new FlightGearDataVariable(nameof(MasterAlt).ToLower(), "/controls/switches/master-alt");
        public static readonly FlightGearDataVariable MasterAvionics = new FlightGearDataVariable(nameof(MasterAvionics).ToLower(),
                "/controls/switches/master-avionics");
        public static readonly FlightGearDataVariable BrakeParking = new FlightGearDataVariable(nameof(BrakeParking).ToLower(),
                "/sim/model/c172p/brake-parking");
        public static readonly FlightGearDataVariable Primer = new FlightGearDataVariable(nameof(Primer).ToLower(), "/controls/engines/engine/primer");
        public static readonly FlightGearDataVariable Starter = new FlightGearDataVariable(nameof(Starter).ToLower(), "/controls/switches/starter");
        public static readonly FlightGearDataVariable SpeedBrake = new FlightGearDataVariable(nameof(SpeedBrake).ToLower(), "/controls/flight/speedbrake");
        public static readonly FlightGearDataVariable Rpm = new FlightGearDataVariable(nameof(Rpm).ToLower(), "/engines/engine/rpm");

        public string Name { get; private set; }
        public string Path { get; private set; }

        protected FlightGearDataVariable(string name, string path) {
            this.Name = name;
            this.Path = path;
        }
    }
}
