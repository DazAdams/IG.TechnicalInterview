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
		private readonly DbContextOptions<SupplierContext> dbContextOptions = new DbContextOptionsBuilder<SupplierContext>()
		.UseInMemoryDatabase(databaseName: "SupplierDatabase")
		.Options;
		private SupplierService _supplierService;
		private Guid _validId;
		private Guid _inActiveSupplierId;
		private Guid _inActiveSupplierId2;
		const string VALID_PHONE = "1111111111";

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
			_inActiveSupplierId = Guid.NewGuid();
			_inActiveSupplierId2 = Guid.NewGuid();
			var phones = Builder<Phone>.CreateListOfSize(2).Build();
			var emails = Builder<Email>.CreateListOfSize(2).Build();
			var suppliers = Builder<Supplier>.CreateListOfSize(10)
				.All()
					.With(x => x.Phones = phones)
					.With(x => x.Emails = emails)
					.With(x => x.Id = Guid.NewGuid())
				.Random(1).With(x => x.Id = _validId)
				.Build();

			//Add  InActive Suppliers

			suppliers.Add(new Supplier
			{
				Id = _inActiveSupplierId,
				FirstName = "Test",
				LastName = "McTester",
				Title = "Dr",
				ActivationDate = null
			});

			suppliers.Add(new Supplier
			{
				Id = _inActiveSupplierId2,
				FirstName = "Test",
				LastName = "McTester2",
				Title = "Mr",
				ActivationDate = null
			});

			context.Suppliers.AddRange(suppliers);
			context.SaveChanges();
		}

		#region GetSuppliers
		[Test]
		public async Task GetSuppliers_ReturnsSuppliers()
		{
			var result = await _supplierService.GetSuppliers();

			Assert.IsTrue(result.Any());
		}
		#endregion

		#region GetSupplier
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
		#endregion

		#region InsertSupplier
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
			var supplier = Builder<Supplier>.CreateNew()
				.With(x => x.ActivationDate = DateTime.UtcNow.Date.AddDays(1))
				.With(x => x.Id = id).Build();
			await _supplierService.InsertSupplier(supplier);
			var matched = await _supplierService.GetSupplier(supplier.Id);
			Assert.AreEqual(id, matched.Id);
		}

		[Test]
		public void InsertSupplier_InvalidPhoneNumberNonNumeric_ThrowsArgumentException()
		{

			List<Phone> phones = new List<Phone>
			{
				new Phone { Id = Guid.NewGuid(), IsPreferred = true, PhoneNumber = "111111aa" }
			};

			var supplier = Builder<Supplier>.CreateNew()
				.With(x => x.ActivationDate = DateTime.UtcNow.Date.AddDays(1))
				.With(x => x.Phones = phones).Build();
			Assert.ThrowsAsync<ArgumentException>(async () => await _supplierService.InsertSupplier(supplier));
		}

		[Test]
		public void InsertSupplier_InvalidPhoneNumberTooLong_ThrowsArgumentException()
		{

			List<Phone> phones = new List<Phone>
			{
				new Phone { Id = Guid.NewGuid(), IsPreferred = true, PhoneNumber = "111111111111111111" }
			};

			var supplier = Builder<Supplier>.CreateNew()
				.With(x => x.ActivationDate = DateTime.UtcNow.Date.AddDays(1))
				.With(x => x.Phones = phones).Build();
			Assert.ThrowsAsync<ArgumentException>(async () => await _supplierService.InsertSupplier(supplier));
		}

		[Test]
		public async Task InsertSupplier_ValidPhoneNumber_IsSaved()
		{
			var id = Guid.NewGuid();
			Console.WriteLine(id);

			List<Phone> phones = new List<Phone>
			{
				new Phone { Id = Guid.NewGuid(), IsPreferred = true, PhoneNumber = VALID_PHONE }
			};

			var supplier = Builder<Supplier>.CreateNew()
				.With(x => x.ActivationDate = DateTime.UtcNow.Date.AddDays(1))
				.With(x => x.Id = id)
				.With(x => x.Phones = phones).Build();
			await _supplierService.InsertSupplier(supplier);
			var matched = await _supplierService.GetSupplier(supplier.Id);
			Assert.AreEqual(id, matched.Id);
		}

		[Test]
		public void InsertSupplier_InvalidEmailAddress_ThrowsArgumentException()
		{

			List<Email> emails = new List<Email>
			{
				new Email { Id = Guid.NewGuid(), IsPreferred = true, EmailAddress = "name@" }
			};

			var supplier = Builder<Supplier>.CreateNew()
				.With(x => x.ActivationDate = DateTime.UtcNow.Date.AddDays(1))
				.With(x => x.Emails = emails).Build();
			Assert.ThrowsAsync<ArgumentException>(async () => await _supplierService.InsertSupplier(supplier));
		}
		[Test]
		public async Task InsertSupplier_ValidEmailAddress_IsSaved()
		{
			var id = Guid.NewGuid();
			Console.WriteLine(id);

			List<Email> emails = new List<Email>
			{
				new Email { Id = Guid.NewGuid(), IsPreferred = true, EmailAddress = "name@domain.com" }
			};


			var supplier = Builder<Supplier>.CreateNew()
				.With(x => x.ActivationDate = DateTime.UtcNow.Date.AddDays(1))
				.With(x => x.Id = id)
				.With(x => x.Emails = emails).Build();
			await _supplierService.InsertSupplier(supplier);
			var matched = await _supplierService.GetSupplier(supplier.Id);
			Assert.AreEqual(id, matched.Id);
		}

		#endregion

		#region DeleteSupplier
		[Test]
		public async Task DeleteSupplier_IsInactive_ReturnsSupplier()
		{
			var result = await _supplierService.DeleteSupplier(_inActiveSupplierId);
			Assert.AreEqual(_inActiveSupplierId, result.Id);
		}

		[Test]
		public void DeleteSupplier_IsActive_ThrowsException()
		{
			Assert.ThrowsAsync<Exception>(async () => await _supplierService.DeleteSupplier(_validId));
		}
		#endregion
	}
}