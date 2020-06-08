﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NFluent;
using Xunit;

namespace System.Linq.Dynamic.Core.Tests.MikArea
{
    public class Dictionary
    {
        public class Customer
        {
            public string City { get; set; }
            public Dictionary<string, Order> Orders { get; set; }
            public string CompanyName { get; set; }
            public string Phone { get; set; }
        }
        public class Order
        {
        }

        [Fact]
        public void Test_ContainsKey_1()
        {
            List<Customer> customers = new List<Customer>()
            {
                new Customer() { City = "ZZZ1", CompanyName = "ZZZ", Orders = new Dictionary<string, Order>() },
                new Customer() { City = "ZZZ2", CompanyName = "ZZZ", Orders = new Dictionary<string, Order>()  },
                new Customer() { City = "ZZZ3", CompanyName = "ZZZ", Orders = new Dictionary<string, Order>()  }
            };
            customers.ForEach(x => x.Orders.Add(x.City + "TEST", new Order()));



            var data = customers.AsQueryable()
                .Where("Orders.ContainsKey(\"ZZZ2TEST\")")
                .OrderBy("CompanyName")
                .Select("new(City as City, Phone)").ToDynamicList();

            Check.That("ZZZ2").IsEqualTo((string)data.First().City);
        }

        [Fact]
        public void Test_ContainsKey_2()
        {
            List<Customer> customers = new List<Customer>()
            {
                new Customer() { City = "ZZZ1", CompanyName = "ZZZ", Orders = new Dictionary<string, Order>() },
                new Customer() { City = "ZZZ2", CompanyName = "ZZZ", Orders = new Dictionary<string, Order>()  },
                new Customer() { City = "ZZZ3", CompanyName = "ZZZ", Orders = new Dictionary<string, Order>()  }
            };
            customers.ForEach(x => x.Orders.Add(x.City + "TEST", new Order()));



            var data = customers.AsQueryable()
                .Where("Orders.ContainsKey(it.City + \"TEST\")")
                .OrderBy("City")
                .Select("new(City as City, Phone)").ToDynamicList();

            Check.That("ZZZ1").IsEqualTo((string)data.First().City);
            Check.That(3).IsEqualTo(data.Count);
        }
    }
}
