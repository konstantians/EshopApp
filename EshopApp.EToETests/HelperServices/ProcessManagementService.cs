using System.Diagnostics;

namespace EshopApp.EToETests.HelperServices;
public class ProcessManagementService : IProcessManagementService
{
    private Process? mvcProcess;
    private Process? gatewayApiProcess;
    private Process? authMicroserviceProcess;
    private Process? dataMicroserviceProcess;
    private Process? emailMicroserviceProcess;
    private Process? transactionMicroserviceProcess;

    public void BuildAndRunApplication()
    {
        // Get the solution directory with this convoluted way
        string solutionDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)!.Parent!.Parent!.Parent!.Parent!.FullName;
        string solutionPath = $"{solutionDirectory}\\EshopApp.sln";
        string mvcPath = $"{solutionDirectory}\\EshopApp.MVC\\EshopApp.MVC.csproj";
        string gatewayApiPath = $"{solutionDirectory}\\EshopApp.GatewayAPI\\EshopApp.GatewayAPI.csproj";
        string authMicroservicePath = $"{solutionDirectory}\\EshopApp.AuthLibraryAPI\\EshopApp.AuthLibraryAPI.csproj";
        string dataMicroservicePath = $"{solutionDirectory}\\EshopApp.DataLibraryAPI\\EshopApp.DataLibraryAPI.csproj";
        string emailMicroservicePath = $"{solutionDirectory}\\EshopApp.EmailLibraryAPI\\EshopApp.EmailLibraryAPI.csproj";
        string transactionMicroservicePath = $"{solutionDirectory}\\EshopApp.TransactionLibraryAPI\\EshopApp.TransactionLibraryAPI.csproj";

        // Build the solution
        var buildProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"build \"{solutionPath}\" --configuration Debug",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        buildProcess.OutputDataReceived += (sender, args) => Console.WriteLine(args.Data); // Consume standard output
        buildProcess.Start();
        buildProcess.BeginOutputReadLine(); // Begin asynchronously reading the standard output
        buildProcess.WaitForExit();

        // Start the web application
        mvcProcess = Process.Start("dotnet", $"run --project \"{mvcPath}\" --urls=https://localhost:7070");
        gatewayApiProcess = Process.Start("dotnet", $"run --project \"{gatewayApiPath}\" --urls=https://localhost:7255");
        authMicroserviceProcess = Process.Start("dotnet", $"run --project \"{authMicroservicePath}\" --urls=https://localhost:7041");
        dataMicroserviceProcess = Process.Start("dotnet", $"run --project \"{dataMicroservicePath}\" --urls=https://localhost:7087");
        emailMicroserviceProcess = Process.Start("dotnet", $"run --project \"{emailMicroservicePath}\" --urls=https://localhost:7105");
        transactionMicroserviceProcess = Process.Start("dotnet", $"run --project \"{transactionMicroservicePath}\" --urls=https://localhost:7144");
    }

    public void TerminateApplication()
    {

        var processes = Process.GetProcesses();

        // Kill any additional processes, which for some reason keep existing after you kill the started processes
        foreach (Process process in processes)
        {
            if (process.ProcessName == "MSBuild" || process.ProcessName == "msedgewebview2" ||
                process.ProcessName == "dotnet")
            {
                process.Kill();
            }
        }

        //kill the processes
        mvcProcess?.Kill(true);
        gatewayApiProcess?.Kill(true);
        authMicroserviceProcess?.Kill(true);
        dataMicroserviceProcess?.Kill(true);
        emailMicroserviceProcess?.Kill(true);
        transactionMicroserviceProcess?.Kill(true);

        mvcProcess = null;
        gatewayApiProcess = null;
        authMicroserviceProcess = null;
        emailMicroserviceProcess = null;
        transactionMicroserviceProcess = null;
    }
}
