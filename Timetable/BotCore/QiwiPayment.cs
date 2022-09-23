using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text;
using Timetable.BotCore.Abstractions;
using Timetable.Helpers;
using Timetable.Models.Payment;

namespace Timetable.BotCore
{
    public class QiwiPayment : IQiwiPayment, IDisposable
    {
        /// <summary>
        /// Приватный ключ созданный в https://p2p.qiwi.com
        /// </summary>
        public static string Secret { get; set; }

        /// <summary>
        /// Номер qiwi кошелька
        /// </summary>
        public static string Number { get; set; }

        /// <summary>
        /// Код персонализации формы
        /// </summary>
        public static string ThemeCode { get; set; }

        /// <summary>
        /// Oauth токен для просмотра баланса кошелька (актуален до 07.22)
        /// </summary>
        public static string OauthToken { get; set; }

        /// <summary>
        /// Клиент для работы с запросами
        /// </summary>
        private HttpClient client { get; set; }


        public QiwiPayment()
        {
            client = new HttpClient();
        }

        public async Task<PaymentStatus> CheckPayment(string billId)
        {
            using (var req = new HttpRequestMessage(HttpMethod.Get, $"https://api.qiwi.com/partner/bill/v1/bills/{billId}"))
            {
                req.Headers.Add("Accept", "application/json");
                req.Headers.Add("Authorization", $"Bearer {Secret}");

                using (HttpResponseMessage resp = await client.SendAsync(req))
                {
                    resp.EnsureSuccessStatusCode();

                    var response = await resp.Content.ReadAsStringAsync();

                    var responseData = JsonConvert.DeserializeObject<ResponseData>(response);
                    switch (responseData.Status.Value)
                    {
                        case "WAITING":
                            {
                                return PaymentStatus.WAITING;
                            }
                        case "PAID":
                            {
                                return PaymentStatus.PAID;
                            }
                        case "REJECTED":
                            {
                                return PaymentStatus.REJECTED;
                            }
                        case "EXPIRED":
                            {
                                return PaymentStatus.EXPIRED;
                            }
                        default:
                            {
                                return PaymentStatus.NONE;
                            }
                    }
                }
            }
        }

        public async Task<ResponseData> CreatePayment()
        {
            string GuId = Guid.NewGuid().ToString();
            using (var req = new HttpRequestMessage(HttpMethod.Put, $"https://api.qiwi.com/partner/bill/v1/bills/{GuId}"))
            {
                CreateBillRequest request = new CreateBillRequest()
                {
                    Amount = new Amount()
                    {
                        Currency = "RUB",
                        Value = 30.0m,
                    },
                    ExpirationDateTime = DtExtensions.LocalTimeNow().AddMinutes(20),
                    Comment = "После оплаты счёта нажмите кнопку «Проверить», чтобы активировать подписку",
                    Customer = new Customer()
                    {
                        Account = GuId,
                        Email = "aynur.musin.06@mail.ru",
                        Phone = Number,
                    },
                    CustomFields = new CustomFields()
                    {
                        ThemeCode = ThemeCode,
                    },
                };
                string jsondata = JsonConvert.SerializeObject(request);

                req.Content = new StringContent(jsondata, Encoding.UTF8, "application/json");
                req.Headers.Add("Accept", "application/json");
                req.Headers.Add("Authorization", $"Bearer {Secret}");

                using (HttpResponseMessage resp = await client.SendAsync(req))
                {
                    resp.EnsureSuccessStatusCode();

                    var response = await resp.Content.ReadAsStringAsync();

                    return JsonConvert.DeserializeObject<ResponseData>(response);
                }
            }
        }

        public async Task RejectPayment(string billId)
        {
            using (var req = new HttpRequestMessage(HttpMethod.Post, $"https://api.qiwi.com/partner/bill/v1/bills/{billId}/reject"))
            {
                req.Headers.Add("Accept", "application/json");
                req.Headers.Add("Authorization", $"Bearer {Secret}");
                req.Content = new StringContent("", Encoding.UTF8, "application/json");
                using (HttpResponseMessage resp = await client.SendAsync(req))
                {
                    resp.EnsureSuccessStatusCode();
                }
            }
        }

        public void Dispose()
        {
            client.Dispose();
        }

        public async Task<double> GetBalance()
        {
            using (var req = new HttpRequestMessage(HttpMethod.Get, $"https://edge.qiwi.com/funding-sources/v2/persons/{Number}/accounts"))
            {
                req.Headers.Add("Accept", "application/json");
                req.Headers.Add("Authorization", $"Bearer {OauthToken}");
                //req.Content = new StringContent("", Encoding.UTF8, "application/json");
                using (HttpResponseMessage resp = await client.SendAsync(req))
                {
                    resp.EnsureSuccessStatusCode();
                    var response = await resp.Content.ReadAsStringAsync();

                    //https://developer.qiwi.com/ru/qiwi-wallet-personal/?python#balances_list
                    var json = JObject.Parse(response);
                    var rubAlias = json["accounts"].Where(x => x["alias"].ToString() == "qw_wallet_rub").FirstOrDefault();
                    var rubBalance = rubAlias["balance"]["amount"].ToString();
                    double.TryParse(rubBalance, out double balance);
                    return balance;
                }
            }
        }
    }
}
