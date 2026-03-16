#region

using System;
using R2V2.Extensions;

#endregion

namespace R2V2.Infrastructure
{
    public static class Guard
    {
        public static T IsNotNull<TException, T>(T value, string message)
            where TException : Exception
        {
            IsTrue<TException>(value != null, message);

            return value;
        }

        public static T IsNotNull<T>(T value, string message)
        {
            return IsNotNull<NullReferenceException, T>(value, message);
        }


        public static T ArgIsNotNull<T>(T value, string message)
        {
            return IsNotNull<ArgumentNullException, T>(value, message);
        }


        public static void IsTrue<TException>(bool value, string message)
            where TException : Exception
        {
            if (!value)
            {
                Throw<TException>(message);
            }
        }

        public static void Throw<TException>(string message)
        {
            var exception = Activator.CreateInstance(typeof(TException), message) as Exception;

            throw exception;
        }

        public static string IsNotEmpty<TException>(string value, string message)
            where TException : Exception
        {
            IsTrue<TException>(value.IsNotEmpty(), message);

            return value;
        }

        public static string IsNotEmpty<TException>(string value) where TException : Exception
        {
            return IsNotEmpty<TException>(value, string.Empty);
        }


        public static void IsFalse<TException>(bool value, string message)
            where TException : Exception
        {
            IsTrue<TException>(!value, message);
        }
    }
}