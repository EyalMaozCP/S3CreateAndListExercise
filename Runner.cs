using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;

namespace S3CreateAndList
{
    public class Runner
    {
        private enum Actions : int
        {
            Exit = 0,
            ListAllBuckets = 1,
            ListAllFilesInBucket = 2,
            SaveListToFile = 3,
            LoadListFromFile = 4,
            DeleteFileFromBucket = 5,
            UploadFileToBucket = 6,
            PrintActionSet = 9
        }

        private const string FILE_PATH = "List_Of_Files.json"; // Place it in the same folder of the exe.

        // TODO - check if this is ok and what other option we have (maybe env var)
        private static BasicAWSCredentials CREDENTIALS = new("BLA", "BLA");
        
        private static AmazonS3Client s3Client = new AmazonS3Client(CREDENTIALS, Amazon.RegionEndpoint.USWest2);
        private static List<S3Object> filesList = null;
        private const string ACTION_SET = "'1' - List all buckets of your S3\n" +
                              "'2' - List all files in specific bucket\n" +
                              "'3' - Save the files list as JSON in text file\n" +
                              "'4' - Load the data about the files from text file\n" +
                              "'5' - Choose the file from the list and delete it on S3\n" +
                              "'6' - Upload file to S3 bucket\n" +
                              "'0' - Exit";
        public static async Task Run()
        {

            Console.WriteLine(ACTION_SET);
            while (true)
            {
                Console.WriteLine("\nSelect Action ( '9' - Actions set):");
                if (!Enum.TryParse(Console.ReadLine(), out Actions action))
                {
                    Console.WriteLine("Please enter a valid option.");
                    continue;
                }
                switch (action)
                {
                    case Actions.ListAllBuckets:
                        await PrintListAllBuckets(s3Client);
                        break;
                    case Actions.ListAllFilesInBucket:
                        await PrintListAllFilesInBucket();
                        break;
                    case Actions.SaveListToFile:
                        PrintAndSaveListToFile();
                        break;
                    case Actions.LoadListFromFile:
                        LoadAndPrintListFromFile();
                        break;
                    case Actions.DeleteFileFromBucket:
                        await DeleteFileFromBucket();
                        break;
                    case Actions.UploadFileToBucket:
                        await UploadFileToBucket();
                        break;
                    case 0:
                        System.Environment.Exit(0);
                        break;
                    case Actions.PrintActionSet:
                        Console.WriteLine(ACTION_SET);
                        break;
                }
            }
        }

        private static async Task PrintListAllBuckets(AmazonS3Client s3Client)
        {
            ListBucketsResponse listBucketResponse;
            Console.WriteLine("\nGetting a list of your buckets...");
            listBucketResponse = await Repository.GetAllBuckets(s3Client);
            listBucketResponse.Buckets.ForEach((b) => { Console.WriteLine(b.BucketName); });
        }

        private static async Task PrintListAllFilesInBucket()
        {
            Console.WriteLine("Enter s3 bucket name:");
            var bucketName = Console.ReadLine();
            filesList = await Repository.GetFilesFromBucket(s3Client, bucketName);
            if (filesList == null)
            {
                Console.WriteLine("No bucket with such name");
                return;
            }
            if (filesList.Count == 0)
            {
                Console.WriteLine("Bucket is empty.");
                filesList = null;
                return;
            }
            Console.WriteLine("Files:");
            filesList.ForEach((x) => { Console.WriteLine(x.Key); });
        }

        private static void PrintAndSaveListToFile()
        {
            if (filesList == null)
            {
                Console.WriteLine("Files list is empty.");
                return;
            }
            string jsonString = JsonSerializer.Serialize(filesList, new JsonSerializerOptions() { WriteIndented = true });
            File.WriteAllText(FILE_PATH, jsonString);
            Console.WriteLine($"Files list save into: {AppDomain.CurrentDomain.BaseDirectory}{FILE_PATH}");
        }

        private static void LoadAndPrintListFromFile()
        {
            if (!File.Exists(FILE_PATH))
            {
                Console.WriteLine("File doesnt exist (try option 3 first)");
                return;

            }
            string jsoString = File.ReadAllText(FILE_PATH);
            try
            {
                filesList = JsonSerializer.Deserialize(jsoString, typeof(List<S3Object>)) as List<S3Object>;
            }
            catch (Exception exc)
            {
                Console.WriteLine($"Error occured while trying to parse file: {exc.Message}");
                return;
            }
            if (filesList == null)
            {
                Console.WriteLine("File is empty or in incorrect format");
                return;
            }
            Console.WriteLine($"Files loaded from: {AppDomain.CurrentDomain.BaseDirectory}{FILE_PATH}");
            filesList.ForEach((x) => { Console.WriteLine(x.Key); });
        }

        private static async Task DeleteFileFromBucket()
        {
            Console.WriteLine("Enter s3 bucket name:");
            var buckettName = Console.ReadLine();
            Console.WriteLine("Enter file name:");
            var fileName = Console.ReadLine();
            await Repository.DeleteFileFromS3Bucket(s3Client, buckettName, fileName);
        }

        private static async Task UploadFileToBucket()
        {
            Console.WriteLine("Enter s3 bucket name:");
            var bucketName = Console.ReadLine();
            Console.WriteLine("Enter file path:");
            var filePath = Console.ReadLine();
            if (!File.Exists(filePath))
            {
                Console.WriteLine("File does not exist");
                return;
            }
            bool res = await Repository.UploadFileFromS3Bucket(s3Client, bucketName, filePath);
            if (res)
            {
                Console.WriteLine("File uploaded successfully");
            }
            else
            {
                Console.WriteLine("Couldnt upload file");
            }
        }
    }
}