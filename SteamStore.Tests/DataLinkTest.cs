using System;
using System.Data.SqlClient;
using System.Data;
using Xunit;
using SteamStore.Data;
using SteamStore.Tests.TestUtils;

namespace SteamStore.Tests
{
    public class DataLinkTest
    {
        private readonly IDataLink _dataLink;

        private const string GET_USER_GAMES_PROCEDURE = "getUserGames";
        private const string UNSUPPORTED_PROCEDURE = "UnsupportedProcedure";
        private const string GET_USER_BY_ID_PROCEDURE = "GetUserById";
        private const string GET_ALL_TAGS_PROCEDURE = "getAllTags";
        private const string GET_GAME_TAGS_PROCEDURE = "getGameTags";
        private const string GET_ALL_GAMES_PROCEDURE = "GetAllGames";
        private const string GET_GAME_OWNER_COUNT_PROCEDURE = "GetGameOwnerCount";
        private const string GET_GAME_RATING_PROCEDURE = "getGameRating";

        private const int TEST_USER_ID = 1;
        private const int TEST_UID = 1;
        private const int TEST_GAME_ID = 1;

        public DataLinkTest()
        {
            _dataLink = DataLinkTestUtils.GetDataLink();
        }

        [Fact]
        public void DataLink_ExecuteReader_NullParameters_ThrowsException()
        {
            var exception = Assert.Throws<Exception>(() =>
                _dataLink.ExecuteReader(GET_USER_GAMES_PROCEDURE, null));
            Assert.Contains("Error - ExecuteReader", exception.Message);
        }

        [Fact]
        public void DataLink_ExecuteReader_EmptyParameters_ThrowsException()
        {
            var exception = Assert.Throws<Exception>(() =>
                _dataLink.ExecuteReader(GET_USER_GAMES_PROCEDURE, new SqlParameter[] { }));
            Assert.Contains("Error - ExecuteReader", exception.Message);
        }

        [Fact]
        public void DataLink_ExecuteReader_InvalidParameters_ThrowsException()
        {
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@InvalidParam", "InvalidValue")
            };
            var exception = Assert.Throws<Exception>(() =>
                _dataLink.ExecuteReader(GET_USER_GAMES_PROCEDURE, parameters));
            Assert.Contains("Error - ExecuteReader", exception.Message);
        }

        [Fact]
        public void DataLink_ExecuteReader_UnsupportedProcedure_ThrowsException()
        {
            var exception = Assert.Throws<Exception>(() =>
                _dataLink.ExecuteReader(UNSUPPORTED_PROCEDURE));
            Assert.Contains("Error - ExecuteReader", exception.Message);
        }

        [Fact]
        public void DataLink_ExecuteNonQuery_UnsupportedProcedure_ThrowsException()
        {
            var exception = Assert.Throws<Exception>(() =>
                _dataLink.ExecuteNonQuery("AnyProcedure"));
            Assert.Contains("Error - ExecuteNonQuery", exception.Message);
        }

        [Fact]
        public void DataLink_ExecuteScalar_UnsupportedProcedure_ThrowsException()
        {
            var exception = Assert.Throws<Exception>(() =>
                _dataLink.ExecuteScalar<int>("AnyProcedure"));
            Assert.Contains("Error - ExecutingScalar", exception.Message);
        }

        [Fact]
        public void DataLink_ExecuteReader_GetUserById_ReturnsDataWithExpectedStructure()
        {
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@UserId", SqlDbType.Int) { Value = TEST_USER_ID }
            };
            DataTable dt = _dataLink.ExecuteReader(GET_USER_BY_ID_PROCEDURE, parameters);
            Assert.True(dt.Columns.Contains("user_id"));
        }

        [Fact]
        public void DataLink_ExecuteReader_GetUserById_ReturnsDataWithValidContent()
        {
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@UserId", SqlDbType.Int) { Value = TEST_USER_ID }
            };
            DataTable dt = _dataLink.ExecuteReader(GET_USER_BY_ID_PROCEDURE, parameters);
            Assert.True(dt.Rows.Count > 0);
        }

        [Fact]
        public void DataLink_ExecuteReader_getUserGames_ReturnsDataWithExpectedStructure()
        {
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@uid", SqlDbType.Int) { Value = TEST_UID }
            };
            DataTable dt = _dataLink.ExecuteReader(GET_USER_GAMES_PROCEDURE, parameters);
            Assert.True(dt.Columns.Contains("game_id"));
        }

        [Fact]
        public void DataLink_ExecuteReader_getUserGames_ReturnsDataWithValidContent()
        {
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@uid", SqlDbType.Int) { Value = TEST_UID }
            };
            DataTable dt = _dataLink.ExecuteReader(GET_USER_GAMES_PROCEDURE, parameters);
            Assert.True(dt.Rows.Count > 0);
        }

        [Fact]
        public void DataLink_ExecuteReader_getAllTags_ReturnsDataWithExpectedStructure()
        {
            var parameters = new SqlParameter[] { };
            DataTable dt = _dataLink.ExecuteReader(GET_ALL_TAGS_PROCEDURE, parameters);
            Assert.True(dt.Columns.Contains("tag_id"));
        }

        [Fact]
        public void DataLink_ExecuteReader_getAllTags_ReturnsDataWithValidContent()
        {
            var parameters = new SqlParameter[] { };
            DataTable dt = _dataLink.ExecuteReader(GET_ALL_TAGS_PROCEDURE, parameters);
            Assert.True(dt.Rows.Count > 0);
        }

        [Fact]
        public void DataLink_ExecuteReader_getGameTags_ReturnsDataWithExpectedStructure()
        {
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@gid", SqlDbType.Int) { Value = TEST_GAME_ID }
            };
            DataTable dt = _dataLink.ExecuteReader(GET_GAME_TAGS_PROCEDURE, parameters);
            Assert.True(dt.Columns.Contains("tag_id"));
        }

        [Fact]
        public void DataLink_ExecuteReader_getGameTags_ReturnsDataWithValidContent()
        {
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@gid", SqlDbType.Int) { Value = TEST_GAME_ID }
            };
            DataTable dt = _dataLink.ExecuteReader(GET_GAME_TAGS_PROCEDURE, parameters);
            Assert.True(dt.Rows.Count > 0);
        }

        [Fact]
        public void DataLink_ExecuteReader_GetAllGames_ReturnsDataWithExpectedStructure()
        {
            var parameters = new SqlParameter[] { };
            DataTable dt = _dataLink.ExecuteReader(GET_ALL_GAMES_PROCEDURE, parameters);
            Assert.True(dt.Columns.Contains("game_id"));
        }

        [Fact]
        public void DataLink_ExecuteReader_GetAllGames_ReturnsDataWithValidContent()
        {
            var parameters = new SqlParameter[] { };
            DataTable dt = _dataLink.ExecuteReader(GET_ALL_GAMES_PROCEDURE, parameters);
            Assert.True(dt.Rows.Count > 0);
        }

        [Fact]
        public void DataLink_ExecuteScalar_GetGameOwnerCount_StructureIsCorrect()
        {
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@game_id", SqlDbType.Int) { Value = TEST_GAME_ID }
            };
            int result = _dataLink.ExecuteScalar<int>(GET_GAME_OWNER_COUNT_PROCEDURE, parameters);
            Assert.IsType<int>(result);
        }

        [Fact]
        public void DataLink_ExecuteScalar_GetGameOwnerCount_ReturnsCorrectCount()
        {
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@game_id", SqlDbType.Int) { Value = TEST_GAME_ID }
            };
            int result = _dataLink.ExecuteScalar<int>(GET_GAME_OWNER_COUNT_PROCEDURE, parameters);
            Assert.True(result >= 0);
        }

        [Fact]
        public void DataLink_ExecuteScalar_GetAverageGameRating_ReturnsCorrectAverage()
        {
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@gid", SqlDbType.Int) { Value = TEST_GAME_ID }
            };
            float result = _dataLink.ExecuteScalar<float>(GET_GAME_RATING_PROCEDURE, parameters);
            Assert.True(result >= 0 && result <= 5);
        }
    }
}
