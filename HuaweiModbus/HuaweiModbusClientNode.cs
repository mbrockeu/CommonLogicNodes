using EasyModbus;
using LogicModule.Nodes.Helpers;
using LogicModule.ObjectModel;
using LogicModule.ObjectModel.TypeSystem;
using System;
using System.Threading;
using static LogicNodesModbusHelper.LogicNodesModbus;

namespace marcus_brock_mbrock_eu.logic.HuaweiModbusClientNode
{
    public class HuaweiModbus : LogicNodeBase
    {
        #region configuration
        [Parameter(DisplayOrder = 1, InitOrder = 1, IsDefaultShown = false)]
        public IntValueObject TimeSpan { get; private set; }

        [Parameter(DisplayOrder = 2, InitOrder = 2, IsDefaultShown = false)]
        public StringValueObject ModbusHost { get; private set; }
        [Parameter(DisplayOrder = 3, InitOrder = 3, IsDefaultShown = false)]
        public IntValueObject ModbusPort { get; private set; }
        [Parameter(DisplayOrder = 4, InitOrder = 4, IsDefaultShown = false)]
        public IntValueObject Timeout { get; private set; }
        [Parameter(DisplayOrder = 5, InitOrder = 5, IsDefaultShown = false)]
        public ByteValueObject UnitIdentifier { get; private set; }
        #endregion

        #region inputs
        [Input(DisplayOrder = 6, IsInput = true, IsRequired = false)]
        public DoubleValueObject chargePowerMax { get; private set; }

        [Input(DisplayOrder = 7, IsInput = true, IsRequired = false)]
        public DoubleValueObject dischargePowerMax { get; private set; }

        [Input(DisplayOrder = 8, IsInput = true, IsRequired = false)]
        public DoubleValueObject chargingCutoff { get; private set; }

        [Input(DisplayOrder = 9, IsInput = true, IsRequired = false)]
        public DoubleValueObject dischargingCutoff { get; private set; }
        [Input(DisplayOrder =10,IsInput =true,IsRequired =false)]
        public DoubleValueObject workingMode { get; private set; }
        #endregion

        #region outputs
        [Output]
        public DoubleValueObject currentPVPower { get; private set; }

        [Output]
        public DoubleValueObject currentACPower { get; private set; }

        [Output]
        public DoubleValueObject currentGridPower { get; private set; }

        [Output]
        public DoubleValueObject currentBatteryPower { get; private set; }

        [Output]
        public DoubleValueObject todayPVEnergy { get; private set; }

        [Output]
        public DoubleValueObject totalPVEnergy { get; private set; }

        [Output]
        public DoubleValueObject inverterTemperature { get; private set; }

        [Output]
        public DoubleValueObject mppt1Voltage { get; private set; }

        [Output]
        public DoubleValueObject mppt1Current { get; private set; }

        [Output]
        public DoubleValueObject mppt2Voltage { get; private set; }

        [Output]
        public DoubleValueObject mppt2Current { get; private set; }

        [Output]
        public DoubleValueObject totalGridImportedEnergy { get; private set; }

        [Output]
        public DoubleValueObject totalGridExportedEnergy { get; private set; }

        [Output]
        public DoubleValueObject currentBatterySOC { get; private set; }

        [Output]
        public DoubleValueObject todaysPeakPVPower { get; private set; }

        [Output]
        public DoubleValueObject currentReactivePower { get; private set; }

        [Output]
        public DoubleValueObject currentBatteryStatus { get; private set; }

        [Output]
        public DoubleValueObject todayBatteryChargedEnergy { get; private set; }

        [Output]
        public DoubleValueObject todayBatteryDischargedEnergy { get; private set; }

        [Output]
        public DoubleValueObject batteryTemperature { get; private set; }

        [Output]
        public StringValueObject ErrorMessage { get; private set; }

        #endregion
        private ISchedulerService SchedulerService;

        public HuaweiModbus(INodeContext context)
        {
            context.ThrowIfNull("context");
            ITypeService typeService = context.GetService<ITypeService>();

            this.TimeSpan = typeService.CreateInt(PortTypes.Integer, "Abfrageinterval", 60);
            this.ModbusHost = typeService.CreateString(PortTypes.String, "Modbus TCP Host");
            this.ModbusPort = typeService.CreateInt(PortTypes.Integer, "Port", 502);
            this.Timeout = typeService.CreateInt(PortTypes.Integer, "Timeout", 7000);
            this.UnitIdentifier = typeService.CreateByte(PortTypes.Byte, "UnitIdentifier", 1);

            this.chargePowerMax = typeService.CreateDouble(PortTypes.Number, "Max. battery charge power (W)",5000);
            this.dischargePowerMax = typeService.CreateDouble(PortTypes.Number, "Max. battery discharge power (W)",5000);
            this.chargingCutoff = typeService.CreateDouble(PortTypes.Number, "Charging cutoff capacity (%)",100);
            this.dischargingCutoff = typeService.CreateDouble(PortTypes.Number, "Discharging cutoff capacity (%)",0);
            this.workingMode = typeService.CreateDouble(PortTypes.Number, "Working mode Settings (2 - Max; 5 TOU)",2);

            this.currentPVPower = typeService.CreateDouble(PortTypes.Number, "Current PV power (inverter)");
            this.currentACPower = typeService.CreateDouble(PortTypes.Number, "Current AC power (inverter)");
            this.currentGridPower = typeService.CreateDouble(PortTypes.Number, "Current grid power (smartmeter)");
            this.currentBatteryPower = typeService.CreateDouble(PortTypes.Number, "Current battery power (inverter)");
            this.todayPVEnergy = typeService.CreateDouble(PortTypes.Number, "Today PV energy");
            this.totalPVEnergy = typeService.CreateDouble(PortTypes.Number, "Total PV energy");
            this.inverterTemperature = typeService.CreateDouble(PortTypes.Number, "Inverter temperature");
            this.mppt1Voltage = typeService.CreateDouble(PortTypes.Number, "MPPT 1 voltage");
            this.mppt1Current = typeService.CreateDouble(PortTypes.Number, "MPPT 1 current");
            this.mppt2Voltage = typeService.CreateDouble(PortTypes.Number, "MPPT 2 voltage");
            this.mppt2Current = typeService.CreateDouble(PortTypes.Number, "MPPT 2 current");
            this.totalGridImportedEnergy = typeService.CreateDouble(PortTypes.Number, "Total energy imported (smartmeter)");
            this.totalGridExportedEnergy = typeService.CreateDouble(PortTypes.Number, "Total energy exported (smartmeter)");
            this.currentBatterySOC = typeService.CreateDouble(PortTypes.Number, "Current battery SoC");
            this.todaysPeakPVPower = typeService.CreateDouble(PortTypes.Number, "Today PV peak power");
            this.currentReactivePower = typeService.CreateDouble(PortTypes.Number, "Current reactive power");
            this.currentBatteryStatus = typeService.CreateDouble(PortTypes.Number, "Current battery status");
            this.todayBatteryChargedEnergy = typeService.CreateDouble(PortTypes.Number, "Today battery charged energy");
            this.todayBatteryDischargedEnergy = typeService.CreateDouble(PortTypes.Number, "Today battery discharged engergy");
            this.batteryTemperature = typeService.CreateDouble(PortTypes.Number, "Battery temperature");

            this.ErrorMessage = typeService.CreateString(PortTypes.String, "RAW / Error");
            SchedulerService = context.GetService<ISchedulerService>();
        }
        public override void Startup()
        {
            this.SchedulerService.InvokeIn(new TimeSpan(0, 0, TimeSpan.Value), FetchFromModbusServer);
        }

        public override void Execute()
        {
            var returnMessage = string.Empty;
            ModbusClient modbusClient = null;
            try
            {
                modbusClient = new ModbusClient(this.ModbusHost, this.ModbusPort);
                modbusClient.ConnectionTimeout = Timeout.Value;
                modbusClient.Connect();
                System.Threading.Thread.Sleep(700);

                modbusClient.UnitIdentifier = UnitIdentifier.Value;

                if ((this.chargePowerMax.HasValue && this.chargePowerMax.WasSet))
                {
                    writeRegister(modbusClient, 47075, (int)this.chargePowerMax.Value, DataTypeEnum.INT32, ref returnMessage);
                }
                if ((this.dischargePowerMax.HasValue && this.dischargePowerMax.WasSet))
                {
                    writeRegister(modbusClient, 47077, (int)this.dischargePowerMax.Value, DataTypeEnum.INT32, ref returnMessage);
                }
                if ((this.chargingCutoff.HasValue && this.chargingCutoff.WasSet))
                {
                    writeRegister(modbusClient, 47081, (int)this.chargingCutoff.Value, DataTypeEnum.INT16_UNSIGNED, ref returnMessage);
                }
                if ((this.dischargingCutoff.HasValue && this.dischargingCutoff.WasSet))
                {
                    writeRegister(modbusClient, 47082, (int)this.dischargingCutoff.Value, DataTypeEnum.INT16_UNSIGNED, ref returnMessage);
                }
                if ((this.workingMode.HasValue && this.workingMode.WasSet))
                {
                    writeRegister(modbusClient, 47086, (int)this.workingMode.Value, DataTypeEnum.INT16_UNSIGNED, ref returnMessage);
                }
                ErrorMessage.Value = returnMessage;
            }catch(Exception e)
            {
                ErrorMessage.Value = e.Message;
            }
            finally
            {
                if (modbusClient != null)
                    modbusClient.Disconnect();

                ErrorMessage.Value += "Write complete";
            }
        }

        private void FetchFromModbusServer()
        {
            Thread thread1 = new Thread(FetchFromModbusServerAsync);
            thread1.Start();
        }

        private void FetchFromModbusServerAsync()
        {
            var returnMessage = string.Empty;
            ErrorMessage.Value = "";
            if (ModbusHost.HasValue)
            {
                ModbusClient modbusClient = null;
                try
                {
                    modbusClient = new ModbusClient(ModbusHost.Value, ModbusPort.Value);
                    modbusClient.ConnectionTimeout = Timeout.Value;
                    modbusClient.UnitIdentifier = UnitIdentifier.Value;
                    modbusClient.Connect();

                    // see: https://knx-user-forum.de/forum/%C3%B6ffentlicher-bereich/knx-eib-forum/1643359-gira-x1-und-modbus-tcp-mit-logikbaustein/page7#post1844442
                    System.Threading.Thread.Sleep(700);

                    this.mppt1Voltage.Value = readRegister(modbusClient, 32016, DataTypeEnum.INT16_SIGNED, ref returnMessage) / 10.0;
                    this.mppt1Current.Value = readRegister(modbusClient, 32017, DataTypeEnum.INT16_SIGNED, ref returnMessage) / 100.0;
                    this.mppt2Voltage.Value = readRegister(modbusClient, 32018, DataTypeEnum.INT16_SIGNED, ref returnMessage) / 10.0;
                    this.mppt2Current.Value = readRegister(modbusClient, 32019, DataTypeEnum.INT16_SIGNED, ref returnMessage) / 100.0;
                    this.currentPVPower.Value = readRegister(modbusClient, 32064, DataTypeEnum.INT32, ref returnMessage);
                    this.todaysPeakPVPower.Value = readRegister(modbusClient, 32078, DataTypeEnum.INT32, ref returnMessage);
                    this.currentACPower.Value = readRegister(modbusClient, 32080, DataTypeEnum.INT32    , ref returnMessage);
                    this.currentReactivePower.Value = readRegister(modbusClient, 32082, DataTypeEnum.INT32, ref returnMessage);
                    this.inverterTemperature.Value = readRegister(modbusClient, 32087, DataTypeEnum.INT16_SIGNED, ref returnMessage) / 10.0; 
                    this.totalPVEnergy.Value = readRegister(modbusClient, 32106, DataTypeEnum.INT32, ref returnMessage) / 100.0; // unsigned!
                    this.todayPVEnergy.Value = readRegister(modbusClient, 32114, DataTypeEnum.INT32, ref returnMessage) / 100.0; // unsigned!

                    this.currentBatteryStatus.Value = readRegister(modbusClient, 37000, DataTypeEnum.INT16_UNSIGNED, ref returnMessage);
                    this.currentBatteryPower.Value = readRegister(modbusClient, 37001, DataTypeEnum.INT32, ref returnMessage);
                    this.currentBatterySOC.Value = readRegister(modbusClient, 37004, DataTypeEnum.INT16_UNSIGNED, ref returnMessage) / 10.0;
                    this.todayBatteryChargedEnergy.Value = readRegister(modbusClient, 37015, DataTypeEnum.INT32, ref returnMessage); // unsigned!
                    this.todayBatteryDischargedEnergy.Value = readRegister(modbusClient, 37017, DataTypeEnum.INT32, ref returnMessage); // unsigned!
                    this.batteryTemperature.Value = readRegister(modbusClient, 37022, DataTypeEnum.INT16_SIGNED, ref returnMessage) / 10.0;
                    this.currentGridPower.Value = readRegister(modbusClient, 37113, DataTypeEnum.INT32, ref returnMessage);
                    this.totalGridExportedEnergy.Value = readRegister(modbusClient, 37119, DataTypeEnum.INT32, ref returnMessage) / 100.0;
                    this.totalGridImportedEnergy.Value = readRegister(modbusClient, 37121, DataTypeEnum.INT32, ref returnMessage) / 100.0;
                    this.ErrorMessage.Value = returnMessage;

                    this.SchedulerService.InvokeIn(new TimeSpan(0, 0, TimeSpan.Value), FetchFromModbusServer);

                }
                catch (Exception e)
                {
                    this.ErrorMessage.Value = e.ToString();
                    this.SchedulerService.InvokeIn(new TimeSpan(0, 1, 0), FetchFromModbusServer);
                }
                finally
                {
                    if (modbusClient != null)
                    {
                        modbusClient.Disconnect();
                    }

                    this.ErrorMessage.Value += "Run completed";
                }
            }
        }

    }
}
