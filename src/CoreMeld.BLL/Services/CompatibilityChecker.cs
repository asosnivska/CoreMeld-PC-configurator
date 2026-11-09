using CoreMeld.Domain.Enums;
using CoreMeld.Domain.Models;

namespace CoreMeld.BLL.Services;

public class CompatibilityChecker: ICompatibilityChecker
{
    public bool IsSocketCompatible(Cpu cpu, Motherboard motherboard)
    {
        ArgumentNullException.ThrowIfNull(cpu);
        ArgumentNullException.ThrowIfNull(motherboard);

        return cpu.SocketId == motherboard.SocketId;
    }

    public bool IsRamCompatible(Ram ram, Motherboard motherboard)
    {
        ArgumentNullException.ThrowIfNull(ram);
        ArgumentNullException.ThrowIfNull(motherboard);

        return ram.RamMemoryType == motherboard.MemoryType && ram.ModuleCount <= motherboard.MemorySlots &&
               ram.CapacityGb <= motherboard.MaxMemoryGb;
    }

    public bool IsPowerConnectorCompatible(Gpu? gpu, PowerSupply powerSupply)
    {
        ArgumentNullException.ThrowIfNull(powerSupply);

        if (gpu is null)
        {
            return true;
        }

        if (gpu.PowerConnectorType is null || gpu.PowerConnectorCount is null)
        {
            throw new InvalidOperationException("Дані про роз'єми живлення відеокарти не заповнено.");
        }

        switch (gpu.PowerConnectorType.Value)
        {
            case PowerConnectorType.None:
                return true;

            case PowerConnectorType.Pin6:
            case PowerConnectorType.Pin8:
                if (powerSupply.Pcie8PinCount is null)
                {
                    throw new InvalidOperationException("Кількість PCIe-роз'ємів блока живлення не заповнено.");
                }

                return powerSupply.Pcie8PinCount.Value >= gpu.PowerConnectorCount.Value;

            case PowerConnectorType.Hpwr12V:
                if (powerSupply.Has12Vhpwr is null)
                {
                    throw new InvalidOperationException("Наявність 12VHPWR у блоці живлення не заповнено.");
                }

                return powerSupply.Has12Vhpwr.Value;

            default:
                throw new InvalidOperationException($"Невідомий тип роз'єму: {gpu.PowerConnectorType.Value}.");
        }
    }

    public bool IsCoolerCompatible(Cpu cpu, CpuCooler cooler)
    {
        ArgumentNullException.ThrowIfNull(cpu);
        ArgumentNullException.ThrowIfNull(cooler);

        return cpu.SocketId == cooler.SocketId && cooler.TdpRatingWatts >= cpu.TdpWatts;
    }
}