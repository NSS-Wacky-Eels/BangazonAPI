﻿using System.Collections.Generic;

namespace Bangazon.Models
{
    public class PaymentType
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string AcctNumber { get; set; }
        public int CustomerId { get; set; }
    }
}