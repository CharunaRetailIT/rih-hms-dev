using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HospitalityManagement.Models;
using HospitalityManagement.Models.ViewModels;

namespace HospitalityManagement.Service
{
    public class CustomerService
    {

        ApplicationDbContext context = new ApplicationDbContext();

        public IEnumerable<Customer> GetCustomers()
        {
            try
            {
                IEnumerable<Customer> customer = context.Customers.Where(c=>c.IsDelete==false).OrderBy(g => g.CustomerCode);
                if (customer != null)
                {
                    return customer;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public IEnumerable<Customer> GetByLocId(long id)
        {
            try
            {
                IEnumerable<Customer> customer = context.Customers.Where(g => g.IsDelete == false && g.LocationId == id).OrderBy(g => g.CustomerCode);
                if (customer != null)
                {
                    return customer;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public IEnumerable<Customer> GetActiveCustomers()
        {
            try
            {
                IEnumerable<Customer> customer = context.Customers.Where(g => g.IsDelete == false).OrderBy(g => g.CustomerCode);
                if (customer != null)
                {
                    return customer;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public Customer GetCustomerById(long id)
        {
            try
            {
                Customer customer = context.Customers.Where(g => g.CustomerID == id).FirstOrDefault();
                if (customer != null)
                {
                    return customer;
                }
                else
                    return null;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public Customer GetCustomerByCode(string code)
        {
            try
            {
                Customer customer = context.Customers.Where(g => g.CustomerCode == code).FirstOrDefault();
                if (customer != null)
                {
                    return customer;
                }
                else
                    return null;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public int SaveCustomer(Customer cus)
        {
            try
            {
                context.Customers.Add(cus);
                int res = context.SaveChanges();
                return res;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public int UpdateCustomer(Customer cus)
        {
            try
            {

                //  ..context.SysGroupOfCompanys.Add(goc);
                int res = context.SaveChanges();
                return res;
            }
            catch (Exception ex)
            {

                

                throw;
            }
        }



        public IEnumerable<Customer> GetCustomerByLocId(long locid)
        {
            try
            {
                IEnumerable<Customer> cus = context.Customers.Where(e => e.LocationId == locid)
                                                                                        .OrderBy(k => k.CustomerName);


                if (cus != null)
                {
                    return cus;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }



        public List<Customer> GetCustomerDetailReport(long locid, long customerid)
        {
            try
            {
                List<Customer> customer = new List<Customer>();

                if (locid != 0 && customerid != 0)
                {
                    customer = context.Customers.Where(r => r.CustomerID == customerid && r.LocationId == locid).
                                         OrderBy(c => c.CustomerName).OrderBy(d => d.LocationId).ToList();
                }
                else if (locid != 0 && customerid == 0)
                {
                    customer = context.Customers.Where(r => r.LocationId == locid).
                                         OrderBy(c => c.CustomerName).OrderBy(d => d.LocationId).ToList();
                }
                else if (locid == 0 && customerid != 0)
                {
                    customer = context.Customers.Where(r => r.CustomerID == customerid).
                                         OrderBy(c => c.CustomerName).OrderBy(d => d.LocationId).ToList();
                }
                else if (locid == 0 && customerid == 0)
                {
                    customer = context.Customers.
                                         OrderBy(c => c.CustomerName).OrderBy(d => d.LocationId).ToList();
                }

                if (customer != null)
                {
                    return customer;
                }

                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public Customer FindByCode(string code)
        {
            var customer = context.Customers.Where(c => c.CustomerCode == code).FirstOrDefault();
            if (customer != null)
            {
                return customer;
            }
            else
            {
                return null;
            }

        }



    }
}