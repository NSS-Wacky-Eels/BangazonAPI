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
    public class CustomerController : ControllerBase
    {
        private readonly IConfiguration _config;

        public CustomerController(IConfiguration config)
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

        //// GET api/customers?q=Taco
        //[HttpGet]
        //public async Task<IActionResult> Get(string q)
        //{
        //    string sql = @"
        //    SELECT
        //        c.Id,
        //        c.FirstName,
        //        c.LastName,
        //        c.Id,
        //        c.Name
        //    FROM Customer c
        //    JOIN Cohort c ON s.CohortId = c.Id
        //    WHERE 1=1
        //    ";

        //    if (q != null)
        //    {
        //        string isQ = $@"
        //            AND i.FirstName LIKE '%{q}%'
        //            OR i.LastName LIKE '%{q}%'
        //            OR i.SlackHandle LIKE '%{q}%'
        //        ";
        //        sql = $"{sql} {isQ}";
        //    }

        //    Console.WriteLine(sql);

        //    using (IDbConnection conn = Connection)
        //    {

        //        IEnumerable<Student> students = await conn.QueryAsync<Student, Cohort, Student>(
        //            sql,
        //            (student, cohort) =>
        //            {
        //                student.Cohort = cohort;
        //                return student;
        //            }
        //        );
        //        return Ok(students);
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

        // POST api/students
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

        // PUT api/students/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Student student)
        {
            string sql = $@"
            UPDATE Student
            SET FirstName = '{student.FirstName}',
                LastName = '{student.LastName}',
                SlackHandle = '{student.SlackHandle}'
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
                if (!StudentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // DELETE api/students/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            string sql = $@"DELETE FROM Student WHERE Id = {id}";

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

        private bool StudentExists(int id)
        {
            string sql = $"SELECT Id FROM Student WHERE Id = {id}";
            using (IDbConnection conn = Connection)
            {
                return conn.Query<Student>(sql).Count() > 0;
            }
        }
    }

}
