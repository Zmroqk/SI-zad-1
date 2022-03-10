// See https://aka.ms/new-console-template for more information
using SI_zad_1;
using SI_zad_1.Models;

List<StationCost>? stationsCost = Loader.LoadData<StationCost>("Data/easy_cost.json");
List<StationFlow>? stationsFlow = Loader.LoadData<StationFlow>("Data/easy_flow.json");
if(stationsCost == null || stationsFlow == null)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.Error.WriteLine("Loaded data is incorrect or is not available");
    Console.ForegroundColor = ConsoleColor.White;
}
else
{
    LearningManager manager = new LearningManager(stationsCost, stationsFlow);
    manager.Init(10, 3, 3, 9);
    for(int i = 0; i < 100; i++)
    {
        Console.WriteLine($"Epoch: {manager.Epoch} Results: {manager.ToString()}");
        manager.Evolve(LearningManager.SelectionMethod.Roulette, selectionCount: 1);
    }
}

