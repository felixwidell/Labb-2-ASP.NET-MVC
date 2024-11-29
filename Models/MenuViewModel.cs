using System.ComponentModel;

namespace RestaurangWebAPI.Models
{
    public class MenuViewModel
    {
        public int Id { get; set; }
        public string FoodName { get; set; }
        public int Price { get; set; }
        public bool IsAvaiable { get; set; }
    }
}
