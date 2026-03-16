#region

using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Text;

#endregion

namespace R2Library.Data.ADO.R2.DataServices
{
    public class DisciplineToSpecialtyDataService : DataServiceBase
    {
        private static readonly string _specialtyCodeByDisciplineIdQuery = new StringBuilder()
            .Append("select s.vchSpecialtyCode ")
            .Append("from   tDiscipline d ")
            .Append(" join  tSpecialty s on s.vchSpecialtyName = d.vchDisciplineName ")
            .Append("where  iDisciplineId = @DisciplineId; ")
            .ToString();


        public string GetSpecialtyCodeByDisciplineId(int disciplineId)
        {
            string specialtyCode = null;

            SqlConnection cnn = null;
            SqlCommand command = null;
            SqlDataReader reader = null;

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            Log.DebugFormat("sql: {0}", _specialtyCodeByDisciplineIdQuery);
            try
            {
                cnn = GetConnection();

                command = cnn.CreateCommand();
                command.CommandText = _specialtyCodeByDisciplineIdQuery;
                command.CommandTimeout = 15;

                SetCommandParmater(command, "DisciplineId", disciplineId);

                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    if (specialtyCode == null)
                    {
                        specialtyCode = GetStringValue(reader, "vchSpecialtyCode");
                    }
                    else
                    {
                        Log.WarnFormat("More than one specialty code returned: {0}", specialtyCode);
                    }
                }

                stopWatch.Stop();

                if (specialtyCode == null)
                {
                    Log.WarnFormat("Specialty code not found, disciplineId: {0}", disciplineId);
                }


                Log.DebugFormat("query time: {0}, specialtyCode: {1}", stopWatch.ElapsedMilliseconds, specialtyCode);
                return specialtyCode;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                throw;
            }
            finally
            {
                DisposeConnections(cnn, command, reader);
            }
        }
    }
}