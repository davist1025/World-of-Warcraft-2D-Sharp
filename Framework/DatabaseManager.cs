﻿using Framework.Entity;
using Framework.Network;
using Framework.Network.Cryptography;
using Framework.Network.Entity;
using Isopoh.Cryptography.Argon2;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Framework.Entity.Vector;

namespace Framework
{
    /// <summary>
    /// Handles everything related to the database.
    /// </summary>
    public class DatabaseManager
    {
        public enum Status
        {
            OK,
            RowExists,
            Fatal
        }

        private const string SqlServer = "localhost";
        private const string SqlUsername = "root";
        private const string SqlPassword = "admin";

        private static readonly string AuthenticationConnectionStr =
            $"Server={SqlServer}; Database=demi_auth; User Id={SqlUsername}; Password={SqlPassword};";

        private static readonly string CharacterConnectionStr =
            $"Server={SqlServer}; Database=demi_character; User Id={SqlUsername}; Password={SqlPassword};";

        private static async Task<Status> InitializeAsync(MySqlConnection connection)
        {
            var status = Status.OK;

            try { await connection.OpenAsync(); }
            catch (MySqlException) { status = Status.Fatal; }

            return status;
        }

        /// <summary>
        /// Test database connections.
        /// </summary>
        public static Status Initialize()
        {
            List<Status> statuses = new List<Status>();

            using (var connection = new MySqlConnection(AuthenticationConnectionStr))
            {
                Status status = InitializeAsync(connection).Result;
                statuses.Add(status);
            }

            using (var connection = new MySqlConnection(CharacterConnectionStr))
            {
                Status status = InitializeAsync(connection).Result;
                statuses.Add(status);
            }

            foreach (var status in statuses)
                if (status == Status.Fatal)
                    return Status.Fatal;
            return Status.OK;
        }

        /// <summary>
        /// Execute an asynchronous command.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        private static async Task<Status> ExecuteCommand(MySqlConnection connection, MySqlCommand command)
        {
            var status = Status.OK;

            try
            {
                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
            }
            catch (MySqlException) { status = Status.Fatal; }

            return status;
        }

        /// <summary>
        /// Execute an asynchronous count.
        /// Currently only checks for a single existing row rather then a count.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        private static async Task<Status> ExecuteCount(MySqlConnection connection, MySqlCommand command)
        {
            var status = Status.OK;

            try
            {
                await connection.OpenAsync();
                int count = Convert.ToInt32(await command.ExecuteScalarAsync());
                if (count > 0)
                    status = Status.RowExists;
            } catch (MySqlException ex)
            {
                status = Status.Fatal;
                Console.WriteLine(ex.Message);
            }

            return status;
        }

        /// <summary>
        /// Execute an account reader.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        private static async Task<Account> ExecuteAccountReader(MySqlConnection connection, MySqlCommand command)
        {
            var account = new Account();

            try
            {
                await connection.OpenAsync();
                var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    account.ID = reader.GetInt32(0);
                    account.Username = reader.GetString(1);
                    account.Password_Hashed = reader.GetString(2);
                    account.Security = (AccountSecurity)reader.GetInt32(3);
                }
            }
            catch (MySqlException) { account.Status = Account.LoginStatus.ServerError; }

            return account;
        }

        /// <summary>
        /// Execute an account id-read.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        private static async Task<int> ExecuteAccountIDReader(MySqlConnection connection, MySqlCommand command)
        {
            var id = -1;

            try
            {
                await connection.OpenAsync();
                var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    id = int.Parse(reader["user_id"].ToString());
                }
            }
            catch (MySqlException) { }

            return id;
        }

        /// <summary>
        /// Execute a realm reader.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        private static async Task<List<Realm>> ExecuteRealmReader(MySqlConnection connection, MySqlCommand command)
        {
            var realmlist = new List<Realm>();

            try
            {
                await connection.OpenAsync();
                var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var realm = new Realm()
                    {
                        ID = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Port = reader.GetInt32(2)
                    };
                    realmlist.Add(realm);
                }
            }
            catch (MySqlException) { }

            return realmlist;
        }

        /// <summary>
        /// Execute a character reader.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        private static async Task<List<RealmCharacter>> ExecuteCharacterListReader(MySqlConnection connection, MySqlCommand command)
        {
            var characters = new List<RealmCharacter>();

            try
            {
                await connection.OpenAsync();
                var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    characters.Add(new RealmCharacter()
                    {
                        GUID = reader["CharacterID"].ToString(),
                        Name = reader["Username"].ToString(),
                        Level = (int)reader["Level"],
                        Class = (Class)reader["Class"],
                        Race = (Race)reader["Race"]
                    });
                }
            }
            catch (MySqlException ex) { Console.WriteLine(ex.Message); }

            return characters;
        }

        /// <summary>
        /// Fetch a character guid.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        private static async Task<string> FetchCharacterGUID(MySqlConnection connection, MySqlCommand command)
        {
            var guid = string.Empty;

            try
            {
                await connection.OpenAsync();
                var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                    guid = reader["character_id"].ToString();
            }
            catch (MySqlException) { }

            return guid;
        }

        /// <summary>
        /// Fetch a map id.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        private static async Task<int> FetchMapID(MySqlConnection connection, MySqlCommand command)
        {
            var mapId = -1;

            try
            {
                await connection.OpenAsync();
                var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                    mapId = int.Parse(reader["map_id"].ToString());
            }
            catch (MySqlException) { }

            return mapId;
        }

        /// <summary>
        /// Execute a reader gathering all of a specific character's data.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        private static async Task<Vector> ExecuteVectorReader(MySqlConnection connection, MySqlCommand command)
        {
            var vector = new Vector();

            try
            {
                await connection.OpenAsync();
                var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    vector = new Vector()
                    {
                        MapID = int.Parse(reader["map_id"].ToString()),
                        CellID = int.Parse(reader["cell_id"].ToString()),
                        X = float.Parse(reader["x"].ToString()),
                        Y = float.Parse(reader["y"].ToString()),
                        Direction = (MoveDirection)int.Parse(reader["direction"].ToString())
                    };
                }
            }
            catch (MySqlException ex) { Console.WriteLine(ex.Message); }

            return vector;
        }

        /// <summary>
        /// Fetch all realms for the realmlist.
        /// </summary>
        /// <returns></returns>
        public static List<Realm> FetchRealms()
        {
            var realmlist = new List<Realm>();
            var query = "SELECT * FROM realmlist";

            using (var connection = new MySqlConnection(AuthenticationConnectionStr))
            {
                using (var command = new MySqlCommand(query))
                {
                    command.Connection = connection;
                    realmlist = ExecuteRealmReader(connection, command).Result;
                }
            }
            return realmlist;
        }

        /// <summary>
        /// Fetch the map id belonging to the specified race id.
        /// </summary>
        /// <param name="raceId"></param>
        /// <returns></returns>
        public static int FetchMapIDForRace(int raceId)
        {
            var mapId = -1;
            var query = "SELECT map_id FROM character_spawns WHERE race_id=@raceId";

            using (var connection = new MySqlConnection(CharacterConnectionStr))
            {
                using (var command = new MySqlCommand(query))
                {
                    command.Connection = connection;
                    command.Parameters.AddWithValue("@raceId", raceId);
                    mapId = FetchMapID(connection, command).Result;
                }
            }

            return mapId;
        }

        /// <summary>
        /// Fetch the mapId for the character; Used for the character list.
        /// </summary>
        /// <param name="characterId"></param>
        /// <returns></returns>
        public static int FetchMapIDForCharacter(string characterId)
        {
            var mapId = -1;
            var query = "SELECT DISTINCT map_id FROM character_location_data WHERE character_id=@characterId";

            using (var connection = new MySqlConnection(CharacterConnectionStr))
            {
                using (var command = new MySqlCommand(query))
                {
                    command.Connection = connection;
                    command.Parameters.AddWithValue("@characterId", characterId);
                    mapId = FetchMapID(connection, command).Result;
                }
            }

            return mapId;
        }

        /// <summary>
        /// Does this user exist?
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public static Status UserExists(string username)
        {
            var status = Status.OK;
            var query = "SELECT COUNT(*) FROM account WHERE username=@username";

            using (var connection = new MySqlConnection(AuthenticationConnectionStr))
            {
                using (var command = new MySqlCommand(query))
                {
                    command.Parameters.AddWithValue("@username", username);
                    command.Connection = connection;

                    status = ExecuteCount(connection, command).Result;
                }
            }

            return status;
        }

        /// <summary>
        /// Create a new user.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static Status CreateUser(string username, string password)
        {
            var existStatus = UserExists(username);
            if (existStatus == Status.RowExists || existStatus == Status.Fatal)
                return existStatus;

            var password_hash = CryptoHelper.ArgonHash(CryptoHelper.ComputeSHA256(password));
            var query = "INSERT INTO account (username, password_hash, security) VALUES (@username, @password, @security)";
            var status = Status.OK;

            using (var connection = new MySqlConnection(AuthenticationConnectionStr))
            {
                using (var command = new MySqlCommand(query))
                {
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@password", password_hash);
                    command.Parameters.AddWithValue("@security", AccountSecurity.Player);
                    command.Connection = connection;

                    status = ExecuteCommand(connection, command).Result;
                }
            }

            if (status != Status.OK)
                return status;

            var userId = FetchAccountID(username);
            query = "INSERT INTO account_online (user_id) VALUES (@userId)";

            using (var connection = new MySqlConnection(AuthenticationConnectionStr))
            {
                using (var command = new MySqlCommand(query))
                {
                    command.Parameters.AddWithValue("@userId", userId);
                    command.Connection = connection;

                    status = ExecuteCommand(connection, command).Result;
                }
            }

            return status;
        }

        /// <summary>
        /// Does this character exist?
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static Status CharacterExists(string name)
        {
            var status = Status.OK;
            var query = "SELECT COUNT(*) FROM character_data WHERE name=@name";

            using (var connection = new MySqlConnection(CharacterConnectionStr))
            {
                using (var command = new MySqlCommand(query))
                {
                    command.Parameters.AddWithValue("@name", name);
                    command.Connection = connection;

                    status = ExecuteCount(connection, command).Result;
                }
            }

            return status;
        }

        /// <summary>
        /// Attempts to create a character.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="name"></param>
        /// <param name="race"></param>
        /// <returns></returns>
        public static Status CreateCharacter(int userId, string name, Race race, Map map)
        {
            name = name.Substring(0, 1).ToUpper() + name.Substring(1).ToLower();
            var existStatus = CharacterExists(name);
            if (existStatus == Status.RowExists || existStatus == Status.Fatal)
                return existStatus;

            var guid = Guid.NewGuid();
            var status = Status.OK;

            var query = "INSERT INTO character_data (user_id, character_id, name, level, class_id, race_id) VALUES (@userId, @characterId, @name, @level, @classId, @raceId)";
            using (var connection = new MySqlConnection(CharacterConnectionStr))
            {
                using (var command = new MySqlCommand(query))
                {
                    command.Parameters.AddWithValue("@userId", userId);
                    command.Parameters.AddWithValue("@characterId", guid);
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@level", 1);
                    command.Parameters.AddWithValue("@classId", (int)Class.Warrior);
                    command.Parameters.AddWithValue("@raceId", (int)race);
                    command.Connection = connection;

                    status = ExecuteCommand(connection, command).Result;
                }
            }

            query = "INSERT INTO character_location_data (character_id, map_id, cell_id, x, y, direction) VALUES (@characterId, @mapId, -1, @x, @y, 0)";
            using (var connection = new MySqlConnection(CharacterConnectionStr))
            {
                using (var command = new MySqlCommand(query))
                {
                    command.Parameters.AddWithValue("@characterId", guid);
                    command.Parameters.AddWithValue("@mapId", map.ID);
                    command.Parameters.AddWithValue("@x", map.Spawns.Objects["Player"].X);
                    command.Parameters.AddWithValue("@y", map.Spawns.Objects["Player"].Y);
                    command.Connection = connection;

                    status = ExecuteCommand(connection, command).Result;
                }
            }

            return status;
        }

        /// <summary>
        /// Get the characters for the specified user.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static List<RealmCharacter> FetchCharacters(int userId)
        {
            var characters = new List<RealmCharacter>();
            var query = "SELECT character_data.name as Username, " +
                "character_data.level as Level, " +
                "character_data.class_id as Class, " +
                "character_data.race_id as Race, " +
                "character_data.character_id as CharacterID " +
                "FROM character_data " +
                "WHERE character_data.user_id=@userId";

            using (var connection = new MySqlConnection(CharacterConnectionStr))
            {
                using (var command = new MySqlCommand(query))
                {
                    command.Parameters.AddWithValue("@userId", userId);
                    command.Connection = connection;
                    characters = ExecuteCharacterListReader(connection, command).Result;
                }
            }

            foreach (var character in characters)
                character.Vector = FetchCharacterVector(character.GUID);

            return characters;
        }

        /// <summary>
        /// Fetch the given character's vector.
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        private static Vector FetchCharacterVector(string guid)
        {
            var vector = new Vector();
            var query = "SELECT map_id, cell_id, x, y, direction FROM character_location_data WHERE character_id=@characterId";

            using (var connection = new MySqlConnection(CharacterConnectionStr))
            {
                using (var command = new MySqlCommand(query))
                {
                    command.Parameters.AddWithValue("@characterId", guid);
                    command.Connection = connection;
                    vector = ExecuteVectorReader(connection, command).Result;
                }
            }

            return vector;
        }

        /// <summary>
        /// Attempt to delete the given character for the specified user.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Status DeleteCharacter(int userId, string name)
        {
            var guidQuery = "SELECT character_id FROM character_data WHERE user_id=@userId AND name=@name";
            var guid = string.Empty;

            using (var connection = new MySqlConnection(CharacterConnectionStr))
            {
                using (var command = new MySqlCommand(guidQuery))
                {
                    command.Parameters.AddWithValue("@userId", userId);
                    command.Parameters.AddWithValue("@name", name);
                    command.Connection = connection;
                    guid = FetchCharacterGUID(connection, command).Result;
                }
            }

            /** I have no real way of testing this at the moment. **/
            if (guid == string.Empty)
                return Status.Fatal;

            var tables = new string[]
            {
                "character_data",
                "character_location_data"
            };
            
            foreach (var table in tables)
            {
                var status = Status.OK;

                var deleteQuery = "DELETE FROM "+table+" WHERE character_id=@characterId";
                using (var connection = new MySqlConnection(CharacterConnectionStr))
                {
                    using (var command = new MySqlCommand(deleteQuery))
                    {
                        command.Parameters.AddWithValue("@characterId", guid);
                        command.Connection = connection;
                        status = ExecuteCommand(connection, command).Result;
                    }
                }

                if (status == Status.Fatal)
                    return status;
            }

            return Status.OK;
        }

        /// <summary>
        /// This method assumes the user does in-fact exist.
        /// Attempt to log the player in with the given password.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static Account TryLogin(string username, string password)
        {
            var account = new Account();
            var query = "SELECT * FROM account WHERE username=@username";

            using (var connection = new MySqlConnection(AuthenticationConnectionStr))
            {
                using (var command = new MySqlCommand(query))
                {
                    command.Parameters.AddWithValue("@username", username);
                    command.Connection = connection;
                    account = ExecuteAccountReader(connection, command).Result;
                }
            }

            if (account.Status == Account.LoginStatus.ServerError)
                return account;

            bool isPasswordCorrect = CryptoHelper.VerifyPassword(account.Password_Hashed, password);
            if (!isPasswordCorrect)
            {
                account.Status = Account.LoginStatus.Unknown;
                return account;
            }
            account.Status = Account.LoginStatus.LoggedIn;

            return account;
        }

        /// <summary>
        /// Fetch the account that the given sessionId belongs to.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public static Account FetchAccount(string sessionId)
        {
            var account = new Account();
            var query = "SELECT * FROM account WHERE session_id=@sessionId";

            using (var connection = new MySqlConnection(AuthenticationConnectionStr))
            {
                using (var command = new MySqlCommand(query))
                {
                    command.Parameters.AddWithValue("@sessionId", sessionId);
                    command.Connection = connection;
                    account = ExecuteAccountReader(connection, command).Result;
                }
            }

            if (account.Status == Account.LoginStatus.ServerError)
                return account;

            return account;
        }

        /// <summary>
        /// Fetch an account ID based on username.
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        private static int FetchAccountID(string username)
        {
            var id = -1;
            var query = "SELECT user_id FROM account WHERE username=@username";

            using (var connection = new MySqlConnection(AuthenticationConnectionStr))
            {
                using (var command = new MySqlCommand(query))
                {
                    command.Parameters.AddWithValue("@username", username);
                    command.Connection = connection;
                    id = ExecuteAccountIDReader(connection, command).Result;
                }
            }

            return id;
        }

        /// <summary>
        /// Update the online status of a player in the world.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="characterId"></param>
        public static void UpdateOnlineCharacter(int userId, string characterId)
        {
            var updateCharacterId = false;
            var query = string.Empty;
            if (characterId != string.Empty)
            {
                query = "UPDATE account_online SET character_id=@characterId, is_online=1 WHERE user_id=@userId";
                updateCharacterId = true;
            }
            else
                query = "UPDATE account_online SET character_id=NULL, is_online=0 WHERE user_id=@userId";

            using (var connection = new MySqlConnection(AuthenticationConnectionStr))
            {
                using (var command = new MySqlCommand(query))
                {
                    if (updateCharacterId)
                        command.Parameters.AddWithValue("@characterId", characterId);
                    command.Parameters.AddWithValue("@userId", userId);
                    command.Connection = connection;
                    var status = ExecuteCommand(connection, command).Result;
                }
            }
        }

        /// <summary>
        /// Update account sessionId.
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public static void UpdateSession(ref Account account, bool isNewSession)
        {
            if (account == null)
                return;

            var updateQuery = string.Empty;
            if (isNewSession)
                updateQuery = "UPDATE account SET session_id=@sessionId WHERE user_id=@userId";
            else
                updateQuery = "UPDATE account SET session_id=NULL WHERE user_id=@userId";
            var sessionId = Guid.NewGuid();
            var sessionUpdate = Status.OK;
            using (var connection = new MySqlConnection(AuthenticationConnectionStr))
            {
                using (var command = new MySqlCommand(updateQuery))
                {
                    if (isNewSession)
                        command.Parameters.AddWithValue("@sessionId", sessionId);
                    command.Parameters.AddWithValue("@userId", account.ID);
                    command.Connection = connection;
                    sessionUpdate = ExecuteCommand(connection, command).Result;
                }
            }

            if (sessionUpdate != Status.OK)
                account.Status = Account.LoginStatus.ServerError;
            else
                account.SessionID = sessionId;
        }

        /// <summary>
        /// Save character data.
        /// </summary>
        /// <param name="character"></param>
        public static void SaveCharacter(WorldCharacter character)
        {
            var query = "UPDATE character_location_data SET x=@x, y=@y, direction=@direction WHERE character_id=@characterId";
            using (var connection = new MySqlConnection(CharacterConnectionStr))
            {
                using (var command = new MySqlCommand(query))
                {
                    command.Parameters.AddWithValue("@x", character.Vector.X);
                    command.Parameters.AddWithValue("@y", character.Vector.Y);
                    command.Parameters.AddWithValue("@direction", (int)character.Vector.Direction);
                    command.Parameters.AddWithValue("@characterId", character.GUID);
                    command.Connection = connection;
                    var status = ExecuteCommand(connection, command).Result;
                }
            }
        }

        /// <summary>
        /// Reset all session id's.
        /// </summary>
        public static void ResetSessions()
        {
            var query = "UPDATE account SET session_id=NULL";
            using (var connection = new MySqlConnection(AuthenticationConnectionStr))
            {
                using (var command = new MySqlCommand(query))
                {
                    command.Connection = connection;
                    var status = ExecuteCommand(connection, command).Result;
                }
            }
        }

        /// <summary>
        /// Reset all online characters.
        /// </summary>
        public static void ResetOnlineCharacters()
        {
            var query = "UPDATE account_online SET character_id=NULL, is_online=0";
            using (var connection = new MySqlConnection(AuthenticationConnectionStr))
            {
                using (var command = new MySqlCommand(query))
                {
                    command.Connection = connection;
                    var status = ExecuteCommand(connection, command).Result;
                }
            }
        }
    }
}
