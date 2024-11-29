using System.ComponentModel;

namespace RestaurangWebAPI.Models
{
    public class CustomerViewModel
    {
        public int Id { get; set; }
        [DisplayName("Customer Name")]
        public string CustomerName { get; set; }
        public int Phone { get; set; }
    }
}
