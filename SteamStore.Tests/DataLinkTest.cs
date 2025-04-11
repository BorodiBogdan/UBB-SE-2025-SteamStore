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

        private const int NumberRowsEmptyTable = 0;

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
        public void DataLink_OpenConnection_DoesNotThrow()
        {
            dataLink.OpenConnection();
            Assert.True(true);
        }

        [Fact]
        public void DataLink_CloseConnection_DoesNotThrow()
        {
            dataLink.CloseConnection();
            Assert.True(true);
        }

        [Fact]
        public void ExecuteReader_NullParameters_ThrowsException()
        {
            var exception = Assert.Throws<Exception>(() =>
                dataLink.ExecuteReader(SqlConstants.GetUserGamesProcedure, null));
            Assert.Contains(ErrorExecuteReader, exception.Message);
        }

        [Fact]
        public void ExecuteReader_EmptyParameters_ThrowsException()
        {
            var exception = Assert.Throws<Exception>(() =>
                dataLink.ExecuteReader(SqlConstants.GetUserGamesProcedure, new SqlParameter[] { }));
            Assert.Contains(ErrorExecuteReader, exception.Message);
        }

        [Fact]
        public void ExecuteReader_InvalidParameters_ThrowsException()
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
        public void ExecuteReader_UnsupportedProcedure_ThrowsException()
        {
            var exception = Assert.Throws<Exception>(() =>
                dataLink.ExecuteReader(UnsupportedProcedure));
            Assert.Contains(ErrorExecuteReader, exception.Message);
        }

        [Fact]
        public void ExecuteNonQuery_UnsupportedProcedure_ThrowsException()
        {
            var exception = Assert.Throws<Exception>(() =>
                dataLink.ExecuteNonQuery(AnyProcedure));
            Assert.Contains(ErrorExecuteNonQuery, exception.Message);
        }

        [Fact]
        public void ExecuteScalar_UnsupportedProcedure_ThrowsException()
        {
            var exception = Assert.Throws<Exception>(() =>
                dataLink.ExecuteScalar<int>(AnyProcedure));
            Assert.Contains(ErrorExecuteScalar, exception.Message);
        }

        [Fact]
        public void ExecuteReader_GetUserById_ReturnsDataWithExpectedStructure()
        {
            var parameters = new SqlParameter[]
            {
                new SqlParameter(SqlConstants.UserIdParameterWithCapitalLetter, SqlDbType.Int) { Value = TestUserIdentifier }
            };
            DataTable dataTable = dataLink.ExecuteReader(SqlConstants.GetUserByIdProcedure, parameters);
            Assert.True(dataTable.Columns.Contains(SqlConstants.UserIdColumn));
        }

        [Fact]
        public void ExecuteReader_GetUserById_ReturnsDataWithValidContent()
        {
            var parameters = new SqlParameter[]
            {
                new SqlParameter(SqlConstants.UserIdParameterWithCapitalLetter, SqlDbType.Int) { Value = TestUserIdentifier }
            };
            DataTable dataTable = dataLink.ExecuteReader(SqlConstants.GetUserByIdProcedure, parameters);

            Assert.True(dataTable.Rows.Count > NumberRowsEmptyTable);

        }

        [Fact]
        public void ExecuteReader_getUserGames_ReturnsDataWithExpectedStructure()
        {
            var parameters = new SqlParameter[]
            {
                new SqlParameter(SqlConstants.UserIdentifierParameter, SqlDbType.Int) { Value = TestUserIdentifier }
            };
            DataTable dataTable = dataLink.ExecuteReader(SqlConstants.GetUserGamesProcedure, parameters);
            Assert.True(dataTable.Columns.Contains(SqlConstants.GAMEIDCOLUMN));
        }

        [Fact]
        public void ExecuteReader_getUserGames_ReturnsDataWithValidContent()
        {
            var parameters = new SqlParameter[]
            {
                new SqlParameter(SqlConstants.UserIdentifierParameter, SqlDbType.Int) { Value = TestUserIdentifier }
            };
            DataTable dataTable = dataLink.ExecuteReader(SqlConstants.GetUserGamesProcedure, parameters);

            Assert.True(dataTable.Rows.Count > NumberRowsEmptyTable);

        }

        [Fact]
        public void ExecuteReader_getAllTags_ReturnsDataWithExpectedStructure()
        {
            var parameters = new SqlParameter[] { };
            DataTable dataTable = dataLink.ExecuteReader(SqlConstants.GetAllTagsProcedure, parameters);
            Assert.True(dataTable.Columns.Contains(SqlConstants.TagIdColumn));
        }

        [Fact]
        public void ExecuteReader_getAllTags_ReturnsDataWithValidContent()
        {
            var parameters = new SqlParameter[] { };
            DataTable dataTable = dataLink.ExecuteReader(SqlConstants.GetAllTagsProcedure, parameters);

            Assert.True(dataTable.Rows.Count > NumberRowsEmptyTable);

        }

        [Fact]
        public void ExecuteReader_getGameTags_ReturnsDataWithExpectedStructure()
        {
            var parameters = new SqlParameter[]
            {
                new SqlParameter(SqlConstants.GameIdShortcutParameter, SqlDbType.Int) { Value = TestGameIdentifier }
            };
            DataTable dataTable = dataLink.ExecuteReader(SqlConstants.GetGameTagsProcedure, parameters);
            Assert.True(dataTable.Columns.Contains(SqlConstants.TagIdColumn));
        }

        [Fact]
        public void ExecuteReader_getGameTags_ReturnsDataWithValidContent()
        {
            var parameters = new SqlParameter[]
            {
                new SqlParameter(SqlConstants.GameIdShortcutParameter, SqlDbType.Int) { Value = TestGameIdentifier }
            };
            DataTable dataTable = dataLink.ExecuteReader(SqlConstants.GetGameTagsProcedure, parameters);

            Assert.True(dataTable.Rows.Count > NumberRowsEmptyTable);

        }

        [Fact]
        public void ExecuteReader_GetAllGames_ReturnsDataWithExpectedStructure()
        {
            var parameters = new SqlParameter[] { };
            DataTable dataTable = dataLink.ExecuteReader(SqlConstants.GetAllGamesProcedure, parameters);
            Assert.True(dataTable.Columns.Contains(SqlConstants.GAMEIDCOLUMN));
        }

        [Fact]
        public void ExecuteReader_GetAllGames_ReturnsDataWithValidContent()
        {
            var parameters = new SqlParameter[] { };
            DataTable dataTable = dataLink.ExecuteReader(SqlConstants.GetAllGamesProcedure, parameters);
            Assert.True(dataTable.Rows.Count > NumberRowsEmptyTable);

        }

        [Fact]
        public void ExecuteScalar_GetGameOwnerCount_StructureIsCorrect()
        {
            var parameters = new SqlParameter[]
            {
                new SqlParameter(SqlConstants.GameIdParameter, SqlDbType.Int) { Value = TestGameIdentifier }
            };
            int result = dataLink.ExecuteScalar<int>(GetGameOwnerCountProcedure, parameters);
            Assert.IsType<int>(result);
        }

        [Fact]
        public void ExecuteScalar_GetGameOwnerCount_ReturnsCorrectCount()
        {
            var parameters = new SqlParameter[]
            {
                new SqlParameter(SqlConstants.GameIdParameter, SqlDbType.Int) { Value = TestGameIdentifier }
            };
            int result = dataLink.ExecuteScalar<int>(GetGameOwnerCountProcedure, parameters);
            Assert.True(result >= GameOwnerCountComparisonValue);
        }

        [Fact]
        public void ExecuteScalar_GetAverageGameRating_ReturnsCorrectAverage()
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
