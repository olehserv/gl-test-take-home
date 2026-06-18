using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Domain;

namespace DesktopApp;

/// <summary>
/// View-model for the main window. Owns the measurement list and the refresh command;
/// all data access goes through <see cref="IMeasurementApiClient"/>. Errors are surfaced
/// in <see cref="Status"/> rather than swallowed.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly IMeasurementApiClient _client;

    [ObservableProperty]
    private string _status = "Ready";

    public ObservableCollection<Measurement> Measurements { get; } = [];

    public MainViewModel(IMeasurementApiClient client) => _client = client;

    [RelayCommand]
    private async Task RefreshAsync()
    {
        try
        {
            Status = "Loading…";

            var measurements = await _client.GetMeasurementsAsync(type: null);

            Measurements.Clear();
            foreach (var measurement in measurements)
                Measurements.Add(measurement);

            Status = $"Loaded {Measurements.Count} measurement(s).";
        }
        catch (Exception ex)
        {
            Status = $"Error: {ex.Message}";
        }
    }
}
