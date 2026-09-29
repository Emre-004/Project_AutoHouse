namespace Project_AutoHouse.Models
{
    public class Favorite
    {
        public int Id { get; set; }
        public string UserEmail { get; set; } // Кой е добавил колата
        public int CarAdId { get; set; }      // Коя кола е добавена
        public CarAd CarAd { get; set; }      // Връзка към самата обява
    }
}