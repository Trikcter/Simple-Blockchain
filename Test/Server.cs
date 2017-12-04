using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Configuration;

namespace Test
{
    public class Server
    {
        public Server(Blockchain blockchain,string host,string port)
        {
            var server = new TinyWebServer.WebServer(request =>
                {
                    string path = request.Url.PathAndQuery.ToLower();
                    string query;
                    if (path.Contains("?"))
                    {
                        string[] parts = path.Split('?');
                        path = parts[0];
                        query = parts[1];
                    }

                    switch (path)
                    {
                        case "/mine":
                            return blockchain.Mine();
                        case "/chain":
                            return blockchain.GetChain();
                        case "/node/register":
                            string json = new StreamReader(request.InputStream).ReadToEnd();
                            var urlList = new { Urls = "" };
                            var obj = JsonConvert.DeserializeAnonymousType(json, urlList);
                            return blockchain.RegNode(obj.Urls);
                        case "/transaction/new":
                            json = new StreamReader(request.InputStream).ReadToEnd();
                            Transaction trx = JsonConvert.DeserializeObject<Transaction>(json);
                            int blockId = blockchain.NewTransaction(trx.sender, trx.recipient, trx.amount);
                            return "Ваша транзакция была добавлена";
                        case "/node/resolve":
                            return blockchain.Consensus();
                    }

                    return "";
                },
                "http://"+host+":"+port+"/mine/",
                "http://" + host + ":" + port + "/chain/",
                "http://" + host + ":" + port + "/transaction/new/",
                "http://" + host + ":" + port + "/node/register/",
                "http://" + host + ":" + port + "/node/resolve/"
            );

            server.Run();
        }
    }
}
