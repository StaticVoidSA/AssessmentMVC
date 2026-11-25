using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.Identity.Client;
using Sizanani_Logistics.Models;
using System.Collections.Specialized;
using System.ComponentModel;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Sizanani_Logistics.Controllers
{
    public class VehiclesController : Controller
    {
        private readonly string _connStr;

        public VehiclesController(IConfiguration configuration)
        {
            _connStr = configuration.GetConnectionString("DefaultConnectionString");
        }

        // Display List of Vehicles
        public IActionResult Index()
        {
            List<Vehicle> vehicles = new List<Vehicle>();

            using (var conn = new SqlConnection(_connStr))
            {
                var command = new SqlCommand("SELECT * FROM Vehicles", conn);
                conn.Open();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        vehicles.Add(new Vehicle
                        {
                            VehicleId = Convert.ToInt32(reader["VehicleId"]),
                            Registration = reader["Registration"].ToString() ?? "",
                            Type = reader["Type"].ToString() ?? "",
                            Model = reader["Model"].ToString() ?? "",
                            Weight = Convert.ToInt32(reader["Weight"]),
                            ContractorId = Convert.ToInt32(reader["ContractorId"])
                        });
                    }
                }
            }


            return View(vehicles);
        }

        [HttpGet]
        public IActionResult Edit(int vehicleId)
        {
            using var conn = new SqlConnection(_connStr);
            using var cmd = new SqlCommand(@"SELECT * FROM Vehicles WHERE VehicleId = @VehicleId", conn);
            cmd.Parameters.AddWithValue("@VehicleId", vehicleId);

            conn.Open();

            using var reader = cmd.ExecuteReader();

            if (!reader.Read())
            {
                return NotFound();
            }

            var vehicle = new Vehicle
            {
                VehicleId = Convert.ToInt32(reader["VehicleId"]),
                Registration = reader["Registration"].ToString() ?? "",
                Type = reader["Type"].ToString() ?? "",
                Model = reader["Model"].ToString() ?? "",
                Weight = Convert.ToInt32(reader["Weight"]),
                ContractorId = Convert.ToInt32(reader["ContractorId"])
            };

            return View(vehicle);
        }

        [HttpPost]
        public IActionResult Edit(Vehicle vehicle)
        {
            if (!ModelState.IsValid)
            {
                return View(vehicle);
            }

            using (var conn = new SqlConnection(_connStr))
            {
                using var command = new SqlCommand("UPDATE Vehicles SET Registration = @Registration, Type = @Type, Model = @Model, Weight = @Weight WHERE VehicleId = @VehicleId", conn);
                command.Parameters.AddWithValue("@VehicleId", vehicle.VehicleId);
                command.Parameters.AddWithValue("@Registration", vehicle.Registration ?? "");
                command.Parameters.AddWithValue("@Type", vehicle.Type ?? "");
                command.Parameters.AddWithValue("@Model", vehicle.Model ?? "");
                command.Parameters.AddWithValue("@Weight", vehicle.Weight ?? 0);

                conn.Open();

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    return NotFound();
                }
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(int? vehicleId) {

            if (vehicleId != null) {

                using (var conn = new SqlConnection(_connStr))
                {
                    var command = new SqlCommand("DELETE FROM Vehicles WHERE VehicleId = @vehicleId", conn);
                    command.Parameters.AddWithValue("@vehicleId", vehicleId);
                    conn.Open();
                    command.ExecuteNonQuery();
                }
            }

            return RedirectToAction("Index", "Vehicles");
        }

        public IActionResult Summation()
        {
            List<ContractorSummation> summations = new List<ContractorSummation>();

            using (var conn = new SqlConnection(_connStr))
            {
                var query = @"SELECT Contractors.ContractorName, COUNT(Vehicles.VehicleId) AS VehicleCount, ISNULL(SUM(Vehicles.Weight), 0) AS TotalTons
                    FROM Contractors
                    LEFT JOIN Vehicles
                    ON Contractors.ContractorId = Vehicles.ContractorId
                    GROUP BY Contractors.ContractorId, Contractors.ContractorName
                    ORDER BY Contractors.ContractorName";

                using var command = new SqlCommand(query, conn);

                conn.Open();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        summations.Add(new ContractorSummation
                        {
                            ContractorName = reader["ContractorName"].ToString() ?? "",
                            VehicleCount = Convert.ToInt32(reader["VehicleCount"]),
                            TotalTons = Convert.ToInt32(reader["TotalTons"])
                        });
                    }
                }
            }

            return View(summations);
        }

        [HttpGet]
        public IActionResult ContractorVehicles(int? contractorId) {
            List<Contractor> contractors = new List<Contractor>();
            List<Vehicle> vehicles = new List<Vehicle>();

            // Populate contractors drop down
            using (var conn = new SqlConnection(_connStr))
            {
                var command = new SqlCommand("SELECT ContractorId, ContractorName FROM Contractors", conn);
                conn.Open();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        contractors.Add(new Contractor
                        {
                            ContractorId = Convert.ToInt32(reader["ContractorId"]),
                            ContractorName = reader["ContractorName"].ToString() ?? ""
                        });
                    }
                }
            }

            // Populate vehicles table based off contractorId 
            if (contractorId != null)
            {
                using (var conn = new SqlConnection(_connStr))
                {
                    var query = @"SELECT VehicleId, Type, Registration, Model, Weight, ContractorId FROM Vehicles WHERE ContractorId = @contractorId";
                    var vehicles_command = new SqlCommand(query, conn);

                    vehicles_command.Parameters.AddWithValue("@contractorId", contractorId);
                    conn.Open();

                    using (var reader = vehicles_command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            vehicles.Add(new Vehicle
                            {
                                VehicleId = Convert.ToInt32(reader["VehicleId"]),
                                Registration = reader["Registration"].ToString() ?? "",
                                Type = reader["Type"].ToString() ?? "",
                                Model = reader["Model"].ToString() ?? "",
                                Weight = Convert.ToInt32(reader["Weight"]),
                                ContractorId = Convert.ToInt32(reader["ContractorId"])
                            });
                        }
                    }
                }
            }

            ViewBag.Contractors = new SelectList(contractors, "ContractorId", "ContractorName", contractorId);

            return View(vehicles);
        }

        [HttpGet]
        public IActionResult RegisterVehicle() {
            List<Contractor> contractors = new List<Contractor>();

            using (var conn = new SqlConnection(_connStr))
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

            ViewBag.Contractors = new SelectList(contractors, "ContractorId", "ContractorName");
            return View();
        }

        [HttpPost]
        public IActionResult RegisterVehicle(Vehicle vehicle)
        {

            if (!ModelState.IsValid)
            {
                return View(vehicle);
            }

            using (var conn = new SqlConnection(_connStr))
            {
                var query = @"INSERT INTO Vehicles (Registration, Type, Model, Weight, ContractorId) VALUES(@Registration, @Type, @Model, @Weight, @ContractorId)";
                var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Registration", vehicle.Registration ?? "");
                cmd.Parameters.AddWithValue("@Type", vehicle.Type ?? "");
                cmd.Parameters.AddWithValue("@Model", vehicle.Model ?? "");
                cmd.Parameters.AddWithValue("@Weight", vehicle.Weight ?? 0);
                cmd.Parameters.AddWithValue("@ContractorId", vehicle.ContractorId); 

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("ContractorVehicles", "Vehicles", new { contractorId = vehicle.ContractorId }); // Redirect to all Contractor Vehicles when adding
        }


    }
}
