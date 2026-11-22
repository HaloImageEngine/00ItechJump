using DocumentFormat.OpenXml.Office.CustomXsn;
using DocumentFormat.OpenXml.Office.SpreadSheetML.Y2023.MsForms;
using DocumentFormat.OpenXml.Spreadsheet;
using EmpireOneRestAPIITJ.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EmpireOneRestAPIITJ.DataManager
{
    public class DataAccess
    {
        private readonly string _cs;

        public DataAccess()
        {
            // Read from Web.config <connectionStrings>
            var cs = ConfigurationManager.ConnectionStrings["ITechJumpDB"]?.ConnectionString;
            if (string.IsNullOrWhiteSpace(cs))
                throw new InvalidOperationException("Connection string 'ITechJumpDB' not found in Web.config.");

            _cs = cs;
        }

        // ---------- Helpers ----------
        private static string SafeStr(object o) =>
            o == null || o is DBNull ? null : o.ToString();

        private static int SafeInt(object o, int fallback = 0)
        {
            if (o == null || o is DBNull) return fallback;
            return int.TryParse(o.ToString(), out var v) ? v : fallback;
        }

        private static string[] SortedFive(int a, int b, int c, int d, int e)
        {
            var arr = new[] { a, b, c, d, e };
            Array.Sort(arr);
            return new[]
            {
                arr[0].ToString(),
                arr[1].ToString(),
                arr[2].ToString(),
                arr[3].ToString(),
                arr[4].ToString()
            };
        }

        // ---------- Queries (async) ----------

        public async Task<List<ModelDropDownCat>> Get_DropDownCatAsync(string cat, CancellationToken ct)
        {
            var results = new List<ModelDropDownCat>();

            using (var conn = new SqlConnection(_cs))
            using (var cmd = new SqlCommand("spGetDropDownCategories", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                cmd.Parameters.Add("@cat", SqlDbType.VarChar, 50)
                    .Value = (object)cat ?? DBNull.Value;

                await conn.OpenAsync(ct).ConfigureAwait(false);

                using (var rdr = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
                {
                    while (await rdr.ReadAsync(ct).ConfigureAwait(false))
                    {
                        // Assuming ordinal order: ID, Category, Description, Sequence
                        results.Add(new ModelDropDownCat
                        {
                            ID = SafeInt(rdr[0]),
                            Category = SafeStr(rdr[1]),
                            Description = SafeStr(rdr[2]),
                            Sequence = SafeStr(rdr[3])
                        });
                    }
                }
            }

            return results;
        }

        public async Task<List<ModelProductPlansGet>> Get_ProductPlansYearlyAsync(string BillingInterval, CancellationToken ct)
        {
            var results = new List<ModelProductPlansGet>();

            using (var conn = new SqlConnection(_cs))
            using (var cmd = new SqlCommand("spGetProductYearly", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                cmd.Parameters.Add("@BillingInterval", SqlDbType.VarChar, 10)
                    .Value = (object)BillingInterval ?? DBNull.Value;

                await conn.OpenAsync(ct).ConfigureAwait(false);

                using (var rdr = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
                {
                    // spGetProductYearly columns:
                    // ProductId, PlanCode, ProductName, ProductDescription,
                    // StripeProductId, StripePriceId, ProductTaxCode,
                    // Price, Currency, BillingInterval, IsActive

                    var oProductId = rdr.GetOrdinal("ProductId");
                    var oPlanCode = rdr.GetOrdinal("PlanCode");
                    var oProductName = rdr.GetOrdinal("ProductName");
                    var oProductDesc = rdr.GetOrdinal("ProductDescription");
                    var oStripeProductId = rdr.GetOrdinal("StripeProductId");
                    var oStripePriceId = rdr.GetOrdinal("StripePriceId");
                    var oProductTaxCode = rdr.GetOrdinal("ProductTaxCode");
                    var oPrice = rdr.GetOrdinal("Price");
                    var oCurrency = rdr.GetOrdinal("Currency");
                    var oBillingInterval = rdr.GetOrdinal("BillingInterval");
                    var oIsActive = rdr.GetOrdinal("IsActive");

                    while (await rdr.ReadAsync(ct).ConfigureAwait(false))
                    {
                        var item = new ModelProductPlansGet
                        {
                            ProductID = rdr.GetInt32(oProductId),
                            PlanCode = rdr.GetString(oPlanCode),
                            ProductName = rdr.GetString(oProductName),
                            ProductDescription = rdr.GetString(oProductDesc),
                            StripeProductId = rdr.GetString(oStripeProductId),
                            StripePriceId = rdr.GetString(oStripePriceId),
                            ProductTaxCode = rdr.GetString(oProductTaxCode),
                            Price = rdr.GetDecimal(oPrice),
                            Currency = rdr.GetString(oCurrency),
                            BillingInterval = rdr.GetString(oBillingInterval),
                            IsActive = rdr.GetBoolean(oIsActive)
                        };

                        results.Add(item);
                    }
                }
            }

            return results;
        }

        public async Task<List<ModelAudioQandA>> Get_AudioQandAbyQidAsync(int Qid, CancellationToken ct)
        {
            var results = new List<ModelAudioQandA>();

            using (var conn = new SqlConnection(_cs))
            using (var cmd = new SqlCommand("Get_Next_QandA_ByQid", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                await conn.OpenAsync(ct).ConfigureAwait(false);

                cmd.Parameters.Add("@Qid", SqlDbType.Int).Value = Qid;

                using (var rdr = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
                {
                    // Cache ordinals for speed & safety (must match SP column names)
                    var oQid = rdr.GetOrdinal("Qid");
                    var oQuestion = rdr.GetOrdinal("Question");
                    var oDescription = rdr.GetOrdinal("Description");
                    var oAudioLoc = rdr.GetOrdinal("AudioLoc");
                    var oDocLoc = rdr.GetOrdinal("DocLoc");
                    var oActive = rdr.GetOrdinal("Active");

                    while (await rdr.ReadAsync(ct).ConfigureAwait(false))
                    {
                        var item = new ModelAudioQandA
                        {
                            Qid = rdr.GetInt32(oQid),
                            Question = rdr.IsDBNull(oQuestion) ? null : rdr.GetString(oQuestion),
                            Description = rdr.IsDBNull(oDescription) ? null : rdr.GetString(oDescription),
                            AudioLoc = rdr.IsDBNull(oAudioLoc) ? null : rdr.GetString(oAudioLoc),
                            DocLoc = rdr.IsDBNull(oDocLoc) ? null : rdr.GetString(oDocLoc),
                            Active = !rdr.IsDBNull(oActive) && rdr.GetBoolean(oActive)
                        };

                        results.Add(item);
                    }
                }
            }

            return results;
        }


        public async Task<List<ModelQuestions>> Get_AllQuestionsAsync(CancellationToken ct)
        {
            var results = new List<ModelQuestions>();

            using (var conn = new SqlConnection(_cs))
            using (var cmd = new SqlCommand("spGetAllQuestion", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                await conn.OpenAsync(ct).ConfigureAwait(false);

                // If the SP requires an @id, keep this
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = 1;

                using (var rdr = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
                {
                    // Cache ordinals for speed & safety (must match SP column names)
                    int oQuestion = rdr.GetOrdinal("Question");
                    int oCategory = rdr.GetOrdinal("Category");
                    int oAnswer = rdr.GetOrdinal("Answer");
                    int oID = rdr.GetOrdinal("ID");

                    while (await rdr.ReadAsync(ct).ConfigureAwait(false))
                    {
                        var item = new ModelQuestions
                        {
                            ID = (oID >= 0 && !rdr.IsDBNull(oID)) ? rdr.GetInt32(oID) : 0,
                            Category = (oCategory >= 0 && !rdr.IsDBNull(oCategory)) ? rdr.GetString(oCategory) : "C6",
                            Question = (oQuestion >= 0 && !rdr.IsDBNull(oQuestion)) ? rdr.GetString(oQuestion) : "??",
                            Answer = (oAnswer >= 0 && !rdr.IsDBNull(oAnswer)) ? rdr.GetString(oAnswer) : "No Answer"
                        };

                        results.Add(item);
                    }
                }
            }

            return results;
        }

        public async Task<List<ModelQuestions>> Get_QuestionsbyCatAsync(string cat, CancellationToken ct)
        {
            var results = new List<ModelQuestions>();

            using (var conn = new SqlConnection(_cs))
            using (var cmd = new SqlCommand("spGetDropDownList", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                await conn.OpenAsync(ct).ConfigureAwait(false);

                cmd.Parameters.Add("@cat", SqlDbType.VarChar)
                    .Value = (object)cat ?? DBNull.Value;

                using (var rdr = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
                {
                    // Cache ordinals for speed & safety (must match SP column names)
                    int oQuestion = rdr.GetOrdinal("Question");
                    int oCategory = rdr.GetOrdinal("Category");
                    int oAnswer = rdr.GetOrdinal("Answer");
                    int oID = rdr.GetOrdinal("ID");

                    while (await rdr.ReadAsync(ct).ConfigureAwait(false))
                    {
                        var item = new ModelQuestions
                        {
                            ID = (oID >= 0 && !rdr.IsDBNull(oID)) ? rdr.GetInt32(oID) : 0,
                            Category = (oCategory >= 0 && !rdr.IsDBNull(oCategory)) ? rdr.GetString(oCategory) : "C6",
                            Question = (oQuestion >= 0 && !rdr.IsDBNull(oQuestion)) ? rdr.GetString(oQuestion) : "??",
                            Answer = (oAnswer >= 0 && !rdr.IsDBNull(oAnswer)) ? rdr.GetString(oAnswer) : "No Answer"
                        };

                        results.Add(item);
                    }
                }
            }

            return results;
        }

        public async Task<List<ModelQuestions>> Get_SearchByKeyword_Async(string keyword, CancellationToken ct)
        {
            var results = new List<ModelQuestions>();

            using (var conn = new SqlConnection(_cs))
            using (var cmd = new SqlCommand("spGetSearchbyKeyWord", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                await conn.OpenAsync(ct).ConfigureAwait(false);

                cmd.Parameters.Add("@Keyword", SqlDbType.VarChar)
                    .Value = (object)keyword ?? DBNull.Value;

                using (var rdr = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
                {
                    // Cache ordinals for speed & safety (must match SP column names)
                    int oQuestion = rdr.GetOrdinal("Question");
                    int oCategory = rdr.GetOrdinal("Category");
                    int oAnswer = rdr.GetOrdinal("Answer");
                    int oID = rdr.GetOrdinal("ID");

                    while (await rdr.ReadAsync(ct).ConfigureAwait(false))
                    {
                        var item = new ModelQuestions
                        {
                            ID = (oID >= 0 && !rdr.IsDBNull(oID)) ? rdr.GetInt32(oID) : 0,
                            Category = (oCategory >= 0 && !rdr.IsDBNull(oCategory)) ? rdr.GetString(oCategory) : "C6",
                            Question = (oQuestion >= 0 && !rdr.IsDBNull(oQuestion)) ? rdr.GetString(oQuestion) : "??",
                            Answer = (oAnswer >= 0 && !rdr.IsDBNull(oAnswer)) ? rdr.GetString(oAnswer) : "No Answer"
                        };

                        results.Add(item);
                    }
                }
            }

            return results;
        }

        public async Task<int> InsertQuestion(string cat, string question, string answer)
        {
            int newId = 0;


            var connDB = _cs;
            if (answer.Length > 0)
            {
                try
                {
                    using (SqlConnection sqlCon = new SqlConnection(connDB))
                    {
                        sqlCon.Open();

                        using (SqlCommand sql_cmnd = new SqlCommand("spInsertQuestion", sqlCon))
                        {
                            sql_cmnd.CommandType = CommandType.StoredProcedure;

                            // ✅ Input Parameters
                            sql_cmnd.Parameters.AddWithValue("@Category", cat);
                            sql_cmnd.Parameters.AddWithValue("@Question", question);
                            sql_cmnd.Parameters.AddWithValue("@Answer", answer);

                            // ✅ OUTPUT Parameter
                            SqlParameter outputIdParam = new SqlParameter
                            {
                                ParameterName = "@QuestionID",
                                SqlDbType = SqlDbType.Int,
                                Direction = ParameterDirection.Output
                            };

                            sql_cmnd.Parameters.Add(outputIdParam);

                            // Execute async
                            await sql_cmnd.ExecuteNonQueryAsync().ConfigureAwait(false);


                            // ✅ Read OUTPUT Value
                            if (outputIdParam.Value != DBNull.Value)
                            {
                                newId = Convert.ToInt32(outputIdParam.Value);
                            }
                        }
                    }
                }
                catch (SqlException ex)
                {
                    string logWriterErr = ex.Message;
                    // TODO: log properly if needed
                }
            }

            return newId;
        }

        public List<ModelKeyWord> GetKeyWordByQID(string QID)
        {
            var qDataList = new List<ModelKeyWord>();
            SqlDataReader rdr = null;
            var connDB = _cs;

            try
            {
                using (SqlConnection sqlCon = new SqlConnection(connDB))
                {
                    sqlCon.Open();

                    using (SqlCommand sql_cmnd = new SqlCommand("spGetKeywordByQID", sqlCon))
                    {
                        sql_cmnd.CommandType = CommandType.StoredProcedure;

                        // Correct parameter usage
                        sql_cmnd.Parameters.Add("@QID", SqlDbType.Int).Value = Convert.ToInt32(QID);

                        rdr = sql_cmnd.ExecuteReader();

                        while (rdr.Read())
                        {
                            // ✅ NEW instance each loop
                            ModelKeyWord qData = new ModelKeyWord();

                            // Use column indexes or names depending on your resultset
                            qData.Category = rdr[0].ToString();
                            qData.QuestionID = rdr[1].ToString();
                            qData.Keyword = rdr[2].ToString();

                            qDataList.Add(qData); // ✅ Add unique object each time
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                // TODO: Log exception as needed
                // logWriter = new("Error on GetKeyWordByQID: " + ex.Message, Globals.CurrentUser.Username);
            }
            finally
            {
                rdr?.Close();
            }

            return qDataList;
        }

        /// <summary>
        /// Get all test results for a given user id.
        /// </summary>
        public List<ModelQuestionsAnswersbyUserId> Get_TestResultbyUserIdList(string uid)
        {
            if (string.IsNullOrWhiteSpace(uid))
                throw new ArgumentException("User ID is required.", nameof(uid));

            if (!int.TryParse(uid, out var userId))
                throw new ArgumentException("User ID must be a valid integer.", nameof(uid));

            var results = new List<ModelQuestionsAnswersbyUserId>();
            var connDB = _cs;

            try
            {
                using (var sqlCon = new SqlConnection(connDB))
                using (var sqlCmd = new SqlCommand("Get_TestResultsByUserId", sqlCon))
                {
                    sqlCmd.CommandType = CommandType.StoredProcedure;
                    sqlCmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;

                    sqlCon.Open();

                    using (var rdr = sqlCmd.ExecuteReader())
                    {
                        // These must match the columns returned by Get_TestResultsByUserId
                        int oAnswerResponseId = rdr.GetOrdinal("AnswerResponseId");
                        int oUserId = rdr.GetOrdinal("UserId");
                        int oUserAlias = rdr.GetOrdinal("UserAlias");
                        int oQuestionId = rdr.GetOrdinal("QuestionId");
                        int oQuestionShort = rdr.GetOrdinal("QuestionShort");
                        int oAnswerShort = rdr.GetOrdinal("AnswerShort");
                        int oScore = rdr.GetOrdinal("Score");

                        while (rdr.Read())
                        {
                            var item = new ModelQuestionsAnswersbyUserId
                            {
                                AnswerResponseId = !rdr.IsDBNull(oAnswerResponseId) ? Convert.ToInt32(rdr[oAnswerResponseId]) : 0,
                                UserID = !rdr.IsDBNull(oUserId) ? Convert.ToInt32(rdr[oUserId]) : 0,
                                UserAlias = !rdr.IsDBNull(oUserAlias) ? rdr[oUserAlias].ToString() : null,
                                QuestionID = !rdr.IsDBNull(oQuestionId) ? Convert.ToInt32(rdr[oQuestionId]) : 0,
                                QuestionShort = !rdr.IsDBNull(oQuestionShort) ? rdr[oQuestionShort].ToString() : null,
                                AnswerShort = !rdr.IsDBNull(oAnswerShort) ? rdr[oAnswerShort].ToString() : null,
                                Score = !rdr.IsDBNull(oScore) ? rdr[oScore].ToString() : null
                            };

                            results.Add(item);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                string logWriterErr = ex.Message;
                // TODO: log or rethrow as needed
            }

            return results;
        }

        /// <summary>
        /// Get a single test result by TestId (AnswerResponse.Id).
        /// Maps the fields returned by [dbo].[Get_TestResultbyTestId].
        /// </summary>
        public ModelQuestionsAnswersbyTestId Get_TestResultbyTestId(string testId)
        {
            var connDB = _cs;

            try
            {
                using (var sqlCon = new SqlConnection(connDB))
                using (var sqlCmd = new SqlCommand("Get_TestResultbyTestId", sqlCon))
                {
                    sqlCmd.CommandType = CommandType.StoredProcedure;
                    sqlCmd.Parameters.Add("@TestId", SqlDbType.Int).Value = testId;

                    sqlCon.Open();

                    using (var rdr = sqlCmd.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        if (!rdr.Read())
                            return null;

                        // Match the stored proc columns exactly:
                        //  ar.Id            AS AnswerResponseId,
                        //  ar.UserId,
                        //  ui.UserAlias,
                        //  ar.QuestionId,
                        //  q.Question       AS QuestionFull,
                        //  ar.Answer        AS AnswerUser,
                        //  ar.Answer        AS AnswerFull,
                        //  ar.Score,
                        //  ar.ScoreDescription

                        int oAnswerResponseId = rdr.GetOrdinal("AnswerResponseId");
                        int oUserId = rdr.GetOrdinal("UserId");
                        int oUserAlias = rdr.GetOrdinal("UserAlias");
                        int oQuestionId = rdr.GetOrdinal("QuestionId");
                        int oQuestionFull = rdr.GetOrdinal("QuestionFull");
                        int oAnswerUser = rdr.GetOrdinal("AnswerUser");
                        int oAnswerFull = rdr.GetOrdinal("AnswerFull");
                        int oScore = rdr.GetOrdinal("Score");
                        int oScoreDescription = rdr.GetOrdinal("ScoreDescription");

                        var item = new ModelQuestionsAnswersbyTestId
                        {
                            AnswerResponseId = !rdr.IsDBNull(oAnswerResponseId) ? Convert.ToInt32(rdr[oAnswerResponseId]) : 0,
                            UserID = !rdr.IsDBNull(oUserId) ? Convert.ToInt32(rdr[oUserId]) : 0,

                            // Model has UserAlias as int, but SP returns a string.
                            // Try to parse; fall back to 0 if not numeric.
                            UserAlias = !rdr.IsDBNull(oUserAlias)
                                             ? (int.TryParse(rdr[oUserAlias].ToString(), out var ua) ? ua : 0)
                                             : 0,

                            QuestionID = !rdr.IsDBNull(oQuestionId) ? rdr[oQuestionId].ToString() : null,
                            QuestionFull = !rdr.IsDBNull(oQuestionFull) ? rdr[oQuestionFull].ToString() : null,
                            AnswerUser = !rdr.IsDBNull(oAnswerUser) ? rdr[oAnswerUser].ToString() : null,
                            AnswerFull = !rdr.IsDBNull(oAnswerFull) ? rdr[oAnswerFull].ToString() : null,

                            // Model Score is int; SP Score may be numeric or string -> Convert.ToInt32 with null guard
                            Score = !rdr.IsDBNull(oScore) && int.TryParse(rdr[oScore].ToString(), out var sc) ? sc : 0,
                            ScoreDesc = !rdr.IsDBNull(oScoreDescription) ? rdr[oScoreDescription].ToString() : null
                        };

                    

                        return item;
                    }
                }
            }
            catch (SqlException ex)
            {
                string logWriterErr = ex.Message;
                return null;
            }
        }
        public void InsertKeyWord(string cat, string questionid, string keyword)
        {

            SqlConnection conn = null;
            SqlDataReader rdr = null;

            var connDB = ConfigurationManager.ConnectionStrings["TestingDB"].ConnectionString;

            try
            {
                using (SqlConnection sqlCon = new SqlConnection(connDB))
                {
                    sqlCon.Open();

                    using (SqlCommand sql_cmnd = new SqlCommand("spInsertKeyWords", sqlCon))
                    {
                        sql_cmnd.CommandType = CommandType.StoredProcedure;

                        sql_cmnd.Parameters.AddWithValue("@category", SqlDbType.NVarChar).Value = cat;
                        sql_cmnd.Parameters.AddWithValue("@questionid", SqlDbType.NVarChar).Value = questionid;
                        sql_cmnd.Parameters.AddWithValue("@keyword", SqlDbType.NVarChar).Value = keyword;

                        sql_cmnd.ExecuteNonQuery();
                        sqlCon.Close();

                    }
                }
            }
            catch (SqlException ex)
            {

                string logWriterErr = ex.Message;
            }
            finally
            {
                conn?.Close();
                rdr?.Close();
            }
        }

        public async Task InsertKeyWordAsync(string cat, string questionid, string keyword)
        {
            var connDB = _cs;

            try
            {
                using (var sqlCon = new SqlConnection(connDB))
                {
                    await sqlCon.OpenAsync().ConfigureAwait(false);

                    using (var sql_cmnd = new SqlCommand("spInsertKeyWords", sqlCon))
                    {
                        sql_cmnd.CommandType = CommandType.StoredProcedure;

                        // Note: you probably meant to pass the values directly, not SqlDbType
                        sql_cmnd.Parameters.AddWithValue("@category", cat);
                        sql_cmnd.Parameters.AddWithValue("@questionid", questionid);
                        sql_cmnd.Parameters.AddWithValue("@keyword", keyword);

                        await sql_cmnd.ExecuteNonQueryAsync().ConfigureAwait(false);
                    }
                }
            }
            catch (SqlException ex)
            {
                string logWriterErr = ex.Message;
                // TODO: log properly if needed
            }
        }


        public async Task<ModelGradeReturn> InsertAnswerAsync(string trackid, string answer)
        {
            var connDB = _cs;

            // Initialize result
            var gradeResult = new ModelGradeReturn
            {
                QuestionID = trackid,
                KeywordList = new List<string>(),
                NumKeyWords = 0,
                Matches = 0,
                Grade = 0m
            };

            try
            {
                // 1) Save the answer (async)
                using (var sqlCon = new SqlConnection(connDB))
                {
                    await sqlCon.OpenAsync().ConfigureAwait(false);

                    using (var sql_cmnd = new SqlCommand("spInsertAnswerResponse", sqlCon))
                    {
                        sql_cmnd.CommandType = CommandType.StoredProcedure;

                        sql_cmnd.Parameters.AddWithValue("@trackid", trackid);
                        sql_cmnd.Parameters.AddWithValue("@answer", answer);

                        await sql_cmnd.ExecuteNonQueryAsync().ConfigureAwait(false);
                    }
                }

                // 2) Get the list of keywords for this question id (sync for now)
                var keywordModels = GetKeyWordByQID(trackid);



                if (keywordModels != null && keywordModels.Count > 0)
                {
                    var answerLower = answer.ToLowerInvariant();
                    int totalKeywords = keywordModels.Count;

                    // 2a) Build the full list of KeywordUnit from all keywords
                    var allKeywordUnits =
                        keywordModels
                            .Where(k => !string.IsNullOrWhiteSpace(k.Keyword))
                            .Select(k => new KeywordUnit
                            {
                                QuestionId = k.QuestionID,
                                Keyword = k.Keyword
                            })
                            .ToList();
                    var allKeywords =
                        keywordModels
                            .Where(k => !string.IsNullOrWhiteSpace(k.Keyword))
                            .Select(k => k.Keyword)
                            .ToList();

                    // 2b) Subset: matching keywords (case‑insensitive substring match)
                    var matchingKeywordUnits =
                        allKeywordUnits
                            .Where(ku => answerLower.Contains(ku.Keyword.ToLowerInvariant()))
                            .ToList();

                    int matchCount = matchingKeywordUnits.Count;

                    // 2c) Calculate percentage grade
                    decimal percentGrade = 0m;
                    if (totalKeywords > 0)
                    {
                        percentGrade = Math.Round((decimal)matchCount / totalKeywords * 100m);
                    }

                    // 2d) Populate GradeReturn with the FULL list of keywords
                    gradeResult.KeywordList = allKeywords;
                    gradeResult.NumKeyWords = totalKeywords;
                    gradeResult.Matches = matchCount;
                    gradeResult.Grade = percentGrade;
                }
            }
            catch (SqlException ex)
            {
                string logWriterErr = ex.Message;
                // TODO: log properly if needed
            }

            return gradeResult;
        }

        public async Task<ModelGradeReturn> InsertAnswerScoreAsync(string trackid, string answer, string userid, string category)
        {
            var connDB = _cs;
            string scoredescription = string.Empty;
            // Initialize result
            var gradeResult = new ModelGradeReturn
            {
                QuestionID = trackid,
                KeywordList = new List<string>(),
                NumKeyWords = 0,
                Matches = 0,
                Grade = 0m
            };

            try
            {
               

                // 2) Get the list of keywords for this question id (sync for now)
                var keywordModels = GetKeyWordByQID(trackid);



                if (keywordModels != null && keywordModels.Count > 0)
                {
                    var answerLower = answer.ToLowerInvariant();
                    int totalKeywords = keywordModels.Count;

                    // 2a) Build the full list of KeywordUnit from all keywords
                    var allKeywordUnits =
                        keywordModels
                            .Where(k => !string.IsNullOrWhiteSpace(k.Keyword))
                            .Select(k => new KeywordUnit
                            {
                                QuestionId = k.QuestionID,
                                Keyword = k.Keyword
                            })
                            .ToList();
                    var allKeywords =
                        keywordModels
                            .Where(k => !string.IsNullOrWhiteSpace(k.Keyword))
                            .Select(k => k.Keyword)
                            .ToList();

                    // 2b) Subset: matching keywords (case‑insensitive substring match)
                    var matchingKeywordUnits =
                        allKeywordUnits
                            .Where(ku => answerLower.Contains(ku.Keyword.ToLowerInvariant()))
                            .ToList();

                    int matchCount = matchingKeywordUnits.Count;

                    // 2c) Calculate percentage grade
                    decimal percentGrade = 0m;
                    if (totalKeywords > 0)
                    {
                        percentGrade = Math.Round((decimal)matchCount / totalKeywords * 100m);
                    }

                    // 2d) Populate GradeReturn with the FULL list of keywords
                    gradeResult.KeywordList = allKeywords;
                    gradeResult.NumKeyWords = totalKeywords;
                    gradeResult.Matches = matchCount;
                    gradeResult.Grade = percentGrade;

                    scoredescription = percentGrade.ToString() + " Keyword List "  + " Keyword Match Count " + matchCount.ToString();
                }

                // 1) Save the answer (async)
                using (var sqlCon = new SqlConnection(connDB))
                {
                    await sqlCon.OpenAsync().ConfigureAwait(false);

                    using (var sql_cmnd = new SqlCommand("spInsertAnswersScore", sqlCon))
                    {
                        sql_cmnd.CommandType = CommandType.StoredProcedure;


                        sql_cmnd.Parameters.AddWithValue("@userid", userid);
                        sql_cmnd.Parameters.AddWithValue("@questionid", trackid);
                        sql_cmnd.Parameters.AddWithValue("@category", category);
                        sql_cmnd.Parameters.AddWithValue("@answer", answer);
                        sql_cmnd.Parameters.AddWithValue("@score", gradeResult.Grade);
                        sql_cmnd.Parameters.AddWithValue("@scoredescription", scoredescription);

                        await sql_cmnd.ExecuteNonQueryAsync().ConfigureAwait(false);
                    }
                }
            }
            catch (SqlException ex)
            {
                string logWriterErr = ex.Message;
                // TODO: log properly if needed
            }

            return gradeResult;
        }


        public async Task<int> Insert_iTech_Card_QuestionAsync(
            string cat,
            string question,
            string answer,
            CancellationToken ct)
        {
            using (var conn = new SqlConnection(_cs))
            using (var cmd = new SqlCommand("Insert_iTech_Question", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                var now = DateTime.Now;

                // Keeping parameter mappings consistent with your original code/SP
                cmd.Parameters.Add("@userid", SqlDbType.NVarChar, 50)
                    .Value = (object)cat ?? DBNull.Value;

                cmd.Parameters.Add("@CreateDate", SqlDbType.DateTime)
                    .Value = now;

                cmd.Parameters.Add("@Question", SqlDbType.NVarChar, 100)
                    .Value = (object)question ?? DBNull.Value;

                cmd.Parameters.Add("@Answer", SqlDbType.NVarChar, 50)
                    .Value = (object)answer ?? DBNull.Value;

                await conn.OpenAsync(ct).ConfigureAwait(false);
                return await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            }
        }

        public async Task<List<ModelKeyWord>> Get_KeyWordByQIDAsync(string QID, CancellationToken ct = default)
        {
            var qDataList = new List<ModelKeyWord>();
            var connDB = _cs;

            try
            {
                using (var sqlCon = new SqlConnection(connDB))
                {
                    await sqlCon.OpenAsync(ct).ConfigureAwait(false);

                    using (var sql_cmnd = new SqlCommand("spGetKeywordByQID", sqlCon))
                    {
                        sql_cmnd.CommandType = CommandType.StoredProcedure;

                        // Correct parameter usage
                        sql_cmnd.Parameters.Add("@QID", SqlDbType.Int).Value = Convert.ToInt32(QID);

                        using (var rdr = await sql_cmnd.ExecuteReaderAsync(ct).ConfigureAwait(false))
                        {
                            while (await rdr.ReadAsync(ct).ConfigureAwait(false))
                            {
                                var qData = new ModelKeyWord
                                {
                                    Category = rdr[0].ToString(),
                                    QuestionID = rdr[1].ToString(),
                                    Keyword = rdr[2].ToString()
                                };

                                qDataList.Add(qData);
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                // TODO: Log exception as needed
                // logWriter = new("Error on GetKeyWordByQIDAsync: " + ex.Message, Globals.CurrentUser.Username);
            }

            return qDataList;
        }

            public async Task<ModelSubscriptionResponse> Get_SubscriptionInfobyAliasAsync(string UserAlias, CancellationToken ct = default)
            {
                ModelSubscriptionResponse result = null;
                var connDB = _cs;

                try
                {
                    using (var sqlCon = new SqlConnection(connDB))
                    {
                        await sqlCon.OpenAsync(ct).ConfigureAwait(false);

                        using (var sql_cmnd = new SqlCommand("Get_Subscription_ByAlias", sqlCon))
                        {
                            sql_cmnd.CommandType = CommandType.StoredProcedure;

                            sql_cmnd.Parameters.Add("@UserAlias", SqlDbType.VarChar, 8).Value = UserAlias;

                            using (var rdr = await sql_cmnd.ExecuteReaderAsync(CommandBehavior.SingleRow, ct).ConfigureAwait(false))
                            {
                                if (await rdr.ReadAsync(ct).ConfigureAwait(false))
                                {
                                    var oUserId = rdr.GetOrdinal("UserId");
                                    var oUserAlias = rdr.GetOrdinal("UserAlias");
                                    var oSubscriptionId = rdr.GetOrdinal("SubscriptionId");
                                    var oPlanCode = rdr.GetOrdinal("PlanCode");
                                    var oStatus = rdr.GetOrdinal("Status");
                                    var oAutoRenew = rdr.GetOrdinal("AutoRenew");
                                    var oCurrentPeriodStartUtc = rdr.GetOrdinal("CurrentPeriodStartUtc");
                                    var oCurrentPeriodEndUtc = rdr.GetOrdinal("CurrentPeriodEndUtc");
                                    var oTrialEndUtc = rdr.GetOrdinal("TrialEndUtc");
                                    var oPurchaseCurrency = rdr.GetOrdinal("PurchaseCurrency");
                                    var oPlanName = rdr.GetOrdinal("PlanName");
                                    var oPlanDescription = rdr.GetOrdinal("PlanDescription");
                                    var oPlanCurrentPrice = rdr.GetOrdinal("PlanCurrentPrice");
                                    var oBillingPeriod = rdr.GetOrdinal("BillingPeriod");
                                    var oPlanIsActive = rdr.GetOrdinal("PlanIsActive");

                                    result = new ModelSubscriptionResponse
                                    {
                                        UserId = rdr.GetInt32(oUserId),
                                        UserAlias = rdr.IsDBNull(oUserAlias) ? null : rdr.GetString(oUserAlias),
                                        SubscriptionId = rdr.GetInt32(oSubscriptionId),
                                        PlanCode = rdr.IsDBNull(oPlanCode) ? null : rdr.GetString(oPlanCode),
                                        Status = rdr.IsDBNull(oStatus) ? null : rdr.GetString(oStatus),
                                        AutoRenew = !rdr.IsDBNull(oAutoRenew) && rdr.GetBoolean(oAutoRenew),
                                        CurrentPeriodStartUtc = rdr.IsDBNull(oCurrentPeriodStartUtc) ? (DateTime?)null : rdr.GetDateTime(oCurrentPeriodStartUtc),
                                        CurrentPeriodEndUtc = rdr.IsDBNull(oCurrentPeriodEndUtc) ? (DateTime?)null : rdr.GetDateTime(oCurrentPeriodEndUtc),
                                        TrialEndUtc = rdr.IsDBNull(oTrialEndUtc) ? (DateTime?)null : rdr.GetDateTime(oTrialEndUtc),
                                        PurchaseCurrency = rdr.IsDBNull(oPurchaseCurrency) ? null : rdr.GetString(oPurchaseCurrency),
                                        PlanName = rdr.IsDBNull(oPlanName) ? null : rdr.GetString(oPlanName),
                                        PlanDescription = rdr.IsDBNull(oPlanDescription) ? null : rdr.GetString(oPlanDescription),
                                        PlanCurrentPrice = rdr.GetDecimal(oPlanCurrentPrice),
                                        BillingPeriod = rdr.IsDBNull(oBillingPeriod) ? null : rdr.GetString(oBillingPeriod),
                                        PlanIsActive = !rdr.IsDBNull(oPlanIsActive) && rdr.GetBoolean(oPlanIsActive)
                                    };
                                }
                            }
                        }
                    }
                }
                catch (SqlException ex)
                {
                    // TODO: Log exception as needed
                    // logWriter = new("Error on Get_SubscriptionInfobyAliasAsync: " + ex.Message, Globals.CurrentUser.Username);
                }

                return result;
            }


        /// <summary>
        /// Inserts a new log activity record and returns the Activity_Id.
        /// Overload with all parameters for flexibility.
        /// </summary>
        public async Task<int> InsertAuditLog(ModelLogActivity model)
        {
            int activityId = 0;
            var connDB = _cs;

            try
            {
                using (SqlConnection sqlCon = new SqlConnection(connDB))
                {
                    await sqlCon.OpenAsync().ConfigureAwait(false);

                    using (SqlCommand sql_cmnd = new SqlCommand("sp_InsertAuditLog", sqlCon))
                    {
                        sql_cmnd.CommandType = CommandType.StoredProcedure;

                        // Input Parameters
                        sql_cmnd.Parameters.AddWithValue("@Activity_Date",
                            model.Activity_Date.HasValue ? (object)model.Activity_Date.Value : DBNull.Value);

                        sql_cmnd.Parameters.AddWithValue("@UserAlias",
                            (object)model.UserAlias ?? DBNull.Value);

                        sql_cmnd.Parameters.AddWithValue("@UserId",
                            model.UserId.HasValue ? (object)model.UserId.Value : DBNull.Value);

                        sql_cmnd.Parameters.AddWithValue("@ActivityCode",
                            (object)model.ActivityCode ?? DBNull.Value);

                        sql_cmnd.Parameters.AddWithValue("@ActivityMessage",
                            (object)model.ActivityMessage ?? DBNull.Value);

                        sql_cmnd.Parameters.AddWithValue("@ProgramSend",
                            (object)model.ProgramSend ?? DBNull.Value);

                        sql_cmnd.Parameters.AddWithValue("@Pages",
                            (object)model.Pages ?? DBNull.Value);

                        sql_cmnd.Parameters.AddWithValue("@Site",
                            (object)model.Site ?? DBNull.Value);

                        // OUTPUT Parameter for Activity_Id (required by SP signature)
                        SqlParameter outputIdParam = new SqlParameter("@Activity_Id", SqlDbType.Int)
                        {
                            Direction = ParameterDirection.Output
                        };
                        sql_cmnd.Parameters.Add(outputIdParam);

                        // Execute and read result set (SP returns Activity_Id via SELECT)
                        using (SqlDataReader reader = await sql_cmnd.ExecuteReaderAsync().ConfigureAwait(false))
                        {
                            if (await reader.ReadAsync().ConfigureAwait(false))
                            {
                                // SP uses SELECT SCOPE_IDENTITY() AS Activity_Id
                                activityId = Convert.ToInt32(reader["Activity_Id"]);
                                model.Activity_Id = activityId;  // Update the model with the new ID
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                await LogErrorAsync(ex.Number.ToString(), ex.Message, "InsertAuditLog").ConfigureAwait(false);
                throw new InvalidOperationException($"Database error inserting log activity", ex);
            }
            catch (Exception ex)
            {
                await LogErrorAsync("Unexpected", ex.Message, "InsertAuditLog").ConfigureAwait(false);
                throw;
            }

            return activityId;
        }

        private async Task LogErrorAsync(string errorCode, string errorMessage, string programSend)
        {
            try
            {
                using (SqlConnection sqlCon = new SqlConnection(_cs))
                {
                    await sqlCon.OpenAsync().ConfigureAwait(false);

                    using (SqlCommand sql_cmnd = new SqlCommand("sp_InsertLogError", sqlCon))
                    {
                        sql_cmnd.CommandType = CommandType.StoredProcedure;

                        // Input Parameters
                        sql_cmnd.Parameters.AddWithValue("@ErrorDate", DateTime.UtcNow);
                        sql_cmnd.Parameters.AddWithValue("@ErrorCode", (object)errorCode ?? DBNull.Value);
                        sql_cmnd.Parameters.AddWithValue("@ErrorMessage", (object)errorMessage ?? DBNull.Value);
                        sql_cmnd.Parameters.AddWithValue("@ProgramSend", (object)programSend ?? DBNull.Value);

                        // Execute without reading result - fire and forget style for logging
                        await sql_cmnd.ExecuteNonQueryAsync().ConfigureAwait(false);
                    }
                }
            }
            catch
            {
                // Silently fail - don't throw exceptions from logging
                // Could write to Windows Event Log or trace as fallback
            }
        }


    }
}
