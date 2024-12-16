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
				Console.WriteLine("3. Transfer Money Using IBAN");
				Console.WriteLine("4. Delete an IBAN Account");
				Console.WriteLine("5. Freeze/Unfreeze an IBAN Account");
				Console.WriteLine("6. Logout");
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
							TransferMoneyUsingIBAN();
							break;
						case 4:
							DeleteIBANAccount();
							break;
						case 5:
							FreezeUnfreezeIBANAccount();
							break;
						case 6:
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
				var reader = _dbHandler.GetIBANAccountDetails(iban);
				if (reader.Read())
				{
					int ownerId = Convert.ToInt32(reader["AccountHolderId"]);
					reader.Close();

					if (ownerId != _accountId)
					{
						Console.WriteLine("You can only delete your own IBAN accounts.");
					}
					else
					{
						_dbHandler.DeleteIBANAccount(iban);
						Console.WriteLine("IBAN account deleted successfully.");
					}
				}
				else
				{
					Console.WriteLine($"The IBAN {iban} does not exist.");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error: {ex.Message}");
			}

			Console.WriteLine("Press Enter to return to the menu...");
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
				if (reader.Read())
				{
					int ownerId = Convert.ToInt32(reader["AccountHolderId"]);
					bool isFrozen = Convert.ToBoolean(reader["Frozen"]);
					string status = isFrozen ? "frozen" : "active";
					reader.Close();

					if (ownerId != _accountId)
					{
						Console.WriteLine("You can only freeze/unfreeze your own IBAN accounts.");
					}
					else
					{
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
				}
				else
				{
					Console.WriteLine($"The IBAN {iban} does not exist.");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error: {ex.Message}");
			}

			Console.WriteLine("Press Enter to return to the menu...");
			Console.ReadLine();
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
			Console.WriteLine("Press Enter to return to the menu...");
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
					if (_dbHandler.IsIBANExists(iban))
					{
						Console.WriteLine("IBAN already exists. Please choose a unique IBAN.");
					}
					else
					{
						_dbHandler.AddBankAccount(_accountId, iban, balance, accountType);
						Console.WriteLine("New IBAN account added successfully.");
					}
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

			Console.WriteLine("Press Enter to return to the menu...");
			Console.ReadLine();
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
			Console.WriteLine("Press Enter to return to the menu...");
			Console.ReadLine();
		}
	}
}
