using Microsoft.AspNetCore.Mvc;
using ejercicioSemana3.Models;
using Microsoft.Data.SqlClient;
using System.Net;
using System.Reflection.PortableExecutable;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;

namespace ejercicioSemana3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderHistoryController : ControllerBase
    {
        private readonly string connectionString = 
            //"Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=BDp6;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False" +
             "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=BDp6Parte2;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False";

        [HttpGet]
        [Route("ordersHistory")]
        public List<OrderHistory> getOrdersHistories()
        {
            List<OrderHistory> orderHistories = new List<OrderHistory>();

            string query = "SELECT * FROM ORDERS_HISTORY";

            SqlConnection sqlConnection = new SqlConnection(connectionString);
            sqlConnection.Open();
            SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
            SqlDataReader reader = sqlCommand.ExecuteReader();

            while (reader.Read())
            {
                OrderHistory orderHistory = buildOrderHistory(reader);
                orderHistories.Add(orderHistory);
            }

            sqlConnection.Close();
            return orderHistories;
        }

        [HttpGet]
        [Route("ordersHistoryByYear")]
        public List<OrderHistory> getOrderHistoriesByYear(int year)
        {
            try
            {
                List<OrderHistory> orderHistories = new List<OrderHistory>();

                string query = "SELECT * FROM ORDERS_HISTORY WHERE ORDER_DATE LIKE @year";

                SqlConnection sqlConnection = new SqlConnection(connectionString);
                sqlConnection.Open();
                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                sqlCommand.Parameters.AddWithValue("@year", "%" + year + "%");
                SqlDataReader reader = sqlCommand.ExecuteReader();

                while (reader.Read())
                {
                    OrderHistory orderHistory = buildOrderHistory(reader);
                    orderHistories.Add(orderHistory);
                }

                sqlConnection.Close();
                return orderHistories;

            } catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpPost]
        [Route("createOrdersHistories")]
        public List<OrderHistoryDTO> createOrderHistories(List<OrderHistoryDTO> orderHistoriesToInsert)
        {
            try
            {
                List<OrderHistoryDTO> orderHistories = new List<OrderHistoryDTO>();
                string queryString = "INSERT INTO ORDERS_HISTORY (ORDER_DATE, ACTION, STATUS, SYMBOL, QUANTITY, PRICE) VALUES (GETDATE(), @action, @status, @symbol, @quantity, @price) ";
                
                SqlConnection connection = new SqlConnection(connectionString);
                connection.Open();

                foreach (OrderHistoryDTO order in orderHistoriesToInsert)
                {
                    SqlCommand command = new SqlCommand(queryString, connection);
                    command.Parameters.AddWithValue("@action", order.Action);
                    command.Parameters.AddWithValue("@status", order.Status);
                    command.Parameters.AddWithValue("@symbol", order.Symbol);
                    command.Parameters.AddWithValue("@quantity", order.Quantity);
                    command.Parameters.AddWithValue("@price", order.Price);

                    SqlDataReader reader = command.ExecuteReader();
                    orderHistories.Add(order);
                }
                connection.Close();
                return orderHistories;

            } 
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private OrderHistory buildOrderHistory(SqlDataReader reader)
        {
            OrderHistory orderHistory = new OrderHistory();
            orderHistory.TX_Number = int.Parse(reader[0].ToString());
            orderHistory.OrderDate = (DateTime)reader[1];
            orderHistory.Action = reader[2].ToString();
            orderHistory.Status = reader[3].ToString();
            orderHistory.Symbol = reader[4].ToString();
            orderHistory.Quantity = int.Parse(reader[5].ToString());
            orderHistory.Price = decimal.Parse(reader[6].ToString());
            return orderHistory;
        }
        


        private decimal unit_price(string symbol)
        {
            List<price> unit_price = new List<price>();

            string query = "select * from dbo.STOCK_MARKET_SHARES s where s.SYMBOL=@symbol";

            SqlConnection sqlConnection = new SqlConnection(connectionString);
            sqlConnection.Open();
            //string symbol1= symbol.Replace('"','¡');


            string str = symbol;
            str = string.Join("", str.Split('"', ',', '.', ';', '\''));
            Console.WriteLine(str);

            SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
            sqlCommand.Parameters.AddWithValue("@symbol",str);
          
            SqlDataReader reader = sqlCommand.ExecuteReader();
            //string Unit_prices;
             decimal precio=0;
            while (reader.Read())
            {
                price unit_prices = new price();

                unit_prices.Id = int.Parse(reader[0].ToString());
                unit_prices.Symbol = reader[1].ToString();

                unit_prices.Unit_price = decimal.Parse(reader[2].ToString());
                precio = unit_prices.Unit_price;
                //unit_price.Add(unit_prices);
            }
            

             
            //unit_prices.Unit_price = decimal.Parse(reader[0].ToString());

            //decimal prices = decimal.Parse(unit_price);
            sqlConnection.Close();
            return precio;
        }
         

        [HttpPost]
        [Route("createOrders")]
        public List<OrderHistoryDTO> createOrderParte2(List<OrderHistoryDTO> orderHistoriesToInsert)
        {
            try
            {
                List<OrderHistoryDTO> orderHistories = new List<OrderHistoryDTO>();
                string queryString = "INSERT INTO ORDERS (ORDER_DATE, ACTION, STATUS, SYMBOL, QUANTITY, PRICE) VALUES (GETDATE(), @action, @status, @symbol, @quantity, @price); " +
                    "INSERT INTO ORDERS_HISTORY (ORDER_DATE, ACTION, STATUS, SYMBOL, QUANTITY, PRICE) VALUES (GETDATE(), @action, @status, @symbol, @quantity, @price) ";
                // usar trigger

                SqlConnection connection = new SqlConnection(connectionString);
                connection.Open();
               
                foreach (OrderHistoryDTO order in orderHistoriesToInsert)
                {
                    // decimal ART = decimal.Parse( unit_price(order.Symbol).ToString());
                   
                    order.Price= unit_price(order.Symbol);
                    order.Price = order.Price * order.Quantity;
                    // Console.WriteLine(ART.ToString());
                    //decimal price =unit_price(order.Symbol);

                    if (order.Status == "FILED")
                    {

                    }
                    SqlCommand command = new SqlCommand(queryString, connection);

                    
                    command.Parameters.AddWithValue("@action", order.Action);
                    command.Parameters.AddWithValue("@status", order.Status);
                    command.Parameters.AddWithValue("@symbol", order.Symbol);
                    command.Parameters.AddWithValue("@quantity", order.Quantity);
                    command.Parameters.AddWithValue("@price", order.Price);

                    SqlDataReader reader = command.ExecuteReader();
                    orderHistories.Add(order);
                    // SI LA CUENTA DE PRICEO NO TE QUEDA EN DECIMAL FALLA
                }
                
                

               
                connection.Close();
                return orderHistories;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            
        }




        [HttpPost]
        [Route("createOrdersMejorado")]
        public List<OrderHistoryDTO> createOrderParte2Mejorado(List<OrderHistoryDTO> orderHistoriesToInsert)
        {
            try
            {
                List<OrderHistoryDTO> orderHistories = new List<OrderHistoryDTO>();
                string queryString =
                    // "INSERT INTO ORDERS (ORDER_DATE, ACTION, STATUS, SYMBOL, QUANTITY, PRICE) VALUES (GETDATE(), @action, @status, @symbol, @quantity, @price); " +
                    "INSERT INTO ORDERS_HISTORY (TX_NUMBER, ORDER_DATE, ACTION, STATUS, SYMBOL, QUANTITY, PRICE) VALUES (@id, GETDATE(), @action, @status, @symbol, @quantity, @price) "
                    ;
                // usar trigger

                SqlConnection connection = new SqlConnection(connectionString);
                connection.Open();

                foreach (OrderHistoryDTO order in orderHistoriesToInsert)
                {
                    // decimal ART = decimal.Parse( unit_price(order.Symbol).ToString());
                    string ErrorStatus = "No a ingresado un estado valido intente con FILLED, EXECUTED, CANCELLED o PENDING.";

                    order.Price = unit_price(order.Symbol);
                    order.Price = order.Price * order.Quantity;
                    // Console.WriteLine(ART.ToString());
                    //decimal price =unit_price(order.Symbol);
                    int txnumer = 0;
                    //FILLED, EXECUTED, CANCELLED o PENDING.(**)
                    if (order.Status == "FILLED")
                    {
                        txnumer = agregar(order);
                        SqlCommand command = new SqlCommand(queryString, connection);

                        command.Parameters.AddWithValue("@id", txnumer);
                        command.Parameters.AddWithValue("@action", order.Action);
                        command.Parameters.AddWithValue("@status", order.Status);
                        command.Parameters.AddWithValue("@symbol", order.Symbol);
                        command.Parameters.AddWithValue("@quantity", order.Quantity);
                        command.Parameters.AddWithValue("@price", order.Price);

                        SqlDataReader reader = command.ExecuteReader();
                        orderHistories.Add(order);

                    }
                    if (order.Status == "EXECUTED")
                    {
                        txnumer = agregar(order);
                    }
                    if (order.Status == "CANCELLED")
                    {
                        txnumer = agregar(order);
                    }
                    if (order.Status == "PENDING")
                    {
                        txnumer = agregar(order);
                    }
                    else
                    {
                        
                        //return "No a ingresado un estado valido intente con FILLED, EXECUTED, CANCELLED o PENDING";
                    }

                    
                    // SI LA CUENTA DE PRICEO NO TE QUEDA EN DECIMAL FALLA
                }




                connection.Close();
                return orderHistories;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }


        }

        private int agregar(OrderHistoryDTO order)
        {
            List<price> unit_price = new List<price>();

            string query = "INSERT INTO ORDERS (ORDER_DATE, ACTION, STATUS, SYMBOL, QUANTITY, PRICE) VALUES (GETDATE(), @action, @status, @symbol, @quantity, @price);"+
                "Select * from ORDERS WHERE ORDER_DATE=GETDATE()";

            SqlConnection sqlConnection = new SqlConnection(connectionString);
            sqlConnection.Open();
            //string symbol1= symbol.Replace('"','¡');




            SqlCommand command = new SqlCommand(query, sqlConnection);


            command.Parameters.AddWithValue("@action", order.Action);
            command.Parameters.AddWithValue("@status", order.Status);
            command.Parameters.AddWithValue("@symbol", order.Symbol);
            command.Parameters.AddWithValue("@quantity", order.Quantity);
            command.Parameters.AddWithValue("@price", order.Price);

            SqlDataReader reader = command.ExecuteReader();
           
            int txnumer= 0;
            while (reader.Read())
            {
                ordersHistory solo_id = new ordersHistory();

                solo_id.id = int.Parse(reader[0].ToString());
                txnumer = solo_id.id;
                //unit_price.Add(unit_prices);
            }



            //unit_prices.Unit_price = decimal.Parse(reader[0].ToString());

            //decimal prices = decimal.Parse(unit_price);
            sqlConnection.Close();
            return txnumer;
        }

    }
}
