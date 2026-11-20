using BDCS.Model.Patient;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;

namespace BDCS.Data
{
    public class SisoftLabDAL
    {
        // Bridge chain validated against the live Sisoft schema:
        // HISFSAT (HS_KEY = patient, HIZMETKEY = ordered procedure, FS_KEY = result link)
        //   -> HLABS (FS_KEY, PARAM/SONUC = the itemized lab result)
        //   -> HHIZMET (HZ_KEY, HZ_KOD = the procedure/CPT code shown on Sisoft's own
        //      "Procedure Definitions" screen).
        // One HZ_KOD can occasionally produce more than one distinct PARAM (a panel
        // code), so this returns the latest result per (CptCode, Param) pair and lets
        // the caller - which knows the local test's name - disambiguate when needed.
        public List<SisoftLabResult> GetLatestLabResults(string hospitalNumber, IEnumerable<string> cptCodes)
        {
            List<SisoftLabResult> results = new List<SisoftLabResult>();

            if (!long.TryParse(hospitalNumber, out long hsKey))
            {
                return results;
            }

            List<string> codes = (cptCodes ?? Enumerable.Empty<string>())
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Select(c => c.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (codes.Count == 0)
            {
                return results;
            }

            string connString = ConfigurationManager.ConnectionStrings["SisoftConnection"]?.ConnectionString;
            if (string.IsNullOrEmpty(connString))
            {
                throw new InvalidOperationException("Sisoft connection string ('SisoftConnection') is not configured.");
            }

            List<string> codeParamNames = new List<string>();
            for (int i = 0; i < codes.Count; i++)
            {
                codeParamNames.Add(":c" + i);
            }

            string query = $@"
SELECT CPTCODE, PARAM, SONUC, BIRIM, RESULTDATE FROM (
    SELECT hz.HZ_KOD AS CPTCODE, l.PARAM AS PARAM, l.SONUC AS SONUC, l.BIRIM AS BIRIM,
           NVL(l.BITZAMAN, l.BASZAMAN) AS RESULTDATE,
           ROW_NUMBER() OVER (PARTITION BY hz.HZ_KOD, l.PARAM ORDER BY NVL(l.BITZAMAN, l.BASZAMAN) DESC) AS RN
    FROM PKLITEST.HISFSAT i
    JOIN PKLITEST.HLABS l ON l.FS_KEY = i.FS_KEY
    JOIN PKLITEST.HHIZMET hz ON hz.HZ_KEY = i.HIZMETKEY
    WHERE i.HS_KEY = :hsKey
      AND hz.HZ_KOD IN ({string.Join(",", codeParamNames)})
      AND l.SONUC IS NOT NULL
)
WHERE RN = 1";

            using (OracleConnection con = new OracleConnection(connString))
            using (OracleCommand cmd = new OracleCommand(query, con))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add(new OracleParameter("hsKey", OracleDbType.Int64) { Value = hsKey });
                for (int i = 0; i < codes.Count; i++)
                {
                    cmd.Parameters.Add(new OracleParameter("c" + i, OracleDbType.Varchar2) { Value = codes[i] });
                }

                con.Open();
                using (OracleDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        results.Add(new SisoftLabResult
                        {
                            CptCode = GetString(reader, "CPTCODE"),
                            Param = GetString(reader, "PARAM"),
                            Result = GetString(reader, "SONUC"),
                            Unit = GetString(reader, "BIRIM"),
                            ResultDate = GetNullableDate(reader, "RESULTDATE"),
                        });
                    }
                }
            }

            return results;
        }

        static string GetString(IDataRecord reader, string column)
        {
            int ordinal = reader.GetOrdinal(column);
            if (reader.IsDBNull(ordinal)) return null;
            string value = reader.GetValue(ordinal).ToString().Trim();
            return value.Length == 0 ? null : value;
        }

        static DateTime? GetNullableDate(IDataRecord reader, string column)
        {
            int ordinal = reader.GetOrdinal(column);
            return reader.IsDBNull(ordinal) ? (DateTime?)null : Convert.ToDateTime(reader.GetValue(ordinal));
        }
    }
}
