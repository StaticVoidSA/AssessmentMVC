using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Sizanani_Logistics.Models;
using System.ComponentModel.Design;
using System.Diagnostics.Contracts;

namespace Sizanani_Logistics.Controllers
{
    public class ContractorsController : Controller
    {
        private readonly string _connString;

        public ContractorsController(IConfiguration configuration)
        {
            _connString = configuration.GetConnectionString("DefaultConnectionString");
        }

        [HttpGet]
        public IActionResult Index()
        {
            List<Contractor> contractors = new List<Contractor>();

            using (var conn = new SqlConnection(_connString))
            {
                var command = new SqlCommand("SELECT * FROM Contractors", conn);
                conn.Open();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        contractors.Add(new Contractor
                        {
                            ContractorId = Convert.ToInt32(reader["ContractorId"]),
                            ContractorName = reader["ContractorName"].ToString() ?? "",
                            Email = reader["Email"].ToString() ?? "",
                            PhoneNumber = reader["PhoneNumber"].ToString() ?? ""
                        });
                    }
                }
            }


            return View(contractors);
        }

        [HttpGet]
        public IActionResult Edit(int contractorId) {
            using var conn = new SqlConnection(_connString);
            using var cmd = new SqlCommand(@"SELECT * FROM Contractors WHERE ContractorId = @ContractorId", conn);
            cmd.Parameters.AddWithValue("@ContractorId", contractorId);

            conn.Open();

            using var reader = cmd.ExecuteReader();

            if (!reader.Read())
            {
                return NotFound();
            }

            var contractor = new Contractor
            {
                ContractorId = Convert.ToInt32(reader["ContractorId"]),
                ContractorName = reader["ContractorName"].ToString() ?? "",
                Email = reader["Email"].ToString() ?? "",
                PhoneNumber = reader["PhoneNumber"].ToString() ?? ""
            };

            return View(contractor);
        }

        [HttpPost]
        public IActionResult Edit(Contractor contractor)
        {
            if (!ModelState.IsValid)
            {
                return View(contractor);
            }

            using (var conn = new SqlConnection(_connString))
            {
                using var command = new SqlCommand("UPDATE Contractors SET ContractorName = @ContractorName, Email = @Email, PhoneNumber = @PhoneNumber WHERE ContractorId = @ContractorId", conn);
                command.Parameters.AddWithValue("@ContractorId", contractor.ContractorId);
                command.Parameters.AddWithValue("@ContractorName", contractor.ContractorName ?? "");
                command.Parameters.AddWithValue("@Email", contractor.Email ?? "");
                command.Parameters.AddWithValue("@PhoneNumber", contractor.PhoneNumber ?? "");

                conn.Open();
                command.ExecuteNonQuery();
            }

            return RedirectToAction("Index", "Contractors");
        }

        public IActionResult Delete(int? contractorId)
        {

            if (contractorId != null)
            {

                using (var conn = new SqlConnection(_connString))
                {
                    var command = new SqlCommand("DELETE FROM Vehicles WHERE ContractorId = @contractorId; DELETE FROM Contractors WHERE ContractorId = @contractorId", conn);
                    command.Parameters.AddWithValue("@contractorId", contractorId);
                    conn.Open();
                    command.ExecuteNonQuery();
                }
            }

            return RedirectToAction("Index", "Contractors");
        }

        [HttpGet]
        public IActionResult RegisterContractor()
        {
            return View();
        }

        [HttpPost]
        public IActionResult RegisterContractor(Contractor contractor) 
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(contractor);
                }

                using (var conn = new SqlConnection(_connString))
                { 
                    var cmd = new SqlCommand("INSERT INTO Contractors (ContractorName, Email, PhoneNumber) VALUES(@ContractorName, @Email, @PhoneNumber)", conn);
                    cmd.Parameters.AddWithValue("@ContractorName", contractor.ContractorName ?? "");
                    cmd.Parameters.AddWithValue("@Email", contractor.Email ?? "");
                    cmd.Parameters.AddWithValue("@PhoneNumber", contractor.PhoneNumber ?? "");

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
