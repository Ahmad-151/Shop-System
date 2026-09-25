using Microsoft.Data.SqlClient;
using ShopAPI.DTOClasses;
using System.Data;

namespace ShopAPI.Repositories
{
    public class PersonRepository : IPersonRepository
    {
        private readonly string _connectionString;
        public PersonRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ShopDbConnection");
        }

        public async Task<List<PersonDTO>> GetAllAsync()
        {
            List<PersonDTO> listPerson = new List<PersonDTO>();
            string sql = "Select Person_ID, PersonName, PhoneNumber, Address, Role From Persons;";
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                await con.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            listPerson.Add(new PersonDTO
                            {
                                PersonId = Convert.ToInt32(reader["Person_ID"]),
                                PersonName = reader["PersonName"].ToString(),
                                PhoneNumber = reader["PhoneNumber"].ToString(),
                                Address = reader["Address"].ToString(),
                                Role = reader["Role"].ToString()
                            });
                        }
                    }
                }
            }
            return listPerson;
        }
        public async Task<int> RegisterAsync(RegisterPersonDTO registerDto, string passwordHash)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                await con.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("dbo.SP_AddNewPerson", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@PersonName", SqlDbType.NVarChar, 150).Value = registerDto.PersonName;
                    cmd.Parameters.Add("@PhoneNumber", SqlDbType.NVarChar, 20).Value = registerDto.PhoneNumber;
                    cmd.Parameters.Add("@Address", SqlDbType.NVarChar, 250).Value = registerDto.Address;
                    cmd.Parameters.Add("@PasswordHash", SqlDbType.Char, 60).Value = passwordHash;
                    cmd.Parameters.Add("@Role", SqlDbType.NVarChar, 20).Value = "Customer";

                    var newId = cmd.Parameters.Add("@NewID", SqlDbType.Int);
                    newId.Direction = ParameterDirection.Output;

                    await cmd.ExecuteNonQueryAsync();
                    return (int)(newId.Value ?? 0);
                }
            }
        }

        public async Task<PersonDTO> GetByIdAsync(int id)
        {
            string sql = "Select Person_ID, PersonName, PhoneNumber, Address, Role From Persons where Person_ID = @Person_ID";
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                await con.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.Add("@Person_ID", SqlDbType.Int).Value = id;
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (!await reader.ReadAsync()) return null;
                        return new PersonDTO
                        {
                            PersonId = Convert.ToInt32(reader["Person_ID"]),
                            PersonName = reader["PersonName"].ToString(),
                            PhoneNumber = reader["PhoneNumber"].ToString(),
                            Address = reader["Address"].ToString(),
                            Role = reader["Role"].ToString()
                        };
                    }
                }
            }
        }

        public async Task<bool> UpdateAsync(UpdatePersonDTO updatePersonDTO, int id)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("dbo.SP_UpdatePerson", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@PhoneNumber", SqlDbType.NVarChar, 20).Value = updatePersonDTO.PhoneNumber;
                    cmd.Parameters.Add("@Address", SqlDbType.NVarChar, 250).Value = updatePersonDTO.Address;
                    cmd.Parameters.Add("@Person_ID", SqlDbType.Int).Value = id;
                    var rowsaff = cmd.Parameters.Add("@RowsAffected", SqlDbType.Int);
                    rowsaff.Direction = ParameterDirection.Output;

                    await cmd.ExecuteNonQueryAsync();
                    return ((int)(rowsaff.Value ?? 0)) > 0;
                }
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                await con.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("dbo.SP_DeletePerson", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Person_ID", SqlDbType.Int).Value = id;
                    var rowsaff = cmd.Parameters.Add("@RowsAffected", SqlDbType.Int);
                    rowsaff.Direction = ParameterDirection.Output;

                    await cmd.ExecuteNonQueryAsync();
                    return ((int)(rowsaff.Value ?? 0)) > 0;
                }
            }
        }

        public async Task<bool> BannAsync(BannedPersonDTO bannedPersonDTO, int id)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                await con.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("dbo.BannPerson", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Person_ID", SqlDbType.Int).Value = id;
                    cmd.Parameters.Add("@Reason", SqlDbType.NVarChar, 200).Value = bannedPersonDTO.Reason;
                    cmd.Parameters.Add("@BannedAt", SqlDbType.DateTime2).Value = bannedPersonDTO.BannedAt;
                    var rowsaff = cmd.Parameters.Add("@RAff", SqlDbType.Int);
                    rowsaff.Direction = ParameterDirection.Output;

                    await cmd.ExecuteNonQueryAsync();
                    return ((int)(rowsaff.Value ?? 0)) > 0;
                }
            }
        }
        public async Task<bool> AddToFavouriteAsync(int discount, int id)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                await con.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("dbo.SP_MakeDisCountFor", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@DC", SqlDbType.Int).Value = discount;
                    cmd.Parameters.Add("@Person_ID", SqlDbType.Int).Value = id;

                    await cmd.ExecuteNonQueryAsync();
                    return true;
                }
            }
        }

        public async Task<PersonAuthDTO> GetByPhoneNumberAsync(string phoneNumber)
        {
            string sql = "Select Person_ID, PersonName, PhoneNumber, PasswordHash, Role From Persons where PhoneNumber = @PhoneNumber";
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                await con.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.Add("@PhoneNumber", SqlDbType.NVarChar, 20).Value = phoneNumber;
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (!await reader.ReadAsync()) return null;
                        return new PersonAuthDTO
                        {
                            Person_ID = Convert.ToInt32(reader["Person_ID"]),
                            PersonName = reader["PersonName"].ToString(),
                            PhoneNumber = reader["PhoneNumber"].ToString(),
                            PasswordHash = reader["PasswordHash"] is DBNull ? null : reader["PasswordHash"].ToString(),
                            Role = reader["Role"].ToString()
                        };
                    }
                }
            }
        }
    }
}
