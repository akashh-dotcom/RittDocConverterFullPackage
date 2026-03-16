#region

using System;
using System.Data;
using System.Reflection;
using log4net;

#endregion

namespace R2Library.Data.ADO.Core
{
    public class DbReaderHelper : DbCommandHelper
    {
        protected new static readonly ILog Log =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.FullName);

        protected string GetStringValue(IDataReader reader, string fieldName)
        {
            try
            {
                return GetStringValue(reader, fieldName, null);
            }
            catch (Exception ex)
            {
                Log.WarnFormat("fieldName: {0}", fieldName);
                Log.Error(ex.Message, ex);
                throw;
            }
        }

        protected string GetStringValue(IDataReader reader, int fieldIndex)
        {
            try
            {
                return GetStringValue(reader, fieldIndex, null);
            }
            catch (Exception ex)
            {
                Log.WarnFormat("fieldIndex: {0}", fieldIndex);
                Log.Error(ex.Message, ex);
                throw;
            }
        }

        protected string GetStringValue(IDataReader reader, string fieldName, string defaultValue)
        {
            try
            {
                return GetStringValue(reader, reader.GetOrdinal(fieldName), defaultValue);
            }
            catch (Exception ex)
            {
                Log.WarnFormat("fieldName: {0}, defaultValue: {1}", fieldName, defaultValue);
                Log.Error(ex.Message, ex);
                throw;
            }
        }

        protected string GetStringValue(IDataReader reader, int fieldIndex, string defaultValue)
        {
            try
            {
                if (reader.IsDBNull(fieldIndex))
                {
                    return defaultValue;
                }

                return reader.GetString(fieldIndex);
            }
            catch (Exception ex)
            {
                Log.WarnFormat("fieldIndex: {0}, defaultValue: {1}", fieldIndex, defaultValue);
                Log.Error(ex.Message, ex);
                throw;
            }
        }


        protected T GetEnumValue<T>(IDataReader reader, string fieldName)
        {
            try
            {
                return GetEnumValue<T>(reader, fieldName, null);
            }
            catch (Exception ex)
            {
                Log.WarnFormat("fieldName: {0}", fieldName);
                Log.Error(ex.Message, ex);
                throw;
            }
        }

        protected T GetEnumValue<T>(IDataReader reader, int fieldIndex)
        {
            try
            {
                return GetEnumValue<T>(reader, fieldIndex, null);
            }
            catch (Exception ex)
            {
                Log.WarnFormat("fieldIndex: {0}", fieldIndex);
                Log.Error(ex.Message, ex);
                throw;
            }
        }

        protected T GetEnumValue<T>(IDataReader reader, string fieldName, string defaultValue)
        {
            try
            {
                return GetEnumValue<T>(reader, reader.GetOrdinal(fieldName), defaultValue);
            }
            catch (Exception ex)
            {
                Log.WarnFormat("fieldName: {0}, defaultValue: {1}", fieldName, defaultValue);
                Log.Error(ex.Message, ex);
                throw;
            }
        }

        protected T GetEnumValue<T>(IDataReader reader, int fieldIndex, string defaultValue)
        {
            try
            {
                if (reader.IsDBNull(fieldIndex))
                {
                    return (T)Enum.Parse(typeof(T), defaultValue);
                }

                return (T)Enum.Parse(typeof(T), reader.GetString(fieldIndex));
            }
            catch (Exception ex)
            {
                Log.WarnFormat("fieldIndex: {0}, defaultValue: {1}", fieldIndex, defaultValue);
                Log.Error(ex.Message, ex);
                throw;
            }
        }


        protected DateTime GetDateValue(IDataReader reader, string fieldName)
        {
            try
            {
                var fieldIndex = reader.GetOrdinal(fieldName);
                return GetDateValue(reader, fieldIndex);
            }
            catch (Exception ex)
            {
                Log.WarnFormat("fieldName: {0}", fieldName);
                Log.Error(ex.Message, ex);
                throw;
            }
        }

        protected DateTime GetDateValue(IDataReader reader, int fieldIndex)
        {
            try
            {
                if (reader.IsDBNull(fieldIndex))
                {
                    return DateTime.MinValue;
                }

                return reader.GetDateTime(fieldIndex);
            }
            catch (Exception ex)
            {
                Log.WarnFormat("fieldIndex: {0}", fieldIndex);
                Log.Error(ex.Message, ex);
                throw;
            }
        }

        protected DateTime GetDateValue(IDataReader reader, string fieldName, DateTime defaultValue)
        {
            try
            {
                var fieldIndex = reader.GetOrdinal(fieldName);
                return GetDateValue(reader, fieldIndex, defaultValue);
            }
            catch (Exception ex)
            {
                Log.WarnFormat("fieldName: {0}", fieldName);
                Log.Error(ex.Message, ex);
                throw;
            }
        }

        protected DateTime GetDateValue(IDataReader reader, int fieldIndex, DateTime defaultValue)
        {
            try
            {
                return reader.IsDBNull(fieldIndex) ? defaultValue : reader.GetDateTime(fieldIndex);
            }
            catch (Exception ex)
            {
                Log.WarnFormat("fieldIndex: {0}", fieldIndex);
                Log.Error(ex.Message, ex);
                throw;
            }
        }

        protected DateTime? GetDateValueOrNull(IDataReader reader, string fieldName)
        {
            try
            {
                var fieldIndex = reader.GetOrdinal(fieldName);
                return GetDateValueOrNull(reader, fieldIndex);
            }
            catch (Exception ex)
            {
                Log.WarnFormat("fieldName: {0}", fieldName);
                Log.Error(ex.Message, ex);
                throw;
            }
        }

        protected DateTime? GetDateValueOrNull(IDataReader reader, int fieldIndex)
        {
            try
            {
                if (reader.IsDBNull(fieldIndex))
                {
                    return null;
                }

                return reader.GetDateTime(fieldIndex);
            }
            catch (Exception ex)
            {
                Log.WarnFormat("fieldIndex: {0}", fieldIndex);
                Log.Error(ex.Message, ex);
                throw;
            }
        }

        protected bool GetBoolValue(IDataReader reader, string fieldName, bool defaultValue)
        {
            try
            {
                var fieldIndex = reader.GetOrdinal(fieldName);
                return GetBoolValue(reader, fieldIndex, defaultValue);
            }
            catch (Exception ex)
            {
                Log.WarnFormat("fieldName: {0}, defaultValue: {1}", fieldName, defaultValue);
                Log.Error(ex.Message, ex);
                throw;
            }
        }

        protected bool GetBoolValue(IDataReader reader, int fieldIndex, bool defaultValue)
        {
            try
            {
                if (reader.IsDBNull(fieldIndex))
                {
                    return defaultValue;
                }

                return reader.GetBoolean(fieldIndex);
            }
            catch (Exception ex)
            {
                Log.WarnFormat("fieldIndex: {0}, defaultValue: {1}", fieldIndex, defaultValue);
                Log.Error(ex.Message, ex);
                throw;
            }
        }

        protected int GetInt32Value(IDataReader reader, string fieldName, int defaultValue)
        {
            try
            {
                var fieldIndex = reader.GetOrdinal(fieldName);
                return GetInt32Value(reader, fieldIndex, defaultValue);
            }
            catch (Exception ex)
            {
                Log.WarnFormat("fieldName: {0}, defaultValue: {1}", fieldName, defaultValue);
                Log.Error(ex.Message, ex);
                throw;
            }
        }

        protected int GetInt32Value(IDataReader reader, int fieldIndex, int defaultValue)
        {
            try
            {
                if (reader.IsDBNull(fieldIndex))
                {
                    return defaultValue;
                }

                return reader.GetInt32(fieldIndex);
            }
            catch (Exception ex)
            {
                Log.WarnFormat("fieldIndex: {0}, defaultValue: {1}", fieldIndex, defaultValue);
                Log.Error(ex.Message, ex);
                throw;
            }
        }

        protected int? GetInt32Value(IDataReader reader, string fieldName)
        {
            try
            {
                var fieldIndex = reader.GetOrdinal(fieldName);
                return GetInt32Value(reader, fieldIndex);
            }
            catch (Exception ex)
            {
                Log.WarnFormat("fieldName: {0}", fieldName);
                Log.Error(ex.Message, ex);
                throw;
            }
        }

        protected int? GetInt32Value(IDataReader reader, int fieldIndex)
        {
            try
            {
                if (reader.IsDBNull(fieldIndex))
                {
                    return null;
                }

                return reader.GetInt32(fieldIndex);
            }
            catch (Exception ex)
            {
                Log.WarnFormat("fieldIndex: {0}", fieldIndex);
                Log.Error(ex.Message, ex);
                throw;
            }
        }

        protected short GetInt16Value(IDataReader reader, string fieldName, short defaultValue)
        {
            try
            {
                var fieldIndex = reader.GetOrdinal(fieldName);
                return GetInt16Value(reader, fieldIndex, defaultValue);
            }
            catch (Exception ex)
            {
                Log.WarnFormat("fieldName: {0}, defaultValue: {1}", fieldName, defaultValue);
                Log.Error(ex.Message, ex);
                throw;
            }
        }

        protected short GetInt16Value(IDataReader reader, int fieldIndex, short defaultValue)
        {
            try
            {
                if (reader.IsDBNull(fieldIndex))
                {
                    return defaultValue;
                }

                return reader.GetInt16(fieldIndex);
            }
            catch (Exception ex)
            {
                Log.WarnFormat("fieldIndex: {0}, defaultValue: {1}", fieldIndex, defaultValue);
                Log.Error(ex.Message, ex);
                throw;
            }
        }

        protected long GetInt64Value(IDataReader reader, string fieldName, long defaultValue)
        {
            try
            {
                var fieldIndex = reader.GetOrdinal(fieldName);
                return GetInt64Value(reader, fieldIndex, defaultValue);
            }
            catch (Exception ex)
            {
                Log.WarnFormat("fieldName: {0}, defaultValue: {1}", fieldName, defaultValue);
                Log.Error(ex.Message, ex);
                throw;
            }
        }

        protected long GetInt64Value(IDataReader reader, int fieldIndex, long defaultValue)
        {
            try
            {
                if (reader.IsDBNull(fieldIndex))
                {
                    return defaultValue;
                }

                return long.Parse(reader[fieldIndex].ToString());
            }
            catch (Exception ex)
            {
                Log.WarnFormat("fieldIndex: {0}, defaultValue: {1}", fieldIndex, defaultValue);
                Log.Error(ex.Message, ex);
                throw;
            }
        }

        protected double GetDoubleValue(IDataReader reader, string fieldName, double defaultValue)
        {
            try
            {
                var fieldIndex = reader.GetOrdinal(fieldName);
                return GetDoubleValue(reader, fieldIndex, defaultValue);
            }
            catch (Exception ex)
            {
                Log.WarnFormat("fieldName: {0}, defaultValue: {1}", fieldName, defaultValue);
                Log.Error(ex.Message, ex);
                throw;
            }
        }

        protected double GetDoubleValue(IDataReader reader, int fieldIndex, double defaultValue)
        {
            try
            {
                if (reader.IsDBNull(fieldIndex))
                {
                    return defaultValue;
                }

                return reader.GetDouble(fieldIndex);
            }
            catch (Exception ex)
            {
                Log.WarnFormat("fieldIndex: {0}, defaultValue: {1}", fieldIndex, defaultValue);
                Log.Error(ex.Message, ex);
                throw;
            }
        }

        protected decimal GetDecimalValue(IDataReader reader, string fieldName, decimal defaultValue)
        {
            try
            {
                var fieldIndex = reader.GetOrdinal(fieldName);
                return GetDecimalValue(reader, fieldIndex, defaultValue);
            }
            catch (Exception ex)
            {
                Log.WarnFormat("fieldName: {0}, defaultValue: {1}", fieldName, defaultValue);
                Log.Error(ex.Message, ex);
                throw;
            }
        }

        protected decimal GetDecimalValue(IDataReader reader, int fieldIndex, decimal defaultValue)
        {
            try
            {
                if (reader.IsDBNull(fieldIndex))
                {
                    return defaultValue;
                }

                return reader.GetDecimal(fieldIndex);
            }
            catch (Exception ex)
            {
                Log.WarnFormat("fieldIndex: {0}, defaultValue: {1}", fieldIndex, defaultValue);
                Log.Error(ex.Message, ex);
                throw;
            }
        }

        protected float GetFloatValue(IDataReader reader, string fieldName, float defaultValue)
        {
            try
            {
                var fieldIndex = reader.GetOrdinal(fieldName);
                Log.DebugFormat("fieldName: {0}, fieldIndex: {1}", fieldName, fieldIndex);
                return GetFloatValue(reader, fieldIndex, defaultValue);
            }
            catch (Exception ex)
            {
                Log.WarnFormat("fieldName: {0}, defaultValue: {1}", fieldName, defaultValue);
                Log.Error(ex.Message, ex);
                throw;
            }
        }

        protected float GetFloatValue(IDataReader reader, int fieldIndex, float defaultValue)
        {
            try
            {
                if (reader.IsDBNull(fieldIndex))
                {
                    return defaultValue;
                }

                return reader.GetFloat(fieldIndex);
            }
            catch (Exception ex)
            {
                Log.WarnFormat("fieldIndex: {0}, defaultValue: {1}", fieldIndex, defaultValue);
                Log.Error(ex.Message, ex);
                throw;
            }
        }

        protected Guid GetGuidValue(IDataReader reader, string fieldName, Guid defaultValue)
        {
            try
            {
                var fieldIndex = reader.GetOrdinal(fieldName);
                return GetGuidValue(reader, fieldIndex, defaultValue);
            }
            catch (Exception ex)
            {
                Log.WarnFormat("fieldName: {0}, defaultValue: {1}", fieldName, defaultValue);
                Log.Error(ex.Message, ex);
                throw;
            }
        }

        protected Guid GetGuidValue(IDataReader reader, int fieldIndex, Guid defaultValue)
        {
            try
            {
                if (reader.IsDBNull(fieldIndex))
                {
                    return defaultValue;
                }

                return reader.GetGuid(fieldIndex);
            }
            catch (Exception ex)
            {
                Log.WarnFormat("fieldIndex: {0}, defaultValue: {1}", fieldIndex, defaultValue);
                Log.Error(ex.Message, ex);
                throw;
            }
        }

        protected byte GetByteValue(IDataReader reader, string fieldName, byte defaultValue)
        {
            try
            {
                var fieldIndex = reader.GetOrdinal(fieldName);
                return GetByteValue(reader, fieldIndex, defaultValue);
            }
            catch (Exception ex)
            {
                Log.WarnFormat("fieldName: {0}, defaultValue: {1}", fieldName, defaultValue);
                Log.Error(ex.Message, ex);
                throw;
            }
        }

        protected byte GetByteValue(IDataReader reader, int fieldIndex, byte defaultValue)
        {
            try
            {
                if (reader.IsDBNull(fieldIndex))
                {
                    return defaultValue;
                }

                return reader.GetByte(fieldIndex);
            }
            catch (Exception ex)
            {
                Log.WarnFormat("fieldIndex: {0}, defaultValue: {1}", fieldIndex, defaultValue);
                Log.Error(ex.Message, ex);
                throw;
            }
        }

        protected byte[] GetByteArrayValue(IDataReader reader, int fieldIndex)
        {
            try
            {
                if (reader.IsDBNull(fieldIndex))
                {
                    return null;
                }

                reader.Read();
                var size = reader.GetBytes(0, 0, null, 0, 0); //get the length of data
                var values = new byte[size];

                const int bufferSize = 8;
                long bytesRead = 0;
                var curPos = 0;

                while (bytesRead < size)
                {
                    bytesRead += reader.GetBytes(0, curPos, values, curPos, bufferSize);
                    curPos += bufferSize;
                }

                return values;
            }
            catch (Exception ex)
            {
                Log.WarnFormat("fieldIndex: {0}", fieldIndex);
                Log.Error(ex.Message, ex);
                throw;
            }
        }


        protected bool GetBoolValue(DataRow dataRow, string fieldName, bool defaultValue)
        {
            var c = dataRow.Table.Columns[fieldName];
            var fieldIndex = c.Ordinal;

            return GetBoolValue(dataRow, fieldIndex, defaultValue);
        }

        protected bool GetBoolValue(DataRow dataRow, int fieldIndex, bool defaultValue)
        {
            if (dataRow[fieldIndex] == DBNull.Value)
            {
                return defaultValue;
            }

            return (bool)dataRow[fieldIndex];
        }

        protected byte[] GetByteArrayValue(DataRow dataRow, int fieldIndex)
        {
            if (dataRow[fieldIndex] == DBNull.Value)
            {
                return null;
            }

            return (byte[])dataRow[fieldIndex];
        }

        protected byte GetByteValue(DataRow dataRow, string fieldName, byte defaultValue)
        {
            var c = dataRow.Table.Columns[fieldName];
            var fieldIndex = c.Ordinal;

            return GetByteValue(dataRow, fieldIndex, defaultValue);
        }

        protected byte GetByteValue(DataRow dataRow, int fieldIndex, byte defaultValue)
        {
            if (dataRow[fieldIndex] == DBNull.Value)
            {
                return defaultValue;
            }

            return (byte)dataRow[fieldIndex];
        }

        protected DateTime GetDateValue(DataRow dataRow, string fieldName)
        {
            var c = dataRow.Table.Columns[fieldName];
            var fieldIndex = c.Ordinal;
            return GetDateValue(dataRow, fieldIndex);
        }

        protected DateTime GetDateValue(DataRow dataRow, int fieldIndex)
        {
            if (dataRow[fieldIndex] == DBNull.Value)
            {
                return DateTime.MinValue;
            }

            return (DateTime)dataRow[fieldIndex];
        }

        protected decimal GetDecimalValue(DataRow dataRow, string fieldName, decimal defaultValue)
        {
            var c = dataRow.Table.Columns[fieldName];
            var fieldIndex = c.Ordinal;
            return GetDecimalValue(dataRow, fieldIndex, defaultValue);
        }

        protected decimal GetDecimalValue(DataRow dataRow, int fieldIndex, decimal defaultValue)
        {
            if (dataRow[fieldIndex] == DBNull.Value)
            {
                return defaultValue;
            }

            return (decimal)dataRow[fieldIndex];
        }

        protected double GetDoubleValue(DataRow dataRow, string fieldName, double defaultValue)
        {
            var c = dataRow.Table.Columns[fieldName];
            var fieldIndex = c.Ordinal;
            return GetDoubleValue(dataRow, fieldIndex, defaultValue);
        }

        protected double GetDoubleValue(DataRow dataRow, int fieldIndex, double defaultValue)
        {
            if (dataRow[fieldIndex] == DBNull.Value)
            {
                return defaultValue;
            }

            return (double)dataRow[fieldIndex];
        }

        protected Guid GetGuidValue(DataRow dataRow, string fieldName, Guid defaultValue)
        {
            var c = dataRow.Table.Columns[fieldName];
            var fieldIndex = c.Ordinal;
            return GetGuidValue(dataRow, fieldIndex, defaultValue);
        }

        protected Guid GetGuidValue(DataRow dataRow, int fieldIndex, Guid defaultValue)
        {
            if (dataRow[fieldIndex] == DBNull.Value)
            {
                return defaultValue;
            }

            return (Guid)dataRow[fieldIndex];
        }

        protected int GetInt16Value(DataRow dataRow, string fieldName, short defaultValue)
        {
            var c = dataRow.Table.Columns[fieldName];
            var fieldIndex = c.Ordinal;
            return GetInt16Value(dataRow, fieldIndex, defaultValue);
        }

        protected short GetInt16Value(DataRow dataRow, int fieldIndex, short defaultValue)
        {
            if (dataRow[fieldIndex] == DBNull.Value)
            {
                return defaultValue;
            }

            return short.Parse(dataRow[fieldIndex].ToString());
        }

        protected int GetInt32Value(DataRow dataRow, string fieldName, int defaultValue)
        {
            var c = dataRow.Table.Columns[fieldName];
            var fieldIndex = c.Ordinal;
            return GetInt32Value(dataRow, fieldIndex, defaultValue);
        }

        protected int GetInt32Value(DataRow dataRow, int fieldIndex, int defaultValue)
        {
            if (dataRow[fieldIndex] == DBNull.Value)
            {
                return defaultValue;
            }

            return int.Parse(dataRow[fieldIndex].ToString());
        }

        protected long GetInt64Value(DataRow dataRow, string fieldName, long defaultValue)
        {
            var c = dataRow.Table.Columns[fieldName];
            var fieldIndex = c.Ordinal;
            return GetInt64Value(dataRow, fieldIndex, defaultValue);
        }

        protected long GetInt64Value(DataRow dataRow, int fieldIndex, long defaultValue)
        {
            if (dataRow[fieldIndex] == DBNull.Value)
            {
                return defaultValue;
            }

            return long.Parse(dataRow[fieldIndex].ToString());
        }

        protected string GetStringValue(DataRow dataRow, string fieldName)
        {
            return GetStringValue(dataRow, fieldName, null);
        }

        protected string GetStringValue(DataRow dataRow, int fieldIndex)
        {
            return GetStringValue(dataRow, fieldIndex, null);
        }

        protected string GetStringValue(DataRow dataRow, string fieldName, string defaultValue)
        {
            var c = dataRow.Table.Columns[fieldName];
            var fieldIndex = c.Ordinal;
            return GetStringValue(dataRow, fieldIndex, defaultValue);
        }

        protected string GetStringValue(DataRow dataRow, int fieldIndex, string defaultValue)
        {
            if (dataRow[fieldIndex] == DBNull.Value)
            {
                return defaultValue;
            }

            return (string)dataRow[fieldIndex];
        }

        protected float GetFloatValue(DataRow dataRow, int fieldIndex, float defaultValue)
        {
            if (dataRow[fieldIndex] == DBNull.Value)
            {
                return defaultValue;
            }

            return (float)dataRow[fieldIndex];
        }
    }

    public static class DataRecordExtensions
    {
        public static bool HasColumn(this IDataRecord dr, string columnName)
        {
            for (var i = 0; i < dr.FieldCount; i++)
            {
                if (dr.GetName(i).Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }

            return false;
        }
    }
}