namespace Timetable.Helpers
{
    public static class ConcurrentRandom
    {
        private static object locker = new object();


        private static Random rnd = new Random();


        /// <summary>
        /// Мультипоточная имплементация генерации
        /// псевдослучайных чисел без повторений
        /// с использованием блокировщика
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns>
        /// Случайное число в заданном диапазоне
        /// </returns>
        public static int Next(int min, int max)
        {
            lock (locker)
            {
                return rnd.Next(min, max);
            }
        }

        /// <summary>
        /// Мультипоточная имплементация генерации
        /// псевдослучайных чисел без повторений
        /// с использованием блокировщика
        /// </summary>
        /// <returns>
        /// Случайное число
        /// </returns>
        public static int Next()
        {
            lock (locker)
            {
                return rnd.Next();
            }
        }
    }
}
