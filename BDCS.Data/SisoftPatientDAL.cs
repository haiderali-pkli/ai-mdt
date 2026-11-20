using BDCS.Model.Patient;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System;
using System.Configuration;
using System.Data;

namespace BDCS.Data
{
    public class SisoftPatientDAL
    {
        // HASTA = patient demographics table in Sisoft's Oracle schema.
        // HS_KEY doubles as the hospital's MRN (confirmed against Sisoft's own
        // "MRN" field - ARSIVNO/OLDMRN exist but are unused/empty in this DB).
        const string LookupQuery = @"
SELECT h.HS_KEY, h.KIMLIKNO, h.HS_AD, h.HS_AD2, h.HS_SOYAD, h.BABAAD, h.ANNEAD,
       h.CINSIYET, h.DOGUMTARIH, h.DOGUMYER, h.ADRES, h.ADRES2, h.TELEFON, h.GSM,
       h.EPOSTA, h.KANGRUBU, h.RESIM, eu.UL_AD AS COUNTRYNAME
FROM PKLITEST.HASTA h
LEFT JOIN PKLITEST.EULKE eu ON eu.UL_KEY = h.UYRUK
WHERE h.HS_KEY = :hsKey";

        public SisoftPatientInfo GetPatientByMrn(string mrn)
        {
            if (!long.TryParse(mrn, out long hsKey))
            {
                return null;
            }

            string connString = ConfigurationManager.ConnectionStrings["SisoftConnection"]?.ConnectionString;
            if (string.IsNullOrEmpty(connString))
            {
                throw new InvalidOperationException("Sisoft connection string ('SisoftConnection') is not configured.");
            }

            using (OracleConnection con = new OracleConnection(connString))
            using (OracleCommand cmd = new OracleCommand(LookupQuery, con))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add(new OracleParameter("hsKey", OracleDbType.Int64) { Value = hsKey });

                con.Open();
                using (OracleDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        return null;
                    }

                    string genderCode = GetString(reader, "CINSIYET");

                    return new SisoftPatientInfo
                    {
                        HospitalNumber = GetString(reader, "HS_KEY"),
                        Cnic = GetString(reader, "KIMLIKNO"),
                        FirstName = GetString(reader, "HS_AD"),
                        MiddleName = GetString(reader, "HS_AD2"),
                        LastName = GetString(reader, "HS_SOYAD"),
                        FatherName = GetString(reader, "BABAAD"),
                        MotherName = GetString(reader, "ANNEAD"),
                        Gender = MapGender(genderCode),
                        DOB = GetNullableDate(reader, "DOGUMTARIH"),
                        PlaceOfBirth = GetString(reader, "DOGUMYER"),
                        MobileNumber = GetString(reader, "GSM"),
                        ContactNumber = GetString(reader, "TELEFON"),
                        EmailId = GetString(reader, "EPOSTA"),
                        Address = JoinAddress(GetString(reader, "ADRES"), GetString(reader, "ADRES2")),
                        Country = GetString(reader, "COUNTRYNAME"),
                        BloodGroup = MapBloodGroup(GetString(reader, "KANGRUBU")),
                        PhotoDataUri = GetPhotoDataUri(reader, "RESIM"),
                    };
                }
            }
        }

        static string MapGender(string code)
        {
            switch (code)
            {
                case "E": return "Male";
                case "K": return "Female";
                default: return null;
            }
        }

        // KANGRUBU is a numeric ABO/Rh code (Cozum HIS convention): 0/1=A+/-, 2/3=B+/-,
        // 4/5=AB+/-, 6/7=O+/-. Confirmed against this DB's own code-frequency distribution
        // (B+ and O+ dominate, matching expected population blood-group spread).
        static string MapBloodGroup(string code)
        {
            switch (code)
            {
                case "0": return "A+";
                case "1": return "A-";
                case "2": return "B+";
                case "3": return "B-";
                case "4": return "AB+";
                case "5": return "AB-";
                case "6": return "O+";
                case "7": return "O-";
                default: return null;
            }
        }

        static string GetPhotoDataUri(OracleDataReader reader, string column)
        {
            int ordinal = reader.GetOrdinal(column);
            if (reader.IsDBNull(ordinal)) return null;

            using (OracleBlob blob = reader.GetOracleBlob(ordinal))
            {
                if (blob == null || blob.IsNull || blob.Length <= 0) return null;

                byte[] bytes = new byte[blob.Length];
                blob.Read(bytes, 0, (int)blob.Length);
                return "data:" + DetectImageMime(bytes) + ";base64," + Convert.ToBase64String(bytes);
            }
        }

        static string DetectImageMime(byte[] bytes)
        {
            if (bytes.Length >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF) return "image/jpeg";
            if (bytes.Length >= 4 && bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47) return "image/png";
            if (bytes.Length >= 2 && bytes[0] == 0x42 && bytes[1] == 0x4D) return "image/bmp";
            if (bytes.Length >= 4 && bytes[0] == 0x47 && bytes[1] == 0x49 && bytes[2] == 0x46) return "image/gif";
            return "image/jpeg";
        }

        static string JoinAddress(string line1, string line2)
        {
            if (string.IsNullOrWhiteSpace(line1)) return line2;
            if (string.IsNullOrWhiteSpace(line2)) return line1;
            return line1.TrimEnd() + ", " + line2.TrimStart();
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
