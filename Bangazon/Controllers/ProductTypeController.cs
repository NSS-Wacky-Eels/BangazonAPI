using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Bangazon.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Dapper;



namespace Bangazon.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductTypeController : ControllerBase
    {
        private readonly IConfiguration _config;

        public ProductTypeController(IConfiguration config)
        {
            _config = config;
        }
        // Create connection to sql
        public IDbConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get(string q)
        {
            //Query sql server to select (a,b,c,) from table
            string sql = @"
            SELECT
                pt.Id,
                pt.Name
            FROM ProductType pt
            ";
            if (q != null)
            {
                string isQ = $@"
                    AND pt.Id LIKE '%{q}%'
                    OR pt.Name LIKE '%{q}%'
                ";
                sql = $"{sql} {isQ}";
            }
            Console.WriteLine(sql);
            //Using database 
            using (IDbConnection conn = Connection)
            {

                IEnumerable<ProductType> productTypes = await conn.QueryAsync<ProductType>(
                    sql
                );
                return Ok(productTypes);
            }
        }

        //GET api/<controller>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get([FromRoute]int id)
        {
            string sql = $@"
            SELECT
                pt.Id,
                pt.Name
            FROM ProductType pt
            WHERE pt.Id = {id}
            ";

            using (IDbConnection conn = Connection)
            {
                IEnumerable<ProductType> productTypes = await conn.QueryAsync<ProductType>(sql);
                return Ok(productTypes);
            }
        }
        // POST api/<controller>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ProductType productType)
        {
            string sql = $@"INSERT INTO ProductType 
            (Id, Name)
            VALUES
            (
                '{productType.Id}'
                ,'{productType.Name}'
            );
            SELECT SCOPE_IDENTITY();";

            using (IDbConnection conn = Connection)
            {
                var newId = (await conn.QueryAsync<int>(sql)).Single();
                productType.Id = newId;
                return CreatedAtRoute("GetProductType", new { id = newId }, productType);
            }
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] ProductType productType)
        {
            string sql = $@"
            UPDATE ProductType
            SET Id = '{productType.Id}',
                LastName = '{productType.Name}'
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
                if (!ProductType(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }
        // DELETE api/<controller>/5
        [HttpDelete("{id}")]

        public async Task<IActionResult> Delete(int id)
        {
            string sql = $@"DELETE FROM ProductType WHERE Id = {id}";

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
        private bool ProductType(int id)
        {
            string sql = $"SELECT Id FROM ProductType WHERE Id = {id}";
            using (IDbConnection conn = Connection)
            {
                return conn.Query<ProductType>(sql).Count() > 0;
            }
        }
    }
}

   

