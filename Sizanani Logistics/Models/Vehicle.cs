namespace Sizanani_Logistics.Models
{
    public class Vehicle
    {
        public int VehicleId { get; set; }
        public string Registration { get; set; }
        public string Type { get; set; }
        public string Model { get; set; }
        public int? Weight { get; set; }
        public int ContractorId { get; set; }
    }
}
