// See https://aka.ms/new-console-template for more information
using MagSafeLockServer;
string Id = "1";
string prompt = "> ";
int pollTime = 100;
string localhost = "http://timer"; //point to smart timer either dns or IP
string webhost = ""; // Set to where you have your php hosted


PeriodicTimer CurrentTimer = null;

HttpClient httpClient = new();

Console.WriteLine("Timer Console (help for help)");
WritePrompt();
string? command = Console.ReadLine();
while (command != null && !command.ToLower().Equals("quit") && !command.ToLower().Equals("exit"))
{
    Parse(command);
    WritePrompt();
    command = Console.ReadLine();
}
Task.Run(async () =>
{
    if (!(await HttpGetCall(httpClient, $"{localhost}/setLock?t_state=0")).StartsWith("Error"))
        WriteOutput("Unlocked");
});

// Functions

async Task<string> HttpGetCall(HttpClient client, string Url)
{
    try
    {
        return await httpClient.GetStringAsync(Url);
    }
    catch (Exception e)
    {
        return $"Error: {e.Message}";
    }
}

void Parse(string command)
{
    var parts = command.ToLower().Split(" ");
    switch (parts[0].ToLower())
    {
        case "":
            break;
        case "help":
            WriteOutput("Commands:                          | Effect:", false);
            WriteOutput("-----------------------------------------------------------------------------------------", false);
            WriteOutput("new\t\t\t\t   | Creates a new id from webserver and sets id to it", false);
            WriteOutput("id [id]\t\t\t\t   | Get or set id", false);
            WriteOutput("localhost [host]\t\t   | Get or set localhost", false);
            WriteOutput("webhost [host]\t\t\t   | Get or set webhost", false);
            WriteOutput("id [id]\t\t\t\t   | Set or get id", false);
            WriteOutput("lock\t\t\t\t   | Locks the smarttimer", false);
            WriteOutput("unlock\t\t\t\t   | Unlocks the smarttimer", false);
            WriteOutput("test [local/web]\t\t   | Test if there is connection to local or web", false);
            WriteOutput("remote [timed/endless] [time] [id] | Start remote lock", false);
            WriteOutput("timer [time]\t\t\t   | Start timed lock for time minutes", false);
            WriteOutput("-----------------------------------------------------------------------------------------", false);
            break;
        case "new":
            Task.Run(async () =>
            {
                string result = await HttpGetCall(httpClient, $"{webhost}/server.php?new");
                if (!result.StartsWith("Error"))
                {
                    Id = result;
                    WriteOutput($"New ID set to {Id}");
                }
            });
            break;
        case "id":
            if (parts.Length == 1)
            {
                WriteOutput($"Id is {Id}", false);
            }
            if (parts.Length == 2)
            {
                Id = parts[1];
                WriteOutput($"Id set to {Id}", false);
            }
            break;
        case "lock":
            Task.Run(async () =>
            {
                string result = await HttpGetCall(httpClient, $"{localhost}/setLock?t_state=1&i_state=10000000");
                if (!result.StartsWith("Error"))
                    WriteOutput("Locked");
            });
            break;
        case "unlock":
            Task.Run(async () =>
            {
                string result = await HttpGetCall(httpClient, $"{localhost}/setLock?t_state=0");
                if (!result.StartsWith("Error"))
                WriteOutput("Unlocked");
            });
            break;
        case "test":
            if (parts[1].Equals("local"))
            {
                Task.Run(async () =>
                {
                    string response = await HttpGetCall(httpClient, $"{localhost}/isAlive");
                    if (response == "OK") WriteOutput($"Found SmartTimer at {localhost}");
                });
            };
            if (parts[1].Equals("web"))
            {
                Task.Run(async () =>
                {
                    string response = await HttpGetCall(httpClient, $"{webhost}/server.php?isAlive");
                    if (response == "OK") WriteOutput($"Found web server at {webhost}");
                });
            };
            break;
        case "remote":
            if (parts.Length > 1 && parts[1].Equals("timed"))
            {
                if (parts.Length == 2)
                    RemoteTimed("", 5);
                if (parts.Length == 3)
                    RemoteTimed(parts[2], 5);
                if (parts.Length == 4)
                {
                    Id = parts[3];
                    RemoteTimed(parts[2], 5);
                }
            }
            if (parts.Length == 2 && parts[1].Equals("endless"))
                RemoteEndless();

            break;
        case "timer":
            if (parts.Length == 1)
                Timed("", 5);
            if (parts.Length == 2)
                Timed(parts[1], 5);
            break;
        case "webhost":
            if (parts.Length == 1)
            {
                WriteOutput($"Web host set to: {webhost}", false);
            }
            if (parts.Length == 2)
            {
                webhost = parts[1];
            }
            break;
        case "localhost":
            if (parts.Length == 1)
            {
                WriteOutput($"Local host set to: {localhost}", false);
            }
            if (parts.Length == 2)
            {
                localhost = parts[1];
            }
            break;
        case "cancel":
            if (CurrentTimer != null)
                CurrentTimer.Dispose();
            break;
        default:
            WriteOutput("Unknown Command!", false);
            break;
    }
}

void WriteOutput(string output, Boolean prompt = true)
{
    var color = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine(output);
    Console.ForegroundColor = color;
    if (prompt) WritePrompt();
}

void WritePrompt()
{
    Console.Write(DateTime.Now.ToShortTimeString());
    Console.Write(" ");
    Console.Write(prompt);
}

void RemoteTimed(string duration, int defaultLength)
{
    string CurrentState = HttpGetCall(httpClient, $"{webhost}/server.php?id={Id}").Result;
    if (CurrentState.StartsWith("Error")) {
        WriteOutput(CurrentState, false);
        return;
    }

    int durationInt = int.TryParse(duration, out durationInt) ? durationInt : defaultLength;
    Timers timer = new Timers();
    CurrentTimer = timer.TimedTimer(TimeSpan.FromMilliseconds(pollTime), TimeSpan.FromMinutes(durationInt), async () =>
    {
        if (CurrentState == "1")
        {
            string result = await HttpGetCall(httpClient, $"{localhost}/setLock?t_state=1&i_state=10000000");
            if (result.StartsWith("Error"))
                WriteOutput(result);
        }
        else
        {
            string result = await HttpGetCall(httpClient, $"{localhost}/setLock?t_state=0");
            if (result.StartsWith("Error"))
                WriteOutput(result);
        }
        WriteOutput($"{webhost} Lock-id: {Id}");
    }, async () =>
    {
        string newState = await HttpGetCall(httpClient, $"{webhost}/server.php?id={Id}");

        if (CurrentState != newState)
        {
            CurrentState = newState;
            if (newState == "1")
            {
                string result = await HttpGetCall(httpClient, $"{localhost}/setLock?t_state=1&i_state=10000000");
            }
            else
            {
                string result = await HttpGetCall(httpClient, $"{localhost}/setLock?t_state=0");
            }
            WriteOutput($"changed state to {(newState == "0" ? "unlocked" : "locked")}");
        }
    }, async () =>
    {
        await HttpGetCall(httpClient, $"{localhost}/setLock?t_state=0");
        WriteOutput("Ended, unlocked");
    });
}

void RemoteEndless()
{
    Timers timer = new Timers();

    string CurrentState = "";
    CurrentTimer = timer.EndlessTimer(TimeSpan.FromMilliseconds(pollTime), async () =>
    {
        CurrentState = await HttpGetCall(httpClient, $"{webhost}/server.php?id={Id}");
        if (CurrentState == "1")
        {
            await HttpGetCall(httpClient, $"{localhost}/setLock?t_state=1&i_state=10000000");
        }
        else
        {
            await HttpGetCall(httpClient, $"{localhost}/setLock?t_state=0");
        }
        WriteOutput($"{webhost} Lock-id: {Id}");
    }, async () =>
    {
        string newState = await HttpGetCall(httpClient, $"{webhost}/server.php?id={Id}");

        if (CurrentState != newState)
        {
            CurrentState = newState;
            if (newState == "1")
            {
                await HttpGetCall(httpClient, $"{localhost}/setLock?t_state=1&i_state=10000000");
            }
            else
            {
                await HttpGetCall(httpClient, $"{localhost}/setLock?t_state=0");
            }
            WriteOutput($"changed state to {(newState == "0" ? "unlocked" : "locked")}");
        }
    }, async () =>
    {
        string result = await HttpGetCall(httpClient, $"{localhost}/setLock?t_state=0");
        WriteOutput("Ended, unlocked");
    });
}


void Timed(string duration, int defaultLength)
{
    Timers timer = new Timers();
    int durationInt = int.TryParse(duration, out durationInt) ? durationInt : defaultLength;

    CurrentTimer = timer.TimedTimer(TimeSpan.FromMilliseconds(pollTime), TimeSpan.FromMinutes(durationInt), async () =>
    {
        string result = await HttpGetCall(httpClient, $"{localhost}/setLock?t_state=1&i_state=10000000");
        WriteOutput("Locked");
    }, () =>
    {
    }, async () =>
    {
        string result = await HttpGetCall(httpClient, $"{localhost}/setLock?t_state=0");
        WriteOutput("Ended, unlocked");
    });
}