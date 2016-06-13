using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NETCoreService;
using System.Text;

namespace NETCoreWCFClient
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            NETCoreServiceClient client = new NETCoreServiceClient();
            var t = client.GetDataAsync(100);
            Console.WriteLine(t.Result);
            var t1 = client.GetListAsync();
            foreach (var item in t1.Result)
            {
                Console.WriteLine(item);
            }
            CompositeType composite = new CompositeType();
            composite.BoolValue = true;
            composite.StringValue = "客户端调用";
            var t2 = client.GetDataUsingDataContractAsync(composite);
            Console.WriteLine(t2.Result.StringValue);
            Console.ReadKey();
        }
    }
}
