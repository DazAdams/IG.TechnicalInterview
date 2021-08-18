using IG.TechnicalInterview.Domain;
using Moq;
using NUnit.Framework;

namespace IG.TechnicalInterview.Test
{
	public class SupplierServiceTests
	{
		internal Mock<ISupplierService> _supplierService;

		[SetUp]
		public void Setup()
		{
			_supplierService = new Mock<ISupplierService>();
		}

		[Test]
		public void Test1()
		{
			Assert.Pass();
		}
	}
}