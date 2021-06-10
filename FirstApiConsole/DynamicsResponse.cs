using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirstApiConsole
{
    public class DynamicsResponse<T>
    {
        public List<T> Value { get; set; }
    }
}
