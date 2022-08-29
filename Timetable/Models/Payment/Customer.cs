namespace Timetable.Models.Payment
{
    public class Customer
    {
        /// <summary>
        /// Номер телефона пользователя (в международном формате)
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// E-mail пользователя
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Идентификатор пользователя в вашей системе
        /// </summary>
        public string Account { get; set; }
    }
}
