using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    public class Transaction
    {
        public string sender;
        public string recipient;
        public int amount;
        
        public Transaction(string sender,string recipient,int amount)
        {
            this.sender = sender;
            this.recipient = recipient;
            this.amount = amount;
        }
    }
}
