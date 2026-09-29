using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project_AutoHouse.Models
{
    public class CarAd
    {
        [Key]
        public int Id { get; set; }
        public string City { get; set; } = "София"; 
        public string Make { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }

      
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public string ImageUrl { get; set; }
        public string OwnerEmail { get; set; }
        public int Views { get; set; } = 0; 

        public string Description { get; set; }
        public string Extras { get; set; } // Тук ще пазим списъка с екстри
        public string PhoneNumber { get; set; }


        public string FuelType { get; set; }     // За Дизел/Бензин...
        public string Transmission { get; set; } // За Ръчка/Автоматик
        public int Horsepower { get; set; }      // За конските сили (цяло число)
    }
}

