using System;
using System.Data.SqlClient;
using System.Data;
using Xunit;
using SteamStore.Data;
using SteamStore.Constants;
using SteamStore.Tests.TestUtils;

namespace SteamStore.Tests
{
    public class DataLinkTest
    {
        private readonly IDataLink dataLink;

        private const string UnsupportedProcedure = "UnsupportedProcedure";
        private const string GetGameOwnerCountProcedure = "GetGameOwnerCount";
        private const string AnyProcedure = "AnyProcedure";
        private const string InvalidParameter = "@InvalidParameter";
        private const string InvalidValue = "InvalidValue";
        private const string ErrorExecuteReader = "Error - ExecuteReader";
        private const string ErrorExecuteNonQuery = "Error - ExecuteNonQuery";
        private const string ErrorExecuteScalar = "Error - ExecutingScalar";
        private const int DataTableRowCountComparisonValue = 0;
        private const int GameOwnerCountComparisonValue = 0;
        private const int GameRatingComparisonMinimum = 0;
        private const int GameRatingComparisonMaximum = 5;
        private const int TestUserIdentifier = 1;
        private const int TestGameIdentifier = 1;

        public DataLinkTest()
        {
            dataLink = DataLinkTestUtils.GetDataLink();
        }

        [Fact]
        public void DataLink_ExecuteReader_NullParameters_ThrowsException()
        {
            var exception = Assert.Throws<Exception>(() =>
                dataLink.ExecuteReader(SqlConstants.GetUserGamesProcedure, null));
            Assert.Contains(ErrorExecuteReader, exception.Message);
        }

        [Fact]
        public void DataLink_ExecuteReader_EmptyParameters_ThrowsException()
        {
            var exception = Assert.Throws<Exception>(() =>
                dataLink.ExecuteReader(SqlConstants.GetUserGamesProcedure, new SqlParameter[] { }));
            Assert.Contains(ErrorExecuteReader, exception.Message);
        }

        [Fact]
        public void DataLink_ExecuteReader_InvalidParameters_ThrowsException()
        {
            var parameters = new SqlParameter[]
            {
                new SqlParameter(InvalidParameter, InvalidValue)
            };
            var exception = Assert.Throws<Exception>(() =>
                dataLink.ExecuteReader(SqlConstants.GetUserGamesProcedure, parameters));
            Assert.Contains(ErrorExecuteReader, exception.Message);
        }

        [Fact]
        public void DataLink_ExecuteReader_UnsupportedProcedure_ThrowsException()
        {
            var exception = Assert.Throws<Exception>(() =>
                dataLink.ExecuteReader(UnsupportedProcedure));
            Assert.Contains(ErrorExecuteReader, exception.Message);
        }

        [Fact]
        public void DataLink_ExecuteNonQuery_UnsupportedProcedure_ThrowsException()
        {
            var exception = Assert.Throws<Exception>(() =>
                dataLink.ExecuteNonQuery(AnyProcedure));
            Assert.Contains(ErrorExecuteNonQuery, exception.Message);
        }

        [Fact]
        public void DataLink_ExecuteScalar_UnsupportedProcedure_ThrowsException()
        {
            var exception = Assert.Throws<Exception>(() =>
                dataLink.ExecuteScalar<int>(AnyProcedure));
            Assert.Contains(ErrorExecuteScalar, exception.Message);
        }

        [Fact]
        public void DataLink_ExecuteReader_GetUserById_ReturnsDataWithExpectedStructure()
        {
            var parameters = new SqlParameter[]
            {
                new SqlParameter(SqlConstants.UserIdParameterWithCapitalLetter, SqlDbType.Int) { Value = TestUserIdentifier }
            };
            DataTable dt = dataLink.ExecuteReader(SqlConstants.GetUserByIdProcedure, parameters);
            Assert.True(dt.Columns.Contains(SqlConstants.UserIdColumn));
        }

        [Fact]
        public void DataLink_ExecuteReader_GetUserById_ReturnsDataWithValidContent()
        {
            var parameters = new SqlParameter[]
            {
                new SqlParameter(SqlConstants.UserIdParameterWithCapitalLetter, SqlDbType.Int) { Value = TestUserIdentifier }
            };
            DataTable dt = dataLink.ExecuteReader(SqlConstants.GetUserByIdProcedure, parameters);
            Assert.True(dt.Rows.Count > DataTableRowCountComparisonValue);
        }

        [Fact]
        public void DataLink_ExecuteReader_getUserGames_ReturnsDataWithExpectedStructure()
        {
            var parameters = new SqlParameter[]
            {
                new SqlParameter(SqlConstants.UserIdentifierParameter, SqlDbType.Int) { Value = TestUserIdentifier }
            };
            DataTable dt = dataLink.ExecuteReader(SqlConstants.GetUserGamesProcedure, parameters);
            Assert.True(dt.Columns.Contains(SqlConstants.GAMEIDCOLUMN));
        }

        [Fact]
        public void DataLink_ExecuteReader_getUserGames_ReturnsDataWithValidContent()
        {
            var parameters = new SqlParameter[]
            {
                new SqlParameter(SqlConstants.UserIdentifierParameter, SqlDbType.Int) { Value = TestUserIdentifier }
            };
            DataTable dt = dataLink.ExecuteReader(SqlConstants.GetUserGamesProcedure, parameters);
            Assert.True(dt.Rows.Count > DataTableRowCountComparisonValue);
        }

        [Fact]
        public void DataLink_ExecuteReader_getAllTags_ReturnsDataWithExpectedStructure()
        {
            var parameters = new SqlParameter[] { };
            DataTable dt = dataLink.ExecuteReader(SqlConstants.GetAllTagsProcedure, parameters);
            Assert.True(dt.Columns.Contains(SqlConstants.TagIdColumn));
        }

        [Fact]
        public void DataLink_ExecuteReader_getAllTags_ReturnsDataWithValidContent()
        {
            var parameters = new SqlParameter[] { };
            DataTable dt = dataLink.ExecuteReader(SqlConstants.GetAllTagsProcedure, parameters);
            Assert.True(dt.Rows.Count > DataTableRowCountComparisonValue);
        }

        [Fact]
        public void DataLink_ExecuteReader_getGameTags_ReturnsDataWithExpectedStructure()
        {
            var parameters = new SqlParameter[]
            {
                new SqlParameter(SqlConstants.GameIdShortcutParameter, SqlDbType.Int) { Value = TestGameIdentifier }
            };
            DataTable dt = dataLink.ExecuteReader(SqlConstants.GetGameTagsProcedure, parameters);
            Assert.True(dt.Columns.Contains(SqlConstants.TagIdColumn));
        }

        [Fact]
        public void DataLink_ExecuteReader_getGameTags_ReturnsDataWithValidContent()
        {
            var parameters = new SqlParameter[]
            {
                new SqlParameter(SqlConstants.GameIdShortcutParameter, SqlDbType.Int) { Value = TestGameIdentifier }
            };
            DataTable dt = dataLink.ExecuteReader(SqlConstants.GetGameTagsProcedure, parameters);
            Assert.True(dt.Rows.Count > DataTableRowCountComparisonValue);
        }

        [Fact]
        public void DataLink_ExecuteReader_GetAllGames_ReturnsDataWithExpectedStructure()
        {
            var parameters = new SqlParameter[] { };
            DataTable dt = dataLink.ExecuteReader(SqlConstants.GetAllGamesProcedure, parameters);
            Assert.True(dt.Columns.Contains(SqlConstants.GAMEIDCOLUMN));
        }

        [Fact]
        public void DataLink_ExecuteReader_GetAllGames_ReturnsDataWithValidContent()
        {
            var parameters = new SqlParameter[] { };
            DataTable dt = dataLink.ExecuteReader(SqlConstants.GetAllGamesProcedure, parameters);
            Assert.True(dt.Rows.Count > DataTableRowCountComparisonValue);
        }

        [Fact]
        public void DataLink_ExecuteScalar_GetGameOwnerCount_StructureIsCorrect()
        {
            var parameters = new SqlParameter[]
            {
                new SqlParameter(SqlConstants.GameIdParameter, SqlDbType.Int) { Value = TestGameIdentifier }
            };
            int result = dataLink.ExecuteScalar<int>(GetGameOwnerCountProcedure, parameters);
            Assert.IsType<int>(result);
        }

        [Fact]
        public void DataLink_ExecuteScalar_GetGameOwnerCount_ReturnsCorrectCount()
        {
            var parameters = new SqlParameter[]
            {
                new SqlParameter(SqlConstants.GameIdParameter, SqlDbType.Int) { Value = TestGameIdentifier }
            };
            int result = dataLink.ExecuteScalar<int>(GetGameOwnerCountProcedure, parameters);
            Assert.True(result >= GameOwnerCountComparisonValue);
        }

        [Fact]
        public void DataLink_ExecuteScalar_GetAverageGameRating_ReturnsCorrectAverage()
        {
            var parameters = new SqlParameter[]
            {
                new SqlParameter(SqlConstants.GameIdShortcutParameter, SqlDbType.Int) { Value = TestGameIdentifier }
            };
            float result = dataLink.ExecuteScalar<float>(SqlConstants.GetGameRatingProcedure, parameters);
            Assert.True(result >= GameRatingComparisonMinimum && result <= GameRatingComparisonMaximum);
        }
    }
}
