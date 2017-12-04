using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Runtime.Serialization.Formatters.Binary;
using System.Net;
using System.IO;

namespace Test
{
    public class Blockchain
    {
        private List<Block> chain = new List<Block>(); // Сам блокчейн
        private List<Transaction> _transactions = new List<Transaction>(); // Список транзакций в одном блоке
        private List<Node> nodes = new List<Node>(); //  Список узлов в сети

        private string NodeID; // номер узла

        public Blockchain()
        {
            this.NewBlock(10); // Конструктор для создания нулевого блока.
            NodeID = Guid.NewGuid().ToString().Replace("-", "");
        }

        public Block NewBlock(Int64 proof, string previous_block = null) // Добавление нового блока в цепь
        {
            Block block = new Block();
            block.Id = chain.Count + 1;
            block.timestamp = DateTime.Now;
            block.transactions = _transactions.ToList();
            block.proof = proof;
            block.previous_block = previous_block == null ? previous_block : Hash(chain.Last());

            chain.Add(block);
            _transactions.Clear();
            return block;
        }

        public int NewTransaction(string sender, string recipient, int amount)
        {

            Transaction currentTransaction = new Transaction(sender, recipient, amount);
            _transactions.Add(currentTransaction);
            return chain.Count + 1;
        }

        public void ListChain()
        {
            for(var i = 0; i <= chain.Count-1; i++)
            {
                Console.WriteLine("ID Block " + chain[i].Id + "\nTimestamp " + chain[i].timestamp + "\nProof " + chain[i].proof);
            }
        }

        private string Hash(Block block)
        {
            string blockTxt = JsonConvert.SerializeObject(block);
            return SHA256(blockTxt);
        }

        private string SHA256(string data)
        {
            var sha256 = new SHA256Managed();
            var hashBuilder = new StringBuilder();

            byte[] bytes = Encoding.Unicode.GetBytes(data);
            byte[] hash = sha256.ComputeHash(bytes);
            foreach (byte x in hash)
                hashBuilder.Append(x);

            return hashBuilder.ToString();
        }

        private bool ValidProof(Int64 last_proof,Int64 proof)
        {
            string proof_hash = last_proof.ToString() + proof.ToString();
            var result = SHA256(proof_hash); 
            return result.StartsWith("00");
        }

        public Int64 ProofOfWork(Int64 last_proof)
        {
            Int64 proof = 10;
            while (ValidProof(last_proof, proof) == false)
            {
                proof++;
            }
            return proof;
        }

        public string Mine()
        {
            Int64 proof = ProofOfWork(chain.Last().proof);
            NewTransaction("God",NodeID,1);
            Block block = NewBlock(proof,Hash(chain.Last()));

            var response = new
            {
                Message = "Новый блок добавлен",
                Id = block.Id,
                Transactions = block.transactions,
                Proof = block.proof,
                Previous_block = block.previous_block
            };

            return JsonConvert.SerializeObject(response); // Вернуть ответ.
        }

        public string GetChain()
        {
            var response = new {
                chain = chain.ToArray(),
                length = chain.Count()
            };

            return JsonConvert.SerializeObject(response); 
        }

        public string RegNode(string node)
        {
            string url = node;
            nodes.Add(new Node { Address = new Uri(url)});
            var response = new
            {
                answer = nodes.ToArray()
            };

            return JsonConvert.SerializeObject(response);
        }

        public string Consensus()
        {
            var hi = "hello";
            bool what = ResolveConflict();
            var response = new
            {
                Message = what ? "цепочка была заменена" : "цепочка не изменена",
                Chain = chain.ToArray()
            };

            return JsonConvert.SerializeObject(response);
        }

        private bool ResolveConflict()
        {
            List<Block> newChain = null;
            int length = chain.Count();

            foreach(Node node in nodes){
                var url = new Uri(node.Address,"/chain");
                var request = (HttpWebRequest)WebRequest.Create(url);
                var response = (HttpWebResponse)request.GetResponse();

                if(response.StatusCode == HttpStatusCode.OK)
                {
                    var model = new
                    {
                        chain = new List<Block>(),
                        length = 0
                    };
                    string json = new StreamReader(response.GetResponseStream()).ReadToEnd();
                    var data = JsonConvert.DeserializeAnonymousType(json, model);

                    if(data.chain.Count > chain.Count && ValidChain(data.chain))
                    {
                        length = data.chain.Count();
                        newChain = data.chain;
                    }
                }
            }

            if(newChain != null)
            {
                chain = newChain;
                return true;
            }

            return false;

        }

        private bool ValidChain(List<Block> chain)
        {
            int index = 1;
            Block block = null;
            Block firstblock = chain.First();
            while(index < chain.Count())
            {
                block = chain.ElementAt(index);
                if (block.previous_block != Hash(firstblock))
                    return false;

                if (!ValidProof(firstblock.proof,block.proof))
                    return false;

                firstblock = block;
                index++;
            }
            return true;
        }
    }
}
