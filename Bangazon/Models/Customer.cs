using System.Collections.Generic;

namespace Bangazon.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<Product> ProductsList = new List<Product>();
        public List<PaymentType> PaymentTypeList = new List<PaymentType>();
    }
}
