using System;
using System.Data.SQLite;
using NUnit.Framework;
using BankAccountManagement.Domain;

namespace BankAccountManagement.Tests
{
	[TestFixture]
	public class DatabaseHandlerTests
	{
		private DatabaseHandler _dbHandler;

		[SetUp]
		public void Setup()
		{
			_dbHandler = new DatabaseHandler();
			_dbHandler.ResetDatabase(); // Ensure a clean state before each test
		}

		[Test]
		public void AddAccount_ShouldAddAccountSuccessfully()
		{
			// Act
			Assert.DoesNotThrow(() => _dbHandler.AddAccount("TestUser", "TestPassword", "Customer"));
		}

		[Test]
		public void AddBankAccount_ShouldAddBankAccountSuccessfully()
		{
			// Arrange
			_dbHandler.AddAccount("TestUser", "TestPassword", "Customer");
			var reader = _dbHandler.ValidateLogin("TestUser", "TestPassword");
			reader.Read();
			var accountId = Convert.ToInt32(reader["AccountId"]);
			reader.Close();

			// Act
			Assert.DoesNotThrow(() => _dbHandler.AddBankAccount(accountId, "TESTIBAN123", 1000, "Savings"));
		}

		[Test]
		public void GetBankAccountsByUserId_ShouldReturnCorrectAccount()
		{
			// Arrange
			_dbHandler.AddAccount("TestUser", "TestPassword", "Customer");
			var reader = _dbHandler.ValidateLogin("TestUser", "TestPassword");
			reader.Read();
			var accountId = Convert.ToInt32(reader["AccountId"]);
			reader.Close();
			_dbHandler.AddBankAccount(accountId, "TESTIBAN123", 1000, "Savings");

			// Act
			var accountsReader = _dbHandler.GetBankAccountsByUserId(accountId);

			// Assert
			Assert.IsTrue(accountsReader.Read());
			Assert.AreEqual("TESTIBAN123", accountsReader["IBAN"]);
			accountsReader.Close();
		}

		[Test]
		public void TransferMoneyByIBAN_ShouldTransferSuccessfully()
		{
			// Arrange
			_dbHandler.AddAccount("User1", "Pass1", "Customer");
			_dbHandler.AddAccount("User2", "Pass2", "Customer");

			var reader1 = _dbHandler.ValidateLogin("User1", "Pass1");
			reader1.Read();
			var accountId1 = Convert.ToInt32(reader1["AccountId"]);
			reader1.Close();

			var reader2 = _dbHandler.ValidateLogin("User2", "Pass2");
			reader2.Read();
			var accountId2 = Convert.ToInt32(reader2["AccountId"]);
			reader2.Close();

			_dbHandler.AddBankAccount(accountId1, "IBAN1", 5000, "Savings");
			_dbHandler.AddBankAccount(accountId2, "IBAN2", 1000, "Savings");

			// Act
			var result = _dbHandler.TransferMoneyByIBAN("IBAN1", "IBAN2", 500);

			// Assert
			Assert.IsTrue(result);

			// Verify balances
			var account1Reader = _dbHandler.GetIBANAccountDetails("IBAN1");
			account1Reader.Read();
			Assert.AreEqual(4500, Convert.ToDecimal(account1Reader["Balance"]));
			account1Reader.Close();

			var account2Reader = _dbHandler.GetIBANAccountDetails("IBAN2");
			account2Reader.Read();
			Assert.AreEqual(1500, Convert.ToDecimal(account2Reader["Balance"]));
			account2Reader.Close();
		}

		[Test]
		public void FreezeIBANAccount_ShouldPreventTransfers()
		{
			// Arrange
			_dbHandler.AddAccount("User1", "Pass1", "Customer");
			_dbHandler.AddAccount("User2", "Pass2", "Customer");

			var reader1 = _dbHandler.ValidateLogin("User1", "Pass1");
			reader1.Read();
			var accountId1 = Convert.ToInt32(reader1["AccountId"]);
			reader1.Close();

			var reader2 = _dbHandler.ValidateLogin("User2", "Pass2");
			reader2.Read();
			var accountId2 = Convert.ToInt32(reader2["AccountId"]);
			reader2.Close();

			_dbHandler.AddBankAccount(accountId1, "IBAN1", 5000, "Savings");
			_dbHandler.AddBankAccount(accountId2, "IBAN2", 1000, "Savings");

			_dbHandler.FreezeIBANAccount("IBAN1", true);

			// Act
			var exception = Assert.Throws<Exception>(() => _dbHandler.TransferMoneyByIBAN("IBAN1", "IBAN2", 500));

			// Assert
			Assert.AreEqual("Sender's account is frozen.", exception.Message);
		}
	}
}
