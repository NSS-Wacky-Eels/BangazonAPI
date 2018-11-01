using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Bangazon.Models
{
    public class TrainingProgram
    {
        public int Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string StartDate { get; set; }

        public string EndDate { get; set; }

        public int MaxAttendees { get; set; }

        public bool Started { get; set; }
    }
}