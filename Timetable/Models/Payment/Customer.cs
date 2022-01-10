namespace Timetable.Models.Payment
{
    public class Customer
    {
        /// <summary>
        /// Номер телефона пользователя (в международном формате)
        /// </summary>
        public string phone { get; set; }

        /// <summary>
        /// E-mail пользователя
        /// </summary>
        public string email { get; set; }

        /// <summary>
        /// Идентификатор пользователя в вашей системе
        /// </summary>
        public string account { get; set; }
    }
}
