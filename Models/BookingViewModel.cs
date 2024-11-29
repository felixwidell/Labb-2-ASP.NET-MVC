namespace RestaurangWebAPI.Models
{
    public class BookingViewModel
    {
        public int Id { get; set; }
        public virtual CustomerViewModel Customers { get; set; }
        public virtual TableViewModel Tables { get; set; }
        public DateTime BookingDate { get; set; }
        public int PeopleAmount { get; set; }
    }
}
