#region

using System.Web.Mvc;
using Newtonsoft.Json;

#endregion

namespace R2V2.Web.Helpers
{
    public static class TempDataExtensions
    {
        /// <summary>
        ///     Use this to add Items to Temp Data
        /// </summary>
        public static void AddItem(this TempDataDictionary tempData, string objectName, object obj)
        {
            tempData[objectName] = JsonConvert.SerializeObject(obj);
        }

        /// <summary>
        ///     Use this to Get Items to Temp Data
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static T GetItem<T>(this TempDataDictionary tempData, string objectName)
        {
            var test = tempData[objectName]?.ToString();
            if (test == null || string.IsNullOrWhiteSpace(test))
            {
                return default(T);
            }

            return JsonConvert.DeserializeObject<T>(tempData[objectName].ToString());
        }

        /// <summary>
        ///     Use this to Delete Items from Temp Data
        /// </summary>
        public static void DeleteItem(this TempDataDictionary tempData, string objectName)
        {
            tempData[objectName] = null;
        }
    }
}