﻿using CarDealer.Data;
using CarDealer.DTO;
using CarDealer.Models;
using ProductShop.XMLHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CarDealer
{
    public class StartUp
    {
        public static void Main()
        {
            var carDealerContext = new CarDealerContext();

            carDealerContext.Database.EnsureDeleted();
            carDealerContext.Database.EnsureCreated();

            string inputXml1 = File.ReadAllText(@"Datasets/suppliers.xml");
            string inputXml2 = File.ReadAllText(@"Datasets/parts.xml");
            string inputXml3 = File.ReadAllText(@"Datasets/cars.xml");
            //string inputXml4 = File.ReadAllText(@"Datasets/customers.xml");
            //string inputXml5 = File.ReadAllText(@"Datasets/sales.xml");

            Console.WriteLine(ImportSuppliers(carDealerContext, inputXml1));
            Console.WriteLine(ImportParts(carDealerContext, inputXml2));
            Console.WriteLine(ImportCars(carDealerContext, inputXml3));
            //Console.WriteLine(ImportCustomers(carDealerContext, inputXml4));
            //Console.WriteLine(ImportSales(carDealerContext, inputXml5));
        }

        //09. Import Suppliers
        public static string ImportSuppliers(CarDealerContext context, string inputXml1)
        {
            var rootAttributeName = "Suppliers";

            var suppliersDTO = XMLConverter.Deserializer<ImportSupplierDTO>(inputXml1, rootAttributeName);

            var suppliers = suppliersDTO.Select(x => new Supplier
            {
                Name = x.Name,
                IsImporter = x.IsImporter
            });

            context.Suppliers.AddRange(suppliers);

            context.SaveChanges();

            return $"Successfully imported {suppliers.Count()}";
        }

        //10. Import Parts
        public static string ImportParts(CarDealerContext context, string inputXml2)
        {
            var rootAttributeName = "Parts";

            var partsDTO = XMLConverter.Deserializer<ImportPartDTO>(inputXml2, rootAttributeName);

            var supplierIds = context.Suppliers.Select(x => x.Id);

            var parts = partsDTO
                .Where(x => supplierIds.Contains(x.SupplierId))
                .Select(x => new Part
                {
                    Name = x.Name,
                    Price = x.Price,
                    Quantity = x.Quantity,
                    SupplierId = x.SupplierId
                });

            context.Parts.AddRange(parts);

            context.SaveChanges();

            return $"Successfully imported {parts.Count()}";
        }

        //11. Import Cars
        public static string ImportCars(CarDealerContext context, string inputXml3)
        {
            var rootAttributeName = "Cars";

            var carsDTO = XMLConverter.Deserializer<ImportCarDTO>(inputXml3, rootAttributeName).ToArray();

            var partIds = context.Parts.Select(x => x.Id).ToArray();

            var cars = carsDTO.Select(x => new Car
            {
                Make = x.Make,
                Model = x.Model,
                TravelledDistance = x.TravelledDistance,
                PartCars = x.Parts
                .Where(y => partIds.Contains(y.Id))
                .Select(y => new PartCar
                {
                    PartId = y.Id
                })
                .Distinct()
                .ToArray()
            })
            .ToArray();

            context.Cars.AddRange(cars);

            context.SaveChanges();

            return $"Successfully imported {cars.Length}";
        }

        ////12. Import Customers
        //public static string ImportCustomers(CarDealerContext context, string inputJson)
        //{
        //    var customers = JsonConvert.DeserializeObject<Customer[]>(inputJson);

        //    context.Customers.AddRange(customers);

        //    context.SaveChanges();

        //    return $"Successfully imported { customers.Count()}";
        //}

        ////13. Import Sales
        //public static string ImportSales(CarDealerContext context, string inputJson)
        //{
        //    var sales = JsonConvert.DeserializeObject<Sale[]>(inputJson);

        //    context.Sales.AddRange(sales);

        //    context.SaveChanges();

        //    return $"Successfully imported { sales.Count()}";
        //}

        ////14. Export Ordered Customers
        //public static string GetOrderedCustomers(CarDealerContext context)
        //{
        //    var customers = context.Customers
        //        .ProjectTo<CustomerDTO>()
        //        .OrderBy(x => x.BirthDate)
        //        .ThenBy(x => x.IsYoungDriver);

        //    var settings = new JsonSerializerSettings
        //    {
        //        DateFormatString = "dd/MM/yyyy",
        //        Formatting = Formatting.Indented
        //    };

        //    return JsonConvert.SerializeObject(customers, settings);
        //}

        ////15. Export Cars From Make Toyota
        //public static string GetCarsFromMakeToyota(CarDealerContext context)
        //{
        //    var categories = context.Cars
        //        .Where(x => x.Make == "Toyota")
        //        .ProjectTo<CarDTO>()
        //        .OrderBy(x => x.Model)
        //        .ThenByDescending(x => x.TravelledDistance);

        //    return JsonConvert.SerializeObject(categories, Formatting.Indented);
        //}

        ////16. Export Local Suppliers
        //public static string GetLocalSuppliers(CarDealerContext context)
        //{
        //    var suppliers = context.Suppliers
        //        .Where(x => x.IsImporter == false)
        //        .ProjectTo<SupplierDTO>();

        //    return JsonConvert.SerializeObject(suppliers, Formatting.Indented);
        //}

        ////17. Export Cars With Their List Of Parts
        //public static string GetCarsWithTheirListOfParts(CarDealerContext context)
        //{
        //    var carsParts = context.Cars
        //        .Select(cp => new
        //        {
        //            car = new
        //            {
        //                cp.Make,
        //                cp.Model,
        //                cp.TravelledDistance
        //            },
        //            parts = cp.PartCars
        //            .Select(p => new
        //            {
        //                p.Part.Name,
        //                Price = p.Part.Price.ToString("f2")
        //            })
        //        });

        //    return JsonConvert.SerializeObject(carsParts, Formatting.Indented);
        //}

        ////18. Export Total Sales By Customer
        //public static string GetTotalSalesByCustomer(CarDealerContext context)
        //{
        //    var customer = context.Customers
        //        .Where(x => x.Sales.Any())
        //        .Select(c => new
        //        {
        //            fullName = c.Name,
        //            boughtCars = c.Sales.Count,
        //            spentMoney = c.Sales
        //            .Select(s => s.Car
        //                          .PartCars
        //                          .Sum(pc => pc.Part.Price))
        //            .Sum()
        //        })
        //        .OrderByDescending(x => x.spentMoney)
        //        .ThenBy(x => x.boughtCars);

        //    return JsonConvert.SerializeObject(customer, Formatting.Indented);
        //}

        ////19. Export Sales With Applied Discount
        //public static string GetSalesWithAppliedDiscount(CarDealerContext context)
        //{
        //    var sales = context.Sales
        //        .Take(10)
        //        .Select(s => new
        //        {
        //            car = new
        //            {
        //                s.Car.Make,
        //                s.Car.Model,
        //                s.Car.TravelledDistance
        //            },
        //            customerName = s.Customer.Name,
        //            Discount = s.Discount.ToString("f2"),
        //            price = s.Car
        //                     .PartCars
        //                     .Sum(pc => pc.Part.Price)
        //                     .ToString("f2"),
        //            priceWithDiscount = (s.Car
        //                                  .PartCars
        //                                  .Sum(pc => pc.Part.Price) * ((100 - s.Discount) / 100))
        //                                  .ToString("f2")
        //        });

        //    return JsonConvert.SerializeObject(sales, Formatting.Indented);
        //}
    }
}