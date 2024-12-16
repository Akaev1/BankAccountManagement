using System;
using BankAccountManagement.Domain;

namespace BankAccountManagement.ConsoleApp
{
	public class MenuHelper
	{
		private readonly DatabaseHandler _dbHandler;

		public MenuHelper(DatabaseHandler dbHandler)
		{
			_dbHandler = dbHandler;
		}

		public void MainMenu()
		{
			bool running = true;

			while (running)
			{
				Console.Clear();
				Console.WriteLine("===== Welcome to Bank Account Management System =====");
				Console.WriteLine("1. Login");
				Console.WriteLine("2. Create Account");
				Console.WriteLine("3. Reset Database");
				Console.WriteLine("4. Exit");
				Console.WriteLine("======================================================");
				Console.Write("Choose an option: ");

				if (int.TryParse(Console.ReadLine(), out int choice))
				{
					switch (choice)
					{
						case 1:
							LoginMenu();
							break;
						case 2:
							CreateAccountMenu();
							break;
						case 3:
							ResetDatabaseMenu();
							break;
						case 4:
							running = false;
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

				if (running)
				{
					Console.WriteLine("\nPress Enter to continue...");
					Console.ReadLine();
				}
			}
		}

		private void LoginMenu()
		{
			Console.Clear();
			Console.WriteLine("===== Login =====");
			Console.Write("Enter your Name: ");
			string name = Console.ReadLine();

			Console.Write("Enter your Password: ");
			string password = Console.ReadLine();

			if (name == "admin" && password == "admin123")
			{
				Console.WriteLine("Welcome, Admin!");
				AdminMenu adminMenu = new AdminMenu(_dbHandler);
				adminMenu.Show();
				return;
			}

			try
			{
				var reader = _dbHandler.ValidateLogin(name, password);
				if (reader.Read())
				{
					string role = reader["Role"].ToString();
					int accountId = Convert.ToInt32(reader["AccountId"]);
					Console.WriteLine($"Welcome, {name}!");
					reader.Close();

					if (role == "Customer")
					{
						UserMenu userMenu = new UserMenu(_dbHandler, accountId);
						userMenu.Show();
					}
					else
					{
						Console.WriteLine("Invalid role. Please contact support.");
					}
				}
				else
				{
					Console.WriteLine("Invalid credentials. Please try again.");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error: {ex.Message}");
			}

			Console.WriteLine("\nPress Enter to return to the main menu...");
			Console.ReadLine();
		}

		private void CreateAccountMenu()
		{
			Console.Clear();
			Console.WriteLine("===== Create a New Account =====");
			Console.Write("Enter Account Holder's Name: ");
			string name = Console.ReadLine();

			Console.Write("Enter a Password: ");
			string password = Console.ReadLine();

			Console.Write("Enter Role (default is 'Customer'): ");
			string role = Console.ReadLine();
			role = string.IsNullOrWhiteSpace(role) ? "Customer" : role;

			try
			{
				_dbHandler.AddAccount(name, password, role);
				Console.WriteLine("Account created successfully!");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error: {ex.Message}");
			}

			Console.WriteLine("\nPress Enter to return to the main menu...");
			Console.ReadLine();
		}

		private void ResetDatabaseMenu()
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

			Console.WriteLine("\nPress Enter to return to the main menu...");
			Console.ReadLine();
		}
	}
}
