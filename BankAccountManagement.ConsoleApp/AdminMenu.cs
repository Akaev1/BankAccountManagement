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
				Console.WriteLine("4. Reset Database");
				Console.WriteLine("5. Logout");
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
							ResetDatabase();
							break;
						case 5:
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
					Console.WriteLine("Press Enter to continue...");
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
						bool freeze = !isFrozen;
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
		}

		private void ResetDatabase()
		{
			Console.Clear();
			Console.WriteLine("===== Reset Database =====");
			Console.WriteLine("WARNING: This will delete all data and recreate the database.");
			Console.Write("Type 'CONFIRM' to proceed: ");
			string input = Console.ReadLine();

			if (input?.ToUpper() == "CONFIRM")
			{
				try
				{
					_dbHandler.ResetDatabase();
					Console.WriteLine("Database reset successfully.");
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error resetting database: {ex.Message}");
				}
			}
			else
			{
				Console.WriteLine("Database reset cancelled.");
			}
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
		}
	}
}
