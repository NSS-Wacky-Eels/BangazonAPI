﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using Bangazon.Models;
using Microsoft.AspNetCore.Http;

namespace Bangazon.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly IConfiguration _config;

        public CustomersController(IConfiguration config)
        {
            _config = config;
        }

        public IDbConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }


        // GET customer by ID
        [HttpGet("{id}", Name = "GetCustomer")]
        public async Task<IActionResult> Get([FromRoute]int id)
        {
            string sql = $@"
            SELECT
                c.Id,
                c.FirstName,
                c.LastName
            FROM Customer c
            WHERE c.Id = {id}
            ";

            using (IDbConnection conn = Connection)
            {
                IEnumerable<Customer> customers = await conn.QueryAsync<Customer>(sql);
                return Ok(customers);
            }
        }

        // GET customer and return PaymentTypes and Products
        // api/customers?_includes=product || api/customer?q=Madi
        [HttpGet]
        public async Task<IActionResult> Get(string q, string _includes)
        {
            string sql = $@"
            SELECT
                c.Id,
                c.FirstName,
                c.LastName
            FROM Customer c
            WHERE 1=1
            ";

            // IF the '_includes' is not empty AND it's equal to 'product'
            if (_includes != null && _includes == "product")
            {
                // run this SQL statement that joins customers and products by customerId
                string includeSQL = @"
                SELECT
                    c.Id,
                    c.FirstName,
                    c.LastName,
                    p.Id,
                    p.Title,
                    p.Price,
                    p.Description,
                    p.Quantity,
                    p.CustomerId,
                    p.ProductTypeId
                FROM Customer c
                JOIN Product p ON p.CustomerId = c.Id
                WHERE 1=1
                ";

                using (IDbConnection conn = Connection)
                {
                    Console.WriteLine(includeSQL);

                    // Created a Dictionary to store the current instances of ALL Customers
                    Dictionary<int, Customer> ProductReport = new Dictionary<int, Customer>();

                    IEnumerable<Customer> customers = await conn.QueryAsync<Customer, Product, Customer>(
                            includeSQL,
                            (customer, product) =>
                            {
                                // for each customer and product, IF the ProductReport DOES NOT contain the key of the customer.Id
                                if (!ProductReport.ContainsKey(customer.Id))
                                {
                                    // ProductReport[1] = customer object
                                    // Then make an Id with the customer.Id, and then set it's value to the 'customer'
                                    ProductReport[customer.Id] = customer;
                                }
                                // ELSE, go to the index of the customer's Id in ProductReport, and add the current product to the ProductList
                                ProductReport[customer.Id].ProductsList.Add(product);
                                return customer;
                            }
                        );
                    return Ok(ProductReport);
                }
            
            // IF the '_includes' is NOT empty AND it is equal to 'paymenttype'
            } else if (_includes != null && _includes == "paymenttype")
            {
                // run this SQL statement that joins the customer and paymenttype by the customer's Id
                string includeSQL = @"
                SELECT
                    c.Id,
                    c.FirstName,
                    c.LastName,
                    pt.Id,
                    pt.Name,
                    pt.AcctNumber,
                    pt.CustomerId
                FROM Customer c
                JOIN PaymentType pt ON pt.CustomerId = c.Id
                WHERE 1=1
                ";

                using (IDbConnection conn = Connection)
                {
                    Console.WriteLine(includeSQL);

                    // created a dictionary to hold the current instance of the customer 
                    Dictionary<int, Customer> PaymentTypeReport = new Dictionary<int, Customer>();

                    IEnumerable<Customer> customers = await conn.QueryAsync<Customer, PaymentType, Customer>(
                            includeSQL,
                            (customer, paymentType) =>
                            {
                                // for each customer and paymenttype, IF the PaymentTypeReport DOES NOT contain the key of the customer's Id
                                if (!PaymentTypeReport.ContainsKey(customer.Id))
                                {
                                    // ProductReport[1] = customer object
                                    // Make an Id with the customer.Id, and then set it's value to the 'customer'
                                    PaymentTypeReport[customer.Id] = customer;
                                }
                                // ELSE, go to the index of the customer's Id in PaymentTypeReport, and add the paymentType to the PaymentTypeList
                                PaymentTypeReport[customer.Id].PaymentTypeList.Add(paymentType);
                                return customer;
                            }
                        );
                    return Ok(PaymentTypeReport);
                }
            
            // IF 'q' is NOT empty
            } else if (q != null)
            {
                // run this SQL statement that checks if 'q' matches the customer's FirstName or LastName in any way
                string isQ = $@"
                    AND c.FirstName LIKE '%{q}%'
                    OR c.LastName LIKE '%{q}%'
                ";
                    sql = $"{sql} {isQ}";

                Console.WriteLine(sql);
                using (IDbConnection conn = Connection)
                {
                    IEnumerable<Customer> customers = await conn.QueryAsync<Customer>(sql);
                    return Ok(customers);
                }
            }

            // THIS CONNECTION IS OUTSIDE OF ALL 'IF' STATEMENTS AND IS FOR THE INITIAL SQL STATEMENT ON LINE 60
            Console.WriteLine(sql);
            using (IDbConnection conn = Connection)
            {
                IEnumerable<Customer> customers = await conn.QueryAsync<Customer>(sql);
                return Ok(customers);
            }

        }

        // POST api/customer
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Customer customer)
        {
            string sql = $@"INSERT INTO Customer 
            (FirstName, LastName)
            VALUES
            (
                '{customer.FirstName}'
                ,'{customer.LastName}'
            );
            SELECT SCOPE_IDENTITY();";

            using (IDbConnection conn = Connection)
            {
                var newId = (await conn.QueryAsync<int>(sql)).Single();
                customer.Id = newId;
                return CreatedAtRoute("GetCustomer", new { id = newId }, customer);
            }
        }

        // PUT api/customers/3
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Customer customer)
        {
            string sql = $@"
            UPDATE Customer
            SET FirstName = '{customer.FirstName}',
                LastName = '{customer.LastName}'
            WHERE Id = {id}";

            try
            {
                using (IDbConnection conn = Connection)
                {
                    int rowsAffected = await conn.ExecuteAsync(sql);
                    if (rowsAffected > 0)
                    {
                        return new StatusCodeResult(StatusCodes.Status204NoContent);
                    }
                    throw new Exception("No rows affected");
                }
            }
            catch (Exception)
            {
                if (!CustomerExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool CustomerExists(int id)
        {
            string sql = $"SELECT Id FROM Customer WHERE Id = {id}";
            using (IDbConnection conn = Connection)
            {
                return conn.Query<Customer>(sql).Count() > 0;
            }
        }
    }

}
