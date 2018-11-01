using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Http;
using Bangazon.Models;
using Dapper;

namespace Bangazon.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentController : ControllerBase
    {
        private readonly IConfiguration _config;

        public DepartmentController(IConfiguration config)
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

        [HttpGet("{id}", Name = "GetDepartment")]
        public async Task<IActionResult> Get([FromRoute]int id)
        {
            string sql = $@"
            SELECT
                d.Id,
                d.Name,
                d.Budget
            FROM Department d
            WHERE d.Id = {id}
            ";

            using (IDbConnection conn = Connection)
            {
                IEnumerable<Department> departments = await conn.QueryAsync<Department>(sql);
                return Ok(departments);
            }
        }


        // GET api/department?_filter=budget&_taco=3000000. Here we are setting two parameters _filter and _gt for client to use to sort.

        [HttpGet]
        public async Task<IActionResult> Get(string _filter,int _taco)
        {
            string sql = @"
            SELECT
                d.Id,
                d.Name,
                d.Budget
            FROM Department d
            ";
            if (_filter == "budget")
            {
                string isQ = $@"
                    WHERE d.Budget >= {_taco}
                  
                ";
                sql = $"{sql} {isQ}";
            }

            using (IDbConnection conn = Connection)
            {
                IEnumerable<Department> departments = await conn.QueryAsync<Department>(sql);
                return Ok(departments);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Department department)
        {
            string sql = $@"INSERT INTO Department 
            (Name, Budget)
            VALUES
            (
                '{department.Name}'
                ,'{department.Budget}'
            );
            SELECT SCOPE_IDENTITY();";

            using (IDbConnection conn = Connection)
            {
                var newId = (await conn.QueryAsync<int>(sql)).Single();
                department.Id = newId;
                return CreatedAtRoute("GetDepartment", new { id = newId }, department);
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Department department)
        {
            string sql = $@"
            UPDATE Department
            SET Name = '{department.Name}',
                Budget = '{department.Budget}'
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
                if (!DepartmentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }
        

        private bool DepartmentExists(int id)
            {
                string sql = $"SELECT Id FROM Product WHERE Id = {id}";
                using (IDbConnection conn = Connection)
                {
                    return conn.Query<Department>(sql).Count() > 0;
                
            }

        }
    }
}
     