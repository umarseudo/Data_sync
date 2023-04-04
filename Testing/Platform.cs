using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Testing
{
    internal class Platform
    {
        public int id { get; set; }
        public string uniqueName { get; set; }
        public float latitude { get; set; }
        public float longitude { get; set; }
        public string createdAt { get; set; }
        public string updatedAt { get; set; }
        public List<Well> well { get; set;}
    }
}
