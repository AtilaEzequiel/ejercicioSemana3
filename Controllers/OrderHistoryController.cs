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
                // CREOQ UE YA NO HACE FALTA ABRIR LA CONEXIO CON L ABASE DE DATOS
                List<OrderHistoryDTO> orderHistories = new List<OrderHistoryDTO>();
                string queryString =
                 // "INSERT INTO ORDERS (ORDER_DATE, ACTION, STATUS, SYMBOL, QUANTITY, PRICE) VALUES (GETDATE(), @action, @status, @symbol, @quantity, @price); " +
                 //   "INSERT INTO ORDERS_HISTORY (TX_NUMBER, ORDER_DATE, ACTION, STATUS, SYMBOL, QUANTITY, PRICE) VALUES (@id, GETDATE(), @action, @status, @symbol, @quantity, @price) "+
                 "select * from ORDERS";
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
                        txnumer = agregarORDER(order);
                        txnumer = agregarORDER_HISTORY(order, txnumer);
                        SqlCommand command = new SqlCommand(queryString, connection);
                        command.Parameters.AddWithValue("@id", txnumer);
                        orderHistories.Add(order);


                    }
                    if (order.Status == "EXECUTED")
                    {
                        txnumer = agregarORDER(order);
                      
                        txnumer = agregarORDER_HISTORY(order, txnumer);
                        SqlCommand command = new SqlCommand(queryString, connection);
                        command.Parameters.AddWithValue("@id", txnumer);
                        orderHistories.Add(order); ;
                    }
                    if (order.Status == "CANCELLED")
                    {
                        txnumer = agregarORDER(order);
                        
                        txnumer = agregarORDER_HISTORY(order, txnumer);
                        SqlCommand command = new SqlCommand(queryString, connection);
                        command.Parameters.AddWithValue("@id", txnumer);
                        orderHistories.Add(order);
                    }
                    if (order.Status == "PENDING")
                    {
                        
                        txnumer = agregarORDER(order);
                        txnumer = agregarORDER_HISTORY(order, txnumer);
                        SqlCommand command = new SqlCommand(queryString, connection);
                        command.Parameters.AddWithValue("@id", txnumer);
                        orderHistories.Add(order);
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

        private int agregarORDER(OrderHistoryDTO order)
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

        private int agregarORDER_HISTORY(OrderHistoryDTO order, int id)
        {
            List<price> unit_price = new List<price>();

            string query = "INSERT INTO ORDERS_HISTORY (TX_NUMBER, ORDER_DATE, ACTION, STATUS, SYMBOL, QUANTITY, PRICE) VALUES (@id, GETDATE(), @action, @status, @symbol, @quantity, @price) "
                + "select TX_NUMBER from ORDERS_HISTORY WHERE TX_NUMBER=@id";

            SqlConnection sqlConnection = new SqlConnection(connectionString);
            sqlConnection.Open();
            //string symbol1= symbol.Replace('"','¡');




            SqlCommand command = new SqlCommand(query, sqlConnection);

            command.Parameters.AddWithValue("@id", id);            
            command.Parameters.AddWithValue("@action", order.Action);
            command.Parameters.AddWithValue("@status", order.Status);
            command.Parameters.AddWithValue("@symbol", order.Symbol);
            command.Parameters.AddWithValue("@quantity", order.Quantity);
            command.Parameters.AddWithValue("@price", order.Price);

            SqlDataReader reader = command.ExecuteReader();

            int txnumer = 0;
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

        private int ModificarORDER_HISTORY(OrderHistory orderHistory, string status)
        {
            List<price> unit_price = new List<price>();
            List<OrderHistory> orderHistories = new List<OrderHistory>();

            string query = "update ORDERS set STATUS=@status  WHERE  TX_NUMBER=@id "+" INSERT INTO ORDERS_HISTORY (TX_NUMBER, ORDER_DATE, ACTION, STATUS, SYMBOL, QUANTITY, PRICE) VALUES (@id, GETDATE(), @action, @status, @symbol, @quantity, @price) "
                 ;

            SqlConnection sqlConnection = new SqlConnection(connectionString);
            sqlConnection.Open();
            //string symbol1= symbol.Replace('"','¡');


           

            SqlCommand command = new SqlCommand(query, sqlConnection);
         //   SqlDataReader reader = command.ExecuteReader();
            command.Parameters.AddWithValue("@id", orderHistory.TX_Number);
            command.Parameters.AddWithValue("@action", orderHistory.Action);
            command.Parameters.AddWithValue("@status", status);
            command.Parameters.AddWithValue("@symbol", orderHistory.Symbol);
            command.Parameters.AddWithValue("@quantity", orderHistory.Quantity);
            command.Parameters.AddWithValue("@price", orderHistory.Price);

            command.ExecuteReader();
            //     SqlDataReader reader = command.ExecuteReader();

            int txnumer = 0;
            sqlConnection.Close();

            return txnumer;


            //unit_prices.Unit_price = decimal.Parse(reader[0].ToString());

            //decimal prices = decimal.Parse(unit_price);
         
        }



        [HttpPost]
        [Route("ModificacionOrdersMejorado")]
        public List<OrderHistory> ModificacionOrderParte2Mejorado(List<ModificaOrders> ordermodif)
        {
            try
            {
               

             


                List<OrderHistory> orderHistories = new List<OrderHistory>();

                string query = "Select * from ORDERS WHERE  TX_NUMBER=@id";

                SqlConnection sqlConnection = new SqlConnection(connectionString);
                sqlConnection.Open();
                
                foreach (ModificaOrders order in ordermodif)
                {
                    SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                    sqlCommand.Parameters.AddWithValue("@id",order.TX_Number );
                    //  sqlCommand.Parameters.AddWithValue("@year", "%" + year + "%");
                    SqlDataReader reader = sqlCommand.ExecuteReader();
                    //    FILLED, EXECUTED, CANCELLED o PENDING.(**)
                    if (order.Status=="FILLED" || order.Status== "CANCELLED" || order.Status == "EXECUTED" || order.Status == "PENDING")
                    {
                        while (reader.Read())
                        {
                            OrderHistory orderHistory = buildOrderHistory1(reader, order.Status);
                            ModificarORDER_HISTORY(orderHistory, order.Status);
                            orderHistories.Add(orderHistory);
                        }
                    }
                }

                


                sqlConnection.Close();
                return orderHistories;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }


        }


        private OrderHistory buildOrderHistory1(SqlDataReader reader, string STATUS)
        {
            OrderHistory orderHistory = new OrderHistory();
            orderHistory.TX_Number = int.Parse(reader[0].ToString());
            orderHistory.OrderDate = (DateTime)reader[1];
            orderHistory.Action = reader[2].ToString();
            orderHistory.Status = STATUS;
            orderHistory.Symbol = reader[4].ToString();
            orderHistory.Quantity = int.Parse(reader[5].ToString());
            orderHistory.Price = decimal.Parse(reader[6].ToString());
            return orderHistory;
        }

        /*

        [HttpPost]
        [Route("IngresoDeOrden")]
        public dynamic IngresoDeOrden(string action, string symbol, int quantity, decimal price)
        {
            if (action == "SELL" || action == "BUY")
            {
                SqlConnection connection = new SqlConnection(connectionString);
                //abre la coencta
                connection.Open();
                // guarda en un string el codigo sql a ejecutar
                //string queryString = "Select * from Carrera";

                string queryString = "INSERT INTO ORDERS_HISTORY (ORDER_DATE, ACTION, STATUS, SYMBOL, QUANTITY, PRICE) VALUES (GETDATE(), @action, 'PENDING', @symbol, @quantity, @price) ";
                //  string queryString = "INSERT INTO ORDERS_HISTORY (ORDER_DATE, ACTION, STATUS, SYMBOL, QUANTITY, PRICE) VALUES ('@GETDATE', '@action', 'PENDING', '@symbol', @quantity, @price) SELECT * FROM ORDERS_HISTORY WHERE ORDER_DATE= GETDATE() AND STATUS='PENDING' AND QUANTITY=@quantity AND PRICE=@price AND ACTION='@action' AND SYMBOL='@symbol'";

                // no me acurdo, creoq ue guarda en un comadno sql lo que debe ejecutar y en que conexion hacerlo
                SqlCommand command = new SqlCommand(queryString, connection);
                //  command.ExecuteReader(queryString);
                //string buy = "BUY";
                command.Parameters.AddWithValue("@action", action);
                // command.Parameters.AddWithValue("@GETDATE", "GETDATE()");
                command.Parameters.AddWithValue("@symbol", symbol);
                command.Parameters.AddWithValue("@quantity", quantity);
                command.Parameters.AddWithValue("@price", price);

                //ejecuta el codigo sql y guarda en reader

                // SqlDataReader reader = command.ExecuteReader();


                SqlDataReader reader = command.ExecuteReader();

                string respuestas = "Se CARGO LA ORDEN";
                //cierrra la conexion
                connection.Close();
                //muestra la lista de resutltado
                return (respuestas);
            }
            else
            {
                return "ingresar en action SELL o BUY";
            }
        }
        */
    }
}
