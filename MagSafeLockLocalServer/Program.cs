// See https://aka.ms/new-console-template for more information
using MagSafeLockServer;
string Id = "1";
string prompt = "> ";

string localHost = "http://timer"; //point to smart timer either dns or IP
string webhost = ""; // Set to where you have your php hosted

HttpClient httpClient = new();

Console.WriteLine("Timer Console");
WritePrompt();
string? command = Console.ReadLine();
while(command != null && !command.ToLower().Equals("quit") && !command.ToLower().Equals("exit")) {
    Parse(command);
    WritePrompt();
    command = Console.ReadLine();
}

void Parse(string command) {
    var parts = command.Split(" ");
    switch (parts[0].ToLower())
    {
        case "":
            break;
        case "new":
            Task.Run(async () => { Id = await httpClient.GetStringAsync($"{webhost}/server.php?new"); WriteOutput($"New ID set to {Id}"); });
            break;
        case "setid": 
            if(parts.Length == 2) {
                Id = parts[1];
                WriteOutput($"New ID set to {Id}");
            }
            break;
        case "lock":
            Task.Run(async () => { await httpClient.GetStringAsync($"{localHost}/setLock?t_state=1&i_state=10000000"); WriteOutput("Locked"); });
            break;
        case "unlock":
            Task.Run(async () => { await httpClient.GetStringAsync($"{localHost}/setLock?t_state=0"); WriteOutput("Unlocked"); });
            break;
        case "remote":
            if(parts.Length == 1)
                RemoteTimed("", 5);
            if(parts.Length == 2) 
                RemoteTimed(parts[1], 5);
            if(parts.Length == 3) { 
                Id = parts[2];
                RemoteTimed(parts[1], 5);
            }
            break;
        case "timer":
            if(parts.Length == 1)
                Timed("", 5);
            if(parts.Length == 2) 
                Timed(parts[1], 5);
            break;
        default:
            Console.WriteLine("Unknown Command!");
            break;
    }
} 

void WriteOutput(string output) {
    var color = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine(output);
    Console.ForegroundColor = color;
    WritePrompt();
}

void WritePrompt() {
    Console.Write(DateTime.Now.ToShortTimeString());
    Console.Write(" ");
    Console.Write(prompt);
}

void RemoteTimed(string duration, int defaultLength) {
    Timers t = new Timers();

    int durationInt = int.TryParse(duration, out durationInt) ? durationInt : defaultLength;

    string CurrentState = "";
    
    t.TimedTimer(TimeSpan.FromMilliseconds(100), TimeSpan.FromMinutes(durationInt), async () => {
        CurrentState = await httpClient.GetStringAsync($"{webhost}/server.php?id={Id}");
        if(CurrentState=="1") {
            string result = await httpClient.GetStringAsync($"{localHost}/setLock?t_state=1&i_state=10000000");    
        } else {
            string result = await httpClient.GetStringAsync($"{localHost}/setLock?t_state=0");
        }
        WriteOutput($"{webhost} Lock-id: {Id}");
    }, async () => {
        string newState = await httpClient.GetStringAsync($"{webhost}/server.php?id={Id}");

        if(CurrentState != newState)
        {
            CurrentState = newState;
            if(newState=="1") {
                string result = await httpClient.GetStringAsync($"{localHost}/setLock?t_state=1&i_state=10000000");    
            } else {
                string result = await httpClient.GetStringAsync($"{localHost}/setLock?t_state=0");
            }
            WriteOutput($"changed state to {(newState=="0"?"unlocked":"locked")}");
        }
    }, async () => {
        string result = await httpClient.GetStringAsync($"{localHost}/setLock?t_state=0");
        WriteOutput("Ended, unlocked");
    });
}



void Timed(string duration, int defaultLength) {
    Timers t = new Timers();

    int durationInt = int.TryParse(duration, out durationInt) ? durationInt : defaultLength;

    t.TimedTimer(TimeSpan.FromMilliseconds(100), TimeSpan.FromMinutes(durationInt), async () => {
            string result = await httpClient.GetStringAsync($"{localHost}/setLock?t_state=1&i_state=10000000");    
            WriteOutput("Locked");
    }, () => {
    }, async () => {
        string result = await httpClient.GetStringAsync($"{localHost}/setLock?t_state=0");
        WriteOutput("Ended, unlocked");
    });
}