using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirstApiConsole.Entities
{
    public class Account
    {
        public string AccountId { get; set; }
        public int StatusCode { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}