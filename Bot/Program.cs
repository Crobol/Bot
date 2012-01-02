using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Reflection;
using System.Text;
using Bot.Core.Plugins;
using Nini.Config;

namespace Bot
{
    class Program
    {
        static void Main(string[] args)
        {
            northwindEFEntities db = new northwindEFEntities();
            
            var customers = db.Customers.Take(1);

            foreach (var customer in customers)
            {
                Console.WriteLine(customer.ContactName);
            }

            Bot.Run("config");
        }
    }
}
