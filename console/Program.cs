// See https://aka.ms/new-console-template for more information
using IoTAgentLib;
using IoTAgentLib.Utils;

Console.WriteLine("Hello, IoT!");

IoTAgent agent = new IoTAgent();

foreach (string name in await Helpers.GetContainerNamesAsync())
{
    Console.WriteLine(name);
}

await Helpers.UploadBlobAsync("avg-temperature", "blobo", @"C:\Users\artur\source\repos\IoT2024\console\bin\Debug\net6.0\bvnbvnbv.txt");