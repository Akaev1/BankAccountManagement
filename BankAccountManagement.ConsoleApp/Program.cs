using System;
using BankAccountManagement.Domain;

namespace BankAccountManagement.ConsoleApp
{
	class Program
	{
		static void Main(string[] args)
		{
			var dbHandler = new DatabaseHandler();
			var menuHelper = new MenuHelper(dbHandler);

			menuHelper.MainMenu();
		}
	}
}
