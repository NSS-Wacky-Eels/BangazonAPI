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

        //// GET api/customers?_include=products
        //[HttpGet(Name = "GetCustomerProduct")]
        //public async Task<IActionResult> Get()
        //{
        //    string sql = @"
        //    SELECT
        //        c.Id,
        //        c.FirstName,
        //        c.LastName
        //    FROM Customer c
        //    ";
        //    Console.WriteLine(sql);

        //    using (IDbConnection conn = Connection)
        //    {

        //        IEnumerable<Customer> customers = await conn.QueryAsync<Customer>(sql);
        //        return Ok(customers);
        //    }
        //}

        // GET api/customers/5
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

        // GET api/students?q=Taco
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

            if (_includes != null && _includes == "product")
            {
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

                    IEnumerable<Customer> customers = await conn.QueryAsync<Customer, Product, Customer>(
                            includeSQL,
                            (customer, product) =>
                            {
                                if (customer.Products.ContainsValue(product) == null)
                                {
                                    customer.Products.Add(product.Title, product);
                                    return customer;
                                }
                                return customer;
                            }
                        );
                    return Ok(customers);
                }

            } else if (q != null)
            {
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
