using System;
using System.ServiceModel;

namespace EquifaxRPA
{
    public class Program
    {
        static void Main(string[] args)
        {
            var _sh = new ServiceHost(typeof(SERVICES.Equifax));
            _sh.Open();
            Console.WriteLine("Api running on port : 7075");
            Console.WriteLine("http://localhost:7075/Test");
            Console.ReadLine();
        }
    }
}
