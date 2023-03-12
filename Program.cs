using System;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Azure.Devices;

namespace InvokeDeviceMethod
{
    internal class Program
    {
        private static ServiceClient serviceClient;
        private static JobClient jobClient;

        private static string connectionString = "HostName=ioth.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=5d1yUxfUejmMEV5r8u1vZeXmFZWyzEZ2/eFfJJ4aVkc=";
        private static string deviceId = "iot-dev1";

        private static async Task Main(string[] args)
        {
            Console.WriteLine("Scheduling..");
            await ScheduleMethodJob();
            Console.WriteLine("\nPress Enter to exit.");
            Console.ReadLine();
        }


        private static async Task ScheduleMethodJob()
        {
            
            jobClient = JobClient.CreateFromConnectionString(connectionString);

            string methodJobId = Guid.NewGuid().ToString();

            var methodName = "SetFlashlightState";

            var methodInvocation = new CloudToDeviceMethod(methodName)
            {
                ResponseTimeout = TimeSpan.FromSeconds(30),
            };
            methodInvocation.SetPayloadJson("\"On\"");

            JobResponse result = await jobClient.ScheduleDeviceMethodAsync(methodJobId,
                $"DeviceId IN ['{deviceId}']",
                methodInvocation,
                DateTime.UtcNow.AddSeconds(20),
                (long)TimeSpan.FromMinutes(2).TotalSeconds);

            Console.WriteLine("Started Method Job");

            MonitorJob(methodJobId).Wait();
            Console.WriteLine("Press ENTER to run the next job.");
            Console.ReadLine();
        }

        public static async Task MonitorJob(string jobId)
        {
            JobResponse result;
            do
            {
                result = await jobClient.GetJobAsync(jobId);
                Console.WriteLine("Job Status : " + result.Status.ToString());
                Thread.Sleep(2000);
            } while ((result.Status != JobStatus.Completed) &&
              (result.Status != JobStatus.Failed));
        }
    }
}
