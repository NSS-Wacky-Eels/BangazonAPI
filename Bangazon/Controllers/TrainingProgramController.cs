using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Bangazon.Models;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Bangazon.Controllers
{
    [Route("api/[controller]")]
    public class TrainingProgramController : ControllerBase
    {
        private readonly IConfiguration _config;

        public TrainingProgramController(IConfiguration config)
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
        // GET: api/<TraingProgram>
        [HttpGet]
        public async Task<IActionResult> Get(string q)
        {
            //Query sql server to select (a,b,c,) from table
            string sql = @"
            SELECT
                tp.Id,
                tp.StartDate,
                tp.EndDate,
                tp.MaxAttendees
            FROM TrainingProgram tp
            WHERE 1=1
            ";
            if (q != null)
            {
                string isQ = $@"
                    AND tp.Id LIKE '%{q}%'
                    OR tp.StartDate LIKE '%{q}%'
                    OR tp.EndDate LIKE '%{q}%'
                    OR tp.MaxAttendees '%{q}%'
                ";
                sql = $"{sql} {isQ}";
            }
            Console.WriteLine(sql);
            //Using database 
            using (IDbConnection conn = Connection)
            {

                IEnumerable<TrainingProgram> trainingPrograms = await conn.QueryAsync<TrainingProgram>(
                    sql
                );
                return Ok(trainingPrograms);
            }
        }
        // GET multiple api/<controller>/5
        [HttpGet("{id}", Name = "GetTraingProgram")]
        public async Task<IActionResult> Get([FromRoute]int id)
        {
            string sql = $@"
           SELECT
                tp.Id,
                tp.StartDate,
                tp.EndDate,
                tp.MaxAttendees
            FROM TrainingProgram tp
            WHERE tp.Id = {id}
            ";
            using (IDbConnection conn = Connection)
            {

                IEnumerable<TrainingProgram> trainingPrograms = await conn.QueryAsync<TrainingProgram>(
                    sql
                );
                return Ok(trainingPrograms.Single());
            }

        }
        // POST api/<controller>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] TrainingProgram trainingProgram)
        {
            string sql = $@"INSERT INTO TrainingProgram 
            (StartDate, EndDate, MaxAttendees)
            VALUES
            ( '{trainingProgram.StartDate}',
             '{trainingProgram.EndDate}',
             '{trainingProgram.MaxAttendees}');

            SELECT SCOPE_IDENTITY();";

            using (IDbConnection conn = Connection)
            {
                var newId = (await conn.QueryAsync<int>(sql)).Single();
                trainingProgram.Id = newId;
                return CreatedAtRoute("GetTrainingProgram", new { Id = newId }, trainingProgram);
            }
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] TrainingProgram trainingProgram)
        {
            string sql = $@"
            UPDATE ProductType
            SET Name = '{trainingProgram.StartDate}'
                        '{trainingProgram.EndDate}'
                        '{trainingProgram.MaxAttendees}'
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
                if (!TrainingProgram(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }

            }
        }
        private bool TrainingProgram(int id)
        {
            string sql = $"SELECT Id FROM TrainingProgram WHERE Id = {id}";
            using (IDbConnection conn = Connection)
            {
                return conn.Query<TrainingProgram>(sql).Count() > 0;
            }
        }
    }
}


     











//        // DELETE api/<controller>/5
//        [HttpDelete("{id}")]
//        public void Delete(int id)
//        {
//        }
//    }
//}
