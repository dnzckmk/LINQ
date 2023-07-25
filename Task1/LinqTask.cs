// <copyright file="LinqTask.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using Task1.DoNotChange;

namespace Task1
{
    /*
     Queries:
        Select the customers whose total turnover (the sum of all orders) exceeds a certain value.

        For each customer make a list of suppliers located in the same country and the same city. Compose queries with and without grouping.

        1.     Find all customers with the sum of all orders that exceed a certain value.

        2.     Select the clients, including the date of their first order.

        3.     Repeat the previous query but order the result by year, month, turnover (descending) and customer name.

        4.     Select the clients which either have:

               a. non-digit postal code

               b. undefined region

               c. operator code in the phone is not specified (does not contain parentheses)

        5.     Group the products by category, then by availability in stock with ordering by cost.

        6.     Group the products by “cheap”, “average” and “expensive” following the rules:

                a. From 0 to cheap inclusive

                b. From cheap exclusive to average inclusive

                c. From average exclusive to expensive inclusive

        7.    Calculate the average profitability of each city (average amount of orders per customer) and average rate (average number of orders per customer from each city).

        8.     Build a string of unique supplier country names, sorted first by length and then by country.
     */

    /// <summary>
    /// LinqTask excersize.
    /// </summary>
    public static class LinqTask
    {
        /// <summary>
        /// Find all customers with the sum of all orders that exceed a certain value.
        /// </summary>
        /// <param name="customers">IEnumerable customer.</param>
        /// <param name="limit">Minimum limit for sum of customers orders totals.</param>
        /// <returns>Returns IEnumerable customer.</returns>
        public static IEnumerable<Customer> Linq1(IEnumerable<Customer> customers, decimal limit)
        {
            return customers.Where(customer => customer.Orders.Sum(order => order.Total) > limit);
        }

        /// <summary>
        /// For each customer make a list of suppliers located in the same country and the same city.
        /// </summary>
        /// <param name="customers">IEnumerable customers.</param>
        /// <param name="suppliers">IEnumerable suppliers.</param>
        /// <returns>Returns IEnumerable Customers and Suppliers that are from same country and city.</returns>
        public static IEnumerable<(Customer customer, IEnumerable<Supplier> suppliers)> Linq2(
            IEnumerable<Customer> customers,
            IEnumerable<Supplier> suppliers)
        {
            return customers.Select(customer => (customer, suppliers
            .Where(supplier =>
                    string.Equals(supplier.Country, customer.Country, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(supplier.City, customer.City, StringComparison.OrdinalIgnoreCase))));
        }

        /// <summary>
        /// For each customer make a list of suppliers located in the same country and the same city. Compose queries with and without grouping.
        /// </summary>
        /// <param name="customers">IEnumerable customers.</param>
        /// <param name="suppliers">IEnumerable suppliers.</param>
        /// <returns>Returns IEnumerable Customers and Suppliers that are from same country and city.</returns>
        public static IEnumerable<(Customer customer, IEnumerable<Supplier> suppliers)> Linq2UsingGroup(
            IEnumerable<Customer> customers,
            IEnumerable<Supplier> suppliers)
        {
            return from customer in customers
                   join supplier in suppliers
                   on new { customer.Country, customer.City }
                   equals new { supplier.Country, supplier.City }
                   into matchedSuppliersGroup
                   select (customer, matchedSuppliersGroup);
        }

        /// <summary>
        ///  Find all customers with the sum of all orders that exceed a certain value.
        ///  Select the clients, including the date of their first order.
        ///  Repeat the previous query but order the result by year, month, turnover (descending) and customer name.
        /// </summary>
        /// <param name="customers">Ienumerable customers.</param>
        /// <param name="limit">Minimum limit for sum of customers orders totals.</param>
        /// <returns>Returns IEnumerable customers that ordered as stated in summary.</returns>
        public static IEnumerable<Customer> Linq3(IEnumerable<Customer> customers, decimal limit)
        {
            return customers.Where(customer => customer.Orders.Length > 0 && customer.Orders.Sum(order => order.Total) > limit)
                .OrderBy(customer => FindCustomerOrdersMinDate(customer))
                .ThenByDescending(customer => customer.Orders.Sum(order => order.Total))
                .ThenBy(customer => customer.CompanyName);
        }

        /// <summary>
        /// Select the clients, including the date of their first order.
        /// </summary>
        /// <param name="customers">IEnumerable customer.</param>
        /// <returns>Returns customers with the date of their first order.</returns>
        public static IEnumerable<(Customer customer, DateTime dateOfEntry)> Linq4(IEnumerable<Customer> customers)
        {
            return customers.Where(customer => customer.Orders.Length > 0)
                .Select(customer => (customer, FindCustomerOrdersMinDate(customer)));
        }

        /// <summary>
        /// Select the clients, including the date of their first order.
        /// Repeat the previous query but order the result by year, month, turnover (descending) and customer name.
        /// </summary>
        /// <param name="customers">IEnumerable customer.</param>
        /// <returns>Returns customers with the date of their first order which are in order by minimum date, turnover(descending) and company name.</returns>
        public static IEnumerable<(Customer customer, DateTime dateOfEntry)> Linq5(IEnumerable<Customer> customers)
        {
            return customers.Where(customer => customer.Orders.Length > 0)
                .Select(customer => (customer, FindCustomerOrdersMinDate(customer)))
                .OrderBy(entry => FindCustomerOrdersMinDate(entry.customer))
                .ThenByDescending(entry => entry.customer.Orders.Sum(order => order.Total))
                .ThenBy(entry => entry.customer.CompanyName);
        }

        /// <summary>
        ///  Select the clients which either have:
        ///  non-digit postal code
        ///  undefined region
        ///  operator code in the phone is not specified (does not contain parentheses).
        /// </summary>
        /// <param name="customers">IEnumerable customers.</param>
        /// <returns>Returns IEnumerable customer which either have non-digit postal code, undefined region or operator code in the phone is not specified.</returns>
        public static IEnumerable<Customer> Linq6(IEnumerable<Customer> customers)
        {
            return customers.Where(customer =>
                !int.TryParse(customer.PostalCode, out _) ||
                string.IsNullOrEmpty(customer.Region) ||
                !customer.Phone.Contains('('));
        }

        /// <summary>
        /// Group the products by category, then by availability in stock with ordering by cost.
        /// </summary>
        /// <param name="products">IEnumerable products.</param>
        /// <returns>Returns IEnumerableCategoryGroup which includes Category and UnitsInStockGroup, and UnitsInStockGroup includes units in stock and prices.</returns>
        public static IEnumerable<Linq7CategoryGroup> Linq7(IEnumerable<Product> products)
        {
            /* example of Linq7result

             category - Beverages
                UnitsInStock - 39
                    price - 18.0000
                    price - 19.0000
                UnitsInStock - 17
                    price - 18.0000
                    price - 19.0000
             */

            return products.GroupBy(product => product.Category)
                .Select(group => new Linq7CategoryGroup
                {
                    Category = group.Key,
                    UnitsInStockGroup = group.GroupBy(product => product.UnitsInStock)
                                            .Select(unitGroup => new Linq7UnitsInStockGroup
                                            {
                                                UnitsInStock = unitGroup.Key,
                                                Prices = unitGroup.Select(product => product.UnitPrice)
                                                    .OrderBy(product => product),
                                            }),
                });
        }

        /// <summary>
        /// Group the products by “cheap”, “average” and “expensive” following the rules:
        /// From 0 to cheap inclusive
        /// From cheap exclusive to average inclusive
        /// From average exclusive to expensive inclusive.
        /// </summary>
        /// <param name="products">IEnumerable products.</param>
        /// <param name="cheap">Cheap price value.</param>
        /// <param name="middle">Middle price value.</param>
        /// <param name="expensive">Expensive price value.</param>
        /// <returns>Returns IEnumerable price category and product list.</returns>
        public static IEnumerable<(decimal category, IEnumerable<Product> products)> Linq8(
            IEnumerable<Product> products,
            decimal cheap,
            decimal middle,
            decimal expensive)
        {
            var cheapProducts = products.Where(p => p.UnitPrice <= cheap);
            var middleProducts = products.Where(p => p.UnitPrice > cheap && p.UnitPrice <= middle);
            var expensiveProducts = products.Where(p => p.UnitPrice > middle && p.UnitPrice <= expensive);

            return new[]
            {
                (category: cheap, products: cheapProducts),
                (category: middle, products: middleProducts),
                (category: expensive, products: expensiveProducts),
            };
        }

        /// <summary>
        /// Calculate the average profitability of each city (average amount of orders per customer) and average rate (average number of orders per customer from each city).
        /// </summary>
        /// <param name="customers">IEnumerable customers.</param>
        /// <returns>Returns IEnumerable city, average income and average intensity data.</returns>
        public static IEnumerable<(string city, int averageIncome, int averageIntensity)> Linq9(IEnumerable<Customer> customers)
        {
            return customers.GroupBy(customer => customer.City)
                .Select(group => (
                    city: group.Key,
                    averageIncome: (int)Math.Round(group.Average(customer => customer.Orders.Sum(order => order.Total))),
                    averageIntensity: (int)Math.Round(group.Average(customer => customer.Orders.Length))));
        }

        /// <summary>
        /// Build a string of unique supplier country names, sorted first by length and then by country.
        /// </summary>
        /// <param name="suppliers"><IEnumerable suppliers.</param>
        /// <returns>Returns string of unique supplier country names, sorted first by length and then by country.</returns>
        public static string Linq10(IEnumerable<Supplier> suppliers)
        {
            return string.Join(string.Empty, suppliers.Select(supplier => supplier.Country)
                .Distinct()
                .OrderBy(country => country.Length)
                .ThenBy(country => country));
        }

        private static DateTime FindCustomerOrdersMinDate(Customer customer)
        {
            var min = DateTime.MaxValue;
            foreach (var order in customer.Orders)
            {
                if (order.OrderDate < min)
                {
                    min = order.OrderDate;
                }
            }

            return min;
        }
    }
}