using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;
using Bangazon.Models;
using Dapper;
using Microsoft.AspNetCore.Http;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Bangazon.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IConfiguration _config;

        public OrdersController(IConfiguration config)
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

        // GET api/orders?q=Taco
        [HttpGet]
        public async Task<IActionResult> Get(string q)
        {
            string sql = @"
            SELECT
                o.Id,
                o.CustomerId,
                o.PaymentTypeId,
                c.Id,
                c.FirstName,
                c.LastName
            FROM [Order] o
            JOIN Customer c ON o.CustomerId = c.Id
            WHERE 1=1
            ";

            /*
            if (q != null)
            {
                string isQ = $@"
                    AND i.FirstName LIKE '%{q}%'
                    OR i.LastName LIKE '%{q}%'
                    OR i.SlackHandle LIKE '%{q}%'
                ";
                sql = $"{sql} {isQ}";
            }
            */

            using (IDbConnection conn = Connection)
            {

                IEnumerable<Order> orders = await conn.QueryAsync<Order, Customer, Order>(
                    sql,
                    (order, customer) =>
                    {
                        order.Customer = customer;
                       // order.paymentType = paymentType;
                        return order;
                    }
                );
                return Ok(orders);
            }
        }

        // GET api/orders/5
        [HttpGet("{id}", Name = "GetOrder")]
        public async Task<IActionResult> Get([FromRoute]int id)
        {
            string sql = $@"
            SELECT
                o.Id,
                o.CustomerId,
                o.PaymentTypeId,
                c.Id,
                c.FirstName,
                c.LastName
            FROM [Order] o
            JOIN Customer c ON o.CustomerId = c.Id
            WHERE o.Id = {id}
            ";

            using (IDbConnection conn = Connection)
            {
                IEnumerable<Order> orders = await conn.QueryAsync<Order>(sql);
                return Ok(orders);
            }
        }

        // POST api/orders
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Order order)
        {
            string sql = $@"INSERT INTO [Order] 
            (CustomerId, PaymentTypeId)
            VALUES
            (
                '{order.CustomerId}'
                ,'{order.PaymentTypeId}'
            );
            SELECT SCOPE_IDENTITY();";

            using (IDbConnection conn = Connection)
            {
                var newId = (await conn.QueryAsync<int>(sql)).Single();
                order.Id = newId;
                return CreatedAtRoute("GetOrder", new { id = newId }, order);
            }
        }

        // PUT api/orders/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Order order)
        {
            string sql = $@"
            UPDATE [Order]
            SET CustomerId = '{order.CustomerId}',
                PaymentTypeId = '{order.PaymentTypeId}'
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
                if (!OrderExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // DELETE api/orders/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            string sql = $@"DELETE FROM [Order] WHERE Id = {id}";

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

        private bool OrderExists(int id)
        {
            string sql = $"SELECT Id FROM [Order] WHERE Id = {id}";
            using (IDbConnection conn = Connection)
            {
                return conn.Query<Order>(sql).Count() > 0;
            }
        }
    }
}
