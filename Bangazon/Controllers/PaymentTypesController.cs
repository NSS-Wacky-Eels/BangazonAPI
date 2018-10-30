using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using Microsoft.AspNetCore.Http;
using Bangazon.Models;

namespace Bangazon.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentTypesController : ControllerBase
    {
        private readonly IConfiguration _config;

        public PaymentTypesController(IConfiguration config)
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


        // GET api/paymenttypes
        [HttpGet]
        public async Task<IActionResult> Get(string q)
        {
            using (IDbConnection conn = Connection)
            {
                string sql = @"
            SELECT
                pt.Id,
                pt.Name,
                pt.AcctNumber,
                pt.CustomerId
            FROM PaymentType pt
            WHERE 1=1
            ";

                if (q != null)
                {
                    string isQ = $@"
                    AND i.Name LIKE '%{q}%'
                    OR i.AcctNumber LIKE '%{q}%'
                    OR i.CustomerId LIKE '%{q}%'
                ";
                    sql = $"{sql} {isQ}";
                }
                var paymentTypes = await conn.QueryAsync<PaymentType>(sql);
                return Ok(paymentTypes);
            }
        }

        // GET api/paymenttypes/5
        [HttpGet("{id}", Name = "GetPaymentType")]
        public async Task<IActionResult> Get([FromRoute]int id)
        {
            string sql = $@"
            SELECT
                pt.Id,
                pt.Name,
                pt.AcctNumber,
                pt.CustomerId
            FROM PaymentType pt
            WHERE pt.Id = {id}
            ";

            using (IDbConnection conn = Connection)
            {
                IEnumerable<PaymentType> paymentTypes = await conn.QueryAsync<PaymentType>(sql);
                return Ok(paymentTypes);
            }
        }

        // POST api/paymenttypes
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] PaymentType paymentType)
        {
            string sql = $@"INSERT INTO PaymentType 
                (Name, AcctNumber, CustomerId)
                VALUES
                (
                    '{paymentType.Name}'
                    ,'{paymentType.AcctNumber}'
                    ,{paymentType.CustomerId}
                );
                SELECT SCOPE_IDENTITY();";

            using (IDbConnection conn = Connection)
            {
                var newId = (await conn.QueryAsync<int>(sql)).Single();
                paymentType.Id = newId;
                return CreatedAtRoute("GetPaymentType", new { id = newId }, paymentType);
            }
        }

        // PUT api/paymenttypes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] PaymentType paymentType)
        {
            string sql = $@"
                UPDATE PaymentType
                SET Name = '{paymentType.Name}',
                    AcctNumber = '{paymentType.AcctNumber}',
                    CustomerId = '{paymentType.CustomerId}'
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
                if (!PaymentTypeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // DELETE api/paymenttypes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            string sql = $@"DELETE FROM PaymentType WHERE Id = {id}";

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

        private bool PaymentTypeExists(int id)
        {
            string sql = $"SELECT Id FROM PaymentType WHERE Id = {id}";
            using (IDbConnection conn = Connection)
            {
                return conn.Query<PaymentType>(sql).Count() > 0;
            }
        }
    }

}
