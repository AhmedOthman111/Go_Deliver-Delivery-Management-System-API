using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoDeliver.Core.Entities
{
    public class Employee
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid AppUserId { get; set; }   
        public string Governorate { get; set; } = string.Empty;
        public DateTime HireDate { get; set; } = DateTime.UtcNow;
    }
}
