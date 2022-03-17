using Dapper;
using Microsoft.Extensions.Configuration;
using Parcial1Ud.Data;
using Parcial1U.IService;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace MudBlazorDapperCrud.Service
{
    public class EmployeeService : IEmployeeService
    {
        HashSet<Employee> _employees = new HashSet<Employee>();
        Employee _employee = new Employee();

        public IConfiguration _configuration { get; }
        public string _connectionString = "";

        public EmployeeService(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("EmployeeDB1");
        }
        public string Delete(int employeeId)
        {
            string message = "Failed";
            using (IDbConnection con = new SqlConnection(_connectionString))
            {
                _employee = new Employee()
                {
                    EmployeeId = employeeId
                };

                if (con.State == ConnectionState.Closed) con.Open();
                con.Query<Employee>("SP_Employee", this.SetParameters(_employee, (int)OperationType.Delete),
                    commandType: CommandType.StoredProcedure);

                message = "Deleted";
            }
            return message;
        }

        public Employee GetById(int employeeId)
        {
            _employee = new Employee();
            using (IDbConnection con = new SqlConnection(_connectionString))
            {
                if (con.State == ConnectionState.Closed) con.Open();
                var employees = con.Query<Employee>("SELECT * FROM Employee WHERE EmployeeId=" + employeeId);
                if (employees != null && employees.Count() > 0)
                {
                    _employee = employees.FirstOrDefault();
                }
            }
            return _employee;
        }

        public HashSet<Employee> GetEmployees()
        {
            _employees = new HashSet<Employee>();
            using (IDbConnection con = new SqlConnection(_connectionString))
            {
                if (con.State == ConnectionState.Closed) con.Open();
                var employees = con.Query<Employee>("SELECT * FROM Employee").ToHashSet();
                if (employees != null && employees.Count() > 0)
                {
                    _employees = employees;
                }
            }
            return _employees;
        }

        public Employee SaveOrUpdate(Employee employee)
        {
            _employee = new Employee();
            using (IDbConnection con = new SqlConnection(_connectionString))
            {
                if (con.State == ConnectionState.Closed) con.Open();

                int operationType = Convert.ToInt32(employee.EmployeeId == 0 ? OperationType.Insert : OperationType.Update);

                var employees = con.Query<Employee>("SP_Employee", this.SetParameters(employee, operationType),
                    commandType: CommandType.StoredProcedure);

                if (employees != null && employees.Count() > 0)
                {
                    _employee = employees.FirstOrDefault();
                }
            }
            return _employee;
        }

        private DynamicParameters SetParameters(Employee employee, int operationType)
        {
            DynamicParameters obj = new DynamicParameters();
            obj.Add("@EmployeeId", employee.EmployeeId);
            obj.Add("@Name", employee.Name);
            obj.Add("@Gender", employee.Gender);
            obj.Add("@OperationType", operationType);
            return obj;
        }
    }
}
