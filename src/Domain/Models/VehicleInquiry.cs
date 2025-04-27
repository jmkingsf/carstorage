namespace Domain.Models
{
    public class VehicleInquiry(int length, int quantity)
    {
        public int Length { get; set; } = length;
        public int Quantity { get; set; } = quantity;
    }
}
