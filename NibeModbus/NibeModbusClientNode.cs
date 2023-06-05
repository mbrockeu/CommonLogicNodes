using EasyModbus;
using LogicModule.Nodes.Helpers;
using LogicModule.ObjectModel;
using LogicModule.ObjectModel.TypeSystem;
using System;
using System.Threading;
using static LogicNodesModbusHelper.LogicNodesModbus;

namespace marcus_brock_mbrock_eu.logic.NibeModbusClientNode
{

    public class NibeModbus : LogicNodeBase
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
        public IntValueObject AuxModbus { get; private set; }
        [Input(DisplayOrder = 7, IsInput = true, IsRequired = false)]
        public IntValueObject ExternalCoolingAdjustment { get; private set; }
        [Input(DisplayOrder = 8, IsInput = true, IsRequired = false)]
        public IntValueObject CoolingAdjustment { get; private set; }

        [Input(DisplayOrder = 9, IsInput = true, IsRequired = false)]
        public IntValueObject ExternalHeatingAdjustment { get; private set; }
        [Input(DisplayOrder = 10, IsInput = true, IsRequired = false)]
        public IntValueObject HeatingAdjustment { get; private set; }
        #endregion

        #region outputs
        [Output]
        public DoubleValueObject CurrentTemperatureOutside { get; private set; }
        [Output]
        public DoubleValueObject CurrentTemperatureFlow { get; private set; }
        [Output]

        public BoolValueObject IsHeatingAllowed { get; private set; }
        [Output]
        public BoolValueObject IsCoolingAllowed { get; private set; }

        [Output]
        public StringValueObject ErrorMessage { get; private set; }
        #endregion

        private ISchedulerService SchedulerService;

        public NibeModbus(INodeContext context)
        {
            context.ThrowIfNull("context");
            ITypeService typeService = context.GetService<ITypeService>();

            this.TimeSpan = typeService.CreateInt(PortTypes.Integer, "Abfrageinterval", 60);
            this.ModbusHost = typeService.CreateString(PortTypes.String, "Modbus TCP Host");
            this.ModbusPort = typeService.CreateInt(PortTypes.Integer, "Port", 502);
            this.Timeout = typeService.CreateInt(PortTypes.Integer, "Timeout", 5000);
            this.UnitIdentifier = typeService.CreateByte(PortTypes.Byte, "UnitIdentifier", 1);

            this.AuxModbus = typeService.CreateInt(PortTypes.Integer, "AuxModbus");
            this.ExternalCoolingAdjustment = typeService.CreateInt(PortTypes.Integer, "Externe Justierung Kühlen");
            this.CoolingAdjustment = typeService.CreateInt(PortTypes.Integer, "Justierung Kühlen");
            this.ExternalHeatingAdjustment = typeService.CreateInt(PortTypes.Integer, "Externe Justierung Heizen");
            this.HeatingAdjustment = typeService.CreateInt(PortTypes.Integer, "Justierung Heizen");

            this.CurrentTemperatureOutside = typeService.CreateDouble(PortTypes.Number, "Außentemperatur");
            this.CurrentTemperatureFlow = typeService.CreateDouble(PortTypes.Number, "Vorlauftemperatur");
            this.IsCoolingAllowed = typeService.CreateBool(PortTypes.Bool, "Kühlung zulassen");
            this.IsHeatingAllowed = typeService.CreateBool(PortTypes.Bool, "Heizung zulassen");

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
                modbusClient.UnitIdentifier = UnitIdentifier.Value;
                modbusClient.Connect();


                if ((this.AuxModbus.HasValue && this.AuxModbus.WasSet))
                {
                    writeRegister(modbusClient, 2741, (int)this.AuxModbus.Value, DataTypeEnum.INT32, ref returnMessage);
                }
                if ((this.ExternalCoolingAdjustment.HasValue && this.ExternalCoolingAdjustment.WasSet))
                {
                    writeRegister(modbusClient, 4154, (int)this.ExternalCoolingAdjustment.Value, DataTypeEnum.INT32, ref returnMessage);
                }
                if ((this.CoolingAdjustment.HasValue && this.CoolingAdjustment.WasSet))
                {
                    writeRegister(modbusClient, 975, (int)this.CoolingAdjustment.Value, DataTypeEnum.INT16_UNSIGNED, ref returnMessage);
                }
                if ((this.ExternalHeatingAdjustment.HasValue && this.ExternalHeatingAdjustment.WasSet))
                {
                    writeRegister(modbusClient, 51, (int)this.ExternalHeatingAdjustment.Value, DataTypeEnum.INT16_UNSIGNED, ref returnMessage);
                }
                if ((this.HeatingAdjustment.HasValue && this.HeatingAdjustment.WasSet))
                {
                    writeRegister(modbusClient, 30, (int)this.HeatingAdjustment.Value, DataTypeEnum.INT16_UNSIGNED, ref returnMessage);
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

                    this.CurrentTemperatureFlow.Value = readRegister(modbusClient, 5, DataTypeEnum.INT16_SIGNED, ref returnMessage, FunctionCodeEnum.FC_04) / 10.0;
                    this.CurrentTemperatureOutside.Value = readRegister(modbusClient, 1, DataTypeEnum.INT16_SIGNED,ref returnMessage, FunctionCodeEnum.FC_04) / 10.0;
                    this.IsCoolingAllowed.Value = Convert.ToBoolean(readRegister(modbusClient, 182, DataTypeEnum.INT32, ref returnMessage));
                    this.IsHeatingAllowed.Value = Convert.ToBoolean(readRegister(modbusClient, 181, DataTypeEnum.INT32, ref returnMessage));
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
