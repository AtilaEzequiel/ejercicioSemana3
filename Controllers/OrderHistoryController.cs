using Microsoft.AspNetCore.Mvc;
using ejercicioSemana3.Models;
using Microsoft.Data.SqlClient;
using System.Net;

namespace ejercicioSemana3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderHistoryController : ControllerBase
    {
        private readonly string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=BDp6;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False";

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
        [Route("createOrders")]
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

            } catch (Exception ex)
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
    }
}
