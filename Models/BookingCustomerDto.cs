namespace RestaurangWebAPI.Models
{
    public class BookingCustomerDto
    {
        public string CustomerName { get; set; }
        public int PhoneNumber { get; set; }
        public int TableId { get; set; }
        public DateTime BookingDate { get; set; }
        public int PeopleAmount { get; set; }
    }
}
