﻿// This file was auto-generated by ML.NET Model Builder. 
using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
namespace QL_ChiTieu
{
    public partial class MLModel3
    {
        /// <summary>
        /// model input class for MLModel3.
        /// </summary>
        #region model input class
        public class ModelInput
        {
            [ColumnName(@"TransactionId")]
            public float TransactionId { get; set; }

            [ColumnName(@"CategoryId")]
            public float CategoryId { get; set; }

            [ColumnName(@"Amount")]
            public float Amount { get; set; }

            [ColumnName(@"Note")]
            public string Note { get; set; }

            [ColumnName(@"Date")]
            public DateTime Date { get; set; }

            [ColumnName(@"idUser")]
            public float IdUser { get; set; }

            [ColumnName(@"UseridUser")]
            public float UseridUser { get; set; }

        }

        #endregion

        /// <summary>
        /// model output class for MLModel3.
        /// </summary>
        #region model output class
        public class ModelOutput
        {
            [ColumnName(@"TransactionId")]
            public uint TransactionId { get; set; }

            [ColumnName(@"CategoryId")]
            public uint CategoryId { get; set; }

            [ColumnName(@"Amount")]
            public float Amount { get; set; }

            [ColumnName(@"Note")]
            public string Note { get; set; }

            [ColumnName(@"Date")]
            public DateTime Date { get; set; }

            [ColumnName(@"idUser")]
            public float IdUser { get; set; }

            [ColumnName(@"UseridUser")]
            public float UseridUser { get; set; }

            [ColumnName(@"Score")]
            public float Score { get; set; }

        }

        #endregion

        private static string MLNetModelPath = Path.GetFullPath("MLModel3.zip");

        public static readonly Lazy<PredictionEngine<ModelInput, ModelOutput>> PredictEngine = new Lazy<PredictionEngine<ModelInput, ModelOutput>>(() => CreatePredictEngine(), true);

        /// <summary>
        /// Use this method to predict on <see cref="ModelInput"/>.
        /// </summary>
        /// <param name="input">model input.</param>
        /// <returns><seealso cref=" ModelOutput"/></returns>
        public static ModelOutput Predict(ModelInput input)
        {
            var predEngine = PredictEngine.Value;
            return predEngine.Predict(input);
        }

        private static PredictionEngine<ModelInput, ModelOutput> CreatePredictEngine()
        {
            var mlContext = new MLContext();
            ITransformer mlModel = mlContext.Model.Load(MLNetModelPath, out var _);
            return mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(mlModel);
        }
    }
}
