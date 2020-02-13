using System;
using System.Data.OleDb;
using System.Collections.Generic;

namespace ConsoleApp1
{
    static class Database
    {
        static public int odbc_write(string strStatus, string strQuery, string strProvider, string strSource, int nTimeout)
        {
            // Change the username, password and database according to your needs
            // You can ignore the database option if you want to access all of them.
            // 127.0.0.1 stands for localhost and the default port to connect.
            //Dim strConnection As String = "Provider=" & strProvider & ";" & "Data Source=" & strDataSource & ";"
            //string connectionString = "datasource=127.0.0.1;port=3306;username=root;password=;database=test;";
            string connectionString = $"Provider={strProvider}; Data Source={strSource};";
            // Your query,

            // Prepare the connection
            OleDbConnection databaseConnection = new OleDbConnection(connectionString);

            // Let's do it !
            try
            {
                // Open the database
                databaseConnection.Open();

                using (OleDbCommand cmd = new OleDbCommand(strQuery, databaseConnection))
                {
                    // Execute the query
                    cmd.ExecuteNonQuery();
                }

                // Finally close the connection
                databaseConnection.Close();

                return 1;
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
                // Show any error message.
                //MessageBox.Show(ex.Message);
                return -1;
            }
        }

        static public int odbc_read(string strStatus, string strQuery, string strProvider, string strSource, int nTimeout, ref Dictionary<int, Dictionary<int, string>> param)
        {
            int nNumRecords = 0;
            // Change the username, password and database according to your needs
            // You can ignore the database option if you want to access all of them.
            // 127.0.0.1 stands for localhost and the default port to connect.
            //Dim strConnection As String = "Provider=" & strProvider & ";" & "Data Source=" & strDataSource & ";"
            //string connectionString = "datasource=127.0.0.1;port=3306;username=root;password=;database=test;";
            string connectionString = $"Provider={strProvider}; Data Source={strSource};";
            // Your query,

            // Prepare the connection
            OleDbConnection databaseConnection = new OleDbConnection(connectionString);
            OleDbCommand commandDatabase = new OleDbCommand(strQuery, databaseConnection);
            commandDatabase.CommandTimeout = nTimeout;
            OleDbDataReader reader;

            // Let's do it !
            try
            {
                // Open the database
                databaseConnection.Open();

                // Execute the query
                reader = commandDatabase.ExecuteReader();

                if (reader.HasRows)
                {

                    while (reader.Read())
                    {
                        Dictionary<int, string> rec = new Dictionary<int, string>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            rec.Add(i, reader[i].ToString());
                        }

                        param.Add(nNumRecords++, rec);
                    }
                }
                else
                {

                }

                reader.Close();
            }
            catch (Exception ex)
            {
                // Show any error message.
                //MessageBox.Show(ex.Message);
                strStatus = ex.Message;
                throw;
            }
            finally
            {

                // Finally close the connection
                databaseConnection.Close();
            }

            return nNumRecords;
        }

        static public int odbc_read(string strStatus, string strQuery, string strProvider, string strSource, int nTimeout)
        {
            int nNumRecords = 0;
            // Change the username, password and database according to your needs
            // You can ignore the database option if you want to access all of them.
            // 127.0.0.1 stands for localhost and the default port to connect.
            //Dim strConnection As String = "Provider=" & strProvider & ";" & "Data Source=" & strDataSource & ";"
            //string connectionString = "datasource=127.0.0.1;port=3306;username=root;password=;database=test;";
            string connectionString = $"Provider={strProvider}; Data Source={strSource};";
            // Your query,

            // Prepare the connection
            OleDbConnection databaseConnection = new OleDbConnection(connectionString);
            OleDbCommand commandDatabase = new OleDbCommand(strQuery, databaseConnection);
            commandDatabase.CommandTimeout = nTimeout;
            OleDbDataReader reader;

            // Let's do it !
            try
            {
                // Open the database
                databaseConnection.Open();

                // Execute the query
                reader = commandDatabase.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            nNumRecords++;
                        }
                    }
                }
                else
                {

                }

                reader.Close();
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
                throw;
            }
            finally
            {
                databaseConnection.Close();
            }

            return nNumRecords;
        }

        //static public  void sql_write(string strQuery, string strDataSource, string strPort, string strUserName, string strPassword, string strDatabase, int nTimeout)
        //{
        //    // Change the username, password and database according to your needs
        //    // You can ignore the database option if you want to access all of them.
        //    // 127.0.0.1 stands for localhost and the default port to connect.
        //    string connectionString = $"datasource={strDataSource};port={strPort};username={strUserName};password={strPassword};database={strDatabase};";
        //    // Your query,
        //    //string query = "INSERT INTO `data`(`Status`, `Temp`, `Index`) VALUES (\"" + strStatus + "\",\"" + strTemp + "\"," + Convert.ToString(nIndex) + ");";

        //    // Prepare the connection
        //    MySqlConnection databaseConnection = new MySqlConnection(connectionString);
        //    MySqlCommand commandDatabase = new MySqlCommand(strQuery, databaseConnection);
        //    commandDatabase.CommandTimeout = nTimeout;

        //    // Let's do it !
        //    try
        //    {
        //        // Open the database
        //        databaseConnection.Open();

        //        // Execute the query
        //        commandDatabase.ExecuteNonQuery();

        //        // Finally close the connection
        //        databaseConnection.Close();
        //    }
        //    catch (Exception ex)
        //    {
        //        // Show any error message.
        //        MessageBox.Show(ex.Message);
        //    }
        //}

        //static public void sql_read(string strQuery, string strDataSource, string strPort, string strUserName, string strPassword, string strDatabase, int nTimeout, ref string strOut)
        //{
        //    // Change the username, password and database according to your needs
        //    // You can ignore the database option if you want to access all of them.
        //    // 127.0.0.1 stands for localhost and the default port to connect.
        //    string connectionString = $"datasource={strDataSource};port={strPort};username={strUserName};password={strPassword};database={strDatabase};";
        //    // Your query,
        //    //string query = "SELECT * FROM data";

        //    // Prepare the connection
        //    MySqlConnection databaseConnection = new MySqlConnection(connectionString);
        //    MySqlCommand commandDatabase = new MySqlCommand(strQuery, databaseConnection);
        //    commandDatabase.CommandTimeout = nTimeout;
        //    MySqlDataReader reader;

        //    // Let's do it !
        //    try
        //    {
        //        // Open the database
        //        databaseConnection.Open();

        //        // Execute the query
        //        reader = commandDatabase.ExecuteReader();


        //        // All succesfully executed, now do something

        //        // IMPORTANT : 
        //        // If your query returns result, use the following processor :

        //        if (reader.HasRows)
        //        {
        //            while (reader.Read())
        //            {
        //                // As our database, the array will contain : ID 0, FIRST_NAME 1,LAST_NAME 2, ADDRESS 3
        //                // Do something with every received database ROW
        //                string[] row = { reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetString(3) };

        //                foreach (string r in row)
        //                {

        //                }

        //            }
        //        }
        //        else
        //        {
        //            //Console.WriteLine("No rows found.");

        //        }

        //        reader.Close();
        //        // Finally close the connection
        //        databaseConnection.Close();
        //    }
        //    catch (Exception ex)
        //    {
        //        // Show any error message.
        //        MessageBox.Show(ex.Message);
        //    }
        //}
    }
}
