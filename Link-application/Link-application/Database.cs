using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.OleDb; // nodig voor connectie met database

namespace Link_application
{
    class Database
    {
        // Kenmerken
        private OleDbConnection connection;

        // Constructor
        public Database(string bestand)
        {
            connection = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + bestand);
        }

        // Methodes
        public List<Tuple<string, string>> SettingsOphalen()
        {
            string sqlsetting = "SELECT * FROM setting";
            OleDbCommand settingcommand = new OleDbCommand(sqlsetting, connection);
            List<Tuple<string, string>> settings = new List<Tuple<string, string>>();
            
            try
            {
                connection.Open();
                OleDbDataReader settingreader = settingcommand.ExecuteReader();

                while (settingreader.Read())
                {
                    string soortsetting = Convert.ToString(settingreader["soort"]);
                    string modus = Convert.ToString(settingreader["modus"]);
                    settings.Add(new Tuple<string, string>(soortsetting, modus));
                }
            }
            catch
            {
                return null;
            }
            finally
            {
                connection.Close();
            }
            return settings;
        }

        public List<Tuple<string, int>> ThresholdsOphalen()
        {
            string sqlthreshold = "SELECT * FROM threshold";
            OleDbCommand thresholdcommand = new OleDbCommand(sqlthreshold, connection);
            List<Tuple<string, int>> thresholds = new List<Tuple<string, int>>();

            try
            {
                connection.Open();
                OleDbDataReader thresholdreader = thresholdcommand.ExecuteReader();

                while (thresholdreader.Read())
                {
                    string soortthreshold = Convert.ToString(thresholdreader["soort"]);
                    int waarde = Convert.ToInt32(thresholdreader["waarde"]);
                    thresholds.Add(new Tuple<string, int>(soortthreshold, waarde));
                }
            }
            catch
            {
                return null;
            }
            finally
            {
                connection.Close();
            }
            return thresholds;
        }

        public List<Tuple<string, string>> MuziekOphalen()
        {
            string sqlmuziek = "SELECT * FROM muziek";
            OleDbCommand muziekcommand = new OleDbCommand(sqlmuziek, connection);
            List<Tuple<string, string>> muziek = new List<Tuple<string, string>>();

            try
            {
                connection.Open();
                OleDbDataReader muziekreader = muziekcommand.ExecuteReader();

                while (muziekreader.Read())
                {
                    string gemtoestand = Convert.ToString(muziekreader["gemtoestand"]);
                    string stijl = Convert.ToString(muziekreader["stijl"]);
                    muziek.Add(new Tuple<string, string>(gemtoestand, stijl));
                }
            }
            catch
            {
                return null;
            }
            finally
            {
                connection.Close();
            }
            return muziek;
        }

        public void UpdateSetting(string soort, string modus)
        {
            string sql = "UPDATE setting SET modus = '"+ modus +"' WHERE soort = '" + soort + "'";
            OleDbCommand command = new OleDbCommand(sql, connection);

            try
            {
                connection.Open();
                command.ExecuteNonQuery();
            }
            catch
            {
                
            }
            finally
            {
                connection.Close();
            }
        }

		public void UpdateThreshold(string soort, int waarde)
        {
            string sql = "UPDATE threshold SET waarde = '"+ Convert.ToString(waarde) +"' WHERE soort = '" + soort + "'";
            OleDbCommand command = new OleDbCommand(sql, connection);

            try
            {
                connection.Open();
                command.ExecuteNonQuery();
            }
            catch
            {
                
            }
            finally
            {
                connection.Close();
            }
        }

        public void UpdateMuziek(string gemtoestand, string stijl)
        {
            string sql = "UPDATE muziek SET stijl = '" + stijl + "' WHERE gemtoestand = '" + gemtoestand + "'";
            OleDbCommand command = new OleDbCommand(sql, connection);

            try
            {
                connection.Open();
                command.ExecuteNonQuery();
            }
            catch
            {
                
            }
            finally
            {
                connection.Close();
            }
        }
    }
}
