using IG.TechnicalInterview.Controllers;
using IG.TechnicalInterview.Data.Context;
using IG.TechnicalInterview.Domain;
using IG.TechnicalInterview.Model.Supplier;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using FizzWare.NBuilder;
using System.Threading.Tasks;
using System.Linq;

namespace IG.TechnicalInterview.Test
{
	public class SupplierControllerTests
	{
		private DbContextOptions<SupplierContext> dbContextOptions = new DbContextOptionsBuilder<SupplierContext>()
		.UseInMemoryDatabase(databaseName: "SupplierDatabase")
		.Options;
		private SupplierService _supplierService;
		private Guid _validId;

		[OneTimeSetUp]
		public void Setup()
		{
			SeedDb();
			_supplierService = new SupplierService(new SupplierContext(dbContextOptions));

		}

		private void SeedDb()
		{
			var context = new SupplierContext(dbContextOptions);
			_validId = Guid.NewGuid();
			var phones = Builder<Phone>.CreateListOfSize(2).Build();
			var suppliers = Builder<Supplier>.CreateListOfSize(10)
				.All()
					.With(x => x.Phones = phones)
				.Random(1).With(x => x.Id = _validId).Build();

			context.Suppliers.AddRange(suppliers);
			context.SaveChanges();
		}

		[Test]
		public async Task GetSuppliers_ReturnsSuppliers()
		{
			var result = await _supplierService.GetSuppliers();

			Assert.IsTrue(result.Any());
		}

		[Test]
		public async Task GetSupplier_ValidId_ReturnsSupplier()
		{
			var result = await _supplierService.GetSupplier(_validId);
			Assert.IsTrue(result.Id == _validId);
		}

		[Test]
		public async Task GetSupplier_InvalidId_ReturnsNull()
		{
			var result = await _supplierService.GetSupplier(Guid.Empty);
			Assert.IsNull(result);
		}

		[Test]
		public void InsertSupplier_ActivationDateToday_ThrowsArgumentException()
		{
	
			var supplier = Builder<Supplier>.CreateNew().With(x => x.ActivationDate = DateTime.UtcNow).Build();
			Assert.ThrowsAsync<ArgumentException>(async () => await _supplierService.InsertSupplier(supplier));
		}

		[Test]
		public async Task InsertSupplier_ActivationDateInFuture_IsSaved()
		{
			var id = Guid.NewGuid();
			var supplier = Builder<Supplier>.CreateNew().With(x => x.ActivationDate = DateTime.UtcNow.Date.AddDays(1)).With(x => x.Id = id).Build();
			await _supplierService.InsertSupplier(supplier);
			var matched = await _supplierService.GetSupplier(supplier.Id);
			Assert.AreEqual(id,matched.Id);
		}
	}
}