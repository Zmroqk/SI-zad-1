// See https://aka.ms/new-console-template for more information
using SI_zad_1;
using SI_zad_1.Models;
using OxyPlot;
using OxyPlot.Series;
using SI_zad_1.Components;

void Test(string type,
    int w,
    int h,
    int n,
    List<int> epochs,
    List<int> specimensCount,
    List<double> crossoverProbabilities,
    List<double> mutateProbabilities,
    List<int> selectionsCount,
    List<int> elitizmCounts,
    List<LearningManager.SelectionMethod> selectionMethods,
    string fileTitle,
    bool generate_chart = false,
    bool useElitizm = false
    )
{
    List<StationCost>? stationsCost = Loader.LoadData<StationCost>($"Data/{type}_cost.json");
    List<StationFlow>? stationsFlow = Loader.LoadData<StationFlow>($"Data/{type}_flow.json");
    if (stationsCost == null || stationsFlow == null)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine("Loaded data is incorrect or is not available");
        Console.ForegroundColor = ConsoleColor.White;
        return;
    }
    PlotModel plotModel = new PlotModel();
    PlotModel barPlotModel = new PlotModel();
    StreamWriter wr = new StreamWriter(File.OpenWrite($"csvs/{type}_{fileTitle}.csv"));
    wr.WriteLine("epochs|specimens|crossover probability|mutate probability|selection method|" +
        "tournament selections|use elitizm|elitizm count|best specimen|worst in last epoch|average in last epoch");
    IMutator mutator = new DefaultMutator();
    ICrossover crossover = new OnePointCrossover();
    foreach (int epochCount in epochs)
    {
        foreach (int specimenCount in specimensCount)
        {
            foreach (double crossoverProbability in crossoverProbabilities)
            {
                foreach (double mutateProbability in mutateProbabilities)
                {
                    foreach (int elitizmCount in elitizmCounts)
                    {
                        foreach (LearningManager.SelectionMethod selectionMethod in selectionMethods)
                        {
                            if (selectionMethod == LearningManager.SelectionMethod.Tournament)
                            {
                                foreach (int selectionCount in selectionsCount)
                                {
                                    ISelector selector = new TournamentSelection(stationsCost, stationsFlow, selectionCount);
                                    LearningManager manager = new LearningManager(stationsCost, stationsFlow, selector, mutator, crossover);
                                    manager.Init(specimenCount, w, h, n);
                                    for (int i = 0; i < epochCount; i++)
                                    {
                                        manager.Evolve(
                                            crossoverProbability: crossoverProbability,
                                            mutateProbability: mutateProbability,
                                            useElitizm: useElitizm,
                                            elitizmCount: elitizmCount);
                                    }
                                    if (generate_chart)
                                    {
                                        LineSeries series = new LineSeries()
                                        {
                                            Title = $"tournament_epoch_{epochCount}_specimen_{specimenCount}_crossover_{crossoverProbability}_" +
                                            $"mutate_{mutateProbability}_selection_{selectionCount}_elitizm_{elitizmCount}",
                                            MarkerType = MarkerType.Square,
                                            MarkerSize = 3d,
                                            LabelFormatString = "{1}",
                                            FontSize = 8d
                                        };
                                        BarSeries barSeries = new BarSeries()
                                        {
                                            Title = $"roulette_{epochCount}_specimen_{specimenCount}_crossover_{crossoverProbability}_mutate_{mutateProbability}" +
                                            $"_elitizm_{elitizmCount}",
                                            LabelFormatString = "{0}",
                                            FontSize = 8d
                                        };
                                        int index = 1;
                                        manager.BestSpecimens.ForEach(bs =>
                                        {
                                            series.Points.Add(new DataPoint(bs.epoch, bs.specimen.SpecimenCost(stationsCost, stationsFlow)));
                                        //BarItem barItem = new BarItem(bs.specimen.SpecimenCost(stationsCost, stationsFlow), index++);
                                        //barSeries.Items.Add(barItem);
                                    });
                                        plotModel.Series.Add(series);
                                        //barPlotModel.Series.Add(barSeries);
                                    }
                                    int worstSpecimen = manager.CurrentSpecimens.Max(sp => sp.SpecimenCost(stationsCost, stationsFlow));
                                    double average = manager.CurrentSpecimens.Average(sp => sp.SpecimenCost(stationsCost, stationsFlow));
                                    wr.WriteLine($"{epochCount}|{specimenCount}|{crossoverProbability}|{mutateProbability}|{selectionMethod}|" +
                                        $"{selectionCount}|{useElitizm}|{elitizmCount}|{manager.BestSpecimen.SpecimenCost(stationsCost, stationsFlow)}|" +
                                        $"{worstSpecimen}|{average.ToString("F")}");
                                }

                            }
                            else
                            {
                                ISelector selector = new RouletteSelection(stationsCost, stationsFlow);
                                LearningManager manager = new LearningManager(stationsCost, stationsFlow, selector, mutator, crossover);
                                manager.Init(specimenCount, w, h, n);
                                for (int i = 0; i < epochCount; i++)
                                {
                                    manager.Evolve(
                                        crossoverProbability: crossoverProbability,
                                        mutateProbability: mutateProbability,
                                        useElitizm: useElitizm,
                                        elitizmCount: elitizmCount
                                        );
                                }
                                if (generate_chart)
                                {
                                    LineSeries series = new LineSeries()
                                    {
                                        Title = $"roulette_{epochCount}_specimen_{specimenCount}_crossover_{crossoverProbability}_mutate_{mutateProbability}" +
                                        $"_elitizm_{elitizmCount}",
                                        MarkerType = MarkerType.Square,
                                        MarkerSize = 3d,
                                        LabelFormatString = "{1}",
                                        FontSize = 8d
                                    };
                                    BarSeries barSeries = new BarSeries()
                                    {
                                        Title = $"roulette_{epochCount}_specimen_{specimenCount}_crossover_{crossoverProbability}_mutate_{mutateProbability}" +
                                        $"_elitizm_{elitizmCount}",
                                        LabelFormatString = "{0}",
                                        FontSize = 8d
                                    };
                                    int index = 1;
                                    manager.BestSpecimens.ForEach(bs =>
                                    {
                                        series.Points.Add(new DataPoint(bs.epoch, bs.specimen.SpecimenCost(stationsCost, stationsFlow)));
                                    //BarItem barItem = new BarItem(bs.specimen.SpecimenCost(stationsCost, stationsFlow), index++);
                                    //barSeries.Items.Add(barItem);
                                });
                                    plotModel.Series.Add(series);
                                    //barPlotModel.Series.Add(barSeries);
                                }
                                int worstSpecimen = manager.CurrentSpecimens.Max(sp => sp.SpecimenCost(stationsCost, stationsFlow));
                                double average = manager.CurrentSpecimens.Average(sp => sp.SpecimenCost(stationsCost, stationsFlow));
                                wr.WriteLine($"{epochCount}|{specimenCount}|{crossoverProbability}|{mutateProbability}|{selectionMethod}|-|{useElitizm}|{elitizmCount}|" +
                                    $"{manager.BestSpecimen.SpecimenCost(stationsCost, stationsFlow)}|{worstSpecimen}|{average.ToString("F")}");
                            }
                        }
                    }
                }
            }
        }
    } // end massive foreach block
    if (generate_chart)
    {
        plotModel.Legends.Add(new OxyPlot.Legends.Legend()
        {
            LegendFontSize = 24d,
            LegendPosition = OxyPlot.Legends.LegendPosition.RightTop,
            LegendSymbolLength = 32d
        });
        barPlotModel.Legends.Add(new OxyPlot.Legends.Legend()
        {
            LegendFontSize = 24d,
            LegendPosition = OxyPlot.Legends.LegendPosition.LeftTop,
            LegendSymbolLength = 32d
        });
        plotModel.Background = OxyColors.White;
        barPlotModel.Background = OxyColors.White;
        OxyPlot.Core.Drawing.PngExporter.Export(plotModel, $"charts/{type}_{fileTitle}.png", 3840, 2160);
        //OxyPlot.Core.Drawing.PngExporter.Export(barPlotModel, $"charts/{type}_{fileTitle}_bar.png", 3840, 2160);
    }
    wr.Flush();
    wr.Close();
    Console.WriteLine(fileTitle);
}

var d = new DirectoryInfo("charts");
if (!d.Exists)
    d.Create();
var csvs = new DirectoryInfo("csvs");
if (!csvs.Exists)
    csvs.Create();

#region Easy
Test("easy", 3, 3, 9,
    new List<int>() { 10 },
    new List<int>() { 10, 50, 100 },
    new List<double>() { 0.2d, 0.4d },
    new List<double>() { 0.1d, 0.2d },
    new List<int>() { 1, 3, 5 },
    new List<int>() { 1 },
    new List<LearningManager.SelectionMethod>() {
        LearningManager.SelectionMethod.Roulette
    },
    "epochs_10_specimens_10-50-100_crossover_2d-4d_mutate_1d-2d_roulette",
    true
);
Test("easy", 3, 3, 9,
    new List<int>() { 100 },
    new List<int>() { 10, 50, 100 },
    new List<double>() { 0.2d, 0.4d },
    new List<double>() { 0.1d, 0.2d },
    new List<int>() { 1, 3, 5 },
     new List<int>() { 1 },
    new List<LearningManager.SelectionMethod>() {
        LearningManager.SelectionMethod.Roulette
    },
    "epochs_100_specimens_10-50-100_crossover_2d-4d_mutate_1d-2d_roulette",
    true
);
Test("easy", 3, 3, 9,
    new List<int>() { 10 },
    new List<int>() { 10, 50, 100 },
    new List<double>() { 0.4d },
    new List<double>() { 0.1d },
    new List<int>() { 1, 3, 5 },
     new List<int>() { 1 },
    new List<LearningManager.SelectionMethod>() {
        LearningManager.SelectionMethod.Tournament
    },
    "epochs_10_specimens_10-50-100_crossover_4d_mutate_1d_selection_1-3-5_tournament",
    true
);
Test("easy", 3, 3, 9,
    new List<int>() { 100 },
    new List<int>() { 10, 50, 100 },
    new List<double>() { 0.4d },
    new List<double>() { 0.1d },
    new List<int>() { 1, 5, 10 },
     new List<int>() { 1 },
    new List<LearningManager.SelectionMethod>() {
        LearningManager.SelectionMethod.Tournament
    },
    "epochs_100_specimens_10-50-100_crossover_4d_mutate_1d_selection_1-5-10_tournament",
    true
);

Test("easy", 3, 3, 9,
    new List<int>() { 100 },
    new List<int>() { 100 },
    new List<double>() { 0.2d, 0.4d, 0.6d },
    new List<double>() { 0.1d },
    new List<int>() { 5 },
     new List<int>() { 1 },
    new List<LearningManager.SelectionMethod>() {
        LearningManager.SelectionMethod.Tournament
    },
    "epochs_100_specimens_100_crossover_2d-4d-6d_mutate_1d_selection_5_tournament",
    true
);

Test("easy", 3, 3, 9,
    new List<int>() { 100 },
    new List<int>() { 100 },
    new List<double>() { 0.2d, 0.4d, 0.6d },
    new List<double>() { 0.1d },
    new List<int>() { 4 },
     new List<int>() { 1 },
    new List<LearningManager.SelectionMethod>() {
        LearningManager.SelectionMethod.Roulette
    },
    "epochs_100_specimens_10-50-100_crossover_4d_mutate_1d_roulette",
    true
);

Test("easy", 3, 3, 9,
    new List<int>() { 100 },
    new List<int>() { 100 },
    new List<double>() { 0.4d },
    new List<double>() { 0.1d, 0.2d, 0.3d },
    new List<int>() { 5 },
     new List<int>() { 1 },
    new List<LearningManager.SelectionMethod>() {
        LearningManager.SelectionMethod.Tournament
    },
    "epochs_100_specimens_100_crossover_4d_mutate_1d-2d-3d_selection_5_tournament",
    true
);

Test("easy", 3, 3, 9,
    new List<int>() { 100 },
    new List<int>() { 100 },
    new List<double>() { 0.4d },
    new List<double>() { 0.1d, 0.2d, 0.3d },
    new List<int>() { 4 },
     new List<int>() { 1 },
    new List<LearningManager.SelectionMethod>() {
        LearningManager.SelectionMethod.Roulette
    },
    "epochs_100_specimens_10-50-100_crossover_4d_mutate_1d-2d-3d_roulette",
    true
);

Test("easy", 3, 3, 9,
    new List<int>() { 100 },
    new List<int>() { 100 },
    new List<double>() { 0.4d },
    new List<double>() { 0.1d, 0.2d, 0.3d },
    new List<int>() { 5 },
     new List<int>() { 1, 5, 10 },
    new List<LearningManager.SelectionMethod>() {
        LearningManager.SelectionMethod.Tournament
    },
    "epochs_100_specimens_100_crossover_4d_mutate_1d-2d-3d_selection_5_tournament_useElitizm",
    true,
    useElitizm: true
);

Test("easy", 3, 3, 9,
    new List<int>() { 100 },
    new List<int>() { 100 },
    new List<double>() { 0.4d },
    new List<double>() { 0.1d, 0.2d, 0.3d },
    new List<int>() { 4 },
     new List<int>() { 1, 5, 10 },
    new List<LearningManager.SelectionMethod>() {
        LearningManager.SelectionMethod.Roulette
    },
    "epochs_100_specimens_10-50-100_crossover_4d_mutate_1d-2d-3d_roulette_useElitizm",
    true,
    useElitizm: true
);

Test("easy", 3, 3, 9,
    new List<int>() { 10, 20, 30, 50, 100 },
    new List<int>() { 10, 25, 50, 100 },
    new List<double>() { 0.2d, 0.4d, 0.6d },
    new List<double>() { 0.1d, 0.2d, 0.3d },
    new List<int>() { 1, 5, 10 },
    new List<int>() { 1 },
    new List<LearningManager.SelectionMethod>() {
        LearningManager.SelectionMethod.Tournament,
        LearningManager.SelectionMethod.Roulette
    },
    "epochs_10-20-30-50-100_specimens_10-25-50-100_crossover_2d-4d-6d_mutate_1d-2d-3d_selection_1-5-10_tournament-roulette",
    false
);
Test("easy", 3, 3, 9,
    new List<int>() { 10, 20, 30, 50, 100 },
    new List<int>() { 10, 25, 50, 100 },
    new List<double>() { 0.2d, 0.4d, 0.6d },
    new List<double>() { 0.1d, 0.2d, 0.3d },
    new List<int>() { 1, 5, 10 },
    new List<int>() { 1, 5, 10 },
    new List<LearningManager.SelectionMethod>() {
        LearningManager.SelectionMethod.Tournament,
        LearningManager.SelectionMethod.Roulette
    },
    "epochs_10-20-30-50-100_specimens_10-25-50-100_crossover_2d-4d-6d_mutate_1d-2d-3d_selection_1-5-10_tournament-roulette_useElitizm",
    false
);
# endregion Easy
#region Flat
Test("flat", 1, 12, 12,
    new List<int>() { 10 },
    new List<int>() { 10, 50, 100 },
    new List<double>() { 0.2d, 0.4d },
    new List<double>() { 0.1d, 0.2d },
    new List<int>() { 1, 2, 3, 4 },
     new List<int>() { 1 },
    new List<LearningManager.SelectionMethod>() {
        LearningManager.SelectionMethod.Roulette
    },
    "epochs_10_specimens_10-50-100_crossover_2d-4d_mutate_1d-2d_roulette",
    true
);
Test("flat", 1, 12, 12,
    new List<int>() { 100 },
    new List<int>() { 10, 50, 100 },
    new List<double>() { 0.2d, 0.4d },
    new List<double>() { 0.1d, 0.2d },
    new List<int>() { 1, 2, 3, 4 },
     new List<int>() { 1 },
    new List<LearningManager.SelectionMethod>() {
        LearningManager.SelectionMethod.Roulette
    },
    "epochs_100_specimens_10-50-100_crossover_2d-4d_mutate_1d-2d_roulette",
    true
);
Test("flat", 1, 12, 12,
    new List<int>() { 10 },
    new List<int>() { 10, 50, 100 },
    new List<double>() { 0.4d },
    new List<double>() { 0.1d },
    new List<int>() { 1, 2, 3, 4 },
     new List<int>() { 1 },
    new List<LearningManager.SelectionMethod>() {
        LearningManager.SelectionMethod.Tournament
    },
    "epochs_10_specimens_10-50-100_crossover_4d_mutate_1d_tournament",
    true
);
Test("flat", 1, 12, 12,
    new List<int>() { 100 },
    new List<int>() { 10, 50, 100 },
    new List<double>() { 0.4d },
    new List<double>() { 0.1d },
    new List<int>() { 1, 2, 3, 4 },
     new List<int>() { 1 },
    new List<LearningManager.SelectionMethod>() {
        LearningManager.SelectionMethod.Tournament
    },
    "epochs_100_specimens_10-50-100_crossover_4d_mutate_1d_tournament",
    true
);

Test("flat", 1, 12, 12,
    new List<int>() { 10, 20, 30, 50, 100 },
    new List<int>() { 10, 25, 50, 100 },
    new List<double>() { 0.2d, 0.4d, 0.6d },
    new List<double>() { 0.1d, 0.2d, 0.3d },
    new List<int>() { 1, 2, 3, 4 },
     new List<int>() { 1 },
    new List<LearningManager.SelectionMethod>() {
        LearningManager.SelectionMethod.Tournament,
        LearningManager.SelectionMethod.Roulette
    },
    "epochs_10-20-30-50-100_specimens_10-25-50-100_crossover_2d-4d-6d_mutate_1d-2d-3d_selection_1-2-3-4_tournament-roulette",
    false
);
#endregion Flat
#region Hard
Test("hard", 5, 6, 24,
    new List<int>() { 10 },
    new List<int>() { 10, 50, 100 },
    new List<double>() { 0.2d, 0.4d },
    new List<double>() { 0.1d, 0.2d },
    new List<int>() { 1, 3, 5 },
     new List<int>() { 1 },
    new List<LearningManager.SelectionMethod>() {
        LearningManager.SelectionMethod.Roulette
    },
    "epochs_10_specimens_10-50-100_crossover_2d-4d_mutate_1d-2d_roulette",
    true
);
Test("hard", 5, 6, 24,
    new List<int>() { 100 },
    new List<int>() { 10, 50, 100 },
    new List<double>() { 0.2d, 0.4d },
    new List<double>() { 0.1d, 0.2d },
    new List<int>() { 1, 10, 25 },
     new List<int>() { 1 },
    new List<LearningManager.SelectionMethod>() {
        LearningManager.SelectionMethod.Roulette
    },
    "epochs_100_specimens_10-50-100_crossover_2d-4d_mutate_1d-2d_roulette",
    true
);
Test("hard", 5, 6, 24,
    new List<int>() { 10 },
    new List<int>() { 10, 50, 100 },
    new List<double>() { 0.4d },
    new List<double>() { 0.1d },
    new List<int>() { 1, 5, 10 },
     new List<int>() { 1 },
    new List<LearningManager.SelectionMethod>() {
        LearningManager.SelectionMethod.Tournament
    },
    "epochs_10_specimens_10-50-100_crossover_4d_mutate_1d_selection_1-5-10_tournament",
    true
);
Test("hard", 5, 6, 24,
    new List<int>() { 100 },
    new List<int>() { 10, 50, 100 },
    new List<double>() { 0.4d },
    new List<double>() { 0.1d },
    new List<int>() { 1, 5, 10 },
     new List<int>() { 1 },
    new List<LearningManager.SelectionMethod>() {
        LearningManager.SelectionMethod.Tournament
    },
    "epochs_100_specimens_10-50-100_crossover_4d_mutate_1d_selection_1-5-10_tournament",
    true
);

Test("hard", 5, 6, 24,
    new List<int>() { 100 },
    new List<int>() { 100 },
    new List<double>() { 0.2d, 0.4d, 0.6d },
    new List<double>() { 0.1d },
    new List<int>() { 5 },
     new List<int>() { 1 },
    new List<LearningManager.SelectionMethod>() {
        LearningManager.SelectionMethod.Tournament
    },
    "epochs_100_specimens_10-50-100_crossover_4d_mutate_1d_selection_5_tournament",
    true
);

Test("hard", 5, 6, 24,
    new List<int>() { 100 },
    new List<int>() { 100 },
    new List<double>() { 0.2d, 0.4d, 0.6d },
    new List<double>() { 0.1d },
    new List<int>() { 10 },
     new List<int>() { 1 },
    new List<LearningManager.SelectionMethod>() {
        LearningManager.SelectionMethod.Roulette
    },
    "epochs_100_specimens_10-50-100_crossover_4d_mutate_1d_roulette",
    true
);

Test("hard", 5, 6, 24,
    new List<int>() { 100 },
    new List<int>() { 100 },
    new List<double>() { 0.4d },
    new List<double>() { 0.1d, 0.2d, 0.3d },
    new List<int>() { 5 },
     new List<int>() { 1 },
    new List<LearningManager.SelectionMethod>() {
        LearningManager.SelectionMethod.Tournament
    },
    "epochs_100_specimens_100_crossover_4d_mutate_1d-2d-3d_selection_5_tournament",
    true
);

Test("hard", 5, 6, 24,
    new List<int>() { 100 },
    new List<int>() { 100 },
    new List<double>() { 0.4d },
    new List<double>() { 0.1d, 0.2d, 0.3d },
    new List<int>() { 5 },
     new List<int>() { 1 },
    new List<LearningManager.SelectionMethod>() {
        LearningManager.SelectionMethod.Roulette
    },
    "epochs_100_specimens_10-50-100_crossover_4d_mutate_1d-2d-3d_roulette",
    true
);

Test("hard", 5, 6, 24,
    new List<int>() { 100 },
    new List<int>() { 100 },
    new List<double>() { 0.4d },
    new List<double>() { 0.1d, 0.2d, 0.3d },
    new List<int>() { 5 },
     new List<int>() { 1, 5, 10 },
    new List<LearningManager.SelectionMethod>() {
        LearningManager.SelectionMethod.Tournament
    },
    "epochs_100_specimens_100_crossover_4d_mutate_1d-2d-3d_selection_5_tournament_useElitizm",
    true,
    useElitizm: true
);

Test("hard", 5, 6, 24,
    new List<int>() { 100 },
    new List<int>() { 100 },
    new List<double>() { 0.4d },
    new List<double>() { 0.1d, 0.2d, 0.3d },
    new List<int>() { 5 },
     new List<int>() { 1, 5, 10 },
    new List<LearningManager.SelectionMethod>() {
        LearningManager.SelectionMethod.Roulette
    },
    "epochs_100_specimens_10-50-100_crossover_4d_mutate_1d-2d-3d_roulette_useElitizm",
    true,
    useElitizm: true
);

Test("hard", 5, 6, 24,
    new List<int>() { 10, 20, 30, 50, 100 },
    new List<int>() { 10, 25, 50, 100 },
    new List<double>() { 0.2d, 0.4d, 0.6d },
    new List<double>() { 0.1d, 0.2d, 0.3d },
    new List<int>() { 1, 5, 10 },
     new List<int>() { 1 },
    new List<LearningManager.SelectionMethod>() {
        LearningManager.SelectionMethod.Tournament,
        LearningManager.SelectionMethod.Roulette
    },
    "epochs_10-20-30-50-100_specimens_10-25-50-100_crossover_2d-4d-6d_mutate_1d-2d-3d_selection_1-5-10_tournament-roulette",
    false
);
Test("hard", 5, 6, 24,
    new List<int>() { 10, 20, 30, 50, 100 },
    new List<int>() { 10, 25, 50, 100 },
    new List<double>() { 0.2d, 0.4d, 0.6d },
    new List<double>() { 0.1d, 0.2d, 0.3d },
    new List<int>() { 1, 5, 10 },
     new List<int>() { 1, 5, 10 },
    new List<LearningManager.SelectionMethod>() {
        LearningManager.SelectionMethod.Tournament,
        LearningManager.SelectionMethod.Roulette
    },
    "epochs_10-20-30-50-100_specimens_10-25-50-100_crossover_2d-4d-6d_mutate_1d-2d-3d_selection_1-5-10_tournament-roulette_useElitizm",
    false,
    useElitizm: true
);
#endregion Hard