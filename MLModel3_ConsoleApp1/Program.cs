﻿
// This file was auto-generated by ML.NET Model Builder. 

using MLModel3_ConsoleApp1;

// Create single instance of sample data from first line of dataset for model input
MLModel3.ModelInput sampleData = new MLModel3.ModelInput()
{
    TransactionId = 18F,
    CategoryId = 11F,
    Note = @"Lương tháng 12",
    IdUser = 1F,
};

// Make a single prediction on the sample data and print results
var predictionResult = MLModel3.Predict(sampleData);

Console.WriteLine("Using model to make single prediction -- Comparing actual Amount with predicted Amount from sample data...\n\n");


Console.WriteLine($"TransactionId: {18F}");
Console.WriteLine($"CategoryId: {11F}");
Console.WriteLine($"Amount: {1.5E+07F}");
Console.WriteLine($"Note: {@"Lương tháng 12"}");
Console.WriteLine($"IdUser: {1F}");


Console.WriteLine($"\n\nPredicted Amount: {predictionResult.Score}\n\n");
Console.WriteLine("=============== End of process, hit any key to finish ===============");
Console.ReadKey();

