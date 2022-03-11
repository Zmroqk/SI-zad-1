// See https://aka.ms/new-console-template for more information
using SI_zad_1;
using SI_zad_1.Models;

List<StationCost>? stationsCost = Loader.LoadData<StationCost>("Data/hard_cost.json");
List<StationFlow>? stationsFlow = Loader.LoadData<StationFlow>("Data/hard_flow.json");
if(stationsCost == null || stationsFlow == null)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.Error.WriteLine("Loaded data is incorrect or is not available");
    Console.ForegroundColor = ConsoleColor.White;
}
else
{
    LearningManager manager = new LearningManager(stationsCost, stationsFlow);
    manager.Init(1000, 5, 6, 24);
    for(int i = 0; i < 100; i++)
    {
        Console.WriteLine(manager.ToString(i));
        manager.Evolve(LearningManager.SelectionMethod.Roulette, selectionCount: 1);
    }
    Console.ForegroundColor = ConsoleColor.Blue;
    Console.WriteLine($"Best specimen cost: {manager.ToStringBestSpecimen()}");
    Console.ForegroundColor = ConsoleColor.White;
}

