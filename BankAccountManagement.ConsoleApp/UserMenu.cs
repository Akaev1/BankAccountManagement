using System;
using BankAccountManagement.Domain;

namespace BankAccountManagement.ConsoleApp
{
	public class UserMenu
	{
		private readonly DatabaseHandler _dbHandler;
		private readonly int _accountId;

		public UserMenu(DatabaseHandler dbHandler, int accountId)
		{
			_dbHandler = dbHandler;
			_accountId = accountId;
		}

		public void Show()
		{
			bool loggedIn = true;

			while (loggedIn)
			{
				Console.Clear();
				Console.WriteLine("===== Customer Menu =====");
				Console.WriteLine("1. View All IBAN Accounts");
				Console.WriteLine("2. Add a New IBAN Account");
				Console.WriteLine("3. Deposit Money");
				Console.WriteLine("4. Withdraw Money");
				Console.WriteLine("5. Transfer Money Using IBAN");
				Console.WriteLine("6. Delete an IBAN Account");
				Console.WriteLine("7. Freeze/Unfreeze an IBAN Account");
				Console.WriteLine("8. Logout");
				Console.WriteLine("=========================");
				Console.Write("Choose an option: ");

				if (int.TryParse(Console.ReadLine(), out int choice))
				{
					switch (choice)
					{
						case 1:
							ViewAllIBANAccounts();
							break;
						case 2:
							AddNewIBANAccount();
							break;
						case 3:
							DepositMoney();
							break;
						case 4:
							WithdrawMoney();
							break;
						case 5:
							TransferMoneyUsingIBAN();
							break;
						case 6:
							DeleteIBANAccount();
							break;
						case 7:
							FreezeUnfreezeIBANAccount();
							break;
						case 8:
							loggedIn = false;
							break;
						default:
							Console.WriteLine("Invalid choice. Please try again.");
							break;
					}
				}
				else
				{
					Console.WriteLine("Invalid input. Please enter a number.");
				}

				if (loggedIn)
				{
					Console.WriteLine("\nPress Enter to continue...");
					Console.ReadLine();
				}
			}
		}

		private void DepositMoney()
		{
			Console.Clear();
			Console.WriteLine("===== Deposit Money =====");
			Console.Write("Enter your IBAN: ");
			string iban = Console.ReadLine();

			Console.Write("Enter the amount to deposit: ");
			if (decimal.TryParse(Console.ReadLine(), out decimal amount) && amount > 0)
			{
				try
				{
					_dbHandler.UpdateBalance(iban, amount, true); // true for deposit
					Console.WriteLine($"Successfully deposited {amount} to IBAN {iban}.");
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error: {ex.Message}");
				}
			}
			else
			{
				Console.WriteLine("Invalid amount. Please enter a positive number.");
			}
		}

		private void WithdrawMoney()
		{
			Console.Clear();
			Console.WriteLine("===== Withdraw Money =====");
			Console.Write("Enter your IBAN: ");
			string iban = Console.ReadLine();

			Console.Write("Enter the amount to withdraw: ");
			if (decimal.TryParse(Console.ReadLine(), out decimal amount) && amount > 0)
			{
				try
				{
					_dbHandler.UpdateBalance(iban, amount, false); // false for withdraw
					Console.WriteLine($"Successfully withdrew {amount} from IBAN {iban}.");
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error: {ex.Message}");
				}
			}
			else
			{
				Console.WriteLine("Invalid amount. Please enter a positive number.");
			}
		}

		private void ViewAllIBANAccounts()
		{
			Console.Clear();
			Console.WriteLine("===== Your IBAN Accounts =====");
			try
			{
				var reader = _dbHandler.GetBankAccountsByUserId(_accountId);
				Console.WriteLine("IBAN\t\tBalance\tAccount Type");
				while (reader.Read())
				{
					Console.WriteLine($"{reader["IBAN"]}\t{reader["Balance"]}\t{reader["AccountType"]}");
				}
				reader.Close();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error: {ex.Message}");
			}
			Console.WriteLine("\nPress Enter to return to the menu...");
			Console.ReadLine();
		}

		private void AddNewIBANAccount()
		{
			Console.Clear();
			Console.WriteLine("===== Add New IBAN Account =====");
			Console.Write("Enter IBAN: ");
			string iban = Console.ReadLine();

			Console.Write("Enter Initial Balance: ");
			if (decimal.TryParse(Console.ReadLine(), out decimal balance))
			{
				Console.Write("Enter Account Type (Savings/Current): ");
				string accountType = Console.ReadLine();

				try
				{
					_dbHandler.AddBankAccount(_accountId, iban, balance, accountType);
					Console.WriteLine("New IBAN account added successfully.");
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error: {ex.Message}");
				}
			}
			else
			{
				Console.WriteLine("Invalid balance. Please try again.");
			}
		}

		private void TransferMoneyUsingIBAN()
		{
			Console.Clear();
			Console.WriteLine("===== Transfer Money Using IBAN =====");
			Console.Write("Enter Your IBAN: ");
			string fromIBAN = Console.ReadLine();

			Console.Write("Enter Recipient's IBAN: ");
			string toIBAN = Console.ReadLine();

			Console.Write("Enter Transfer Amount: ");
			if (decimal.TryParse(Console.ReadLine(), out decimal amount))
			{
				try
				{
					bool success = _dbHandler.TransferMoneyByIBAN(fromIBAN, toIBAN, amount);
					Console.WriteLine(success ? "Transfer successful!" : "Transfer failed. Please check details.");
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error: {ex.Message}");
				}
			}
			else
			{
				Console.WriteLine("Invalid amount. Please try again.");
			}
		}

		private void FreezeUnfreezeIBANAccount()
		{
			Console.Clear();
			Console.WriteLine("===== Freeze/Unfreeze an IBAN Account =====");
			Console.Write("Enter the IBAN to check: ");
			string iban = Console.ReadLine();

			try
			{
				var reader = _dbHandler.GetIBANAccountDetails(iban);

				if (reader.Read())
				{
					bool isFrozen = Convert.ToBoolean(reader["Frozen"]);
					string status = isFrozen ? "frozen" : "active";

					Console.WriteLine($"The IBAN {iban} is currently {status}.");

					Console.Write("Do you wish to toggle this account's status? (YES/NO): ");
					string input = Console.ReadLine()?.Trim().ToUpper();

					if (input == "YES")
					{
						_dbHandler.FreezeIBANAccount(iban, !isFrozen);
						Console.WriteLine($"IBAN {iban} status has been successfully updated.");
					}
					else
					{
						Console.WriteLine("No changes were made.");
					}
				}
				else
				{
					Console.WriteLine($"The IBAN {iban} does not exist.");
				}

				reader.Close();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error: {ex.Message}");
			}
		}

		private void DeleteIBANAccount()
		{
			Console.Clear();
			Console.WriteLine("===== Delete an IBAN Account =====");
			Console.Write("Enter the IBAN to delete: ");
			string iban = Console.ReadLine();

			try
			{
				_dbHandler.DeleteIBANAccount(iban);
				Console.WriteLine("IBAN account deleted successfully.");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error: {ex.Message}");
			}
		}
	}
}
