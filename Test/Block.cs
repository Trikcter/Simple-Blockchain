using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    public class Block
    {
        public Int64 Id;
        public DateTime timestamp;
        public List <Transaction> transactions;
        public Int64 proof;
        public string previous_block;
    }
}
