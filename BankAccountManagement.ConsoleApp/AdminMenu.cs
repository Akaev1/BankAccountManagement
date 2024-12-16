// AdminMenu.cs
using System;
using BankAccountManagement.Domain;

namespace BankAccountManagement.ConsoleApp
{
	public class AdminMenu
	{
		private readonly DatabaseHandler _dbHandler;

		public AdminMenu(DatabaseHandler dbHandler)
		{
			_dbHandler = dbHandler;
		}

		public void Show()
		{
			bool adminLoggedIn = true;

			while (adminLoggedIn)
			{
				Console.Clear();
				Console.WriteLine("===== Admin Dashboard =====");
				Console.WriteLine("1. View All Bank Accounts");
				Console.WriteLine("2. Delete an IBAN Account");
				Console.WriteLine("3. Freeze/Unfreeze an IBAN Account");
				Console.WriteLine("4. Logout");
				Console.WriteLine("===========================");
				Console.Write("Choose an option: ");

				if (int.TryParse(Console.ReadLine(), out int choice))
				{
					switch (choice)
					{
						case 1:
							ViewAllBankAccounts();
							break;
						case 2:
							DeleteIBANAccount();
							break;
						case 3:
							FreezeUnfreezeIBANAccount();
							break;
						case 4:
							adminLoggedIn = false;
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

				if (adminLoggedIn)
				{
					Console.WriteLine("\nPress Enter to continue...");
					Console.ReadLine();
				}
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

			Console.WriteLine("\nPress Enter to return to the admin dashboard...");
			Console.ReadLine();
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

				if (reader.Read()) // Check if the IBAN exists
				{
					bool isFrozen = Convert.ToBoolean(reader["Frozen"]);
					string status = isFrozen ? "frozen" : "active";

					Console.WriteLine($"The IBAN {iban} is currently {status}.");

					if (isFrozen)
					{
						Console.Write("Do you wish to Unfreeze this account? (YES/NO): ");
					}
					else
					{
						Console.Write("Do you wish to Freeze this account? (YES/NO): ");
					}

					string input = Console.ReadLine()?.Trim().ToUpper();

					if (input == "YES")
					{
						bool freeze = !isFrozen; // Toggle freeze status
						_dbHandler.FreezeIBANAccount(iban, freeze);

						string action = freeze ? "frozen" : "unfrozen";
						Console.WriteLine($"IBAN {iban} has been successfully {action}.");
					}
					else if (input == "NO")
					{
						Console.WriteLine("No changes were made to the IBAN.");
					}
					else
					{
						Console.WriteLine("Invalid input. Please enter YES or NO.");
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

			Console.WriteLine("\nPress Enter to return to the menu...");
			Console.ReadLine();
		}




		private void ViewAllBankAccounts()
		{
			Console.Clear();
			Console.WriteLine("===== All Bank Accounts =====");

			try
			{
				var reader = _dbHandler.GetAllBankAccounts();
				Console.WriteLine("BankAccountId\tIBAN\t\tBalance\tAccountType\tCreatedDate");
				while (reader.Read())
				{
					Console.WriteLine($"{reader["BankAccountId"]}\t{reader["IBAN"]}\t{reader["Balance"]}\t{reader["AccountType"]}\t{reader["CreatedDate"]}");
				}
				reader.Close();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error: {ex.Message}");
			}

			Console.WriteLine("\nPress Enter to return to the Admin Dashboard...");
			Console.ReadLine();
		}

		private void AddBankAccount()
		{
			Console.Clear();
			Console.WriteLine("===== Add Bank Account =====");
			Console.Write("Enter Account Holder ID: ");
			if (int.TryParse(Console.ReadLine(), out int accountHolderId))
			{
				Console.Write("Enter IBAN: ");
				string iban = Console.ReadLine();

				Console.Write("Enter Initial Balance: ");
				if (decimal.TryParse(Console.ReadLine(), out decimal balance))
				{
					Console.Write("Enter Account Type (Savings/Current): ");
					string accountType = Console.ReadLine();

					try
					{
						_dbHandler.AddBankAccount(accountHolderId, iban, balance, accountType);
						Console.WriteLine("Bank account added successfully.");
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
			else
			{
				Console.WriteLine("Invalid Account Holder ID.");
			}

			Console.WriteLine("\nPress Enter to return to the Admin Dashboard...");
			Console.ReadLine();
		}

		private void TransferMoney()
		{
			Console.Clear();
			Console.WriteLine("===== Transfer Money =====");
			Console.Write("Enter Sender's IBAN: ");
			string fromIBAN = Console.ReadLine();

			Console.Write("Enter Recipient's IBAN: ");
			string toIBAN = Console.ReadLine();

			Console.Write("Enter Transfer Amount: ");
			if (decimal.TryParse(Console.ReadLine(), out decimal amount))
			{
				try
				{
					bool success = _dbHandler.TransferMoneyByIBAN(fromIBAN, toIBAN, amount);
					if (success)
					{
						Console.WriteLine("Transfer successful!");
					}
					else
					{
						Console.WriteLine("Transfer failed. Please check details.");
					}
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

			Console.WriteLine("\nPress Enter to return to the Admin Dashboard...");
			Console.ReadLine();
		}
	}
}