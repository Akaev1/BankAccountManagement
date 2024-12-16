using System;
using System.Data.SQLite;
using System.Threading;

namespace BankAccountManagement.Domain
{
	public class DatabaseHandler
	{
		private const string ConnectionString = "Data Source=BankDB.sqlite;Version=3;Cache=Shared;";
		private static SQLiteConnection _sharedConnection;
		private static readonly object ConnectionLock = new object(); 

		public DatabaseHandler()
		{
			EnsureConnection();
			InitializeDatabase();
		}

		private void EnsureConnection()
		{
			lock (ConnectionLock)
			{
				if (_sharedConnection == null)
				{
					_sharedConnection = new SQLiteConnection(ConnectionString);
					_sharedConnection.Open();
				}
				else if (_sharedConnection.State != System.Data.ConnectionState.Open)
				{
					_sharedConnection.Open();
				}
			}
		}

		private void InitializeDatabase()
		{
			using (var command = _sharedConnection.CreateCommand())
			{
				command.CommandText = @"CREATE TABLE IF NOT EXISTS Accounts (
                    AccountId INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Password TEXT NOT NULL,
                    Role TEXT DEFAULT 'Customer',
                    CreatedDate TEXT NOT NULL
                );";
				command.ExecuteNonQuery();

				command.CommandText = @"CREATE TABLE IF NOT EXISTS BankAccounts (
                    BankAccountId INTEGER PRIMARY KEY AUTOINCREMENT,
                    AccountHolderId INTEGER NOT NULL,
                    IBAN TEXT UNIQUE NOT NULL,
                    Balance NUMERIC NOT NULL DEFAULT 0,
                    AccountType TEXT NOT NULL,
                    Frozen BOOLEAN DEFAULT 0,
                    CreatedDate TEXT NOT NULL,
                    FOREIGN KEY (AccountHolderId) REFERENCES Accounts(AccountId)
                );";
				command.ExecuteNonQuery();

				command.CommandText = @"CREATE TABLE IF NOT EXISTS Transactions (
                    TransactionId INTEGER PRIMARY KEY AUTOINCREMENT,
                    AccountId INTEGER NOT NULL,
                    Type TEXT NOT NULL,
                    Amount REAL NOT NULL,
                    TargetAccountId INTEGER,
                    Date TEXT NOT NULL,
                    FOREIGN KEY(AccountId) REFERENCES BankAccounts(BankAccountId)
                );";
				command.ExecuteNonQuery();
			}
		}

		public SQLiteDataReader GetIBANAccountDetails(string iban)
		{
			EnsureConnection();
			var command = _sharedConnection.CreateCommand();
			command.CommandText = "SELECT * FROM BankAccounts WHERE IBAN = @IBAN;";
			command.Parameters.AddWithValue("@IBAN", iban);
			return command.ExecuteReader(System.Data.CommandBehavior.CloseConnection);
		}

		public void ResetDatabase()
		{
			EnsureConnection();
			using (var transaction = _sharedConnection.BeginTransaction())
			{
				try
				{
					var command = _sharedConnection.CreateCommand();

					command.CommandText = "DROP TABLE IF EXISTS Accounts;";
					command.ExecuteNonQuery();

					command.CommandText = "DROP TABLE IF EXISTS BankAccounts;";
					command.ExecuteNonQuery();

					command.CommandText = "DROP TABLE IF EXISTS Transactions;";
					command.ExecuteNonQuery();

					transaction.Commit();
					InitializeDatabase();
				}
				catch (Exception ex)
				{
					transaction.Rollback();
					Console.WriteLine($"Error resetting database: {ex.Message}");
				}
			}
		}

		public void DeleteIBANAccount(string iban)
		{
			EnsureConnection();
			using (var command = _sharedConnection.CreateCommand())
			{
				command.CommandText = "DELETE FROM BankAccounts WHERE IBAN = @IBAN;";
				command.Parameters.AddWithValue("@IBAN", iban);
				command.ExecuteNonQuery();
			}
		}

		public void FreezeIBANAccount(string iban, bool freeze)
		{
			EnsureConnection();
			using (var command = _sharedConnection.CreateCommand())
			{
				command.CommandText = "UPDATE BankAccounts SET Frozen = @Frozen WHERE IBAN = @IBAN;";
				command.Parameters.AddWithValue("@Frozen", freeze);
				command.Parameters.AddWithValue("@IBAN", iban);
				command.ExecuteNonQuery();
			}
		}

		public void AddAccount(string name, string password, string role)
		{
			EnsureConnection();
			using (var command = _sharedConnection.CreateCommand())
			{
				command.CommandText = @"INSERT INTO Accounts (Name, Password, Role, CreatedDate) 
                                        VALUES (@Name, @Password, @Role, @CreatedDate);";
				command.Parameters.AddWithValue("@Name", name);
				command.Parameters.AddWithValue("@Password", password);
				command.Parameters.AddWithValue("@Role", role);
				command.Parameters.AddWithValue("@CreatedDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
				command.ExecuteNonQuery();
			}
		}

		public SQLiteDataReader ValidateLogin(string name, string password)
		{
			EnsureConnection();
			var command = _sharedConnection.CreateCommand();
			command.CommandText = "SELECT * FROM Accounts WHERE Name = @Name AND Password = @Password;";
			command.Parameters.AddWithValue("@Name", name);
			command.Parameters.AddWithValue("@Password", password);
			return command.ExecuteReader(System.Data.CommandBehavior.CloseConnection);
		}

		public void AddBankAccount(int accountHolderId, string iban, decimal balance, string accountType)
		{
			EnsureConnection();
			using (var command = _sharedConnection.CreateCommand())
			{
				command.CommandText = @"INSERT INTO BankAccounts (AccountHolderId, IBAN, Balance, AccountType, CreatedDate) 
                                        VALUES (@AccountHolderId, @IBAN, @Balance, @AccountType, @CreatedDate);";
				command.Parameters.AddWithValue("@AccountHolderId", accountHolderId);
				command.Parameters.AddWithValue("@IBAN", iban);
				command.Parameters.AddWithValue("@Balance", balance);
				command.Parameters.AddWithValue("@AccountType", accountType);
				command.Parameters.AddWithValue("@CreatedDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
				command.ExecuteNonQuery();
			}
		}

		public SQLiteDataReader GetBankAccountsByUserId(int accountHolderId)
		{
			EnsureConnection();
			var command = _sharedConnection.CreateCommand();
			command.CommandText = "SELECT * FROM BankAccounts WHERE AccountHolderId = @AccountHolderId;";
			command.Parameters.AddWithValue("@AccountHolderId", accountHolderId);
			return command.ExecuteReader(System.Data.CommandBehavior.CloseConnection);
		}

		public bool TransferMoneyByIBAN(string fromIBAN, string toIBAN, decimal amount)
		{
			using (var connection = new SQLiteConnection(ConnectionString))
			{
				connection.Open();
				using (var transaction = connection.BeginTransaction())
				{
					try
					{
						string checkSenderFrozen = "SELECT Frozen FROM BankAccounts WHERE IBAN = @IBAN;";
						using (var command = new SQLiteCommand(checkSenderFrozen, connection, transaction))
						{
							command.Parameters.AddWithValue("@IBAN", fromIBAN);
							var result = command.ExecuteScalar();
							if (result == null) throw new Exception("Sender IBAN not found.");
							if (Convert.ToBoolean(result)) throw new Exception("Sender's account is frozen.");
						}

						string checkReceiverFrozen = "SELECT Frozen FROM BankAccounts WHERE IBAN = @IBAN;";
						using (var command = new SQLiteCommand(checkReceiverFrozen, connection, transaction))
						{
							command.Parameters.AddWithValue("@IBAN", toIBAN);
							var result = command.ExecuteScalar();
							if (result == null) throw new Exception("Receiver IBAN not found.");
							if (Convert.ToBoolean(result)) throw new Exception("Receiver's account is frozen.");
						}

						string getSenderBalance = "SELECT Balance FROM BankAccounts WHERE IBAN = @IBAN;";
						decimal senderBalance;
						using (var command = new SQLiteCommand(getSenderBalance, connection, transaction))
						{
							command.Parameters.AddWithValue("@IBAN", fromIBAN);
							var result = command.ExecuteScalar();
							senderBalance = result != null ? Convert.ToDecimal(result) : throw new Exception("Sender IBAN not found.");
						}

						if (senderBalance < amount) throw new Exception("Insufficient funds.");

						string deductQuery = "UPDATE BankAccounts SET Balance = Balance - @Amount WHERE IBAN = @IBAN;";
						using (var command = new SQLiteCommand(deductQuery, connection, transaction))
						{
							command.Parameters.AddWithValue("@Amount", amount);
							command.Parameters.AddWithValue("@IBAN", fromIBAN);
							command.ExecuteNonQuery();
						}

						string addQuery = "UPDATE BankAccounts SET Balance = Balance + @Amount WHERE IBAN = @IBAN;";
						using (var command = new SQLiteCommand(addQuery, connection, transaction))
						{
							command.Parameters.AddWithValue("@Amount", amount);
							command.Parameters.AddWithValue("@IBAN", toIBAN);
							command.ExecuteNonQuery();
						}

						transaction.Commit();
						return true;
					}
					catch (Exception ex)
					{
						transaction.Rollback();
						Console.WriteLine($"Transfer failed: {ex.Message}");
						return false;
					}
				}
			}
		}

		public SQLiteDataReader GetAllBankAccounts()
		{
			EnsureConnection();
			var command = _sharedConnection.CreateCommand();
			command.CommandText = "SELECT * FROM BankAccounts;";
			return command.ExecuteReader(System.Data.CommandBehavior.CloseConnection);
		}
	}
}
