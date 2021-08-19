using IG.TechnicalInterview.Data.Context;
using IG.TechnicalInterview.Model.Extensions;
using IG.TechnicalInterview.Model.Supplier;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Text.RegularExpressions;

namespace IG.TechnicalInterview.Domain
{
	public class SupplierService : ISupplierService
	{
		private readonly SupplierContext _context;

		public SupplierService(SupplierContext context)
		{
			_context = context;
		}

		public async Task<Supplier> GetSupplier(Guid id)
		{
			var supplier = await _context.Suppliers
				.Include(x => x.Emails)
				.Include(x => x.Phones)
				.FirstOrDefaultAsync(x => x.Id == id);

			return supplier;
		}

		public async Task<List<Supplier>> GetSuppliers()
		{
			return await _context.Suppliers
				.Include(x => x.Emails)
				.Include(x => x.Phones)
				.ToListAsync();
		}

		public async Task InsertSupplier(Supplier supplier)
		{
			var isValid = ValidateSupplier(supplier);

			if (isValid)
			{
				_context.Suppliers.Add(supplier);
				await _context.SaveChangesAsync();
			}
		}

		private static bool ValidateSupplier(Supplier supplier)
		{
			// Validate supplier
			// ActivationDate
			if (supplier.ActivationDate.Date < DateTime.UtcNow.Date.AddDays(1))
			{
				throw new ArgumentException("ActivationDate", "Activation Date must be tomorrow or later");
			}

			// Email
			var emailRegex = @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z";
			var invalidEmails = supplier.Emails.Where(x => !Regex.IsMatch(x.EmailAddress, emailRegex,RegexOptions.IgnoreCase));
			if (invalidEmails.Any())
			{
				var emails = string.Join(", ", invalidEmails.Select(x => x.EmailAddress).ToArray());
				var errorText = $"There are {invalidEmails.Count()} invalid email addresses.  The following email addresses much be in the correct format: {emails}";

				throw new ArgumentException("EmailAddress", errorText);
			}

			// Phone
			var phoneRegEx = @"^[0-9]{0,10}$"; // $"//"/^[0-9]{0,10}$/gm";
			var invalidPhoneNumbers = supplier.Phones.Where(x=> !Regex.IsMatch(x.PhoneNumber,phoneRegEx));

			if (invalidPhoneNumbers.Any())
			{
				var phoneNumbers = string.Join(", ", invalidPhoneNumbers.Select(x => x.PhoneNumber).ToArray());
				var errorText = $"There are {invalidPhoneNumbers.Count()} invalid phone numbers.  The following phone numbers must by numeric and no more than 10 digits : {phoneNumbers}";

				throw new ArgumentException("PhoneNumber", errorText);
			}


			return true;
		}

		public async Task<Supplier> DeleteSupplier(Guid id)
		{
			var supplier = await _context.Suppliers.FindAsync(id);
			if (supplier != null)
			{
				if (supplier.IsActive())
				{
					throw new Exception($"Supplier {id} is active, can't be deleted");
				}

				_context.Suppliers.Remove(supplier);
			}

			return supplier;
		}
	}
}
