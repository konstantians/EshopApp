using System.Diagnostics;

namespace EshopApp.GatewayAPI.Tests.Utilities;
public class ProcessManagementService : IProcessManagementService
{
    private Process? gatewayApiProcess;
    private Process? authServiceProcess;
    private Process? dataServiceProcess;
    private Process? emailServiceProcess;
    private Process? transactionServiceProcess;
    private Process? mvcClientProcess;

    public void BuildAndRunApplication(bool startAuthService, bool startDataService, bool startEmailService, bool startTransactionService, bool startMvcClient)
    {
        // Get the solution directory with this convoluted way
        string solutionDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)!.Parent!.Parent!.Parent!.Parent!.FullName;
        string solutionPath = $"{solutionDirectory}\\EshopApp.sln";
        string gatewayApiPath = $"{solutionDirectory}\\EshopApp.GatewayAPI\\EshopApp.GatewayAPI.csproj";
        string authLibraryRestApiPath = $"{solutionDirectory}\\EshopApp.AuthLibraryAPI\\EshopApp.AuthLibraryAPI.csproj";
        string dataLibraryRestApiPath = $"{solutionDirectory}\\EshopApp.DataLibraryAPI\\EshopApp.DataLibraryAPI.csproj";
        string emailLibraryRestApiPath = $"{solutionDirectory}\\EshopApp.EmailLibraryAPI\\EshopApp.EmailLibraryAPI.csproj";
        string transactionLibraryRestApiPath = $"{solutionDirectory}\\EshopApp.TransactionLibraryAPI\\EshopApp.TransactionLibraryAPI.csproj";
        string mvcClientPath = $"{solutionDirectory}\\EshopApp.MVC\\EshopApp.MVC.csproj";

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
        gatewayApiProcess = Process.Start("dotnet", $"run --project \"{gatewayApiPath}\" --urls=https://localhost:7189");
        if (startAuthService)
            authServiceProcess = Process.Start("dotnet", $"run --project \"{authLibraryRestApiPath}\" --urls=https://localhost:7041");
        if (startDataService)
            dataServiceProcess = Process.Start("dotnet", $"run --project \"{dataLibraryRestApiPath}\" --urls=https://localhost:7087");
        if (startEmailService)
            emailServiceProcess = Process.Start("dotnet", $"run --project \"{emailLibraryRestApiPath}\" --urls=https://localhost:7105");
        if (startTransactionService)
            transactionServiceProcess = Process.Start("dotnet", $"run --project \"{transactionLibraryRestApiPath}\" --urls=https://localhost:7144");
        if (startMvcClient)
            mvcClientProcess = Process.Start("dotnet", $"run --project \"{mvcClientPath}\" --urls=https://localhost:7070");
    }

    public void TerminateApplication()
    {

        var processes = Process.GetProcesses();

        // Kill any additional processes, which for some reason keep existing after you kill the started processes
        foreach (Process process in processes)
        {
            if (process.ProcessName == "MSBuild" || process.ProcessName == "msedgewebview2" || process.ProcessName == "dotnet")
                process.Kill();
        }

        // kill the processes
        authServiceProcess?.Kill(true);
        gatewayApiProcess?.Kill(true);
        dataServiceProcess?.Kill(true);
        emailServiceProcess?.Kill(true);
        transactionServiceProcess?.Kill(true);
        mvcClientProcess?.Kill(true);

        authServiceProcess = null;
        gatewayApiProcess = null;
        dataServiceProcess = null;
        emailServiceProcess = null;
        transactionServiceProcess = null;
        mvcClientProcess = null;
    }
}
