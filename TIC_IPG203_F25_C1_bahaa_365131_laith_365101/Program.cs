using System;
using System.Collections.Generic;
using System.Linq;
namespace HomeRepairCenter;

public interface IHomeAppliance
{
    string SerialNumber { get; }
    string Model { get; }
    string Status { get; }
    decimal CalculateRepairCost();
    void ChangeStatus(string newStatus);
}
public delegate void ApplianceStatusChangedHandler(
    HomeAppliance appliance, string oldStatus, string newStatus);

public abstract class HomeAppliance : IHomeAppliance
{
    private readonly string _serialNumber;
    private string _model;
    private string _status;

    public string SerialNumber => _serialNumber;

    public string Model
    {
        get => _model;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Model cannot be empty.");

            _model = value.Trim();
        }
    }

    public string Status => _status;

    public event ApplianceStatusChangedHandler? StatusChanged;
    public event EventHandler? RepairCompleted;
    public event EventHandler? DeliveryDelayed;

    protected HomeAppliance(string serialNumber, string model)
    {
        if (!InputValidator.IsValidSerialNumber(serialNumber))
            throw new ArgumentException("Invalid serial number.");

        _serialNumber = serialNumber.Trim();
        Model = model;
        _status = "ةنايصلا ديق";
    }

    public abstract decimal CalculateRepairCost();

    public void ChangeStatus(string newStatus)
    {
        if (string.IsNullOrWhiteSpace(newStatus))
            throw new ArgumentException("Status cannot be empty.");

        string oldStatus = _status;
        _status = newStatus.Trim();

        if (oldStatus != _status)
            StatusChanged?.Invoke(this, oldStatus, _status);

        if (_status == "حالصالا مت")
            RepairCompleted?.Invoke(this, EventArgs.Empty);

        if (_status == "رخأتم")
            DeliveryDelayed?.Invoke(this, EventArgs.Empty);
    }

    public override string ToString() =>
        $"{GetType().Name} | Serial: {SerialNumber} | Model: {Model} | " +
        $"Status: {Status} | Cost: {CalculateRepairCost():0.00}";
}
public class Refrigerator : HomeAppliance
{
    public bool HasIceMaker { get; }

    public Refrigerator(string serialNumber, string model, bool hasIceMaker)
        : base(serialNumber, model)
    {
        HasIceMaker = hasIceMaker;
    }

    public override decimal CalculateRepairCost()
        => HasIceMaker ? 85m : 65m;
}
public class WashingMachine : HomeAppliance
{
    public bool IsFrontLoad { get; }

    public WashingMachine(string serialNumber, string model, bool isFrontLoad)
        : base(serialNumber, model)
    {
        IsFrontLoad = isFrontLoad;
    }

    public override decimal CalculateRepairCost()
        => IsFrontLoad ? 75m : 55m;
}
public class AirConditioner : HomeAppliance
{
    public int CapacityBTU { get; }

    public AirConditioner(string serialNumber, string model, int capacityBTU)
        : base(serialNumber, model)
    {
        if (capacityBTU <= 0)
            throw new ArgumentOutOfRangeException(nameof(capacityBTU));

        CapacityBTU = capacityBTU;
    }

    public override decimal CalculateRepairCost()
        => CapacityBTU >= 18000 ? 110m : 90m;
}
public class ApplianceManager
{
    private readonly List<HomeAppliance> _appliances = new();

    private static readonly List<HomeAppliance> AllRegisteredAppliances = new();

    public static int RepairedDevicesCount =>
        AllRegisteredAppliances.Count(a => a.Status == "حالصالا مت");

    public IReadOnlyList<HomeAppliance> Appliances =>
        _appliances.AsReadOnly();

    public void AddAppliance(HomeAppliance appliance)
    {
        if (appliance is null)
            throw new ArgumentNullException(nameof(appliance));

        _appliances.Add(appliance);
        AllRegisteredAppliances.Add(appliance);

        appliance.StatusChanged += OnStatusChanged;
        appliance.RepairCompleted += OnRepairCompleted;
        appliance.DeliveryDelayed += OnDeliveryDelayed;
    }

    public decimal CalculateTotalRepairCost()
        => _appliances.Sum(a => a.CalculateRepairCost());

    public void DisplayAll()
    {
        foreach (HomeAppliance appliance in _appliances)
            Console.WriteLine(appliance);
    }

    public void DisplayByStatus(string status)
    {
        foreach (HomeAppliance appliance in _appliances
                     .Where(a => a.Status == status))
        {
            Console.WriteLine(appliance);
        }
    }

    private static void OnStatusChanged(
        HomeAppliance appliance, string oldStatus, string newStatus)
    {
        Console.WriteLine(
            $"Status changed: {appliance.SerialNumber}: {oldStatus} -> {newStatus}");
    }

    private static void OnRepairCompleted(object? sender, EventArgs e)
    {
        if (sender is HomeAppliance appliance)
            Console.WriteLine($"Repair completed: {appliance.SerialNumber}");
    }

    private static void OnDeliveryDelayed(object? sender, EventArgs e)
    {
        if (sender is HomeAppliance appliance)
            Console.WriteLine($"Delivery delayed: {appliance.SerialNumber}");
    }
}

public static class InputValidator
{
    public static bool IsValidSerialNumber(string serialNumber)
    {
        if (string.IsNullOrWhiteSpace(serialNumber))
            return false;

        string value = serialNumber.Trim();

        return value.Length >= 4 &&
               value.Length <= 20 &&
               value.All(char.IsLetterOrDigit);
    }

    public static bool IsValidDate(DateTime date)
        => date <= DateTime.Today.AddYears(5);
}
    class Program
    {
        static void Main(string[] args)
        {
            ApplianceManager manager = new();

            HomeAppliance fridge = new Refrigerator("FRG1001", "Samsung RT", true);
            HomeAppliance washer = new WashingMachine("WSH2002", "LG Front Load", true);
            HomeAppliance ac = new AirConditioner("AC3003", "Gree Split", 18000);

            manager.AddAppliance(fridge);
            manager.AddAppliance(washer);
            manager.AddAppliance(ac);

            Console.WriteLine("All appliances:");
            manager.DisplayAll();

            Console.WriteLine($"Total repair cost: {manager.CalculateTotalRepairCost():0.00}");

            fridge.ChangeStatus("حالصالا مت");
            washer.ChangeStatus("رخأتم");

            Console.WriteLine("Devices still under maintenance:");
            manager.DisplayByStatus("ةنايصلا ديق");

            Console.WriteLine($"Repaired devices count: {ApplianceManager.RepairedDevicesCount}");

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }